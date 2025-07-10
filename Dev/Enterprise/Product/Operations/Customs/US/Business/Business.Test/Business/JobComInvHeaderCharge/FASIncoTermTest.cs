using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FASIncoTermTest : Common.Testing.BaseFASIncoTermTest
	{
		protected override string CountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override string IncotermToTest => TermsOfDeliveryList.Codes.FAS;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge) => base.ExpectedValueForThisChargeRecommeded(charge) && charge.Code != USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			switch (charge.Code)
			{
				case CustomsChargeTypeList.Codes.ExWorks:
				case CustomsChargeTypeList.Codes.PackingCost:
				case CustomsChargeTypeList.Codes.ForeignInlandFreight:
				case CustomsChargeTypeList.Codes.AdditionCharge:
					return true;
				default:
					return charge.IsIncoTermNeutral;
			}
		}
	}
}
