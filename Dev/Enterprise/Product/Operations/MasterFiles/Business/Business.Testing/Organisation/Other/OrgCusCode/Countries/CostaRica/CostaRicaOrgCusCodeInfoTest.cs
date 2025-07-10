using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CostaRicaOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.CostaRica;

		public void TestValidateAEO()
		{
			var invalidCodeMessage = "CR AEO number should consist of 12 numeric characters.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCode;
			cusCode.OK_CodeType = CostaRicaOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator;

			cusCode.OK_CustomsRegNo = "12345678901";
			AssertHasError("Invalid Length", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "12345678901X";
			AssertHasError("Invalid character", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "123456789012";
			AssertNoError("Valid Code", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(CountryCode);
		}
	}
}
