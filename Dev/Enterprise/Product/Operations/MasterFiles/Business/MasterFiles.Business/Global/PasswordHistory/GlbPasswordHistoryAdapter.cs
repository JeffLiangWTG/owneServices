using System;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	struct GlbPasswordHistoryAdapter : IUserSecretHashComponents
	{
		public GlbPasswordHistoryAdapter(GlbPasswordHistory record)
		{
			this.record = record;
		}

		readonly GlbPasswordHistory record;

		public UserSecretHashAlgorithm Algorithm => UserSecretHashAlgorithmExtensions.FromStringRepresentation(record.PWH_Algorithm);
		public byte[] Hash => record.PWH_Hash;
		public byte[] Salt => record.PWH_Salt;
		public int IterationsCount => record.PWH_IterationCount;

		public void Update(UserSecretHashAlgorithm algorithm, byte[] hash, byte[] salt, int iterationsCount)
		{
			if (algorithm != UserSecretHashAlgorithm.Pbkdf2HmacSha1)
			{
				throw new InvalidOperationException("Only PBKDF2-HMAC-SHA1 is supported in the current application.");
			}

			record.PWH_Algorithm = algorithm.GetStringRepresentation();
			record.PWH_Hash = hash;
			record.PWH_Salt = salt;
			record.PWH_IterationCount = iterationsCount;
		}
	}
}
