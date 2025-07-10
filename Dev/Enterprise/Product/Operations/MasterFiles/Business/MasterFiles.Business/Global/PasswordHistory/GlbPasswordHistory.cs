using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPasswordHistory : AutoGlbPasswordHistory, IPasswordStored
	{
		public GlbPasswordHistory(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsMatched(string password, byte[] lookupHash = null)
			=> IsMatched(password, UserSecretsContext.DefaultContext, lookupHash);

		public bool IsMatched(string password, IUserSecretsContext userSecretsContext, byte[] lookupHash = null)
		{
			if (userSecretsContext == null)
			{
				throw new ArgumentNullException(nameof(userSecretsContext));
			}

			if (password == null)
			{
				return false;
			}

			if (lookupHash == null)
			{
				lookupHash = PasswordHistoryHelper.HashPasswordForLookup(PWH_ParentID.ToGuid(), password);
			}

			if (!PWH_TruncatedHash.IsEmpty && PWH_TruncatedHash != lookupHash)
			{
				return false;
			}

			try
			{
				return userSecretsContext.IsMatchingSecret(password, new GlbPasswordHistoryAdapter(this));
			}
			catch (Exception ex) when (ex is ArgumentException)
			{
				if (Parent != null && Parent.ShouldSavePasswordHistory)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($@"The password history is corrupted, please investigate why this happen
PWH_PK: {PK}
PWH_ParentTableCode: {PWH_ParentTableCode}
PWH_ParentID: {PWH_ParentID}
PWH_Hash: {Convert.ToBase64String(this.PWH_Hash)}
PWH_Salt: {Convert.ToBase64String(PWH_Salt)}
PWH_IterationCount: {PWH_IterationCount}
PWH_Algorithm: {PWH_Algorithm}
"), ex);
				}

				return false;
			}
		}

		#region IPasswordStored

		int IPasswordStored.PasswordHashIterations => PWH_IterationCount;

		ZBlob IPasswordStored.PasswordSalt => PWH_Salt;

		ZBlob IPasswordStored.PasswordHash => PWH_Hash;

		bool IPasswordStored.VerifyPassword(IUserSecretsContext userSecretsContext, string password) => IsMatched(password, userSecretsContext);

		#endregion

		IGlbPasswordHistoryParent Parent
		{
			get
			{
				if (PWH_ParentID.IsValid && PWH_ParentTableCode.IsValid)
				{
					return Factory.GetCachedValue(FormattableString.Invariant($"{PWH_ParentTableCode}:{PWH_ParentID}"), () =>
					{
						var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(PWH_ParentTableCode, false);
						return parentType != null ? (IGlbPasswordHistoryParent)Factory.Load(parentType, PWH_ParentID) : null;
					});
				}
				else
				{
					return null;
				}
			}
		}

		[DocumentMacroIgnore]
		public override ZBlob PWH_Hash { get => base.PWH_Hash; set => base.PWH_Hash = value; }

		[DocumentMacroIgnore]
		public override ZBlob PWH_Salt { get => base.PWH_Salt; set => base.PWH_Salt = value; }
	}
}
