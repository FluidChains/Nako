// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MongoStorageOperations.cs" company="SoftChains">
//   Copyright 2016 Dan Gershony
//   //  Licensed under the MIT license. See LICENSE file in the project root for full license information.
//   //  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//   //  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//   //  OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Nako.Storage.Mongo
{
    using core.Storage.Types;
    #region Using Directives

    using MongoDB.Driver;
    using Nako.Client;
    using Nako.Client.Types;
    using Nako.Config;
    using Nako.Extensions;
    using Nako.Operations;
    using Nako.Operations.Types;
    using Nako.Storage.Mongo.Types;
    using Nako.Storage.Types;
    using Nako.Sync;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion

    /// <summary>
    /// Mongo storage operations.
    /// </summary>
    public class MongoStorageOperations : IStorageOperations
    {
        private readonly IStorage storage;

        private readonly Tracer tracer;

        private readonly NakoConfiguration configuration;

        private readonly MongoData data;

        private readonly SyncConnection syncConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoStorageOperations"/> class.
        /// </summary>
        public MongoStorageOperations(IStorage storage,
            MongoData mongoData,
            Tracer tracer,
            NakoConfiguration nakoConfiguration,
            SyncConnection syncConnection)
        {
            this.data = mongoData;
            this.configuration = nakoConfiguration;
            this.tracer = tracer;
            this.storage = storage;
            this.syncConnection = syncConnection;
        }

        #region Public Methods and Operators

        public void ValidateBlock(SyncBlockTransactionsOperation item)
        {
            if (item.BlockInfo != null)
            {
                var lastBlock = this.storage.BlockGetBlockCount(1).FirstOrDefault();

                if (lastBlock != null)
                {
                    if (lastBlock.Hash == item.BlockInfo.Hash)
                    {
                        if (lastBlock.SyncComplete)
                        {
                            throw new InvalidOperationException("This should never happen.");
                        }
                    }
                    else
                    {
                        if (item.BlockInfo.PreviousBlockHash != lastBlock.Hash)
                        {
                            this.InvalidBlockFound(lastBlock, item);
                            return;
                        }

                        this.CreateBlock(item.BlockInfo);

                        ////if (string.IsNullOrEmpty(lastBlock.NextBlockHash))
                        ////{
                        ////    lastBlock.NextBlockHash = item.BlockInfo.Hash;
                        ////    this.SyncOperations.UpdateBlockHash(lastBlock);
                        ////}
                    }
                }
                else
                {
                    this.CreateBlock(item.BlockInfo);
                }
            }
        }

        public InsertStats InsertTransactions(SyncBlockTransactionsOperation item)
        {
            var stats = new InsertStats { Items = new List<MapTransactionAddress>() };

            if (item.BlockInfo != null)
            {
                // remove all transactions from the memory pool
                item.Transactions.ForEach(t =>
                    {
                        DecodedRawTransaction outer;
                        this.data.MemoryTransactions.TryRemove(t.TxId, out outer);
                    });

                // break the work in to batches transactions
                var queue = new Queue<DecodedRawTransaction>(item.Transactions);
                do
                {
                    var transactions = this.GetBatch(this.configuration.MongoBatchSize, queue).ToList();
                    var bitcoinClient = CryptoClientFactory.Create(
                                        syncConnection.ServerDomain,
                                        syncConnection.RpcAccessPort,
                                        syncConnection.User,
                                        syncConnection.Password,
                                        syncConnection.Secure);
                    try
                    {
                        if (item.BlockInfo != null)
                        {
                            var inserts = new List<MapTransactionBlock>();
                            var insertDetails = new List<MapTransactionDetail>();
                            foreach (var tx in transactions)
                            {
                                var isCoinBase = !string.IsNullOrWhiteSpace(tx.VIn.First().CoinBase);
                                var syncVin = tx.VIn.Select(vin =>
                                {
                                    var previousTransaction = isCoinBase ?
                                        null : bitcoinClient.GetRawTransaction(vin.TxId, 1);

                                    return new SyncVin
                                    {
                                        TxId = vin.TxId,
                                        CoinBase = vin.CoinBase,
                                        IsCoinBase = !string.IsNullOrWhiteSpace(vin.CoinBase),
                                        ScriptSig = vin.ScriptSig,
                                        Sequence = vin.Sequence,
                                        VOut = vin.VOut,
                                        PreviousVout = previousTransaction?.VOut.First(o => o.N == vin.VOut)
                                    };
                                }).ToList();
                                var totalVout = tx.VOut.Sum(o => o.Value);
                                var totalVin = syncVin.Sum(i => i.PreviousVout?.Value);

                                inserts.Add(new MapTransactionBlock
                                {
                                    BlockIndex = item.BlockInfo.Height,
                                    TransactionId = tx.TxId,
                                    TotalVout = totalVout,
                                    TotalVin = totalVin,
                                    Time = tx.Time,
                                    BlockHash = tx.BlockHash,
                                    BlockTime = tx.BlockTime,
                                    Locktime = tx.Locktime,
                                    Version = tx.Version,
                                    IsCoinBase = isCoinBase
                                });

                                insertDetails.Add(new MapTransactionDetail
                                {
                                    TransactionId = tx.TxId,
                                    Vin = syncVin,
                                    Vout = tx.VOut
                                });
                            }

                            stats.Transactions += inserts.Count();
                            this.data.MapTransactionBlock.InsertMany(inserts, new InsertManyOptions { IsOrdered = false });
                            this.data.MapTransactionDetails.InsertMany(insertDetails, new InsertManyOptions { IsOrdered = false });
                        }
                    }
                    catch (MongoBulkWriteException mbwex)
                    {
                        if (!mbwex.Message.Contains("E11000 duplicate key error collection"))
                        {
                            throw;
                        }
                    }

                    // insert inputs and add to the list for later to use on the notification task.
                    var inputs = this.CreateInputs(item.BlockInfo.Height, transactions).ToList();
                    var queueInner = new Queue<MapTransactionAddress>(inputs);
                    do
                    {
                        try
                        {
                            var itemsInner = this.GetBatch(this.configuration.MongoBatchSize, queueInner).ToList();
                            if (itemsInner.Any())
                            {
                                stats.Inputs += itemsInner.Count();
                                stats.Items.AddRange(itemsInner);
                                this.data.MapTransactionAddress.InsertMany(itemsInner, new InsertManyOptions { IsOrdered = false });
                            }
                        }
                        catch (MongoBulkWriteException mbwex)
                        {
                            if (!mbwex.Message.Contains("E11000 duplicate key error collection"))
                            {
                                throw;
                            }
                        }
                    }
                    while (queueInner.Any());

                    // insert outputs
                    var outputs = this.CreateOutputs(transactions).ToList();
                    stats.Outputs += outputs.Count();
                    outputs.ForEach(outp => this.data.MarkOutput(outp.InputTransactionId, outp.InputIndex, outp.TransactionId));
                }
                while (queue.Any());

                // mark the block as synced.
                this.CompleteBlock(item.BlockInfo);
            }
            else
            {
                // memory transaction push in to the pool.
                item.Transactions.ForEach(t =>
                {
                    this.data.MemoryTransactions.TryAdd(t.TxId, t);
                });

                stats.Transactions = this.data.MemoryTransactions.Count();

                // todo: for accuracy - remove transactions from the mongo memory pool that are not anymore in the syncing pool
                // remove all transactions from the memory pool
                // this can be done using the SyncingBlocks objects - see method SyncOperations.FindPoolInternal()

                // add to the list for later to use on the notification task.
                var inputs = this.CreateInputs(-1, item.Transactions).ToList();
                stats.Items.AddRange(inputs);
            }

            return stats;
        }

        #endregion

        private void CompleteBlock(BlockInfo block)
        {
            this.data.CompleteBlock(block.Hash);
        }

        private void CreateBlock(BlockInfo block)
        {
            var blockInfo = new MapBlock
            {
                Height = block.Height,
                Hash = block.Hash,
                Size = block.Size,
                Time = block.Time,
                Bits = block.Bits,
                Confirmations = block.Confirmations,
                Difficulty = block.Difficulty,
                Flags = block.Flags,
                Merkleroot = block.Merkleroot,
                Mint = block.Mint,
                Nonce = block.Nonce,
                ProofHash = block.ProofHash,
                Version = block.Version,
                NextBlockHash = block.NextBlockHash,
                PreviousBlockHash = block.PreviousBlockHash,
                TransactionCount = block.Transactions.Count(),
                SyncComplete = false
            };

            this.data.InsertBlock(blockInfo);
        }

        private IEnumerable<T> GetBatch<T>(int maxItems, Queue<T> queue)
        {
            //var total = 0;
            var items = new List<T>();

            // todo: optimize this
            var aggregate = Extensions.TakeAndRemove(queue, maxItems).ToList();
            items.AddRange(aggregate);

            //do
            //{
            //    var aggregate = Extensions.TakeAndRemove(queue, 100).ToList();

            //    items.AddRange(aggregate);

            //    total = items.SelectMany(s => s.VIn).Cast<object>().Concat(items.SelectMany(s => s.VOut).Cast<object>()).Count();
            //}
            //while (total < maxItems && queue.Any());

            return items;
        }

        private void InvalidBlockFound(SyncBlockInfo lastBlock, SyncBlockTransactionsOperation item)
        {
            // Re-org happened.
            throw new SyncRestartException();
        }

        private IEnumerable<SyncTransactionInfo> CreateTransactions(BlockInfo block, IEnumerable<DecodedRawTransaction> transactions)
        {
            var trxInfps = transactions.Select(trx => new SyncTransactionInfo
            {
                TransactionHash = trx.TxId,
                Timestamp = block == null ? UnixUtils.DateToUnixTimestamp(DateTime.UtcNow) : block.Time
            });

            return trxInfps;
        }

        private IEnumerable<MapTransactionAddress> CreateInputs(long blockIndex, IEnumerable<DecodedRawTransaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                var rawTransaction = transaction;
                var coinBase = rawTransaction.VIn.Any(v => v.CoinBase != null);

                var transactionOutputs = from output in rawTransaction.VOut
                                         where output.Value >= 0
                                                 && output.ScriptPubKey != null
                                                 && output.ScriptPubKey.Addresses != null
                                                 && output.ScriptPubKey.Addresses.Any()
                                         select new MapTransactionAddress
                                         {
                                             Id = string.Format("{0}-{1}", rawTransaction.TxId, output.N),
                                             TransactionId = rawTransaction.TxId,
                                             Value = Convert.ToDouble(output.Value),
                                             Index = output.N,
                                             Addresses = output.ScriptPubKey.Addresses,
                                             ScriptHex = output.ScriptPubKey.Hex,
                                             BlockIndex = blockIndex,
                                             CoinBase = coinBase,
                                             Time = rawTransaction.Time,
                                         };

                foreach (var output in transactionOutputs)
                {
                    yield return output;
                }
            }
        }

        private IEnumerable<dynamic> CreateOutputs(IEnumerable<DecodedRawTransaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                var rawTransaction = transaction;

                var transactionInputs = from input in transaction.VIn ////.Select((vin, index) => new { Item = vin, Index = index })
                                        where input.TxId != null
                                        select new
                                        {
                                            TransactionId = rawTransaction.TxId,
                                            InputTransactionId = input.TxId,
                                            InputIndex = input.VOut
                                        };

                foreach (var input in transactionInputs)
                {
                    yield return input;
                }
            }
        }
    }
}
