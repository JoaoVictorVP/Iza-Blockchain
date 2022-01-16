namespace IzaBlockchain
{
    public static class Blockchain
    {
        public static string Path = BlockchainGenerals.Name + '/';
        public static string GetPath(string relativePath) => System.IO.Path.Combine(Path, relativePath);
        static Dictionary<string, MemData> memdatas = new Dictionary<string, MemData>(32);

        public static void AddMemData(string name, MemData memdata)
        {
            if (!memdatas.ContainsKey(name))
                memdatas.Add(name, memdata);
        }
        public static MemData GetMemData(string name) => memdatas[name];

        /// <summary>
        /// Initialize and begin blockchain (load files and etc...)
        /// </summary>
        public static void Begin()
        {

        }

        /// <summary>
        /// Finalize and end blockchain (save files and etc...)
        /// </summary>
        public static void End()
        {

        }
    }
}