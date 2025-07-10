using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList.ChargeTypeOtherList;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItem : ASYCUDA.Business.AsycudaPackedItem,
		ILinePriceCalculationFieldSettingSupporter,
		IReconcileCandidate,
		ITariffFormatProvider
	{
		public AsycudaPackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaPackedItem.Schema
		{
			public const string API_Preference = "API_Preference";
			public const int API_PreferenceMaxLength = 3;
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackedItemValidation Validation => (AsycudaPackedItemValidation)base.Validation;

		protected override ManifestBase.AsycudaPackedItemValidation GetNewValidation() => new AsycudaPackedItemValidation(this);

		public new AsycudaPackedItemLookups Lookups => (AsycudaPackedItemLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackedItemLookups GetNewLookups() => new AsycudaPackedItemLookups(this);

		[ResourceStringData("45E08AF8-C241-4124-A625-7A5F0FE06298", Caption = "Sequence", ShortCaption = "Seq.")]
		public override ZInt API_LineNo
		{
			get => base.API_LineNo;
			set
			{
				if (value > 0)
				{
					base.API_LineNo = value;
				}
			}
		}

		[MaxLength(512)]
		[ResourceStringData("40B840A3-E136-43ED-9D90-6D1E524E017F", Caption = "Goods Description", ShortCaption = "Desc.")]
		public override ZString API_GoodsDescription { get => base.API_GoodsDescription; set => base.API_GoodsDescription = value; }

		[ResourceStringData("2AB7882A-56CF-4CBB-A54B-6599D0692D5C", Caption = "Model")]
		public override ZString API_Model { get => base.API_Model; set => base.API_Model = value; }

		[ResourceStringData("3388F79C-976E-49FA-9D3A-E0C47E3A7700", Caption = "Brand Name", ShortCaption = "Brand")]
		public override ZString API_Brand { get => base.API_Brand; set => base.API_Brand = value; }

		[ResourceStringData("5A698826-C701-452B-9CA2-2626F20D64F5", Caption = "Specification", FullDescription = "Identifies the element or component element in a commodity including description, percentage, quantity, name, active ingredient and yield amount.")]
		public override ZString API_Remarks { get => base.API_Remarks; set => base.API_Remarks = value; }

		[MaxLength(15)]
		[ResourceStringData("575853A0-A536-4EF7-8CD2-C4E4A4FD6F43", Caption = "Tariff Code", ShortCaption = "Tariff")]
		public override ZString API_FormattedTariff
		{
			get => base.API_FormattedTariff;
			set
			{
				var oldValue = API_FormattedTariff;
				base.API_FormattedTariff = value;
				if (oldValue != API_FormattedTariff && !IsCopying && Header.RequiresDefaultPackitemTaxes)
				{
					ClearAndDefaultPackedItemTax();
				}
			}
		}

		public ZString ModeOfStatisticsOrDutyTreatment => IsExport ? TW.Business.Constants.ProcedureCodes._02 : TW.Business.Constants.ProcedureCodes._31;

		public ZPropertyInfo ModeOfStatisticsOrDutyTreatmentInfo => GetZPropertyInfo(nameof(ModeOfStatisticsOrDutyTreatment));

		[DecimalPlaces(0)]
		[ResourceStringData("D908342B-AFD8-417F-9611-277C22A00801", Caption = "Customs Value")]
		public override ZDecimal API_CustomsValue
		{
			get => base.API_CustomsValue;
			set
			{
				var oldValue = API_CustomsValue;
				base.API_CustomsValue = value;
				if (oldValue != API_CustomsValue && !IsCopying)
				{
					var bill = Bill;
					if (Header.RequiresDefaultPackitemTaxes)
					{
						var calculatedAsycudaTaxes = AsycudaTaxes.Cast<AsycudaPackedItemTax>().Where(c => c.AET_RateOverrideReasonCode.IsEmpty && !c.IsVAT).ToList();
						calculatedAsycudaTaxes.ForEach(tax =>
						{
							tax.CalculatedAET_BaseValue();
							tax.SetRateAndMethodOfCalculation();
						});
						ReCalculateVATPackedItemTax();
						ReCalculateTPFPackedItemTax(bill?.RequiresCalculateTPF ?? false);
					}
					if (!IsValidationSuspended)
					{
						bill?.Validation.ValidateABL_CustomsValue();
						AsycudaTaxes.MarkAsNeedingValidation();
					}
				}
			}
		}

		[DecimalPrecision(11)]
		[DecimalPlaces(5)]
		[ResourceStringData("3D95274E-E811-4E55-B74A-F97887F06627", Caption = "Quantity", ShortCaption = "Qty")]
		public override ZDecimal API_CustomsQty
		{
			get => base.API_CustomsQty;
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.Quantity))
				{
					var oldValue = API_CustomsQty;
					base.API_CustomsQty = value;
					if (!IsCopying && oldValue != API_CustomsQty)
					{
						Bill?.CalculateCustomsValue();
						UpdateLinePriceOrUnitPriceIfChangedFromQuantityChanges();
						CalculatedPackedItemTaxBaseValue();
					}
					API_CustomsQtyInfo.RefreshBinding(oldValue);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.CustomsUQList))]
		[ResourceStringData("4B5F8DBC-0B1F-4B3C-A796-3C7324F2A6E1", Caption = "Quantity Unit", ShortCaption = "UQ")]
		public override ZString API_CustomsUQ
		{
			get => base.API_CustomsUQ;
			set
			{
				var oldValue = API_CustomsUQ;
				base.API_CustomsUQ = value;
				if (oldValue != API_CustomsUQ && !IsCopying)
				{
					CalculatedPackedItemTaxBaseValue();
				}
			}
		}

		[DecimalPrecision(13)]
		[DecimalPlaces(6)]
		[ResourceStringData("84681F16-637E-45DA-9DDC-CB238F4B60E4", Caption = "Unit Price", ShortCaption = "Unit Price")]
		public override ZDecimal API_UnitPrice
		{
			get => base.API_UnitPrice;
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.UnitPrice))
				{
					var oldValue = API_UnitPrice;
					base.API_UnitPrice = value;
					if (!IsCopying && oldValue != API_UnitPrice)
					{
						UpdateLinePriceOrQuantityIfChangedFromUnitPriceChanges();
						Bill?.CalculateCustomsValue();
					}
					API_UnitPriceInfo.RefreshBinding(oldValue);
				}
			}
		}

		[DecimalPrecision(17)]
		[DecimalPlaces(2)]
		[ResourceStringData("8087F853-A0B6-4D3C-BACC-C90F5AA135E4", Caption = "Goods Value", ShortCaption = "Goods Value")]
		public override ZDecimal API_GoodsValue
		{
			get => base.API_GoodsValue;
			set
			{
				using (GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.LinePrice))
				{
					var oldValue = API_GoodsValue;
					base.API_GoodsValue = value;
					if (!IsCopying && oldValue != API_GoodsValue)
					{
						UpdateUnitPriceOrQuantityIfChangedFromUnitPriceChanges();
						Bill?.CalculateCustomsValue();
					}
					API_GoodsValueInfo.RefreshBinding(oldValue);
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("C2EB18AD-94B8-42D0-B2F5-403A7AB2BF23", Caption = "Goods Value Currency")]
		public override ZString API_RX_NKGoodsValueCurrency { get => base.API_RX_NKGoodsValueCurrency; set => base.API_RX_NKGoodsValueCurrency = value; }

		[DecimalPrecision(11)]
		[DecimalPlaces(3)]
		[ResourceStringData("B4BDBF65-3753-4EA9-9FBD-D91F66553399", Caption = "Net Weight", ShortCaption = "NW")]
		public override ZDecimal API_NetWeight
		{
			get => base.API_NetWeight;
			set
			{
				var oldValue = API_NetWeight;
				base.API_NetWeight = value < 0 ? ZDecimal.Zero : value;
				if (oldValue != API_NetWeight && !IsCopying)
				{
					CalculatedPackedItemTaxBaseValue();
				}
			}
		}

		public ZDecimal PackedItemNetWeightInKG => new ZWeight(API_NetWeight, API_NetWeightUQ).InKilogramsSafe;

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.NetWeightUQList))]
		[ResourceStringData("313F4F00-3D04-4E11-AEEA-886BD845960D", Caption = "Net Weight Unit", ShortCaption = "UQ")]
		public override ZString API_NetWeightUQ
		{
			get => base.API_NetWeightUQ;
			set
			{
				var oldValue = API_NetWeightUQ;
				base.API_NetWeightUQ = value;
				if (oldValue != API_NetWeightUQ && !IsCopying)
				{
					CalculatedPackedItemTaxBaseValue();
				}
			}
		}

		[ReadOnlyMember(nameof(API_CustomsQty2_ReadOnly))]
		[DecimalPrecision(9)]
		[DecimalPlaces(4)]
		[ResourceStringData("A9BC7615-133A-443B-9F4E-D37745AA959B", Caption = "Statistical Quantity", ShortCaption = "Stats. Qty")]
		public override ZDecimal API_CustomsQty2
		{
			get => base.API_CustomsQty2;
			set
			{
				var oldValue = API_CustomsQty2;
				base.API_CustomsQty2 = value;
				if (oldValue != API_CustomsQty2 && !IsCopying)
				{
					CalculatedPackedItemTaxBaseValue();
				}
			}
		}

		ZBool API_CustomsQty2_ReadOnly => API_CustomsUQ2.IsEmpty || IsExport;

		[ReadOnly(true)]
		[ResourceStringData("CED97F18-1892-4F79-831F-5E2A02C97EC4", Caption = "Statistical Quantity Unit", ShortCaption = "UQ")]
		public override ZString API_CustomsUQ2 { get => base.API_CustomsUQ2; set => base.API_CustomsUQ2 = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.Countries))]
		[ResourceStringData("141A0DA5-C7F8-47FE-AF7D-6E0759DB6425", Caption = "Goods Origin", ShortCaption = "Origin")]
		public override ZString API_RN_NKGoodsOrigin
		{
			get => base.API_RN_NKGoodsOrigin;
			set
			{
				var oldValue = API_RN_NKGoodsOrigin;
				base.API_RN_NKGoodsOrigin = value;
				if (!IsCopying && API_RN_NKGoodsOrigin != oldValue)
				{
					SetDefaultPreferenceValue();
					if (Header.RequiresDefaultPackitemTaxes)
					{
						RemoveDutyPackedItemTax();
						DefaultDutyPackedItemTax();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsExport))]
		[MaxLength(Schema.API_PreferenceMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemLookups.PreferenceList))]
		[ResourceStringData("8B52C361-9976-4C34-B3D7-696423B3C517", Caption = "Preference", ShortCaption = "Preference")]
		public ZString API_Preference
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.API_Preference);
			set
			{
				var oldValue = API_Preference;
				CheckMaximumLength(API_PreferenceInfo, value);
				this.SetSystemDefinedValue(Schema.API_Preference, value);
				if (oldValue != API_Preference && !IsCopying)
				{
					if (Header.RequiresDefaultPackitemTaxes)
					{
						RemoveDutyPackedItemTax();
						DefaultDutyPackedItemTax();
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateAPI_Preference();
				}
				API_PreferenceInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo API_PreferenceInfo => GetZPropertyInfo(nameof(API_Preference));

		public IEnumerable<RateView> ApplicableRates => UniversalTariff?.GetApplicableRates(DutyRateSelectionCriteria) ?? Enumerable.Empty<RateView>();

		public ZString AdValoremRateFormulaDerivedFrom => Factory.GetValue(ref adValoremRateFormulaDerivedFrom, () => ApplicableRates.Where(x => x.RateCode == Enterprise.Customs.TW.Business.UniversalReferenceConstants.RefCusRateCodes.DTA).Select(x => x.ZZ2_RateFormulaDerivedFrom).FirstOrDefault());

		CachedProperty<ZString> adValoremRateFormulaDerivedFrom;

		[ResourceStringData("BBB434B0-A241-40B1-8A76-FB901BA37048", Caption = "Ad-Valorem Duty Rate", ShortCaption = "Ad-Valorem Duty")]
		public ZString FormattedAdValoremDutyRate => Factory.GetValue(ref formattedAdValoremDutyRate, () => CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(AdValoremRateFormulaDerivedFrom));

		CachedProperty<ZString> formattedAdValoremDutyRate;

		public ZPropertyInfo FormattedAdValoremDutyRateInfo => GetZPropertyInfo(nameof(FormattedAdValoremDutyRate));

		public ZString SpecificDutyRateFormulaDerivedFrom => Factory.GetValue(ref specificDutyRateFormulaDerivedFrom, () => ApplicableRates.Where(x => x.RateCode == Enterprise.Customs.TW.Business.UniversalReferenceConstants.RefCusRateCodes.DTS).Select(x => x.ZZ2_RateFormulaDerivedFrom).FirstOrDefault());

		CachedProperty<ZString> specificDutyRateFormulaDerivedFrom;

		[ResourceStringData("B96280E2-534D-43E3-908A-0584254CCCB7", Caption = "Specific Duty Rate", ShortCaption = "Specific Duty")]
		public ZString FormattedSpecificDutyRate => Factory.GetValue(ref formattedSpecificDutyRate, () => CommonHelper.GetFormattedStringForRateFormulaDerivedFrom(SpecificDutyRateFormulaDerivedFrom));

		CachedProperty<ZString> formattedSpecificDutyRate;

		public ZPropertyInfo FormattedSpecificDutyRateInfo => GetZPropertyInfo(nameof(FormattedSpecificDutyRate));

		public IZZRateSelectionCriteria DutyRateSelectionCriteria => Factory.GetValue(ref dutyRateSelectionCriteria, GetDutyRateSelectionCriteria);

		CachedProperty<IZZRateSelectionCriteria> dutyRateSelectionCriteria;

		IZZRateSelectionCriteria GetDutyRateSelectionCriteria()
		{
			return new RateSelectionCriteria(this, Universal.Constants.RateTypes.Duty, ZString.Empty);
		}

		public void SetDefaultPreferenceValue()
		{
			var isImport = IsImport;
			if (!isImport)
			{
				API_Preference = ZString.Empty;
			}
			else if (API_Preference.IsEmpty && !API_Tariff.IsEmpty && !API_RN_NKGoodsOrigin.IsEmpty)
			{
				var codeList = Lookups.PreferenceList;
				var codesToLookFor = new string[] { TW.Business.Constants.PreferenceCodes.Preference2, TW.Business.Constants.PreferenceCodes.Preference1, TW.Business.Constants.PreferenceCodes.Standard };
				API_Preference = codesToLookFor.FirstOrDefault(codeList.ContainsCode);
			}
		}

		void CalculatedPackedItemTaxBaseValue()
		{
			AsycudaTaxes.Cast<AsycudaPackedItemTax>().Where(c => new ChargeTypeBaseList().ContainsCode(c.AET_ChargeType) && c.AET_RateOverrideReasonCode.IsEmpty).ToList().ForEach(tax => tax.CalculatedAET_BaseValue());
			ReCalculateVATPackedItemTax();
		}

		public void ClearPackedItemTax()
		{
			AsycudaTaxes.Find(c => c.AET_RateOverrideReasonCode.IsEmpty).DeleteAll();
		}

		void ClearAndDefaultPackedItemTax()
		{
			ClearPackedItemTax();
			DefaultPackedItemTaxFromCusTariffSortedDictionary();
			DefaultDutyPackedItemTax();
		}

		public void DefaultPackedItemTaxFromCusTariffSortedDictionary()
		{
			foreach (var pair in GetValidRefCusTariffSortedDictionary())
			{
				var type = pair.Key.ZZI_TariffType.Left(3);
				if (!DefaultChangeIsRequired(type))
				{
					continue;
				}

				if (pair.Value.Count == 1)
				{
					var tariffCode = pair.Value[0].ZZ1_TariffCode;
					if (type == ChargeTypeOtherList.Codes.SS)
					{
						var rateFormulaDerivedFrom = pair.Value[0].Rates?.FirstOrDefault()?.ZZ2_RateFormulaDerivedFrom ?? ZString.Empty;
						var rateFromRateFormulaDerivedFrom = AsycudaPackedItemTax.GetRateFromRateFormulaDerivedFrom(rateFormulaDerivedFrom, this);
						if (rateFromRateFormulaDerivedFrom.isGetRate && rateFromRateFormulaDerivedFrom.rate > ZDecimal.Zero)
						{
							CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(type, tariffCode);
						}
					}
					else
					{
						CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(type, tariffCode);
					}
				}
				else
				{
					CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(type, ZString.Empty);
				}
			}
		}

		bool DefaultChangeIsRequired(ZString type) => !AsycudaTaxes.Cast<AsycudaPackedItemTax>().Any(c => c.AET_RateOverrideReasonCode == RateOverrideReasonCodeList.Codes.Override && c.AET_ChargeType == type);

		void RemoveDutyPackedItemTax()
		{
			var dutyPackedItemTaxs = AsycudaTaxes.Where(c => c.IsDuty && c.AET_RateOverrideReasonCode.IsEmpty).ToArray();
			dutyPackedItemTaxs.ForEach(AsycudaTaxes.RemoveAndDelete);
		}

		public void DefaultDutyPackedItemTax()
		{
			if (!FormattedAdValoremDutyRate.IsEmpty && CommonHelper.GetDecimalForRateFormulaDerivedFrom(FormattedAdValoremDutyRate) != ZDecimal.Zero && DefaultChangeIsRequired(ChargeTypeOtherList.Codes.DTA))
			{
				var tax = CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(ChargeTypeOtherList.Codes.DTA, ZString.Empty);
				tax.SetRateAndMethodOfCalculation();
			}
			if (!SpecificDutyRateFormulaDerivedFrom.IsEmpty && CommonHelper.GetDecimalForRateFormulaDerivedFrom(SpecificDutyRateFormulaDerivedFrom) != ZDecimal.Zero && DefaultChangeIsRequired(ChargeTypeOtherList.Codes.DTS))
			{
				var tax = CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(ChargeTypeOtherList.Codes.DTS, ZString.Empty);
				tax.SetRateAndMethodOfCalculation();
			}
			ReCalculateVATPackedItemTax();
			ReCalculateTPFPackedItemTax(Bill?.RequiresCalculateTPF ?? false);
		}

		public void ReCalculateVATPackedItemTax()
		{
			AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.VAT && c.AET_RateOverrideReasonCode.IsEmpty).DeleteAll();
			if (DefaultChangeIsRequired(ChargeTypeOtherList.Codes.VAT))
			{
				var baseValue = VATBaseAmount;
				if (baseValue > 0)
				{
					var tax = CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(ChargeTypeOtherList.Codes.VAT, ZString.Empty);
					tax.AET_Rate = Bill?.VatRate ?? ZDecimal.Zero;
					tax.AET_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
					tax.AET_BaseValue = baseValue.Truncate(2);
				}
			}
		}

		public ZDecimal VATBaseAmount
		{
			get
			{
				var asycudaTaxes = AsycudaTaxes.Cast<AsycudaPackedItemTax>().ToList();
				var dtaChargeAmount = asycudaTaxes.Where(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTA && c.AET_MethodOfPayment == TaxFeePaymentMethodList.Codes.CAS).Sum(c => c.AET_ChargeAmount);
				var dtsChargeAmount = asycudaTaxes.Where(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.DTS && c.AET_MethodOfPayment == TaxFeePaymentMethodList.Codes.CAS).Sum(c => c.AET_ChargeAmount);
				var otherChargesAmount = asycudaTaxes.Where(c => c.IsBelongVATTaxCharge && c.AET_MethodOfPayment == TaxFeePaymentMethodList.Codes.CAS).Sum(c => c.AET_ChargeAmount);
				return API_CustomsValue + Math.Max(dtaChargeAmount, dtsChargeAmount) + otherChargesAmount;
			}
		}

		public void ReCalculateTPFPackedItemTax(bool requiresCalculateTPF)
		{
			AsycudaTaxes.Find(c => c.AET_ChargeType == ChargeTypeOtherList.Codes.TPF && c.AET_RateOverrideReasonCode.IsEmpty).DeleteAll();
			if (requiresCalculateTPF && DefaultChangeIsRequired(ChargeTypeOtherList.Codes.TPF) && API_CustomsValue > 0)
			{
				var tax = CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(ChargeTypeOtherList.Codes.TPF, ZString.Empty);
				tax.AET_Rate = Bill?.TpfRate ?? ZDecimal.Zero;
				tax.AET_MethodOfCalculation = Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;
				tax.AET_BaseValue = API_CustomsValue;
			}
		}

		AsycudaPackedItemTax CreateOrFindTaxWithSuspendPackedItemTaxDefaulting(ZString chargeType, ZString tariff)
		{
			var tax = AsycudaTaxes.Cast<AsycudaPackedItemTax>().FirstOrDefault(c => c.AET_RateOverrideReasonCode.IsEmpty && c.AET_ChargeType == chargeType);
			if (tax == null)
			{
				tax = AsycudaTaxes.AddNew();
				using (tax.SuspendPackedItemTaxDefaulting())
				{
					tax.AET_ChargeType = chargeType;
					if (!tariff.IsEmpty)
					{
						tax.AET_Tariff = tariff.Left(tax.AET_TariffInfo.MaxLength);
					}
				}
			}
			return tax;
		}

		void UpdateLinePriceOrQuantityIfChangedFromUnitPriceChanges()
		{
			if (IsChangedFromUnitPrice && API_UnitPrice > ZDecimal.Zero)
			{
				if (API_GoodsValue == ZDecimal.Zero && API_CustomsQty > ZDecimal.Zero)
				{
					API_GoodsValue = CalculateLinePrice();
				}
				else if (API_CustomsQty == ZDecimal.Zero && API_GoodsValue > ZDecimal.Zero)
				{
					API_CustomsQty = CalculateQuantity();
				}
			}
		}

		void UpdateLinePriceOrUnitPriceIfChangedFromQuantityChanges()
		{
			if (IsChangedFromQuantity && API_CustomsQty > ZDecimal.Zero)
			{
				if (API_GoodsValue == ZDecimal.Zero && API_UnitPrice > ZDecimal.Zero)
				{
					API_GoodsValue = CalculateLinePrice();
				}
				else if (API_UnitPrice == ZDecimal.Zero && API_GoodsValue > ZDecimal.Zero)
				{
					API_UnitPrice = CalculateUnitPrice();
				}
			}
		}

		void UpdateUnitPriceOrQuantityIfChangedFromUnitPriceChanges()
		{
			if (IsChangedFromLinePrice && API_GoodsValue > ZDecimal.Zero)
			{
				if (API_UnitPrice == ZDecimal.Zero && API_CustomsQty > ZDecimal.Zero)
				{
					API_UnitPrice = CalculateUnitPrice();
				}
				else if (API_CustomsQty == ZDecimal.Zero && API_UnitPrice > ZDecimal.Zero)
				{
					API_CustomsQty = CalculateQuantity();
				}
			}
		}

		decimal CalculateUnitPrice()
		{
			return Utilities.Round(API_GoodsValue / API_CustomsQty, 6);
		}

		decimal CalculateQuantity()
		{
			return Utilities.Round(API_GoodsValue / API_UnitPrice, 4);
		}

		decimal CalculateLinePrice()
		{
			return Utilities.Round(API_UnitPrice * API_CustomsQty, 2);
		}

		#region FieldSettingInProgress

		void ILinePriceCalculationFieldSettingSupporter.Start(object type)
		{
			if (type != null)
			{
				if (!FieldSettingTypes.ContainsKey(type))
				{
					FieldSettingTypes.Add(type, 0);
				}
				FieldSettingTypes[type]++;
			}
		}

		void ILinePriceCalculationFieldSettingSupporter.Stop(object type)
		{
			if (type != null && FieldSettingTypes.ContainsKey(type))
			{
				FieldSettingTypes[type]--;
			}
		}

		Dictionary<object, int> FieldSettingTypes => fieldSettingTypes ?? (fieldSettingTypes = new Dictionary<object, int>());
		Dictionary<object, int> fieldSettingTypes;

		IDisposable GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType type)
		{
			return new LinePriceCalculationFieldSettingSupporter(this, type);
		}

		bool IsChangedFromUnitPrice => IsSettingUnitPrice && !IsSettingQuantity && !IsSettingLinePrice;

		bool IsChangedFromQuantity => !IsSettingUnitPrice && IsSettingQuantity && !IsSettingLinePrice;

		bool IsChangedFromLinePrice => !IsSettingUnitPrice && !IsSettingQuantity && IsSettingLinePrice;

		bool IsSettingUnitPrice => IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.UnitPrice);

		bool IsSettingQuantity => IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.Quantity);

		bool IsSettingLinePrice => IsFieldSettingInProgress(LinePriceCalculationFieldSettingType.LinePrice);

		bool IsFieldSettingInProgress(LinePriceCalculationFieldSettingType type) => FieldSettingTypes.TryGetValue(type, out var index) && index > 0;

		#endregion

		[MaxLength(30)]
		[ReadOnlyMember(nameof(IsImport))]
		[ResourceStringData("E6BA452E-2574-4FEE-A44E-022F52B77ABA", Caption = "Buyer Part No.", ShortCaption = "Buyer Part No.")]
		public override ZString API_CustomsBuyerPartNo { get => base.API_CustomsBuyerPartNo; set => base.API_CustomsBuyerPartNo = value; }

		[MaxLength(30)]
		[ReadOnlyMember(nameof(IsImport))]
		[ResourceStringData("0159D154-3D93-4358-AF5D-8C2E3260F6D7", Caption = "Supplier Part No.", ShortCaption = "Supplier Part No.")]
		public override ZString API_CustomsSupplierPartNo { get => base.API_CustomsSupplierPartNo; set => base.API_CustomsSupplierPartNo = value; }

		[MaxLength(14)]
		[ReadOnlyMember(nameof(IsImport))]
		[ResourceStringData("F0F9929D-550C-4BF4-90C2-4A711E6185BE", Caption = "Previous Bonded Entry No.", ShortCaption = "Entry No.")]
		public override ZString API_PreviousEntryNo { get => base.API_PreviousEntryNo; set => base.API_PreviousEntryNo = value; }

		[MaxLength(4)]
		[ReadOnlyMember(nameof(IsImport))]
		[ResourceStringData("9C4F0033-9FC9-4363-B73D-6131B22DBD2A", Caption = "Previous Bonded Entry Line No.", ShortCaption = "Line No.")]
		public override ZShort API_PreviousEntryLineNo { get => base.API_PreviousEntryLineNo; set => base.API_PreviousEntryLineNo = value; }

		[MaxLength(11)]
		[ResourceStringData("575853A0-A536-4EF7-8CD2-C4E4A4FD6F43", Caption = "Tariff Code", ShortCaption = "Tariff")]
		public override ZString API_Tariff
		{
			get => base.API_Tariff;
			set
			{
				var oldValue = base.API_Tariff;
				base.API_Tariff = value;
				if (!IsCopying && oldValue != base.API_Tariff)
				{
					UpdateCustomsUQ2();
					if (UniversalTariff != null)
					{
						SetDefaultPreferenceValue();
					}
				}
			}
		}

		public override void Delete()
		{
			AsycudaTaxes.RemoveAndDeleteAll();
			base.Delete();
		}

		void UpdateCustomsUQ2()
		{
			var uq2 = UniversalTariff?.ZZ1_ZZ8_UQ2 ?? ZString.Empty;
			API_CustomsUQ2Info.Value = uq2.SubstringSafe(0, API_CustomsUQ2Info.MaxLength);
			if (API_CustomsUQ2.IsEmpty)
			{
				API_CustomsQty2 = ZDecimal.Zero;
			}
		}

		#region IReconcileCandidate members
		ZDecimal IReconcileCandidate.ReconciledCustomsValue { get => API_CustomsValue; set => API_CustomsValue = value; }

		ZDecimal IReconcileCandidate.PreReconciledCustomsValue => API_CustomsValue;

		ZInt IReconcileCandidate.SecondarySortingValue => API_LineNo;
		#endregion

		ZDateTime EffectiveAssessmentDate => Header?.DeclarationDate ?? ZDateTime.Today;

		internal IDictionary<RefCusTariffType, List<TariffView>> GetValidRefCusTariffSortedDictionary() => TariffHelper.GetValidRefCusTariffSortedDictionary(Factory, UniversalTariff, API_Tariff, EffectiveAssessmentDate);

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		TaiwanTariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new TaiwanTariffFormatter());
		TaiwanTariffFormatter tariffFormatter;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public ZBool IsExport => Header?.IsExport ?? false;

		public ZBool IsImport => Header?.IsImport ?? false;

		public ZBool IsAir => Header?.IsAir ?? false;

		protected override IAsycudaPackedItemTaxCollection<ManifestBase.AsycudaTax, ManifestBase.AsycudaPackedItem> CreateNewAsycudaPackedItemTaxCollection() => new AsycudaPackedItemTaxCollection(this);

		public new AsycudaPackedItemTaxCollection AsycudaTaxes => (AsycudaPackedItemTaxCollection)base.AsycudaTaxes;
	}

	public class RateSelectionCriteria : IZZRateSelectionCriteria, IZZApplicabilitySelectionCriteria
	{
		public ZDateTime EffectiveDate { get; protected set; }

		public ZString TradeGroupCountry { get; protected set; }

		public ISet<ZString> SecondTradeGroups { get; protected set; }

		public ZString DataGrouping { get; protected set; }

		public ZString PrimaryPreference { get; protected set; }

		public ISet<ZString> AdditionalCodes { get; protected set; }

		public ZString ConcessionOrder { get; protected set; }

		public ZString RateType { get; protected set; }

		public ZString RateCode { get; protected set; }

		public RateDirection Direction { get; protected set; }

		public RateSelectionCriteria(AsycudaPackedItem packedItem, ZString rateType, ZString rateCode)
		{
			EffectiveDate = packedItem.Bill.Header.DeclarationDate;
			TradeGroupCountry = packedItem.API_RN_NKGoodsOrigin;
			SecondTradeGroups = new HashSet<ZString>();
			DataGrouping = Core.Constants.CountryCodes.Taiwan;
			PrimaryPreference = packedItem.API_Preference;
			AdditionalCodes = new HashSet<ZString> { };
			ConcessionOrder = ZString.Empty;
			RateType = rateType;
			RateCode = rateCode;
			Direction = RateDirection.Both;
		}
	}
}
