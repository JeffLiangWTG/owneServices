using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DDPIncoTermTest : Common.Testing.BaseDDPIncoTermTest
	{
		protected override string CountryContext() => JobDeclaration.USIMPIncoTermAndCharge;

		protected override string CostAndFreightIncoTerm => TermsOfDeliveryList.Codes.CAF;

		protected override string IncotermToTest => TermsOfDeliveryList.Codes.DDP;

		protected override bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge) => base.ExpectedValueForThisChargeRecommeded(charge) && charge.Code != USCustomsChargeTypeList.Codes.OverseasInsurance;

		protected override bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge)
		{
			switch (charge.Code)
			{
				case CustomsChargeTypeList.Codes.AdditionCharge:
					return true;
				default:
					return charge.Code != USCustomsChargeTypeList.Codes.OverseasInsurance;
			}
		}
	}
}
