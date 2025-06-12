using System.Security.Cryptography;
using System.Text;

namespace eServices.Dms.Core.OpsPortal.Services;

public class AppAccessService(IConfiguration configuration)
{
	public virtual string GetAccessKey(string password)
	{
		using var aes = Aes.Create();
		aes.Key = Convert.FromBase64String(configuration["DMSCORE_IDENTITY_KEY"]
			?? throw new InvalidOperationException("Missing environment variable: DMSCORE_IDENTITY_KEY"));
		byte[] iv = aes.IV;
		using var mem = new MemoryStream();
		mem.Write(iv, 0, iv.Length);
		using var enc = new CryptoStream(mem, aes.CreateEncryptor(), CryptoStreamMode.Write);
		enc.Write(System.Text.Encoding.UTF8.GetBytes(password));
		enc.FlushFinalBlock();
		return Convert.ToBase64String(mem.ToArray());
	}

	public virtual string GetBasicAuthHeader(string userName, string accessKey)
		=> $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{GetRawAccessKey(accessKey)}"))}";

	public virtual string GetSqlAccessKey(string accessKey) => GetRawAccessKey(accessKey);

	private string GetRawAccessKey(string accessKey)
	{
		var keyBytes = Convert.FromBase64String(accessKey);
		using var aes = Aes.Create();
		aes.Key = Convert.FromBase64String(configuration["DMSCORE_IDENTITY_KEY"]
			?? throw new InvalidOperationException("Missing environment variable: DMSCORE_IDENTITY_KEY"));
		var iv = keyBytes[0..aes.IV.Length];
		using var mem = new MemoryStream(keyBytes[aes.IV.Length..]);
		using var decryptor = aes.CreateDecryptor(aes.Key, iv);
		using var cryptoStream = new CryptoStream(mem, decryptor, CryptoStreamMode.Read);
		using var reader = new StreamReader(cryptoStream);
		return reader.ReadToEnd();
	}
}
