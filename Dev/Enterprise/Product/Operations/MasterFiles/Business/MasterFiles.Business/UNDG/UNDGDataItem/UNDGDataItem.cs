using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = nameof(OnUniversalCopyFinish))]
	public class UNDGDataItem : AutoUNDGDataItem, IUNDGDataItem, IUNDGSubstancePivotParent, ISupportDataImporting
	{
		public UNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoUNDGDataItem.Schema
		{
			public const string ExceptedQuantity = "ExceptedQuantity";
			public const string PSAGroup = "PSAGroup";
			public const string SubstancePK = "SubstancePK";
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new UNDGDataItemFetchStrategy(this);
		}

		#endregion

		#region Properties

		#region IsAutoAddedItem

		public bool IsAutoAddedItem
		{
			get { return isAutoAddedItem; }
			internal set
			{
				if (isAutoAddedItem != value)
				{
					OnAutoAddedItemNowValid();
					isAutoAddedItem = value;
				}
			}
		}

		bool isAutoAddedItem;

		internal event EventHandler AutoAddedItemNowValid;

		void OnAutoAddedItemNowValid()
		{
			if (AutoAddedItemNowValid != null)
			{
				AutoAddedItemNowValid(this, EventArgs.Empty);
			}
		}

		#endregion

		#region DI_NECWeight

		public override ZDecimal DI_NECWeight
		{
			get
			{
				return base.DI_NECWeight;
			}
			set
			{
				base.DI_NECWeight = value;
				Validation.ValidateDI_NECWeightUQ();
			}
		}

		public virtual bool DI_NECWeight_ReadOnly
		{
			get
			{
				if (Substance == null)
				{
					return true;
				}
				return !IsClassOneDgSubstance;
			}
		}

		#endregion

		#region DI_NECWeightUQ

		[List("Lookups.WeightUnits")]
		public override ZString DI_NECWeightUQ
		{
			get
			{
				return base.DI_NECWeightUQ;
			}
			set
			{
				base.DI_NECWeightUQ = value;
				Validation.ValidateDI_NECWeight();
			}
		}

		public virtual bool DI_NECWeightUQ_ReadOnly
		{
			get
			{
				if (Substance == null)
				{
					return true;
				}
				return !IsClassOneDgSubstance;
			}
		}

		#endregion

		#region IsClassOneSubstance

		public bool IsClassOneDgSubstance
		{
			get
			{
				var match = Regex.Match(Substance.DG_Class.ToString(), @"^1(\.\d)?([a-zA-Z])?$");
				return match.Success;
			}
		}

		#endregion

		#region DI_OC_DGContact

		[List("Lookups.Contacts")]
		public override ZGuid DI_OC_DGContact
		{
			get { return base.DI_OC_DGContact; }
			set
			{
				base.DI_OC_DGContact = value;
				IsAutoAddedItem = false;
			}
		}

		#endregion

		#region ExceptedQuantity

		public ZString ExceptedQuantity => Substance?.DG_ExceptedQuantityCode ?? ZString.Empty;

		#endregion

		#region Proper Shipping Name

		public ZString ProperShippingName => Substance?.DG_PSN ?? ZString.Empty;

		#endregion

		#region PSAGroup

		public ZString PSAGroup
		{
			get
			{
				var psaGroupUNDGReferences = Substance != null
						? Substance.UNDGCountryReferences.Cast<UNDGCountryReference>()
							.Where(x => x.DCR_RN_NKCountry == Core.Constants.CountryCodes.Singapore && x.DCR_Type == Core.Constants.UNDGCountryReference.Type.PSA)
							.OrderBy(x => x.DCR_HasFlashPointLower ? (decimal)x.DCR_FlashPointLowerCentigrade : -273.15m)
						: Enumerable.Empty<UNDGCountryReference>();

				var countryReference = psaGroupUNDGReferences.FirstOrDefault(x => (x.DCR_HasFlashPointUpper || x.DCR_HasFlashPointLower)
														&& (!x.DCR_HasFlashPointLower || x.DCR_FlashPointLowerCentigrade <= DI_DGFlashPoint)
														&& (!x.DCR_HasFlashPointUpper || x.DCR_FlashPointUpperCentigrade >= DI_DGFlashPoint))
					?? psaGroupUNDGReferences.FirstOrDefault(x => (!x.DCR_HasFlashPointUpper && !x.DCR_HasFlashPointLower));

				return countryReference?.DCR_Code ?? ZString.Empty;
			}
		}

		public ZPropertyInfo DG_PSAGroupInfo
		{
			get { return GetZPropertyInfo(Schema.PSAGroup); }
		}

		public virtual bool IsPSAGroupApplicable => false;

		#endregion

		#region Subs
		public override UNDGSubstance Subs
		{
			get
			{
				return UNDGSubstancePivotCollection.DefaultSubstance;
			}
		}

		#endregion

		#region DI_DG_Substance

		public UNDGSubstance Substance
		{
			get
			{
				return UNDGSubstancePivotCollection.DefaultSubstance;
			}
		}

		#endregion

		#region SubstanceCode

		public ZString SubstanceCode
		{
			get
			{
				return Substance == null
					? ZString.Empty
					: (ZString)(Substance.DG_UNNO + Substance.DG_Variant);
			}
		}

		#endregion

		#region DI_DG

		public UNDGSubstance UNDGSubstance => Factory.Load<UNDGSubstance>(DI_DG);

		[RelatedBusinessObject("UNDGSubstance")]
		[List("Lookups.UNDGSubstances")]
		public override ZGuid DI_DG
		{
			get { return base.DI_DG; }
			set
			{
				if (base.DI_DG != value)
				{
					var updateDG_Class = DI_IMOClass.IsEmpty || UNDGSubstance == null || UNDGSubstance.DG_Class == DI_IMOClass;
					var substance = Factory.Load<UNDGSubstance>(value);

					UNDGSubstancePivotCollection.UpdateDefaultPivot(substance);
					LoadLinkedDGSubstanceInfos();

					base.DI_DG = value;
					UpdateUNDG(UNDGSubstance, updateDG_Class);
					ValidateNetExplosiveContent();
				}
			}
		}

		[RelatedBusinessObject("UNDGSubstance")]
		[List("Lookups.UNDGSubstances")]
		public ZGuid SubstancePK
		{
			get
			{
				if (substancePK == null)
				{
					var substance = UNDGSubstancePivotCollection.DefaultSubstance;
					substancePK = substance?.PK ?? ZGuid.Empty;
				}

				return substancePK ?? ZGuid.Empty;
			}
			set
			{
				if (substancePK != value)
				{
					var oldSubstance = Factory.Load<UNDGSubstance>(substancePK ?? ZGuid.Empty);
					var updateDG_Class = DI_IMOClass.IsEmpty || oldSubstance == null || oldSubstance.DG_Class == DI_IMOClass;
					var substance = Factory.Load<UNDGSubstance>(value);

					UNDGSubstancePivotCollection.UpdateDefaultPivot(substance);
					LoadLinkedDGSubstanceInfos();

					substancePK = value;
					base.DI_DG = value;
					UpdateUNDG(substance, updateDG_Class);
					ValidateNetExplosiveContent();

					if (!IsValidationSuspended)
					{
						Validation.ValidateSubstancePK();
					}
				}
			}
		}

		ZGuid? substancePK;

		void ValidateNetExplosiveContent()
		{
			if (!IsValidationSuspended && !IsDeleted)
			{
				Validation.ValidateDI_PackageCount();
				Validation.ValidateDI_F3_NKPackType();
				Validation.ValidateDI_DGWeight();
				Validation.ValidateDI_UnitOfWeight();
				Validation.ValidateDI_DGVolume();
				Validation.ValidateDI_UnitOfVolume();
				Validation.ValidateDI_NECWeight();
				Validation.ValidateDI_NECWeightUQ();
			}
		}

		public virtual ZPropertyInfo SubstancePKInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.SubstancePK); }
		}

		void UpdateUNDG(UNDGSubstance sub, bool dbClass)
		{
			if (sub != null)
			{
				if (!isImportingData && !sub.DG_FlashPoint.IsEmpty)
				{
					DI_DGFlashPoint = Convert.ToDecimal(TemperatureFormatter.FormatTemperatureString(sub.DG_FlashPoint), CultureInfo.InvariantCulture);
					DI_IsCombustible = true;
				}
				if (dbClass)
				{
					DI_IMOClass = sub.DG_Class;
				}
				DI_MPMarinePollutant = sub.DG_MP != UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code ? sub.DG_MP : ZString.Empty;

				CalculateLimitedQuantity();
			}

			IsAutoAddedItem = false;
			OnSubstanceOrClassUpdated();

			DI_TechnicalNameInfo.RefreshBinding();
			DI_MPMarinePollutantInfo.RefreshBinding();
			DI_DG_ClassForBindingInfo.RefreshBinding();
			SubstancePKInfo.RefreshBinding();

			if (!IsValidationSuspended && !IsDeleted)
			{
				Validation.ValidateDI_IMOClass();
				Validation.ValidateDI_DG_ClassForBinding();
			}

			if (DI_NECWeight_ReadOnly)
			{
				DI_NECWeight = ZDecimal.Zero;
			}
			if (DI_NECWeight_ReadOnly)
			{
				DI_NECWeightUQ = ZString.Empty;
			}
		}

		#region DI_DG_ClassForBinding

		public ZString DI_DG_ClassForBinding => UNDGSubstance?.DG_Class ?? ZString.Empty;

		public ZPropertyInfo DI_DG_ClassForBindingInfo => GetZPropertyInfo(nameof(DI_DG_ClassForBinding));

		#endregion

		#endregion

		public override ZBool DI_IsCombustible
		{
			get { return new ZBool(GetValueFromRowSafely(UNDGDataItemSchema.DI_IsCombustible)); }
			set
			{
				SetPropertyValue(DI_IsCombustibleInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDI_IsCombustible();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString DI_ParentTableCode
		{
			get { return base.DI_ParentTableCode; }
			set { base.DI_ParentTableCode = value; }
		}

		[BusinessObjectTestExclude]
		public override ZGuid DI_ParentID
		{
			get { return base.DI_ParentID; }
			set { base.DI_ParentID = value; }
		}

		#region DI_IMOClass

		[List("Lookups.DGClassList")]
		public override ZString DI_IMOClass
		{
			get { return base.DI_IMOClass; }
			set
			{
				if (base.DI_IMOClass != value)
				{
					base.DI_IMOClass = value;
					IsAutoAddedItem = false;

					OnSubstanceOrClassUpdated();

					if (!IsValidationSuspended && !IsDeleted)
					{
						Validation.ValidateDI_DG_NKSubs();
						Validation.ValidateDI_DG();
					}
				}
			}
		}

		#endregion

		#region DI_UnitOfVolume

		[List("Lookups.VolumeUnits")]
		public override ZString DI_UnitOfVolume
		{
			get { return base.DI_UnitOfVolume; }
			set
			{
				if (base.DI_UnitOfVolume != value)
				{
					base.DI_UnitOfVolume = value;
					IsAutoAddedItem = false;
					CalculateLimitedQuantity();
				}
			}
		}

		#endregion

		#region DI_UnitOfWeight

		[List("Lookups.WeightUnits")]
		public override ZString DI_UnitOfWeight
		{
			get { return base.DI_UnitOfWeight; }
			set
			{
				if (base.DI_UnitOfWeight != value)
				{
					base.DI_UnitOfWeight = value;
					IsAutoAddedItem = false;
					CalculateLimitedQuantity();
				}
			}
		}

		#endregion

		#region DI_DGFlashPoint

		[DecimalPlaces(1)]
		public override ZDecimal DI_DGFlashPoint
		{
			get { return base.DI_DGFlashPoint; }
			set
			{
				base.DI_DGFlashPoint = value;
				IsAutoAddedItem = false;
			}
		}

		protected virtual bool DI_DGFlashPoint_ReadOnly
		{
			get
			{
				if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
				{
					return !DI_IsCombustible;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region DI_DGVolume
		[MeasureUnit(Schema.DI_UnitOfVolume, MeasureUnitType.Volume)]
		public override ZDecimal DI_DGVolume
		{
			get { return base.DI_DGVolume; }
			set
			{
				if (base.DI_DGVolume != value)
				{
					base.DI_DGVolume = value;
					IsAutoAddedItem = false;
					CalculateLimitedQuantity();
				}
			}
		}

		#endregion

		#region DI_DGWeight
		[MeasureUnit(Schema.DI_UnitOfWeight, MeasureUnitType.Weight)]
		public override ZDecimal DI_DGWeight
		{
			get { return base.DI_DGWeight; }
			set
			{
				if (base.DI_DGWeight != value)
				{
					base.DI_DGWeight = value;
					IsAutoAddedItem = false;
					CalculateLimitedQuantity();
					Validation.ValidateDI_NECWeight();
				}
			}
		}

		#endregion

		#region DI_IsLimitedQuantity

		public override ZBool DI_IsLimitedQuantity
		{
			get => DI_QuantityClassification == UNDGDataItemLookups.UNDGDataItemQuantityClasses.Code.LIM;
			set
			{
				base.DI_IsLimitedQuantity = value;
				QuantityClassificationHelper.SetQuantityClassification(this, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDI_DGWeight();
					Validation.ValidateDI_DGVolume();
				}
			}
		}

		#endregion

		#region DI_IsHighwayRouteControlledQuantity

		protected virtual bool DI_IsHighwayRouteControlledQuantity_ReadOnly => true;

		#endregion

		#region DI_RadionuclideElement

		[List("Lookups.RadionuclideElementList")]
		public override ZString DI_RadionuclideElement
		{
			get { return base.DI_RadionuclideElement; }
			set
			{
				if (base.DI_RadionuclideElement != value)
				{
					base.DI_RadionuclideElement = value;
				}
			}
		}

		public virtual bool DI_RadionuclideElement_ReadOnly => true;

		[List("Lookups.RadionuclideElementSuffixList")]
		public override ZString DI_RadionuclideElementSuffix
		{
			get { return base.DI_RadionuclideElementSuffix; }
			set
			{
				if (base.DI_RadionuclideElementSuffix != value)
				{
					base.DI_RadionuclideElementSuffix = value;
				}
			}
		}

		public virtual bool DI_RadionuclideElementSuffix_ReadOnly => true;

		public virtual bool DI_RadioactiveMaximumActivity_ReadOnly => true;

		[List("Lookups.RadioactiveMaximumActivityUnitList")]
		public override ZString DI_RadioactiveMaximumActivityUnit
		{
			get { return base.DI_RadioactiveMaximumActivityUnit; }
			set
			{
				if (base.DI_RadioactiveMaximumActivityUnit != value)
				{
					base.DI_RadioactiveMaximumActivityUnit = value;
				}
			}
		}

		public virtual bool DI_RadioactiveMaximumActivityUnit_ReadOnly => true;

		#endregion

		#region DI_MaterialFormDescription

		protected virtual bool DI_MaterialFormDescription_ReadOnly => true;

		#endregion

		#region DI_IsFissileExcepted

		protected virtual bool DI_IsFissileExcepted_ReadOnly => true;

		#endregion

		#region DI_IsExclusiveUse

		protected virtual bool DI_IsExclusiveUse_ReadOnly => true;

		#endregion

		#region Empty

		internal bool IsEmptyItem
		{
			get { return Substance == null && DI_IMOClass.IsEmpty && DI_OC_DGContact.IsEmpty && DI_DGFlashPoint.IsEmpty && DI_UnitOfVolume.IsEmpty && DI_UnitOfWeight.IsEmpty && DI_DGWeight.IsEmpty && DI_DGVolume.IsEmpty; }
		}

		#endregion

		#region DI_TechnicalName

		public bool DI_TechnicalName_ReadOnly
		{
			get { return Substance == null; }
		}

		#endregion

		#region DI_MPMarinePollutant

		[BusinessObjectTestExclude()]
		[List("Lookups.MarinePollutantList")]
		public override ZString DI_MPMarinePollutant
		{
			get
			{
				if (DI_MPMarinePollutant_ReadOnly)
				{
					return Substance != null ? Substance.DG_MP : ZString.Empty;
				}
				else
				{
					return base.DI_MPMarinePollutant;
				}
			}
			set { base.DI_MPMarinePollutant = value; }
		}

		public bool DI_MPMarinePollutant_ReadOnly
		{
			get { return Substance == null; }
		}

		#endregion

		#region Limited Quantity

		void CalculateLimitedQuantity()
		{
			if (!isImportingData)
			{
				DI_IsLimitedQuantity = false;

				if (Substance != null && Substance.DG_LQMaxAmt > 0)
				{
					if (Core.Constants.Weight.ContainsCode(Substance.DG_LQMaxAmtUQ) && Core.Constants.Weight.ContainsCode(DI_UnitOfWeight) && DI_DGWeight > 0m)
					{
						decimal limitedQty = Core.Constants.Weight.Convert(Substance.DG_LQMaxAmt, Substance.DG_LQMaxAmtUQ, DI_UnitOfWeight);
						DI_IsLimitedQuantity = DI_DGWeight <= limitedQty;
					}
					else if (Core.Constants.Volume.ContainsCode(Substance.DG_LQMaxAmtUQ) && Core.Constants.Volume.ContainsCode(DI_UnitOfVolume) && DI_DGVolume > 0m)
					{
						decimal limitedQty = Core.Constants.Volume.Convert(Substance.DG_LQMaxAmt, Substance.DG_LQMaxAmtUQ, DI_UnitOfVolume);
						DI_IsLimitedQuantity = DI_DGVolume <= limitedQty;
					}
				}
			}
		}

		#endregion

		#region DI_HasOverpack

		public override ZBool DI_HasOverpack
		{
			get => base.DI_HasOverpack;
			set
			{
				if (value != base.DI_HasOverpack)
				{
					base.DI_HasOverpack = value;

					if (!value)
					{
						DI_OverpackID = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region DI_OverpackID

		public bool DI_OverpackID_ReadOnly => !DI_HasOverpack;

		#endregion

		#region DI_PackingInstructionSection

		[List("Lookups.PackingInstructionSectionList")]
		public override ZString DI_PackingInstructionSection
		{
			get => base.DI_PackingInstructionSection;
			set => base.DI_PackingInstructionSection = value;
		}

		#endregion

		#region DI_ApprovalCertificateType

		[List("Lookups.ApprovalCertificateTypeList")]
		public override ZString DI_ApprovalCertificateType
		{
			get => base.DI_ApprovalCertificateType;
			set
			{
				if (base.DI_ApprovalCertificateType != value)
				{
					base.DI_ApprovalCertificateType = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_ApprovalCertificateIDMark();
					}
				}
			}
		}

		#endregion

		#region DI_ApprovalCertificateIDMark

		public override ZString DI_ApprovalCertificateIDMark
		{
			get => base.DI_ApprovalCertificateIDMark;
			set
			{
				if (base.DI_ApprovalCertificateIDMark != value)
				{
					base.DI_ApprovalCertificateIDMark = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_ApprovalCertificateType();
					}
				}
			}
		}

		#endregion

		#region Approval Certificate ReadOnly

		public bool DI_ApprovalCertificateType_ReadOnly => ApprovalCertificate_ReadOnly;

		public bool DI_ApprovalCertificateIDMark_ReadOnly => ApprovalCertificate_ReadOnly;

		protected virtual bool ApprovalCertificate_ReadOnly => true;

		#endregion

		#region N.O.S

		protected virtual bool DI_IsNotOtherwiseSpecified_ReadOnly => true;

		#endregion

		#region Radioactive Label Category

		[List("Lookups.RadioactiveLabelCategoryList")]
		public override ZString DI_RadioactiveLabelCategory
		{
			get => base.DI_RadioactiveLabelCategory;
			set
			{
				if (base.DI_RadioactiveLabelCategory != value)
				{
					base.DI_RadioactiveLabelCategory = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateDI_RadioactiveLabelCategory();
					}
				}
			}
		}

		protected virtual bool DI_RadioactiveLabelCategory_ReadOnly => false;

		#endregion

		#region Radioactive Transport Index

		protected virtual bool DI_RadioactiveTransportIndex_ReadOnly => true;

		#endregion

		#region Check Forbidden For Aircraft

		public virtual bool IsForbiddenForPassengerAircraft()
		{
			return Substance != null
				&& Substance.DG_LQ2OrPaxMaxAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
		}

		public virtual bool IsForbiddenForCargoAircraft()
		{
			return Substance != null
				&& Substance.DG_CargoPackAmtType == UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode;
		}

		#endregion

		#endregion

		#region IUNDGSubstancePivotParent

		[ChildEditable(true)]
		public UNDGSubstancePivotCollection UNDGSubstancePivotCollection
		{
			get
			{
				if (undgSubstancePivotCollection == null)
				{
					undgSubstancePivotCollection = new UNDGSubstancePivotCollection(Factory, this, UNDGDataItemSchema.Constants.Prefix);
					RegisterEditableChildObject(undgSubstancePivotCollection);
				}

				return undgSubstancePivotCollection;
			}
		}

		UNDGSubstancePivotCollection undgSubstancePivotCollection;

		#endregion

		#region ISupportDataImporting

		bool ISupportDataImporting.IsImportingData
		{
			get { return isImportingData; }
			set { isImportingData = value; }
		}
		bool isImportingData;

		public IDisposable TempSetIsImportingData()
		{
			isImportingData = true;
			return new DisposableAction(() => isImportingData = false);
		}

		#endregion

		#region LinkedDGSubstanceInfoCollection

		public LinkedDGSubstanceInfoCollection LinkedDGSubstanceInfoCollection
		{
			get
			{
				if (linkedDGSubstanceInfoCollection == null)
				{
					linkedDGSubstanceInfoCollection = new LinkedDGSubstanceInfoCollection(Factory);
					LoadLinkedDGSubstanceInfos();
				}

				return linkedDGSubstanceInfoCollection;
			}
		}

		LinkedDGSubstanceInfoCollection linkedDGSubstanceInfoCollection;

		void LoadLinkedDGSubstanceInfos()
		{
			if (linkedDGSubstanceInfoCollection == null)
			{
				return;
			}

			linkedDGSubstanceInfoCollection.RemoveAndDeleteAll();

			if (Substance != null)
			{
				foreach (var subs in UNDGSubstanceLoader.LoadSubstances(Factory, Substance.DG_UNNO))
				{
					linkedDGSubstanceInfoCollection.Add(new LinkedDGSubstanceInfo(this, subs));
				}
			}

			LinkedDGSubstanceInfoCollection.RefreshBinding();
		}

		#endregion

		#region SubstanceOrClassUpdated

		internal event EventHandler SubstanceOrClassUpdated;

		void OnSubstanceOrClassUpdated()
		{
			if (SubstanceOrClassUpdated != null && !isUpdatingSubstance)
			{
				try
				{
					isUpdatingSubstance = true;
					SubstanceOrClassUpdated(this, EventArgs.Empty);
				}
				finally
				{
					isUpdatingSubstance = false;
				}
			}
		}
		bool isUpdatingSubstance;

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsAutoAddedItem); }
		}

		public override void Delete()
		{
			if (!IsDeleting && !IsAutoAddedItem && !IsDeleted && IsInDatabase)
			{
				LogDangerousGoodsChanges(true);
			}
			UNDGSubstancePivotCollection.DeleteAll();
			base.Delete();
		}

		protected override void OnFactorySaving()
		{
			if (IsSavedByFactory && !IsParentValidToSave)
			{
				isAutoAddedItem = true;
			}
			base.OnFactorySaving();
		}

		public bool IsParentValidToSave => (!DI_ParentTableCode.IsEmpty && DI_ParentID.IsValid);

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var undgDataItem = (UNDGDataItem)base.CloneInternal(args);
			undgDataItem.UNDGSubstancePivotCollection.UpdateDefaultPivot(undgDataItem.UNDGSubstance);
			return undgDataItem;
		}

		#endregion

		#region TryGetPSNWithAdditionalTextIfSupportForNOS

		public virtual ZString TryGetPSNWithAdditionalTextIfSupportForNOS()
		{
			return Substance?.DG_PSN ?? ZString.Empty;
		}

		#endregion

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			DI_ApprovalCertificateType = ZString.Empty;
			DI_ParentID = new ZGuid("12345678-90AB-CDEF-FEDC-BA0987654321");
			DI_ParentTableCode = JobPackLinesSchema.Constants.Prefix;
			DI_PackingInstructionSection = ZString.Empty;
			DI_RadioactiveLabelCategory = ZString.Empty;
			DI_RadioactiveMaximumActivityUnit = ZString.Empty;
		}

#endif
		#endregion

		#region Universal Copy

		void OnUniversalCopyFinish()
		{
			UNDGSubstancePivotCollection.UpdateDefaultPivot(UNDGSubstance);
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			LogDangerousGoodsChanges();
		}

		void LogDangerousGoodsChanges(bool fromDelete = false)
		{
			var parentBOWithLogs = GetParentWithLogs();
			if (parentBOWithLogs != null && (!IsInDatabase || fromDelete || DI_DGInfo.HasChanges))
			{
				parentBOWithLogs.Logs.CreateOrRecreateEventLog(Events.DangerousGoodsChanged, EstimateActual.Actual, ZDateTimeOffset.Now);
			}

			IStmALogParent GetParentWithLogs()
			{
				if (DI_ParentID.IsEmpty)
				{
					return null;
				}

				var parentBO = Factory
					.GetBizOsForPK(DI_ParentID.ToGuid())
					.FirstOrDefault(parent => !parent.IsDeleted && DI_ParentTableCode == parent.TablePrefix)
					?? Factory.Load(DI_ParentTableCode, DI_ParentID);

				return parentBO as IStmALogParent;
			}
		}

		#endregion

#if DEBUG
		public int SubstanceUpdatedCounter
		{
			get
			{
				return SubstanceOrClassUpdated.GetInvocationList().Length;
			}
		}
#endif
	}
}
