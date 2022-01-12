namespace IzaBlockchain
{
    public static class Blockchain
    {
        public static string Path = BlockchainGenerals.Name + '/';
        public static string GetPath(string relativePath) => System.IO.Path.Combine(Path, relativePath);
        static Dictionary<string, MemData> memdatas = new Dictionary<string, MemData>(32);

        public static void AddMemData(string name, MemData memdata) => memdatas[name] = memdata;
        public static MemData GetMemData(string name) => memdatas[name];
    }
}