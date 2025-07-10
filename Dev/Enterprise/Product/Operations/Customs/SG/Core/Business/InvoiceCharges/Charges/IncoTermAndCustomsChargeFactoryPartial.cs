using System.Collections.Generic;

namespace Enterprise.Customs.SG.V4.Business
{
	public partial class IncoTermAndCustomsChargeFactory : Common.IncoTermAndCustomsChargeFactory
	{
		protected override Common.ICustomsChargeCode[] GetCharges()
		{
			var result = new List<Common.ICustomsChargeCode>();
			result.Add(OverseasFreight);
			result.Add(OverseasInsurance);
			result.Add(Other);
			result.Add(OptionalItem);
			return result.ToArray();
		}
	}
}
