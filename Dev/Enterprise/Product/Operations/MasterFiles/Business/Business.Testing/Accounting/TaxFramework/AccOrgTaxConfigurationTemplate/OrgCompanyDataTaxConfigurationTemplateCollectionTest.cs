using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCompanyDataTaxConfigurationTemplateCollection))]
	sealed class OrgCompanyDataTaxConfigurationTemplateCollectionTest_ForReceivable : OrgCompanyDataTaxConfigurationTemplateCollectionTest
	{
		protected override OrgCompanyDataTaxConfigurationTemplateCollection GetCollectionToTest()
		{
			return new OrgCompanyDataTaxConfigurationTemplateCollection(Factory, true);
		}
	}

	[TestedType(typeof(OrgCompanyDataTaxConfigurationTemplateCollection))]
	sealed class OrgCompanyDataTaxConfigurationTemplateCollectionTest_ForPayable : OrgCompanyDataTaxConfigurationTemplateCollectionTest
	{
		protected override OrgCompanyDataTaxConfigurationTemplateCollection GetCollectionToTest()
		{
			return new OrgCompanyDataTaxConfigurationTemplateCollection(Factory, false);
		}
	}

	[TestedType(typeof(OrgCompanyDataTaxConfigurationTemplateCollection))]
	abstract class OrgCompanyDataTaxConfigurationTemplateCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCompanyDataTaxConfigurationTemplateCollection>
	{
		public void TestRelationshipFilter()
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = nonCurrentCompany.PK;

			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AR1", true, currentCompany.PK);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AP1", false, currentCompany.PK);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AR2", true, nonCurrentCompany.PK);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AP2", false, nonCurrentCompany.PK);
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(1, collection.Count);
			AssertEquals("Should contain currentCompany tax config", collection.IsReceivable ? "AR1" : "AP1", collection[0].OCT_Code);
		}

		public void TestAdditionalFilter()
		{
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AR1", true, null, true);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AP1", false, null, true);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AR2", true, null, false);
			TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("AP2", false, null, false);
			Factory.Save();

			var collection = GetCollectionToTest();
			AssertEquals(1, collection.Count);
			AssertEquals("Should contain currentCompany tax config", collection.IsReceivable ? "AR1" : "AP1", collection[0].OCT_Code);
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
