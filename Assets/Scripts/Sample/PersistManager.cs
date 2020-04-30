using Base.PersistManager;

namespace Sample
{
    public class PersistManager : PersistManagerBase
    {
        public const string Key = @"sample";
        public override string PersistentKey => Key;
    }
}

