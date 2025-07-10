using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryLineFeeValidation : Customs.Business.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
		{
		}

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		protected override void CheckCF_BaseValue()
		{
			base.CheckCF_BaseValue();
			var baseInfo = Parent.CF_BaseValueInfo;
			MandatoryValidation.MessageErrorIfNotEntered(baseInfo);
			MandatoryValidation.MessageErrorIfIsNegative(baseInfo);
		}

		protected override void CheckCF_ChargeAmount()
		{
			base.CheckCF_ChargeAmount();
			var chargeInfo = Parent.CF_ChargeAmountInfo;
			MandatoryValidation.MessageErrorIfIsNegative(chargeInfo);
		}

		protected override void CheckCF_ChargeType()
		{
			base.CheckCF_ChargeType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CF_ChargeTypeInfo);
		}

		protected override void CheckCF_MethodOfPayment()
		{
			base.CheckCF_MethodOfPayment();
			var targetInfo = Parent.CF_MethodOfPaymentInfo;
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (!Parent.CF_Rate.IsEmpty)
			{
				MandatoryValidation.CheckEntered(targetInfo);
			}
		}

		protected override void CheckCF_Rate()
		{
			base.CheckCF_Rate();
			if (Parent.CF_Rate.DecimalPlaces > 5)
			{
				Parent.CF_RateInfo.AddMessageError(Res.GetString("7aa82c05-ad53-4bf2-8ae1-4da8021c3b81", "Tax Rate allows only 5 decimal places."));
			}
		}

		protected override void CheckCF_RateOverrideReasonCode()
		{
			base.CheckCF_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_RateOverrideReasonCodeInfo);
		}
	}
}
