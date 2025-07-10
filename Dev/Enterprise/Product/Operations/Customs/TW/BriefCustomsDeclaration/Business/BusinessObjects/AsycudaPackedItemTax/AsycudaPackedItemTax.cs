using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList.ChargeTypeOtherList;
using UniversalReferenceConstants = Enterprise.Customs.TW.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemTax : ASYCUDA.Business.AsycudaTax, IUnitConverterDataProvider
	{
		const string VFD = "VFD";

		public AsycudaPackedItemTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AET_MethodOfPayment = TaxFeePaymentMethodList.Codes.CAS;
		}

		public new AsycudaPackedItem PackedItem => Factory.Load<AsycudaPackedItem>(AET_API_AsycudaPackedItem);

		public new AsycudaPackedItemTaxLookups Lookups => (AsycudaPackedItemTaxLookups)base.Lookups;

		AsycudaPackedItemTaxCollection AsycudaPackedItemTaxes => PackedItem?.AsycudaTaxes;

		protected override ManifestBase.AsycudaTaxLookups GetNewLookups() => new AsycudaPackedItemTaxLookups(this);

		protected override ManifestBase.AsycudaTaxValidation GetNewValidation() => new AsycudaPackedItemTaxValidation(this);

		[ResourceStringData("0BC96921-9D8A-475C-92A3-5F898D5AE683", Caption = "Action")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.RateOverrideReasonCodeList))]
		public override ZString AET_RateOverrideReasonCode { get => base.AET_RateOverrideReasonCode; set => base.AET_RateOverrideReasonCode = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemTaxLookups.ChargeTypeList))]
		[ResourceStringData("DEBE23E5-DF21-472A-AD84-CC347E31ACA1", Caption = "Type", FullDescription = "The types of the duties other than tariffs.")]
		public override ZString AET_ChargeType
		{
			get => base.AET_ChargeType;
			set
			{
				var oldValue = AET_ChargeType;
				base.AET_ChargeType = value;
				if (!IsCopying && oldValue != AET_ChargeType)
				{
					DefaultTariff();
					ClearTariffIfNeed();
					SetRateAndMethodOfCalculation();
					DefaultBaseValueIfNeed();
				}
			}
		}

		[ResourceStringData("12219344-6F1F-41D9-AFEE-9E0CB8109EBC", Caption = "Type Description", ShortCaption = "Type Desc.", FullDescription = "The type description of the duties other than tariffs.")]
		public ZString AET_ChargeTypeDesc => Lookups.ChargeTypeList.GetDescriptionFromCode(AET_ChargeType) ?? ZString.Empty;

		public ZPropertyInfo AET_ChargeTypeDescInfo => GetZPropertyInfo(nameof(AET_ChargeTypeDesc));

		public bool IsBelongVATTaxCharge => new VATTaxChargeList().ContainsCode(AET_ChargeType);

		public bool IsTaxCharge => new ChargeTypeTaxList().ContainsCode(AET_ChargeType);

		public bool IsTobaccoTax => AET_ChargeType == ChargeTypeOtherList.Codes.TT;

		public bool IsHealthWelfareSurcharge => AET_ChargeType == ChargeTypeOtherList.Codes.HWS;

		public bool IsDuty => AET_ChargeType == ChargeTypeOtherList.Codes.DTA || AET_ChargeType == ChargeTypeOtherList.Codes.DTS;

		public bool IsVAT => AET_ChargeType == ChargeTypeOtherList.Codes.VAT;

		public bool IsTPF => AET_ChargeType == ChargeTypeOtherList.Codes.TPF;

		public bool IsPercentageCharge => new PercentageChargeList().ContainsCode(AET_ChargeType);

		[ReadOnlyMember(nameof(AET_TariffReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemTaxLookups.TariffCollection))]
		[ResourceStringData("7CAE5212-B125-4AF6-B4CA-15E13094F911", Caption = "Tariff", FullDescription = "The types of the goods belonging to the duties other than tariffs.")]
		public override ZString AET_Tariff
		{
			get => base.AET_Tariff;
			set
			{
				var oldValue = AET_Tariff;
				base.AET_Tariff = value;
				if (!IsCopying && oldValue != AET_Tariff)
				{
					SetRateAndMethodOfCalculation();
				}
			}
		}

		public void SetRateAndMethodOfCalculation()
		{
			var rate = GetRateView();
			if (rate != null)
			{
				var rateFromRateFormulaDerivedFrom = GetRateFromRateFormulaDerivedFrom(rate.ZZ2_RateFormulaDerivedFrom, PackedItem);
				if (rateFromRateFormulaDerivedFrom.isGetRate)
				{
					AET_Rate = rateFromRateFormulaDerivedFrom.rate;
				}
				SetMethodOfCalculationFromRateFormula(rate.ZZ2_RateFormula);
			}
			else if (IsPercentageCharge)
			{
				AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
				if (PackedItem?.Bill is AsycudaBill bill)
				{
					if (IsVAT)
					{
						AET_Rate = bill.VatRate;
					}
					else if (IsTPF)
					{
						AET_Rate = bill.TpfRate;
					}
				}
			}

			if (IsTobaccoTax)
			{
				if (HwsRate == null)
				{
					AsycudaPackedItemTaxes?.RemoveHealthWelfareSurcharge();
				}
				else
				{
					AsycudaPackedItemTaxes?.CreateHealthWelfareSurchargeIfNeed();
				}
			}
		}

		void DefaultBaseValueIfNeed()
		{
			if (IsVAT && PackedItem is AsycudaPackedItem packedItem)
			{
				AET_BaseValue = packedItem.VATBaseAmount;
			}
		}

		internal static (ZBool isGetRate, ZDecimal rate) GetRateFromRateFormulaDerivedFrom(ZString formattedTariffRate, AsycudaPackedItem packedItem)
		{
			var isGetRate = false;
			ZDecimal rate;
			if (ZDecimal.TryParse(formattedTariffRate, out rate))
			{
				isGetRate = true;
			}
			else if (formattedTariffRate.TrimStart().ToLower().StartsWith((NoResString)"if("))
			{
				var rateFormulaParts = formattedTariffRate.Substring(3, formattedTariffRate.Length - 4).Split(new char[] { ',' });
				if (rateFormulaParts.Length == 3)
				{
					var unitCustomsValueArray = rateFormulaParts[0].Split(">=");
					var formulaDerivedFrom = rateFormulaParts[1].Split(new char[] { '*' })[0].Trim();
					if (unitCustomsValueArray.Length > 1 &&
						ZDecimal.TryParse(unitCustomsValueArray[1], out var unitCustomsValue) &&
						ZDecimal.TryParse(formulaDerivedFrom, out rate))
					{
						rate = (packedItem?.API_CustomsValue ?? ZDecimal.Zero) >= unitCustomsValue ? rate : ZDecimal.Zero;
						isGetRate = true;
					}
				}
			}
			else
			{
				var unitAndRate = formattedTariffRate.Split('/');
				rate = unitAndRate.Length > 0 ? ZDecimal.ParseSafe(unitAndRate[0], ZDecimal.Zero) : ZDecimal.Zero;
				isGetRate = true;
			}
			return (isGetRate, rate);
		}

		void SetMethodOfCalculationFromRateFormula(ZString rateFormula)
		{
			if (!rateFormula.IsEmpty)
			{
				if (rateFormula.Contains(VFD))
				{
					AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
				}
				else
				{
					var methodOfCalculationRegex = new Regex("\\[(?<methodOfCalculation>.*)\\]");
					if (methodOfCalculationRegex.Match(rateFormula) is Match match && match.Success)
					{
						AET_MethodOfCalculation = match.Groups[1].Value;
					}
				}
			}
		}

		public bool AET_TariffReadOnly => !IsTaxCharge;

		[ResourceStringData("0CD25380-243A-46FE-BC31-2EAACC2078B7", Caption = "Tariff Description", ShortCaption = "Tariff Desc.", FullDescription = "The types of the goods belonging to the duties other than tariffs.")]
		public ZString AET_TariffDesc => UniversalTariff?.FullTariffDescription(EffectiveAssessmentDate, false, false) ?? ZString.Empty;

		public ZPropertyInfo AET_TariffDescInfo => GetZPropertyInfo(nameof(AET_TariffDesc));

		public TariffView UniversalTariff => Factory.GetValue(ref universalTariffCached, delegate
		{
			var type = IsHealthWelfareSurcharge ? new ZString(ChargeTypeOtherList.Codes.TT) : AET_ChargeType;
			var tariff = IsHealthWelfareSurcharge ? AsycudaPackedItemTaxes?.FirstTobaccoTax?.AET_Tariff ?? ZString.Empty : AET_Tariff;
			return !type.IsEmpty && !tariff.IsEmpty ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Taiwan, type, tariff, EffectiveAssessmentDate) : null;
		});

		CachedProperty<TariffView> universalTariffCached;

		public ZDateTime EffectiveAssessmentDate => PackedItem?.Header?.DeclarationDate ?? ZDateTime.Today;

		[DecimalPlaces(5)]
		[ReadOnlyMember(nameof(AET_RateReadOnly))]
		[ResourceStringData("9C76DC3C-A745-47F9-A559-91834E2E7C46", Caption = "Rate", FullDescription = "The rate for duties, taxes and fees.")]
		public override ZDecimal AET_Rate
		{
			get => base.AET_Rate;
			set
			{
				var oldValue = AET_Rate;
				base.AET_Rate = value;
				if (!IsCopying && oldValue != AET_Rate)
				{
					CalculatedAET_ChargeAmount();
				}
			}
		}

		bool AET_RateReadOnly => AET_RateOverrideReasonCode.IsEmpty;

		RateView GetRateView()
		{
			switch (AET_ChargeType)
			{
				case ChargeTypeOtherList.Codes.TT:
					return TatRate;
				case ChargeTypeOtherList.Codes.HWS:
					return HwsRate;
				case ChargeTypeOtherList.Codes.DTA:
					return PackedItem?.ApplicableRates?.FirstOrDefault(reate => reate.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTA);
				case ChargeTypeOtherList.Codes.DTS:
					return PackedItem?.ApplicableRates?.FirstOrDefault(reate => reate.RateCode == UniversalReferenceConstants.RefCusRateCodes.DTS);
				default:
					return UniversalTariff?.Rates?.FirstOrDefault();
			}
		}

		RateView TatRate => UniversalTariff?.Rates?.FirstOrDefault(reate => reate.RateCode == UniversalReferenceConstants.RefCusRateCodes.TAT);

		RateView HwsRate => UniversalTariff?.Rates?.FirstOrDefault(reate => reate.RateCode == UniversalReferenceConstants.RefCusRateCodes.HWS);

		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemTaxLookups.MethodOfCalculationList))]
		[ReadOnlyMember(nameof(AET_MethodOfCalculationReadOnly))]
		[ResourceStringData("FC4ACEEF-3F5D-4C52-AF0E-B1E8F279A8EA", Caption = "Method Of Calculation", FullDescription = "The Method Of Calculation for duties, taxes and fees.")]
		public override ZString AET_MethodOfCalculation
		{
			get => base.AET_MethodOfCalculation;
			set
			{
				var oldValue = AET_MethodOfCalculation;
				base.AET_MethodOfCalculation = value;
				if (!IsCopying && oldValue != AET_MethodOfCalculation)
				{
					CalculatedAET_BaseValue();
				}
			}
		}

		bool AET_MethodOfCalculationReadOnly => IsPercentageCharge || AET_RateOverrideReasonCode.IsEmpty;

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(AET_BaseValueReadOnly))]
		[ResourceStringData("AAD6375D-655C-4F12-873D-2C177B86027E", Caption = "Base Amount", FullDescription = "The Base Amount for duties, taxes and fees.")]
		public override ZDecimal AET_BaseValue
		{
			get => base.AET_BaseValue;
			set
			{
				var oldValue = AET_BaseValue;
				base.AET_BaseValue = value;
				if (!IsCopying && oldValue != AET_BaseValue)
				{
					CalculatedAET_ChargeAmount();
				}
			}
		}

		public bool AET_BaseValueReadOnly => AET_ChargeType.IsEmpty;

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(AET_ChargeAmountReadOnly))]
		[ResourceStringData("644C562E-110C-40A1-A7CE-B88F28FA53F0", Caption = "Amount", FullDescription = "The Amount of duties, taxes and fees.")]
		public override ZDecimal AET_ChargeAmount
		{
			get => base.AET_ChargeAmount;
			set
			{
				var oldValue = AET_ChargeAmount;
				base.AET_ChargeAmount = value;
				if (!IsCopying && oldValue != AET_ChargeAmount && new ChargeTypeBaseList().ContainsCode(AET_ChargeType))
				{
					PackedItem?.ReCalculateVATPackedItemTax();
				}
			}
		}

		bool AET_ChargeAmountReadOnly => AET_RateOverrideReasonCode.IsEmpty;

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackedItemTaxLookups.MethodOfPaymentList))]
		[ResourceStringData("A8E05B43-4CBC-4613-803B-D4AC2CF955CC", Caption = "Payment Method", FullDescription = "The payment method for duties other than tariffs.")]
		public override ZString AET_MethodOfPayment { get => base.AET_MethodOfPayment; set => base.AET_MethodOfPayment = value; }

		[ResourceStringData("9C796F36-265E-43C8-AE2B-13F02E0A6AE7", Caption = "Payment Method Description", ShortCaption = "Payment Method Desc.", FullDescription = "The payment method description for duties other than tariffs.")]
		public ZString AET_MethodOfPaymentDesc => Lookups.MethodOfPaymentList.GetDescriptionFromCode(AET_MethodOfPayment);

		public ZPropertyInfo AET_MethodOfPaymentDescInfo => GetZPropertyInfo(nameof(AET_MethodOfPaymentDesc));

		AdditionalDutiesTariffTypeList GetAdditionalDutiesTariffTypeList() => new AdditionalDutiesTariffTypeList(Factory, Core.Constants.CountryCodes.Taiwan);

		public RefCusTariffType UniversalTariffType => !AET_ChargeType.IsEmpty ? GetAdditionalDutiesTariffTypeList()[AET_ChargeType] : null;

		void DefaultTariff()
		{
			if (!IsTariffDefaultingSuspended)
			{
				var tariff = ZString.Empty;
				var tariffType = UniversalTariffType;
				var parentPackedItem = PackedItem;
				if (tariffType != null && parentPackedItem != null)
				{
					var tariffsRelated = new List<TariffView>();
					var relatedTariffsFound = parentPackedItem.GetValidRefCusTariffSortedDictionary().TryGetValue(tariffType, out tariffsRelated);
					if (relatedTariffsFound && tariffsRelated.Count == 1)
					{
						tariff = tariffsRelated[0].ZZ1_TariffCode.Left(AET_TariffInfo.MaxLength);
					}
				}
				AET_Tariff = tariff;
			}
		}

		void ClearTariffIfNeed()
		{
			if (AET_TariffReadOnly)
			{
				AET_Tariff = ZString.Empty;
			}
		}

		ZDecimal NetWeight => PackedItem.API_NetWeight;

		ZString NetWeightUQ => PackedItem.API_NetWeightUQ;

		UnitConverter UnitConverter => fUnitConverter ?? (fUnitConverter = new UnitConverter(this));
		UnitConverter fUnitConverter;

		internal void CalculatedAET_BaseValue()
		{
			var methodOfCalculation = AET_MethodOfCalculation;
			if (PackedItem != null && !methodOfCalculation.IsEmpty)
			{
				if (methodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage)
				{
					AET_BaseValue = PackedItem.API_CustomsValue;
				}
				else if (UnitConverter.ConversionFactor(PackedItem.API_CustomsUQ, methodOfCalculation) is ZDecimal conversionFactor && conversionFactor > ZDecimal.Zero)
				{
					AET_BaseValue = conversionFactor * PackedItem.API_CustomsQty;
				}
				else if (UnitConverterHelper.CanConvertFromNetWeightToCustomsUnit(NetWeight, NetWeightUQ, methodOfCalculation))
				{
					AET_BaseValue = Weight.Convert(NetWeight, NetWeightUQ, UnitConverterHelper.ConvertToCW1StandardWeightUnit(methodOfCalculation));
				}
				else if (AET_MethodOfCalculation == PackedItem.API_CustomsUQ2)
				{
					AET_BaseValue = PackedItem.API_CustomsQty2;
				}
				else
				{
					AET_BaseValue = ZDecimal.Zero;
				}
			}
		}

		void CalculatedAET_ChargeAmount()
		{
			AET_ChargeAmount = Utilities.Round(AET_Rate * AET_BaseValue, 3);
		}

		#region IUnitConverterDataProvider
		ZString IUnitConverterDataProvider.CountryCode => Core.Constants.CountryCodes.Taiwan;
		BusinessObjectFactory IUnitConverterDataProvider.Factory => Factory;
		IEnumerable<IUnitConverter> IUnitConverterDataProvider.GetUnitConversionFactorsFromProductUnits() => System.Array.Empty<IUnitConverter>();
		MasterFiles.Business.OrgSupplierPart IUnitConverterDataProvider.Product => null;
		ZGuid IUnitConverterDataProvider.SupplierFK => ZGuid.Empty;
		bool IUnitConverterDataProvider.ProductHasSpecificUnitConversions => false;
		ZString IUnitConverterDataProvider.Type => RPTypeList.Codes.CommercialInvoice;
		#endregion

		#region Suspend Tariff Defaulting

		int tariffDefaultingSuspenderIndex;

		bool IsTariffDefaultingSuspended => tariffDefaultingSuspenderIndex > 0;

		internal IDisposable SuspendPackedItemTaxDefaulting()
		{
			return new TariffDefaultingSuspender(this);
		}

		class TariffDefaultingSuspender : IDisposable
		{
			public TariffDefaultingSuspender(AsycudaPackedItemTax tax)
			{
				packedItemTax = tax;
				packedItemTax.tariffDefaultingSuspenderIndex++;
			}

			readonly AsycudaPackedItemTax packedItemTax;

			public void Dispose()
			{
				packedItemTax.tariffDefaultingSuspenderIndex--;
			}
		}
		#endregion
	}
}
