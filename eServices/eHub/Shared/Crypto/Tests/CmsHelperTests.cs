using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using CargoWise.eHub.Shared.Crypto.Tests.TestFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Shared.Crypto.Tests
{
	[TestClass]
	public class CmsHelperTests
	{
		[TestMethod]
		public void Crypto_CmsHelpers_ComputeSignatureBase64()
		{
			var keyData = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var content = TestFileHelpers.GetResourceData("TestFiles.Sample.txt");

			string signature = CmsHelpers.ComputeSignatureBase64(content, keyData, "miragef7", 76);

			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.Signature.txt"), signature);
		}

		[TestMethod]
		public void Crypto_CmsHelpers_EncryptDecrypt()
		{
			var message = TestFileHelpers.GetResourceData("TestFiles.Sample.txt");
			var publicKeyData = TestFileHelpers.GetResourceData("TestFiles.00505655TST.cer");
			var privateKeyData = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");

			var sha256 = new AlgorithmIdentifier(Oid.FromFriendlyName("SHA256", OidGroup.HashAlgorithm));
			var tripleDES = new AlgorithmIdentifier(Oid.FromFriendlyName("3DES", OidGroup.EncryptionAlgorithm));
			var aes256 = new AlgorithmIdentifier(Oid.FromFriendlyName("AES256", OidGroup.EncryptionAlgorithm));

			try
			{
				CmsHelpers.EncryptMessage(message, publicKeyData, sha256);
				Assert.Fail("Should throw");
			}
			catch (CryptographicException ex)
			{
				Assert.AreEqual("Unknown cryptographic algorithm.\r\n", ex.Message);
			}

			var encryptedDefault = CmsHelpers.EncryptMessage(message, publicKeyData);
			var encryptedTripleDES = CmsHelpers.EncryptMessage(message, publicKeyData, tripleDES);
			var encryptedAES256 = CmsHelpers.EncryptMessage(message, publicKeyData, aes256);

			var decryptedDefault = CmsHelpers.DecryptMessage(encryptedDefault, privateKeyData, "miragef7");
			var decryptedTripleDES = CmsHelpers.DecryptMessage(encryptedTripleDES, privateKeyData, "miragef7");
			var decryptedAES256 = CmsHelpers.DecryptMessage(encryptedAES256, privateKeyData, "miragef7");

			var encodedMessage = Encoding.UTF8.GetString(message);
			Assert.AreEqual(encodedMessage, Encoding.UTF8.GetString(decryptedDefault));
			Assert.AreEqual(encodedMessage, Encoding.UTF8.GetString(decryptedTripleDES));
			Assert.AreEqual(encodedMessage, Encoding.UTF8.GetString(decryptedAES256));
		}

		[TestMethod]
		public void Crypto_CmsHelpers_ExtractFromSignature()
		{
			var signature = TestFileHelpers.GetResourceText("TestFiles.SignatureWithData.P7M");
			var decrypted = CmsHelpers.ExtractDataFromSignature(Convert.FromBase64String(signature));
			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.ExtractedContentFromSignature.txt").Replace("\r\n", "\n").TrimEnd(), Encoding.UTF8.GetString(decrypted).Replace("\r\n", "\n").TrimEnd());
		}

		[TestMethod]
		public void Crypto_CmsHelpers_ComputeSignature_AddSignatureAttributes()
		{
			var keyData = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var content = TestFileHelpers.GetResourceData("TestFiles.Sample.txt");

			var attributes = new[] { new Pkcs9SigningTime(new DateTime(2018, 11, 30)) };

			var signature = CmsHelpers.ComputeSignature(content, keyData, "miragef7", attributes, true, Oid.FromFriendlyName("SHA1", OidGroup.HashAlgorithm));
			Assert.AreEqual(TestFileHelpers.GetResourceText("TestFiles.SignatureWithSigningTime.txt"), Convert.ToBase64String(signature));
		}

		[TestMethod]
		public void Crypto_CmsHelpers_ComputeSignature_HashingAlgorithms()
		{
			var privateKey = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx");
			var publicKey = TestFileHelpers.GetResourceData("TestFiles.00505655TST.cer");
			var content = TestFileHelpers.GetResourceData("TestFiles.Sample.txt");

			var tripleDES = Oid.FromFriendlyName("3DES", OidGroup.EncryptionAlgorithm);
			try
			{
				CmsHelpers.ComputeSignature(content, privateKey, "miragef7", new List<Pkcs9AttributeObject>(), true, tripleDES);
				Assert.Fail("Should throw");
			}
			catch (CryptographicException ex)
			{
				Assert.AreEqual("Unknown cryptographic algorithm.\r\n", ex.Message);
			}

			var sha1 = Oid.FromFriendlyName("SHA1", OidGroup.HashAlgorithm);
			var sha256 = Oid.FromFriendlyName("SHA256", OidGroup.HashAlgorithm);
			var signatureSHA1 = CmsHelpers.ComputeSignature(content, privateKey, "miragef7", new List<Pkcs9AttributeObject>(), true, sha1);
			var signatureSHA256 = CmsHelpers.ComputeSignature(content, privateKey, "miragef7", new List<Pkcs9AttributeObject>(), true, sha256);

			CmsHelpers.VerifySignature(content, signatureSHA1, publicKey);
			CmsHelpers.VerifySignature(content, signatureSHA256, publicKey);
		}
	}
}
