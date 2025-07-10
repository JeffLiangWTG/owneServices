using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESPersonIdentificationNumbersProviderTest : Customs.Business.Testing.DataProviderTestCase<AESPersonIdentificationNumbersProvider>
{
	public void TestNIP() => AssertEquals("NIP65123", Provider.NIP);

	public void TestRegon()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Regon with 9 chars is padded with zeros", $"REGON523400000", Provider.Regon);

			regonCusCode.OK_CustomsRegNo = "REGON123456789";
			AssertEquals("Regon with 14 chars is unchanged", "REGON123456789", GetProvider().Regon);

			regonCusCode.OK_CustomsRegNo = "";
			AssertNull("Empty Regon remain unchanged", GetProvider().Regon);
		});
	}

	public void TestPESEL() => AssertEquals("EORI5235", Provider.PESEL);

	public void TestOtherIdentificationNumber() => AssertEquals("TID12345", Provider.OtherIdentificationNumber);

	protected override AESPersonIdentificationNumbersProvider GetProvider() => new AESPersonIdentificationNumbersProvider(cusCodeCodeCollection);

	protected override void SetUp()
	{
		base.SetUp();
		var orgHeader = Factory.New<OrgHeader>();
		cusCodeCodeCollection = new OrgCusCodeCollection(orgHeader, Factory);

		var tinCusCode = cusCodeCodeCollection.AddNew();
		tinCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.NIP;
		tinCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		tinCusCode.OK_CustomsRegNo = "NIP65123";

		regonCusCode = cusCodeCodeCollection.AddNew();
		regonCusCode.OK_CodeType = OrgCusCode.CodeTypes.GovBusinessCode;
		regonCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		regonCusCode.OK_CustomsRegNo = "REGON5234";

		var peselCusCode = cusCodeCodeCollection.AddNew();
		peselCusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.PES;
		peselCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		peselCusCode.OK_CustomsRegNo = "EORI5235";

		var tidCusCode = cusCodeCodeCollection.AddNew();
		tidCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
		tidCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		tidCusCode.OK_CustomsRegNo = "TID12345";
	}

	OrgCusCodeCollection cusCodeCodeCollection;
	OrgCusCode regonCusCode;
}
