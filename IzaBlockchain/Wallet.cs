using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IzaBlockchain;

[JsonConverter(typeof(WalletConvert_Json))]
public class Wallet
{
    public readonly PrivateAddress PrivateAddress;
    public Address PublicAddress;

    /// <summary>
    /// Size of cipher key in wallet encryption
    /// </summary>
    const int PasswordKeySize = 256;

    public static string Serialize(Wallet wallet, string password = null)
    {
        string json = JsonConvert.SerializeObject(wallet);

        if(password != null)
        {
            Span<byte> passwordBytes = stackalloc byte[Encoding.UTF8.GetByteCount(password)];
            Encoding.UTF8.GetBytes(password, passwordBytes);
            var key = SHA256.HashData(passwordBytes);

            var aes = new AesGcm(key);

            Span<byte> jsonBytes = stackalloc byte[Encoding.UTF8.GetByteCount(json)];
            Encoding.UTF8.GetBytes(json, jsonBytes);

            int nonceSize = AesGcm.NonceByteSizes.MaxSize;
            int tagSize = AesGcm.TagByteSizes.MaxSize;
            int cipherSize = jsonBytes.Length;

            Span<byte> nonce = stackalloc byte[nonceSize];
            Span<byte> tag = stackalloc byte[tagSize];
            Span<byte> cipher = stackalloc byte[cipherSize];

            RandomNumberGenerator.Fill(nonce);

            aes.Encrypt(nonce, jsonBytes, cipher, tag);

            json = Convert.ToHexString(cipher) + "|||" + Convert.ToHexString(nonce) + "|||" + Convert.ToHexString(tag);
        }

        return json;
    }

    public static Wallet Deserialize(string json, string password = null)
    {
        if(password != null)
        {
            string[] split = json.Split("|||");
            json = split[0];
            string hexNonce = split[1];
            string hexTag = split[2];

            Span<byte> nonce = Convert.FromHexString(hexNonce);
            Span<byte> tag = Convert.FromHexString(hexTag);
            Span<byte> cipher = Convert.FromHexString(json);

            Span<byte> passwordBytes = stackalloc byte[Encoding.UTF8.GetByteCount(password)];
            Encoding.UTF8.GetBytes(password, passwordBytes);
            var key = SHA256.HashData(passwordBytes);

            var aes = new AesGcm(key);

            Span<byte> plaintext = stackalloc byte[cipher.Length];

            aes.Decrypt(nonce, cipher, tag, plaintext);

            json = Encoding.UTF8.GetString(plaintext);
        }

        return JsonConvert.DeserializeObject<Wallet>(json);
    }

/*    public static string Serialize(Wallet wallet, string password = null)
    {
        string json = JsonConvert.SerializeObject(wallet);

        if (password != null)
        {
            using var aes = Aes.Create();
            aes.KeySize = PasswordKeySize;

            Span<byte> passwordBytes = stackalloc byte[Encoding.UTF8.GetByteCount(password)];
            Encoding.UTF8.GetBytes(password, passwordBytes);
            var key = SHA256.HashData(passwordBytes);
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using (var mem = new MemoryStream())
            {
                using (var stream = new CryptoStream(mem, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    stream.Write(jsonBytes);
                }
                //json = Convert.ToHexString(mem.ToArray());
                json = Convert.ToHexString(mem.ToArray()) + "|||" + Convert.ToHexString(aes.IV);
            }
        }
        return json;
    }

    public static Wallet Deserialize(string json, string password = null)
    {
        if (password != null)
        {
            string[] split = json.Split("|||");
            json = split[0];
            string ivHex = split[1];

            using var aes = Aes.Create();
            aes.KeySize = PasswordKeySize;

            Span<byte> passwordBytes = stackalloc byte[Encoding.UTF8.GetByteCount(password)];
            Encoding.UTF8.GetBytes(password, passwordBytes);
            var key = SHA256.HashData(passwordBytes);
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.IV = Convert.FromHexString(ivHex);

            using (var mem = new MemoryStream(Convert.FromHexString(json)))
            using (var stream = new CryptoStream(mem, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                Span<byte> jsonBytes = stackalloc byte[(int)mem.Length];
                stream.Read(jsonBytes);
                json = Encoding.UTF8.GetString(jsonBytes);
            }
        }
        return JsonConvert.DeserializeObject<Wallet>(json);
    }*/

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

        string privateAddress = (string)jsonObject.Property("PrivateAddress").Value;

        var pAddr = PrivateAddress.FromString(privateAddress);

        return new Wallet(pAddr);
    }

    public override void WriteJson(JsonWriter writer, Wallet? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("PrivateAddress");
        writer.WriteValue(value.PrivateAddress.ToString());

        writer.WriteEndObject();
    }
}