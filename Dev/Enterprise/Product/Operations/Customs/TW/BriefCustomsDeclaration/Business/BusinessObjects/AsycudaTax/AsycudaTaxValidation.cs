using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaTaxValidation : ASYCUDA.Business.AsycudaTaxValidation
	{
		public AsycudaTaxValidation(AsycudaTax parent) : base(parent)
		{
		}

		protected new AsycudaTax Parent => (AsycudaTax)base.Parent;

		ResourceString GetInvalidCodeMessage(ZString invalidCode) => ResString.GetMultilingualString("8A560F85-750A-41FF-A748-092A32B90C38", "'{0}' is invalid", invalidCode);

		protected override void CheckAET_RateOverrideReasonCode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_RateOverrideReasonCodeInfo, GetInvalidCodeMessage(Parent.AET_RateOverrideReasonCode));
		}

		protected override void CheckAET_MethodOfPayment()
		{
			base.CheckAET_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_MethodOfPaymentInfo, GetInvalidCodeMessage(Parent.AET_MethodOfPayment));
		}

		protected override void CheckAET_ChargeType()
		{
			base.CheckAET_ChargeType();
			ListValidation.MessageErrorIfInvalidCode(Parent.AET_ChargeTypeInfo, GetInvalidCodeMessage(Parent.AET_ChargeType));
		}

		protected override void CheckAET_MethodOfCalculation()
		{
		}
	}
}
