using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public static class PasswordHistoryHelper
	{
		internal static GlbPasswordHistory NewPasswordHistory(IGlbPasswordHistoryParent parent, string password)
		{
			if (!parent.ShouldSavePasswordHistory)
			{
				return null;
			}

			if (password == null || CWSupportLoginToken.IsValidToken(password))
			{
				// password reset, or using Support Token - record current password to history
				return NewPasswordHistoryFromCurrentPasswordHash(parent);
			}

			if (string.IsNullOrEmpty(password))
			{
				// should not record empty password to history
				return null;
			}

			var result = parent.BusinessEntity.Factory.New<GlbPasswordHistory>();
			result.PWH_ParentID = parent.BusinessEntity.PK;
			result.PWH_ParentTableCode = parent.BusinessEntity.TablePrefix;
			result.PWH_TruncatedHash = HashPasswordForLookup(parent.BusinessEntity.PK.ToGuid(), password);
			result.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow;
			// when a user is changing password during login, CurrentUser could be null as it hasn't logged in, but we know it is the user itself changing the password
			result.PWH_SystemCreateUser = GlbStaff.CurrentUser?.GS_Code ?? (parent as GlbStaff)?.GS_Code ?? User.ServiceUserCode;
			UserSecretsContext.DefaultContext.SaveSecret(password, UserSecretHashAlgorithmExtensions.PreferredAlgorithm, DataRegistry.Instance.PasswordHashingIterationsCount, new GlbPasswordHistoryAdapter(result));
			return result;
		}

		static GlbPasswordHistory NewPasswordHistoryFromCurrentPasswordHash(IGlbPasswordHistoryParent parent)
		{
			if (!parent.ShouldSavePasswordHistory)
			{
				return null;
			}

			if (parent.PasswordHash.IsEmpty || parent.PasswordSalt.IsEmpty || parent.PasswordHashIterations == ZInt.Zero)
			{
				// Don't save password with corrupted/empty hash, salt or iteration to history
				return null;
			}

			var result = parent.BusinessEntity.Factory.New<GlbPasswordHistory>();
			result.PWH_ParentID = parent.BusinessEntity.PK;
			result.PWH_ParentTableCode = parent.BusinessEntity.TablePrefix;
			result.PWH_Algorithm = UserSecretHashAlgorithmExtensions.GetStringRepresentation(UserSecretHashAlgorithmExtensions.PreferredAlgorithm);
			result.PWH_TruncatedHash = null;
			result.PWH_IterationCount = parent.PasswordHashIterations;
			result.PWH_Salt = parent.PasswordSalt;
			result.PWH_Hash = parent.PasswordHash;
			result.PWH_SystemCreateTimeUtc = ZDateTime.UtcNow;
			// when a user is changing password during login, CurrentUser could be null as it hasn't logged in, but we know it is the user itself changing the password
			result.PWH_SystemCreateUser = GlbStaff.CurrentUser?.GS_Code ?? (parent as GlbStaff)?.GS_Code ?? User.ServiceUserCode;
			return result;
		}

		public static byte[] HashPasswordForLookup(Guid parentPK, string password)
		{
			var bytes = Encoding.Unicode.GetBytes(parentPK + password);
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(bytes).Take(4).ToArray();
			}
		}

		public static void AddPasswordHistory(IGlbPasswordHistoryParent parent, string password)
		{
			if (parent.PasswordHistoryCount <= 0)
			{
				return;
			}

			var passwordHistory = NewPasswordHistory(parent, password);
			if (passwordHistory == null)
			{
				return;
			}

			var passwordHistories = GetPasswordHistories(parent);
			passwordHistories.Add(passwordHistory);

			while (passwordHistories.Count > Math.Max(0, parent.PasswordHistoryCount - 1))
			{
				passwordHistories.RemoveAndDelete(passwordHistories.First());
			}
		}

		public static bool HasPasswordBeenUsed(IGlbPasswordHistoryParent parent, string password)
		{
			var passwordHistories = GetPasswordHistories(parent);
			var lookupHash = HashPasswordForLookup(parent.BusinessEntity.PK.ToGuid(), password);
			// Should check for current password and GlbPasswordHistory
			return parent.VerifyPassword(password) || passwordHistories.Cast<GlbPasswordHistory>().OrderByDescending(p => p.PWH_SystemCreateTimeUtc).Take(Math.Max(0, parent.PasswordHistoryCount - 1)).Any(p => p.IsMatched(password, lookupHash));
		}

		public static GlbPasswordHistoryCollection GetPasswordHistories(IGlbPasswordHistoryParent parent)
		{
			var passwordHistories = new GlbPasswordHistoryCollection(parent);
			passwordHistories.Load();
			return passwordHistories;
		}

		public static void MovePasswordHistories(IGlbPasswordHistoryParent source, IGlbPasswordHistoryParent destination)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (destination == null)
			{
				throw new ArgumentNullException(nameof(destination));
			}

			var sourceCollection = GetPasswordHistories(source);
			var histories = sourceCollection.Cast<GlbPasswordHistory>().ToList();
			sourceCollection.RemoveAll();

			var currentPasswordHashAsHistory = NewPasswordHistoryFromCurrentPasswordHash(source);
			if (currentPasswordHashAsHistory != null)
			{
				histories.Add(currentPasswordHashAsHistory);
			}

			if (histories.Any())
			{
				var destinationCollection = GetPasswordHistories(destination);
				histories.ForEach(x =>
				{
					((IBusinessObjectInternals)x).ParentCollections.ToList().ForEach(p => p.Remove(x));
					destinationCollection.Add(x);
					x.PWH_TruncatedHash = ZBlob.Empty;
				});

				while (destinationCollection.Count > Math.Max(0, destination.PasswordHistoryCount - 1))
				{
					destinationCollection.RemoveAndDelete(destinationCollection.First());
				}
			}
		}
	}
}
