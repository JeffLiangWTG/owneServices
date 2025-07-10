using System;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(APIKeyGeneratorStrategy))]
	public sealed class ApiKeyGeneratorStrategyTest : TestCase
	{
		#region Fields

		APIKeyGeneratorStrategy apiKeyGeneratorStrategy;

		#endregion

		public void TestGenerateApiKey_ValidCompany_ReturnsApiKey()
		{
			var apiKey = apiKeyGeneratorStrategy.GenerateAPIKey(GlbCompany.CurrentCompany);

			AssertNotNull(apiKey);
			AssertStartsWith("Start of the api key, rest is random. 'IVCESRKEJFCECVD4'=base32('EDIEDIDAT|')", "IVCESRKEJFCECVD4", apiKey);
		}

		public void TestGenerateApiKey_NullCompany_ThrowsArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				apiKeyGeneratorStrategy.GenerateAPIKey(company: null);
			});
		}

		public void TestApiKeyGeneratorStrategy_NullAESCrypto_ThrowsArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new APIKeyGeneratorStrategy(aesCrypto: null);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (apiKeyGeneratorStrategy == null)
			{
				var aesCryptoMock = new Mock<IAESCrypto>();
				aesCryptoMock
					.Setup(x => x.EncryptStringAES(It.IsAny<string>(), It.IsAny<string>()))
					.Returns((string plainText, string sharedSecret) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText)));

				apiKeyGeneratorStrategy = new APIKeyGeneratorStrategy(aesCryptoMock.Object);
			}
		}
	}
}
