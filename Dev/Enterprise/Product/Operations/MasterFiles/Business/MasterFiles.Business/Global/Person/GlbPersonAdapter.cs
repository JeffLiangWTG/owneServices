using System;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public struct GlbPersonAdapter : IUserSecretHashComponents
	{
		public GlbPersonAdapter(GlbPerson record)
		{
			this.record = record;
		}

		readonly GlbPerson record;

		public UserSecretHashAlgorithm Algorithm => UserSecretHashAlgorithm.Pbkdf2HmacSha1;
		public byte[] Hash => record.PER_PasswordHash;
		public byte[] Salt => record.PER_PasswordSalt;
		public int IterationsCount => record.PER_PasswordHashIterations;

		public void Update(UserSecretHashAlgorithm algorithm, byte[] hash, byte[] salt, int iterationsCount)
		{
			if (algorithm != UserSecretHashAlgorithm.Pbkdf2HmacSha1)
			{
				throw new InvalidOperationException("Only PBKDF2-HMAC-SHA1 is supported in the current application.");
			}

			record.PER_PasswordHash = hash;
			record.PER_PasswordSalt = salt;
			record.PER_PasswordHashIterations = iterationsCount;
		}
	}
}
