using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class AllocateSelectionJobMawb : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string ReservedUntil = "ReservedUntil";
			public const string IsCompletedAWBReturned = "IsCompletedAWBReturned";
			public const string IsBorrowedAWBInvoiced = "IsBorrowedAWBInvoiced";
			public const string AllocatedTo = "AllocatedTo";
		}

		public AllocateSelectionJobMawb(BusinessObjectFactory factory, BusinessObject[] jobMawbs) : base(factory)
		{
			SelectedJobMawbs = jobMawbs;
		}

		#region Process

		public void Process()
		{
			foreach (JobMawb job in SelectedJobMawbs)
			{
				var savableJobMawb = Factory.Load<JobMawb>(job.PK);

				savableJobMawb.JM_OH_AllocatedTo = AllocatedTo;
				savableJobMawb.JM_ReservedUntil = ReservedUntil;
				savableJobMawb.JM_IsCompletedAWBReturned = IsCompletedAWBReturned;
				savableJobMawb.JM_IsBorrowedAWBInvoiced = IsBorrowedAWBInvoiced;
			}
		}

		#endregion

		#region Lists

		protected OrgHeaderCollection fForwarderList;
		public OrgHeaderCollection ForwarderList
		{
			get
			{
				if (fForwarderList == null)
				{
					fForwarderList = new ForwarderCollection(Factory);
				}
				return fForwarderList;
			}
		}

		#endregion

		#region Properties

		#region AllocatedTo

		[List("ForwarderList")]
		public ZGuid AllocatedTo
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fAllocatedTo; }
			set
			{
				SetNonPersistentPropertyValue(AllocatedToInfo, ref fAllocatedTo, value);
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateAllocatedTo();
				}
			}
		}
		protected ZGuid fAllocatedTo;

		public void ValidateAllocatedTo()
		{
			AllocatedToInfo.ClearAllNotifications();
			CheckAllocatedToIsEmpty();
			TypeValidation.CheckValidGuid(AllocatedToInfo);
		}

		public ZPropertyInfo AllocatedToInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AllocatedTo); }
		}

		protected void CheckAllocatedToIsEmpty()
		{
			if (AllocatedTo.IsEmpty && !AllocatedToInfo.HasErrors())
			{
				AllocatedToInfo.AddError(Res.GetString("feb7e009-9a62-4df0-a865-63f0cdd1ae35", "You need to allocate the Waybills to a forwarder before changes can be made."));
			}
		}

		#endregion

		#region ReservedUntil

		public ZDateTime ReservedUntil
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fReservedUntil; }
			set
			{
				SetNonPersistentPropertyValue(ReservedUntilInfo, ref fReservedUntil, value);
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateReservedUntil();
				}
			}
		}
		protected ZDateTime fReservedUntil;

		public void ValidateReservedUntil()
		{
			ReservedUntilInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ReservedUntilInfo);
		}

		public ZPropertyInfo ReservedUntilInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ReservedUntil); }
		}

		#endregion

		#region IsCompletedAWBReturned

		public ZBool IsCompletedAWBReturned
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIsCompletedAWBReturned; }
			set
			{
				SetNonPersistentPropertyValue(IsCompletedAWBReturnedInfo, ref fIsCompletedAWBReturned, value);
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateIsCompletedAWBReturned();
				}
			}
		}
		protected ZBool fIsCompletedAWBReturned;

		public void ValidateIsCompletedAWBReturned()
		{
			CheckAllocatedToIsEmpty();
			IsCompletedAWBReturnedInfo.ClearAllNotifications();
		}

		public ZPropertyInfo IsCompletedAWBReturnedInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IsCompletedAWBReturned); }
		}

		#endregion

		#region IsBorrowedAWBInvoiced

		public ZBool IsBorrowedAWBInvoiced
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return fIsBorrowedAWBInvoiced; }
			set
			{
				SetNonPersistentPropertyValue(IsBorrowedAWBInvoicedInfo, ref fIsBorrowedAWBInvoiced, value);
				HasChanges = true;
				if (!IsValidationSuspended)
				{
					ValidateIsBorrowedAWBInvoiced();
				}
			}
		}
		protected ZBool fIsBorrowedAWBInvoiced;

		public void ValidateIsBorrowedAWBInvoiced()
		{
			CheckAllocatedToIsEmpty();
			IsBorrowedAWBInvoicedInfo.ClearAllNotifications();
		}

		public ZPropertyInfo IsBorrowedAWBInvoicedInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.IsBorrowedAWBInvoiced); }
		}

		#endregion

		#endregion

		#region Implementation

		protected BusinessObject[] SelectedJobMawbs;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateIsBorrowedAWBInvoiced();
			ValidateIsCompletedAWBReturned();
			ValidateAllocatedTo();
			ValidateReservedUntil();

			base.RunPreSaveValidationCore();
		}

		#endregion
	}
}
