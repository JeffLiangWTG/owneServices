using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Semaphores.Common;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveUsersModuleBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ActiveUsersModuleBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");
		}

		#region Active Users

		public new void Refresh()
		{
			using (fActiveUsers.SuspendListChanged())
			{
				var dbActiveUsers = GetAllInternalActiveUserSessions();
				var count = fActiveUsers.Count;

				for (int i = count - 1; i >= 0; i--)
				{
					var item = fActiveUsers[i];
					var key = item.AU_HeartbeatId.ToGuid();
					var isAliveOnDb = dbActiveUsers.Remove(key);

					if (!isAliveOnDb)
					{
						fActiveUsers.Remove(item);
					}
				}

				foreach (var userInfo in dbActiveUsers.Values)
				{
					var aUser = fActiveUsers.AddNew();
					aUser.AU_HeartbeatId = userInfo.HeartbeatId;
					aUser.AU_FullName = userInfo.FullName;
					aUser.AU_GS = userInfo.UserPk;
					aUser.AU_ComputerName = userInfo.ComputerName;
					aUser.AU_ProcessID = userInfo.ProcessId;
					aUser.AU_UTCLoginTime = userInfo.LoginTimeUtc;
				}
			}
		}

		protected virtual Dictionary<Guid, IActiveUserSession> GetAllInternalActiveUserSessions() =>
																	ActiveUserQuery.GetAllInternalActiveUserSessions(true)
																	.Where(x => x.UserType == LogonType.Staff)
																	.ToDictionary(x => x.HeartbeatId);

		public ActiveUsersCollection ActiveUsers
		{
			get
			{
				if (fActiveUsers == null)
				{
					fActiveUsers = new ActiveUsersCollection(Factory);
					Refresh();
				}

				return fActiveUsers;
			}
		}

		ActiveUsersCollection fActiveUsers;

		#endregion
	}
}
