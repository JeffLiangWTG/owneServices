using System;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public struct GlbStaffAdapter : IUserSecretHashComponents
	{
		public GlbStaffAdapter(GlbStaff record)
		{
			this.record = record;
		}

		readonly GlbStaff record;

		public UserSecretHashAlgorithm Algorithm => UserSecretHashAlgorithm.Pbkdf2HmacSha1;
		public byte[] Hash => record.GS_PasswordHash;
		public byte[] Salt => record.GS_PasswordSalt;
		public int IterationsCount => record.GS_PasswordHashIterations;

		public void Update(UserSecretHashAlgorithm algorithm, byte[] hash, byte[] salt, int iterationsCount)
		{
			if (algorithm != UserSecretHashAlgorithm.Pbkdf2HmacSha1)
			{
				throw new InvalidOperationException("Only PBKDF2-HMAC-SHA1 is supported in the current application.");
			}

			record.GS_PasswordHash = hash;
			record.GS_PasswordSalt = salt;
			record.GS_PasswordHashIterations = iterationsCount;
		}
	}
}
