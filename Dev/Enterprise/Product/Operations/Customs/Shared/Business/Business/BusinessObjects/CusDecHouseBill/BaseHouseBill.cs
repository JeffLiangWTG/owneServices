using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(Bill.Schema.CU_BillUniqueCode), DescriptionProperty(Bill.Schema.CU_BillUniqueCode)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class Bill : AutoCusDecHouseBill,
		Integration.Customs.IBill,
		IDeclarationProvider,
		ISupportDataImporting,
		ISynchroniserReadOnlyMembersProvider,
		ITopLevelBizOProviderForJobDocAddress,
		IClusterKeyWorker,
		IAddInfoChildSupporter,
		ITypeDeciderContext,
		IAdditionalDebuggingDetails
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoCusDecHouseBill.Schema
		{
			public const string CU_BillUniqueCode = "CU_BillUniqueCode";
			public const string CU_ParentBillUniqueCode = "CU_ParentBillUniqueCode";
			public const string CU_HouseBill = "CU_HouseBill";
			public const string CU_MasterBill = "CU_MasterBill";
			public const string CU_BillTypeDescription = "CU_BillTypeDescription";
			public const string CU_MessageStatusDescription = "CU_MessageStatusDescription";
		}

		#endregion Schema

		public static readonly BaseCusDecHouseBillTypeDecider TypeDecider = new BaseCusDecHouseBillTypeDecider();

		#region Calculated Properties

		public static class BillAndParentCombination
		{
			public static ZString BillAndParentBill(ZString bill, ZString parentBill)
			{
				return bill + (parentBill.IsEmpty ? "" : " (" + parentBill + ")");
			}

			public static ZString BillAndParentBill(ZString bill, ZString billType, ZString parentBill, ZString parentBillType)
			{
				return BillAndParentBill(BillTypeAndNumber(bill, billType), BillTypeAndNumber(parentBill, parentBillType));
			}

			public static ZString BillTypeAndNumber(ZString bill, ZString billType)
			{
				return bill.IsEmpty ? "" : billType + ":" + bill;
			}
		}

		public
#if DEBUG
			virtual
#endif
			ZString CU_BillUniqueCode
		{
			get
			{
				if (cU_BillUniqueCodeCached == null)
				{
					cU_BillUniqueCodeCached = new CachedProperty<ZString>(Factory, delegate
					{
						Bill parentBill = this.ParentBill;
						ZString parentBillCode = parentBill == null ? ZString.Empty : parentBill.CU_BillTypeAndNum;
						return BillAndParentCombination.BillAndParentBill(CU_BillTypeAndNum, parentBillCode);
					}
					);
				}
				return cU_BillUniqueCodeCached.Value;
			}
		}

		CachedProperty<ZString> cU_BillUniqueCodeCached;

		public ZPropertyInfo CU_BillUniqueCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CU_BillUniqueCode); }
		}

		public ZString CU_BillTypeAndNum
		{
			get { return BillAndParentCombination.BillTypeAndNumber(CU_BillNum, CU_BillType); }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(Schema.CU_BillNumMaxLength + 10)]
		public ZString CU_ParentBillUniqueCode
		{
			get
			{
				Bill parentBill = this.ParentBill;
				return parentBill == null ? ZString.Empty : parentBill.CU_BillUniqueCode;
			}
			set
			{
				if (CU_ParentBillUniqueCode != value && Declaration != null)
				{
					Bill newParentBill = Declaration.Bills.FindByBillUniqueCode(value);
					if (newParentBill != null && newParentBill != this)
					{
						CU_CU_ParentBill = newParentBill.PK;
					}
					else
					{
						CU_CU_ParentBill = ZGuid.Empty;
					}
					Declaration.JE_HouseBillInfo.RefreshBinding();
					Declaration.JE_MasterBillInfo.RefreshBinding();
				}
				CU_ParentBillUniqueCodeInfo.RefreshBinding();
				Validation.ValidateCU_ParentBillUniqueCode();
			}
		}

		public ZPropertyInfo CU_ParentBillUniqueCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CU_ParentBillUniqueCode)); }
		}

		protected virtual bool CU_ParentBillUniqueCode_ReadOnly
		{
			get { return IsMasterBill || (Declaration != null && Declaration.IsDataSyncFromShipment); }
		}

		protected virtual bool CU_IssueDate_ReadOnly
		{
			get { return Declaration != null && Declaration.IsDataSyncFromShipment; }
		}

		void DefaultPackingInfoFromDecToBillsIfNeeded()
		{
			if (Declaration.ShouldDefaultPackingInfoFromDeclarationToBills)
			{
				BasePackingGroup packGroup = Declaration.PackingGroups.GetElementWithNoHouseBill();
				if (packGroup != null)
				{
					packGroup.CR_CU_HouseBill = this.PK;
				}
				else if (Declaration.PackingInformationCollection != null)
				{
					IPackingInformation packingInformation = Declaration.PackingInformationCollection.GetElementWithNoHouseBill()
						?? Declaration.PackingInformationCollection.AddNew();

					HouseBillContainer houseContainer = packingInformation.HouseBillContainer;

					BaseCusContainer container = houseContainer.Container ?? Declaration.CusContainers.GetElementWithoutPackingGroups();

					if (container == null && Declaration.CusContainers.Count == 1)
					{
						container = Declaration.CusContainers[0];
					}

					packingInformation.HouseBillContainer = new HouseBillContainer(this, container);

					if (Declaration.PackingInformationCollection.Count == 1 && Declaration.LowestBills.Count == 1)
					{
						var primaryHouseBill = Declaration.PrimaryHouseBill;
						if (this == primaryHouseBill || (!Declaration.PackingLinesShouldOnlyBeCreatedAtTheHouseBillLevel && primaryHouseBill == null && this == Declaration.PrimaryMasterBill))
						{
							DefaultPackingInfoFromDecToBillsCore(packingInformation);
						}
					}
				}
			}
		}

		protected virtual void DefaultPackingInfoFromDecToBillsCore(IPackingInformation packingInformation)
		{
			packingInformation.PackQty = Declaration.JE_TotalNoOfPacks;
			packingInformation.PackType = Declaration.JE_TotalNoOfPacksPackType;
		}

		public virtual bool IsLowestBill
		{
			get { return ChildBills.Count == 0; }
		}

		public bool IsMasterBill
		{
			get { return CU_BillType == BillTypeList.Codes.MasterBill; }
		}

		public bool IsHouseBill
		{
			get { return CU_BillType == BillTypeList.Codes.HouseBill; }
		}

		public bool IsSubHouseBill
		{
			get { return CU_BillType == BillTypeList.Codes.SubHouseBill; }
		}

		#endregion Calculated Properties

		#region Related Business Objects

		[RelatedBusinessObject("Declaration")]
		public override ZGuid CU_JE
		{
			get { return base.CU_JE; }
			set
			{
				bool hasChanged = CU_JE != value;
				base.CU_JE = value;
				if (!value.IsEmpty)
				{
					fCU_JECached = value;
				}
				if (hasChanged && Declaration != null)
				{
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public BaseJobDeclaration Declaration
		{
			get
			{
				var result = Factory.Load<BaseJobDeclaration>(CU_JE) ?? Factory.Load<BaseJobDeclaration>(CU_JECached);
				return result;
			}
		}

		public Bill ParentBill
		{
			get { return Factory.Load<Bill>(CU_CU_ParentBill); }
		}

		public Bill GetBillOfType(ZString billType)
		{
			Bill bill = this;
			while (bill != null)
			{
				if (bill.CU_BillType == billType)
				{
					return bill;
				}
				bill = bill.ParentBill;
			}
			return null;
		}

		#endregion Related Business Objects

		#region Overriden Properties

		[List(nameof(Lookups) + "." + nameof(CusDecHouseBillLookups.MessageStatusList))]
		public override ZString CU_Status
		{
			get { return base.CU_Status; }
			set { base.CU_Status = value; }
		}

		public ZString CU_MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(CU_Status); }
		}

		ZString Integration.Customs.IBill.CU_MessageStatus
		{
			get { return CU_Status; }
			set { CU_Status = value; }
		}

		public ZPropertyInfo CU_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CU_MessageStatusDescription); }
		}

		[ReadOnlyMember(nameof(IsMasterBill))]
		public override ZGuid CU_CU_ParentBill
		{
			get { return base.CU_CU_ParentBill; }
			set
			{
				if (value == PK)
				{
					throw new InvalidOperationException("You are trying to link this record to itself as a parent.");
				}
				else
				{
					bool hasChanged = CU_CU_ParentBill != value;
					base.CU_CU_ParentBill = value;

					if (hasChanged && !IsCopying)
					{
						if (Declaration != null)
						{
							Declaration.MarkAsNeedingValidation();
							Declaration.LowestBills.Rebuild();

							if (ParentBill != null)
							{
								ParentBill.MarkAsNeedingValidation();
								Declaration.BillGUIPresentationFlagUpdater.UpdateWhenCU_CU_ParentBillChanged(ParentBill, this);
							}
						}
					}
				}
			}
		}

		public ZString CU_BillTypeDescription
		{
			get { return CU_BillType.IsEmpty ? "" : Lookups.CU_BillTypeList.GetDescriptionFromCode(CU_BillType); }
		}

		public ZPropertyInfo CU_BillTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CU_BillTypeDescription); }
		}

		public override ZString CU_BillType
		{
			get { return base.CU_BillType; }
			set
			{
				bool hasChanged = CU_BillType != value;

				base.CU_BillType = value;
				if (hasChanged && !IsCopying)
				{
					if (!value.IsEmpty && Declaration != null)
					{
						Declaration.BillGUIPresentationFlagUpdater.UpdateWhenCU_BillTypeChanged(this);
					}
					if (IsMasterBill)
					{
						CU_CU_ParentBill = ZGuid.Empty;
						LinkMasterBillToExistingHouseBills();
					}
					if (Declaration != null)
					{
						Declaration.MarkAsNeedingValidation();
					}
				}
			}
		}

		void LinkMasterBillToExistingHouseBills()
		{
			if (Declaration != null)
			{
				var masterBills = Declaration.Bills.FindByBillType(BillTypeList.Codes.MasterBill);
				if (masterBills.Length == 1)
				{
					var houseBills = Declaration.Bills.FindByBillType(BillTypeList.Codes.HouseBill);
					foreach (Bill bill in houseBills)
					{
						if (bill.CU_CU_ParentBill.IsEmpty)
						{
							bill.CU_CU_ParentBill = this.PK;
						}
					}
				}
			}
		}

		internal bool IsThisTheOnlyMasterBill
		{
			get { return IsMasterBill && IsThisTheOnlyBillForThisType; }
		}

		internal bool IsThisTheOnlyHouseBillLinkedToPrimaryMasterBillOfDeclaration
		{
			get
			{
				if (IsThisTheOnlyBillForThisType && CU_BillType == BillTypeList.Codes.HouseBill)
				{
					ZGuid primaryMasterBillPK = Declaration.PrimaryMasterBill == null ? ZGuid.Empty : Declaration.PrimaryMasterBill.PK;
					return primaryMasterBillPK == CU_CU_ParentBill;
				}
				return false;
			}
		}

		bool IsThisTheOnlyBillForThisType
		{
			get
			{
				int countForThisToBePrimary = Declaration.Bills.Contains(this) ? 1 : 0;
				return Declaration.Bills.Find(new ZQuery(CusDecHouseBillSchema.CU_BillType, CU_BillType)).Length == countForThisToBePrimary;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used via reflection")]
		bool CU_BillType_ReadOnly
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.IsDataSyncFromShipment || declaration.IsGlobalManifestIntegrationEnabled);
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool CU_GUIPresentationRecord
		{
			get { return base.CU_GUIPresentationRecord; }
			set
			{
				bool hasChanged = CU_GUIPresentationRecord != value;
				base.CU_GUIPresentationRecord = value;
				if (hasChanged && !IsCopying && Declaration != null)
				{
					if (Declaration.PrimaryHouseBill != null)
					{
						Declaration.PrimaryHouseBill.MarkAsNeedingValidation();
					}

					if (Declaration.PrimaryMasterBill != null)
					{
						Declaration.PrimaryMasterBill.MarkAsNeedingValidation();
					}

					Declaration.JE_MasterBillInfo.RefreshBinding();
					Declaration.JE_HouseBillInfo.RefreshBinding();
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CU_BillNum
		{
			get { return base.CU_BillNum; }
			set
			{
				bool hasChanges = !CU_BillNum.EqualsIgnoringCase(value);
				base.CU_BillNum = value;

				if (hasChanges && !IsCopying)
				{
					if (Declaration != null)
					{
						//When JE_MasterBill is set, we dont know whether it is a direct master or master bill
						if (!IsMasterBill && !value.IsEmpty && PackingGroups.Count == 0)
						{
							DefaultPackingInfoFromDecToBillsIfNeeded();
						}

						//this should happen after value is assigned
						Declaration.MarkAsNeedingValidation();
						Declaration.BillGUIPresentationFlagUpdater.SynchroniseFromBillNum(this);
						Declaration.JE_MasterBillInfo.RefreshBinding();
					}

					if (ShouldDeleteBillIfNumberIsEmpty && value.IsEmpty)
					{
						Delete();
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used via reflection")]
		bool CU_BillNum_ReadOnly
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && (declaration.IsDataSyncFromShipment || declaration.IsGlobalManifestIntegrationEnabled);
			}
		}

		protected virtual ZBool ShouldDeleteBillIfNumberIsEmpty
		{
			get { return false; }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(Schema.CU_BillNumMaxLength)]
		public ZString CU_HouseBill
		{
			get
			{
				Bill bill = GetBillOfType(BillTypeList.Codes.HouseBill);
				return bill != null ? bill.CU_BillNum : ZString.Empty;
			}
			set
			{
				if (IsMasterBill)
				{
					throw new NotSupportedException("HouseBill setter() is triggered for a master bill");
				}
				CU_BillNum = value;
				CU_HouseBillInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CU_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.CU_HouseBill); }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(Schema.CU_BillNumMaxLength)]
		public ZString CU_MasterBill
		{
			get
			{
				Bill parentBill = GetBillOfType(BillTypeList.Codes.MasterBill);
				return (parentBill != null) ? parentBill.CU_BillNum : ZString.Empty;
			}
			set
			{
				bool hasChanged = !CU_MasterBill.EqualsIgnoringCase(value);
				if (hasChanged)
				{
					if (IsMasterBill)
					{
						CU_BillNum = value;
					}
					else
					{
						if (CU_BillType != BillTypeList.Codes.HouseBill)
						{
							throw new NotSupportedException("Masterbill may not be set on Sub House Bills");
						}
						Bill billWithThisNum = Declaration.Bills.FindByBillNumberAndType(value, BillTypeList.Codes.MasterBill);
						if (billWithThisNum != null)
						{
							CU_CU_ParentBill = billWithThisNum.PK;
						}
						else
						{
							throw new NotSupportedException("MasterBill was attempt to be set for a non-master bill object which does not have a parent master bill");
						}
					}
				}
				CU_MasterBillInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CU_MasterBillInfo
		{
			get { return GetZPropertyInfo(Schema.CU_MasterBill); }
		}

		[List(nameof(Lookups) + "." + nameof(CusDecHouseBillLookups.NoOfPacksPackType_List))]
		public override ZString CU_PackType
		{
			get { return base.CU_PackType; }
			set { base.CU_PackType = value; }
		}

		public virtual bool IsBillNumberAWB
		{
			get { return Declaration != null && Declaration.IsAir; }
		}

		#endregion Overriden Properties

		#region Business Objects Override

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.BaseBillFetchStrategy(this);
		}

		#endregion Business Objects Override

		#region Collections

		BasePackingGroupCollection fPackingGroups;

		public BasePackingGroupCollection PackingGroups
		{
			get
			{
				if (fPackingGroups == null && Declaration != null)
				{
					fPackingGroups = CreateNewPackingGroup();
					fPackingGroups.Rebuild();
				}

				return fPackingGroups;
			}
		}

		protected virtual BasePackingGroupCollection CreateNewPackingGroup()
		{
			return new BasePackingGroupCollection(this);
		}

		public void ReloadPackingGroup(bool reLoadExistingRows)
		{
			if (fPackingGroups != null)
			{
				fPackingGroups.Reload(reLoadExistingRows);
			}
		}

		[ChildEditable(true)]
		public BaseBillContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = GetContainerCollection();
					fContainers.Load();
					RegisterEditableChildObject(fContainers);
				}

				if (RefreshContainers)
				{
					RefreshContainers = false;
					fContainers.Load();
				}

				return fContainers;
			}
		}

		BaseBillContainerCollection fContainers;

		internal bool RefreshContainers
		{
			get;
			set;
		}

		protected virtual BaseBillContainerCollection GetContainerCollection()
		{
			return new BaseBillContainerCollection(this);
		}

		public InvoiceHeaderActiveCollection Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = CreateNewHouseBillLevelInvoiceCollection();
				}
				return fInvoices;
			}
		}

		InvoiceHeaderActiveCollection fInvoices;

		protected virtual InvoiceHeaderActiveCollection CreateNewHouseBillLevelInvoiceCollection()
		{
			return new InvoiceHeaderActiveCollection(this);
		}

		public IChildBillCollection<Bill, BaseJobDeclaration> ChildBills
		{
			get
			{
				if (fChildBills == null && Declaration != null)
				{
					fChildBills = CreateNewChildBills();
				}
				return fChildBills;
			}
		}

		IChildBillCollection<Bill, BaseJobDeclaration> fChildBills;

		internal void RefreshChildBills()
		{
			fChildBills = null;
		}

		protected virtual IChildBillCollection<Bill, BaseJobDeclaration> CreateNewChildBills()
		{
			return new ChildBillCollection<Bill, BaseJobDeclaration>(this, Declaration);
		}

		public CusEntryHeader[] Entries
		{
			get
			{
				if (fEntries == null)
				{
					if (isMergingInProgress)
					{
						string methodName = GetType().FullName + ".Entries";
						ErrorReporter.ReportOnce(methodName + " was accessed during merge", methodName + " should not be accessed during merge");
					}

					var result = new HashSet<CusEntryHeader>();
					bool containsAllEntries = false;
					var entriesCount = Declaration.CustomsEntryHeaders.Count;
					if (entriesCount > 0)
					{
						foreach (BaseJobComInvoiceHeader invoice in Invoices)
						{
							foreach (CusEntryHeader entry in invoice.Entries)
							{
								if (result.Add(entry))
								{
									containsAllEntries = result.Count == entriesCount;
									if (containsAllEntries)
									{
										break;
									}
								}
							}
							if (containsAllEntries)
							{
								break;
							}
						}
					}
					fEntries = result.ToArray();
				}
				return fEntries;
			}
		}

		CusEntryHeader[] fEntries;

		bool isMergingInProgress;

		internal void SetMergingInProgress(bool started)
		{
			isMergingInProgress = started;
			if (started)
			{
				RefreshEntries();
			}
		}

		internal void RefreshEntries()
		{
			fEntries = null;
		}

		#endregion Collections

		#region Overriden Methods

		public override void OnLoaded()
		{
			base.OnLoaded();
			fCU_JECached = CU_JE;
		}

		ZGuid fCU_JECached;

		protected ZGuid CU_JECached
		{
			get { return fCU_JECached; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			var declaration = Declaration;
			if (declaration != null)
			{
				foreach (BaseJobComInvoiceHeader invoice in Invoices.ToArray())
				{
					invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
				}

				declaration.Bills.Reload(false);
				fChildBills?.Rebuild();
				ChildBills.RemoveReferenceFromChildren();
				PackingGroups.RemoveAndDeleteAll();
			}

			var parentBill = this.ParentBill;
			base.Delete();

			//ChildBills should rebuild before LowestBills as lowest bills depend on the count of child bills
			if (parentBill != null)
			{
				parentBill.ChildBills.Rebuild();
			}

			if (declaration != null)
			{
				declaration.LowestBills.Rebuild();
			}
		}

		public override void OnSaving()
		{
			if (WillBeDeletedDuringSave)
			{
				Delete();
			}
			base.OnSaving();
		}

		protected internal virtual bool WillBeDeletedDuringSave
		{
			get { return !CU_GUIPresentationRecord && CU_BillNum.IsEmpty && CU_IssueDate.IsEmpty && (Declaration == null || PackingGroups.Count == 0) && !IsImportingData; }
		}

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || (Declaration?.IsPersistent ?? false));

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion Overriden Methods

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && !declaration.IsDataSyncFromShipment && !declaration.IsGlobalManifestIntegrationEnabled;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsGlobalManifestIntegrationEnabled ? ReasonForCannotDeleteWhenIntegratedWithManifest : ReasonForCannotDeleteWhenSynchronised;
			}
		}

		internal static MultilingualString ReasonForCannotDeleteWhenIntegratedWithManifest => ResString.GetMultilingualString("{305CB624-39CA-4D5E-940D-9B6D70EA2038}", "This bill was copied from Global Manifest and cannot be deleted.");
		internal static MultilingualString ReasonForCannotDeleteWhenSynchronised => ResString.GetMultilingualString("bfbde3ad-e3b1-4d0d-846f-bc4084d2824e", "Declaration values are copied from shipment. If you want to delete this record, please do it in shipment. Or you should tick 'Override default values from shipment'.");

		#endregion ICanDelete Members

		#region ISupportDataImporting Members

		public bool IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		bool fIsImportingData;

		#endregion ISupportDataImporting Members

		#region Test

#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new MyBusinessObjectTestDataHelper();
		}

		class MyBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (!property.Name.Equals(Schema.CU_MasterBill) && !property.Name.Equals(Schema.CU_HouseBill) && !property.Name.Equals(Schema.CU_BillType))
				{
					base.PopulateString(property);
				}
			}
		}

#endif

		#endregion Test

		#region ReadOnly

		public List<string> SynchroniserReadOnlyMembers
		{ get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		public BusinessObject GetTopBusinessObject()
		{
			return Factory.Load(JobDeclarationSchema.Constants.Prefix, CU_JE);
		}

		#endregion ReadOnly

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CU_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BaseJobDeclaration);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CU_JEInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(BasePackingGroup), CusDecHouseContainerPivotSchema.CR_CU_HouseBill);
				yield return new ClusterKeyChildInfo(ObjectFactory.GetType<Integration.Customs.US.ICusUSDecHouseBill>(), CusUSDecHouseBillSchema.USB_CU);
			}
		}

		#endregion IClusterKeyWorker

		#region IAddInfoChildSupporter Members

		BusinessObject IAddInfoChildSupporter.AddInfoChild => GetAddInfoChild();

		protected virtual BusinessObject GetAddInfoChild() => null;

		SchemaGuidColumn IAddInfoChildSupporter.ChildForeignKeyColumn => GetChildForeignKeyColumn();

		protected virtual SchemaGuidColumn GetChildForeignKeyColumn() => null;

		void IAddInfoChildSupporter.RegisterListChangedCalledRefreshBinding(IBindingList element) => RegisterListChangedCalledRefreshBinding(element);

		void IAddInfoChildSupporter.UnRegisterListChangedCalledRefreshBinding(IBindingList element) => UnRegisterListChangedCalledRefreshBinding(element);

		#endregion IAddInfoChildSupporter Members

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion ITypeDeciderContext Members

		#region IAdditionalDebuggingDetails Members

		IEnumerable<string> IAdditionalDebuggingDetails.AdditionalDetails
		{
			get
			{
				yield return $"{GetType().FullName} - In DB:{IsInDatabase} - {CU_BillNum} - {PK}";
				if (Declaration is IAdditionalDebuggingDetails additionalDebuggingDetails)
				{
					foreach (var detail in additionalDebuggingDetails.AdditionalDetails)
					{
						yield return detail;
					}
				}
			}
		}

		#endregion IAdditionalDebuggingDetails Members
	}
}
