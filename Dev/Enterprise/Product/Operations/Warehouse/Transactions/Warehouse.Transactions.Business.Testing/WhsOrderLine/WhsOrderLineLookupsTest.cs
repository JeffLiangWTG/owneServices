using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderLineLookupsTest : WhsPickableDocketLineLookupsTest<WhsOrder, WhsOrderLine>
	{
		#region TestSupplierParts

		public void TestSupplierParts()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("1");
			OrgHeader org = helper.CreateClient();
			OrgSupplierPart prod1 = helper.CreateProduct(org, "P1");
			OrgSupplierPart prod2 = helper.CreateProduct(org, "P2");
			prod2.OP_CanResell = false;
			WhsOrder order = helper.CreateWhsOrder(org, whs);
			WhsOrderLine line1 = helper.CreateWhsOrderLine(order, prod1, 10);
			WhsOrderLine line2 = helper.CreateWhsOrderLine(order, prod2, 20);

			Factory.Save();
			OrgSupplierPartCollection parts = line1.Lookups.SupplierParts;
			ZQuery additionalFilter = ((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter();
			parts.Load(additionalFilter);
			AssertCollectionContains(prod1, parts);

			parts = line2.Lookups.SupplierParts;
			additionalFilter = ((IBusinessObjectCollectionTestingMembers)parts).GetAdditionalFilter();
			parts.Load(additionalFilter);
			AssertCollectionNotContains(prod2, parts);
		}

		#endregion
	}
}
