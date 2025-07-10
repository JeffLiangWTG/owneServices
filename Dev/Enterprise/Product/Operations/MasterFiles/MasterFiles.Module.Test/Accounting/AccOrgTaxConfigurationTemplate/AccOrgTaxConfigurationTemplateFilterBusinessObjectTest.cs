using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateFilterBusinessObject))]
	sealed class AccOrgTaxConfigurationTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AccOrgTaxConfigurationTemplateFilterBusinessObject();
		}

		readonly AccOrgTaxConfigurationTemplateFilterBusinessObject filterBO = new AccOrgTaxConfigurationTemplateFilterBusinessObject();

		public void TestTemplateCode()
		{
			var tacConfigurationTemplate1 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate1.OCT_Code = "100";
			var tacConfigurationTemplate2 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate2.OCT_Code = "210";
			var tacConfigurationTemplate3 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate3.OCT_Code = "310";
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["Template Code"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("Count", 3, collection.Count);

			filter.Property = "10";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 3, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should contain tacConfigurationTemplate2", true, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should contain tacConfigurationTemplate3", true, collection.Contains(tacConfigurationTemplate3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 1, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should not contain tacConfigurationTemplate2", false, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should not contain tacConfigurationTemplate3", false, collection.Contains(tacConfigurationTemplate3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 0, collection.Count);
			AssertEquals("Should not contain tacConfigurationTemplate1", false, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should not contain tacConfigurationTemplate2", false, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should not contain tacConfigurationTemplate3", false, collection.Contains(tacConfigurationTemplate3));
		}

		public void TestTemplateDescription()
		{
			var tacConfigurationTemplate1 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate1.OCT_Description = "100";
			var tacConfigurationTemplate2 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate2.OCT_Description = "210";
			var tacConfigurationTemplate3 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate3.OCT_Description = "310";
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["Template Description"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("Count", 3, collection.Count);

			filter.Property = "10";
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 3, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should contain tacConfigurationTemplate2", true, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should contain tacConfigurationTemplate3", true, collection.Contains(tacConfigurationTemplate3));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 1, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should not contain tacConfigurationTemplate2", false, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should not contain tacConfigurationTemplate3", false, collection.Contains(tacConfigurationTemplate3));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 0, collection.Count);
			AssertEquals("Should not contain tacConfigurationTemplate1", false, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should not contain tacConfigurationTemplate2", false, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should not contain tacConfigurationTemplate3", false, collection.Contains(tacConfigurationTemplate3));
		}

		public void TestTemplateType()
		{
			var tacConfigurationTemplate1 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate1.OCT_IsReceivable = true;
			var tacConfigurationTemplate2 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate2.OCT_IsReceivable = true;
			var tacConfigurationTemplate3 = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			tacConfigurationTemplate3.OCT_IsReceivable = false;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO["Template Type"];
			AssertNotNull(filter);
			filter.IsActive = true;

			var collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("Count", 3, collection.Count);

			filter.Property = "All";
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 3, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should contain tacConfigurationTemplate2", true, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should contain tacConfigurationTemplate3", true, collection.Contains(tacConfigurationTemplate3));

			filter.Property = "A/R";
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 2, collection.Count);
			AssertEquals("Should contain tacConfigurationTemplate1", true, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should contain tacConfigurationTemplate2", true, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should not contain tacConfigurationTemplate3", false, collection.Contains(tacConfigurationTemplate3));

			filter.Property = "A/P";
			collection = new AccOrgTaxConfigurationTemplateCollection(Factory, filterBO.Filter);
			AssertEquals("taxOverrideGroups.Count", 1, collection.Count);
			AssertEquals("Should not contain tacConfigurationTemplate1", false, collection.Contains(tacConfigurationTemplate1));
			AssertEquals("Should not contain tacConfigurationTemplate2", false, collection.Contains(tacConfigurationTemplate2));
			AssertEquals("Should contain tacConfigurationTemplate3", true, collection.Contains(tacConfigurationTemplate3));
		}

		public void TestActiveStatus()
		{
			using (AccOrgTaxConfigurationTemplateModule testModule = new AccOrgTaxConfigurationTemplateModule())
			{
				ModuleTextFilter filter = (ModuleTextFilter)testModule.FilterBusinessObject["Active Status"];
				AssertNotNull("The 'Active Status' should be exists in the TaxConfigurationTemplate module", filter);
				Assert("The 'Active Status' should be visible always", filter.Visibility == FilterVisibility.AlwaysApplied);
			}
		}
	}
}
