using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IBillCollection<out TBill, out TDeclaration> : IDependentBusinessObjectCollection
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		new TBill this[int index] { get; }
		new TBill AddNew();

		event CollectionCountChangedEventHandler CountChanged;

		ZString HouseBillsCommaSeparated { get; }
		ZString MasterBillsCommaSeparated { get; }
		TBill PrimaryHouseBill { get; }
		TBill PrimaryMasterBill { get; }
		int NumberOfMasterBill { get; }
		int NumberOfHouseBill { get; }

		void Load();
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
		TBill CreatePrimaryBill(ZString billType);
		TBill FindByBillUniqueCode(ZString houseBillNo);
		TBill FindByBillNumberAndType(ZString billNum, ZString billType, Func<TBill, bool> additionalMatch = null);
		TBill FindAnyBillWithHouseBillMasterBillCombination(ZString houseBillNum, ZString masterBill);
		TBill FindHouseBillsByHouseBillNumAndParentMasterBill(ZString houseBillNum, ZString parentMasterBillNum);
		TBill[] FindByBillType(ZString billType);
		TBill GetElementWithoutPackingGroups();
		TBill GetPrimaryBill(ZString billType);
		void MarkAsNeedingValidation();
		void RemoveAndDelete(BusinessObject bill);
		void RemoveAndDeleteAll();
		void SetMergingInProgress(bool started);
		void ValidateCU_BillType();
		void Sort(string propertyName);
	}

	/// <summary>
	/// Contains all types of bills hanging to CU_JE
	/// </summary>
	public class BillCollection<TBill, TDeclaration> : DependentBusinessObjectCollection<TBill, TDeclaration>, IBillCollection<TBill, TDeclaration>
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		public BillCollection(TDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public void ValidateCU_BillType()
		{
			foreach (TBill bill in this)
			{
				bill.Validation.ValidateCU_BillType();
			}
		}

		public BillCollection(TDeclaration declaration, ZQuery query)
			: base(declaration, query)
		{
		}

		public class BillNumComparer : IComparer<TBill>
		{
			public int Compare(TBill x, TBill y)
			{
				return string.Compare(x.CU_BillNum, y.CU_BillNum, true);
			}
		}

		public void SetMergingInProgress(bool started)
		{
			foreach (TBill bill in this)
			{
				bill.SetMergingInProgress(started);
			}
		}

		public TBill GetElementWithoutPackingGroups()
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.IsLowestBill && bill.PackingGroups.Count == 0);
		}

		#region Find

		public TBill FindHouseBillsByHouseBillNumAndParentMasterBill(ZString houseBillNum, ZString parentMasterBillNum)
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.IsHouseBill && bill.CU_BillNum.EqualsIgnoringCase(houseBillNum) && bill.CU_MasterBill.EqualsIgnoringCase(parentMasterBillNum));
		}

		/// <summary>
		/// If passed house bill num is empty, then this might return master bill object with matching master bill number
		/// </summary>
		public TBill FindAnyBillWithHouseBillMasterBillCombination(ZString houseBillNum, ZString masterBill)
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.CU_HouseBill.EqualsIgnoringCase(houseBillNum) && bill.CU_MasterBill.EqualsIgnoringCase(masterBill));
		}

		public TBill FindByBillNumberAndType(ZString billNum, ZString billType, Func<TBill, bool> additionalMatch = null)
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.CU_BillType.EqualsIgnoringCase(billType) && bill.CU_BillNum.EqualsIgnoringCase(billNum) && (additionalMatch == null || additionalMatch(bill)));
		}

		public TBill[] FindByBillType(ZString billType)
		{
			return (TBill[])Find(new ZQuery(CusDecHouseBillSchema.CU_BillType, billType));
		}

		public TBill FindByBillUniqueCode(ZString uniqueCode)
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.CU_BillUniqueCode.EqualsIgnoringCase(uniqueCode));
		}

		public TBill PrimaryHouseBill
		{
			get { return GetPrimaryBill(BillTypeList.Codes.HouseBill); }
		}

		public TBill PrimaryMasterBill
		{
			get { return GetPrimaryBill(BillTypeList.Codes.MasterBill); }
		}

		public TBill GetPrimaryBill(ZString billType)
		{
			return this.Cast<TBill>().FirstOrDefault(bill => bill.CU_GUIPresentationRecord && billType == bill.CU_BillType);
		}

		public TBill CreatePrimaryBill(ZString billType)
		{
			TBill result = null;

			//When CU_BillNum is entered, it can be synched with JE_Master/JE_HouseBill
			using (Declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				result = AddNew();
				result.CU_BillType = billType;
				result.CU_GUIPresentationRecord = true;
			}
			return result;
		}

		public TBill CreatePrimaryBillIfNull(ZString billType)
		{
			return GetPrimaryBill(billType) ?? CreatePrimaryBill(billType);
		}

		public ZString HouseBillsCommaSeparated
		{
			get { return GetBillNumbersCommaSeperatedFor(BillTypeList.Codes.HouseBill); }
		}

		public ZString MasterBillsCommaSeparated
		{
			get { return GetBillNumbersCommaSeperatedFor(BillTypeList.Codes.MasterBill); }
		}

		protected ZString GetBillNumbersCommaSeperatedFor(ZString billType)
		{
			List<TBill> ordered = new List<TBill>(new TypedEnumerable<TBill>(this));
			ordered.Sort(new BillNumComparer());

			ZStringBuilder result = new ZStringBuilder();

			foreach (TBill bill in ordered.Where(bill => bill.CU_BillType == billType))
			{
				result.Append(bill.CU_BillNum + ",");
			}

			return result.ToString().Trim(',');
		}

		#endregion

		#region Declaration
		protected TDeclaration Declaration => Master;

		#endregion

		#region Overriden

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			using (Declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				base.Load(alternativeAdditionalFilter);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			TBill bill = (TBill)child;

			if (!((IBusinessObjectInternals)child).IsCopying && !Declaration.IsImportingData)
			{
				bill.CU_BillType = Count == 0 ? BillTypeList.Codes.MasterBill : BillTypeList.Codes.HouseBill;

				if (bill.IsHouseBill && NumberOfMasterBill == 1 && Declaration.PrimaryMasterBill != null)
				{
					bill.CU_CU_ParentBill = Declaration.PrimaryMasterBill.PK;
				}
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			TBill bill = (TBill)bizOAdded;
			TBill parentBill = (TBill)bill.ParentBill;
			if (!bill.IsInDatabase && parentBill != null) // If SetDefaultsForNewChild() has set the Parent Bill...
			{
				parentBill.ChildBills.Rebuild(); // ChildBills should rebuild before LowestBills as lowest bills depend on the count of child bills.
				Declaration.LowestBills.Rebuild();
				if (!parentBill.IsValidationSuspended)
				{
					parentBill.Validation.ValidateCU_BillNum(); // Revalidate Master Bill when it's no longer the lowest level bill.
				}
				if (!Declaration.HasChanges)
				{
					Declaration.HasChanges = true;
				}
			}
		}

		public int NumberOfMasterBill => Find(new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill)).Length;

		public int NumberOfHouseBill => Find(new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill)).Length;

		protected override bool AllowNewCore => Declaration != null && !Declaration.IsDataSyncFromShipment && !Declaration.IsGlobalManifestIntegrationEnabled;

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			TBill bill = (TBill)bizO;
			if (bill.CU_GUIPresentationRecord)
			{
				Declaration.BillGUIPresentationFlagUpdater.UpdateWhenABillIsRemoved(bill);
				Declaration.JE_HouseBillInfo.RefreshBinding();
				Declaration.JE_MasterBillInfo.RefreshBinding();
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject dependent, bool forDelete)
		{
			//CU_JE is removed in the base, but the FK is the link for PackingGroups or Packages (view collections of Declaration.PackingGroups or Declaration.Packages)
			//When bill is removed, PackingGroups need to be removed as well.
		}

		#endregion
	}
}
