// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapBlock.cs" company="SoftChains">
//   Copyright 2016 Dan Gershony
//   //  Licensed under the MIT license. See LICENSE file in the project root for full license information.
//   //  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
//   //  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
//   //  OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Nako.Storage.Mongo.Types
{
    public class MapBlock
    {
        public string Hash { get; set; }

        public long Confirmations { get; set; }

        public long Size { get; set; }

        public long Height { get; set; }

        public long Version { get; set; }

        public string Merkleroot { get; set; }

        public string Mint { get; set; }

        public long Time { get; set; }

        public long Nonce { get; set; }

        public string Bits { get; set; }

        public decimal Difficulty { get; set; }

        public string PreviousBlockHash { get; set; }

        public string NextBlockHash { get; set; }

        public string Flags { get; set; }

        public string ProofHash { get; set; }

        public bool SyncComplete { get; set; }

        public int TransactionCount { get; set; }
    }
}