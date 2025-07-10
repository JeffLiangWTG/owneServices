using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyAllocationMutex : ZGlobalMutex
	{
		public AgencyAllocationMutex(JobVoyage voyage)
			: base(MutexIDs.AgencyAllocation, voyage.PK.ToString()) { }

		public (string Caption, string Message, bool AllowRelease) GetFriendlyMessage(LockInfo info)
		{
			var caption = Res.GetString("ff77aa1e-1388-4613-9d84-5b6df1809524", "Another user has this voyage locked");
			var message = Res.GetString("a3e1d9ac-e269-40c9-a92c-4f0918a17af7", "Another user has this voyage locked. Try again later.");
			var allowRelease = false;
			const int timeoutInMinutes = 5;

			if (info != null && info.UserWithLock != null)
			{
				if (Env.CurrentUser.IsController)
				{
					message = Res.GetString("83a637af-e48d-40f4-9f17-4e4223e9b56d", "User {0} has this voyage locked since {1}. Do you want to release the existing lock?", info.UserWithLock.GS_LoginName, info.LockStartTime);
					allowRelease = true;
				}
				else if ((ZDateTime.UtcNow - info.LockStartTime).TotalMinutes > timeoutInMinutes)
				{
					message = Res.GetString("8cd3dad7-adc9-43e3-bf16-8b9bca21d7be", "User {0} has this voyage locked since {1}. Please contact a controller to release the existing lock.", info.UserWithLock.GS_LoginName, info.LockStartTime);
				}
				else
				{
					message = Res.GetString("faf3bdb7-b8e7-4ea9-ae79-17b783bf492b", "User {0} has this voyage locked since {1}. Try again later.", info.UserWithLock.GS_LoginName, info.LockStartTime);
				}
			}

			return (caption, message, allowRelease);
		}

		public void ReleaseLocks(LockInfo info)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(info.UserWithLock, nameof(info.UserWithLock));

			ReleaseLocks(info.UserWithLock.PK.ToGuid());
		}
	}
}
