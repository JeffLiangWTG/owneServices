using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.PL.ICusEntryLineFee
{
	public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.CusEntryLineFee.Schema
	{
		public const string CF_BaseValueForDisplay = "CF_BaseValueForDisplay";
		public const string CF_RateForDisplay = "CF_RateForDisplay";
	}

	public new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;

	protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

	protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation()
	{
		return IsImport ? new ImportCusEntryLineFeeValidation(this) : new CusEntryLineFeeValidation(this);
	}

	[MaxLength(1)]
	public override ZString CF_MethodOfPayment { get => base.CF_MethodOfPayment; set => base.CF_MethodOfPayment = value.ToUpper(); }

	public override ZString CF_MethodOfCalculation
	{
		get => base.CF_MethodOfCalculation;
		set
		{
			var oldValue = CF_MethodOfCalculation;
			base.CF_MethodOfCalculation = value;
			if (oldValue != CF_MethodOfCalculation && !IsCopying)
			{
				if (!UsePercentageAsCalculationMethod)
				{
					CF_Rate = ZDecimal.Zero;
				}

				if (!CanDisplayBaseValue)
				{
					CF_BaseValue = ZDecimal.Zero;
				}
			}
		}
	}

	[ResourceStringData("PLCusEntryLineFee|RateForDisplay", Caption = "Tax Rate")]
	[ReadOnlyMember(nameof(RateForDisplayReadOnly))]
	[BusinessObjectMaxLengthTestExclude]
	public ZString CF_RateForDisplay
	{
		get => UsePercentageAsCalculationMethod
			? (ZString)CF_Rate.ToString(CF_RateDecimalPlaces, true)
			: ZString.Empty;
		set
		{
			var oldValue = CF_RateForDisplay;
			CF_Rate = value.IsEmpty ? ZDecimal.Zero : ParseDisplayValueToDecimal(value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateTaxRateForDisplay();
			}
			CF_RateForDisplayInfo.RefreshBinding(oldValue);
		}
	}

	[ResourceStringData("PLCusEntryLineFee|BaseValueForDisplay", Caption = "Base Amount")]
	[ReadOnlyMember(nameof(BaseValueForDisplayReadOnly))]
	[BusinessObjectMaxLengthTestExclude]
	public ZString CF_BaseValueForDisplay
	{
		get => CanDisplayBaseValue
			? (ZString)CF_BaseValue.ToString(CF_BaseValueDecimalPlaces, true)
			: ZString.Empty;
		set
		{
			var oldValue = CF_BaseValueForDisplay;
			CF_BaseValue = value.IsEmpty ? ZDecimal.Zero : ParseDisplayValueToDecimal(value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateBaseAmountForDisplay();
			}
			CF_BaseValueForDisplayInfo.RefreshBinding(oldValue);
		}
	}

	public ZPropertyInfo CF_RateForDisplayInfo => GetZPropertyInfo(Schema.CF_RateForDisplay);

	public ZPropertyInfo CF_BaseValueForDisplayInfo => GetZPropertyInfo(Schema.CF_BaseValueForDisplay);

	public ZString CF_RateFieldType => UsePercentageAsCalculationMethod ? nameof(FieldType.Decimal) : nameof(FieldType.Text);

	public ZString CF_BaseValueFieldType => CanDisplayBaseValue ? nameof(FieldType.Decimal) : nameof(FieldType.Text);

	public override ZDecimal CF_BaseValue
	{
		get => ZArchitecture.Core.Utilities.Round(base.CF_BaseValue, CF_BaseValueDecimalPlaces);
		set => base.CF_BaseValue = value;
	}

	public ZInt CF_RateDecimalPlaces => CusEntryLineFeeSchema.CF_Rate.Scale;
	public ZInt CF_BaseValueForDisplayDecimalPlaces => CF_BaseValueDecimalPlaces;

	protected override int CF_BaseValueDecimalPlacesCore => 4;

	protected override bool IncludeForVatCalculationCore => !notVatableChargeTypes.Contains(CF_ChargeType) && !notVatableMethodsOfPayment.Contains(CF_MethodOfPayment)
																											&& base.IncludeForVatCalculationCore;

	protected override IFeeRounder GetNewChargeAmountRounder() => new IntegerCusEntryLineFeeRounder();

	protected override ZString GetDefaultMethodOfPaymentValue()
	{
		if (!IsImport)
		{
			return base.GetDefaultMethodOfPaymentValue();
		}

		if (CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR884(this))
		{
			return new ZString(PLMethodOfPaymentList.Codes.L);
		}

		switch (CF_ChargeType)
		{
			case EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat:
				return
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR246(this) ? new ZString(PLMethodOfPaymentList.Codes.Z) :
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR885(this) ? new ZString(PLMethodOfPaymentList.Codes.G) :
					(EntryLine?.Declaration?.ZG_VATDeferType ?? ZString.Empty);
			case TaxTypeList.Codes.ExciseTax:
				return
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR1538(this) ? new ZString(PLMethodOfPaymentList.Codes.L) :
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR247(this) ? new ZString(PLMethodOfPaymentList.Codes.Z) :
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR296(this) ? new ZString(PLMethodOfPaymentList.Codes.L) :
					(EntryLine?.Declaration?.ZG_ExciseCode ?? ZString.Empty);
			case EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts:
				return
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR245(this) ? new ZString(PLMethodOfPaymentList.Codes.Z) :
					CusEntryLineFeeMethodOfPaymentHelper.CheckRuleR966(this) ? new ZString(PLMethodOfPaymentList.Codes.L) :
					GetDeclarationPaymentMethod();
			case EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty:
				return
					(EntryLine?.Fees.Cast<CusEntryLineFee>()
						.FirstOrDefault(f => f.CF_ChargeType.Equals(EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts))
						?.CF_MethodOfPayment.Equals(PLMethodOfPaymentList.Codes.Z) ?? false)
						? new ZString(PLMethodOfPaymentList.Codes.Z)
						: new ZString(PLMethodOfPaymentList.Codes.D);
			default:
				return GetDeclarationPaymentMethod();
		}

		ZString GetDeclarationPaymentMethod() => EntryLine?.Declaration?.JE_PaymentMethod ?? ZString.Empty;
	}

	protected override bool AllowZeroOrEmptyAmountCore => true;

	internal bool UsePercentageAsCalculationMethod => CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage;

	internal bool CanDisplayBaseValue => UsePercentageAsCalculationMethod || CF_ChargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;

	readonly IReadOnlyCollection<ZString> notVatableMethodsOfPayment =
	[
		PLMethodOfPaymentList.Codes.L,
		PLMethodOfPaymentList.Codes.Z
	];

	readonly IReadOnlyCollection<ZString> notVatableChargeTypes =
	[
		TaxTypeList.Codes.ProvisionalAntidumpingDuties,
		TaxTypeList.Codes.ProvisionalCountervailingDuties,
		TaxTypeList.Codes.AdditionalDutiesSecurity,
		TaxTypeList.Codes.Additional1P1TaxDuties,
		TaxTypeList.Codes.GuaranteedCharges
	];

	bool RateForDisplayReadOnly => IsActionBlank || !UsePercentageAsCalculationMethod;

	bool BaseValueForDisplayReadOnly => IsActionBlank || !CanDisplayBaseValue;

	bool IsImport => Factory.GetValue(ref isImport, () => EntryLine?.Declaration?.IsImport ?? false);
	CachedProperty<bool> isImport;

	ZDecimal ParseDisplayValueToDecimal(ZString value)
	{
		var numberFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;
		return ZDecimal.TryParse(value, numberFormat, out var result) ? result : ZDecimal.Zero;
	}
}
