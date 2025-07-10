using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SouthAfricanVATValidationTest : TestCaseWithFactory
	{
		public void TestSouthAfricanVATValidation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;

			customsCode.OK_CustomsRegNo = "4770181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

			customsCode.OK_CustomsRegNo = "4A70181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The VAT must be 10 digits only");

			customsCode.OK_CustomsRegNo = "5770181941";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The VAT must start with a '4'");

			customsCode.OK_CustomsRegNo = "4770181942";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The VAT is invalid");

			customsCode.OK_CustomsRegNo = "477018194";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
			AssertHasWarningContaining(customsCode.OK_CustomsRegNoInfo, "The VAT must be 10 digits long");

			customsCode.OK_CustomsRegNo = "NA";
			AssertEquals("There is no warning in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());
		}
	}
}
