using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDynamicWorkOrderLineLookupsTest : WhsComponentOrderLineLookupsTest<WhsDynamicWorkOrder, WhsDynamicWorkOrderLine>
	{
		public void TestSupplierParts_CalculatedOffChildComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.Delete();
			data.Part2.Delete();

			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = Helper.CreateProduct("JUICE", data.Org1);
			var secondaryProduct = Helper.CreateProduct("PEEL", data.Org1);
			var componentProduct1 = Helper.CreateProduct("ORANGE", data.Org1);
			var componentProduct2 = Helper.CreateProduct("WATER", data.Org1);
			var componentProduct3 = Helper.CreateProduct("POTASSIUMBENZOATE", data.Org1);
			var invalidProduct = Helper.CreateProduct("FROGURT", data.Org1);
			var allProducts = new[] { mainProduct, secondaryProduct, componentProduct1, componentProduct2, componentProduct3, invalidProduct };
			Factory.Save();

			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "JUICE LOOSENER");
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 1m);
			workOrderLineMain.IsMainInwardProcessedItem = true;
			AssertSupplierPartLookups("Main product lines should show all products.", allProducts, workOrderLineMain);

			var workOrderLineMainComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 1m);
			workOrderLineMainComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertSupplierPartLookups("Main component lines should show all products.", allProducts, workOrderLineMainComponent1);

			var workOrderLineSecondary = Helper.CreateWhsDynamicWorkOrderLine(workOrder, secondaryProduct, 1m);
			workOrderLineSecondary.IsSecondaryInwardProcessedItem = true;
			AssertSupplierPartLookups("Secondary product lines should show all products.", allProducts, workOrderLineSecondary);

			var workOrderLineSecondaryComponent1 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct1, 0.1m);
			workOrderLineSecondaryComponent1.WE_WE_ParentDocketLine = workOrderLineSecondary.PK;
			AssertSupplierPartLookups("Secondary component lines should show components off the main line.", new[] { componentProduct1 }, workOrderLineSecondaryComponent1);

			var workOrderLineMainComponent2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct2, 2m);
			workOrderLineMainComponent2.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertSupplierPartLookups("Secondary component lines should show components off the main line.", new[] { componentProduct1, componentProduct2 }, workOrderLineSecondaryComponent1);

			var workOrderLineMainComponent3 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, componentProduct3, 10m);
			workOrderLineMainComponent3.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertSupplierPartLookups("Secondary component lines should show components off the main line.", new[] { componentProduct1, componentProduct2, componentProduct3 }, workOrderLineSecondaryComponent1);

			// Dodgy line with no product
			workOrderLineMainComponent3.WE_OP = ZGuid.Empty;
			AssertSupplierPartLookups("Secondary component lines should show components off the main line.", new[] { componentProduct1, componentProduct2 }, workOrderLineSecondaryComponent1);

			// Line doesn't have docket set yet, go via parent line which will be set
			workOrderLineSecondaryComponent1.WE_WD = ZGuid.Empty;
			AssertSupplierPartLookups("Secondary component lines should show components off the main line.", new[] { componentProduct1, componentProduct2 }, workOrderLineSecondaryComponent1);

			var workOrderLineMain2 = Helper.CreateWhsDynamicWorkOrderLine(workOrder, mainProduct, 1m);
			workOrderLineMain2.IsMainInwardProcessedItem = true;
			AssertSupplierPartLookups("Should be robust to second main product existing.", new[] { componentProduct1, componentProduct2 }, workOrderLineSecondaryComponent1);

			void AssertSupplierPartLookups(string message, IEnumerable<OrgSupplierPart> supplierParts, WhsDynamicWorkOrderLine workOrderLine)
			{
				workOrderLine.Lookups.SupplierParts.Load();
				AssertContainsExactElementsInAnyOrder(message, supplierParts.Select(p => p.OP_PartNum), workOrderLine.Lookups.SupplierParts.Select(p => p.OP_PartNum));
			}
		}
	}
}
