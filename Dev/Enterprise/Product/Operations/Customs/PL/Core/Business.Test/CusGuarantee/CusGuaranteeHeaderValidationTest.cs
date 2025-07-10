namespace Enterprise.Customs.PL.Business.Testing;

class CusGuaranteeHeaderValidationTest : EU.Business.Testing.CusGuarantee.CusGuaranteeHeaderValidationTest
{
	public void TestCheckMainAccessCode_DigitsOnly()
	{
		var guarantee = Factory.New<CusGuaranteeHeader>();
		const string mainAccessCodeOnlyDigitsMessage = "Main Access Code should contain digits only";
		CombineAssertions(() =>
		{
			guarantee.Validation.ValidateMainAccessCode();
			AssertNoMessageError("No digits only message - empty", guarantee.MainAccessCodeInfo, mainAccessCodeOnlyDigitsMessage);
			guarantee.MainAccessCode = "FDSA";
			AssertHasMessageError("Digits only message", guarantee.MainAccessCodeInfo, mainAccessCodeOnlyDigitsMessage);
			guarantee.MainAccessCode = "222";
			AssertNoMessageError("No digits only message - correct", guarantee.MainAccessCodeInfo, mainAccessCodeOnlyDigitsMessage);
		});
	}

	public void TestCheckMainAccessCode_4Characters()
	{
		var guarantee = Factory.New<CusGuaranteeHeader>();
		const string mainAccessCodeFourCharactersMessage = "Main Access Code should contain 4 characters";
		CombineAssertions(() =>
		{
			guarantee.Validation.ValidateMainAccessCode();
			AssertNoMessageError("No four characters message - empty", guarantee.MainAccessCodeInfo, mainAccessCodeFourCharactersMessage);
			guarantee.MainAccessCode = "FDSA";
			AssertNoMessageError("No four characters message - correct", guarantee.MainAccessCodeInfo, mainAccessCodeFourCharactersMessage);
			guarantee.MainAccessCode = "222";
			AssertHasMessageError("Four characters message", guarantee.MainAccessCodeInfo, mainAccessCodeFourCharactersMessage);
		});
	}
}
