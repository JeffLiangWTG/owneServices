using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesHeaderCollectionBuilderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var org = Factory.New<OrgHeader>();
			var builder = new SalesHeaderCollectionBuilder();

			var collection = (SalesHeaderCollection)builder.New(org, true);
			AssertNotNull(collection.EntitySalesCollection);
			AssertEquals(org, collection.EntitySalesCollection.Entity);
		}

		public void TestNew_Opportunity()
		{
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var builder = new SalesHeaderCollectionBuilder();

			var collection = (SalesHeaderCollection)builder.New(opportunity, true);
			AssertNotNull(collection.EntitySalesCollection);
			AssertEquals(opportunity, collection.EntitySalesCollection.Entity);

			opportunity.P8_OH = ZGuid.Empty;
			AssertNull(collection.EntitySalesCollection);
		}
	}
}
