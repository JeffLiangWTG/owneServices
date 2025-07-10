using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class KoreaSouthOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.KoreaSouth;

		public void TestIOrgCusCodeCustomsRegNoValidationProvider_Validate()
		{
			var validationProvider = OrgCusCodeCountryFactory.GetIOrgCusCodeCustomsRegNoValidationProvider(CountryCode);
			AssertNotNull("Pre-condition", validationProvider);

			AssertIsValidating(OrgCusCode.CodeTypes.VATCode, "123");
			AssertIsValidating(KoreaSouthComplianceInfo.CodeTypes.OfficeID, "123");
			AssertIsValidating(KoreaSouthComplianceInfo.CodeTypes.KoreanRegNoForResident, "123");
			AssertIsValidating(KoreaSouthComplianceInfo.CodeTypes.KBT, MasterFilesTestHelper.GetRandomString(101));
			AssertIsValidating(KoreaSouthComplianceInfo.CodeTypes.KBC, MasterFilesTestHelper.GetRandomString(101));
			AssertIsValidating(KoreaSouthComplianceInfo.CodeTypes.AEO, "123", true);

			void AssertIsValidating(ZString codeType, ZString invalidValue, bool isMessageError = false)
			{
				var code = Factory.NewWithValidTestData<OrgCusCode>();
				AssertEquals("Pre-condition", false, isMessageError ? code.OK_CustomsRegNoInfo.HasMessageErrors() : code.OK_CustomsRegNoInfo.HasErrors());

				using (code.SuspendValidationTesting())
				{
					code.OK_CodeType = codeType;
					code.OK_CustomsRegNo = invalidValue;
					validationProvider.Validate(code);

					Assert("Should has error", isMessageError ? code.OK_CustomsRegNoInfo.HasMessageErrors() : code.OK_CustomsRegNoInfo.HasErrors());
				}
			}
		}
	}
}
