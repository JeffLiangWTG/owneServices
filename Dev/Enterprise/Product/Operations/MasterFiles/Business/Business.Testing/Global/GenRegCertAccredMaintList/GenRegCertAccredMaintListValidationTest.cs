using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenRegCertAccredMaintListValidationTest : BusinessObjectValidationTestCase
	{
		public void TestXZ_Type()
		{
			var certificateTypeList = new OverrideImmuneCodeDescriptionBoolCollection();
			certificateTypeList.Add("ENA", null, true);
			certificateTypeList.Add("DIS", null, false);
			SystemDataRegistry.Instance.StaffCertificateTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certificateTypeList);

			var certificate = Factory.New<GenRegCertAccredMaintList>();
			var masterParent = Factory.New<GlbStaff>();
			certificate.MasterParent = masterParent;

			certificate.XZ_Type = "";
			AssertHasError(certificate.XZ_TypeInfo, "Please enter a Type.");

			certificate.XZ_Type = "MMM";
			AssertHasError(certificate.XZ_TypeInfo, "Enter a valid Type.");

			certificate.XZ_Type = "ENA";
			AssertNoErrors("Certificate Type is completely valid", certificate.XZ_TypeInfo);
			AssertNoWarnings("Certificate Type is completely valid", certificate.XZ_TypeInfo);

			certificate.XZ_Type = "DIS";
			AssertHasErrors("Inactive Certificate Type is invalid", certificate.XZ_TypeInfo);
			Factory.Save();
			certificate.Validation.ValidateAll();
			AssertNoErrors("Inactive Certificate Type is still invalid but should not give an error since it's been saved before", certificate.XZ_TypeInfo);
			AssertHasWarnings("Inactive Certificate Type gives a warning to show it's disabled", certificate.XZ_TypeInfo);
		}

		public void TestXZ_Type_UKStaffHandlingSecuredCargoCertificate()
		{
			const string expectedError = "Please upload the UK Cargo Supervisor certificate to eDocs using Document Type \"SEC\" (Staff / Group Security Documents) before saving.";

			var staff = Factory.New<GlbStaff>();
			var cert1 = Factory.New<GenRegCertAccredMaintList>();
			cert1.MasterParent = staff;
			cert1.XZ_Type = "PR2";
			AssertNoErrors("No eDocs requirement for PR2", cert1.XZ_TypeInfo);

			cert1.XZ_Type = "CS1";
			AssertHasError("CS1 requires SEC eDoc", cert1.XZ_TypeInfo, expectedError);

			staff.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "stuff.txt", "COR");
			cert1.Validation.ValidateXZ_Type();
			AssertHasError("Incorrect document type", cert1.XZ_TypeInfo, expectedError);

			staff.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes("Test eDoc"), "cert.txt", "SEC");
			cert1.Validation.ValidateXZ_Type();
			AssertNoError("SEC eDoc added", cert1.XZ_TypeInfo, expectedError);
		}

		public void TestXZ_Comment()
		{
			var certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_Comment = "";
			AssertHasError(certificate.XZ_CommentInfo, "Please enter a Comment.");

			certificate.XZ_Comment = "Some Comment";
			AssertNoError(certificate.XZ_CommentInfo, "Please enter a Comment.");

			certificate.XZ_ParentTableCode = GlbStaffSchema.Constants.Prefix;
			certificate.XZ_Comment = "";
			AssertNoErrors(certificate.XZ_CommentInfo);

			certificate.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			certificate.XZ_Comment = "";
			certificate.Validation.ValidateXZ_Comment();
			AssertNoErrors(certificate.XZ_CommentInfo);

			certificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO;
			certificate.Validation.ValidateXZ_Comment();
			AssertHasWarningContaining(certificate.XZ_CommentInfo, "Please enter Chinese name of the operator here if it is different from Full Name of the staff.");

			certificate.XZ_Comment = "Li";
			certificate.Validation.ValidateXZ_Comment();
			AssertNoWarnings(certificate.XZ_CommentInfo);

			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.Australia;
			certificate.XZ_Comment = "";
			certificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificate.Validation.ValidateXZ_Comment();
			AssertNoWarnings(certificate.XZ_CommentInfo);

			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.China;
			certificate.Validation.ValidateXZ_Comment();
			AssertHasWarningContaining(certificate.XZ_CommentInfo, "Please enter Chinese name of the broker here if it is different from Full Name of the staff.");

			certificate.XZ_Comment = "Li";
			certificate.Validation.ValidateXZ_Comment();
			AssertNoWarnings(certificate.XZ_CommentInfo);
		}

		[TestDate(2010, 5, 26, 16, 37, 55)]
		public void TestXZ_ExpiryOrDueDate()
		{
			var certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ExpiryOrDueDate = new ZDateTime(2011, 5, 26);
			AssertNoErrors(certificate.XZ_ExpiryOrDueDateInfo);
			AssertNoWarnings(certificate.XZ_ExpiryOrDueDateInfo);

			certificate.XZ_ExpiryOrDueDate = new ZDateTime(2018, 3, 6);
			AssertNoErrors(certificate.XZ_ExpiryOrDueDateInfo);
			AssertHasWarning(certificate.XZ_ExpiryOrDueDateInfo, "The date '06-Mar-2018' is more than 1 year from now.");

			certificate.XZ_ExpiryOrDueDate = new ZDateTime(2025, 5, 26, 16, 38, 0);
			AssertHasError(certificate.XZ_ExpiryOrDueDateInfo, "The date '26-May-2025' is more than 15 years from now and thus is not valid.");
			AssertNoWarnings(certificate.XZ_ExpiryOrDueDateInfo);
		}

		public void TestXZ_ExpiryOrDueDate_UKStaffHandlingSecuredCargoCertificates()
		{
			const string expectedError = "Please enter an Expiry Date.";

			var cert = Factory.New<GenRegCertAccredMaintList>();
			cert.XZ_Type = "ENA";
			AssertNoError("Expiry not required for ENA", cert.XZ_ExpiryOrDueDateInfo, expectedError);

			cert.XZ_Type = "CS2";
			AssertHasError("Expiry required for CS2", cert.XZ_ExpiryOrDueDateInfo, expectedError);

			cert.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
			AssertNoError("Has Expiry", cert.XZ_ExpiryOrDueDateInfo, expectedError);
		}

		public void TestXZ_RN_NKCountryOfIssuance()
		{
			var certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_RN_NKCountryOfIssuance = "??";
			AssertHasErrorContaining(certificate.XZ_RN_NKCountryOfIssuanceInfo, ListValidation.InvalidCodeError);

			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.UnitedStates;
			AssertNoErrorContaining(certificate.XZ_RN_NKCountryOfIssuanceInfo, ListValidation.InvalidCodeError);

			certificate.XZ_Type = Constants.StaffDefaultCertificateIDAndTrainingTypes.CNO;
			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.UnitedStates;
			AssertHasWarningContaining(certificate.XZ_RN_NKCountryOfIssuanceInfo, "Country/Region should be CN for Type 'CNO'.");

			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.China;
			AssertNoWarnings(certificate.XZ_RN_NKCountryOfIssuanceInfo);
		}

		public void TestXZ_StateOrProvinceOfIssuance()
		{
			var certificate = Factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_StateOrProvinceOfIssuance = "BB";
			AssertNoErrorContaining(certificate.XZ_StateOrProvinceOfIssuanceInfo, ListValidation.InvalidCodeError);

			certificate.XZ_RN_NKCountryOfIssuance = Constants.CountryCodes.UnitedStates;
			certificate.Validation.ValidateXZ_StateOrProvinceOfIssuance();
			AssertHasErrorContaining(certificate.XZ_StateOrProvinceOfIssuanceInfo, ListValidation.InvalidCodeError);

			certificate.XZ_StateOrProvinceOfIssuance = "CA";
			AssertNoErrorContaining(certificate.XZ_StateOrProvinceOfIssuanceInfo, ListValidation.InvalidCodeError);
		}
	}
}
