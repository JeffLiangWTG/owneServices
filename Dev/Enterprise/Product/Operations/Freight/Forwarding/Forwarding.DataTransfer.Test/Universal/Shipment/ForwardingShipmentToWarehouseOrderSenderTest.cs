using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ForwardingShipmentToWarehouseOrderSenderTest : TestCaseWithFactory
	{
		#region TestUpdatePackLinesFromWarehouseOrderThrowsExceptionIfNotWarehouseOrder

		public void TestUpdatePackLinesFromWarehouseOrderThrowsExceptionIfNotWarehouseOrder()
		{
			AssertExceptionThrown(typeof(InvalidOperationException),
				() => ForwardingShipmentToWarehouseOrderSender.UpdatePackLinesFromWarehouseOrder(Factory.New<ForwardingShipment>(), Factory.New<ForwardingShipment>()));
		}

		#endregion

		#region TestUpdatePackLinesFromWarehouseOrderUsesPacklinesOverReleaseLines

		public void TestUpdatePackLinesFromWarehouseOrderUsesPacklinesOverReleaseLines()
		{
			var warehouseHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = warehouseHelper.CreateWarehouse("WHS", "A");
			var client = warehouseHelper.CreateClient("SAM");
			var product = warehouseHelper.CreateProduct(client, "BOWLHAT");
			var stock = warehouseHelper.CreateStock(warehouse.PK, client, product.PK, 20m);

			Factory.Save();

			var warehouseOrder = warehouseHelper.CreateWhsOrder(client, warehouse.PK, "ORD1", null);
			var orderLine = warehouseHelper.CreateWhsOrderLine(warehouseOrder, product.PK, 13.2m);
			var pick = warehouseHelper.CreateWhsPick(new[] { warehouseOrder });
			warehouseHelper.WhsPickAllocationItems(pick);

			var warehouseOrderBO = (BusinessObject)Factory.Load<IWhsOrder>(warehouseOrder);
			warehouseOrderBO[WhsDocketSchema.WD_DocketID] = "W001";

			var packageJob = (BusinessObject)Factory.LoadTop1<IPkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, warehouseOrder));
			var packHelper = ObjectFactory.New<IPkgPackageTestHelper>(Factory);
			packHelper.CreatePackage(packageJob.PK, "ABC", 1, "PLT", 14.1m, "M3", 13.1m, "G");

			var shipment = Factory.New<ForwardingShipment>();

			Factory.Save();

			AssertEquals("Precondition", 0, shipment.OuterPackLines.Count);
			ForwardingShipmentToWarehouseOrderSender.UpdatePackLinesFromWarehouseOrder(shipment, warehouseOrderBO);
			AssertEquals(1, shipment.OuterPackLines.Count);

			var packLine = shipment.OuterPackLines[0];
			AssertEquals(1, packLine.JL_PackageCount);
			AssertEquals("ABC", packLine.JL_RefNumber);
			AssertEquals("PLT", packLine.JL_F3_NKPackType);
			AssertEquals(13.1m, packLine.JL_ActualWeight);
			AssertEquals("G", packLine.JL_ActualWeightUQ);
			AssertEquals(14.1m, packLine.JL_ActualVolume);
			AssertEquals("M3", packLine.JL_ActualVolumeUQ);
		}

		#endregion

		#region TestUpdatePackLinesFromWarehouseOrder

		public void TestUpdatePackLinesFromWarehouseOrder()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var client = helper.CreateClient("SAM");
			var product = helper.CreateProduct(client, "BOWLHAT");
			var stock = helper.CreateStock(warehouse.PK, client, product.PK, 20m);

			Factory.Save();

			var warehouseOrder = helper.CreateWhsOrder(client, warehouse.PK, "ORD1", null);
			var orderLine = helper.CreateWhsOrderLine(warehouseOrder, product.PK, 13.2m);
			var pick = helper.CreateWhsPick(new[] { warehouseOrder });
			helper.WhsPickAllocationItems(pick);

			var warehouseOrderBO = (BusinessObject)Factory.Load<IWhsOrder>(warehouseOrder);
			warehouseOrderBO[WhsDocketSchema.WD_DocketID] = "W001";

			var shipment = Factory.New<ForwardingShipment>();

			Factory.Save();

			AssertEquals("Precondition", 0, shipment.OuterPackLines.Count);
			ForwardingShipmentToWarehouseOrderSender.UpdatePackLinesFromWarehouseOrder(shipment, warehouseOrderBO);
			AssertEquals(1, shipment.OuterPackLines.Count);

			var packLine = shipment.OuterPackLines[0];
			AssertEquals(13, packLine.JL_PackageCount);
			AssertEquals("UNT", packLine.JL_F3_NKPackType);
			AssertEquals(26.4m, packLine.JL_ActualWeight);
			AssertEquals("KG", packLine.JL_ActualWeightUQ);
			AssertEquals("BOWLHAT", packLine.JL_Description);
		}

		public void TestUpdatePackLinesFromWarehouseOrder_WithContainers()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var client = helper.CreateClient("SAM");
			var product = helper.CreateProduct(client, "BOWLHAT");
			var stock = helper.CreateStock(warehouse.PK, client, product.PK, 20m);

			Factory.Save();

			var warehouseOrder = helper.CreateWhsOrder(client, warehouse.PK, "ORD1", null);
			var orderLine = helper.CreateWhsOrderLine(warehouseOrder, product.PK, 13.2m);
			var pick = helper.CreateWhsPick(new[] { warehouseOrder });
			helper.WhsPickAllocationItems(pick);

			var warehouseOrderBO = (BusinessObject)Factory.Load<IWhsOrder>(warehouseOrder);
			warehouseOrderBO[WhsDocketSchema.WD_DocketID] = "W001";

			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER1";
			container.JC_RC = refContainer.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);

			Factory.Save();

			ForwardingShipmentToWarehouseOrderSender.UpdatePackLinesFromWarehouseOrder(shipment, warehouseOrderBO);
			AssertEquals(2, shipment.OuterPackLines.Count);

			AssertEquals(packLine.PK, shipment.OuterPackLines[0].PK);

			var packLine2 = shipment.OuterPackLines[1];
			AssertEquals(13, packLine2.JL_PackageCount);
			AssertEquals("UNT", packLine2.JL_F3_NKPackType);
			AssertEquals(26.4m, packLine2.JL_ActualWeight);
			AssertEquals("KG", packLine2.JL_ActualWeightUQ);
			AssertEquals("BOWLHAT", packLine2.JL_Description);
			AssertEquals(container, packLine2.GetContainer(consol));
		}

		#endregion

		#region TestCreateWarehouseOrderFromForwardingShipment

		public void TestCreateWarehouseOrderFromForwardingShipment()
		{
			var publishResult1 = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(null);
			AssertNull(publishResult1.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult1.ResultType);
			AssertEquals(@"Failed to create Warehouse Order:
Null Entity", publishResult1.ErrorMessage);

			var shipment = Factory.New<ForwardingShipment>();
			var publishResult2 = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(shipment);
			AssertNull(publishResult2.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult2.ResultType);
			AssertEquals("errorMessage", @"Failed to create Warehouse Order:
Error - Cannot Import Order
No Client Address was provided.
No Warehouse was provided.".Trim(), publishResult2.ErrorMessage);

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			helper.CreateWarehouse("WHS", "A");
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsWarehouseClient = true;
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			AssertEquals("Precondition", 0, shipment.AttachedWarehouseOrders.Count);
			var publishResult3 = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(shipment);
			var order = publishResult3.FindJobIfExists();
			AssertNotNull(order);
			AssertEquals(UniversalResult.Internal, publishResult3.ResultType);
			AssertEquals("", publishResult3.ErrorMessage);
			AssertEquals("W00000001", order[WhsDocketSchema.WD_DocketID]);

			var shipmentInOtherFactory = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			AssertContainsExactElementsInAnyOrder("Make sure Warehouse Order correctly added to shipment.", new[] { order.PK }, shipmentInOtherFactory.AttachedWarehouseOrders.ToArray().Select(o => o.PK));
		}

		public void TestCreateWarehouseOrderFromForwardingShipment_DefaultWarehouse_WarehouseAddressOnForwardingShipment()
		{
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WHS", "A");
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsWarehouseClient = true;
			shipment.ConsignorPK = consignor.PK;
			var whsAddress = shipment.DocAddresses.CreateWithAddressType(DocAddressType.Warehouse);
			whsAddress.E2_OA_Address = whs.WW_OA_WarehouseAddress;
			Factory.Save();

			helper.CreateWarehouse("ABC", "B");
			helper.CreateWarehouse("ZZZ", "C");
			Factory.Save();

			var publishResult = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(shipment);
			var order = (IWhsOrder)publishResult.FindJobIfExists();

			AssertNotNull(order);
			AssertEquals("Order's warehouse should default to the warehouse on the forwarding shipment, when it exists.", whs.PK, order.WD_WW_Whs);
		}

		#endregion

		#region TestCreateWarehouseOrderFromShipmentWithConsol

		public void TestCreateWarehouseOrderFromShipmentsAndSubshipmentsInConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			var subShipment = Factory.New<ForwardingShipment>();
			var bottomLevelSubShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = shipment.PK;
			bottomLevelSubShipment.JS_JS_ColoadMasterShipment = subShipment.PK;

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsWarehouseClient = true;
			bottomLevelSubShipment.ConsignorPK = consignor.PK;

			Factory.Save();

			var publishResult = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(bottomLevelSubShipment);
			AssertEquals("Precondition", "", publishResult.ErrorMessage);
			var order = publishResult.FindJobIfExists();
			AssertNotNull(order);
			AssertEquals(UniversalResult.Internal, publishResult.ResultType);
			AssertEquals("W00000001", order[WhsDocketSchema.WD_DocketID]);
			AssertEquals(warehouse.PK, order[WhsDocketSchema.WD_WW_Whs]);

			var shipmentInOtherFactory = new BusinessObjectFactory().Load<ForwardingShipment>(bottomLevelSubShipment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { order.PK }, shipmentInOtherFactory.AttachedWarehouseOrders.ToArray().Select(o => o.PK));
		}

		#endregion

		#region TestCreateWarehouseOrderFromSubShipments

		public void TestCreateWarehouseOrderFromSubShipmentsWithinShipments()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			var subShipment = Factory.New<ForwardingShipment>();
			var bottomLevelSubShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			bottomLevelSubShipment.JS_JS_ColoadMasterShipment = subShipment.PK;
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsWarehouseClient = true;
			bottomLevelSubShipment.ConsignorPK = consignor.PK;

			Factory.Save();

			var publishResult = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(bottomLevelSubShipment);
			AssertEquals("Precondition", "", publishResult.ErrorMessage);
			var order = publishResult.FindJobIfExists();
			AssertNotNull(order);
			AssertEquals(UniversalResult.Internal, publishResult.ResultType);
			AssertEquals("W00000001", order[WhsDocketSchema.WD_DocketID]);
			AssertEquals(warehouse.PK, order[WhsDocketSchema.WD_WW_Whs]);

			var shipmentInOtherFactory = new BusinessObjectFactory().Load<ForwardingShipment>(bottomLevelSubShipment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { order.PK }, shipmentInOtherFactory.AttachedWarehouseOrders.ToArray().Select(o => o.PK));
		}

		#endregion
	}
}
