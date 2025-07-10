using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccSurchargeApplication))]
	sealed class AccSurchargeApplicationTest : EnterpriseBusinessObjectTestCase
	{
		#region TestASP_AT_ReadOnly

		public void TestASP_AT_ReadOnly_WhenCompanyIsNotSetYet()
		{
			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = ZGuid.Empty;

			AssertNoExceptionThrown(() => _ = surchargeApplication.ASP_ATInfo.ReadOnly);
		}

		public void TestASP_AT_ReadOnly_NoCountryFactory()
		{
			var globalFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory();

			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNotNull("Precondition: company", surchargeApplication.Company);

			Assert(surchargeApplication.ASP_ATInfo.ReadOnly);
			globalFactory.Verify(x => x.GetCountryFactory(It.IsAny<ZString>()), "Postcondition");
		}

		public void TestASP_AT_ReadOnly_CountryDoesNotSupportTaxIDForSurchargeApplicationConfiguration()
		{
			var countryFactory = new Mock<IAccountingCountryFactory>();
			var globalFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory(countryFactory.Object);

			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNotNull("Precondition: company", surchargeApplication.Company);

			Assert(surchargeApplication.ASP_ATInfo.ReadOnly);
			globalFactory.Verify(x => x.GetCountryFactory(It.IsAny<ZString>()), "Postcondition");
		}

		public void TestASP_AT_ReadOnly_CountrySupportsTaxIDForSurchargeApplicationConfiguration()
		{
			var countryFactory = TestMockObjectCreator.CreateIAccountingCountryFactoryImplementingInterface<ISurchargeApplicationConfigurationUsesTaxId>();
			var globalFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory((IAccountingCountryFactory)countryFactory.Object);

			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertNotNull("Precondition: company", surchargeApplication.Company);

			Assert(!surchargeApplication.ASP_ATInfo.ReadOnly);
			globalFactory.Verify(x => x.GetCountryFactory(It.IsAny<ZString>()), "Postcondition");
		}

		[ExpectNoExceptions]
		public void TestASP_AT_ReadOnly_CorrectCountryCodeIsUsed()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.SaintBarthelemy;

			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = company.PK;

			var globalFactory = TestMockObjectCreator.CreateAndRegisterIGlobalAccountingCountryFactory();
			_ = surchargeApplication.ASP_ATInfo.ReadOnly;
			globalFactory.Verify(x => x.GetCountryFactory(CountryCodes.SaintBarthelemy));

			company.GC_RN_NKCountryCode = CountryCodes.Djibouti;
			_ = surchargeApplication.ASP_ATInfo.ReadOnly;
			globalFactory.Verify(x => x.GetCountryFactory(CountryCodes.Djibouti));
		}

		#endregion

		public void TestSurchargeCodeDescription()
		{
			var surchargeConfiguration = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surchargeConfiguration.ASC_Code = "AAA";
			surchargeConfiguration.ASC_Description = "Test AAA";
			surchargeConfiguration.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var surchargeApplication = Factory.NewWithValidTestData<AccSurchargeApplication>();
			surchargeApplication.ASP_ASC_NKSurchargeCode = "AAA";
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;
			AssertEquals("Test AAA", surchargeApplication.SurchargeCodeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GlbCompany.CurrentCompany.AccSurchargeApplications.AddNew();
		}
	}
}
