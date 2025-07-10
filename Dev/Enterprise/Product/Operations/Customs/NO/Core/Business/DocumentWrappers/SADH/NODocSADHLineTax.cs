using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHLineTax : DocBaseWrapper, IDutyCategory
{
	public static NODocSADHLineTax New(CusEntryLineFee fee, BusinessObjectFactory factory)
	{
		return new NODocSADHLineTax(fee, factory);
	}

	NODocSADHLineTax(CusEntryLineFee fee, BusinessObjectFactory factory) : base(fee, factory)
	{
		this.fee = Argument.NotNull(fee, nameof(fee));
	}

	internal NODocSADHLineTax(CusEntryLineFee fee, ZInt chargeAmount, BusinessObjectFactory factory) : base(fee, factory)
	{
		this.fee = Argument.NotNull(fee, nameof(fee));
		chargeAmountTotal = Argument.NotNull(chargeAmount, nameof(chargeAmount));
	}

	public ZString Type => fee.CF_ChargeType.Substring(0, 2);

	public ZInt ChargeAmount => (ZInt)fee.CF_ChargeAmount;

	public ZInt ChargeAmountTotal => chargeAmountTotal;

	readonly ZInt chargeAmountTotal;

	public ZString BaseValue => fee.CF_BaseValue.ToString("f2", NOCultureInfo);

	public ZString Rate => GetRate();

	readonly CusEntryLineFee fee;

	ZString GetRate()
	{
		return fee.CF_Rate.ToString("f2", NOCultureInfo) + fee.CF_RateType;
	}

	static CultureInfo NOCultureInfo => new("nb-NO");

	#region IDutyCategory
	bool IDutyCategory.IsCustomsDuty => fee.IsCustomsDuty;

	bool IDutyCategory.IsAgriculturalDuty => fee.IsAgriculturalDuty;

	bool IDutyCategory.IsExciseDuty => fee.IsExciseDuty;

	bool IDutyCategory.IsVAT => fee.IsVAT;

	string IDutyCategory.DutyCode => fee.DutyCode;
	#endregion
}
