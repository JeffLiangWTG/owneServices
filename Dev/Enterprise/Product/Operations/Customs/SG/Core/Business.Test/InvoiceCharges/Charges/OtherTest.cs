namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class OtherTest : Common.Testing.OtherChargesTest
	{
		protected override bool ExpectedIsVATible => true;
		protected override bool ExpectedIsDutiable => true;
		protected override bool ExpectedIsVATibleDeemedForThisCharge => true;
		protected override bool ExpectedIsDutiableDeemedForThisCharge => true;
		protected override bool ExpectedIsPercentageApplicable => true;
		protected override Common.ICustomsChargeCode GetChargeCodeToTest() => IncoTermAndCustomsChargeFactory.Other;
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Singapore;
	}
}
