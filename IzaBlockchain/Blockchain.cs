using Newtonsoft.Json;

namespace IzaBlockchain;

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
    public static MemData GetMemData<T>(string name) where T : MemData => memdatas[name] as T;

    public static LocalData Local = new LocalData();

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
public class LocalData : MemData
{
    public override string RelativePath => "../LocalData.mem";

    const string CollectionName = "Data";
    public void SetData(string key, object value)
    {
        string jsonValue = JsonConvert.SerializeObject(value);
        var data = db.GetCollection<Data>(CollectionName);
        if (data.Exists(dat => dat.Key == key))
            data.Update(new Data(key, jsonValue));
        else
            data.Insert(new Data(key, jsonValue));
        data.EnsureIndex(dat => dat.Key, true);
    }



    public T GetData<T>(string key)
    {
        var dat = db.GetCollection<Data>(CollectionName);

        string jsonValue = dat.FindOne(dat => dat.Key == key).Value;

        return JsonConvert.DeserializeObject<T>(jsonValue);
    }
    public object GetData(string key)
    {
        var dat = db.GetCollection<Data>(CollectionName);

        string jsonValue = dat.FindOne(dat => dat.Key == key).Value;

        return JsonConvert.DeserializeObject(jsonValue);
    }

    record struct Data(string Key, string Value);
}