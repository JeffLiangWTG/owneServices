using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Semaphores.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ActiveUsersModuleBusinessObjectForTest : ActiveUsersModuleBusinessObject
	{
		readonly Dictionary<Guid, IActiveUserSession> activeUserSessionForTest = new Dictionary<Guid, IActiveUserSession>();

		public ActiveUsersModuleBusinessObjectForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Dictionary<Guid, IActiveUserSession> GetAllInternalActiveUserSessions()
		{
			return activeUserSessionForTest;
		}

		public void AddActiveUserSession(Guid heartbeatId, string fullName, Guid userPk, string logonIdentificationCode, LogonType userType, string computerName, int processId, DateTime loginTimeUtc, string loginSource)
		{
			var user = new ActiveUserForTest(heartbeatId, fullName, userPk, logonIdentificationCode, userType, computerName, processId, loginTimeUtc, loginSource);
			activeUserSessionForTest.Add(heartbeatId, user);
		}

		public void RemoveActiveUserSession(Guid heartbeatId)
		{
			activeUserSessionForTest.Remove(heartbeatId);
		}

		public void ReorderActiveUsers()
		{
			var bkp = ActiveUsers.ToList();
			ActiveUsers.RemoveAll();
			bkp.Reverse();

			foreach (var item in bkp)
			{
				ActiveUsers.Add(item);
			}
		}
	}
}
