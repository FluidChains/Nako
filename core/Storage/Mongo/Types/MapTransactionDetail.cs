using Nako.Client.Types;
using System.Collections.Generic;

namespace Nako.Storage.Mongo.Types
{
    public class MapTransactionDetail
    {
        public string TransactionId { get; set; }

        public List<Vin> VIn { get; set; }

        public List<Vout> VOut { get; set; }
    }
}
