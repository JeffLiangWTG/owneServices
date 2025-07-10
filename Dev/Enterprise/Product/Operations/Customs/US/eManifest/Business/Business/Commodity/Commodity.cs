using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.US.eManifest.Business
{
	[UserDefinedValues]
	public class Commodity : AutoCusInBondCargoDesc
		, IUNDGDataItemProvider
		, ICusCodeDataTypeSupporter
		, Integration.Customs.US.eManifest.ICusInBondCargoDesc
		, IUniversalCopySelectivelySupportable
	{
		public Commodity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Shipment

		public Shipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.Load<Shipment>(BY_ParentID);
				}
				return shipment;
			}
			set
			{
				shipment = value;
			}
		}
		Shipment shipment;

		#endregion

		#region Schema

		public new class Schema : CusInBondCargoDesc.Schema
		{
			public const string BY_C4Codes = "BY_C4Codes";
			public const string BY_HarmonizedNumbers = "BY_HarmonizedNumbers";
			public const string BY_HazardousGoodsIdentifier = "BY_HazardousGoodsIdentifier";
			public const string BY_HazardousGoodsContact = "BY_HazardousGoodsContact";
			public const string BY_VehicleIdentificationNumbers = "BY_VehicleIdentificationNumbers";
		}

		#endregion

		#region Properties

		#region BY_C4Codes

		[BusinessObjectMaxLengthTestExclude]
		public ZString BY_C4Codes
		{
			get { return (c4CodesCached ?? (c4CodesCached = new CachedProperty<ZString>(Factory, () => C4Codes.DataAsString()))).Value; }
			set
			{
				var oldValue = BY_C4Codes;

				C4Codes.PopulateDataFromString(value);
				BY_C4CodesInfo.RefreshBinding();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_C4Codes, Shipment.Schema.FirstCommodityC4Codes);
			}
		}

		public ZPropertyInfo BY_C4CodesInfo
		{
			get { return GetZPropertyInfo(Schema.BY_C4Codes); }
		}

		protected internal bool BY_C4Codes_ReadOnly
		{
			get { return (Shipment?.B0_ShipmentType ?? ZString.Empty) != ShipmentTypes.Codes.BRASS; }
		}

		CachedProperty<ZString> c4CodesCached;

		#endregion

		#region BY_BJ_Equipment

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.Equipment))]
		[RelatedBusinessObject("Equipment")]
		public override ZGuid BY_BJ_Equipment
		{
			get { return base.BY_BJ_Equipment; }
			set
			{
				var oldValue = BY_BJ_Equipment;
				base.BY_BJ_Equipment = value;

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_BJ_Equipment, Shipment.Schema.FirstCommodityEquipment);
			}
		}

		public Equipment Equipment
		{
			get { return Factory.Load<Equipment>(BY_BJ_Equipment); }
		}

		#endregion

		#region BY_HarmonizedNumbers

		[BusinessObjectMaxLengthTestExclude]
		public ZString BY_HarmonizedNumbers
		{
			get
			{
				return (harmonizedNumbersCached
						?? (harmonizedNumbersCached =
							new CachedProperty<ZString>(Factory, () => HarmonizedNumbers.DataAsString(c => c.CY_TariffFormatted)))).Value;
			}
			set
			{
				var oldValue = BY_HarmonizedNumbers;
				HarmonizedNumbers.PopulateDataFromString(value, (c, v) => c.CY_TariffFormatted = v);
				BY_HarmonizedNumbersInfo.RefreshBinding();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_HarmonizedNumbers, Shipment.Schema.FirstCommodityHarmonizedNumbers);
			}
		}

		public ZPropertyInfo BY_HarmonizedNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.BY_HarmonizedNumbers); }
		}

		CachedProperty<ZString> harmonizedNumbersCached;

		#endregion

		#region BY_HazardousGoodsIdentifier

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.UNDGSubs))]
		[ReadOnlyMember(nameof(HazardousGoodsReadOnly))]
		public ZGuid BY_HazardousGoodsIdentifier
		{
			get
			{
				return (hazardousGoodsIdentifierCached
						?? (hazardousGoodsIdentifierCached =
							new CachedProperty<ZGuid>(Factory, () => GetUNDGValue(undg => undg.DI_DG)))).Value;
			}
			set
			{
				var oldValue = BY_HazardousGoodsIdentifier;

				SetUNDGValue(undg => undg.DI_DG = value);
				BY_HazardousGoodsIdentifierInfo.RefreshBinding();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_HazardousGoodsIdentifier, Shipment.Schema.FirstCommodityHazardousGoodsIdentifier);
			}
		}

		public ZPropertyInfo BY_HazardousGoodsIdentifierInfo
		{
			get { return GetZPropertyInfo(Schema.BY_HazardousGoodsIdentifier); }
		}

		T GetUNDGValue<T>(Func<UNDGDataItem, T> valueGetter)
		{
			return UNDGs.Count > 1 ? default(T) : UNDGs.Select(valueGetter).FirstOrDefault();
		}

		void SetUNDGValue(Action<UNDGDataItem> valueSetter)
		{
			if (UNDGs.Count <= 1)
			{
				var undg = (UNDGs.FirstOrDefault() ?? UNDGs.AddNew());
				valueSetter(undg);
				if (undg.DI_DG.IsEmpty && undg.DI_OC_DGContact.IsEmpty)
				{
					undg.Delete();
				}
			}
		}

		protected internal bool HazardousGoodsReadOnly
		{
			get { return UNDGs.Count > 1; }
		}

		CachedProperty<ZGuid> hazardousGoodsIdentifierCached;

		#endregion

		#region BY_HazardousGoodsContact

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.Contacts))]
		[ReadOnlyMember(nameof(HazardousGoodsReadOnly))]
		public ZGuid BY_HazardousGoodsContact
		{
			get
			{
				return (hazardousGoodsContactCached
						?? (hazardousGoodsContactCached =
							new CachedProperty<ZGuid>(Factory, () => GetUNDGValue(undg => undg.DI_OC_DGContact)))).Value;
			}
			set
			{
				var oldValue = BY_HazardousGoodsContact;

				SetUNDGValue(undg => undg.DI_OC_DGContact = value);
				BY_HazardousGoodsContactInfo.RefreshBinding();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_HazardousGoodsContact, Shipment.Schema.FirstCommodityHazardousGoodsContact);
			}
		}

		public ZPropertyInfo BY_HazardousGoodsContactInfo
		{
			get { return GetZPropertyInfo(Schema.BY_HazardousGoodsContact); }
		}

		CachedProperty<ZGuid> hazardousGoodsContactCached;

		#endregion

		#region BY_HazardousGoodsContactPhone

		public ZString BY_HazardousGoodsContactPhone
		{
			get
			{
				if (hazardousGoodsContactPhoneCached == null)
				{
					hazardousGoodsContactPhoneCached = new CachedProperty<ZString>(
						Factory,
						() =>
						{
							var contact = GetUNDGValue(undg => undg.DGContact);
							return contact == null ? ZString.Empty : contact.OC_Phone;
						});
				}
				return hazardousGoodsContactPhoneCached.Value;
			}
		}

		CachedProperty<ZString> hazardousGoodsContactPhoneCached;

		#endregion

		#region BY_ManifestUnitCode

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.QuantityUnits))]
		public override ZString BY_ManifestUnitCode
		{
			get { return base.BY_ManifestUnitCode; }
			set
			{
				var oldValue = BY_ManifestUnitCode;
				base.BY_ManifestUnitCode = value;

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_ManifestUnitCode, Shipment.Schema.FirstCommodityManifestUnitCode);
			}
		}

		#endregion

		#region BY_MonetaryValue

		[DecimalPlaces(0)]
		public override ZDecimal BY_MonetaryValue
		{
			get { return base.BY_MonetaryValue; }
			set
			{
				var oldValue = BY_MonetaryValue;
				base.BY_MonetaryValue = value;
				if (oldValue != BY_MonetaryValue && !IsCopying)
				{
					var shipment = Shipment;
					if (shipment != null)
					{
						if (shipment.B0_GoodsValue.IsEmpty && shipment.Commodities.Count == 1)
						{
							shipment.B0_GoodsValue = BY_MonetaryValue;
						}
						shipment.MarkAsNeedingValidation();
					}
				}

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_MonetaryValue, Shipment.Schema.FirstCommodityMonetaryValue);
			}
		}

		[ReadOnly(true)]

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.Currencies))]
		public ZString BY_MonetaryValueCurrency
		{
			get { return Constants.CurrencyCodes.UnitedStates; }
		}

		#endregion

		#region BY_PieceCount

		public override ZInt BY_PieceCount
		{
			get { return base.BY_PieceCount; }
			set
			{
				var oldValue = BY_PieceCount;
				var hasChanges = oldValue != value;
				base.BY_PieceCount = value;
				if (hasChanges && !IsCopying && Shipment is Shipment shipment)
				{
					shipment.MarkAsNeedingValidation();
					if (BY_ManifestUnitCode.IsEmpty)
					{
						BY_ManifestUnitCode = shipment.B0_ManifestUQ.Left(CusInBondCargoDesc.Schema.BY_ManifestUnitCodeMaxLength);
					}
				}

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_PieceCount, Shipment.Schema.FirstCommodityPieceCount);
			}
		}

		#endregion

		#region BY_RN_NKCountryOfOrigin

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.Countries))]
		[ReadOnlyMember(nameof(BY_RN_NKCountryOfOrigin_ReadOnly))]
		public override ZString BY_RN_NKCountryOfOrigin
		{
			get { return base.BY_RN_NKCountryOfOrigin; }
			set
			{
				var oldValue = BY_RN_NKCountryOfOrigin;
				base.BY_RN_NKCountryOfOrigin = value;

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_RN_NKCountryOfOrigin, Shipment.Schema.FirstCommodityCountryOfOrigin);
			}
		}

		protected internal bool BY_RN_NKCountryOfOrigin_ReadOnly
		{
			get { return (Shipment?.B0_ShipmentType ?? ZString.Empty) != ShipmentTypes.Codes.LowValue; }
		}

		#endregion

		#region BY_VehicleIdentificationNumbers

		[BusinessObjectMaxLengthTestExclude]
		public ZString BY_VehicleIdentificationNumbers
		{
			get { return (vinsChached ?? (vinsChached = new CachedProperty<ZString>(Factory, () => VehicleIdentificationNumbers.DataAsString()))).Value; }
			set
			{
				var oldValue = BY_VehicleIdentificationNumbers;

				VehicleIdentificationNumbers.PopulateDataFromString(value);
				BY_VehicleIdentificationNumbersInfo.RefreshBinding();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_VehicleIdentificationNumbers, Shipment.Schema.FirstCommodityVehicleIdentificationNumbers);
			}
		}

		public ZPropertyInfo BY_VehicleIdentificationNumbersInfo
		{
			get { return GetZPropertyInfo(Schema.BY_VehicleIdentificationNumbers); }
		}

		CachedProperty<ZString> vinsChached;

		#endregion

		#region BY_GrossWeight

		[MeasureUnit(Schema.BY_GrossWeightUnit, MeasureUnitType.Weight)]
		public override ZDecimal BY_GrossWeight
		{
			get { return base.BY_GrossWeight; }
			set
			{
				var oldValue = BY_GrossWeight;
				var hasChanges = oldValue != value;
				base.BY_GrossWeight = value;
				if (hasChanges && !IsCopying && Shipment is Shipment shipment)
				{
					shipment.MarkAsNeedingValidation();
					if (BY_GrossWeightUnit.IsEmpty)
					{
						BY_GrossWeightUnit = shipment.B0_WeightUQ;
					}
				}

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_GrossWeight, Shipment.Schema.FirstCommodityWeight);
			}
		}

		#endregion

		#region BY_GrossWeightUnit

		[List(nameof(Lookups) + "." + nameof(CommodityLookups.WeightUnits))]
		public override ZString BY_GrossWeightUnit
		{
			get { return base.BY_GrossWeightUnit; }
			set
			{
				var oldValue = BY_GrossWeightUnit;
				base.BY_GrossWeightUnit = value;
				Shipment?.MarkAsNeedingValidation();

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_GrossWeightUnit, Shipment.Schema.FirstCommodityWeightUnit);
			}
		}

		#endregion

		#region BY_Description

		public override ZString BY_Description
		{
			get => base.BY_Description;
			set
			{
				var oldValue = BY_Description;
				base.BY_Description = value;

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_Description, Shipment.Schema.FirstCommodityDescription);
			}
		}

		#endregion

		#region BY_MarksAndNumbers

		public override ZString BY_MarksAndNumbers
		{
			get => base.BY_MarksAndNumbers;
			set
			{
				var oldValue = BY_MarksAndNumbers;
				base.BY_MarksAndNumbers = value;

				RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(oldValue, () => BY_MarksAndNumbers, Shipment.Schema.FirstCommodityMarksAndNumbers);
			}
		}

		#endregion

		#endregion

		#region Collections

		#region C4 Codes

		[ChildEditable(true)]
		public C4CodeCollection C4Codes
		{
			get
			{
				if (c4Codes == null)
				{
					c4Codes = new C4CodeCollection(this);
					c4Codes.Load();
					RegisterEditableChildObject(c4Codes);
				}
				return c4Codes;
			}
		}

		C4CodeCollection c4Codes;

		#endregion

		#region Harmonized Numbers

		[ChildEditable(true)]
		public HarmonizedNumberCollection HarmonizedNumbers
		{
			get
			{
				if (harmonizedNumbers == null)
				{
					harmonizedNumbers = new HarmonizedNumberCollection(this);
					harmonizedNumbers.Load();
					RegisterEditableChildObject(harmonizedNumbers);
				}
				return harmonizedNumbers;
			}
		}

		HarmonizedNumberCollection harmonizedNumbers;

		#endregion

		#region UNDGs

		[ChildEditable]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (undgs == null)
				{
					undgs = new UNDGDataItemCollection(this);
					undgs.CountChanged += UNDGs_CountChanged;
					RegisterEditableChildObject(undgs);
				}
				return undgs;
			}
		}

		void UNDGs_CountChanged(object sender, EventArgs e)
		{
			UnHookValidationFromRemovedItem();
			HookValidationOnAddedItem();
			Validation.ValidateBY_HazardousGoodsIdentifier();
			Validation.ValidateBY_HazardousGoodsContact();
		}

		void UnHookValidationFromRemovedItem()
		{
			foreach (var validation in Validations.Where(v => !UNDGs.Any(undg => undg.PK == v.Key)).ToArray())
			{
				validation.Value.UnHookValidation();
				Validations.Remove(validation.Key);
			}
		}

		void HookValidationOnAddedItem()
		{
			foreach (var undg in UNDGs.Where(u => !Validations.ContainsKey(u.PK)))
			{
				var validation = new DGContactValidation(this, undg);
				validation.HookValidation();
				Validations[undg.PK] = validation;
			}
		}

		Dictionary<ZGuid, DGContactValidation> Validations
		{
			get { return validations ?? (validations = new Dictionary<ZGuid, DGContactValidation>()); }
		}

		Dictionary<ZGuid, DGContactValidation> validations;
		UNDGDataItemCollection undgs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		#endregion

		#region Vehicle Identification Numbers

		[ChildEditable(true)]
		public VehicleIdentificationNumberCollection VehicleIdentificationNumbers
		{
			get
			{
				if (vehicleIdentificationNumbers == null)
				{
					vehicleIdentificationNumbers = new VehicleIdentificationNumberCollection(this);
					vehicleIdentificationNumbers.Load();
					RegisterEditableChildObject(vehicleIdentificationNumbers);
				}
				return vehicleIdentificationNumbers;
			}
		}

		VehicleIdentificationNumberCollection vehicleIdentificationNumbers;

		#endregion

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.VehicleIdentificationNumber, typeof(VehicleIdentificationNumber));
			result.Add(CusCodeDataTypeList.Codes.HarmonizedNumber, typeof(HarmonizedNumber));
			result.Add(CusCodeDataTypeList.Codes.C4Code, typeof(C4Code));
			return result;
		}

		#endregion

		#region IUniversalCopySelectivelySupportable Members

		bool IUniversalCopySelectivelySupportable.SupportsUniversalCopy => false;

		string IUniversalCopySelectivelySupportable.ReasonForNotSupportingUniversalCopy => string.Empty;

		#endregion

		#region Overrides

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (BY_RN_NKCountryOfOrigin_ReadOnly)
			{
				BY_RN_NKCountryOfOriginInfo.ClearValue();
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				C4Codes.RemoveAndDeleteAll();
				HarmonizedNumbers.RemoveAndDeleteAll();
				VehicleIdentificationNumbers.RemoveAndDeleteAll();
				UNDGs.DeleteAll();
			}
			base.Delete();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override bool SupportsNotes => false;

		#region Validation

		public new CommodityValidation Validation
		{
			get { return (CommodityValidation)base.Validation; }
		}

		protected override CusInBondCargoDescValidation GetNewValidation()
		{
			return new CommodityValidation(this);
		}

		internal bool ValidateAllHasBeenRun { get; set; }

		#endregion

		#region Lookups

		public new CommodityLookups Lookups
		{
			get { return (CommodityLookups)base.Lookups; }
		}

		protected override CusInBondCargoDescLookups GetNewLookups()
		{
			return new CommodityLookups(this);
		}

		#endregion

		#endregion

		void RefreshBindingOfShipmentFirstCommodityPropertyIfNeeded(IZType oldValue, Func<IZType> getNewValue, string propertyName)
		{
			if (!IsCopying)
			{
				var newValue = getNewValue();
				if (oldValue != newValue && Shipment is Shipment shipment)
				{
					var item = shipment.FirstCommodity;
					if (object.ReferenceEquals(item, this))
					{
						var info = shipment.ZPropertyInfoHash.GetPropertySafe(propertyName);
						info?.RefreshBinding(oldValue);
					}
				}
			}
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CommodityFetchStrategy(this);

		#endregion
	}
}
