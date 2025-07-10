using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.AIM.Messaging;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AsycudaTransferBill : ASYCUDA.Business.AsycudaTransferBill
	{
		public AsycudaTransferBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ASYCUDA.Business.AsycudaTransferBill.Schema
		{
			public const string InBondNumber = "InBondNumber";
			public const string ATB_MessageStatusDescription = "ATB_MessageStatusDescription";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ATB_BillOfLadingType = Core.Constants.ShipmentTypes.StandardHouse;
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || (TransferHeader?.ReadOnly ?? false);
			set => base.ReadOnly = value;
		}

		#region ATB_BillNumber

		[List(nameof(Lookups) + "." + nameof(AsycudaTransferBillLookups.ManifestBillsList))]
		public override ZString ATB_BillNumber
		{
			get => base.ATB_BillNumber;
			set
			{
				var oldValue = ATB_BillNumber;
				base.ATB_BillNumber = value;
				if (ATB_BillNumber != oldValue && !IsCopying)
				{
					SynchroniseATB_BillNumberToATB_ABL_Bill();
				}
			}
		}

		void SynchroniseATB_BillNumberToATB_ABL_Bill()
		{
			var billInfoIndex = Lookups.ManifestBillsList.IndexOfCode(ATB_BillNumber);
			if (billInfoIndex >= 0)
			{
				ATB_ABL_Bill = (ZGuid)Lookups.ManifestBillsList[billInfoIndex].PK;

				var bill = Bill;
				ATB_BillOfLadingType = bill != null && !bill.ABL_BolType.IsEmpty ? bill.ABL_BolType : (ZString)Core.Constants.ShipmentTypes.StandardHouse;
			}
			else
			{
				ATB_ABL_Bill = ZGuid.Empty;
				ATB_BillOfLadingType = Core.Constants.ShipmentTypes.StandardHouse;
			}
		}

		#endregion

		#region ATB_ABL_Bill

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(ATB_ABL_Bill);

		[RelatedBusinessObject(nameof(Bill))]
		public override ZGuid ATB_ABL_Bill
		{
			get => base.ATB_ABL_Bill;
			set => base.ATB_ABL_Bill = value;
		}

		#endregion

		#region ATB_MessageStatus

		public override ZString ATB_MessageStatus
		{
			get => base.ATB_MessageStatus;
			set
			{
				var oldValue = ATB_MessageStatus;
				base.ATB_MessageStatus = value;
				if (ATB_MessageStatus != oldValue)
				{
					ATB_MessageStatusDescriptionInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("ACEManifest.Business.AsycudaTransferBill|ATB_MessageStatusDescription", Caption = "Message Status Desc.")]
		public ZString ATB_MessageStatusDescription => Lookups.TransferMessageStatusList.GetDescriptionFromCode(ATB_MessageStatus);

		public ZPropertyInfo ATB_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ATB_MessageStatusDescription); }
		}

		public bool ATB_MessageStatus_ReadOnly => true;
		public bool ATB_MessageStatusDescription_ReadOnly => true;

		public bool IsActive => ATB_MessageStatus != AIMTransferStatusCodes.Codes.Arrived && ATB_MessageStatus != AIMTransferStatusCodes.Codes.TransferCancelled;
		public bool IsSubmitted => !ATB_MessageStatus.IsEmpty && ATB_MessageStatus != AIMTransferStatusCodes.Codes.ArrivalError && ATB_MessageStatus != AIMTransferStatusCodes.Codes.TransferError;
		public bool IsSendable => ATB_MessageStatus.IsEmpty || ATB_MessageStatus == AIMTransferStatusCodes.Codes.TransferError;
		public bool IsCancellable => ATB_MessageStatus == AIMTransferStatusCodes.Codes.TransferSent || ATB_MessageStatus == AIMTransferStatusCodes.Codes.TransferAccepted;

		#endregion

		#region CustomsStatus

		public override ZString ATB_CustomsStatus
		{
			get { return base.ATB_CustomsStatus; }
			set
			{
				var oldValue = ATB_CustomsStatus;
				base.ATB_CustomsStatus = value;
				if (!IsCopying && oldValue != ATB_CustomsStatus)
				{
					LogManager.AddALogIfNecessary(oldValue, ATB_CustomsStatus);
				}
			}
		}

		#endregion

		#region InBondNumber

		[ResourceStringData("ACEManifest.Business.AsycudaTransferBill|InBondNumber", Caption = "In-Bond Number")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString InBondNumber
		{
			get
			{
				return InBondNumberObj?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				var oldValue = InBondNumber;
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayInBondNumber();
					}
					else
					{
						inBondNumberObj = CreateInBondNumberObjIfNeeded();
						inBondNumberObj.CE_EntryNum = value;
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateInBondNumber();
				}
				InBondNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo InBondNumberInfo
		{
			get { return GetZPropertyInfo(Schema.InBondNumber); }
		}

		public bool InBondNumber_ReadOnly => true;

		CusEntryNumber CreateInBondNumberObjIfNeeded()
		{
			return InBondNumberObj ?? CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.ImportControlNumber, Core.Constants.CountryCodes.UnitedStates);
		}

		void ThrowAwayInBondNumber()
		{
			InBondNumberObj?.Delete();
		}

		public CusEntryNumber InBondNumberObj
		{
			get
			{
				if (inBondNumberObj == null || inBondNumberObj.IsDeleted)
				{
					inBondNumberObj = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.ImportControlNumber, Core.Constants.CountryCodes.UnitedStates, true);
				}
				return inBondNumberObj;
			}
		}
		CusEntryNumber inBondNumberObj;

		#region Allocate In-Bond Number

		/// <summary>
		/// this in-bond number has been created to allow allocation of an in-bond number to a movement before it is sent.
		/// This in-bond number is added to the CusEntryNum table, and when in-bond numbers are allocated as part of the saving of MQEDIMessage,
		/// the process first checks to see whether this number exists. If it does, then this will become the in-bond number for the movement.
		/// </summary>
		public ZString DisallowAllocateInBondNumber
		{
			get { return DoubleCheckThatInBondNumberIsEmpty() ? "" : CusInBondMoveHeader.Constants.InBondNumberAlreadyAllocated(InBondNumber); }
		}

		protected bool DoubleCheckThatInBondNumberIsEmpty()
		{
			ReloadInBondNumber();
			return InBondNumber.IsEmpty;
		}

		public void ReloadInBondNumber()
		{
			if (IsInDatabase)
			{
				var currentInBondNumber = inBondNumberObj == null || inBondNumberObj.IsDeleted ? ZString.Empty : inBondNumberObj.CE_EntryNum;
				if (inBondNumberObj == null)
				{
					inBondNumberObj = CusEntryNumber.Load(this, CusEntryNumberTypes.Standard.ImportControlNumber, Core.Constants.CountryCodes.UnitedStates, true);
				}
				else if (inBondNumberObj.IsInDatabase)
				{
					inBondNumberObj.Reload();
				}

				if (inBondNumberObj != null && !inBondNumberObj.IsDeleted && inBondNumberObj.CE_EntryNum != currentInBondNumber)
				{
					InBondNumberInfo.RefreshBinding(currentInBondNumber);
				}
			}
		}

		public void AllocateInBondNumber(string userEnteredInBondNumber)
		{
			if (string.IsNullOrEmpty(userEnteredInBondNumber))
			{
				allocateInBondNumberOnSaving = true;
			}
			else
			{
				InBondNumber = userEnteredInBondNumber;
				InBondNumberObj.CE_EntryIsSystemGenerated = false;
			}
		}
		bool allocateInBondNumberOnSaving;

		void AllocateNextInBondNumber()
		{
			ZString inbondNumber;
			if (InBondNumberGenerator.TryGetNextInBondNumber(HeaderBranch, out inbondNumber))
			{
				InBondNumber = inbondNumber;
				var obj = InBondNumberObj;
				if (obj != null)
				{
					obj.CE_EntryIsSystemGenerated = true;
				}
			}
		}

		public bool LockInBondNumberAllocationMutex()
		{
			return InBondNumberAllocationMutex.IsLocked ? (bool)InBondNumberAllocationMutex.HasLock : InBondNumberAllocationMutex.Lock();
		}

		public void UnLockInBondNumberAllocationMutex()
		{
			if (inBondNumberAllocationMutex != null && inBondNumberAllocationMutex.IsLocked && inBondNumberAllocationMutex.HasLock)
			{
				inBondNumberAllocationMutex.Unlock();
			}
		}

		public bool InBondNumberAllocationMutexHasLock()
		{
			return InBondNumberAllocationMutex.HasLock;
		}

		public bool InBondNumberAllocationMutexIsLocked()
		{
			return InBondNumberAllocationMutex.IsLocked;
		}

		public string GetInBondNumberAllocationMutexLockInfo() => InBondNumberAllocationMutex.GetMutexLockByInfo();

		ZGlobalMutex InBondNumberAllocationMutex
		{
			get
			{
				if (inBondNumberAllocationMutex == null)
				{
					inBondNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, "INB" + PK.ToString());
				}
				return inBondNumberAllocationMutex;
			}
		}
		ZGlobalMutex inBondNumberAllocationMutex;

		public static string InBondNumberAllocationMutexLockText(string lockInfo)
		{
			return Res.GetString("ACEManifest.Business.AsycudaTransferBill|InBondNumberAllocationMutex", "{0} is in the process of allocating an InBond Number for this movement.\r\nPlease re-open the job later.", lockInfo);
		}

		#endregion

		#region ResetInBondNumber

		public void ResetInBondNumber(ZString reason)
		{
			Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(ResetInBondNumberMessage, InBondNumber, reason));
			InBondNumber = ZString.Empty;
		}

		public ZBool IsInBondNumberResetable
		{
			get { return !IsInBondNumberKnownToCustoms; }
		}

		bool IsInBondNumberKnownToCustoms
		{
			get { return IsWaitingForResponse || (!LogManager.HasAWithdrawnLog && LogManager.HasAClearLog); }
		}

		public bool IsWaitingForResponse
		{
			get { return LogManager.IsAnAwaitingStatus(ATB_CustomsStatus); }
		}

		public const string ResetInBondNumberMessage = "Previous In-Bond #: {0}. Reset reason: {1}.";

		#endregion
		#endregion

		#region Implementation

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (allocateInBondNumberOnSaving)
			{
				oldInBondNumber = InBondNumber;
				AllocateNextInBondNumber();
			}
		}
		ZString oldInBondNumber;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (allocateInBondNumberOnSaving)
			{
				allocateInBondNumberOnSaving = false;
				if (saveSucceeded)
				{
					InBondNumberAvailabilityChecker.ReportLimitHasReachedIfNeeded(HeaderBranch);
				}
				else
				{
					InBondNumber = oldInBondNumber;
				}
				oldInBondNumber = ZString.Empty;
			}
		}

		public override void Delete()
		{
			if (!InBondNumberAllocationMutexIsLocked())
			{
				ThrowAwayInBondNumber();
				UnLockInBondNumberAllocationMutex();
				base.Delete();
			}
		}

		public bool HasBeenDeleted
		{
			get
			{
				var isDeleted = IsDeleted;
				if (!isDeleted && IsInDatabase)
				{
					isDeleted = new BusinessObjectFactory().Load<AsycudaTransferBill>(this.PK)?.IsDeleted ?? true;
				}
				return isDeleted;
			}
		}

		public override bool CanDelete => !InBondNumberAllocationMutexIsLocked() && base.CanDelete;

		public override MultilingualString ReasonForNotAbleToDelete => InBondNumberAllocationMutexIsLocked() ? ResString.GetMultilingualString("980A981C-1938-4EB5-BFFD-1D0B0D289AB1", "{0} is in the process of allocating a new InBond Number for this movement; this movement cannot be deleted.", GetInBondNumberAllocationMutexLockInfo()) : base.ReasonForNotAbleToDelete;

		public GlbBranch HeaderBranch => TransferHeader.ArrivalHeader.ManifestHeader.Branch;

		public InBond.Business.StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new InBond.Business.StatusLogManager(Logs)); }
		}
		InBond.Business.StatusLogManager fLogManager;

		#endregion

		public new AsycudaTransferHeader TransferHeader => (AsycudaTransferHeader)base.TransferHeader;

		public new AsycudaTransferBillLookups Lookups => (AsycudaTransferBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaTransferBillLookups GetNewLookups() => new AsycudaTransferBillLookups(this);
		public new AsycudaTransferBillValidation Validation => (AsycudaTransferBillValidation)base.Validation;
		protected override ManifestBase.AsycudaTransferBillValidation GetNewValidation() => new AsycudaTransferBillValidation(this);
	}
}
