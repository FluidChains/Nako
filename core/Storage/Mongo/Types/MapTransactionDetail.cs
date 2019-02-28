using core.Storage.Types;
using Nako.Client.Types;
using System.Collections.Generic;

namespace Nako.Storage.Mongo.Types
{
    public class MapTransactionDetail
    {
        public string TransactionId { get; set; }
        public List<SyncVin> Vin { get; set; }
        public List<Vout> Vout { get; set; }
    }
}
