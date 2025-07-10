using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	abstract class PickingOrReleaseFilterBusinessObjectTestCase<T> : DocketFilterBusinessObjectTest<T, WhsPickableDocket>
		where T : PickingFilterBusinessObject, new()
	{
		#region TestFilterCustomLineAttributes

		public virtual void TestFilterCustomLineAttributes()
		{
			SetupTestData();
			SetupTestLineData();

			Line111.WE_CustomAttrib1 = "CA111";
			Line112.WE_CustomAttrib1 = "CA112";
			Line121.WE_CustomAttrib1 = "CA121";
			Line122.WE_CustomAttrib1 = "CA122";
			Line123.WE_CustomAttrib1 = "CA123";
			Line211.WE_CustomAttrib1 = "CA211";
			Line212.WE_CustomAttrib1 = "CA212";
			Line213.WE_CustomAttrib1 = "CA213";
			Line221.WE_CustomAttrib1 = "CA221";
			Line222.WE_CustomAttrib1 = "CA222";

			Line111.WE_CustomAttrib4 = "CA411";
			Line112.WE_CustomAttrib4 = "CA412";
			Line121.WE_CustomAttrib4 = "CA421";
			Line122.WE_CustomAttrib4 = "CA422";
			Line123.WE_CustomAttrib4 = "CA423";
			Line211.WE_CustomAttrib4 = "CA411";
			Line212.WE_CustomAttrib4 = "CA412";
			Line213.WE_CustomAttrib4 = "CA413";
			Line221.WE_CustomAttrib4 = "CA421";
			Line222.WE_CustomAttrib4 = "CA422";

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocketLine.CustomAttrib1", (ZString)"CA1");
			PickAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocketLine.CustomAttrib4", (ZString)"CA421");
			PickAssert(false, true, false, false);

			DocketFilter = null;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocketLine.CustomAttrib4", (ZString)"CA421");
			PickAssert(false, true, false, true);

			var receive1 = Helper.CreateWhsReceive(Org2, Whs1, "1", Helper.Notify);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive1, Part21, 100m);
			inv.WI_CustomAttrib4 = "CA421";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_TransactionLine = Line211.PK;
			pickLine.WZ_WE_InventoryLine = inv.WI_WE_InDocketLine;
			pickLine.WZ_Units = 10m;

			Factory.Save();

			PickAssert(false, true, true, true);
		}

		#endregion

		#region TestFilterPartAttributes

		public virtual void TestFilterPartAttributes()
		{
			SetupTestData();
			SetupTestLineData();

			SetClientAttributesType(Org1, true, false, true, false, false);
			SetClientAttributesType(Org2, true, false, true, false, false);
			SetProductAttributesUse(Org1, Part, true, false, true, false, false);
			SetProductAttributesUse(Org1, Part11, true, false, true, false, false);
			SetProductAttributesUse(Org1, Part12, true, false, true, false, false);
			SetProductAttributesUse(Org2, Part21, true, false, true, false, false);
			SetProductAttributesUse(Org2, Part22, true, false, true, false, false);

			Line111.WE_PartAttrib1 = "CA111";
			Line112.WE_PartAttrib1 = "CA112";
			Line121.WE_PartAttrib1 = "CA121";
			Line122.WE_PartAttrib1 = "CA122";
			Line123.WE_PartAttrib1 = "CA123";
			Line211.WE_PartAttrib1 = "CA211";
			Line212.WE_PartAttrib1 = "CA212";
			Line213.WE_PartAttrib1 = "CA213";
			Line221.WE_PartAttrib1 = "CA221";
			Line222.WE_PartAttrib1 = "CA222";

			Line111.WE_PartAttrib3 = "CA411";
			Line112.WE_PartAttrib3 = "CA412";
			Line121.WE_PartAttrib3 = "CA421";
			Line122.WE_PartAttrib3 = "CA422";
			Line123.WE_PartAttrib3 = "CA423";
			Line211.WE_PartAttrib3 = "CA411";
			Line212.WE_PartAttrib3 = "CA412";
			Line213.WE_PartAttrib3 = "CA413";
			Line221.WE_PartAttrib3 = "CA421";
			Line222.WE_PartAttrib3 = "CA422";

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 1", (ZString)"CA1");
			PickAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3", (ZString)"CA421");
			PickAssert(false, true, false, false);

			DocketFilter = null;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3", (ZString)"CA421");
			PickAssert(false, true, false, true);

			var receive1 = Helper.CreateWhsReceive(Org2, Whs1, "1", Helper.Notify);
			var inv = Helper.CreateWhsReceiveInventoryLine(receive1, Part21, 100m);
			inv.WI_PartAttrib3 = "CA421";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_WE_TransactionLine = Line211.PK;
			pickLine.WZ_WE_InventoryLine = inv.WI_WE_InDocketLine;
			pickLine.WZ_Units = 10m;

			Factory.Save();

			PickAssert(false, true, true, true);
		}

		#endregion

		#region TestFilterPartAttributes_ExcludesZeroUnitPickLines

		public void TestFilterPartAttributes_ExcludesZeroUnitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();
			Asserter.AddToScope(pick);

			var filter = (ModuleTextFilter)FilterStripBizO["Part Attribute 1"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "PA1";
			Asserter.AssertMatches("Should not Consider Zero Unit Pick Lines.", filter);
		}

		#endregion

		#region TestFilterWarehouse

		public override void TestFilterWarehouse()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestFilterProduct

		public override void TestFilterProduct()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestFilterProductCategory

		public override void TestFilterProductCategory()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestFilterCommodityCode

		public override void TestFilterCommodityCode()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestFilterEntryKey

		public override void TestFilterEntryKey()
		{
			Assert("incomplete test", true);
		}
		#endregion

		#region TestFilterDocketID

		public override void TestFilterDocketID()
		{
			Assert("picks/releases don't have docket id", true);
		}

		#endregion

		#region TestFilterPickNumber

		public virtual void TestFilterPickNumber()
		{
			SetupTestData();

			Factory.Save();
			Pick11.WP_PickNo = "P00000001";
			Pick12.WP_PickNo = "P00000331";
			Pick21.WP_PickNo = "P00000444";
			Pick22.WP_PickNo = "P00000252";

			Pick21.WP_PickStatus = CodeLists.PickStatus.Codes.Created;
			Pick22.WP_PickStatus = CodeLists.PickStatus.Codes.PickSlip;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"P00000001");
			PickAssert(true, false, false, false);

			DocketFilter = null;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"P00000444");
			PickAssert(false, false, true, false);

			DocketFilter = null;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"000331");
			PickAssert(false, true, false, false);

			DocketFilter = null;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Status", (ZString)CodeLists.PickStatus.Codes.Created);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"252");
			PickAssert(false, false, false, true);
		}

		#endregion

		#region TestFilterCarrierServiceLevel

		public void TestFilterCarrierServiceLevel()
		{
			SetupTestData();

			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			var serviceLevel1 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "TS1";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Service 1";
			var serviceLevel2 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "TS2";
			serviceLevel2.PL_CarrierServiceLevelDescription = "Service 2";

			Docket12.WD_PL_NKCarrierServiceLevel = "TS1";
			Docket21.WD_PL_NKCarrierServiceLevel = "TS2";
			Docket22.WD_PL_NKCarrierServiceLevel = "TS2";
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", ZString.Empty);
			PickAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", new ZString("TS1"));
			PickAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", new ZString("TS2"));
			PickAssert(false, false, true, true);
		}

		#endregion

		#region TestFilterConsignee

		public void TestFilterConsignee()
		{
			SetupTestData();

			((WhsPickableDocket)Docket11).ConsigneePK = ZGuid.Empty;
			((WhsPickableDocket)Docket12).ConsigneePK = Org2.PK;
			((WhsPickableDocket)Docket21).ConsigneePK = Org1.PK;
			((WhsPickableDocket)Docket22).ConsigneePK = Org1.PK;
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Consignee", ZGuid.Empty);
			PickAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Consignee", Org2.PK);
			PickAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Consignee", Org1.PK);
			PickAssert(false, false, true, true);
		}

		public void TestFilterConsignee_Order()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consignee1 = Helper.CreateClient("CNE1");
			var consignee2 = Helper.CreateClient("CNE2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);

			Factory.Save();

			order1.ConsigneePK = consignee1.PK;
			order2.ConsigneePK = consignee2.PK;

			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			filter.Property = ZGuid.Empty;
			var pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			filter.Property = ZGuid.Empty;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);
		}

		public void TestFilterConsignee_WorkOrder() => TestFilterConsignee_WorkOrderCore();

		protected virtual void TestFilterConsignee_WorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			var consignee2 = Helper.CreateClient("CNE2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			workOrder3.ConsigneeAddressPK = ZGuid.Empty;

			Factory.Save();

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			filter.Property = ZGuid.Empty;
			var pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			filter.Property = ZGuid.Empty;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);
		}

		public void TestFilterConsignee_WorkOrder_NotWronglyFilterByOtherAddressType() => TestFilterConsignee_WorkOrder_NotWronglyFilterByOtherAddressTypeCore();

		protected virtual void TestFilterConsignee_WorkOrder_NotWronglyFilterByOtherAddressTypeCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = Helper.CreateProduct(data.Org1, "MainP");
			var componentProduct = Helper.CreateProduct(data.Org1, "ComP");
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var orgHeader1 = Helper.CreateClient("OH1");
			var orgHeader2 = Helper.CreateClient("OH2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, orgHeader1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, orgHeader2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = CreateWhsWorkOrderWithTransportCo(data.Org1, orgHeader2, data.Whs1, "OR3", mainProduct);
			workOrder3.ConsigneeAddressPK = ZGuid.Empty;

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			filter.Property = ZGuid.Empty;
			var pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			filter.Property = orgHeader1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);

			filter.Property = orgHeader2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			filter.Property = ZGuid.Empty;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			filter.Property = orgHeader1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);

			filter.Property = orgHeader2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);
		}

		WhsWorkOrder CreateWhsWorkOrderWithConsignee(OrgHeader client, OrgHeader consignee, WhsWarehouse whs, ZString externalReference, OrgSupplierPart part)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, externalReference, part, 1m);
			order.ConsigneePK = consignee.PK;
			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());

			var workOrder = order.CurrentWorkOrders.Single();
			AssertEquals(workOrder.ConsigneeAddressPK, consignee.MainAddress.PK);

			return workOrder;
		}

		WhsWorkOrder CreateWhsWorkOrderWithTransportCo(OrgHeader client, OrgHeader transportCo, WhsWarehouse whs, ZString externalReference, OrgSupplierPart part)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, externalReference, part, 1m);
			order.TransportCoPK = transportCo.PK;
			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());

			var workOrder = order.CurrentWorkOrders.Single();
			AssertEquals(workOrder.TransportCoDocAddress.E2_OA_Address, transportCo.MainAddress.PK);

			return workOrder;
		}

		public void TestFilterConsignee_OrderAndWorkOrder() => TestFilterConsignee_OrderAndWorkOrderCore();

		protected virtual void TestFilterConsignee_OrderAndWorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = Helper.CreateProduct(data.Org1, "MainP");
			var componentProduct = Helper.CreateProduct(data.Org1, "ComP");
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			var consignee2 = Helper.CreateClient("CNE2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);
			order1.ConsigneePK = consignee1.PK;
			order2.ConsigneePK = consignee2.PK;

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "WOR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "WOR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			workOrder3.ConsigneeAddressPK = ZGuid.Empty;
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var pick3 = Helper.CreatePickNew(workOrder1);
			var pick4 = Helper.CreatePickNew(workOrder2);
			var pick5 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			filter.Property = ZGuid.Empty;
			var pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3, pick4, pick5 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick3 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2, pick4 }, pickCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			filter.Property = ZGuid.Empty;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3, pick4, pick5 }, pickCollection);

			filter.Property = consignee1.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2, pick4 }, pickCollection);

			filter.Property = consignee2.PK;
			pickCollection = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick3 }, pickCollection);
		}

		#endregion

		#region FilterTransportCo

		protected override WhsPickableDocket GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, docketReference, data.Part1, 1m);
			if (transportCo != null)
			{
				order.TransportCoPK = transportCo.PK;
			}

			return order;
		}

		protected override void TestFilterTransportCoCore()
		{
			if (SupportsTransportCoFilters)
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");
				var transportCo3 = Helper.CreateClient("TRC3");

				Factory.Save();

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
				var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
				var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 3m);
				var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 3m);

				Factory.Save();

				order1.TransportCoPK = transportCo1.PK;
				order2.TransportCoPK = transportCo2.PK;
				order3.TransportCoPK = transportCo3.PK;
				order4.TransportCoPK = transportCo3.PK;
				order5.TransportCoPK = ZGuid.Empty;

				Factory.Save();

				var pick1 = Helper.CreatePickNew(order1);
				var pick2 = Helper.CreatePickNew(order2);
				var pick3 = Helper.CreatePickNew(order3);
				var pick4 = Helper.CreatePickNew(order4);
				var pick5 = Helper.CreatePickNew(order5);

				Factory.Save();

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;

				filter.Property = ZGuid.Empty;
				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 5 picks", 5, picks.Length);

				filter.Property = transportCo1.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick1", pick1, picks.Single());

				filter.Property = transportCo2.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick2", pick2, picks.Single());

				filter.Property = transportCo3.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 2 picks", 2, picks.Length);
				Assert("Should return pick3 and pick4", picks.All(p => p.PK == pick3.PK || p.PK == pick4.PK));
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co.", true);
			}
		}

		protected override void TestFilterTransportCo_NotEqualCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");
				var transportCo3 = Helper.CreateClient("TRC3");

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
				var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
				var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 3m);
				var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 3m);

				order1.TransportCoPK = transportCo1.PK;
				order2.TransportCoPK = transportCo2.PK;
				order3.TransportCoPK = transportCo3.PK;
				order4.TransportCoPK = transportCo3.PK;
				order5.TransportCoPK = ZGuid.Empty;

				var pick1 = Helper.CreatePickNew(order1);
				var pick2 = Helper.CreatePickNew(order2);
				var pick3 = Helper.CreatePickNew(order3);
				var pick4 = Helper.CreatePickNew(order4);
				var pick5 = Helper.CreatePickNew(order5);

				Factory.Save();

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.IsActive = true;

				filter.Property = ZGuid.Empty;
				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 5 picks", 5, picks.Length);

				filter.Property = transportCo1.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 3 pick", 3, picks.Length);
				Assert("Should return all picks except pick1 and pick5", picks.All(p => p.PK != pick1.PK && p.PK != pick5.PK));

				filter.Property = transportCo2.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 3 pick", 3, picks.Length);
				Assert("Should return all picks except pick2 and pick5", picks.All(p => p.PK != pick2.PK && p.PK != pick5.PK));

				filter.Property = transportCo3.PK;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 2 picks", 2, picks.Length);
				Assert("Should return pick1 and pick2", picks.All(p => p.PK == pick1.PK || p.PK == pick2.PK));
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		protected override void TestFilterTransportCo_IsBlankCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");
				var transportCo3 = Helper.CreateClient("TRC3");

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
				var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
				var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 3m);
				var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 3m);

				order1.TransportCoPK = transportCo1.PK;
				order2.TransportCoPK = transportCo2.PK;
				order3.TransportCoPK = transportCo3.PK;
				order4.TransportCoPK = transportCo3.PK;
				order5.TransportCoPK = ZGuid.Empty;

				var pick1 = Helper.CreatePickNew(order1);
				var pick2 = Helper.CreatePickNew(order2);
				var pick3 = Helper.CreatePickNew(order3);
				var pick4 = Helper.CreatePickNew(order4);
				var pick5 = Helper.CreatePickNew(order5);

				Factory.Save();

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				filter.IsActive = true;

				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick5", pick5, picks.Single());
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		protected override void TestFilterTransportCo_IsNotBlankCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");
				var transportCo3 = Helper.CreateClient("TRC3");

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
				var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
				var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 3m);
				var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 3m);

				order1.TransportCoPK = transportCo1.PK;
				order2.TransportCoPK = transportCo2.PK;
				order3.TransportCoPK = transportCo3.PK;
				order4.TransportCoPK = transportCo3.PK;
				order5.TransportCoPK = ZGuid.Empty;

				var pick1 = Helper.CreatePickNew(order1);
				var pick2 = Helper.CreatePickNew(order2);
				var pick3 = Helper.CreatePickNew(order3);
				var pick4 = Helper.CreatePickNew(order4);
				var pick5 = Helper.CreatePickNew(order5);

				Factory.Save();

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				filter.IsActive = true;

				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 4 picks", 4, picks.Length);
				Assert("Should return all picks excpet pick5", picks.All(p => p.PK != pick5.PK));
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		#endregion

		#region TestFilterPackageID

		protected override void TestPackageIDCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup 2 orders with a package on each

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 10m);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var order2Package = packageJob2.Packages.AddNew().Packages.AddNew("PLT"); // intentionally a child-Package
			order2Package.KP_PackageID = "o12";

			// pick the orders

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);

			Factory.Save(); // for dbonlyquery

			Asserter.AddToScope(pick1);
			Asserter.AddToScope(pick2);
			var filter = (ModuleTextFilter)FilterStripBizO["Package ID"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "o11";
			Asserter.AssertMatches("Equals 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, pick1);
			filter.Property = "o12";
			Asserter.AssertMatches("Equals 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Equals '1' should return no Orders (each has Package ID 'o11' and 'o12' respectively).", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "o11";
			Asserter.AssertMatches("Starts With 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, pick1);
			filter.Property = "o12";
			Asserter.AssertMatches("Starts With 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, pick2);
			filter.Property = "o";
			Asserter.AssertMatches("Starts With 'o' should return both Order1 and Order2 (each has Package ID 'o11' and 'o12' respectively).", filter, pick1, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Starts With '1' should return no Orders (each has Package ID 'o11' and 'o12' respectively).", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "o11";
			Asserter.AssertMatches("Contains 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, pick1);
			filter.Property = "o12";
			Asserter.AssertMatches("Contains 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Contains '1' should return both Order1 and Order2 (each has Package ID 'o11' and 'o12' respectively).", filter, pick1, pick2);
		}

		protected override bool SupportsPackageIdFilter => true;

		#endregion

		#region TestFilterHandlingUnit

		protected override void TestHandlingUnitCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 80m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1, order3);
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit1 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit2 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "HU2", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, handlingUnitPackage2);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(order3.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit3 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob3 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit3);
			var handlingUnitPackage3 = PackingHelper.CreatePackage(handlingUnitPackageJob3, "HU3", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage3, package3, handlingUnitPackage3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit 'HU1').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Equals 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Equals 'HU3' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit 'HU3').", filter, pick1);
			filter.Property = "H";
			Asserter.AssertMatches("Equals 'H' should return no Picks.", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1";
			Asserter.AssertMatches("Starts With 'HU1' should return only Pick1, (Pick1 contains Order1 which has a Package with Handling Unit starts with 'HU1').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Starts With 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit starts with 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Starts With 'HU4' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit starts with 'HU3').", filter, pick1);
			filter.Property = "HU";
			Asserter.AssertMatches("Starts With 'HU' should return Pick1 and Pick2 contains an Order which has a Package with Handling Unit starts with 'HU'.", filter, pick1, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Starts With '1' should return no Picks.", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1";
			Asserter.AssertMatches("Contains 'HU1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit contains 'HU1').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Contains 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit contains 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Contains 'HU3' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit contains 'HU3').", filter, pick1);
			filter.Property = "HU";
			Asserter.AssertMatches("Contains 'HU' should return Pick1 and Pick2 contains an Order which has a Package with Handling Unit contains 'HU'.", filter, pick1, pick2);
			filter.Property = "4";
			Asserter.AssertMatches("Contains 'r'  should return no Picks.", filter);
		}

		public void TestHandlingUnit_PackedInAnotherHandlingUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit1 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit2 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "HU2", 1, PkgUnit.Package);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(order3.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit3 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob3 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit3);
			var handlingUnitPackage3 = PackingHelper.CreatePackage(handlingUnitPackageJob3, "HU3", 1, PkgUnit.Package);

			var topHandlingUnit1 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var topHandlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(topHandlingUnit1);
			var topHandlingUnitPackage1 = PackingHelper.CreatePackage(topHandlingUnitPackageJob1, "TopHU1", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(topHandlingUnitPackage1, handlingUnitPackage1, topHandlingUnitPackage1);
			PackingHelper.PackHandlingUnit(topHandlingUnitPackage1, handlingUnitPackage3, topHandlingUnitPackage1);

			var topHandlingUnit2 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var topHandlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(topHandlingUnit2);
			var topHandlingUnitPackage2 = PackingHelper.CreatePackage(topHandlingUnitPackageJob2, "TopHU2", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(topHandlingUnitPackage2, handlingUnitPackage2, topHandlingUnitPackage2);

			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, topHandlingUnitPackage1);
			PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, topHandlingUnitPackage2);
			PackingHelper.PackHandlingUnit(handlingUnitPackage3, package3, topHandlingUnitPackage1);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit 'HU1').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Equals 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit 'HU2').", filter, pick2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Equals 'TopHU1' should return Pick1 and Pick3 contains an Order which has a Package with Handling Unit 'TopHU1'.", filter, pick1, pick3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Equals 'TopHU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit 'TopHU2').", filter, pick2);
			filter.Property = "H";
			Asserter.AssertMatches("Equals 'H' should return no Picks.", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1";
			Asserter.AssertMatches("Starts With 'HU1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit starts with 'HU1').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Starts With 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit starts with 'HU2').", filter, pick2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Starts With 'SubHU1' should return Pick1 and Pick3 contains an Order which has a Package with Handling Unit starts with 'SubHU1'.", filter, pick1, pick3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Starts With 'SubHU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit starts with 'SubHU2').", filter, pick2);
			filter.Property = "Top";
			Asserter.AssertMatches("Starts With 'Top' should return Pick1, Pick2 and Pick3 contains an Order which has a Package with Handling Unit starts with 'Top'.", filter, pick1, pick2, pick3);
			filter.Property = "U";
			Asserter.AssertMatches("Starts With 'U' should return no Picks.", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1";
			Asserter.AssertMatches("Contains 'HU1' should return Pick1 and Pick3 contains an Order which has a Package with Handling Unit contains 'HU1'.", filter, pick1, pick3);
			filter.Property = "HU2";
			Asserter.AssertMatches("Contains 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit contains 'HU2').", filter, pick2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Contains 'TopHU1' should return Pick1 and Pick3 contains an Order which has a Package with Handling Unit contains 'TopHU1'.", filter, pick1, pick3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Contains 'TopHU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit contains 'TopHU2').", filter, pick2);
			filter.Property = "Top";
			Asserter.AssertMatches("Contains 'Top' should return Pick1, Pick2 and Pick3 contains an Order which has a Package with Handling Unit contains 'Top'.", filter, pick1, pick2, pick3);
			filter.Property = "4";
			Asserter.AssertMatches("Contains '4' should return no Picks.", filter);
		}

		public void TestHandlingUnit_MultiplePackageInOneOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1, order3);
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1_1 = PackingHelper.CreatePackage(packageJob1, "PKG1_1", 1, PkgUnit.Box);
			package1_1.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			var package1_2 = PackingHelper.CreatePackage(packageJob1, "PKG1_2", 1, PkgUnit.Box);
			package1_2.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			var handlingUnit1_1 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1_1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1_1);
			var handlingUnitPackage1_1 = PackingHelper.CreatePackage(handlingUnitPackageJob1_1, "HU1_1", 1, PkgUnit.Package);
			var handlingUnit1_2 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1_2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1_2);
			var handlingUnitPackage1_2 = PackingHelper.CreatePackage(handlingUnitPackageJob1_2, "HU1_2", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage1_1, package1_1, handlingUnitPackage1_1);
			PackingHelper.PackHandlingUnit(handlingUnitPackage1_2, package1_2, handlingUnitPackage1_2);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit2 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob2 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit2);
			var handlingUnitPackage2 = PackingHelper.CreatePackage(handlingUnitPackageJob2, "HU2", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage2, package2, handlingUnitPackage2);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(order3.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit3 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob3 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit3);
			var handlingUnitPackage3 = PackingHelper.CreatePackage(handlingUnitPackageJob3, "HU3", 1, PkgUnit.Package);
			PackingHelper.PackHandlingUnit(handlingUnitPackage3, package3, handlingUnitPackage3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Equals 'HU1_1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit 'HU1_1').", filter, pick1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Equals 'HU1_2' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit 'HU1_2').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Equals 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Equals 'HU3' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit 'HU3').", filter, pick1);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Starts With 'HU1_1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit start with 'HU1_1').", filter, pick1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Starts With 'HU1_2' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit start with 'HU1_2').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Starts With 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Starts With 'HU3' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit start with 'HU3').", filter, pick1);
			filter.Property = "HU";
			Asserter.AssertMatches("Starts With 'HU' should return Pick1 and Pick2 which contains an Order which has a Package with Handling Unit start With 'HU'.", filter, pick1, pick2);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Contains 'HU1_1' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit contains 'HU1_1').", filter, pick1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Contains 'HU1_2' should return only Pick1 (Pick1 contains Order1 which has a Package with Handling Unit contains 'HU1_2').", filter, pick1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Contains 'HU2' should return only Pick2 (Pick2 contains Order2 which has a Package with Handling Unit contains 'HU2').", filter, pick2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Contains 'HU3' should return only Pick1 (Pick1 contains Order3 which has a Package with Handling Unit contains 'HU3').", filter, pick1);
			filter.Property = "HU";
			Asserter.AssertMatches("Contains 'HU' should return Pick1 and Pick2 which contains an Order which has a Package with Handling Unit contains 'HU'.", filter, pick1, pick2);
		}

		protected override bool SupportsHandlingUnitFilter => true;

		#endregion

		#region Test Filter Package Type

		protected override void TestPackageTypeFilter_ResultNoMatchCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var order2Package = packageJob2.Packages.AddNew("CRT");
			order2Package.KP_PackageID = "o12";

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);

			Factory.Save();

			Asserter.AddToScope(pick1);
			Asserter.AddToScope(pick2);
			var filter = (ModuleTextFilter)DocketFilter["Package Type"];

			filter.Property = "ABC";
			Asserter.AssertMatches("Should not return any picks for Package Type 'ABC'.", filter);
		}

		protected override void TestPackageTypeFilter_ResultPartialMatchCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var order2Package = packageJob2.Packages.AddNew("PLT");
			order2Package.KP_PackageID = "o12";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var order3Package = packageJob3.Packages.AddNew("CRT");
			order3Package.KP_PackageID = "o13";

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order3.IsAttachedToPickButNotFinalised);

			Factory.Save();

			Asserter.AddToScope(pick1);
			Asserter.AddToScope(pick2);
			Asserter.AddToScope(pick3);
			var filter = (ModuleTextFilter)DocketFilter["Package Type"];

			filter.Property = "CRT";
			Asserter.AssertMatches("Should return only pick3, which has a Package with type 'CRT'.", filter, pick3);

			filter.Property = "PLT";
			Asserter.AssertMatches("Should return only pick1 and pick2, which has a Package with type 'PLT'.", filter, pick1, pick2);
		}

		protected override bool SupportsPackageTypeFilter => true;

		#endregion

		#region TestFilterPalletID

		protected override void TestPalletIDCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");

			// setup receives
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE1", data.Part1, 10m, locationA1, "ID123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE2", data.Part2, 10m, locationA1, "ID456");
			Factory.Save();

			// setup 2 orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save(); // for dbonlyquery

			Asserter.AddToScope(pick1);
			Asserter.AddToScope(pick2);
			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ID123";
			Asserter.AssertMatches("Equals 'ID123' should return only pick1 (pick1 has a PalletID 'ID123').", filter, pick1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Equals 'ID456' should return only pick2 (pick2 has a PalletID 'ID456').", filter, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Equals '1' should return no Picks (each has PalletID 'ID123' and 'ID456' respectively).", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ID123";
			Asserter.AssertMatches("Starts With 'ID123' should return only pick1 (pick1 has a PalletID 'ID123').", filter, pick1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Starts With 'ID456' should return only pick2 (pick2 has a PalletID 'ID456').", filter, pick2);
			filter.Property = "I";
			Asserter.AssertMatches("Starts With 'I' should return both pick1 and pick2 (each has PalletID 'ID123' and 'ID456' respectively).", filter, pick1, pick2);
			filter.Property = "1";
			Asserter.AssertMatches("Starts With '1' should return no Picks (each has PalletID 'ID123' and 'ID456' respectively).", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ID123";
			Asserter.AssertMatches("Contains 'ID123' should return only pick1 (pick1 has a PalletID 'ID123').", filter, pick1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Contains 'ID456' should return only pick2 (pick2 has a PalletID 'ID456').", filter, pick2);
			filter.Property = "D";
			Asserter.AssertMatches("Contains 'D' should return both pick1 and pick2 (each has PalletID 'ID123' and 'ID456' respectively).", filter, pick1, pick2);

			// not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "ID123";
			Asserter.AssertMatches("Not contains 'ID123' should return only pick2 (pick1 has a PalletID 'ID123').", filter, pick2);
			filter.Property = "ID456";
			Asserter.AssertMatches("Not contains 'ID456' should return only pick1 (pick2 has a PalletID 'ID456').", filter, pick1);
			filter.Property = "D";
			Asserter.AssertMatches("Not contains 'D' should return no Picks (each has PalletID 'ID123' and 'ID456' respectively).", filter);
		}

		public void TestPalletID_PickedUsingInTransitTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.DefaultLocation, "PLT123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 35m, data.Whs1.DefaultLocation, "PLT456");
			Factory.Save();

			var orderForReceive1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var pickForReceive1 = Helper.CreatePickNew(orderForReceive1);

			var orderForReceive2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var pickForReceive2 = Helper.CreatePickNew(orderForReceive2);

			var shortOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pickForShortOrder = Helper.CreatePickNew(shortOrder);

			Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);

			foreach (var pickLine in pickForReceive1.GetAllPickLines().ToArray())
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_PalletID = transferLine.WE_PalletID.Replace("PLT", "TFR");
			}

			foreach (var pickLine in pickForReceive2.GetAllPickLines().ToArray())
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_PalletID = transferLine.WE_PalletID.Replace("PLT", "TFR");
			}

			Factory.Save();

			Asserter.AddToScope(pickForReceive1, pickForReceive2, pickForShortOrder);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "TFR";
			Asserter.AssertMatches("Should not find based on the transfer reference", filter);

			filter.Property = "PLT";
			Asserter.AssertMatches("Should find both picks with stock allocated from receives.", filter, pickForReceive1, pickForReceive2);

			filter.Property = "PLT123";
			Asserter.AssertMatches("Should find pick1.", filter, pickForReceive1);
		}

		protected new FilterStripAsserter<WhsPick> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsPick>(Factory, (p) => p.WP_PickNo));
		FilterStripAsserter<WhsPick> asserter;

		#endregion

		#region TestFilterReceiveReferencePickSlip

		public void TestFilterReceiveReferencePickSlip()
		{
			SetupTestData();
			SetupTestLineData();

			var receive = Helper.CreateWhsReceive(Org1, Whs1, "RECREF1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, Part11, 10m, Whs1.DefaultLocation);
			receive.RunPreSaveValidation(); // to generate docket line
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = 1m;
			pickLine.WZ_WE_TransactionLine = Line111.PK;
			pickLine.WZ_WE_InventoryLine = inv.WI_WE_InDocketLine;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Receive Reference (Pick Slip)", (ZString)"REC");
			PickAssert(true, false, false, false);
		}

		#endregion

		#region TestFilterReceiveReferencePickSlip_NotPhysicallyPicked

		public void TestFilterReceiveReferencePickSlip_NotPhysicallyPicked()
		{
			TestFilterReceiveReferencePickSlip_Core(pickUsingInTransitTransfer: false);
		}

		public void TestFilterReceiveReferencePickSlip_PhysicallyPickedUsingInTransitTransfer()
		{
			TestFilterReceiveReferencePickSlip_Core(pickUsingInTransitTransfer: true);
		}

		public void TestFilterReceiveReferencePickSlip_Core(bool pickUsingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 35m);
			Factory.Save();

			var orderForReceive1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var pickForReceive1 = Helper.CreatePickNew(orderForReceive1);

			var orderForReceive2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var pickForReceive2 = Helper.CreatePickNew(orderForReceive2);

			var shortOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pickForShortOrder2 = Helper.CreatePickNew(shortOrder);

			Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);

			if (pickUsingInTransitTransfer)
			{
				foreach (var pickLine in pickForReceive1.GetAllPickLines().ToArray())
				{
					var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
					transferLine.Docket.WD_ExternalReference = "T1";
				}

				foreach (var pickLine in pickForReceive2.GetAllPickLines().ToArray())
				{
					var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
					transferLine.Docket.WD_ExternalReference = "T2";
				}
			}

			Factory.Save();

			Asserter.AddToScope(pickForReceive1, pickForReceive2, pickForShortOrder2);

			var filter = (ModuleTextFilter)FilterStripBizO["Receive Reference (Pick Slip)"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "O";
			Asserter.AssertMatches("Should not find based on the order reference", filter);

			filter.Property = "T";
			Asserter.AssertMatches("Should not find based on the transfer reference", filter);

			filter.Property = "R";
			Asserter.AssertMatches("Should find both picks with stock allocated from receives.", filter, pickForReceive1, pickForReceive2);

			filter.Property = "R1";
			Asserter.AssertMatches("Should find pick1.", filter, pickForReceive1);
		}

		#endregion

		#region TestFilterReceiveReferencePickSlip_IgnoresZeroUnitPickLines

		public void TestFilterReceiveReferencePickSlip_IgnoresZeroUnitPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();
			Asserter.AddToScope(pick);

			var filter = (ModuleTextFilter)FilterStripBizO["Receive Reference (Pick Slip)"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "R1";
			Asserter.AssertMatches("Should not Consider Zero Unit Pick Lines.", filter);
		}

		#endregion

		#region ExternalReferenceFieldName

		protected override ZString ExternalReferenceFieldName => "Order No.";

		#endregion

		#region TestPickStatuses

		public void TestPickStatuses()
		{
			AssertEquals(5, DocketFilter.DocketStatuses.IndexOfCode("OPN"));
		}

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = new PickStatus();
			status.AddPair("OPN", "Open - Not Finalized or Canceled");
			return status;
		}

		protected override bool SupportsDocketStatus => false;

		#endregion

		#region TestPickTypes

		public void TestPickTypes() => TestPickTypesCore();

		protected virtual void TestPickTypesCore()
		{
			var pickTypes = new PickingFilterBusinessObject().PickTypes;
			AssertEquals(true, pickTypes.ContainsCode(CodeLists.PickType.Codes.DynamicWorkOrder));
			AssertEquals(true, pickTypes.ContainsCode(CodeLists.PickType.Codes.HeldInventoryOrder));
			AssertEquals(true, pickTypes.ContainsCode(CodeLists.PickType.Codes.Order));
			AssertEquals(true, pickTypes.ContainsCode(CodeLists.PickType.Codes.WorkOrder));
		}

		#endregion

		#region TestFilterPickType

		public void TestFilterPickType() => TestFilterPickTypeCore();

		protected virtual void TestFilterPickTypeCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = Helper.CreateProduct(data.Org1, $"P111");
			var subProduct = Helper.CreateProduct(data.Org1, $"P211");
			var bom = Helper.CreateProductBOM(mainProduct, subProduct, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line = Helper.CreateWhsReceiveLine(receive, subProduct, 1234m);
			line.WE_WL = data.Whs1.DefaultLocation.PK;
			receive.FinaliseDocket();
			AssertEquals("The receive should be finalized.", true, receive.IsFinalised);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var dynamicOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var heldOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "O4", mainProduct, 1m);

			var pick1 = Helper.CreatePickNew(order);

			var pick2 = Helper.CreatePickNew(dynamicOrder);
			pick2.WP_PickType = PickType.Codes.DynamicWorkOrder;

			var pick3 = Helper.CreatePickNew(heldOrder);
			pick3.WP_PickType = PickType.Codes.HeldInventoryOrder;

			var pick4 = Helper.CreatePickNew(workOrder);
			pick4.WP_PickType = PickType.Codes.WorkOrder;

			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Pick Type"];
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				filter.IsActive = true;

				filter.Property = PickType.Codes.Order;
				var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick1", pick1, picks.Single());

				filter.Property = PickType.Codes.DynamicWorkOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick2", pick2, picks.Single());

				filter.Property = PickType.Codes.HeldInventoryOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick3", pick3, picks.Single());

				filter.Property = PickType.Codes.WorkOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should only return 1 pick", 1, picks.Length);
				AssertEquals("Should return pick4", pick4, picks.Single());

				filter.Property = "";
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 4 picks", 4, picks.Length);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

				filter.Property = PickType.Codes.Order;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 3 picks", 3, picks.Length);
				Assert("All picks should not have Order PickType", picks.All(p => p.WP_PickType != PickType.Codes.Order));

				filter.Property = PickType.Codes.DynamicWorkOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 3 picks", 3, picks.Length);
				Assert("All picks should not have DynamicWorkOrder PickType", picks.All(p => p.WP_PickType != PickType.Codes.DynamicWorkOrder));

				filter.Property = PickType.Codes.HeldInventoryOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 3 picks", 3, picks.Length);
				Assert("All picks should not have HeldInvenotryOrder PickType", picks.All(p => p.WP_PickType != PickType.Codes.HeldInventoryOrder));

				filter.Property = PickType.Codes.WorkOrder;
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 3 picks", 3, picks.Length);
				Assert("All picks should not have WorkOrder PickType", picks.All(p => p.WP_PickType != PickType.Codes.WorkOrder));

				filter.Property = "";
				picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
				AssertEquals("Should return 4 picks", 4, picks.Length);
			}
		}

		#endregion

		#region TestFilterPickType_RegistryOff

		public void TestFilterPickType_RegistryOff()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);

			var pick1 = Helper.CreatePickNew(order1);

			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var filter = (ModuleTextFilter)FilterStripBizO["Pick Type"];
				AssertNull(filter);
			}
		}

		#endregion

		#region TestOrderTypes

		public void TestOrderTypes()
		{
			var orderTypes = new OrderType();
			AssertEquals(true, orderTypes.ContainsCode(CodeLists.OrderType.Codes.BackOrder));
			AssertEquals(true, orderTypes.ContainsCode(CodeLists.OrderType.Codes.Customs));
			AssertEquals(true, orderTypes.ContainsCode(CodeLists.OrderType.Codes.Order));
			AssertEquals(true, orderTypes.ContainsCode(CodeLists.OrderType.Codes.RepeatOrder));
		}

		#endregion

		#region TestFilterOrderType

		public void TestFilterOrderType()
		{
			SetupTestData();

			Docket11.WD_DocketSubType = CodeLists.OrderType.Codes.BackOrder;
			Docket12.WD_DocketSubType = CodeLists.OrderType.Codes.Customs;
			Docket21.WD_DocketSubType = CodeLists.OrderType.Codes.Order;
			Docket22.WD_DocketSubType = CodeLists.OrderType.Codes.RepeatOrder;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Order Type", ZString.Empty);
			PickAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Order Type", (ZString)CodeLists.OrderType.Codes.BackOrder);
			PickAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Order Type", (ZString)CodeLists.OrderType.Codes.Customs);
			PickAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Order Type", (ZString)CodeLists.OrderType.Codes.Order);
			PickAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Order Type", (ZString)CodeLists.OrderType.Codes.RepeatOrder);
			PickAssert(false, false, false, true);
		}

		#endregion

		#region TestFilter_PercentComplete

		public void TestFilter_PercentComplete()
		{
			SetupTestData();

			Pick11.WP_PercentageComplete = 0;
			Pick12.WP_PercentageComplete = 10;
			Pick21.WP_PercentageComplete = 60;
			Pick22.WP_PercentageComplete = 100;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick % Complete", (ZByte)0, (ZByte)0);
			PickAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick % Complete", (ZByte)0, (ZByte)10);
			PickAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick % Complete", (ZByte)40, (ZByte)70);
			PickAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick % Complete", (ZByte)0, (ZByte)100);
			PickAssert(true, true, true, true);
		}

		#endregion

		#region TestFilterConsigneeCompanyName

		public void TestFilterConsigneeCompanyName_PickWithOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise";

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.ConsigneePK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_CompanyName = "CargoWise 123";
			order2.ConsigneeDocAddress.E2_City = "City";
			order2.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).Property = "CargoWise";
			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(2, picks.Length);
			AssertCollectionContains(pick1, picks);
			AssertCollectionContains(pick2, picks);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(2, picks.Length);
			AssertCollectionContains(pick1, picks);
			AssertCollectionContains(pick2, picks);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(1, picks.Length);
			AssertCollectionContains(pick1, picks);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(0, picks.Length);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(0, picks.Length);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(1, picks.Length);
			AssertCollectionContains(pick2, picks);
		}

		public void TestFilterConsigneeCompanyName_PickWithWorkOrder() => TestFilterConsigneeCompanyName_PickWithWorkOrderCore();

		protected virtual void TestFilterConsigneeCompanyName_PickWithWorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			consignee1.OH_FullName = "123";
			var consignee2 = Helper.CreateClient("CNE2");
			consignee2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 20m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			Factory.Save();

			// company name override
			var consignee3 = Helper.CreateClient("CNE3");
			consignee3.OH_FullName = "abc";

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR3", mainProduct, 1m);
			order.ConsigneePK = consignee3.PK;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "123 Override";

			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());
			var workOrder4 = order.CurrentWorkOrders.Single();

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);
			var pick4 = Helper.CreatePickNew(workOrder4);

			Factory.Save();

			var consigneeNameFilter = ((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]);
			consigneeNameFilter.Property = "12";
			consigneeNameFilter.IsActive = true;
			consigneeNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick4 }, picks);

			consigneeNameFilter.Property = "123 Override";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick4 }, picks);

			consigneeNameFilter.Property = "xy";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, picks);

			consigneeNameFilter.Property = "";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3, pick4 }, picks);
		}

		public void TestFilterConsigneeCompanyName_PickWithWorkOrder_NotWronglyFilterByOtherAddressType()
			=> TestFilterConsigneeCompanyName_PickWithWorkOrder_NotWronglyFilterByOtherAddressTypeCore();

		protected virtual void TestFilterConsigneeCompanyName_PickWithWorkOrder_NotWronglyFilterByOtherAddressTypeCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var orgHeader1 = Helper.CreateClient("OH1");
			orgHeader1.OH_FullName = "123";
			var orgHeader2 = Helper.CreateClient("OH2");
			orgHeader2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, orgHeader1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, orgHeader2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = CreateWhsWorkOrderWithTransportCo(data.Org1, orgHeader2, data.Whs1, "OR3", mainProduct);

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var consigneeNameFilter = ((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]);
			consigneeNameFilter.Property = "12";
			consigneeNameFilter.IsActive = true;
			consigneeNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, picks);

			consigneeNameFilter.Property = "xy";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, picks);

			consigneeNameFilter.Property = "";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, picks);
		}

		public void TestFilterConsigneeCompanyName_PickWithOrderAndWorkOrder() => TestFilterConsigneeCompanyName_PickWithOrderAndWorkOrderCore();

		protected virtual void TestFilterConsigneeCompanyName_PickWithOrderAndWorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = Helper.CreateProduct(data.Org1, "MainP");
			var componentProduct = Helper.CreateProduct(data.Org1, "ComP");
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			consignee1.OH_FullName = "123";
			var consignee2 = Helper.CreateClient("CNE2");
			consignee2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", componentProduct, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part1, 1m);
			order1.ConsigneePK = consignee1.PK;
			order2.ConsigneePK = consignee2.PK;

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			var pick4 = Helper.CreatePickNew(workOrder1);
			var pick5 = Helper.CreatePickNew(workOrder2);
			var pick6 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var consigneeNameFilter = ((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]);
			consigneeNameFilter.Property = "12";
			consigneeNameFilter.IsActive = true;
			consigneeNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick4 }, picks);

			consigneeNameFilter.Property = "xy";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2, pick5 }, picks);

			consigneeNameFilter.Property = "";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3, pick4, pick5, pick6 }, picks);
		}

		#endregion

		#region TestFilterTransportCompanyName

		public void TestFilterTransportCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Transport";

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.TransportCoPK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.TransportCoDocAddress.E2_AddressOverride = true;
			order2.TransportCoDocAddress.E2_CompanyName = "CargoWise Transport 123";

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).Property = "CargoWise Transport";
			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(2, picks.Length);
			AssertCollectionContains(pick1, picks);
			AssertCollectionContains(pick2, picks);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(2, picks.Length);
			AssertCollectionContains(pick1, picks);
			AssertCollectionContains(pick2, picks);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(1, picks.Length);
			AssertCollectionContains(pick1, picks);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(0, picks.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(0, picks.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertEquals(1, picks.Length);
			AssertCollectionContains(pick2, picks);
		}

		public void TestFilterTransportCompanyName_WorkOrder() => TestFilterTransportCompanyName_WorkOrderCore();

		protected virtual void TestFilterTransportCompanyName_WorkOrderCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var transportCo1 = Helper.CreateClient("TRC1");
			transportCo1.OH_FullName = "123";
			var transportCo2 = Helper.CreateClient("TRC2");
			transportCo2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 20m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			Factory.Save();

			// company name override
			var transportCo3 = Helper.CreateClient("TRC3");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR3", mainProduct, 1m);
			order.TransportCoPK = transportCo3.PK;
			order.TransportCoDocAddress.E2_AddressOverride = true;
			order.TransportCoDocAddress.E2_CompanyName = "123 Override";

			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());
			var workOrder4 = order.CurrentWorkOrders.Single();

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);
			var pick4 = Helper.CreatePickNew(workOrder4);

			Factory.Save();

			var consigneeNameFilter = ((ModuleTextFilter)FilterStripBizO["Transport Company Name"]);
			consigneeNameFilter.Property = "12";
			consigneeNameFilter.IsActive = true;
			consigneeNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			var picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick4 }, picks);

			consigneeNameFilter.Property = "123 Override";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick4 }, picks);

			consigneeNameFilter.Property = "xy";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, picks);

			consigneeNameFilter.Property = "";
			picks = Factory.Load<WhsPick>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3, pick4 }, picks);
		}

		#endregion

		#region TestFilterTransportCo

		public void TestFilterTransportCo_PickWithOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);

			var transportCo1 = Helper.CreateClient("TRC1");
			var transportCo2 = Helper.CreateClient("TRC2");

			order1.TransportCoPK = transportCo1.PK;
			order2.TransportCoPK = transportCo2.PK;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			var pickCollection = GetPickCollection();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", ZGuid.Empty);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", transportCo1.PK);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", transportCo2.PK);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);
		}

		public void TestFilterTransportCo_PickWithWorkOrders() => TestFilterTransportCo_PickWithWorkOrdersCore();

		protected virtual void TestFilterTransportCo_PickWithWorkOrdersCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var transportCo1 = Helper.CreateClient("TRC1");
			var transportCo2 = Helper.CreateClient("TRC2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");      // no consignee
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(workOrder1);
			var pick2 = Helper.CreatePickNew(workOrder2);
			var pick3 = Helper.CreatePickNew(workOrder3);

			Factory.Save();

			var pickCollection = GetPickCollection();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", ZGuid.Empty);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1, pick2, pick3 }, pickCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", transportCo1.PK);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick1 }, pickCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Co", transportCo2.PK);
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { pick2 }, pickCollection);
		}

		#endregion

		#region Test Distribution Centre Filters

		protected override bool SupportsDistributionCentreFilters => true;

		#region TestFilterDistributionCentreName

		protected override void TestFilterDistributionCentreNameCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Distribution Centre";

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.DistributionCentreDocAddress.OrganisationPK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.DistributionCentreDocAddress.E2_AddressOverride = true;
			order2.DistributionCentreDocAddress.E2_CompanyName = "CargoWise Distribution Centre 123";

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2);

			var filter = (ModuleTextFilter)FilterStripBizO["Distribution Center Name"];
			filter.Property = "CargoWise Distribution Centre";
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			Asserter.AssertMatches("Should return all picks.", filter, pick1, pick2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Asserter.AssertMatches("Should return all picks.", filter, pick1, pick2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Asserter.AssertMatches("Should return only pick1.", filter, pick1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			Asserter.AssertMatches("Should return no picks.", filter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			Asserter.AssertMatches("Should return no picks.", filter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Should return only pick1.", filter, pick2);
		}

		#endregion

		#region TestFilterDistributionCentre

		protected override void TestFilterDistributionCentreCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre2.PK;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order3", data.Part1, 1m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Distribution Centre specified should return all picks.", filter, pick1, pick2, pick3);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = distributionCentre1.PK;
			Asserter.AssertMatches("Distribution Centre 1 specified should return pick1", filter, pick1);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = distributionCentre2.PK;
			Asserter.AssertMatches("Distribution Centre 2 specified should return pick2.", filter, pick2);
		}

		protected override void TestFilterDistributionCentre_NotEqual_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre2.PK;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order3", data.Part1, 1m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Distribution Centre specified should return all picks.", filter, pick1, pick2, pick3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = distributionCentre2.PK;
			Asserter.AssertMatches("Distribution Centre 1 specified should return pick1", filter, pick1);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = distributionCentre1.PK;
			Asserter.AssertMatches("Distribution Centre 2 specified should return pick2.", filter, pick2);
		}

		protected override void TestFilterDistributionCentre_IsBlank_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre2.PK;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order3", data.Part1, 1m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank should return only pick3.", filter, pick3);
		}

		protected override void TestFilterDistributionCentre_IsNotBlank_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order1", data.Part1, 1m);
			order1.DistributionCentreDocAddress.OrganisationPK = distributionCentre1.PK;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order2", data.Part1, 1m);
			order2.DistributionCentreDocAddress.OrganisationPK = distributionCentre2.PK;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "order3", data.Part1, 1m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank should return pick1 and pick2.", filter, pick1, pick2);
		}

		#endregion

		#endregion

		#region TestFilterDocketReferenceAnyForLongValue

		protected override BusinessObject[] GetBizOsForFilterDocketReferenceAny(params WhsDocket[] dockets)
		{
			var bizOs = new BusinessObject[dockets.Length];
			for (var i = 0; i < dockets.Length; i++)
			{
				var pick = Factory.NewWithValidTestData<WhsPick>();
				pick.WP_WW_Whs = dockets[i].WD_WW_Whs;
				dockets[i].WD_WP = pick.PK;
				dockets[i].WD_DocketStatus = DocketStatus.Codes.AttachedToPick;

				bizOs[i] = pick;
			}

			return bizOs;
		}

		#endregion

		#region TestProductFilter_MultipleItemsWithSameCode

		public override void TestProductFilter_MultipleItemsWithSameCode()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var org1 = Helper.CreateClient("O1", "O1");
			var docket11 = CreateDocket(org1, whs1, "11");
			var docket12 = CreateDocket(org1, whs1, "12");
			var docket13 = CreateDocket(org1, whs1, "13");
			Factory.Save();

			var part11 = Helper.CreateProduct(org1, "DIFFCODE1");
			var part12 = Helper.CreateProduct(org1, "DIFFCODE2");
			var part = Helper.CreateProduct(org1, "SAMECODE");

			CreateDocketLine(docket11, part11, 10);
			CreateDocketLine(docket11, part, 10);
			CreateDocketLine(docket12, part11, 10);
			CreateDocketLine(docket12, part12, 10);
			CreateDocketLine(docket12, part, 10);
			CreateDocketLine(docket13, part11, 10);
			CreateDocketLine(docket13, part12, 10);

			Helper.CreatePickNew((WhsOrder)docket11);
			Helper.CreatePickNew((WhsOrder)docket12);
			Helper.CreatePickNew((WhsOrder)docket13);

			Factory.Save();

			var pickCollection = GetPickCollection();
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", org1.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product", part.PK);
			pickCollection.Load(DocketFilter.Filter);
			AssertEquals("Precondition", 2, pickCollection.Count);

			part11.OP_PartNum = "SAMECODE";

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			pickCollection.Load(DocketFilter.Filter);
			AssertEquals("Filter should Grab Matching Product Codes", 3, pickCollection.Count);
		}

		#endregion

		#region TestFilterByPriority

		public void TestFilterByPriority()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);
			order1.WD_PickPriority = 0;
			order2.WD_PickPriority = 2;
			order3.WD_PickPriority = 3;
			order4.WD_PickPriority = 2;

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2, order3);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)FilterStripBizO["Priority"];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 2;

			var pickCollection = new WhsPickCollection(Factory, filter.Query);
			pickCollection.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder(pickCollection.Select(p => p.PK), new[] { pick2.PK });
		}

		#endregion

		#region TestFilter_TrolleyNumber

		public void TestFilter_TrolleyNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T2
			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m); // assigned to T1
			var pick3 = Helper.CreatePickNew(order3);
			var package3 = order3.PackageJob.Packages.AddNew();
			pick3.FinaliseAllOrders();
			pick3.FinalisePick();

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m); // not assigned to any trolley
			var pick4 = Helper.CreatePickNew(order4);
			order4.PackageJob.Packages.AddNew();
			AssertEquals("Precondition - ensure pick1 is NOT finalised.", false, pick1.IsFinalised);
			AssertEquals("Precondition - ensure pick2 is NOT finalised.", false, pick2.IsFinalised);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick3);
			AssertEquals("Precondition - ensure pick4 is NOT finalised.", false, pick4.IsFinalised);

			// create trolleys
			var trolley1 = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package3.PK, 2);

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "BLD");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package2.PK, 1);
			Factory.Save();

			// assert filters
			Asserter.AddToScope(pick1, pick2, pick3, pick4);

			var filter = (ModuleTextFilter)FilterStripBizO[PickingFilterBusinessObject.Schema.TrolleyNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RandomText";
			Asserter.AssertMatches("Should find nothing as none of the trolleys have such name", filter);

			filter.Property = "T1";
			Asserter.AssertMatches("Should find all picks for the trolley, even finalised one.", filter, pick1, pick3);

			filter.Property = "T2";
			Asserter.AssertMatches("Should find all picks for the trolley.", filter, pick2);

			filter.Property = "T";
			Asserter.AssertMatches("Should find picks from both trolleys as their trolley number starts with 'T'.", filter, pick1, pick2, pick3);

			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should not apply any filters, and find all picks.", filter, pick1, pick2, pick3, pick4);

			filter.Property = "";
			filter.ComparisonOperator = "is blank";
			Asserter.AssertMatches("Should find orders with Trolley Number is blank.", filter, pick4);

			filter.Property = "";
			filter.ComparisonOperator = "is not blank";
			Asserter.AssertMatches("Should find orders with Trolley Number is not blank.", filter, pick1, pick2, pick3);
		}

		public void TestFilter_TrolleyNumber_IgnoresFinalisedTrolleys()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1, but trolley job is finalised
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T1
			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();

			// create 2 trolley Jobs for same trolley
			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley.PK, "FIN");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);

			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package2.PK, 1);
			Factory.Save();

			// assert filters
			Asserter.AddToScope(pick1, pick2);

			var filter = (ModuleTextFilter)FilterStripBizO[PickingFilterBusinessObject.Schema.TrolleyNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RandomText";
			Asserter.AssertMatches("Should find nothing as none of the trolleys have such name", filter);

			filter.Property = "T1";
			Asserter.AssertMatches("Should find picks for the trolley, but only from unfinalised trolley job.", filter, pick2);

			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should not apply any filters, and find all picks.", filter, pick1, pick2);
		}

		#endregion

		#region IsServiceLevelUsed

		protected override bool IsServiceLevelUsed => true;

		#endregion

		#region TestFilterDocketReferenceAnyForLongValue

		protected override IBusinessObjectCollection GetCollectionWithFilter(ZQuery filter)
		{
			var collection = GetPickCollection();
			collection.Load(filter);

			return collection;
		}

		#endregion

		#region Test Filter Max Length

		public void TestFilterMaxLength_Picking()
		{
			var transptCompMaxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Transport Company Name should be set correctly.", transptCompMaxLength, FilterStripBizO["Transport Company Name"].MaxLength);
				AssertEquals("MaxLength of Consignee Company Name should be set correctly.", transptCompMaxLength, FilterStripBizO["Consignee Company Name"].MaxLength);
				AssertEquals("MaxLength of Distribution Centre Name should be set correctly.", transptCompMaxLength, FilterStripBizO["Distribution Center Name"].MaxLength);
				AssertEquals("MaxLength of Receive Reference (Pick Slip) should be set correctly.", WhsDocketSchema.WD_ExternalReference.MaxLength, FilterStripBizO["Receive Reference (Pick Slip)"].MaxLength);
				AssertEquals("MaxLength of Trolley Number should be set correctly.", RefEquipmentSchema.RQ_Registration.MaxLength, FilterStripBizO["Trolley Number"].MaxLength);
				AssertEquals("MaxLength of Package ID should be set correctly.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, FilterStripBizO["Package ID"].MaxLength);
			});
		}

		#endregion

		#region Filter FinalizedDate

		protected override void TestFinalizedDateFilterCore()
		{
			// Do not call base, we are changing behaviour of test

			var data = new TestDataSimpleEnvironment(Factory);
			var docket1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "A", data.Part1, 9);
			var docket2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "B", data.Part1, 10);
			var docket3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "C", data.Part2, 11);
			var docket4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "D", data.Part2, 12);
			var docket5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "E", data.Part2, 12);
			var docket6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "F", data.Part1, 12);
			Factory.Save();

			var now = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.Now);

			// Inside range
			var pick1 = Helper.CreatePickNew(docket1);
			docket1.FinaliseDocket();
			docket1.WD_FinalisedDate = now;

			// Above range
			var pick2 = Helper.CreatePickNew(docket2);
			docket2.FinaliseDocket();
			docket2.WD_FinalisedDate = now.AddDays(5);

			// One docket inside, one docket outside range -> Pick should be inside range
			var pick3 = Helper.CreatePickNew(docket3, docket4);
			docket3.FinaliseDocket();
			docket4.FinaliseDocket();
			docket3.WD_FinalisedDate = now.AddDays(-30);
			docket4.WD_FinalisedDate = now.AddDays(-1);

			// Inside range
			var pick4 = Helper.CreatePickNew(docket5);
			docket5.FinaliseDocket();
			docket5.WD_FinalisedDate = now.AddDays(-2);

			// Under range
			var pick5 = Helper.CreatePickNew(docket6);
			docket6.FinaliseDocket();
			docket6.WD_FinalisedDate = now.AddDays(-30);

			Factory.Save();
			Asserter.AddToScope(pick1, pick2, pick3, pick4, pick5);

			var docketFilterStrip = GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateTimeOffsetFilter)docketFilterStrip["Finalized Date"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = now.AddDays(-4).ToDateTime();
			dateFilter.Property2 = now.AddDays(1).ToDateTime();
			Asserter.AssertMatches("These picks should have been filtered through.", dateFilter, pick1, pick3, pick4);
		}

		#endregion

		#region TestFilterClient

		protected override void TestFilterClient_WithComparisonOperator_EqualsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org1 = Helper.CreateClient("Client1");
			var org2 = Helper.CreateClient("Client2");
			var org3 = Helper.CreateClient("Client3");
			Factory.Save();

			var docket1 = CreateDocket(org1, data.Whs1, "R1");
			var docket2 = CreateDocket(org2, data.Whs1, "R2");
			var docket3 = CreateDocket(org3, data.Whs1, "R3");
			Factory.Save();

			var pick1 = Factory.NewWithValidTestData<WhsPick>();
			var pick2 = Factory.NewWithValidTestData<WhsPick>();
			var pick3 = Factory.NewWithValidTestData<WhsPick>();

			pick1.WP_WW_Whs = docket1.WD_WW_Whs;
			pick2.WP_WW_Whs = docket2.WD_WW_Whs;
			pick3.WP_WW_Whs = docket3.WD_WW_Whs;

			docket1.WD_WP = pick1.PK;
			docket2.WD_WP = pick2.PK;
			docket3.WD_WP = pick3.PK;

			docket1.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			docket2.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			docket3.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			var filter = (ModuleGuidFilter)DocketFilter["Client"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = org1.PK;

			var pickCollection = GetPickCollection();
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder("Only pick1 is returned.", new[] { pick1 }, pickCollection);
		}

		protected override void TestFilterClient_WithComparisonOperator_NotEqualsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org1 = Helper.CreateClient("Client1");
			var org2 = Helper.CreateClient("Client2");
			var org3 = Helper.CreateClient("Client3");
			Factory.Save();

			var docket1 = CreateDocket(org1, data.Whs1, "R1");
			var docket2 = CreateDocket(org2, data.Whs1, "R2");
			var docket3 = CreateDocket(org3, data.Whs1, "R3");
			Factory.Save();

			var pick1 = Factory.NewWithValidTestData<WhsPick>();
			var pick2 = Factory.NewWithValidTestData<WhsPick>();
			var pick3 = Factory.NewWithValidTestData<WhsPick>();

			pick1.WP_WW_Whs = docket1.WD_WW_Whs;
			pick2.WP_WW_Whs = docket2.WD_WW_Whs;
			pick3.WP_WW_Whs = docket3.WD_WW_Whs;

			docket1.WD_WP = pick1.PK;
			docket2.WD_WP = pick2.PK;
			docket3.WD_WP = pick3.PK;

			docket1.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			docket2.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			docket3.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			var filter = (ModuleGuidFilter)DocketFilter["Client"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = org1.PK;

			var pickCollection = GetPickCollection();
			pickCollection.Load(DocketFilter.Filter);
			AssertContainsExactElementsInAnyOrder("All except pick1 is returned.", new[] { pick2, pick3 }, pickCollection);
		}

		#endregion

		#region TestFilterDynamicAreaOverride

		public void TestFilterDynamicAreaOverride()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 4);
			var dynamicLocation1 = data.Whs1.FindLocation("A-1-2");
			var dynamicLocation2 = data.Whs1.FindLocation("A-1-4");

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea1 = Helper.CreateArea(data.Whs1, "Dynamic1", AreaTypes.Codes.DynamicPickFace, true, false);
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicArea2 = Helper.CreateArea(data.Whs1, "Dynamic2", AreaTypes.Codes.DynamicPickFace, true, false);
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;

			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row1", 2, 2);
			var dynamicAreaWhs2 = Helper.CreateArea(warehouse2, "Dynamic3", AreaTypes.Codes.DynamicPickFace, true, false);
			var locWhs2 = warehouse2.DefaultLocation;
			locWhs2.WLV_WLT_LocationType = dynamicLocationType.PK;
			locWhs2.WLV_WA_PickingArea = dynamicAreaWhs2.PK;
			Factory.Save();

			var pick1 = Factory.New<WhsPick>();
			pick1.WP_WW_Whs = data.Whs1.PK;
			pick1.WP_WA_DynamicPickAreaOverride = dynamicArea2.PK;

			var pick2 = Factory.New<WhsPick>();
			pick2.WP_WW_Whs = data.Whs1.PK;
			pick2.WP_WA_DynamicPickAreaOverride = dynamicArea2.PK;

			var pick3 = Factory.New<WhsPick>();
			pick3.WP_WW_Whs = data.Whs1.PK;
			pick3.WP_WA_DynamicPickAreaOverride = dynamicArea1.PK;

			Factory.Save();
			Asserter.AddToScope(pick1, pick2, pick3);

			var pickFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)pickFilter["Warehouse"];
			var dynamicAreaFilter = (ModuleGuidFilter)pickFilter[PickingFilterBusinessObject.Schema.DynamicPickAreaOverride];
			AssertEquals("Module Id should be WhsConfigArea.", ModuleIDs.WhsConfigArea, dynamicAreaFilter.ModuleId);

			warehouseFilter.IsActive = true;
			dynamicAreaFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			dynamicAreaFilter.Property = dynamicArea1.PK;
			Asserter.AssertMatches("Should match 1 WhsPick.", pickFilter.Filter, pick3);
			dynamicAreaFilter.Property = dynamicArea2.PK;
			Asserter.AssertMatches("Should match 2 WhsPicks.", pickFilter.Filter, pick1, pick2);

			warehouseFilter.Property = warehouse2.PK;
			dynamicAreaFilter.Property = dynamicAreaWhs2.PK;
			Asserter.AssertMatches("Should match no WhsPick.", pickFilter.Filter);

			warehouseFilter.Property = ZGuid.Empty;
			dynamicAreaFilter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Should match all WhsPicks.", pickFilter.Filter, pick1, pick2, pick3);
		}

		public void TestFilterDynamicAreaOverride_ClearedOnWarehouseChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "Dynamic1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var dynamicAreaFilter = (ModuleGuidFilter)transferFilter[PickingFilterBusinessObject.Schema.DynamicPickAreaOverride];

			warehouseFilter.IsActive = true;
			dynamicAreaFilter.IsActive = true;
			warehouseFilter.Property = data.Whs1.PK;
			dynamicAreaFilter.Property = dynamicArea.PK;
			warehouseFilter.Property = ZGuid.Empty;

			AssertEquals("DynamicPickAreaOverride Filter should be cleared when warehouse is changed.", ZGuid.Empty, dynamicAreaFilter.Property);
		}

		public void TestFilterDynamicAreaOverride_NotClearedOnWarehousePopulationToCorrectValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "Dynamic1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocation = data.Whs1.FindLocation("A-1");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var transferFilter = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)transferFilter["Warehouse"];
			var dynamicAreaFilter = (ModuleGuidFilter)transferFilter[PickingFilterBusinessObject.Schema.DynamicPickAreaOverride];

			warehouseFilter.IsActive = true;
			dynamicAreaFilter.IsActive = true;
			dynamicAreaFilter.Property = dynamicArea.PK;
			warehouseFilter.Property = data.Whs1.PK;

			AssertEquals("DynamicPickAreaOverride Filter should not be cleared when warehouse is populated.", dynamicArea.PK, dynamicAreaFilter.Property);
		}

		public void TestFilterDynamicAreaOverride_Validator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dynamicLocation = data.Whs1.FindLocation("A-1");

			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			var dynamicArea = Helper.CreateArea(data.Whs1, "Dynamic1", AreaTypes.Codes.DynamicPickFace, true, false);
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var pickFilter = GetNewFilterStripBusinessObject();
			var dynamicAreaFilter = (ModuleGuidFilter)pickFilter[PickingFilterBusinessObject.Schema.DynamicPickAreaOverride];
			dynamicAreaFilter.IsActive = true;

			var error = "Dynamic Pick Area Override can not be entered without a warehouse.";

			dynamicAreaFilter.Validation.ValidateProperty();
			AssertNoError(dynamicAreaFilter.PropertyInfo, error);

			dynamicAreaFilter.Property = dynamicArea.PK;
			AssertHasError(dynamicAreaFilter.PropertyInfo, error);

			var warehouseFilter = (ModuleGuidFilter)pickFilter["Warehouse"];
			warehouseFilter.Property = ZGuid.Empty;
			dynamicAreaFilter.Property = dynamicArea.PK;
			AssertHasError(dynamicAreaFilter.PropertyInfo, error);

			warehouseFilter.Property = ZGuid.Invalid;
			dynamicAreaFilter.Property = dynamicArea.PK;
			AssertHasError(dynamicAreaFilter.PropertyInfo, error);

			warehouseFilter.Property = data.Whs1.PK;
			warehouseFilter.IsActive = true;
			dynamicAreaFilter.Property = dynamicArea.PK;
			AssertNoError(dynamicAreaFilter.PropertyInfo, error);
		}

		#endregion

		#region TestSerialNumberFilter

		protected override void TestSerialNumberFilterCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			// setup receives
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, locationA1);
			receiveLine1.WE_SerialNumber = "SN11";
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 1m, locationA2);
			receiveLine2.WE_SerialNumber = "SN12";
			Factory.Save();

			// setup 2 orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			order1.Lines.Single().WE_SerialNumber = "SN11";
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);
			order2.Lines.Single().WE_SerialNumber = "SN12";
			var pick2 = Helper.CreatePickNew(order2);

			Factory.Save(); // for dbonlyquery

			Asserter.AddToScope(pick1);
			Asserter.AddToScope(pick2);

			var filter = (ModuleTextFilter)FilterStripBizO["Serial Number"];

			// equal
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SN11";
			Asserter.AssertMatches("Equals 'SN11' should return only pick1.", filter, pick1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Equals 'SN12' should return only pick2.", filter, pick2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Equals 'SN1' should return no Picks.", filter);

			// starts with
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "SN11";
			Asserter.AssertMatches("Starts with 'SN11' should return only pick1.", filter, pick1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Starts with 'SN12' should return only pick2.", filter, pick2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Starts with 'SN1' should return both Picks.", filter, pick1, pick2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Starts with 'SN2' should return no Picks.", filter);

			// contains
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Contains 'SN11' should return only pick1.", filter, pick1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Contains 'SN12' should return only pick2.", filter, pick2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Contains 'SN1' should return both Picks.", filter, pick1, pick2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Contains 'SN2' should return no Picks.", filter);

			// not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Not contains 'SN11' should return only pick1.", filter, pick2);
			filter.Property = "SN12";
			Asserter.AssertMatches("Not contains 'SN12' should return only pick2.", filter, pick1);
			filter.Property = "SN1";
			Asserter.AssertMatches("Not contains 'SN1' should return no Picks.", filter);
			filter.Property = "SN2";
			Asserter.AssertMatches("Not contains 'SN2' should return both Picks.", filter, pick1, pick2);
		}

		#endregion

		#region TestIsAwaitingReplenishmentFilter

		public void TestIsAwaitingReplenishmentFilter()
		{
			var pick1 = Factory.New<WhsPick>();
			pick1.WP_IsAwaitingReplenishment = false;
			var pick2 = Factory.New<WhsPick>();
			pick2.WP_IsAwaitingReplenishment = true;
			var pick3 = Factory.New<WhsPick>();
			Factory.Save();

			Asserter.AddToScope(pick1, pick2, pick3);

			var pickFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)pickFilter[PickingFilterBusinessObject.Schema.IsAwaitingReplenishment];
			filter.IsActive = true;
			filter.Property = PickingFilterBusinessObject.Schema.IsAwaitingReplenishmentTypeCodes.All;
			Asserter.AssertMatches("All", pickFilter.Filter, pick1, pick2, pick3);

			filter.Property = PickingFilterBusinessObject.Schema.IsAwaitingReplenishmentTypeCodes.AwaitingReplenishment;
			Asserter.AssertMatches("Awaiting Replenishment", pickFilter.Filter, pick2);

			filter.Property = PickingFilterBusinessObject.Schema.IsAwaitingReplenishmentTypeCodes.NotAwaitingReplenishment;
			Asserter.AssertMatches("Not Awaiting Replenishment", pickFilter.Filter, pick1, pick3);
		}

		#endregion

		#region TestFilterSalesChannel

		public void TestFilterSalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var salesChannelAlibaba = Helper.CreateWhsSalesChannel("ABB", "ALIBABA");
			var salesChannelDirectWebSale = Helper.CreateWhsSalesChannel("DIR", "DIRECT WEB SALE");
			var salesChannelShopify = Helper.CreateWhsSalesChannel("SFY", "SHOPIFY");

			var orderABB_1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1);
			orderABB_1.WD_WSH_SalesChannel = salesChannelAlibaba.PK;

			var orderABB_2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1);
			orderABB_2.WD_WSH_SalesChannel = salesChannelAlibaba.PK;

			var orderSFY_1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 1);
			orderSFY_1.WD_WSH_SalesChannel = salesChannelShopify.PK;

			var orderSFY_2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order4", data.Part2, 1);
			orderSFY_2.WD_WSH_SalesChannel = salesChannelShopify.PK;

			var pickingWithSFYOnly = Helper.CreatePickNew(orderSFY_1);
			var pickingWithSFYAndABB = Helper.CreatePickNew(orderABB_1, orderABB_2, orderSFY_2);

			Factory.Save();

			Asserter.AddToScope(pickingWithSFYOnly, pickingWithSFYAndABB);

			var filter = (ModuleGuidFilter)FilterStripBizO["Sales Channel"];
			AssertNotNull("Precondition: Filter Sales Channel exists", filter);

			filter.Property = salesChannelAlibaba.PK;
			Asserter.AssertMatches("Expect 1 Alibaba picking", filter, pickingWithSFYAndABB);

			filter.Property = salesChannelShopify.PK;
			Asserter.AssertMatches("Expect 2 Shopify pickings", filter, pickingWithSFYAndABB, pickingWithSFYOnly);

			filter.Property = salesChannelDirectWebSale.PK;
			Asserter.AssertMatches("Expect none DirectWebSale pickings", filter, Array.Empty<WhsPick>());

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Expect all pickings to return on empty filter", filter, pickingWithSFYOnly, pickingWithSFYAndABB);
		}

		#endregion

		#region Base

		void SetClientAttributesType(OrgHeader client, bool usePartAttrib1, bool usePartAttrib2, bool usePartAttrib3, bool useExpiryDate, bool usePackingDate)
		{
			if (usePartAttrib1)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.One, false);
			}

			if (usePartAttrib2)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.Two, false);
			}

			if (usePartAttrib3)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.Three, false);
			}

			if (useExpiryDate)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.ExpiryDate, true);
			}

			if (usePackingDate)
			{
				Helper.SetClientAttributeType(client, AttributeNumber.PackingDate, true);
			}
		}

		void SetProductAttributesUse(OrgHeader owner, OrgSupplierPart part, bool usePartAttrib1, bool usePartAttrib2, bool usePartAttrib3, bool useExpiryDate, bool usePackingDate)
		{
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.One, usePartAttrib1);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Two, usePartAttrib2);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.Three, usePartAttrib3);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, useExpiryDate);
			Helper.SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, usePackingDate);
		}

		void PickAssert(bool d11, bool d12, bool d21, bool d22)
		{
			var pickCollection = GetPickCollection();
			pickCollection.Load(DocketFilter.Filter);

			var picks = new[]
			{
				new { Include = d11, Pick = Pick11 },
				new { Include = d12, Pick = Pick12 },
				new { Include = d21, Pick = Pick21 },
				new { Include = d22, Pick = Pick22 },
			};
			AssertContainsExactElementsInAnyOrder(picks.Where(o => o.Include).Select(o => o.Pick), pickCollection);
		}

		protected override void DocketAssert(bool d11, bool d12, bool d21, bool d22)
		{
			PickAssert(d11, d12, d21, d22);
		}

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsOrder(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			return Helper.CreateWhsOrderLine((WhsOrder)docket, part, units);
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return null;
		}

		WhsPickCollection GetPickCollection()
		{
			return new WhsPickCollection(Factory);
		}

		protected override bool SupportsFilterForEnteredStatus => false;

		protected override void SetupTestData()
		{
			base.SetupTestData();

			Pick11 = Factory.NewWithValidTestData<WhsPick>();
			Pick12 = Factory.NewWithValidTestData<WhsPick>();
			Pick21 = Factory.NewWithValidTestData<WhsPick>();
			Pick22 = Factory.NewWithValidTestData<WhsPick>();
			Pick11.WP_WW_Whs = Docket11.WD_WW_Whs;
			Pick12.WP_WW_Whs = Docket12.WD_WW_Whs;
			Pick21.WP_WW_Whs = Docket21.WD_WW_Whs;
			Pick22.WP_WW_Whs = Docket22.WD_WW_Whs;

			Docket11.WD_WP = Pick11.PK;
			Docket12.WD_WP = Pick12.PK;
			Docket21.WD_WP = Pick21.PK;
			Docket22.WD_WP = Pick22.PK;

			Docket11.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Docket12.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Docket21.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Docket22.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
		}

		protected WhsPick Pick11;
		protected WhsPick Pick12;
		protected WhsPick Pick21;
		protected WhsPick Pick22;

		#endregion

		#region Implementation

		protected PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));

		protected override bool SupportsTransportCoFilters => true;

		PackingTestHelper packingHelper;

		#endregion
	}
}
