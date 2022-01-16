using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IzaBlockchain;

[JsonConverter(typeof(WalletConvert_Json))]
public class Wallet
{
    public readonly PrivateAddress PrivateAddress;
    public Address PublicAddress;

    public Wallet(PrivateAddress privateAddress)
    {
        PrivateAddress = privateAddress;

        PublicAddress = privateAddress.GetPublicAddress();
    }
}

public class WalletConvert_Json : JsonConverter<Wallet>
{
    public override Wallet? ReadJson(JsonReader reader, Type objectType, Wallet? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        string privateAddress = jsonObject.Property("PrivateAddress").Value<string>();

        var pAddr = PrivateAddress.FromString(privateAddress);

        return new Wallet(pAddr);
    }

    public override void WriteJson(JsonWriter writer, Wallet? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("PrivateAddress");
        writer.WriteValue(value.PublicAddress.ToString());

        writer.WriteEndObject();
    }
}