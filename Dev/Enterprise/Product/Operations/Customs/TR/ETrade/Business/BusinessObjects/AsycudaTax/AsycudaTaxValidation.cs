using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaTaxValidation : ASYCUDA.Business.AsycudaTaxValidation
	{
		public AsycudaTaxValidation(AsycudaTax parent)
			: base(parent)
		{
		}

		protected override void CheckAET_ChargeType()
		{
			base.CheckAET_ChargeType();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_ChargeTypeInfo);
		}

		protected override void CheckAET_MethodOfPayment()
		{
			base.CheckAET_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_MethodOfPaymentInfo);
		}

		protected override void CheckAET_RateOverrideReasonCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_RateOverrideReasonCodeInfo);
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;
	}
}
