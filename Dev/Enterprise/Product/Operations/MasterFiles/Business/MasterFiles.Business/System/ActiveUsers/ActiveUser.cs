using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveUser : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ActiveUser(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "Factory");
		}

		#region Schema

		public static class Schema
		{
			public const string AU_HeartbeatId = "AU_HeartbeatId";
			public const string AU_FullName = "AU_FullName";
			public const string AU_GS = "AU_GS";
			public const string AU_Initials = "AU_Initials";
			public const string AU_UTCLoginTime = "AU_UTCLoginTime";
			public const string AU_YourLocalLoginTime = "AU_YourLocalLoginTime";
			public const string AU_UserLocalLoginTime = "AU_UserLocalLoginTime";
			public const string AU_ComputerName = "AU_ComputerName";
			public const string AU_ProcessID = "AU_ProcessID";
		}

		#endregion

		#region Properties

		public GlbStaff Staff
		{
			get { return Factory.Load<GlbStaff>(AU_GS); }
		}

		#region AU_HeartbeatId

		public ZGuid AU_HeartbeatId
		{
			get { return fAU_HeartbeatId; }
			set
			{
				SetNonPersistentPropertyValue(AU_HeartbeatIdInfo, ref fAU_HeartbeatId, value);
			}
		}

		ZGuid fAU_HeartbeatId;

		public ZPropertyInfo AU_HeartbeatIdInfo
		{
			get { return GetZPropertyInfo(Schema.AU_HeartbeatId); }
		}

		#endregion

		#region AU_FullName

		[CargoWise.ComponentModel.MaxLength(256)]
		public ZString AU_FullName
		{
			get { return fAU_FullName; }
			set
			{
				CheckMaximumLength(AU_FullNameInfo, value);
				SetNonPersistentPropertyValue(AU_FullNameInfo, ref fAU_FullName, value);
			}
		}

		ZString fAU_FullName;

		public ZPropertyInfo AU_FullNameInfo
		{
			get { return GetZPropertyInfo(Schema.AU_FullName); }
		}

		#endregion

		#region AU_GS

		public ZGuid AU_GS
		{
			get { return fAU_GS; }
			set
			{
				SetNonPersistentPropertyValue(AU_GSInfo, ref fAU_GS, value);
			}
		}

		ZGuid fAU_GS;

		public ZPropertyInfo AU_GSInfo
		{
			get { return GetZPropertyInfo(Schema.AU_GS); }
		}

		#endregion

		#region AU_Initials

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString AU_Initials
		{
			get { return Staff == null ? null : Staff.GS_Code; }
		}

		public ZPropertyInfo AU_InitialsInfo
		{
			get { return GetZPropertyInfo(Schema.AU_Initials); }
		}

		#endregion

		#region AU_UTCLoginTime

		public ZDateTime AU_UTCLoginTime
		{
			get { return fAU_UTCLoginTime; }
			set
			{
				SetNonPersistentPropertyValue(AU_UTCLoginTimeInfo, ref fAU_UTCLoginTime, value);
			}
		}
		ZDateTime fAU_UTCLoginTime;

		public ZPropertyInfo AU_UTCLoginTimeInfo
		{
			get { return GetZPropertyInfo(Schema.AU_UTCLoginTime); }
		}

		#endregion

		#region AU_UserLocalLoginTime

		public ZDateTime AU_UserLocalLoginTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!AU_UTCLoginTime.IsEmpty)
				{
					var staff = Staff;
					GlbBranch homeBranch = staff != null ? staff.HomeBranch : null;
					if (homeBranch != null)
					{
						result = Environment.Env.Time.GetUnlocoTimeFromUtc(homeBranch.GB_RL_NKHomePort, AU_UTCLoginTime.ToDateTime());
					}
				}
				return result;
			}
		}

		public ZPropertyInfo AU_UserLocalLoginTimeInfo
		{
			get { return GetZPropertyInfo(Schema.AU_UserLocalLoginTime); }
		}

		#endregion

		#region AU_YourLocalLoginTime

		public ZDateTime AU_YourLocalLoginTime
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!AU_UTCLoginTime.IsEmpty)
				{
					result = Environment.Env.Time.GetLocalTimeFromUtc(AU_UTCLoginTime.ToDateTime());
				}
				return result;
			}
		}

		public ZPropertyInfo AU_YourLocalLoginTimeInfo
		{
			get { return GetZPropertyInfo(Schema.AU_YourLocalLoginTime); }
		}

		#endregion

		#region AU_ComputerName

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString AU_ComputerName
		{
			get { return fAU_ComputerName; }
			set
			{
				CheckMaximumLength(AU_ComputerNameInfo, value);
				SetNonPersistentPropertyValue(AU_ComputerNameInfo, ref fAU_ComputerName, value);
			}
		}

		ZString fAU_ComputerName;

		public ZPropertyInfo AU_ComputerNameInfo
		{
			get { return GetZPropertyInfo(Schema.AU_ComputerName); }
		}

		#endregion

		#region AU_ProcessID

		public ZInt AU_ProcessID
		{
			get { return fAU_ProcessID; }
			set
			{
				SetNonPersistentPropertyValue(AU_ProcessIDInfo, ref fAU_ProcessID, value);
			}
		}
		ZInt fAU_ProcessID;

		public ZPropertyInfo AU_ProcessIDInfo
		{
			get { return GetZPropertyInfo(Schema.AU_ProcessID); }
		}

		#endregion

		#region Active Semaphores

		public ActiveSemaphoreHandleCollection ActiveSemaphores
		{
			get
			{
				if (activeSemaphores == null)
				{
					activeSemaphores = new ActiveSemaphoreHandleCollection(Factory, this);
					RegisterEditableChildObject(activeSemaphores);
					RefreshActiveSemaphores();
				}
				return activeSemaphores;
			}
		}
		ActiveSemaphoreHandleCollection activeSemaphores;

		public void RefreshActiveSemaphores()
		{
			if (activeSemaphores != null)
			{
				activeSemaphores.RemoveAll();

				IActiveSemaphoreHandle[] activeSemaphoreHandles = ActiveUserQuery.GetEnterpriseActiveSemaphoresForUser(
					AU_HeartbeatId.IsValid ? AU_HeartbeatId.ToGuid() : Guid.Empty);

				foreach (IActiveSemaphoreHandle semaphoreHandle in activeSemaphoreHandles)
				{
					ActiveSemaphoreHandle semaphore = activeSemaphores.AddNew();

					semaphore.AS_ServiceClass = semaphoreHandle.Category;
					semaphore.AS_LockInfo = semaphoreHandle.LockInfo;
					semaphore.AS_UseCount = semaphoreHandle.UseCount;
					semaphore.AS_AcquiredTimeUTC = semaphoreHandle.CreateTimeUtc;
				}
			}
		}

		#endregion

		#endregion
	}
}
