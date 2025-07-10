using System;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class RefDbRepoCryptoFixture
{
	[Test]
	public void EncryptAndDecrypt_ShouldReturnOriginalMessage()
	{
		var refDbRepoCrypto = new RefDbRepoCrypto("", "", "");
		const string originalText = "Hello, world!";

		var encryptedData = refDbRepoCrypto.EncryptRSA(originalText);
		var decryptedMessage = refDbRepoCrypto.DecryptRSA(encryptedData);

		Assert.That(decryptedMessage, Is.EqualTo(originalText));
	}

	[Test]
	public void EncryptAES_And_DecryptAES()
	{
		var refDbRepoCrypto = new RefDbRepoCrypto("", "", "");
		string originalText = "Hello, world! Hello, world!";

		var encryptedData = refDbRepoCrypto.EncryptAES(Encoding.UTF8.GetBytes(originalText));
		var decryptedData = refDbRepoCrypto.DecryptAES(encryptedData);
		Assert.That(Encoding.UTF8.GetString(decryptedData), Is.EqualTo(originalText));
	}

	[Test]
	public void EncryptAndDecrypt_OriginalTextIsNull_ThrowException()
	{
		var refDbRepoCrypto = new RefDbRepoCrypto("", "", "");
		Assert.Throws<ArgumentNullException>(() => refDbRepoCrypto.EncryptRSA(null));
		Assert.Throws<ArgumentNullException>(() => refDbRepoCrypto.DecryptRSA(null));
	}

	[Test]
	public void EncryptAndDecrypt_OriginalTextIsEmpty_ShouldReturnEmpty()
	{
		var refDbRepoCrypto = new RefDbRepoCrypto("", "", "");
		var originalText = string.Empty;
		var encryptedMessage = refDbRepoCrypto.EncryptRSA(originalText);
		Assert.That(encryptedMessage, Is.EqualTo(Array.Empty<byte>()));
		var decryptedMessage = refDbRepoCrypto.DecryptRSA(encryptedMessage);
		Assert.That(decryptedMessage, Is.EqualTo(string.Empty));
	}
}
