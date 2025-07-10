using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class CertificateRequirementCheckerTest : TestCaseWithFactory
	{
		public void TestExistsCompanyWithCertificate()
		{
			CertificateRequirementChecker.ResetForTesting();

			CreateCompanyCertificate(CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
			Factory.Save();

			CombineAssertions("One test certificate created.", () =>
			{
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NXM);
			});

			DeleteCompanyCertificates(CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
			CreateCompanyCertificate(CountryCodes.NeutralZone, PasswordTypesList.Codes.NXM);
			Factory.Save();

			CombineAssertions("Cached values should have same results after creating/deleting certificates.", () =>
			{
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NXM);
			});

			CertificateRequirementChecker.ResetForTesting();

			CombineAssertions("Certificate existence is re-evaluated after resetting the cache.", () =>
			{
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NXM);
			});

			// Add two certificates for same type and country
			CreateCompanyCertificate(CountryCodes.Pitcairn, PasswordTypesList.Codes.PLN);
			CreateCompanyCertificate(CountryCodes.Pitcairn, PasswordTypesList.Codes.PLN);
			Factory.Save();

			CombineAssertions("Number of certificates for a country+type does not matter, only existence.", () =>
			{
				AssertCertificate(expected: true, CountryCodes.Pitcairn, PasswordTypesList.Codes.PLN);
				AssertCertificate(expected: false, CountryCodes.Pitcairn, PasswordTypesList.Codes.PHA);
			});
		}

		public void TestExistsCompanyWithCertificateAndStatus()
		{
			CertificateRequirementChecker.ResetForTesting();

			CreateCompanyCertificate(CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
			Factory.Save();

			CombineAssertions("One test certificate created.", () =>
			{
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.PasswordOK);
			});

			DeleteCompanyCertificates(CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB);
			CreateCompanyCertificate(CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.PasswordOK);
			Factory.Save();

			CombineAssertions("Cached values should have same results after creating/deleting certificates.", () =>
			{
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.PasswordOK);
			});

			CertificateRequirementChecker.ResetForTesting();

			CombineAssertions("Certificate existence is re-evaluated after resetting the cache.", () =>
			{
				AssertCertificate(expected: false, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificate(expected: true, CountryCodes.NeutralZone, PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.PasswordOK);
			});
		}

		public void TestExistsStaffWithCertificate()
		{
			CertificateRequirementChecker.ResetForTesting();

			CreateStaffCertificate("111", PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
			Factory.Save();

			CombineAssertions("One test certificate created.", () =>
			{
				AssertCertificateOnStaff(expected: true, "111", PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: false, "111", PasswordTypesList.Codes.NXM, PasswordStatusList.Codes.Valid);
			});

			DeleteStaffCertificates("111", PasswordTypesList.Codes.NCB);
			CreateStaffCertificate("222", PasswordTypesList.Codes.NXM, PasswordStatusList.Codes.Valid);
			Factory.Save();

			CombineAssertions("Cached values should have same results after creating/deleting certificates.", () =>
			{
				AssertCertificateOnStaff(expected: true, "222", PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: false, "222", PasswordTypesList.Codes.NXM, PasswordStatusList.Codes.Valid);
			});

			CertificateRequirementChecker.ResetForTesting();

			CombineAssertions("Certificate existence is re-evaluated after resetting the cache.", () =>
			{
				AssertCertificateOnStaff(expected: false, "222", PasswordTypesList.Codes.NCB, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: true, "222", PasswordTypesList.Codes.NXM, PasswordStatusList.Codes.Valid);
			});

			CreateStaffCertificate("333", PasswordTypesList.Codes.PLN, PasswordStatusList.Codes.Valid);
			CreateStaffCertificate("444", PasswordTypesList.Codes.PLN, PasswordStatusList.Codes.Valid);
			Factory.Save();

			CombineAssertions("Number of certificates for a country+type does not matter, only existence.", () =>
			{
				AssertCertificateOnStaff(expected: true, "333", PasswordTypesList.Codes.PLN, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: true, "444", PasswordTypesList.Codes.PLN, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: false, "333", PasswordTypesList.Codes.PHA, PasswordStatusList.Codes.Valid);
				AssertCertificateOnStaff(expected: false, "444", PasswordTypesList.Codes.PHA, PasswordStatusList.Codes.Valid);
			});

			CreateStaffCertificate("555", PasswordTypesList.Codes.CHT);
			CreateStaffCertificate("666", PasswordTypesList.Codes.CHT);
			Factory.Save();

			CombineAssertions("When PasswordStatus = null, should be ignored by query", () =>
			{
				AssertCertificateOnStaff(expected: true, "555", PasswordTypesList.Codes.CHT);
				AssertCertificateOnStaff(expected: true, "666", PasswordTypesList.Codes.CHT);
				AssertCertificateOnStaff(expected: true, "333", PasswordTypesList.Codes.PLN);
				AssertCertificateOnStaff(expected: true, "444", PasswordTypesList.Codes.PLN);
			});
		}

		void AssertCertificate(bool expected, string countryCode, string certificateType, string passwordStatus = null)
		{
			var actual = CertificateRequirementChecker.ExistsCompanyWithCertificate(countryCode, certificateType, passwordStatus);
			AssertEquals($"Does company in country {countryCode} with certificate {certificateType} exist?", expected, actual);
		}

		void AssertCertificateOnStaff(bool expected, string code, string certificateType, string passwordStatus = null)
		{
			var actual = CertificateRequirementChecker.ExistsStaffWithCertificate(certificateType, passwordStatus);
			AssertEquals($"Does staff {code} with certificate {certificateType} exist?", expected, actual);
		}

		void CreateCompanyCertificate(string countryCode, string certificateType, string passwordStatus = "")
		{
			var company =
				Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, countryCode)).FirstOrDefault()
				?? NewCompanyInCountry(countryCode);

			var certificate = Factory.New<GlbExternalPassword>();
			certificate.GP_GC = company.PK;
			certificate.GP_PasswordType = certificateType;
			certificate.GP_PasswordStatus = passwordStatus;
			certificate.GP_UserID = certificate.PK.ToString();
		}

		void CreateStaffCertificate(string staffCode, string certificateType, string passwordStatus = null)
		{
			var staff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, staffCode)).FirstOrDefault()
				?? NewStaff(staffCode);

			var certificate = Factory.New<GlbExternalPassword>();
			certificate.GP_GS = staff.PK;
			certificate.GP_PasswordType = certificateType;
			certificate.GP_PasswordStatus = passwordStatus;
			certificate.GP_UserID = certificate.PK.ToString();
		}

		GlbStaff NewStaff(string staffCode)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;
			return staff;
		}

		GlbCompany NewCompanyInCountry(string countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = $"#{countryCode}";
			company.GC_RN_NKCountryCode = countryCode;
			return company;
		}

		void DeleteStaffCertificates(string code, string certificateType)
		{
			var staffs = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, code));

			foreach (var staff in staffs)
			{
				var certificateQuery = new ZQuery(GlbExternalPasswordSchema.GP_GS, staff.PK);
				certificateQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, certificateType);
				Factory.Load<GlbExternalPassword>(certificateQuery).DeleteAll();
			}
		}

		void DeleteCompanyCertificates(string countryCode, string certificateType)
		{
			var companiesInCountry = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, countryCode));

			foreach (var company in companiesInCountry)
			{
				var certificateQuery = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
				certificateQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, certificateType);
				Factory.Load<GlbExternalPassword>(certificateQuery).DeleteAll();
			}
		}
	}
}
