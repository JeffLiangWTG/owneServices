using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderLineFilterBusinessObject))]
	public class OrderLineFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrderLineFilterBusinessObject();
		}

		#region TestFilterByClient

		public void TestFilterByClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var client1 = helper.CreateClient("Client1");
			var client2 = helper.CreateClient("Client2");

			var order1 = helper.CreateWhsOrderWithOrderLine(client1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "Order2", data.Part1, 1m);

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleGuidFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Client];
			filter.Property = client1.PK;
			asserter.AssertMatches("Client1 specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByWarehouse

		public void TestFilterByWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var whs1 = helper.CreateWarehouse("Whs1", "A1");
			var whs2 = helper.CreateWarehouse("Whs2", "A2");

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, "Order1", data.Part1, 1m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "Order2", data.Part1, 1m);

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleGuidFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Warehouse];
			filter.Property = whs1.PK;
			asserter.AssertMatches("Whs1 specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterBySubType

		public void TestFilterBySubType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			order1.WD_DocketSubType = OrderType.Codes.Customs;
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);
			order2.WD_DocketSubType = OrderType.Codes.Order;

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.SubType];
			filter.Property = OrderType.Codes.Customs;
			asserter.AssertMatches("SubType CUS specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByConsignee

		public void TestFilterByConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consignee1 = helper.CreateClient("Consignee1");
			var consignee2 = helper.CreateClient("Consignee2");

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			order1.ConsigneePK = consignee1.PK;
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);
			order2.ConsigneePK = consignee2.PK;

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleGuidFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Consignee];
			filter.Property = consignee1.PK;
			asserter.AssertMatches("Consignee1 specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByOrderNo

		public void TestFilterByOrderNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.OrderNo];
			filter.Property = "Order1";
			asserter.AssertMatches("Order No. Order1 specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByStatus

		public void TestFilterByStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			order1.WD_DocketStatus = DocketStatus.Codes.Entered;
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part1, 1m);
			order2.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Status];
			filter.Property = DocketStatus.Codes.Entered;
			asserter.AssertMatches("Status PUT specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		public void TestFilterByStatus_Loading()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 2m);
			var loadPkgPackagePivot2 = helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";

			var package3 = order2.PackageJob.Packages.AddNew("CTN");
			package3.Pack(order2.Lines[0].ReleaseLines[0], 3m);
			Factory.Save();

			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Status];
			filter.Property = WhsOrderStatus.Codes.Loading;
			asserter.AssertMatches("Status LDG specified should return lines from Order2.", filter, order2.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByProduct

		public void TestFilterByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);

			Factory.Save();
			asserter.AddToScope(order1.Lines.Concat(order2.Lines).Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleGuidFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.Product];
			filter.Property = data.Part1.PK;
			asserter.AssertMatches("Part1 specified should return lines from Order1.", filter, order1.Lines.Cast<WhsOrderLine>().ToArray());
		}

		#endregion

		#region TestFilterByDeclarationReference

		public void TestFilterByDeclarationReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.CustomsData.WB_DeclarationReference = "B001";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.CustomsData.WB_DeclarationReference = "B002";

			Factory.Save();
			asserter.AddToScope(order.Lines.Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.DeclarationReference];
			filter.Property = "B001";
			asserter.AssertMatches("B001 specified should return LineOrder1.", filter, orderLine1);
		}

		#endregion

		#region TestFilterByInwardStyle

		public void TestFilterByInwardStyle()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.CustomsData.WB_InwardStyle = "IWS1";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.CustomsData.WB_InwardStyle = "IWS2";

			Factory.Save();
			asserter.AddToScope(order.Lines.Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.InwardStyle];
			filter.Property = "IWS1";
			asserter.AssertMatches("InwardStyle IWS2 specified should return LineOrder1.", filter, orderLine1);
		}

		#endregion

		#region TestFilterByInwardProcedure

		public void TestFilterByInwardProcedure()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			order.WD_DocketSubType = OrderType.Codes.Customs;

			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.CustomsData.WB_InwardProcedure = "IWP1";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.CustomsData.WB_InwardProcedure = "IWP2";

			Factory.Save();
			asserter.AddToScope(order.Lines.Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.InwardProcedure];
			filter.Property = "IWP1";
			asserter.AssertMatches("InwardProcedure IWP2 specified should return LineOrder1.", filter, orderLine1);
		}

		#endregion

		#region TestFilterByConsignee

		public void TestFilterByCustomsEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");

			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_BondedEntryKey = "MRN001";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_BondedEntryKey = "MRN002";

			Factory.Save();
			asserter.AddToScope(order.Lines.Cast<WhsOrderLine>().ToArray());

			var filter = (ModuleTextFilter)orderLineFilter[OrderLineFilterBusinessObject.Schema.CustomsEntryKey];
			filter.Property = "MRN001";
			asserter.AssertMatches("CustomsEntryKey specified should return LineOrder1.", filter, orderLine1);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			helper = new WhsTestHelperFunctions(Factory);
			orderLineFilter = new OrderLineFilterBusinessObject();
			asserter = new FilterStripAsserter<WhsOrderLine>(Factory, x => $"{x.Order.WD_DocketID} {x.WE_LineNo}");
		}

		WhsTestHelperFunctions helper;
		OrderLineFilterBusinessObject orderLineFilter;
		FilterStripAsserter<WhsOrderLine> asserter;
	}
}
