using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineFeeValidation : EU.Business.Declaration.EUUniversalCusEntryLineFeeValidation
{
	public CusEntryLineFeeValidation(CusEntryLineFee parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateBaseAmountForDisplay();
		ValidateTaxRateForDisplay();
	}

	public void ValidateBaseAmountForDisplay()
	{
		ValidateCalculatedProperty(Parent.CF_BaseValueForDisplayInfo);
	}

	public void ValidateTaxRateForDisplay()
	{
		ValidateCalculatedProperty(Parent.CF_RateForDisplayInfo);
	}

	protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

	protected override void CheckBaseValuePrecision(ZPropertyInfo baseValueInfo)
	{
		var baseValue = (ZDecimal)baseValueInfo.Value;
		if (baseValue.DecimalPlaces > 0)
		{
			baseValueInfo.AddError(Res.GetString("5C800354-E28C-4115-9F76-93785285EA9E", "Base Amount allows no decimal place values."));
		}
	}

	protected override void CheckCF_ChargeAmount()
	{
		base.CheckCF_ChargeAmount();
		if (Parent.CF_ChargeAmount.DecimalPlaces > 0)
		{
			Parent.CF_ChargeAmountInfo.AddError(Res.GetString("1997C412-F93E-4E09-AB92-6BE93C7813E6", "Total Amount allows no decimal place values."));
		}
	}

	protected override void CheckCF_MethodOfPayment()
	{
		base.CheckCF_MethodOfPayment();
		if (Parent.CF_MethodOfPayment.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CF_MethodOfPaymentInfo);
		}
	}

	protected void CheckCF_BaseValueForDisplay()
	{
		ValidateCF_BaseValue();
		Parent.CF_BaseValueForDisplayInfo.AddAllNotificationsFrom(Parent.CF_BaseValueInfo);
	}

	protected void CheckCF_RateForDisplay()
	{
		ValidateCF_Rate();
		Parent.CF_RateForDisplayInfo.AddAllNotificationsFrom(Parent.CF_RateInfo);
	}
}
