using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsAsnLineLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Test Cases

		public void TestProductUQList()
		{
			AssertNotNull(Lookups.ProductUQList);
			Assert(Lookups.ProductUQList is CodeDescriptionPairList);
		}

		public void TestSupplierParts()
		{
			AssertNotNull(Lookups.SupplierParts);
			AssertEquals(typeof(WhsOrgSupplierPartCollection), Lookups.SupplierParts.GetType());
			AssertEquals("Collection should not be loaded", 0, Lookups.SupplierParts.Count);
			Line.Docket.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, Lookups.SupplierParts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Line = Factory.New<WhsAsnLine>();
			Line.WN_WD = Factory.New<WhsReceive>().PK;
			Lookups = new WhsAsnLineLookups(Line);
		}

		WhsAsnLineLookups Lookups;
		WhsAsnLine Line;

		#endregion
	}
}
