using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestShouldOnlyAllowOneMainAccreditationPerCertificateCode()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "A01";
			accreditation1.HAC_CertificateCode = "C01";
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "A02";
			accreditation2.HAC_CertificateCode = "C01";

			AssertEquals("Precondition", false, accreditation1.HAC_IsRefresher);
			AssertEquals("Precondition", false, accreditation2.HAC_IsRefresher);

			AssertHasError(accreditation2.HAC_IsRefresherInfo, "There can only be one main Accreditation for certificate code C01");
		}

		public void TestShouldOnlyAllowOneRefresherAccreditationPerCertificateCode()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "A01";
			accreditation.HAC_CertificateCode = "C01";
			var refresherAccreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			refresherAccreditation1.HAC_Code = "B01";
			refresherAccreditation1.HAC_CertificateCode = "C01";
			refresherAccreditation1.HAC_IsRefresher = true;

			AssertNoErrors(refresherAccreditation1.HAC_IsRefresherInfo);

			var refresherAccreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			refresherAccreditation2.HAC_Code = "B02";
			refresherAccreditation2.HAC_CertificateCode = "C01";
			refresherAccreditation2.HAC_IsRefresher = true;

			AssertHasError(refresherAccreditation2.HAC_IsRefresherInfo, "There can only be one refresher Accreditation for certificate code C01");
		}

		public void TestShouldNotAllowRefresherAccreditationWithoutMainAccreditation()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "A01";
			accreditation.HAC_CertificateCode = "C01";
			accreditation.HAC_IsRefresher = true;

			AssertHasError(accreditation.HAC_IsRefresherInfo, "There is only a refresher Accreditation for certificate code C01. Please add main Accreditation.");
		}

		public void TestCheckHAC_CertificateCode()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			AssertEquals("Precondition", string.Empty, accreditation.HAC_CertificateCode);
			accreditation.Validation.ValidateHAC_CertificateCode();
			AssertHasError(accreditation.HAC_CertificateCodeInfo, "Please enter a value.");

			accreditation.HAC_CertificateCode = "CAA";
			AssertHasError(accreditation.HAC_CertificateCodeInfo, "Enter a valid selection.");

			accreditation.HAC_CertificateCode = string.Empty;
			AssertHasError(accreditation.HAC_CertificateCodeInfo, "Please enter a value.");
		}

		public void TestCheckHAC_CodeIsUnique()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "RAC";
			AssertNoErrors(accreditation.HAC_CodeInfo);

			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "raC";
			AssertHasError(accreditation2.HAC_CodeInfo, "Code is not unique. Please enter a unique code.");
		}

		public void TestCheckHAC_MustCompleteInDaysIsNotNegative()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_MustCompleteInDays = -1;
			AssertHasError(accreditation.HAC_MustCompleteInDaysInfo, "value cannot be negative.");
		}

		public void TestCheckRefresherCertExpirationType()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			var accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation1.HAC_Code = "AC1";
			accreditation1.HAC_CertificateCode = "C01";
			AssertEquals(true, accreditation1.HAC_IsRefresher_ReadOnly);
			var accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation2.HAC_Code = "AC2";
			accreditation2.HAC_CertificateCode = "C01";
			accreditation2.HAC_IsRefresher = true;
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.RefresherCompletionDatePlusValidityPeriod;

			Factory.Save();

			accreditation2.HAC_RefresherCertificateExpiryType = "";
			AssertHasError(accreditation2.HAC_RefresherCertificateExpiryTypeInfo, "Please enter a Refresher Cert. Expiration.");
			accreditation2.HAC_RefresherCertificateExpiryType = "@@@";
			AssertHasError(accreditation2.HAC_RefresherCertificateExpiryTypeInfo, "Enter a valid Refresher Cert. Expiration.");
			accreditation2.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			AssertNoErrors(accreditation2.HAC_RefresherCertificateExpiryTypeInfo);

			accreditation1.HAC_RefresherCertificateExpiryType = "aaa";
			AssertHasError(accreditation1.HAC_RefresherCertificateExpiryTypeInfo, "Please do not enter a Refresher Cert. Expiration.");
		}
	}
}
