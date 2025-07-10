using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class MAWBAllocation : IImportExport
	{
		JobMawb allocatedMawbInDatabase;

		public MAWBAllocation(IMAWBAllocationParent parent)
		{
			Argument.NotNull(parent, "parent");

			Parent = parent;
			Factory = Parent.Factory;
			Parent.Factory.Saved += OnParentFactorySaved;

			allocatedMawbInDatabase = AllocatedMawbInDatabase;
		}

		public JobMawb AllocatedMawbInDatabase
		{
			get { return allocatedMawbInDatabase ?? (allocatedMawbInDatabase = FreightJobMawbLink.LoadBOInTheDBFromParentPK(Parent.Prefix, Parent.PK)); }
		}
		public IMAWBAllocationParent Parent { get; private set; }
		public BusinessObjectFactory Factory { get; private set; }

		#region Allocated MAWB Information

		public JobMawb AllocatedMawb
		{
			get { return allocatedMawb ?? (allocatedMawb = FreightJobMawbLink.LoadFromParentPK(Parent.Prefix, Parent.PK)); }
			set { allocatedMawb = value; }
		}
		JobMawb allocatedMawb;

		public ZString MasterBillNeutralMAWB
		{
			get
			{
				return AllocatedMawb == null || PendingAllocation ?
				new ZString(Res.GetString("d230f698-9153-4573-b8de-0994bb42d93a", "Pending Allocation..."))
				: AllocatedMawb.JM_MAWB;
			}
		}

		public ZString AllocatedMAWBNumber
		{
			get
			{
				return AllocatedMawb != null ? AllocatedMawb.JM_Airline3DigitPrefix + AllocatedMawb.JM_MAWB : string.Empty;
			}
		}

		public bool IsAllocatedMAWBDeleted
		{
			get
			{
				return AllocatedMawb != null && AllocatedMawb.IsDeleted;
			}
		}

		public bool IsMAWBAllocatedToParent(ZGuid parentID, ZString parentTablePrefix)
		{
			return AllocatedMawb != null && AllocatedMawb.JM_ParentID == parentID && AllocatedMawb.JM_ParentTableCode == parentTablePrefix;
		}

		bool shouldAllocate;
		bool shouldDeallocate;
		ZGuid oldJobMAWBId;
		ZGuid newJobMAWBId;

		#region For Debug
#if DEBUG

		public void SetShouldAllocate(bool allocate = true)
		{
			shouldAllocate = allocate;
		}

		public void SetNewJobMAWBId(ZGuid value)
		{
			newJobMAWBId = value;
		}

		public bool GetShouldDeallocate()
		{
			return shouldDeallocate;
		}

		public bool GetShouldAllocate()
		{
			return shouldAllocate;
		}

		public void RefreshAllocatedMawbInDatabase()
		{
			allocatedMawbInDatabase = AllocatedMawbInDatabase;
		}
#endif
		#endregion

		#endregion

		public FreightJobMawbLink FreightJobMawbLink
		{
			get { return freightJobMawbLink ?? (freightJobMawbLink = new FreightJobMawbLink(Factory)); }
		}
		FreightJobMawbLink freightJobMawbLink;

		public MAWBStockManagementStrategy MAWBStockManagementStrategy
		{
			get { return mAWBStockManagementStrategy ??= new MAWBStockManagementStrategy(Factory); }
		}
		MAWBStockManagementStrategy mAWBStockManagementStrategy;

		public bool NeutralReadOnly
		{
			get
			{
				return !Parent.IsValidForNeutralMaster
				|| IsMAWBPrinted
				|| (!ImportExportHelper.IsBranchCountry(Parent.MawbPortOfLoading) && !this.IsExport());
			}
		}

		public bool IsMAWBPrinted
		{
			get { return AllocatedMawb != null && !AllocatedMawb.IsDeleted && AllocatedMawb.JM_IsPrinted; }
		}

		#region Allocate

		public bool IsAllocationOfMawbAllowed
		{
			get
			{
				return Parent.IsNeutralMaster
					&& Parent.IsAir
					&& !Parent.MasterBillAirlinePrefix.IsEmpty
					&& (ImportExportHelper.IsBranchCountry(Parent.MawbPortOfLoading) || this.IsExport())
					&& Parent.IsValidForNeutralMaster
					&& !IsMAWBPrinted;
			}
		}

		#region Load Existing MAWB via Factory

		public bool IsLoadMawbFromParentMawbDetailsAllowed
		{
			get { return Parent.IsNeutralMaster && Parent.IsAir; }
		}

		public bool LoadMawbFromParentMawbDetails()
		{
			bool result = false;
			if (!IsLoadMawbFromParentMawbDetailsAllowed)
			{
				MarkForDeallocation();
			}
			else if (AllocatedMawb == null || AllocatedMawb.IsDeleted || (AllocatedMawb.JM_Airline3DigitPrefix != Parent.MasterBillAirlinePrefix || AllocatedMawb.JM_MAWB != Parent.MasterBillMAWB))
			{
				JobMawb mawb = FreightJobMawbLink.LoadExistingByMAWB(Parent.MasterBillAirlinePrefix, Parent.MasterBillMAWB);
				result = AllocateMAWB(mawb);
			}

			return result;
		}

		#endregion

		#region Reallocate printed MAWB via Factory

		public event EventHandler<JobMawbReallocationEventArgs> OnReallocatingPrintedMawb;
		JobMawb unallocatedPrintedMAWB;
		bool shouldUseUnallocatedPrintedMawb;

		void LoadUnallocatedPrintedMawb()
		{
			if (OnReallocatingPrintedMawb != null)
			{
				unallocatedPrintedMAWB = FreightJobMawbLink.LoadUnallocatedPrintedMawb(Parent.NotesParent);
				if (unallocatedPrintedMAWB != null)
				{
					shouldUseUnallocatedPrintedMawb = unallocatedPrintedMAWB.JM_Airline3DigitPrefix == Parent.MasterBillAirlinePrefix
						&& ReallocatePrintedMawb(unallocatedPrintedMAWB);
				}
			}
		}

#if DEBUG
		public
#endif
		bool ReallocatePrintedMawb(JobMawb mawb)
		{
			JobMawbReallocationEventArgs args = new JobMawbReallocationEventArgs(mawb);
			OnReallocatingPrintedMawb(this, args);
			return !args.Cancel;
		}

		public void SetAllocatedMAWBToParent(JobMawb mawb, bool forceAllocateSpecificMAWB = false)
		{
			if (AllocateMAWB(mawb))
			{
				Parent.MasterBillAirlinePrefix = mawb.JM_Airline3DigitPrefix;
				Parent.MasterBillMAWB = mawb.JM_MAWB;
			}
			else
			{
				Parent.MasterBillAirlinePrefix = ZString.Empty;
				Parent.MasterBillMAWB = ZString.Empty;
			}
			shouldAllocate = false;
			if (oldJobMAWBId.IsEmpty)
			{
				shouldDeallocate = false;
			}
			if (forceAllocateSpecificMAWB)
			{
				newJobMAWBId = mawb.PK;
			}
		}

		#endregion

		#region Marking for Allocation/Deallocation/Reallocation

		#region Allocate

		public void MarkForAllocation(bool checkForNull = false)
		{
			if (!checkForNull || AllocatedMawb == null)
			{
				shouldAllocate = true;
				LoadUnallocatedPrintedMawb();
			}
		}

		#endregion

		#region ReAllocate

		public void MarkForReallocation(ZGuid newJobMawbID)
		{
			MarkForReallocation();
			newJobMAWBId = newJobMawbID;
		}

		public void MarkForReallocation(bool checkIsNotPrinted = false, bool checkIsNeutralAndNotPrinted = false, bool checkIsInDatabase = false)
		{
			bool shouldReallocate = (!checkIsNotPrinted || !IsMAWBPrinted) && (!checkIsNeutralAndNotPrinted || IsMAWBNeutralAndNotPrinted) && (!checkIsInDatabase || IsMAWBInDatabase);

			if (shouldReallocate)
			{
				LoadUnallocatedPrintedMawb();
				MarkForDeallocation();
				shouldAllocate = !checkIsInDatabase || IsMAWBInDatabase;
				if (checkIsNeutralAndNotPrinted && checkIsInDatabase)
				{
					Parent.MasterBillMAWB = "";
				}
			}
		}

		public void ReallocateMAWBToNewParent(ZGuid previousParentPK, ZString previousParentTableCode)
		{
			var mawb = FreightJobMawbLink.LoadFromParentPK(previousParentTableCode, previousParentPK);
			if (mawb != null)
			{
				mawb.JM_ParentTableCode = Parent.Prefix;
				mawb.JM_ParentID = Parent.PK;
				shouldAllocate = false;
				shouldDeallocate = false;
			}
		}

		public bool MarkForReallocationIfPrefixChanged(ZString prefix)
		{
			if (allocatedMawbInDatabase != null && allocatedMawbInDatabase.JM_Airline3DigitPrefix == prefix)
			{
				PendingAllocation = false;
				return false;
			}
			else
			{
				MarkForReallocation();
				return true;
			}
		}

		bool IsMAWBInDatabase
		{
			get { return AllocatedMawb != null && AllocatedMawb.IsInDatabase; }
		}

		bool IsMAWBNeutralAndNotPrinted
		{
			get { return Parent.IsNeutralMaster && !IsMAWBPrinted; }
		}

		#endregion

		#region Deallocate

		public void MarkForDeallocation(bool checkMAWBInDatabase = false)
		{
			bool deallocate = !checkMAWBInDatabase || AllocatedMawb != null && AllocatedMawb.IsInDatabase;

			if (deallocate)
			{
				if (allocatedMawb != null && allocatedMawbInDatabase != null)
				{
					shouldDeallocate = true;
					oldJobMAWBId = allocatedMawbInDatabase.PK;
				}
				allocatedMawb = null;
				allocatedMawbInDatabase = null;
				newJobMAWBId = Guid.Empty;
			}
		}

		#endregion

		public bool PendingAllocation
		{
			get
			{
				return shouldAllocate && shouldDeallocate;
			}
			set
			{
				shouldAllocate = value;
				shouldDeallocate = value;
			}
		}

		public bool PendingDeallocation
		{
			get
			{
				return shouldDeallocate && !oldJobMAWBId.IsEmpty;
			}
			set
			{
				shouldDeallocate = value;
			}
		}

		public bool CanGetMAWBAllocator
		{
			get
			{
				return IsAllocationOfMawbAllowed && AllocatedMawb == null || PendingDeallocation;
			}
		}

		#endregion

		#region Allocation / Deallocation via Stored Procedures

		public event EventHandler MawbDeallocated;

		public bool IsPreCheckOfDoingAllocateMAWBNotPass()
		{
			return newJobMAWBId.IsEmpty && (!IsAllocationOfMawbAllowed || !shouldAllocate);
		}

		bool DoAllocateMAWB()
		{
			bool result = false;

			if (IsPreCheckOfDoingAllocateMAWBNotPass())
			{
				return result;
			}

			JobMawb mawb = null;

			if (!newJobMAWBId.IsEmpty)
			{
				mawb = FreightJobMawbLink.TryForceAllocateJobMAWB(newJobMAWBId, Parent.PK, Parent.Prefix);
			}
			else
			{
				if (mawb == null && shouldUseUnallocatedPrintedMawb)
				{
					mawb = unallocatedPrintedMAWB;
				}

				if (mawb == null)
				{
					mawb = FreightJobMawbLink.AllocateUnusedMAWB(Parent.MasterBillAirlinePrefix, Parent.MawbBookingReference,
						Parent.AWBServiceLevel, Parent.PK, Parent.Prefix, exceptMawb: unallocatedPrintedMAWB);
				}
			}

			result = AllocateMAWB(mawb);

			if (result)
			{
				Parent.MasterBillAirlinePrefix = mawb.JM_Airline3DigitPrefix;
				Parent.MasterBillMAWB = mawb.JM_MAWB;
				if (AllocatedMawb != null && AllocatedMawb.JM_ParentID == ZGuid.Empty && AllocatedMawb.JM_ParentTableCode == ZString.Empty)
				{
					AllocatedMawb.JM_ParentID = Parent.PK;
					AllocatedMawb.JM_ParentTableCode = Parent.Prefix;
				}
			}

			if (AllocatedMawb == null)
			{
				Parent.MasterBillMAWB = ZString.Empty;
				if (!newJobMAWBId.IsEmpty)
				{
					Parent.MasterBillAirlinePrefix = ZString.Empty;
				}
			}

			return result;
		}

		void DoDeallocateMAWB()
		{
			if (!oldJobMAWBId.IsEmpty)
			{
				FreightJobMawbLink.DoDeallocateMAWB(oldJobMAWBId, Parent.PK, Parent.NotesParent);
				AllocatedMawb = null;
				MawbDeallocated?.Invoke(this, null);
			}
		}

		public bool PerformMAWBAllocation()
		{
			if (shouldDeallocate)
			{
				DoDeallocateMAWB();
			}
			return DoAllocateMAWB();
		}

		bool AllocateMAWB(JobMawb newMawb)
		{
			bool result = false;
			if (newMawb == null)
			{
				AllocatedMawb = null;
				return result;
			}
			else if (AllocatedMawb != null && newMawb == AllocatedMawb)
			{
				result = true;
			}
			else
			{
				AllocatedMawb = newMawb;
				result = true;
			}

			return result;
		}

		#endregion

		#endregion

		void OnParentFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				PendingAllocation = false;
				newJobMAWBId = ZGuid.Empty;
				oldJobMAWBId = ZGuid.Empty;
				allocatedMawbInDatabase = FreightJobMawbLink.LoadBOInTheDBFromParentPK(Parent.Prefix, Parent.PK);

				if (Parent.MasterBillNeutralMAWBInfo != null)
				{
					Parent.MasterBillNeutralMAWBInfo.RefreshBinding();
				}
			}
			else
			{
				if (oldJobMAWBId.IsValid)
				{
					allocatedMawbInDatabase = FreightJobMawbLink.LoadOrRefreshMAWB(oldJobMAWBId);
				}
			}
		}

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(Parent.MawbPortOfLoading, Parent.MawbPortOfDischarge); }
		}
	}
}
