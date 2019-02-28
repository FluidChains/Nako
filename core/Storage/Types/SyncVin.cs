using Nako.Client.Types;

namespace core.Storage.Types
{
    public class SyncVin : Vin
    {
        public Vout PreviousVout { get; set; }
        public bool IsCoinBase { get; set; }
    }
}
