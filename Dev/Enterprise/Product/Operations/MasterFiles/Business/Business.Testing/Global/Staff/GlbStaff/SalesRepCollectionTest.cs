using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesRepCollection))]
	sealed class SalesRepCollectionTest : ActiveBusinessObjectCollectionTestCase<SalesRepCollection>
	{
		#region Filter

		public void TestRelationshipFilter()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_IsResource = true;
			resource.GS_IsSalesRep = true;

			var nonSalesRep = Factory.NewWithValidTestData<GlbStaff>();
			nonSalesRep.GS_IsSalesRep = false;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_IsSalesRep = true;

			Factory.Save();

			var collection = new SalesRepCollection(Factory);
			AssertCollectionContains(salesRep, collection);
			AssertCollectionNotContains(nonSalesRep, collection);
			AssertCollectionNotContains(resource, collection);
		}

		#endregion

		#region New

		public void TestNew()
		{
			var collection = new SalesRepCollection(Factory);
			var newItem = collection.AddNew();

			AssertEquals(true, newItem.GS_IsSalesRep);
		}

		#endregion

		#region Filter Business Object

		public void TestAddFilterBusinessObjectDefaults()
		{
			var collection = new SalesRepCollection(Factory);
			var salesRepDefault = collection.FilterBusinessObjectDefaults["Sales Rep:Property"];
			AssertNotNull(salesRepDefault);
			AssertEquals("SALES", salesRepDefault.Value);
			AssertEquals(false, salesRepDefault.IsRemovable);
		}

		#endregion
	}
}
