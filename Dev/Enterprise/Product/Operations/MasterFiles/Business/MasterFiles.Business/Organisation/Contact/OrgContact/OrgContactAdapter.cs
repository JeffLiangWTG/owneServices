using System;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public struct OrgContactAdapter : IUserSecretHashComponents
	{
		public OrgContactAdapter(OrgContact record)
		{
			this.record = record;
		}

		readonly OrgContact record;

		public UserSecretHashAlgorithm Algorithm => UserSecretHashAlgorithm.Pbkdf2HmacSha1;
		public byte[] Hash => record.OC_PasswordHash;
		public byte[] Salt => record.OC_PasswordSalt;
		public int IterationsCount => record.OC_PasswordHashIterations;

		public void Update(UserSecretHashAlgorithm algorithm, byte[] hash, byte[] salt, int iterationsCount)
		{
			if (algorithm != UserSecretHashAlgorithm.Pbkdf2HmacSha1)
			{
				throw new InvalidOperationException("Only PBKDF2-HMAC-SHA1 is supported in the current application.");
			}

			record.OC_PasswordHash = hash;
			record.OC_PasswordSalt = salt;
			record.OC_PasswordHashIterations = iterationsCount;
		}
	}
}
