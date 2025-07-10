using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.TW;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.ServiceTasks.Testing
{
	[TestedType(typeof(ServiceTaskEnvironmentChecker))]
	sealed class ServiceTaskEnvironmentCheckerTest : TestCaseWithFactory
	{
		public void TestCheckTWCompanyHasCredentialsForLicensing()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertEquals("Pre-condition, company EDI is set for Taiwan", CountryCodes.Taiwan, company.GC_RN_NKCountryCode);

			var credential = ObjectFactory.New<ITWGlbCompanyWrapper>(company).LicensingCertificate;
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			AssertEquals(ZString.Empty, ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials());
		}

		public void TestCheckTWCompanyHasCredentialsForForwarder()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertEquals("Pre-condition, company EDI is set for Taiwan", CountryCodes.Taiwan, company.GC_RN_NKCountryCode);

			var credential = ObjectFactory.New<ITWGlbCompanyWrapper>(company).ForwarderCertificate;
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			AssertEquals(ZString.Empty, ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials());
		}

		public void TestCheckTWCompanyHasCredentialsForCustomsBrokerage_TradeVan()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertEquals("Pre-condition, company EDI is set for Taiwan", CountryCodes.Taiwan, company.GC_RN_NKCountryCode);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			var credential = Factory.NewWithValidTestData<GlbExternalPassword>();
			credential.GP_GC = company.PK;
			credential.GP_GS = staff.PK;
			credential.GP_PasswordType = PasswordTypesList.Codes.TVA;
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			AssertEquals(ZString.Empty, ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials());
		}

		public void TestCheckTWCompanyHasCredentialsForCustomsBrokerage_UniversalEC()
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertEquals("Pre-condition, company EDI is set for Taiwan", CountryCodes.Taiwan, company.GC_RN_NKCountryCode);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			var credential = Factory.NewWithValidTestData<GlbExternalPassword>();
			credential.GP_GC = company.PK;
			credential.GP_GS = staff.PK;
			credential.GP_PasswordType = PasswordTypesList.Codes.UVC;
			credential.GP_UserID = "USERNAME";
			credential.CurrentDecryptedPassword = "PASSWORD";
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();

			AssertEquals(ZString.Empty, ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials());
		}

		public void TestCheckTWCompanyHasCredentials_NoCredentials()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Taiwan;
			Factory.Save();

			AssertEquals("There is no credential configuration on Taiwan companies.", ServiceTaskEnvironmentChecker.CheckTWCompanyHasCredentials());
		}
	}
}
