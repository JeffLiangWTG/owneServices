using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class GenRegCertAccredMaintListValidationTest : BusinessObjectValidationTestCase
{
	public void TestCertificateXZ_IssueDateValidation()
	{
		var error = "The date '01-Jan-1900' is more than 10 years old and thus is not valid.";
		var glbStaff = Factory.New<GlbStaff>();
		var certificate = glbStaff.Certificates.AddNew();
		certificate.XZ_IssueDate = new ZDateTime(1900, 1, 1);
		certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Poland;
		CombineAssertions(() =>
		{
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.APC;
			certificate.Validation.ValidateXZ_IssueDate();
			AssertHasError("APC type, PL", certificate.XZ_IssueDateInfo, error);
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate.Validation.ValidateXZ_IssueDate();
			AssertNoError("BRK type, PL", certificate.XZ_IssueDateInfo, error);
			certificate.XZ_Type = Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.CAR;
			certificate.Validation.ValidateXZ_IssueDate();
			AssertNoError("CAR type, PL", certificate.XZ_IssueDateInfo, error);
			certificate.XZ_RN_NKCountryOfIssuance = ZString.Empty;
			certificate.Validation.ValidateXZ_IssueDate();
			AssertHasError("CAR type, empty country", certificate.XZ_IssueDateInfo, error);
		});
	}
}
