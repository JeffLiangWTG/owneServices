using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class RegistrationNumberTest : TestCaseWithFactory
	{
		public void TestRegistrationNumber()
		{
			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CodeType = "PAS";
			cusCode.OK_CustomsRegNo = "12345";

			var regNumber = new RegistrationNumber(cusCode);

			CombineAssertions(() =>
			{
				AssertEquals("Type.Code", "PAS", regNumber.Type.Code);
				AssertEquals("Type.Description", "Passport Number", regNumber.Type.Description);
				AssertEquals("CountryOfIssue.Code", "US", regNumber.CountryOfIssue.Code);
#pragma warning disable EDI007 // Customizable Data Translation Rule; ValueMultilingual doesn't exist for regNumber.
				AssertEquals("Value", "12345", regNumber.Value);
#pragma warning restore EDI007 // Customizable Data Translation Rule
			});
		}

		public void TestNullRegistrationNumber()
		{
			var regNumber = new RegistrationNumber(null);

			CombineAssertions(() =>
			{
				AssertNotNull("Type", regNumber.Type);
				AssertNotNull("CountryOfIssue", regNumber.CountryOfIssue);
				AssertNullOrEmpty("Value", regNumber.Value);
			});
		}
	}
}
