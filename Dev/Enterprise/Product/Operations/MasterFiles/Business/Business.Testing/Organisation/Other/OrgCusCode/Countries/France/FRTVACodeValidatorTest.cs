using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FRTVACodeValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestValidationTVA_France()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			var expectedMessage = "VAT Business Registration Number structures for France are either 'FRXX999999999' or 'XX999999999'.";

			cusCode.OK_CustomsRegNo = "FRAB123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "AB123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "55123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR123456789/20190101";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "ab123456789";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR123456789/";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "/20190101";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "20190101";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR12345678";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR12345678/20190101";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR1234567890";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "FR1234567890/20190101";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedMessage);
		}
	}
}
