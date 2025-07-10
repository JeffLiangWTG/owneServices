using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessCompanyLinkRuleWithMultipleCompaniesCollection))]
	class ProcessCompanyLinkRuleWithMultipleCompaniesCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessCompanyLinkRuleWithMultipleCompaniesCollection>
	{
		protected override ProcessCompanyLinkRuleWithMultipleCompaniesCollection GetCollectionToTest()
		{
			master = Factory.NewWithValidTestData<ProcessCompanyLinkRule>();
			var collection = new ProcessCompanyLinkRuleWithMultipleCompaniesCollection(master);
			Factory.Save();
			return collection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.NewWithValidTestData<ProcessCompanyLinkRule>();

		public void TestAddingRuleSetsHasChangesOnMaster()
		{
			var collection = GetCollectionToTest();
			AssertCollectionContains("Precondition", master, collection);
			Assert("Precondition", !master.HasChanges);

			collection.AddNew();

			Assert("Adding new rule for clone sets has changes on master", master.HasChanges);
		}

		ProcessCompanyLinkRule master;
	}
}
