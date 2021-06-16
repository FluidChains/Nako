﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nako.Ui.Blazor
{
    // TODO: Make a common datatypes project with the api
    public class DataTypes
    {
        public class QueryBlocks
        {
            public QueryBlock[] Blocks { get; set; }
        }

        public class QueryBlock
        {
            public string CoinTag { get; set; }
            public string BlockHash { get; set; }
            public long BlockIndex { get; set; }
            public long BlockSize { get; set; }
            public long BlockTime { get; set; }
            public string NextBlockHash { get; set; }
            public string PreviousBlockHash { get; set; }
            public bool Synced { get; set; }
            public int TransactionCount { get; set; }
            public long Confirmations { get; set; }
            public string Bits { get; set; }
            public string Merkleroot { get; set; }
            public long Nonce { get; set; }
            public double Difficulty { get; set; }
            public string ChainWork { get; set; }
            public long Version { get; set; }
            public string PosBlockSignature { get; set; }
            public string PosModifierv2 { get; set; }
            public string PosFlags { get; set; }
            public string PosHashProof { get; set; }
            public string PosBlockTrust { get; set; }
            public string PosChainTrust { get; set; }
            public IEnumerable<string> Transactions { get; set; }
        }

        public class PeerInfo
        {
            public uint Id { get; set; }
            public string Addr { get; set; }
            public string AddrLocal { get; set; }
            public string Services { get; set; }
            public long LastSend { get; set; }
            public long LastRecv { get; set; }
            public long BytesSent { get; set; }
            public long BytesRecv { get; set; }
            public int ConnTime { get; set; }
            public double PingTime { get; set; }
            public double Version { get; set; }
            public string SubVer { get; set; }
            public bool Inbound { get; set; }
            public long StartingHeight { get; set; }
            public int BanScore { get; set; }
            public long SyncedHeaders { get; set; }
            public long SyncedBlocks { get; set; }
            public IList<long> InFlight { get; set; }
            public bool WhiteListed { get; set; }
        }
        public class Statistics
        {
            public string CoinTag { get; set; }
            public string Progress { get; set; }
            public int TransactionsInPool { get; set; }
            public long SyncBlockIndex { get; set; }
            public BlockchainInfoModel BlockchainInfo { get; set; }
            public int BlocksPerMinute { get; set; }
            public double AvgBlockPersistInSeconds { get; set; }
            public double AvgBlockSizeKb { get; set; }
            public NetworkInfoModel NetworkInfo { get; set; }
            public string Error { get; set; }
        }

        public class BlockchainInfoModel
        {
            public string Chain { get; set; }

            public uint Blocks { get; set; }

            public uint Headers { get; set; }

            public string BestBlockHash { get; set; }

            public double Difficulty { get; set; }

            public long MedianTime { get; set; }

            public double VerificationProgress { get; set; }

            public bool IsInitialBlockDownload { get; set; }

            public string Chainwork { get; set; }

            public bool IsPruned { get; set; }
        }

        public class NetworkInfoModel
        {
            public uint Version { get; set; }

            public string SubVersion { get; set; }

            public uint ProtocolVersion { get; set; }

            public string LocalServices { get; set; }

            public bool IsLocalRelay { get; set; }

            public long TimeOffset { get; set; }

            public int? Connections { get; set; }

            public bool IsNetworkActive { get; set; }

            public decimal RelayFee { get; set; }

            public decimal IncrementalFee { get; set; }
        }

        public class ClientInfo
        {
            public string Version { get; set; }
            public string ProtocolVersion { get; set; }
            public string WalletVersion { get; set; }
            public decimal Balance { get; set; }
            public long Blocks { get; set; }
            public double TimeOffset { get; set; }
            public long Connections { get; set; }
            public string Proxy { get; set; }
            //public double Difficulty { get; set; }
            public bool Testnet { get; set; }
            public long KeyPoolEldest { get; set; }
            public long KeyPoolSize { get; set; }
            public decimal PayTxFee { get; set; }
            public decimal RelayTxFee { get; set; }
            public string Errors { get; set; }
        }

        public class QueryTransaction
        {
            public string CoinTag { get; set; }

            public string BlockHash { get; set; }

            public long? BlockIndex { get; set; }

            public DateTime? Timestamp { get; set; }

            public string TransactionId { get; set; }

            public long Confirmations { get; set; }

            public bool IsCoinbase { get; set; }

            public bool IsCoinstake { get; set; }

            public string LockTime { get; set; }

            public bool RBF { get; set; }

            public uint Version { get; set; }

            public IEnumerable<QueryTransactionInput> Inputs { get; set; }

            public IEnumerable<QueryTransactionOutput> Outputs { get; set; }
        }
        public class QueryTransactionInput
        {
            public int InputIndex { get; set; }

            public string InputAddress { get; set; }

            public string CoinBase { get; set; }

            public string InputTransactionId { get; set; }

            public string ScriptSig { get; set; }

            public string ScriptSigAsm { get; set; }

            public string WitScript { get; set; }

            public string SequenceLock { get; set; }

        }
        public class QueryTransactionOutput
        {
            public string Address { get; set; }

            public long Balance { get; set; }

            public int Index { get; set; }

            public string OutputType { get; set; }

            public string ScriptPubKey { get; set; }

            public string ScriptPubKeyAsm { get; set; }

            public string SpentInTransaction { get; set; }
        }

        public class QueryAddress
        {
            public string CoinTag { get; set; }

            public string Address { get; set; }

            public long Balance { get; set; }

            public long? TotalReceived { get; set; }

            public long? TotalSent { get; set; }

            public long UnconfirmedBalance { get; set; }

            public IEnumerable<QueryAddressItem> Transactions { get; set; }

            public IEnumerable<QueryAddressItem> UnconfirmedTransactions { get; set; }
        }

        public class QueryAddressItem
        {
            public int Index { get; set; }

            public string Type { get; set; }

            public string TransactionHash { get; set; }

            public string SpendingTransactionHash { get; set; }

            public long? SpendingBlockIndex { get; set; }

            public string PubScriptHex { get; set; }

            public bool CoinBase { get; set; }

            public bool CoinStake { get; set; }

            public long Value { get; set; }

            public long? BlockIndex { get; set; }

            public long? Confirmations { get; set; }

            public long Time { get; set; }
        }

        public class CoinInfo
        {
            public string CoinTag { get; set; }

            public string LogoUrl { get; set; }

            public string CoinName { get; set; }

            public string CoinInformation { get; set; }

            public long BlockHeight { get; set; }
        }

        public class QueryMempoolTransaction
        {
            public string TransactionId { get; set; }
        }

        public class QueryMempoolTransactions
        {
            public string CoinTag { get; set; }

            public IEnumerable<QueryMempoolTransaction> Transactions { get; set; }
        }

    }
}
