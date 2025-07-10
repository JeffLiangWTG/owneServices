using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow()]
	[CodeProperty(Schema.CW_UniqueCode), DescriptionProperty(Schema.CW_UniqueCode)]
	public class BasePackage : AutoCusDecHouseContainerPack,
		IPackingSearch,
		ICanDelete,
		IPackingInformation,
		IOneToOnePackingInformation,
		IPackLineInfo,
		Integration.Customs.IBasePackage,
		IUNDGDataItemProvider,
		ITypeDeciderContext,
		IClusterKeyWorker
	{
		#region Schema
		public new class Schema : AutoCusDecHouseContainerPack.Schema
		{
			public const string CW_HouseBill = "CW_HouseBill";
			public const string CW_ContainerNoOrEquipmentNo = "CW_ContainerNoOrEquipmentNo";
			public const string CW_UniqueCode = "CW_UniqueCode";
		}
		#endregion

		public BasePackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly BasePackageTypeDecider TypeDecider = new BasePackageTypeDecider();
		public bool IsSynchronising;

		#region IPackingInformation Members

		HouseBillContainer IPackingInformation.HouseBillContainer
		{
			get { return new HouseBillContainer(PackingGroup == null ? null : PackingGroup.Bill, PackingGroup == null ? null : PackingGroup.Container); }
			set
			{
				LinkToPackingGroup(value.HouseBill, value.Container);
			}
		}

		bool IPackingInformation.SupportMarksAndNumbers
		{
			get { return true; }
		}

		ZString IPackingInformation.MarksAndNumbers
		{
			get { return CW_MarksAndNos; }
			set { CW_MarksAndNos = value; }
		}

		ZString IPackingInformation.PackType
		{
			get { return CW_PackType; }
			set { CW_PackType = value; }
		}

		ZInt IPackingInformation.PackQty
		{
			get { return CW_PackQty; }
			set { CW_PackQty = value; }
		}

		internal void MarkAsDeleteForShipmentSynch()
		{
			IsGoingToBeDeletedAfterShipmentSynch = true;
		}

		internal void UnmarkAsDeleteForShipmentSynch()
		{
			IsGoingToBeDeletedAfterShipmentSynch = false;
		}

		internal bool IsGoingToBeDeletedAfterShipmentSynch
		{
			get;
			private set;
		}

		#endregion

		[RelatedBusinessObject("PackingGroup")]
		public override ZGuid CW_CR_HouseContainer
		{
			get { return base.CW_CR_HouseContainer; }
			set
			{
				var oldPackingGroup = PackingGroup;
				base.CW_CR_HouseContainer = value;

				var newPackingGroup = PackingGroup;
				if (oldPackingGroup != newPackingGroup)
				{
					oldPackingGroup?.Packages?.Rebuild();
					newPackingGroup?.Packages?.Rebuild();

					CW_ClusterKey = newPackingGroup?.CR_ClusterKey ?? ZInt.Zero;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Business.BasePackage|CW_Seal", Caption = "Seal Number")]
		public override ZString CW_Seal
		{
			get => base.CW_Seal;
			set => base.CW_Seal = value;
		}

		#region Override of NewBusinessObjectTestDataHelper() to stop setting CW_ContainerNoOrEquipmentNo and CW_HouseBill in FillWithValidTestData()
#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new PackageBusinessObjectTestDataHelper();
		}

		class PackageBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateString(ZPropertyInfo property)
			{
				if (property.Name != Schema.CW_ContainerNoOrEquipmentNo && property.Name != Schema.CW_HouseBill)
				{
					base.PopulateString(property);
				}
			}
		}
#endif
		#endregion

		#region CW_PackQty

		public override ZInt CW_PackQty
		{
			get { return base.CW_PackQty; }
			set
			{
				bool hasChanged = base.CW_PackQty != value;
				base.CW_PackQty = value;
				if (hasChanged && Declaration is BaseJobDeclaration declaration && declaration.IsPackingInformationRelevant)
				{
					(declaration.Validation as BaseJobDeclarationValidation)?.ValidatePackagesActualPackageCount();
				}
			}
		}

		#endregion

		#region PackType

		public override ZString CW_PackType
		{
			get { return base.CW_PackType; }
			set { base.CW_PackType = value.Trim(); }
		}

		public ZString FreightPackageType
		{
			get { return FreightPackageTypeCore; }
		}

		protected virtual ZString FreightPackageTypeCore
		{
			get { return CW_PackType; }
		}

		#endregion // PackType

		#region Lookup Lists
		public virtual CodeDescriptionPairList PackTypeList => Declaration?.Lookups?.PackingUnitTypesList ?? new CodeDescriptionPairList();

		public CodeDescriptionPairList ContainersAndEquipmentsOnDeclaration_List => Declaration?.ContainersAndEquipmentsOnDeclaration_List ?? new CodeDescriptionPairList();

		public IBillCollection<Bill, BaseJobDeclaration> BillsOnDeclaration_List => Declaration is BaseJobDeclaration declaration ? declaration.Bills : null;

		#endregion

		#region CW_HouseBill

		[BusinessObjectTestExclude()]
		public ZString CW_HouseBill
		{
			get
			{
				var bill = PackingGroup?.Bill;
				return (bill != null ? bill.CU_BillUniqueCode : fCW_HouseBill);
			}
			set
			{
				CheckMaximumLength(CW_HouseBillInfo, value);
				LinkToRightHouseBillAndContainerNoOrEquipmentNo(value, CW_ContainerNoOrEquipmentNo);
				fCW_HouseBill = value;
				UpdateChildrenHouseBill(value);
				CW_HouseBillInfo.RefreshBinding();
				Validation.ValidateCW_HouseBill();
				if (Declaration is BaseJobDeclaration declaration)
				{
					(declaration.Validation as BaseJobDeclarationValidation)?.ValidatePackagesActualPackageCount();
				}
			}
		}
		ZString fCW_HouseBill;

		public ZPropertyInfo CW_HouseBillInfo
		{
			get { return GetZPropertyInfo(Schema.CW_HouseBill); }
		}

		public bool CW_HouseBill_ReadOnly
		{
			get { return IsChild; }
		}

		void UpdateChildrenHouseBill(ZString houseBill)
		{
			foreach (var childPackage in Children)
			{
				childPackage.CW_HouseBill = houseBill;
			}
		}

		#endregion

		#region CW_ContainerNoOrEquipmentNo
		[BusinessObjectTestExclude()]
		[MaxLength(20)]
		[List(nameof(ContainersAndEquipmentsOnDeclaration_List))]
		public virtual ZString CW_ContainerNoOrEquipmentNo
		{
			get
			{
				ZString result = fCW_ContainerNoOrEquipmentNo;
				if (PackingGroup is BasePackingGroup packingGroup)
				{
					if (packingGroup.Container is BaseCusContainer container)
					{
						result = container.CO_ContainerNumber;
					}
					else if (packingGroup.Equipment is CusEquipment equipment)
					{
						result = equipment.CEQ_IdentificationNumber;
					}
				}

				return result;
			}
			set
			{
				var oldValue = CW_ContainerNoOrEquipmentNo;

				if (oldValue != value)
				{
					CheckMaximumLength(CW_ContainerNoOrEquipmentNoInfo, value);
					LinkToRightHouseBillAndContainerNoOrEquipmentNo(CW_HouseBill, value);
					fCW_ContainerNoOrEquipmentNo = value;
					UpdateChildrenContainerNo(value);
					SynchronizeInvoiceLineContainersWithPackages(oldValue);
					CW_ContainerNoOrEquipmentNoInfo.RefreshBinding();
					Validation.ValidateCW_ContainerNoOrEquipmentNo();
				}
			}
		}
		ZString fCW_ContainerNoOrEquipmentNo;

		public bool CW_ContainerNoOrEquipmentNo_ReadOnly
		{
			get { return CW_HouseBill.IsEmpty || IsChild; }
		}

		public ZPropertyInfo CW_ContainerNoOrEquipmentNoInfo
		{
			get { return GetZPropertyInfo(Schema.CW_ContainerNoOrEquipmentNo); }
		}

		void UpdateChildrenContainerNo(ZString containerNo)
		{
			foreach (var childPackage in Children)
			{
				childPackage.CW_ContainerNoOrEquipmentNo = containerNo;
			}
		}

		#endregion

		#region LinkToRightHouseBillAndContainerNoOrEquipmentNo

		void LinkToRightHouseBillAndContainerNoOrEquipmentNo(ZString houseBillNo, ZString containerNoOrEquipmentNo)
		{
			if (houseBillNo != CW_HouseBill || containerNoOrEquipmentNo != CW_ContainerNoOrEquipmentNo)
			{
				if (Declaration == null)
				{
					throw new DeveloperNotificationException("You can only set CW_HouseBill or CW_ContainerNoOrEquipmentNo when accessing a Package via the Declaration.Packages Collection.");
				}
				else
				{
					RemoveFromCurrentGroup();
					AddToMatchingOrNewGroup(houseBillNo, containerNoOrEquipmentNo);
				}
			}
		}

		void RemoveFromCurrentGroup()
		{
			var currentGroup = PackingGroup;
			if (currentGroup != null)
			{
				var houseBill = currentGroup.Bill;
				var container = currentGroup.Container;
				var equipment = currentGroup.Equipment;

				using (GetValidationSuspender())
				{
					CW_CR_HouseContainer = ZGuid.Empty;
				}

				houseBill?.Validation.ValidateCU_BillNum();
				container?.Validation.ValidateCO_ContainerNumber();
				equipment?.Validation.ValidateCEQ_IdentificationNumber();
			}
		}

		void AddToMatchingOrNewGroup(ZString houseBillNo, ZString containerNoOrEquipmentNo)
		{
			if (Declaration is BaseJobDeclaration declaration)
			{
				var houseBill = declaration.Bills.FindByBillUniqueCode(houseBillNo);
				var container = declaration.CusContainers.Find(containerNoOrEquipmentNo);
				var equipment = container == null ? declaration.Equipments.FirstOrDefault(x => x.CEQ_IdentificationNumber.EqualsIgnoringCase(containerNoOrEquipmentNo)) : null;

				LinkToPackingGroup(houseBill, (BusinessObject)container ?? equipment);

				houseBill?.Validation.ValidateCU_BillNum();
				container?.Validation.ValidateCO_ContainerNumber();
				equipment?.Validation.ValidateCEQ_IdentificationNumber();
			}
		}

		void SynchronizeInvoiceLineContainersWithPackages(ZString oldContainerNo)
		{
			if (Declaration is BaseJobDeclaration declaration && declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				foreach (BaseJobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.SynchronizeContainersPivotWithPackagesPivot(this, oldContainerNo);
				}
			}
		}

		#endregion

		#region Related Business Objects

		public BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Bill != null ? Bill.Declaration : null;
				}
				return fDeclaration;
			}
			set { fDeclaration = value; }
		}
		BaseJobDeclaration fDeclaration;

		public Bill Bill
		{
			get { return PackingGroup != null ? PackingGroup.Bill : null; }
		}

		public BasePackingGroup PackingGroup
		{
			get
			{
				if (CW_CR_HouseContainer.IsEmpty)
				{
					fPackingGroup = null;
				}
				else if (fPackingGroup == null || fPackingGroup.PK != CW_CR_HouseContainer || fPackingGroup.IsDeleted)
				{
					fPackingGroup = Factory.Load<BasePackingGroup>(CW_CR_HouseContainer);
				}
				return fPackingGroup;
			}
		}
		BasePackingGroup fPackingGroup;

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public IEnumerable<BasePackage> Children
		{
			get
			{
				var query = new ZQuery(CusDecHouseContainerPackSchema.CW_CW_Parent, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.Load<BasePackage>(query);
			}
		}

		#endregion

		public override void Delete()
		{
			DeleteAllPivots();
			UnlinkChildren();
			base.Delete();
		}

		void DeleteAllPivots()
		{
			if (Declaration is BaseJobDeclaration declaration)
			{
				if (declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
				{
					var query = new ZQuery(CusHouseContPackInvoiceLinePivotSchema.CHC_CW, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					var pivotsToKill = Factory.Load<InvoiceLinePackagePivot>(query);
					foreach (var chc in pivotsToKill)
					{
						chc.Delete();
					}
				}

				if (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
				{
					var query = new ZQuery(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;

					var pivotsToKill = Factory.Load<InvoiceHeaderPackagePivot>(query);
					foreach (var chz in pivotsToKill)
					{
						chz.Delete();
					}
				}
			}
		}

		void UnlinkChildren()
		{
			foreach (var childPackage in Children)
			{
				childPackage.CW_CW_Parent = ZGuid.Empty;
			}
		}

		protected override void OnFactorySaving()
		{
			if (!IsDeleted && ShouldDeleteIfPackQtyIsEmpty && CW_PackQty == 0)
			{
				Delete();
			}
			base.OnFactorySaving();
		}

		protected virtual bool ShouldDeleteIfPackQtyIsEmpty
		{
			get { return true; }
		}

		internal void LinkToPackingGroup(Bill houseBill, BusinessObject containerOrEquipment)
		{
			if (Declaration is BaseJobDeclaration declaration)
			{
				BasePackingGroup resultParent = declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(houseBill, containerOrEquipment);

				if (resultParent == null && houseBill != null)
				{
					resultParent = new BasePackingGroup.Loader(Factory).GetPackingGroupMatching(houseBill, containerOrEquipment);
				}

				var recyclablePackingGroups = declaration.PackingGroups.GetPackingGroupsWithNoPackages();

				if (resultParent == null && recyclablePackingGroups.Count > 0)
				{
					resultParent = recyclablePackingGroups[0];
				}

				if (resultParent != null && recyclablePackingGroups.Contains(resultParent))
				{
					recyclablePackingGroups.Remove(resultParent);
				}

				recyclablePackingGroups.ForEach((BasePackingGroup p) => { p.FetchStrategy.FetchForDelete(); });
				recyclablePackingGroups.ForEach((BasePackingGroup p) => { p.Delete(); });

				if (resultParent == null)
				{
					resultParent = declaration.PackingGroups.AddNew();
				}

				SetForeignKeys(resultParent, houseBill, containerOrEquipment);
			}
		}

		void SetForeignKeys(BasePackingGroup packingGroup, Bill houseBill, BusinessObject containerOrEquipment)
		{
			var hasChangesSuspended = (houseBill?.IsSettingHasChangesSuspended ?? false) || (containerOrEquipment?.IsSettingHasChangesSuspended ?? false);
			var validationSuspended = (houseBill?.IsValidationSuspended ?? false) || (containerOrEquipment?.IsValidationSuspended ?? false);

			var hasChangeSuspender = hasChangesSuspended ? packingGroup.SuspendSettingHasChanges() : null;
			var validationSuspender = validationSuspended ? packingGroup.GetValidationSuspender() : null;

			try
			{
				packingGroup.CR_CU_HouseBill = houseBill != null ? houseBill.PK : ZGuid.Empty;
				var container = containerOrEquipment as BaseCusContainer;
				var containerPK = ZGuid.Empty;
				CusEquipment equipment = null;
				if (container == null)
				{
					equipment = containerOrEquipment as CusEquipment;
				}
				else
				{
					containerPK = container.PK;
				}

				packingGroup.CR_CO_Container = containerPK;
				packingGroup.CR_CEQ_Equipment = equipment?.PK ?? ZGuid.Empty;
			}
			finally
			{
				if (hasChangeSuspender != null)
				{
					hasChangeSuspender.Dispose();
				}
				if (validationSuspender != null)
				{
					validationSuspender.Dispose();
				}
			}

			hasChangeSuspender = hasChangesSuspended ? SuspendSettingHasChanges() : null;
			validationSuspender = validationSuspended ? GetValidationSuspender() : null;

			try
			{
				CW_CR_HouseContainer = packingGroup.PK;
			}
			finally
			{
				if (hasChangeSuspender != null)
				{
					hasChangeSuspender.Dispose();
				}
				if (validationSuspender != null)
				{
					validationSuspender.Dispose();
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return Declaration is BaseJobDeclaration declaration && !declaration.ShouldSynchroniseWithShipment(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return Bill.ReasonForCannotDeleteWhenSynchronised; }
		}

		#endregion

		#region IPackLineInfo Members

		ZString IPackLineInfo.ContainerNumber
		{
			get { return CW_ContainerNoOrEquipmentNo; }
		}

		ZString IPackLineInfo.GoodsDescription
		{
			get { return ""; }
		}

		ZDecimal IPackLineInfo.Height
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IPackLineInfo.Length
		{
			get { return ZDecimal.Zero; }
		}

		ZString IPackLineInfo.MarksAndNumbers
		{
			get { return CW_MarksAndNos; }
		}

		ZInt IPackLineInfo.NumberOfPackages
		{
			get { return CW_PackQty; }
		}

		ZString IPackLineInfo.PackType
		{
			get { return CW_PackType; }
		}

		ZString IPackLineInfo.UnitOfDimension
		{
			get { return ZString.Empty; }
		}

		ZVolume IPackLineInfo.Volume
		{
			get { return new ZVolume(); }
		}

		ZWeight IPackLineInfo.Weight
		{
			get { return new ZWeight(); }
		}

		ZDecimal IPackLineInfo.Width
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region PackagePivot

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public BasePackagePivotsCollection<InvoiceLinePackagePivot> InvoiceLinePivotCollection
		{
			get
			{
				if (fInvoiceLinePivotCollection == null)
				{
					fInvoiceLinePivotCollection = GetNewInvoiceLinePivotCollectionCore();

					if (Declaration is BaseJobDeclaration declaration && declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
					{
						fInvoiceLinePivotCollection.Load();
						fInvoiceLinePivotCollection.IsManagedForDataRefresh = true;
						RegisterEditableChildObject(fInvoiceLinePivotCollection);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)fInvoiceLinePivotCollection).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						fInvoiceLinePivotCollection.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return fInvoiceLinePivotCollection;
			}
		}

		BasePackagePivotsCollection<InvoiceLinePackagePivot> fInvoiceLinePivotCollection;

		protected virtual BasePackagePivotsCollection<InvoiceLinePackagePivot> GetNewInvoiceLinePivotCollectionCore()
		{
			return new BasePackagePivotsCollection<InvoiceLinePackagePivot>(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public BasePackagePivotsCollection<InvoiceHeaderPackagePivot> InvoiceHeaderPivotCollection
		{
			get
			{
				if (fInvoiceHeaderPivotCollection == null)
				{
					fInvoiceHeaderPivotCollection = new BasePackagePivotsCollection<InvoiceHeaderPackagePivot>(this);

					if (Declaration is BaseJobDeclaration declaration && declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
					{
						fInvoiceHeaderPivotCollection.Load();
						fInvoiceHeaderPivotCollection.IsManagedForDataRefresh = true;
						RegisterEditableChildObject(fInvoiceHeaderPivotCollection);
					}
					else
					{
						((ILegacyBusinessObjectCollectionInternals)fInvoiceHeaderPivotCollection).SetOverriddenAdditionalFilter(ZQuery.NoResultQuery);
						fInvoiceHeaderPivotCollection.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return fInvoiceHeaderPivotCollection;
			}
		}

		BasePackagePivotsCollection<InvoiceHeaderPackagePivot> fInvoiceHeaderPivotCollection;

		public bool IsPivotCollectionLoaded(PivotLevel pivotLevel)
		{
			return pivotLevel == PivotLevel.Invoice
				? fInvoiceHeaderPivotCollection != null
				: pivotLevel == PivotLevel.InvoiceLine && fInvoiceLinePivotCollection != null;
		}

		public ZInt TotalUsageCount
		{
			get
			{
				if (totalUsageCount == null)
				{
					totalUsageCount = new CachedProperty<ZInt>(Factory, delegate
					{
						var totalCount = 0;
						if (Declaration is BaseJobDeclaration declaration)
						{
							if (declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
							{
								var query = new ZQuery();
								query.AddToFilter(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_JE, declaration.PK);
								query.AddToFilter(CusHouseContPackInvoiceHeaderPivotSchema.CHZ_CW, PK);
								query.FetchOnlyFromLocalCache = !declaration.IsInDatabase;

								totalCount += Factory.Load<InvoiceHeaderPackagePivot>(query).Sum(c => c.CHZ_NumberOfPacks);
							}

							if (declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
							{
								var query = new ZQuery();
								query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.CHC_JE, declaration.PK);
								query.AddToFilter(CusHouseContPackInvoiceLinePivotSchema.CHC_CW, PK);
								query.FetchOnlyFromLocalCache = !declaration.IsInDatabase;

								totalCount += Factory.Load<InvoiceLinePackagePivot>(query).Sum(c => c.CHC_NumberOfPacks);
							}
						}
						return totalCount;
					});
				}
				return totalUsageCount.Value;
			}
		}

		CachedProperty<ZInt> totalUsageCount;

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new BasePackageFetchStrategy(this);
		}

		public bool IsLowestPackage
		{
			get { return Declaration is BaseJobDeclaration declaration && !declaration.Packages.Cast<BasePackage>().Any(x => this.PK == x.CW_CW_Parent); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusDecHouseContainerPackLookups.IrrelevantPackages))]
		public override ZGuid CW_CW_Parent
		{
			get => base.CW_CW_Parent;
			set
			{
				var oldParent = ParentPackage;
				base.CW_CW_Parent = value;
				var parentPackage = ParentPackage;

				var decalaration = Declaration;

				if (decalaration != null)
				{
					decalaration.Packages.MarkAsNeedingValidation();
					decalaration.PackagesActualPackageCountInfo.RefreshBinding();
				}

				if (parentPackage != null && !parentPackage.IsDeleted)
				{
					CW_HouseBill = parentPackage.CW_HouseBill;
					CW_ContainerNoOrEquipmentNo = parentPackage.CW_ContainerNoOrEquipmentNo;
				}

				foreach (var invoiceLine in InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Where(p => p.InvoiceLine != null).Select(p => p.InvoiceLine).ToList())
				{
					if (oldParent != null)
					{
						invoiceLine.ToggleLinkageWithPackage(oldParent, false);
					}

					invoiceLine.ToggleLinkageWithPackage(ParentPackage, true);
				}

				foreach (var invoiceHeader in InvoiceHeaderPivotCollection.Cast<InvoiceHeaderPackagePivot>().Where(p => p.InvoiceHeader != null).Select(p => p.InvoiceHeader).ToList())
				{
					if (oldParent != null)
					{
						invoiceHeader.ToggleLinkageWithPackage(oldParent, false);
					}

					invoiceHeader.ToggleLinkageWithPackage(ParentPackage, true);
				}

				oldParent?.Validation.ValidateCW_PackQty();
				parentPackage?.Validation.ValidateCW_PackQty();
			}
		}

		public ZString CW_UniqueCode => this.CW_PackQty + ":" + this.CW_PackType;

		public BasePackage ParentPackage => Factory.Load<BasePackage>(CW_CW_Parent);

		public ZBool IsChild => !CW_CW_Parent.IsEmpty;

		public IEnumerable<BasePackage> Ancestors
		{
			get
			{
				if (ParentPackage != null && ParentPackage != this)
				{
					yield return ParentPackage;

					foreach (BasePackage ancestor in ParentPackage.Ancestors)
					{
						yield return ancestor;
					}
				}
			}
		}

		public ZPropertyInfo CW_UniqueCodeInfo => GetZPropertyInfo(Schema.CW_UniqueCode);

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Declaration as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CW_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(BasePackingGroup);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CW_CR_HouseContainerInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		public override bool IsSavedByFactory => base.IsSavedByFactory && (IsDeleted || (Declaration?.IsPersistent ?? false));
	}
}
