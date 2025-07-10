using System;
using System.Security.Cryptography;
using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Licensing;

namespace Enterprise.MasterFiles.Business
{
	public class APIKeyGeneratorStrategy : IAPIKeyGeneratorStrategy
	{
		readonly IAESCrypto aesCrypto;

		public APIKeyGeneratorStrategy(IAESCrypto aesCrypto)
		{
			if(aesCrypto == null)
			{
				throw new ArgumentNullException(nameof(aesCrypto));
			}

			this.aesCrypto = aesCrypto;
		}

		public string GenerateAPIKey(GlbCompany company)
		{
			if (company == null)
			{
				throw new ArgumentNullException(nameof(company));
			}

			var companyCode = company.GC_Code;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseKey = registrationKey.EnterpriseCode + companyCode + registrationKey.ServerCode;

			var apiKey = enterpriseKey + "|" + GetRandomString();
			var encryptedApiKey = Encrypt(apiKey);

			return encryptedApiKey;
		}

		string Encrypt(string value)
		{
			var aesEncryptionKey = ObjectFactory.Get<IAccounting>().RSADecrypt(AccountingMasterFilesRegistry.Instance.APIKeyGeneratorStrategyEncryptionKey.Value);
			var encryptedValueBase64 = aesCrypto.EncryptStringAES(value, aesEncryptionKey);

			var encryptedBytes = Convert.FromBase64String(encryptedValueBase64);
			var encryptedValueBase32 = Base32Helper.ToBase32(encryptedBytes);

			return encryptedValueBase32;
		}

		string GetRandomString()
		{
			var randomBytes = new byte[10];
			RandomNumberGenerator.Create().GetNonZeroBytes(randomBytes);

			var randomString = Convert.ToBase64String(randomBytes).Substring(0, 10);

			return randomString;
		}
	}
}
