using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.eTail.Business
{
	public static class HVLVApplicationLockHelper
	{
		public static bool TryAcquireApplicationLocks<T>(this IList<ZGuid> pks, string appLockKey, Action<IEnumerable<ZGuid>> actionForLockedRows, out string errorMessage) where T : BusinessObject
		{
			using (var lockResult = pks.ApplyAppLocks(appLockKey))
			{
				if (lockResult.ItemsWithLocks.Any())
				{
					actionForLockedRows?.Invoke(lockResult.Values);
					errorMessage = string.Empty;
					return true;
				}
				else
				{
					errorMessage = GetAcquireApplicationLockFailedErrorMessage(typeof(T));
					return false;
				}
			}
		}

		public static bool TryAcquireApplicationLock<T>(this ZGuid pk, string appLockKey, Action actionForLocked, out string errorMessage) where T : BusinessObject
		{
			return TryAcquireApplicationLocks<T>(new[] { pk }, appLockKey, _ => actionForLocked.Invoke(), out errorMessage);
		}

		static string GetAcquireApplicationLockFailedErrorMessage(Type bizoType)
		{
			var schema = BusinessObjectFactory.GetTableSchemaFromType(bizoType);
			return Res.GetString("d85fb74f-1ed4-4ab7-9912-e84f71e52981", "Failed to acquire lock for rows in table {0}, data being processed by other user.", schema.TableName);
		}
	}
}
