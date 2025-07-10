using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.RefDbRepo.Common.Utils;

public class RefDbRepoCrypto : IRefDbRepoCrypto
{
	public RefDbRepoCrypto(string publicKeyName, string privateKeyName, string aesKeyName)
	{
#if DEBUG
		cipherKeyPairInfoManager = new RsaCipherKeyPairInfoManager_Debug();
#else
		cipherKeyPairInfoManager = new RsaCipherKeyPairInfoManager(publicKeyName, privateKeyName, aesKeyName);
#endif
	}

	public byte[] EncryptRSA(string plainText)
	{
		if (plainText == null)
		{
			throw new ArgumentNullException(nameof(plainText));
		}

		if (plainText.Length == 0)
		{
			return Array.Empty<byte>();
		}
		using (var rsa = new RSACryptoServiceProvider())
		{
			rsa.ImportParameters(cipherKeyPairInfoManager.GetPublicKey());
			return rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), false);
		}
	}

	public string DecryptRSA(byte[] encryptedData)
	{
		if (encryptedData == null)
		{
			throw new ArgumentNullException(nameof(encryptedData));
		}

		if (encryptedData.Length == 0)
		{
			return string.Empty;
		}
		using (var rsa = new RSACryptoServiceProvider())
		{
			rsa.ImportParameters(cipherKeyPairInfoManager.GetPrivateKey());
			var decryptedBytes = rsa.Decrypt(encryptedData, false);
			return Encoding.UTF8.GetString(decryptedBytes);
		}
	}

	public byte[] EncryptAES(byte[] originalData)
	{
		Argument.Argument.NotNull(originalData, nameof(originalData));

		var key = cipherKeyPairInfoManager.GetAeskey();
		ValidateKeyLength(key);

		using var aes = Aes.Create();
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		aes.Key = key;
		using var outputStream = new MemoryStream();
		outputStream.Write(aes.IV, 0, aes.IV.Length);

		using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
		cryptoStream.Write(originalData, 0, originalData.Length);
		cryptoStream.FlushFinalBlock();
		return outputStream.ToArray();
	}

	public byte[] DecryptAES(byte[] encryptedData)
	{
		var key = cipherKeyPairInfoManager.GetAeskey();
		ValidateKeyLength(key);

		using var inputStream = new MemoryStream(encryptedData);
		var iv = new byte[16];
		var bytesRead = inputStream.Read(iv, 0, 16);
		if (bytesRead < 16)
		{
			throw new InvalidDataException("Invalid Encrpted Data.");
		}

		using var aes = Aes.Create();
		aes.Mode = CipherMode.CBC;
		aes.Padding = PaddingMode.PKCS7;
		aes.Key = key;
		aes.IV = iv;
		using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
		using var outputStream = new MemoryStream();
		cryptoStream.CopyTo(outputStream);
		return outputStream.ToArray();
	}

	static void ValidateKeyLength(byte[] key)
	{
		if (key.Length != 16 && key.Length != 24 && key.Length != 32)
		{
			throw new ArgumentException("Invalid AES key is provided.");
		}
	}

	readonly ICipherKeyPairInfoManager cipherKeyPairInfoManager;
}
