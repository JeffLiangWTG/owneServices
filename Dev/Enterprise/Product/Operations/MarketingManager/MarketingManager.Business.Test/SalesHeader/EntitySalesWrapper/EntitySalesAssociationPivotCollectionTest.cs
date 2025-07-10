using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EntitySalesAssociationPivotCollection))]
	sealed class EntitySalesAssociationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<EntitySalesAssociationPivotCollection>
	{
		public void TestRelationship()
		{
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var sales = org.SalesCollection.AddNew();
			var opp1 = org.SalesOpportunities.AddNew();
			var opp2 = org.SalesOpportunities.AddNew();
			var opp3 = org.SalesOpportunities.AddNew();
			opp3.P8_GC = anotherCompany.PK;
			var opp1SalesPivot = opp1.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var opp2SalesPivot = opp2.AssociatedTradeLanesPivots.AddPivotFor(sales);
			var opp3SalesPivot = opp3.AssociatedTradeLanesPivots.AddPivotFor(sales);

			Factory.Save();

			var entitySales = EntitySalesWrapper.Get(sales, org);
			entitySales.CompanyFilter = Env.CurrentCompany.PK;

			var collection = new EntitySalesAssociationPivotCollection(entitySales);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { opp1SalesPivot, opp2SalesPivot }, collection);

			entitySales.CompanyFilter = anotherCompany.PK;
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { opp3SalesPivot }, collection);

			entitySales.CompanyFilter = ZGuid.Empty;
			collection.RefreshFromDb();
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesValueAssociationPivot>.PKOnlyComparer, new[] { opp1SalesPivot, opp2SalesPivot, opp3SalesPivot }, collection);
		}

		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}

		protected override EntitySalesAssociationPivotCollection GetCollectionToTest()
		{
			var org = Factory.New<OrgHeader>();
			var sales = Factory.New<OrgSales>();
			var product = Factory.New<OrgSalesProduct>();
			var entitySales = EntitySalesWrapper.Get(sales, org);
			return new EntitySalesAssociationPivotCollection(entitySales);
		}
	}
}
