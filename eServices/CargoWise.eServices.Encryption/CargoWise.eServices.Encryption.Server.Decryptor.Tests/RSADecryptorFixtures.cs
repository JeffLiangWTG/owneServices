using NUnit.Framework;
using System;
using System.Text;
using CargoWise.eServices.Encryption.Client.Encryptor;

namespace CargoWise.eServices.Encryption.Server.Decryptor.Tests
{
	[TestFixture]
	public class RSADecryptorFixtures
	{
		#region Member Variables

		const int EncryptedBase64StringLength = 172;
		const int MaxLengthBinary = 86;

		#endregion

		[Test]
		public void TestDecryptedText_FromEncryptedData_IsSameAsOriginal()
		{
			#region TestData

			var testData = new[]
				{
					"WiseTech Global",
					"Cargowise One",
					"eServices",
					"Architecture team",
					"Core team",
					"Customs team",
					"GLOW team",
					"ROPE team",
					"BI team",
					"Machine Learning",
					"Embedded team",
					"Telematics team",
					"IS team",
					"Domestics team",
					"International team",
					"Ratings team",
					"Accounting team",
					"We will not let our people fail",
					"We have a clear purpose and a shared vision",
					"12345678901234567892123456789312345678941234567895123456789612345678971234567898123456",
					"慧咨环球信息技术有限公司",
					"一二三四五六七八九拾一二三四五六七八九贰一二三四五六七八",
					"Хуі Консалтинг Глобальні інформаційні техноло"
				};

			#endregion

			for (var i = 0; i < 10; i++)
			{
				var randomText = Guid.NewGuid().ToString();
				var encrypted = EhubClientEncryptor.Encrypt(randomText);
				var decrypted = EhubServerDecryptor.Decrypt(encrypted);

				Assert.AreEqual(randomText, decrypted);
				Assert.AreEqual(EncryptedBase64StringLength, encrypted.Length);
			}

			foreach(var text in testData)
			{
				var encrypted = EhubClientEncryptor.Encrypt(text);
				var decrypted = EhubServerDecryptor.Decrypt(encrypted);

				Assert.AreEqual(text, decrypted);
				Assert.AreEqual(EncryptedBase64StringLength, encrypted.Length);
			}
		}

		[Test]
		public void TestEncryptByteArray_WithOAEPPadding_MaxLength()
		{
			for (var size = 10; size < int.MaxValue; size++)
			{
				var data = new byte[size];
				for (var i = 0; i < size; i++)
				{
					data[i] = Convert.ToByte('x');
				}

				try
				{
					var encrypted = EhubClientEncryptor.EncryptBinary(data);
					var decrypted = EhubServerDecryptor.DecryptBinary(encrypted);
					var encryptedText = Convert.ToBase64String(encrypted);

					Assert.AreEqual(EncryptedBase64StringLength, encryptedText.Length);
					CollectionAssert.AreEqual(data, decrypted);
				}
				catch (Exception exception)
				{
					Assert.IsTrue(exception.Message.Contains("The parameter is incorrect"));
					Assert.AreEqual(MaxLengthBinary + 1, size);

					break;
				}
			}
		}

		[Test]
		public void TestEncryptText_WithOAEPPadding_MaxLength()
		{
			for (var size = 10; size < int.MaxValue ; size++)
			{
				var text = new string('x', size);
				try
				{
					var encrypted = EhubClientEncryptor.Encrypt(text);
					var decrypted = EhubServerDecryptor.Decrypt(encrypted);

					Assert.AreEqual(text, decrypted);
					Assert.AreEqual(EncryptedBase64StringLength, encrypted.Length);
				}
				catch (Exception exception)
				{
					var data = Encoding.UTF8.GetBytes(text);

					Assert.IsTrue(exception.Message.Contains("The parameter is incorrect"));
					Assert.AreEqual(MaxLengthBinary + 1, data.Length);

					break;
				}
			}
		}

		[Test]
		public void TestEncryptUnicodeText_WithOAEPPadding_MaxLength()
		{
			for (var size = 10; size < int.MaxValue; size++)
			{
				var text = new string('惠', size);
				try
				{
					var encrypted = EhubClientEncryptor.Encrypt(text);
					var decrypted = EhubServerDecryptor.Decrypt(encrypted);

					Assert.AreEqual(text, decrypted);
				}
				catch (Exception exception)
				{
					var data = Encoding.UTF8.GetBytes(text);

					Assert.IsTrue(exception.Message.Contains("The parameter is incorrect"));
					Assert.AreEqual(MaxLengthBinary + 1, data.Length);

					break;
				}
			}
		}
	}
}
