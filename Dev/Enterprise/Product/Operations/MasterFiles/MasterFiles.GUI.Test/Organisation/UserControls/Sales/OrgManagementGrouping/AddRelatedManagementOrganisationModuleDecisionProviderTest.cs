using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddRelatedManagementOrganisationModuleDecisionProviderTest : TestCaseWithFactory
	{
		public void TestProperty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var provider = new AddRelatedManagementOrganisationModuleDecisionProvider(org.OrgManagementGroupingModel.RootNodes.Single());

			AssertEquals(true, provider.ShouldDisplayNotifications);
			AssertEquals(true, provider.ShouldIgnoreAdditionalFilter);
			AssertNotNull(provider.List);
		}

		public void TestGetList()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var rootNode = org.OrgManagementGroupingModel.RootNodes.Single();
			rootNode.AddNewChild(org1);
			rootNode.AddNewChild(org2);
			var node = rootNode.ChildNodes.First();
			node.AddNewChild(org3);
			node.ChildNodes.FirstOrDefault()?.AddNewChild(org4);

			var provider = new AddRelatedManagementOrganisationModuleDecisionProvider(node);

			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org.PK);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org1.PK);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org2.PK);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org3.PK);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org4.PK);

			var rowValidationFilter = ((ILegacyBusinessObjectCollectionInternals)provider.List).AdditionalFilter;
			AssertEquals(query.FilterString, rowValidationFilter.FilterString);
		}
	}
}
