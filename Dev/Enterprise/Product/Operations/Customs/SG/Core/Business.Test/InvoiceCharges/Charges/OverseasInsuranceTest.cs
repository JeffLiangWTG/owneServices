namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class OverseasInsuranceTest : Common.Testing.OverseasInsuranceTest
	{
		protected override Common.ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.OverseasInsurance;
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Singapore;
	}
}
