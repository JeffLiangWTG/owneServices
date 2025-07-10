using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(TWGlbCompanyWrapper))]
	sealed class TWGlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<TWGlbCompanyWrapper>
	{
		public void TestForwarderCertificate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_Code = "TC1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "TB1";
			branch.GB_IsActive = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS1";
			var password1 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password1.GP_PasswordType = "TVA";
			password1.GP_UserID = "1";
			password1.GP_GC = company.PK;
			var password2 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password2.GP_PasswordType = "TVF";
			password2.GP_UserID = "2";
			password2.GP_GC = company.PK;
			var password3 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password3.GP_PasswordType = "TVF";
			password3.GP_GS = staff.PK;
			password3.GP_UserID = "3";
			password3.GP_GC = company.PK;
			var password4 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password4.GP_PasswordType = "TVF";
			password4.GP_UserID = "4";
			password4.GP_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			var factory = new BusinessObjectFactory();
			company = factory.Load<GlbCompany>(company.PK);
			password2 = factory.Load<GlbCompanyCredential>(password2.PK);
			var wrapper = GetWrapper(company);
			AssertEquals(password2, wrapper.ForwarderCertificate);
		}

		public void TestForwarderCertificateType()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_Code = "TC1";
			Factory.Save();

			var wrapper = GetWrapper(company);
			AssertType<GlbCompanyCredential>(wrapper.ForwarderCertificate);
		}

		public void TestLicensingCertificate()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_Code = "TC1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "TB1";
			branch.GB_IsActive = true;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TS1";
			var password1 = Factory.NewWithValidTestData<GlbCompanyLicensingCredential>();
			password1.GP_PasswordType = "TVA";
			password1.GP_UserID = "1";
			password1.GP_GC = company.PK;
			var password2 = Factory.NewWithValidTestData<GlbCompanyLicensingCredential>();
			password2.GP_PasswordType = "NXM";
			password2.GP_UserID = "2";
			password2.GP_GC = company.PK;
			var password3 = Factory.NewWithValidTestData<GlbCompanyLicensingCredential>();
			password3.GP_PasswordType = "NXM";
			password3.GP_GS = staff.PK;
			password3.GP_UserID = "3";
			password3.GP_GC = company.PK;
			var password4 = Factory.NewWithValidTestData<GlbCompanyLicensingCredential>();
			password4.GP_PasswordType = "NXM";
			password4.GP_UserID = "4";
			password4.GP_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();
			var factory = new BusinessObjectFactory();
			company = factory.Load<GlbCompany>(company.PK);
			password2 = factory.Load<GlbCompanyLicensingCredential>(password2.PK);
			var wrapper = GetWrapper(company);
			AssertEquals(password2, wrapper.LicensingCertificate);
		}

		public void TestLicensingCertificateType()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			company.GC_Code = "TC1";
			Factory.Save();

			var wrapper = GetWrapper(company);
			AssertType<GlbCompanyLicensingCredential>(wrapper.LicensingCertificate);
		}

		public void TestGetCachedValue()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var wrapper1 = TWGlbCompanyWrapper.GetWrapper<TWGlbCompanyWrapper>(company);
			var wrapper2 = TWGlbCompanyWrapper.GetWrapper<TWGlbCompanyWrapper>(company);
			AssertSame(wrapper1, wrapper2);
		}
	}
}
