using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ActiveSemaphoreHandle : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ActiveSemaphoreHandle(BusinessObjectFactory factory, ActiveUser parent) : base(factory)
		{
			Argument.NotNull(factory, "Factory");
			Argument.NotNull(parent, "Parent");

			Parent = parent;
		}

		#region Schema

		public static class Schema
		{
			public const string AS_ServiceClass = "AS_ServiceClass";
			public const string AS_LockInfo = "AS_LockInfo";
			public const string AS_UseCount = "AS_UseCount";
			public const string AS_AcquiredTimeUTC = "AS_AcquiredTimeUTC";
		}

		#endregion

		#region Properties

		public ActiveUser Parent { get; private set; }

		#region AS_ServiceClass

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString AS_ServiceClass
		{
			get { return fAS_ServiceClass; }
			set
			{
				CheckMaximumLength(AS_ServiceClassInfo, value);
				SetNonPersistentPropertyValue(AS_ServiceClassInfo, ref fAS_ServiceClass, value);
			}
		}
		ZString fAS_ServiceClass;

		public ZPropertyInfo AS_ServiceClassInfo
		{
			get { return GetZPropertyInfo(Schema.AS_ServiceClass); }
		}

		#endregion

		#region AS_LockInfo

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString AS_LockInfo
		{
			get { return fAS_LockInfo; }
			set
			{
				CheckMaximumLength(AS_LockInfoInfo, value);
				SetNonPersistentPropertyValue(AS_LockInfoInfo, ref fAS_LockInfo, value);
			}
		}
		ZString fAS_LockInfo;

		public ZPropertyInfo AS_LockInfoInfo
		{
			get { return GetZPropertyInfo(Schema.AS_LockInfo); }
		}

		#endregion

		#region AS_UseCount

		[ReadOnly(true)]
		public ZInt AS_UseCount
		{
			get { return fAS_UseCount; }
			set { SetNonPersistentPropertyValue(AS_UseCountInfo, ref fAS_UseCount, value); }
		}
		ZInt fAS_UseCount;

		public ZPropertyInfo AS_UseCountInfo
		{
			get { return GetZPropertyInfo(Schema.AS_UseCount); }
		}

		#endregion

		#region AS_AcquiredTimeUTC

		[ReadOnly(true)]
		public ZDateTime AS_AcquiredTimeUTC
		{
			get { return fAS_AcquiredTimeUTC; }
			set { SetNonPersistentPropertyValue(AS_AcquiredTimeUTCInfo, ref fAS_AcquiredTimeUTC, value); }
		}
		ZDateTime fAS_AcquiredTimeUTC;

		public ZPropertyInfo AS_AcquiredTimeUTCInfo
		{
			get { return GetZPropertyInfo(Schema.AS_AcquiredTimeUTC); }
		}

		#endregion

		#region AcquiredTimeUserLocal

		public ZDateTime AcquiredTimeUserLocal
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!AS_AcquiredTimeUTC.IsEmpty)
				{
					var staff = Parent.Staff;
					GlbBranch homeBranch = staff != null ? staff.HomeBranch : null;
					if (homeBranch != null)
					{
						result = Environment.Env.Time.GetUnlocoTimeFromUtc(homeBranch.GB_RL_NKHomePort, AS_AcquiredTimeUTC.ToDateTime());
					}
				}
				return result;
			}
		}

		#endregion

		#region AcquiredTimeYourLocal

		public ZDateTime AcquiredTimeYourLocal
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!AS_AcquiredTimeUTC.IsEmpty)
				{
					result = Environment.Env.Time.GetLocalTimeFromUtc(AS_AcquiredTimeUTC.ToDateTime());
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
