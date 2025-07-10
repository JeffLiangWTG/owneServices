using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Rfc2898DeriveBytes = WTG.Foundation.Cryptography.Algorithms.Rfc2898DeriveBytes;

namespace Enterprise.MasterFiles.Business
{
	public class AESCrypto : IAESCrypto
	{
		public AESCrypto() : this(HashAlgorithmName.SHA1)
		{
		}

		public AESCrypto(HashAlgorithmName hashAlgorithmName)
		{
			this.hashAlgorithmName = hashAlgorithmName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded random salt")]
		const string RANDOM_SALT = "sD45fdH46#djEg4@d!g";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded random shared secret")]
		public const string RANDOM_SHAREDSECRET = "df#RSDF%6543dGE";

		/// <summary>
		/// Encrypt the given string using AES.  The string can be decrypted using 
		/// DecryptStringAES().  The sharedSecret parameters must match.
		/// </summary>
		/// <param name="plainText">The text to encrypt.</param>
		/// <param name="sharedSecret">A password used to generate a key for encryption.</param>
		public string EncryptStringAES(string plainText, string sharedSecret)
		{
			if (string.IsNullOrEmpty(plainText))
			{
				throw new ArgumentNullException(nameof(plainText));
			}
			if (string.IsNullOrEmpty(sharedSecret))
			{
				throw new ArgumentNullException(nameof(sharedSecret));
			}

			string outStr = null;           // Encrypted string to return
			Aes aesAlg = null;              // Aes object used to encrypt the data.

			try
			{
				// generate the key from the shared secret and the salt
				var key = new Rfc2898DeriveBytes(sharedSecret, Encoding.ASCII.GetBytes(RANDOM_SALT), 1000, hashAlgorithmName);

				// Create an Aes object
				aesAlg = Aes.Create();
				aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

				// Create a decryptor to perform the stream transform.
				ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption.
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					// prepend the IV
					msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
					msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							//Write all data to the stream.
							swEncrypt.Write(plainText);
						}
					}
					outStr = Convert.ToBase64String(msEncrypt.ToArray());
				}
			}
			finally
			{
				// Clear the Aes object.
				if (aesAlg != null)
				{
					aesAlg.Clear();
				}
			}

			// Return the encrypted bytes from the memory stream.
			return outStr;
		}

		/// <summary>
		/// Decrypt the given string.  Assumes the string was encrypted using 
		/// EncryptStringAES(), using an identical sharedSecret.
		/// </summary>
		/// <param name="cipherText">The text to decrypt.</param>
		/// <param name="sharedSecret">A password used to generate a key for decryption.</param>
		public string DecryptStringAES(string cipherText, string sharedSecret)
		{
			if (string.IsNullOrEmpty(cipherText))
			{
				throw new ArgumentNullException(nameof(cipherText));
			}
			if (string.IsNullOrEmpty(sharedSecret))
			{
				throw new ArgumentNullException(nameof(sharedSecret));
			}

			// Declare the Aes object
			// used to decrypt the data.
			Aes aesAlg = null;

			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			try
			{
				// generate the key from the shared secret and the salt
				var key = new Rfc2898DeriveBytes(sharedSecret, Encoding.ASCII.GetBytes(RANDOM_SALT), 1000, hashAlgorithmName);

				// Create the streams used for decryption.                
				byte[] bytes = Convert.FromBase64String(cipherText);
				using (MemoryStream msDecrypt = new MemoryStream(bytes))
				{
					// Create an Aes object
					// with the specified key and IV.
					aesAlg = Aes.Create();
					aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
					// Get the initialization vector from the encrypted stream
					aesAlg.IV = ReadByteArray(msDecrypt);
					// Create a decrytor to perform the stream transform.
					ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							// Read the decrypted bytes from the decrypting stream
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}
			finally
			{
				// Clear the Aes object.
				if (aesAlg != null)
				{
					aesAlg.Clear();
				}
			}

			return plaintext;
		}

		static byte[] ReadByteArray(Stream s)
		{
			byte[] rawLength = new byte[sizeof(int)];
			if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
			{
				throw new InvalidOperationException("Stream did not contain properly formatted byte array");
			}

			byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
			if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
			{
				throw new InvalidOperationException("Did not read byte array properly");
			}

			return buffer;
		}

		readonly HashAlgorithmName hashAlgorithmName;
	}
}
