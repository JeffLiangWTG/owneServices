using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateCollection))]
	sealed class AccOrgTaxConfigurationTemplateCollectionTest : ActiveBusinessObjectCollectionTestCase<AccOrgTaxConfigurationTemplateCollection>
	{
		public void TestRelationshipFilter()
		{
			var currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			var templateForCurrentCompany = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateForCurrentCompany.OCT_GC_Company = currentCompany.PK;

			var nonCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = nonCurrentCompany.PK;
			var templateForNonCurrentCompany = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			templateForNonCurrentCompany.OCT_GC_Company = nonCurrentCompany.PK;

			Factory.Save();

			using (DisposableEnvironment.ForCompany(currentCompany.GC_Code))
			{
				var collection = GetCollectionToTest();
				Assert(collection.Contains(templateForCurrentCompany));
				Assert(!collection.Contains(templateForNonCurrentCompany));
			}

			using (DisposableEnvironment.ForCompany(nonCurrentCompany.GC_Code))
			{
				var collection = GetCollectionToTest();
				Assert(!collection.Contains(templateForCurrentCompany));
				Assert(collection.Contains(templateForNonCurrentCompany));
			}
		}

		public void TestModuleIDAttribute()
		{
			var moduleIDAttributes = GetCollectionToTest().GetType().GetCustomAttributes(typeof(ModuleIDAttribute), false);
			AssertEquals(1, moduleIDAttributes.Length);
			AssertEquals(ModuleId.AccOrgTaxConfigurationTemplate, (moduleIDAttributes[0] as ModuleIDAttribute).ModuleId);
		}
	}
}
