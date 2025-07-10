using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ESSIICodeValidatorTest : TestCaseWithFactory
	{
		public void TestCodeValidation()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var siiCusCode = orgHeader.CustomsCodes.AddNew();
			siiCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			siiCusCode.OK_CodeType = SpainOrgCusCodeInfo.OrgCusCodes.SII;
			var message = "SII registration should match the VAT (NIF) business registration of the taxpayer.";

			AssertEquals(string.Empty, siiCusCode.OK_CustomsRegNo);
			AssertHasError(siiCusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");

			siiCusCode.OK_CustomsRegNo = "9125568";
			AssertHasError(siiCusCode.OK_CustomsRegNoInfo, message);

			var nifCusCode = orgHeader.CustomsCodes.AddNew();
			nifCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Chad;
			nifCusCode.OK_CodeType = SpainOrgCusCodeInfo.OrgCusCodes.NIF;
			nifCusCode.OK_CustomsRegNo = "9125568";
			AssertHasError(siiCusCode.OK_CustomsRegNoInfo, message);

			nifCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Spain;
			siiCusCode.Validation.ValidateOK_CustomsRegNo();
			AssertNoError(siiCusCode.OK_CustomsRegNoInfo, message);

			siiCusCode.OK_CustomsRegNo = "123456";
			AssertHasError(siiCusCode.OK_CustomsRegNoInfo, message);
		}
	}
}
