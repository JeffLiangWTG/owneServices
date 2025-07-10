using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalCopyIgnoreElement(UniversalCopyIgnoreElement.Commodity)]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondCargoDesc : Customs.Business.CusInBondCargoDesc,
		Integration.Customs.US.InBond.ICusInBondCargoDesc,
		IInBondTariffLineDetails,
		Customs.Business.WarehouseExtensions.IWarehouseProductLine,
		Customs.Business.ICusInBondCargoDescTypeProvider,
		IInBondContainerMarksAndNumbers
	{
		public CusInBondCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			shouldCalculatePivot = true;
		}

		public new class Schema : Customs.Business.CusInBondCargoDesc.Schema
		{
			public const string BY_PartNumberForBinding = "BY_PartNumberForBinding";
			public const string BY_FormattedHarmonisedTariffForBinding = "BY_FormattedHarmonisedTariffForBinding";
		}

		#region New Properties

		public bool ShouldSynchronise
		{
			get
			{
				var container = Container;
				return container != null && container.ShouldSynchronise;
			}
		}

		public ZDateTime EffectiveDateForDutyRate => ZDateTime.Today;

		public ZGuid ImporterPK
		{
			get
			{
				var header = Header;
				return header == null ? ZGuid.Empty : header.ImporterOrgPK;
			}
		}

		public OrgHeader Importer
		{
			get
			{
				var header = Header;
				return header == null ? null : header.ImporterOrg;
			}
		}

		public CusInBondBill Bill
		{
			get
			{
				CusInBondContainer container = Container;
				return container == null ? null : container.Bill;
			}
		}

		public ZWeight Weight
		{
			get { return new ZWeight(BY_GrossWeight, BY_GrossWeightUnit); }
		}

		public bool IsDetailedInBond
		{
			get
			{
				var header = Header;
				return header != null && header.IsDetailedInBond;
			}
		}

		public bool HasPieceCountInContainer
		{
			get
			{
				return Container != null && Container.BC_PieceCount != 0;
			}
		}

		public bool IsTopLevelCommodity
		{
			get { return BY_ParentTableCode == CusInBondContainerSchema.Constants.Prefix; }
		}

		public CusInBondCargoDesc ParentCommodity
		{
			get { return IsTopLevelCommodity ? null : Parent as CusInBondCargoDesc; }
		}

		bool CopyParentDefault
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		string ParentTableName
		{
			get
			{
				var header = Header;
				return header != null ? header.ParentTableName : string.Empty;
			}
		}

		#endregion

		#region Override Properties

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_OH_Supplier_ReadOnly))]
		public override ZGuid BY_OH_Supplier
		{
			get { return GetEffectiveSupplierValueToReturn(base.BY_OH_Supplier, IsTopLevelCommodity); }
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				var oldValue = BY_OH_Supplier;
				var newValue = ShouldPartRelatedDataBeReadOnly ? ZGuid.Empty : GetEffectiveSupplierValueToSet(value);
				var hasChanged = !IsCopying && oldValue != newValue;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_OH_Supplier = newValue;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		bool BY_OH_Supplier_ReadOnly
		{
			get { return ShouldPartRelatedDataBeReadOnly; }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Tariffs))]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc|BY_FormattedHarmonisedTariffForBinding", Caption = "Tariff")]
		public ZString BY_FormattedHarmonisedTariffForBinding
		{
			get { return HasChildCommodities ? (ZString)CheckSubLevelMessage : BY_FormattedHarmonisedTariff; }
			set { BY_FormattedHarmonisedTariff = value; }
		}

		public ZPropertyInfo BY_FormattedHarmonisedTariffForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.BY_FormattedHarmonisedTariffForBinding, x => BY_FormattedHarmonisedTariffInfo); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Tariffs))]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc|BY_FormattedHarmonisedTariff", Caption = "Tariff")]
		[ReadOnlyMember(nameof(BY_FormattedHarmonisedTariff_ReadOnly))]
		public override ZString BY_FormattedHarmonisedTariff
		{
			get { return TariffFormatter.DisplayFormat(BY_HarmonisedTariff); }
			set
			{
				ZString oldValue = BY_FormattedHarmonisedTariff;
				BY_HarmonisedTariff = ShouldHarmonisedRelatedDataBeSetBeReadOnly ? ZString.Empty : TariffFormatter.Format(value);
				BY_FormattedHarmonisedTariffInfo.RefreshBinding(oldValue);
			}
		}

		bool BY_FormattedHarmonisedTariff_ReadOnly
		{
			get { return ShouldHarmonisedRelatedDataBeSetBeReadOnly; }
		}

		#region BY_ParentID

		public override ZGuid BY_ParentID
		{
			get { return base.BY_ParentID; }
			set
			{
				var oldValue = BY_ParentID;
				var hasChanged = !IsCopying && oldValue != value;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_ParentID = value;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		public override ZString BY_ParentTableCode
		{
			get { return base.BY_ParentTableCode; }
			set
			{
				var oldValue = BY_ParentTableCode;
				base.BY_ParentTableCode = value;
				if (!IsCopying && oldValue != BY_ParentTableCode && IsTopLevelCommodity && BY_MarksAndNumbers.IsEmpty)
				{
					BY_MarksAndNumbers = Core.Constants.ContainerMarking.NoMarks;
				}
			}
		}

		public BusinessObject Parent
		{
			get
			{
				BusinessObject result = null;
				switch (BY_ParentTableCode)
				{
					case CusInBondContainerSchema.Constants.Prefix:
						result = Factory.Load<CusInBondContainer>(BY_ParentID);
						break;
					case CusInBondCargoDescSchema.Constants.Prefix:
						result = Factory.Load<CusInBondCargoDesc>(BY_ParentID);
						break;
				}

				return result;
			}
		}

		public CusInBondContainer Container
		{
			get
			{
				CusInBondContainer result = null;
				var parent = Parent;
				var container = Parent as CusInBondContainer;
				if (container != null)
				{
					result = container;
				}
				else if (parent is CusInBondCargoDesc)
				{
					var parentCommodity = (CusInBondCargoDesc)parent;
					if (parentCommodity != null)
					{
						result = parentCommodity.Container;
					}
				}
				return result;
			}
		}

		#endregion

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_MarksAndNumbers_ReadOnly))]
		public override ZString BY_MarksAndNumbers
		{
			get { return base.BY_MarksAndNumbers; }
			set { base.BY_MarksAndNumbers = BY_MarksAndNumbers_ReadOnly ? ZString.Empty : value; }
		}

		bool BY_MarksAndNumbers_ReadOnly
		{
			get { return !IsTopLevelCommodity; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_PieceCount_ReadOnly))]
		public override ZInt BY_PieceCount
		{
			get { return base.BY_PieceCount; }
			set { base.BY_PieceCount = BY_PieceCount_ReadOnly ? ZInt.Zero : value; }
		}

		bool BY_PieceCount_ReadOnly
		{
			get { return !IsTopLevelCommodity; }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.ManifestUnitList))]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc|BY_ManifestUnitCode", Caption = "Manifest Unit", MediumCaption = "Manifest UQ")]
		[ReadOnlyMember(nameof(BY_ManifestUnitCode_ReadOnly))]
		public override ZString BY_ManifestUnitCode
		{
			get { return base.BY_ManifestUnitCode; }
			set { base.BY_ManifestUnitCode = BY_ManifestUnitCode_ReadOnly ? ZString.Empty : value; }
		}

		bool BY_ManifestUnitCode_ReadOnly
		{
			get { return !IsTopLevelCommodity; }
		}

		[ReadOnlyMember(nameof(BY_GrossWeight_ReadOnly))]
		[MeasureUnit(Schema.BY_GrossWeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal BY_GrossWeight
		{
			get { return base.BY_GrossWeight; }
			set { base.BY_GrossWeight = ShouldHarmonisedRelatedDataBeSetBeReadOnly ? ZDecimal.Zero : value; }
		}

		bool BY_GrossWeight_ReadOnly => ShouldHarmonisedRelatedDataBeSetBeReadOnly;

		[ReadOnlyMember(nameof(BY_MonetaryValue_ReadOnly))]
		public override ZDecimal BY_MonetaryValue
		{
			get { return base.BY_MonetaryValue; }
			set { base.BY_MonetaryValue = ShouldHarmonisedRelatedDataBeSetBeReadOnly ? ZDecimal.Zero : value; }
		}

		bool BY_MonetaryValue_ReadOnly
		{
			get { return ShouldHarmonisedRelatedDataBeSetBeReadOnly; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.WeightUnitList))]
		[ReadOnlyMember(nameof(BY_GrossWeightUnit_ReadOnly))]
		public override ZString BY_GrossWeightUnit
		{
			get { return base.BY_GrossWeightUnit; }
			set { base.BY_GrossWeightUnit = ShouldHarmonisedRelatedDataBeSetBeReadOnly ? ZString.Empty : value; }
		}

		bool BY_GrossWeightUnit_ReadOnly => ShouldHarmonisedRelatedDataBeSetBeReadOnly;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Tariffs))]
		[MaxLength(Schema.BY_HarmonisedTariffMaxLength)]
		public override ZString BY_HarmonisedTariff
		{
			get { return base.BY_HarmonisedTariff; }
			set
			{
				ZString oldFormattedValue = BY_FormattedHarmonisedTariff;
				base.BY_HarmonisedTariff = TariffFormatter.Format(value);
				BY_FormattedHarmonisedTariffInfo.RefreshBinding(oldFormattedValue);
			}
		}

		public override ZPropertyInfo BY_HarmonisedTariffInfo
		{
			get { return GetZPropertyInfo(Schema.BY_HarmonisedTariff); }
		}

		#region Product Details

		[BusinessObjectTestExclude]
		public override ZGuid BY_OP_Part
		{
			get { return base.BY_OP_Part; }
			set
			{
				var oldValue = BY_OP_Part;
				base.BY_OP_Part = value;
				if (!IsCopying && oldValue != BY_OP_Part)
				{
					if (PartSyncManager != null)
					{
						PartSyncManager.ReloadPart = true;
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Parts))]
		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondCargoDesc|BY_PartNumberForBinding", Caption = "Product Number", MediumCaption = "Product No")]
		public ZString BY_PartNumberForBinding
		{
			get
			{
				var result = ZString.Empty;
				if (IsTopLevelCommodity && (ChildCommodities.HasACommodityWithPartNumber || ChildCommodities.HasACommodityWithDifferentSupplier(GetEffectiveSupplierValueToReturn(base.BY_OH_Supplier, false))))
				{
					result = CheckSubLevelMessage;
				}
				else
				{
					result = BY_PartNumber;
				}
				return result;
			}
			set { BY_PartNumber = value; }
		}

		public ZPropertyInfo BY_PartNumberForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.BY_PartNumberForBinding, x => BY_PartNumberInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.Parts))]
		[ReadOnlyMember(nameof(BY_PartNumber_ReadOnly))]
		public override ZString BY_PartNumber
		{
			get => base.BY_PartNumber;
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				using (GetValidationSuspender())
				{
					var oldValue = BY_PartNumber;
					var newValue = ShouldPartRelatedDataBeReadOnly ? ZString.Empty : value;
					var hasChanged = !IsCopying && oldValue != newValue;
					if (hasChanged)
					{
						MarkPivotRefresh();
					}
					base.BY_PartNumber = newValue;
					if (hasChanged)
					{
						RefreshPart();
						if (BY_PartNumber.IsEmpty)
						{
							using (SuspendPivotRefresh())
							{
								BY_PartAttrib1 = ZString.Empty;
								BY_PartAttrib2 = ZString.Empty;
								BY_PartAttrib3 = ZString.Empty;
								BY_SerialNumber = ZString.Empty;
							}
							BY_WarehouseEntryNumber = ZString.Empty;
							BY_WarehouseEntryLineNo = ZShort.Zero;
							BY_InvoiceQuantity = ZDecimal.Zero;
							if (!IsTopLevelCommodity)
							{
								ChildCommodities.RemoveAndDeleteAll();
							}
						}
						else if (!IsTopLevelCommodity)
						{
							var parentCommodity = ParentCommodity;
							if (parentCommodity != null && parentCommodity.IsTopLevelCommodity && !parentCommodity.BY_PartNumber.IsEmpty)
							{
								parentCommodity.BY_PartNumber = ZString.Empty;
							}
						}
						ChildCommodities.RefreshBinding();
					}
				}
				if (!IsCopying)
				{
					Validation.ValidateBY_PartNumber();
				}
			}
		}

		bool BY_PartNumber_ReadOnly
		{
			get { return ShouldPartRelatedDataBeReadOnly; }
		}

		public bool BY_PartNumber_CanBeSetByCustomer
		{
			get { return IsTopLevelCommodity ? !ChildCommodities.HasACommodityWithPartNumber : ParentCommodityHasNoPart; }
		}

		bool ParentCommodityHasNoPart
		{
			get
			{
				var parent = ParentCommodity;
				return parent != null && parent.BY_PartNumber.IsEmpty;
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.PartAttrib1List))]
		[ReadOnlyMember(nameof(BY_PartAttrib1_ReadOnly))]
		public override ZString BY_PartAttrib1
		{
			get => base.BY_PartAttrib1;
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				var oldValue = BY_PartAttrib1;
				var newValue = BY_PartAttrib1_ReadOnly ? ZString.Empty : value;
				var hasChanged = !IsCopying && oldValue != newValue;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_PartAttrib1 = newValue;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		bool BY_PartAttrib1_ReadOnly => BY_PartNumber.IsEmpty;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.PartAttrib2List))]
		[ReadOnlyMember(nameof(BY_PartAttrib2_ReadOnly))]
		public override ZString BY_PartAttrib2
		{
			get => base.BY_PartAttrib2;
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				var oldValue = BY_PartAttrib2;
				var newValue = BY_PartAttrib2_ReadOnly ? ZString.Empty : value;
				var hasChanged = !IsCopying && oldValue != newValue;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_PartAttrib2 = newValue;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		bool BY_PartAttrib2_ReadOnly => BY_PartNumber.IsEmpty;

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusInBondCargoDescLookups.PartAttrib3List))]
		[ReadOnlyMember(nameof(BY_PartAttrib3_ReadOnly))]
		public override ZString BY_PartAttrib3
		{
			get => base.BY_PartAttrib3;
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				var oldValue = BY_PartAttrib3;
				var newValue = BY_PartAttrib3_ReadOnly ? ZString.Empty : value;
				var hasChanged = !IsCopying && oldValue != newValue;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_PartAttrib3 = newValue;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		bool BY_PartAttrib3_ReadOnly => BY_PartNumber.IsEmpty;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_SerialNumber_ReadOnly))]
		public override ZString BY_SerialNumber
		{
			get => base.BY_SerialNumber;
			set
			{
				CalculatePreviousPivotPKIfNeeded();
				var oldValue = BY_SerialNumber;
				var newValue = BY_SerialNumber_ReadOnly ? ZString.Empty : value;
				var hasChanged = !IsCopying && oldValue != newValue;
				if (hasChanged)
				{
					MarkPivotRefresh();
				}
				base.BY_SerialNumber = newValue;
				if (hasChanged)
				{
					RefreshPart();
				}
			}
		}

		bool BY_SerialNumber_ReadOnly => BY_PartNumber.IsEmpty;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_WarehouseEntryNumber_ReadOnly))]
		public override ZString BY_WarehouseEntryNumber
		{
			get { return base.BY_WarehouseEntryNumber; }
			set { base.BY_WarehouseEntryNumber = BY_WarehouseEntryNumber_ReadOnly ? ZString.Empty : value; }
		}

		bool BY_WarehouseEntryNumber_ReadOnly
		{
			get { return (MoveHeader?.IsExBondAutomationEnabledAndNotDisabled ?? false) && BY_PartNumber.IsEmpty; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_WarehouseEntryLineNo_ReadOnly))]
		public override ZShort BY_WarehouseEntryLineNo
		{
			get { return base.BY_WarehouseEntryLineNo; }
			set { base.BY_WarehouseEntryLineNo = BY_WarehouseEntryLineNo_ReadOnly ? ZShort.Zero : value; }
		}

		bool BY_WarehouseEntryLineNo_ReadOnly
		{
			get { return (MoveHeader?.IsExBondAutomationEnabledAndNotDisabled ?? false) && BY_PartNumber.IsEmpty; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_InvoiceQuantity_ReadOnly))]
		public override ZDecimal BY_InvoiceQuantity
		{
			get { return base.BY_InvoiceQuantity; }
			set
			{
				var oldValue = BY_InvoiceQuantity;
				base.BY_InvoiceQuantity = BY_InvoiceQuantity_ReadOnly ? ZDecimal.Zero : value;
				if (!IsCopying && oldValue != BY_InvoiceQuantity)
				{
					CalculateWeight();
				}
			}
		}

		bool BY_InvoiceQuantity_ReadOnly
		{
			get { return BY_PartNumber.IsEmpty; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(BY_Description_ReadOnly))]
		public override ZString BY_Description
		{
			get { return base.BY_Description; }
			set { base.BY_Description = BY_Description_ReadOnly ? ZString.Empty : value; }
		}

		bool BY_Description_ReadOnly
		{
			get { return !IsTopLevelCommodity; }
		}

		public bool HasChildCommodities
		{
			get { return ChildCommodities.Count > 0; }
		}

		[ChildEditable]
		public CusInBondCargoDescChildCollection ChildCommodities
		{
			get
			{
				if (childCommodities == null)
				{
					childCommodities = new CusInBondCargoDescChildCollection(this);
					childCommodities.Load();
					RegisterEditableChildObject(childCommodities);
				}
				return childCommodities;
			}
		}
		CusInBondCargoDescChildCollection childCommodities;

		#region Pivot

		public CusClassPartPivot Pivot
		{
			get
			{
				RefreshPart();
				if (shouldCalculatePivot || (cachedPivot != null && cachedPivot.IsDeleted))
				{
					shouldCalculatePivot = false;
					cachedPivot = GetPivot();
				}
				return cachedPivot;
			}
		}
		CusClassPartPivot cachedPivot;
		bool shouldCalculatePivot;
		bool shouldRefreshPart;

		CusClassPartPivot GetPivot()
		{
			CusClassPartPivot result = null;
			OrgSupplierPart part = Part;
			bool justUpdatedByDataRefresh = false;
			if (part != null)
			{
				justUpdatedByDataRefresh = part.JustUpdatedByDataRefresh;
				result = part.PivotsForBinding.GetImportMatch(ImporterPK, BY_OH_Supplier, EffectiveDateForDutyRate.Date, GetPartAttribs());
			}
			ZGuid newPivotPK = result == null ? ZGuid.Empty : result.PK;
			wasPivotChanged = justUpdatedByDataRefresh || !previousPivotPK.HasValue || previousPivotPK.Value != newPivotPK;
			previousPivotPK = newPivotPK;
			return result;
		}

		bool WasPivotChanged
		{
			get
			{
				var pivot = Pivot;
				return wasPivotChanged;
			}
		}
		bool wasPivotChanged;

		ZGuid? previousPivotPK;

		void CalculatePreviousPivotPKIfNeeded()
		{
			if (!previousPivotPK.HasValue)
			{
				var pivot = GetPivot();
				previousPivotPK = pivot == null ? ZGuid.Empty : pivot.PK;
			}
		}

		Customs.Business.CusAttributeFilter.AttributeValue[] GetPartAttribs()
		{
			List<Customs.Business.CusAttributeFilter.AttributeValue> result = new List<Customs.Business.CusAttributeFilter.AttributeValue>();
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT1, Value = BY_PartAttrib1 });
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT2, Value = BY_PartAttrib2 });
			result.Add(new Customs.Business.CusAttributeFilter.AttributeValue() { Name = Customs.Business.CusAttributeFilter.AttributeFilterName.AT3, Value = BY_PartAttrib3 });

			return result.ToArray();
		}

		#endregion

		/// <summary>
		/// Called by the PartSyncManager when the Part is changed in another factory
		/// </summary>
		internal void UpdateDetailsOnPartChange()
		{
			if (!IsCopying && !IsDeleted && !IsNull && BY_PartNumber_CanBeSetByCustomer)
			{
				if (WasPivotChanged)
				{
					var existingCommodities = new List<CusInBondCargoDesc>(ChildCommodities.OfType<CusInBondCargoDesc>());
					OrgSupplierPart product = Part;

					if (product == null || product.IsDeleted)
					{
						BY_OP_Part = ZGuid.Empty;
					}
					else
					{
						BY_OP_Part = product.PK;
						CusClassPartPivot[] pivotChildren = null;
						var pivot = Pivot;
						if (pivot == null)
						{
							pivot = Factory.GetNull<CusClassPartPivot>();
						}
						else
						{
							pivotChildren = pivot.Children.OfType<CusClassPartPivot>().Where(x => x.CI_ChildType == ClassificationChildTypeList.Codes.COMPONENT || x.CI_ChildType == ClassificationChildTypeList.Codes.Related).OrderBy(x => x.CI_ChildListOrder).ToArray();
						}
						ZString tariffNo;
						ZString description;
						ZDecimal weight;
						ZString weightUQ;
						CalculateDataFromPivotAndClassification(pivot, product, out tariffNo, out description, out weight, out weightUQ);

						if (pivotChildren == null || pivotChildren.Length == 0)
						{
							existingCommodities.DeleteAll();
							existingCommodities.Clear();
							UpdateData(this, tariffNo, weight, weightUQ);
						}
						else if (!pivot.IsNull)
						{
							var container = Container;
							if (container != null && !container.IsDefaultChildLinesSuspended)
							{
								using (container.SuspendDefaultingChildLines())
								{
									AddChildrenCommodityFromProduct(existingCommodities, tariffNo, container, pivotChildren, weightUQ);
								}
							}

							product.PartUnits.Load();
						}
						if (IsTopLevelCommodity)
						{
							BY_Description = description;
						}
					}
					if (!IsTopLevelCommodity || !BY_OP_Part.IsEmpty)
					{
						existingCommodities.DeleteAll();
					}
				}
				Validation.ValidateBY_PartNumber();
			}
		}

		void UpdateData(CusInBondCargoDesc commodity, ZString tariffNo, ZDecimal weight, ZString weightUQ)
		{
			commodity.BY_HarmonisedTariff = tariffNo;
			commodity.BY_GrossWeight = weight * BY_InvoiceQuantity;
			commodity.BY_GrossWeightUnit = weightUQ;
		}

		void AddChildrenCommodityFromProduct(List<CusInBondCargoDesc> existingCommodities, ZString tariffNo, CusInBondContainer container, CusClassPartPivot[] pivotChildren, ZString partWeightUQ)
		{
			var matchedCommodities = new List<CusInBondCargoDesc>();
			AddOrUpdateCommodity(tariffNo, existingCommodities, matchedCommodities, 0m, partWeightUQ);

			foreach (CusClassPartPivot childPivot in pivotChildren)
			{
				var childWeightUQ = childPivot.CD_WeightUQ;
				var childWeight = childPivot.CD_GrossWeight;
				if (childWeightUQ.IsEmpty)
				{
					childWeightUQ = partWeightUQ;
					childWeight = 0m;
				}
				AddOrUpdateCommodity(childPivot.TariffNumber, existingCommodities, matchedCommodities, childWeight, childWeightUQ);
			}
		}

		void AddOrUpdateCommodity(ZString tariffNo, List<CusInBondCargoDesc> existingCommodities, List<CusInBondCargoDesc> matchedCommodities, ZDecimal weight, ZString weightUQ)
		{
			var commodity = existingCommodities.Find(x => x.BY_HarmonisedTariff == tariffNo && x.BY_PartNumber.IsEmpty);
			if (commodity == null)
			{
				if (!matchedCommodities.Any(x => x.BY_HarmonisedTariff == tariffNo))
				{
					commodity = ChildCommodities.AddNew();
					commodity.BY_HarmonisedTariff = tariffNo;
					matchedCommodities.Add(commodity);
				}
			}
			else
			{
				ClearDataNotApplicableToChildCommodity(commodity);
				existingCommodities.Remove(commodity);
				matchedCommodities.Add(commodity);
			}
			if (!weightUQ.IsEmpty && commodity != null)
			{
				commodity.BY_GrossWeightUnit = weightUQ;
				commodity.BY_GrossWeight = weight * BY_InvoiceQuantity;
			}
		}

		void ClearDataNotApplicableToChildCommodity(CusInBondCargoDesc commodity)
		{
			commodity.BY_Description = ZString.Empty;
			commodity.BY_InvoiceQuantity = ZDecimal.Zero;
			commodity.BY_ManifestUnitCode = ZString.Empty;
			commodity.BY_MarksAndNumbers = ZString.Empty;
			commodity.BY_OH_Supplier = ZGuid.Empty;
			commodity.BY_PartNumber = ZString.Empty;
			commodity.BY_OP_Part = ZGuid.Empty;
			commodity.BY_PieceCount = ZInt.Zero;
		}

		void CalculateDataFromPivotAndClassification(CusClassPartPivot pivot, OrgSupplierPart product, out ZString tariffNo, out ZString description, out ZDecimal weight, out ZString weightUQ)
		{
			var classification = pivot.Classification;
			tariffNo = pivot.CI_TariffNum;
			if (tariffNo.IsEmpty && classification != null)
			{
				tariffNo = pivot.Classification.CC_TariffNum;
			}
			description = product.OP_Desc;
			if (description.IsEmpty && classification != null)
			{
				description = classification.CC_Description;
			}
			if (description.IsEmpty)
			{
				description = GetDescriptionFromHarmonised(tariffNo);
			}
			weight = product.OP_Weight;
			weightUQ = product.OP_WeightUQ;
		}

		ZString GetDescriptionFromHarmonised(ZString harmonisedTariff)
		{
			var tariff = new USCTariff.Loader(Factory).LoadBestMatch(harmonisedTariff, EffectiveDateForDutyRate);
			return tariff != null ? tariff.UE_ShortDescription : ZString.Empty;
		}

		#endregion

		public new CusInBondCargoDescLookups Lookups
		{
			get { return (CusInBondCargoDescLookups)base.Lookups; }
		}

		public new CusInBondCargoDescValidation Validation
		{
			get { return (CusInBondCargoDescValidation)base.Validation; }
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !CopyParentDefault; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = MultilingualString.Join(" ", ResString.GetMultilingualString("C399C656-0046-4B94-A866-C238FDB3CBEE", "This Commodity cannot be deleted"),
						CopyParentDefault
						? ValidationConstants.Synchronize.SynchronizedFromParent(ParentTableName) : (NoResString)".");
				}
				return result;
			}
		}

		#endregion

		#region Override Methods
		public override void OnLoaded()
		{
			base.OnLoaded();
			InitialisePartSyncManager();
			RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered();
		}

		public override void Delete()
		{
			var originalEnabledState = false;
			if (PartSyncManager != null)
			{
				originalEnabledState = PartSyncManager.Enabled;
				PartSyncManager.Enabled = false;
			}
			ChildCommodities.DeleteAll();
			base.Delete();
			if (!IsDeleted && PartSyncManager != null)
			{
				PartSyncManager.Enabled = originalEnabledState;
			}
		}
		#endregion

		public new OrgSupplierPart Part
		{
			get
			{
				OrgSupplierPart result = null;
				if (PartSyncManager.Part != null && !PartSyncManager.Part.IsDeleted)
				{
					result = PartSyncManager.Part;
				}
				return result;
			}
		}

		public CusInBondCargoDescPartSynchronisationManager PartSyncManager
		{
			get { return fPartSyncManager; }
		}

		#region Related Objects

		public new CusInBondHeader Header
		{
			get
			{
				var moveHeader = MoveHeader;
				return moveHeader == null ? null : moveHeader.Header;
			}
		}

		public CusInBondMoveHeader MoveHeader
		{
			get
			{
				var moveDetail = MoveDetail;
				return moveDetail == null ? null : moveDetail.MoveHeader;
			}
		}

		public CusInBondMoveDetail MoveDetail
		{
			get
			{
				var container = Container;
				return container == null ? null : container.MoveDetail;
			}
		}

		#endregion

		#region Implementation

		void CalculateWeight()
		{
			var invoiceQuantity = BY_InvoiceQuantity;
			if (!invoiceQuantity.IsEmpty)
			{
				var part = Part;
				if (part != null)
				{
					var partWeightUQ = Part.OP_WeightUQ;
					if (!HasChildCommodities)
					{
						BY_GrossWeight = invoiceQuantity * Part.OP_Weight;
						BY_GrossWeightUnit = partWeightUQ;
					}
					else
					{
						var existingCommodities = new List<CusInBondCargoDesc>(ChildCommodities.OfType<CusInBondCargoDesc>());
						var pivot = GetPivot();
						if (pivot != null)
						{
							var tariffNo = pivot.TariffNumber;
							UpdateChildCommodityWeight(existingCommodities, tariffNo, ZDecimal.Zero, partWeightUQ);
							foreach (CusClassPartPivot childPivot in pivot.Children)
							{
								var childWeightUQ = childPivot.CD_WeightUQ;
								var childWeight = childPivot.CD_GrossWeight * invoiceQuantity;
								if (childWeightUQ.IsEmpty)
								{
									childWeight = ZDecimal.Zero;
									childWeightUQ = partWeightUQ;
								}
								UpdateChildCommodityWeight(existingCommodities, childPivot.CI_TariffNum, childWeight, childWeightUQ);
							}
						}

						foreach (CusInBondCargoDesc childCommodity in existingCommodities)
						{
							childCommodity.BY_GrossWeight = ZDecimal.Zero;
							childCommodity.BY_GrossWeightUnit = partWeightUQ;
						}
					}
				}
			}
		}

		void UpdateChildCommodityWeight(List<CusInBondCargoDesc> existingCommodities, ZString tariffNo, ZDecimal weight, ZString weightUQ)
		{
			var childCommodity = existingCommodities.Find(x => x.BY_HarmonisedTariff == tariffNo && x.BY_PartNumber.IsEmpty);
			if (childCommodity != null)
			{
				existingCommodities.Remove(childCommodity);
				childCommodity.BY_GrossWeightUnit = weightUQ;
				childCommodity.BY_GrossWeight = weight;
			}
		}

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveSupplierValue(ZGuid headerValue)
		{
			if (effectiveSupplierValueSuspender == null)
			{
				effectiveSupplierValueSuspender = new EffectiveSupplierValueSuspender(this) { HeaderValue = headerValue };
			}
			return effectiveSupplierValueSuspender;
		}

		IZType GetEffectiveSupplierValue()
		{
			return effectiveSupplierValueSuspender == null ? null : effectiveSupplierValueSuspender.HeaderValue;
		}

		EffectiveSupplierValueSuspender effectiveSupplierValueSuspender;

		class EffectiveSupplierValueSuspender : IDisposable
		{
			public EffectiveSupplierValueSuspender(CusInBondCargoDesc commodity)
			{
				this.commodity = commodity;
			}

			public IZType HeaderValue { get; set; }

			readonly CusInBondCargoDesc commodity;

			#region IDisposable Members

			public void Dispose()
			{
				commodity.effectiveSupplierValueSuspender = null;
			}

			#endregion
		}
		#endregion

		bool ShouldPartRelatedDataBeReadOnly
		{
			get { return !BY_PartNumber_CanBeSetByCustomer || (IsTopLevelCommodity && ChildCommodities.HasACommodityWithDifferentSupplier(GetEffectiveSupplierValueToReturn(base.BY_OH_Supplier, false))); }
		}

		bool ShouldHarmonisedRelatedDataBeSetBeReadOnly
		{
			get { return HasChildCommodities; }
		}

		ZGuid GetEffectiveSupplierValueToReturn(ZGuid baseValue, bool checkChildDetails)
		{
			var result = ZGuid.Empty;
			if (BY_PartNumber_CanBeSetByCustomer)
			{
				result = baseValue;

				if (result.IsEmpty)
				{
					var effectiveValue = GetEffectiveSupplierValue();
					if (effectiveValue != null)
					{
						result = (ZGuid)effectiveValue;
					}
					else
					{
						var header = Header;
						if (header != null)
						{
							result = header.BH_OH_Supplier;
						}
					}
				}

				if (checkChildDetails && ChildCommodities.HasACommodityWithDifferentSupplier(result))
				{
					result = ZGuid.Empty;
				}
			}
			return result;
		}

		ZGuid GetEffectiveSupplierValueToSet(ZGuid valuePassed)
		{
			var fallBackSupplierPK = ZGuid.Empty;
			var header = Header;
			if (header != null)
			{
				fallBackSupplierPK = header.BH_OH_Supplier;
			}
			return fallBackSupplierPK == valuePassed ? ZGuid.Empty : valuePassed;
		}

		internal const string CheckSubLevelMessage = "CHECK SUB LEVEL";

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (CusInBondCargoDesc)base.CloneInternal(args);
			result.PartSyncManager.Refresh();
			return result;
		}

		void RefreshDetailsIfPartHasBeenCreatedSinceCodeWasEntered()
		{
			if (!BY_PartNumber.IsEmpty && BY_OP_Part.IsEmpty)
			{
				using (GetValidationSuspender())
				{
					if (BY_HarmonisedTariff.IsEmpty)
					{
						PartSyncManager.Refresh();
					}
					else
					{
						PartSyncManager.OnlySetBY_OP_Part();
					}
				}
			}
		}

		void InitialisePartSyncManager()
		{
			if (fPartSyncManager == null)
			{
				fPartSyncManager = new CusInBondCargoDescPartSynchronisationManager(this);
			}
		}
		CusInBondCargoDescPartSynchronisationManager fPartSyncManager;

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		new TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = new TariffFormatter()); }
		}
		TariffFormatter tariffFormatter;

		protected override Customs.Business.CusInBondCargoDescLookups GetNewLookups()
		{
			return new CusInBondCargoDescLookups(this);
		}

		protected override Customs.Business.CusInBondCargoDescValidation GetNewValidation()
		{
			return new CusInBondCargoDescValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			InitialisePartSyncManager();
		}

		internal void ClearSupplierIfSameWithHeaderValue(ZGuid headerValue)
		{
			CalculatePreviousPivotPKIfNeeded();
			var commodityValue = base.BY_OH_Supplier;
			var needPartRefresh = commodityValue.IsEmpty || headerValue != commodityValue;
			if (needPartRefresh)
			{
				MarkPivotRefresh();
			}
			if (commodityValue.Equals(headerValue))
			{
				using (SuspendEffectiveSupplierValue(headerValue))
				{
					BY_OH_Supplier = ZGuid.Empty;
				}
			}
			if (needPartRefresh)
			{
				RefreshPart();
			}
		}

		internal void RefreshPivot()
		{
			MarkPivotRefresh();
			RefreshPart();
		}

		void RefreshPart()
		{
			if (shouldRefreshPart)
			{
				shouldRefreshPart = false;
				PartSyncManager.Refresh();
			}
		}

		void MarkPivotRefresh()
		{
			if (!IsPivotRefreshSuspended)
			{
				previousPivotPK = ZGuid.Invalid;
				shouldCalculatePivot = true;
				shouldRefreshPart = true;
			}
		}

		IDisposable SuspendPivotRefresh()
		{
			return new PivotRefreshSuspender(this);
		}

		bool IsPivotRefreshSuspended
		{
			get { return _pivotRefreshIndex > 0; }
		}

		int _pivotRefreshIndex;

		class PivotRefreshSuspender : IDisposable
		{
			public PivotRefreshSuspender(CusInBondCargoDesc commodity)
			{
				this.commodity = commodity;
				commodity._pivotRefreshIndex++;
			}

			readonly CusInBondCargoDesc commodity;

			#region IDisposable Members

			public void Dispose()
			{
				commodity._pivotRefreshIndex--;
			}

			#endregion
		}

		protected override Type FeeTypeCore => typeof(Customs.Business.CusInBondFee);
		#endregion

		#region IInBondTariffLineDetails Members

		ZString IInBondTariffLineDetails.CargoDescription
		{
			get { return BY_Description; }
		}

		ZDecimal IInBondTariffLineClassificationDetails.CustomsValue
		{
			get { return BY_MonetaryValue; }
		}

		ZDecimal IInBondTariffLineClassificationDetails.NetWeight
		{
			get { return WeightCalculator.Calculate(Weight); }
		}

		ZString IInBondTariffLineClassificationDetails.NetWeightUQ
		{
			get { return WeightCalculator.CalculateUQ(BY_GrossWeightUnit); }
		}

		ZDecimal IInBondTariffLineDetails.PieceCount
		{
			get { return new ZDecimal(BY_PieceCount); }
		}

		ZString IInBondTariffLineDetails.ManifestUnitCode
		{
			get { return BY_ManifestUnitCode; }
		}

		ZString IInBondTariffLineClassificationDetails.TariffNumber
		{
			get { return BY_HarmonisedTariff.PadRight(Schema.BY_HarmonisedTariffMaxLength, '0'); }
		}

		ZString IInBondTariffLineDetails.MarksAndNumbers
		{
			get { return BY_MarksAndNumbers; }
		}

		IEnumerable<IInBondTariffLineClassificationDetails> IInBondTariffLineDetails.MultiClassifications
		{
			get
			{
				var result = new List<IInBondTariffLineClassificationDetails>();
				foreach (CusInBondCargoDesc childCommodity in ChildCommodities)
				{
					if (childCommodity.HasChildCommodities)
					{
						result.AddRange(childCommodity.ChildCommodities.OfType<IInBondTariffLineClassificationDetails>());
					}
					else
					{
						result.Add(childCommodity);
					}
				}
				result.Sort(new Comparison<IInBondTariffLineClassificationDetails>((x, y) => x.TariffNumber.CompareTo(y.TariffNumber)));

				return result;
			}
		}

		#endregion

		#region IInBondContainerMarksAndNumbers Members

		ZString IInBondContainerMarksAndNumbers.MarksAndNumbers
		{
			get { return BY_MarksAndNumbers; }
		}

		#endregion

		#region ICusInBondCargoDescTypeProvider Members
		Type Customs.Business.ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => GetType();
		#endregion
	}
}
