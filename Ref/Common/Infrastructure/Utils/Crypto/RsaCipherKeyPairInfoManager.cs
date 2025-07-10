using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace CargoWise.RefDbRepo.Common.Utils;

class RsaCipherKeyPairInfoManager : ICipherKeyPairInfoManager
{
	public RsaCipherKeyPairInfoManager(string publicKeyName, string privateKeyName, string aesKeyName)
	{
		var publicKeyPath = Path.Combine(DefaultSecretFolder, publicKeyName);
		if (!File.Exists(publicKeyPath))
		{
			throw new FileNotFoundException("No publicKey file found.");
		}
		var privateKeyPath = Path.Combine(DefaultSecretFolder, privateKeyName);
		if (!File.Exists(privateKeyPath))
		{
			throw new FileNotFoundException("No privateKey file found.");
		}
		var aesKeyPath = Path.Combine(DefaultSecretFolder, aesKeyName);
		if (!File.Exists(aesKeyPath))
		{
			throw new FileNotFoundException("No aesKey file found.");
		}
		aesKey = File.ReadAllBytes(Path.Combine(DefaultSecretFolder, aesKeyName));
		using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
		{
			rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
			publicKey = rsa.ExportParameters(false);
		}
		using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
		{
			rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
			privateKey = rsa.ExportParameters(true);
		}
	}

	public RSAParameters GetPublicKey() => publicKey;

	public RSAParameters GetPrivateKey() => privateKey;

	public byte[] GetAeskey() => aesKey;

	readonly RSAParameters publicKey;
	readonly RSAParameters privateKey;
	readonly byte[] aesKey;

	static readonly string DefaultSecretFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\CipherKeys");
}
