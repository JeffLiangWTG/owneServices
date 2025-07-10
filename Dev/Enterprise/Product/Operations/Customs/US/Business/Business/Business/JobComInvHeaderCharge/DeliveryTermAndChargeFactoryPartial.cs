using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public partial class DeliveryTermAndChargeFactory : Common.IncoTermAndCustomsChargeFactory
	{
		protected override Common.ICustomsChargeCode[] GetCharges()
		{
			var result = new List<Common.ICustomsChargeCode>();
			result.Add(AdditionCharge);
			result.Add(Common.CustomsChargeCodeProvider.Commission);
			result.Add(DeductionCharge);
			result.Add(DisbursementCharge);
			result.Add(Common.CustomsChargeCodeProvider.Discount);
			result.Add(ForeignInlandFreight);
			result.Add(LandingCharges);
			result.Add(Common.CustomsChargeCodeProvider.OtherCharges);
			result.Add(OverseasFreight);
			result.Add(OverseasInsurance);
			result.Add(Common.CustomsChargeCodeProvider.PackingCost);
			return result.ToArray();
		}
	}
}
