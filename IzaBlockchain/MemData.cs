using LiteDB;

namespace IzaBlockchain
{
    public class MemData
    {
        /// <summary>
        /// Normal value is "MemData/Data", points to the path on disk, relative to blockchain storage, where this MemData instance will be stored
        /// </summary>
        public virtual string RelativePath => "MemData/Data";

        protected LiteDatabase db;

        public virtual void Initialize()
        {
            InitializeDB();
        }
        protected void InitializeDB()
        {
            db = new LiteDatabase(Blockchain.GetPath(RelativePath));
        }
        public virtual void End()
        {
            EndDB();
        }
        protected void EndDB()
        {
            db.Dispose();
        }
        public MemData()
        {
            Initialize();
        }
    }
}