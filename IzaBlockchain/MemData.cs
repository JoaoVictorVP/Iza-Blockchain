using LiteDB;

namespace IzaBlockchain
{
    public class MemData
    {
        /// <summary>
        /// Normal value is "MemData/Data", points to the path on disk, relative to blockchain storage, where this MemData instance will be stored
        /// </summary>
        public virtual string RelativePath => "MemData/Data";

        public string FullPath => Blockchain.GetPath(RelativePath);

        protected LiteDatabase db;

        public virtual void Initialize()
        {
            InitializeDB();
        }
        protected void InitializeDB()
        {
            // Temp mod on db
            //db = new LiteDatabase(new MemoryStream(File.ReadAllBytes(FullPath)));
            db = new LiteDatabase(FullPath);
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