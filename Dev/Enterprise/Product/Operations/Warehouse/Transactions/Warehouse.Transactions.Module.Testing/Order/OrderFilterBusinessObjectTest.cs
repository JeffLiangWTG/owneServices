using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderFilterBusinessObject))]
	public class OrderFilterBusinessObjectTest : PickableDocketFilterBusinessObjectTest<OrderFilterBusinessObject>
	{
		#region TestServiceTypeDateBooked_WithNoFields

		public void TestServiceTypeDateBooked_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All Orders should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1, order2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithDateField

		public void TestServiceTypeDateBooked_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithServiceTypeField

		public void TestServiceTypeDateBooked_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching service type and booked date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithNoFields

		public void TestServiceTypeDateCompleted_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All Orders should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1, order2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithDateField

		public void TestServiceTypeDateCompleted_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithServiceTypeField

		public void TestServiceTypeDateCompleted_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "Orders with matching service type and completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(order1, order2) => new[] { order1 });
		}

		#endregion

		#region TestFilterReceiveReferenceCrossDock

		public void TestFilterReceiveReferenceCrossDock()
		{
			SetupTestData();
			SetupTestLineData();

			var receive = Helper.CreateWhsReceive(Org1, Whs1, "RECREF1");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, Part11, 10m);
			receive.RunPreSaveValidation();

			Helper.CreateReservePickLine((WhsOrderLine)Line111, inv, 5m);

			Factory.Save();
			DocketCollection = new WhsOrderCollection(Factory);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Receive Reference (Cross Dock)", (ZString)"REC");
			DocketAssert(true, false, false, false);
		}

		#endregion

		#region TestFilterReceiveReferenceCrossDock_IsBlank_ReturnsData

		public void TestFilterReceiveReferenceCrossDock_IsBlank_ReturnsData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var inventory = receive.Inventory[0];

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord1");
			var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			AssertNotNull("Precondition - Divot was created.", orderLine11.ReserveStockIfAbleTo(inventory));

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 6m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 7m);
			Helper.CreatePickNew(order3);

			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord4");
			Factory.Save();

			Asserter.AddToScope(order1, order2, order3, order4);

			var filter = (ModuleTextFilter)FilterStripBizO["Receive Reference (Cross Dock)"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter.Property = "";
			Asserter.AssertMatches("Order with reserve quantity should only be considered.", filter, order1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Order with reserve quantity should not be considered.", filter, order2, order3, order4);
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
			var orderForReceive2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var shortOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick = Helper.CreatePickNew(orderForReceive1, orderForReceive2, shortOrder);

			var unpickedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);

			if (pickUsingInTransitTransfer)
			{
				foreach (var pickLine in pick.GetAllPickLines().ToArray())
				{
					var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
					transferLine.Docket.WD_ExternalReference = "T1";
				}
			}

			Factory.Save();

			Asserter.AddToScope(orderForReceive1, orderForReceive2, shortOrder, unpickedOrder);

			var filter = (ModuleTextFilter)FilterStripBizO["Receive Reference (Pick Slip)"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "O";
			Asserter.AssertMatches("Should not find based on the order reference", filter);

			filter.Property = "T";
			Asserter.AssertMatches("Should not find based on the transfer reference", filter);

			filter.Property = "R";
			Asserter.AssertMatches("Should find both orders with stock allocated from receives.", filter, orderForReceive1, orderForReceive2);

			filter.Property = "R1";
			Asserter.AssertMatches("Should find order1.", filter, orderForReceive1);
		}

		#endregion

		#region TestFilterReceiveReferencePickSlip_DoesNotConsiderReservedStock

		public void TestFilterReceiveReferencePickSlip_DoesNotConsiderReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNotNull("Precondition - Divot was created.", orderLine.ReserveStockIfAbleTo(inventory));

			Factory.Save();

			Asserter.AddToScope(order);

			var filter = (ModuleTextFilter)FilterStripBizO["Receive Reference (Pick Slip)"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "R1";
			Asserter.AssertMatches("Inventory reserved to orders should not be considered.", filter);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			Factory.Save();
			Asserter.AssertMatches("Pick Lines with Zero units should not be considered.", filter);
		}

		#endregion

		#region TestFilterServiceLevel

		public void TestFilterServiceLevel()
		{
			SetupTestData();

			var transportCo = Factory.NewWithValidTestData<OrgHeader>();
			var serviceLevel1 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "SL1";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Service 1";
			var serviceLevel2 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "SL2";
			serviceLevel2.PL_CarrierServiceLevelDescription = "Service 2";
			var serviceLevel3 = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel3.PL_Code = "SL3";
			serviceLevel3.PL_CarrierServiceLevelDescription = "Service 3";

			((WhsOrder)Docket11).TransportCoPK = transportCo.PK;
			Docket11.WD_PL_NKCarrierServiceLevel = "SL1";
			Docket12.WD_PL_NKCarrierServiceLevel = "SL1";
			Docket21.WD_PL_NKCarrierServiceLevel = "SL2";
			Docket22.WD_PL_NKCarrierServiceLevel = "SL3";

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", ZString.Empty);
			DocketAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", new ZString("SL1"));
			DocketAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Carrier Service Level", new ZString("SL2"));
			DocketAssert(false, false, true, false);
		}

		#endregion

		#region TestWorkflowFiltersPresent

		public void TestWorkflowFiltersPresent()
		{
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", (WorkflowModuleFilter)DocketFilter["Milestone Date"]);
		}

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var consignee = "Consignee";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = consignee;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();

			var collection = new OrderFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[consignee]);
			AssertNotNull(collection[consignee + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}
		#endregion

		#region TestFilterConsigneeCompanyName

		public void TestFilterConsigneeCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise";

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			order1.ConsigneePK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_CompanyName = "CargoWise 123";

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).Property = "CargoWise";
			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order1, orders);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order2, orders);
		}

		#endregion

		#region TestFilterTransportCompanyName

		public void TestFilterTransportCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Transport";

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			order1.TransportCoPK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			order2.TransportCoDocAddress.E2_AddressOverride = true;
			order2.TransportCoDocAddress.E2_CompanyName = "CargoWise Transport 123";

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).Property = "CargoWise Transport";
			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order1, orders);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			((ModuleTextFilter)FilterStripBizO["Transport Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order2, orders);
		}

		#endregion

		#region TestFilterByPickGroup

		public void TestFilterByPickGroup()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Heavy Product";
			var pickGroup2 = collection.AddNew();
			pickGroup2.Description = (NoResString)"Light Weight Product";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				SetupReceiveStockOnHand_100Part1And100Part2(data);

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
				var orderLine11 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
				orderLine11.WE_PickGroup = 1;

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
				var orderLine12 = Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
				orderLine12.WE_PickGroup = 2;
				Factory.Save();

				var filter = (ModuleTextFilter)FilterStripBizO["Pick Group"];
				filter.IsActive = true;
				filter.Property = "1";

				var result1 = Factory.Load<WhsOrder>(filter.Query);
				AssertEquals(1, result1.Length);
				AssertCollectionContains(order1, result1);
				AssertCollectionNotContains(order2, result1);
			}
		}

		#endregion

		#region TestFilterByDangerousProducts

		public void TestFilterByDangerousProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			data.Part1.UNDGs.AddNew();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			Factory.Save();

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order3");
			Helper.CreateWhsOrderLine(order3, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order3, data.Part1, 5m);
			Factory.Save();

			Asserter.AddToScope(order1, order2, order3);
			var filter = (ModuleFlagsFilter)DocketFilter["Has Dangerous Goods"];
			filter.IsActive = true;
			filter.Property0 = ZBool.True;

			Asserter.AssertMatches("Should find both orders containing dangerous goods.", filter, order1, order3);

			filter.Property0 = false;
			Asserter.AssertMatches("Should find only one order that contains no dangerous goods.", filter, order2);
		}

		#endregion

		#region TestFilterByOrderWeight, Volume

		public void TestFilterByOrderWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			order1.WD_TotalWeightUnit = "G";
			order1.WD_TotalWeight = 900m;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			order2.WD_TotalWeightUnit = "KG";
			order2.WD_TotalWeight = 2m;
			Factory.Save();

			var filter = (NumberRangeByUnitFilter)DocketFilter["Order Weight"];

			AssertWeightRangeResult(order1, order2, filter, 0m, 1500m, "G", 1, true);
			AssertWeightRangeResult(order1, order2, filter, 0m, 2000m, "G", 2, false);
			AssertWeightRangeResult(order1, order2, filter, 0m, 1m, "KG", 1, true);
			AssertWeightRangeResult(order1, order2, filter, 0m, 2m, "KG", 2, false);
		}

		public void TestFilterByOrderVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			order1.WD_TotalCubic = 100m;
			order1.WD_TotalCubicUnit = "D3";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			order2.WD_TotalCubic = 2m;
			order2.WD_TotalCubicUnit = "M3";
			Factory.Save();

			var filter = (NumberRangeByUnitFilter)DocketFilter["Order Volume"];

			AssertWeightRangeResult(order1, order2, filter, 0m, 1500m, "D3", 1, true);
			AssertWeightRangeResult(order1, order2, filter, 0m, 2000m, "D3", 2, false);
			AssertWeightRangeResult(order1, order2, filter, 0m, 1m, "M3", 1, true);
			AssertWeightRangeResult(order1, order2, filter, 0m, 2m, "M3", 2, false);
		}

		void AssertWeightRangeResult(WhsOrder order1, WhsOrder order2, NumberRangeByUnitFilter filter, ZDecimal property1, ZDecimal property2, ZString unitName, int expectedLength, bool shouldCheckNotContain)
		{
			filter.IsActive = true;
			filter.Property1 = property1;
			filter.Property2 = property2;
			filter.Property = unitName;

			var result = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(expectedLength, result.Length);
			AssertCollectionContains(order1, result);
			if (!shouldCheckNotContain)
			{
				AssertCollectionContains(order2, result);
			}
			else
			{
				AssertCollectionNotContains(order2, result);
			}
		}

		#endregion

		#region TestFilterByNumberOfOrderLines

		public void TestFilterByNumberOfOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Number of Order Lines"];
			filter.IsActive = true;
			filter.Property1 = 1m;
			filter.Property2 = 2m;
			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(0, result1.Length);

			filter.Property1 = 1m;
			filter.Property2 = 4m;
			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(2, result2.Length);

			filter.Property1 = 4m;
			filter.Property2 = 5m;
			var result3 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(1, result3.Length);
		}

		#endregion

		#region TestFilterBySingleOrderLine

		public void TestFilterBySingleOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);
			Factory.Save();

			//Only Order with single Order Line
			var filter = (ModuleFlagsFilter)DocketFilter["Single Item Orders"];
			filter.IsActive = true;
			filter.Property0 = ZBool.True;
			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(2, result1.Length);
			AssertCollectionContains(order1, result1);
			AssertCollectionContains(order2, result1);
			AssertCollectionNotContains(order3, result1);

			//Only Order with more than one Order Line
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Factory.Save();

			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals(1, result2.Length);
			AssertCollectionNotContains(order1, result2);
			AssertCollectionContains(order2, result2);
			AssertCollectionNotContains(order3, result2);
		}

		#endregion

		#region TestFilterProductCount

		public void TestFilterProductCount_FilterExists()
		{
			AssertNotNull("Filter Product Count exists.", (ModuleNumberRangeFilter)DocketFilter["Product Count"]);
		}

		public void TestFilterProductCount_EqualsCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 1;

			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result1.Length);
			AssertCollectionNotContains(order1, result1);
			AssertCollectionContains(order2, result1);

			filter.Property1 = 2;
			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result2.Length);
			AssertCollectionContains(order1, result2);
			AssertCollectionNotContains(order2, result2);
		}

		public void TestFilterProductCount_LessThanCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 1;

			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result1.Length);
			AssertCollectionNotContains(order1, result1);
			AssertCollectionContains(order2, result1);

			filter.Property2 = 2;
			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 2, result2.Length);
			AssertCollectionContains(order1, result2);
			AssertCollectionContains(order2, result2);
		}

		public void TestFilterProductCount_GreaterThanCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 4m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 1;

			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 2, result1.Length);
			AssertCollectionContains(order1, result1);
			AssertCollectionContains(order2, result1);

			filter.Property1 = 2;
			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result2.Length);
			AssertCollectionContains(order1, result2);
			AssertCollectionNotContains(order2, result2);
		}

		public void TestFilterProductCount_BetweenCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Rec3", part3, 100m, data.Whs1.FindLocation("A-3"), "PLT4");
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 4m);
			Helper.CreateWhsOrderLine(order1, part3, 6m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order2, part3, 15m);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 8m);
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)DocketFilter["Product Count"];
			filter.IsActive = true;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 0;
			filter.Property2 = 1;

			var result1 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result1.Length);
			AssertCollectionNotContains(order1, result1);
			AssertCollectionNotContains(order2, result1);
			AssertCollectionContains(order3, result1);

			filter.Property1 = 2;
			filter.Property2 = 3;
			var result2 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 2, result2.Length);
			AssertCollectionContains(order1, result2);
			AssertCollectionContains(order2, result2);
			AssertCollectionNotContains(order3, result2);

			filter.Property1 = 3;
			filter.Property2 = 3;
			var result3 = Factory.Load<WhsOrder>(filter.Query);
			AssertEquals("Correct number of order returned", 1, result3.Length);
			AssertCollectionContains(order1, result3);
			AssertCollectionNotContains(order2, result3);
			AssertCollectionNotContains(order3, result3);
		}

		#endregion

		#region TestFilterByConsignee

		public override void TestFilterByConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 1m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order3", data.Part2, 10m);

			var consignee1 = Helper.CreateClient("CNE1");
			var consignee2 = Helper.CreateClient("CNE2");
			var consignee3 = Helper.CreateClient("CNE3");
			consignee3.OH_IsActive = false;

			order1.ConsigneePK = consignee1.PK;
			order2.ConsigneePK = consignee2.PK;
			order3.ConsigneePK = consignee3.PK;

			Factory.Save();

			Asserter.AddToScope(order1, order2, order3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Consignee specified should return all orders.", filter, order1, order2, order3);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Consignee 1 specified should return order1", filter, order1);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = consignee2.PK;
			Asserter.AssertMatches("Consignee 2 specified should return order2.", filter, order2);

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = consignee3.PK;
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			Asserter.AssertMatches("Consignee 3 specified should return order2.", filter, order3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Consignee specified should return all orders.", filter, order1, order2, order3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Consignee 1 specified should return order2 and order3.", filter, order2, order3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = consignee2.PK;
			Asserter.AssertMatches("Consignee 2 specified should return order1 and order3.", filter, order1, order3);

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = consignee3.PK;
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			Asserter.AssertMatches("Consignee 3 specified should return order2.", filter, order1, order2);
		}

		#endregion

		#region TestFilterByTransportCo

		protected override bool SupportsTransportCoFilters => true;

		protected override WhsPickableDocket GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, docketReference, data.Part1, 1m);
			if (transportCo != null)
			{
				order.TransportCoPK = transportCo.PK;
			}

			return order;
		}

		#endregion

		#region Test Distribution Centre Filters

		protected override bool SupportsDistributionCentreFilters => true;

		#region TestFilterDistributionCentreName

		protected override void TestFilterDistributionCentreNameCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_FullName = "CargoWise Distribution Centre";

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			order1.DistributionCentreDocAddress.OrganisationPK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			order2.DistributionCentreDocAddress.E2_AddressOverride = true;
			order2.DistributionCentreDocAddress.E2_CompanyName = "CargoWise Distribution Centre 123";

			Factory.Save();

			Asserter.AddToScope(order1, order2);

			var filter = (ModuleTextFilter)FilterStripBizO["Distribution Center Name"];
			filter.Property = "CargoWise Distribution Centre";
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			Asserter.AssertMatches("Should return all orders.", filter, order1, order2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Asserter.AssertMatches("Should return all orders.", filter, order1, order2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Asserter.AssertMatches("Should return only order1.", filter, order1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			Asserter.AssertMatches("Should return no orders.", filter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			Asserter.AssertMatches("Should return no orders.", filter);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Asserter.AssertMatches("Should return only order2.", filter, order2);
		}

		#endregion

		#region TestFilterDistributionCentre

		protected override void TestFilterDistributionCentreCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var docket1 = GetOrderForDistributionCentreFilterTesting(data, "D1", distributionCentre: distributionCentre1);
			var docket2 = GetOrderForDistributionCentreFilterTesting(data, "D2", distributionCentre: distributionCentre2);
			var docket3 = GetOrderForDistributionCentreFilterTesting(data, "D3");

			Factory.Save();

			Asserter.AddToScope(docket1, docket2, docket3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Distribution Centre specified should return all dockets.", filter, docket1, docket2, docket3);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = distributionCentre1.PK;
			Asserter.AssertMatches("Distribution Centre 1 specified should return docket1", filter, docket1);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = distributionCentre2.PK;
			Asserter.AssertMatches("Distribution Centre 2 specified should return docket2.", filter, docket2);
		}

		protected override void TestFilterDistributionCentre_NotEqual_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var docket1 = GetOrderForDistributionCentreFilterTesting(data, "D1", distributionCentre: distributionCentre1);
			var docket2 = GetOrderForDistributionCentreFilterTesting(data, "D2", distributionCentre: distributionCentre2);
			var docket3 = GetOrderForDistributionCentreFilterTesting(data, "D3");

			Factory.Save();

			Asserter.AddToScope(docket1, docket2, docket3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Distribution Centre specified should return all dockets.", filter, docket1, docket2, docket3);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = distributionCentre2.PK;
			Asserter.AssertMatches("Distribution Centre 1 specified should return docket1", filter, docket1);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = distributionCentre1.PK;
			Asserter.AssertMatches("Distribution Centre 2 specified should return docket2.", filter, docket2);
		}

		protected override void TestFilterDistributionCentre_IsBlank_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var docket1 = GetOrderForDistributionCentreFilterTesting(data, "D1", distributionCentre: distributionCentre1);
			var docket2 = GetOrderForDistributionCentreFilterTesting(data, "D2", distributionCentre: distributionCentre2);
			var docket3 = GetOrderForDistributionCentreFilterTesting(data, "D3");

			Factory.Save();

			Asserter.AddToScope(docket1, docket2, docket3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("IsBlank should return only docket3.", filter, docket3);
		}

		protected override void TestFilterDistributionCentre_IsNotBlank_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var distributionCentre1 = Helper.CreateClient("DC1");
			var distributionCentre2 = Helper.CreateClient("DC2");

			var docket1 = GetOrderForDistributionCentreFilterTesting(data, "D1", distributionCentre: distributionCentre1);
			var docket2 = GetOrderForDistributionCentreFilterTesting(data, "D2", distributionCentre: distributionCentre2);
			var docket3 = GetOrderForDistributionCentreFilterTesting(data, "D3");

			Factory.Save();

			Asserter.AddToScope(docket1, docket2, docket3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("IsNotBlank should return docket1 and docket2.", filter, docket1, docket2);
		}

		WhsOrder GetOrderForDistributionCentreFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgHeader distributionCentre = null)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, docketReference, data.Part1, 1m);
			if (distributionCentre != null)
			{
				order.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			}

			return order;
		}

		#endregion

		#endregion

		#region TestFilterConsigneeState

		public void TestFilterConsigneeState()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.Addresses[0].OA_State = "WTGState";

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			order1.ConsigneePK = data.Org1.PK;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_State = "WTGState123";

			Factory.Save();

			var consigneeFilter = ((ModuleTextFilter)FilterStripBizO["Consignee State"]);
			consigneeFilter.Property = "WTGState";
			consigneeFilter.IsActive = true;

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			var orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(2, orders.Length);
			AssertCollectionContains(order1, orders);
			AssertCollectionContains(order2, orders);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order1, orders);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(0, orders.Length);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			orders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, orders.Length);
			AssertCollectionContains(order2, orders);
		}

		#endregion

		#region TestFilterConsigneeCountry

		public void TestFilterConsigneeCountry()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org1 = data.Org1;
			var org2 = Helper.CreateClient("222", "222");
			var org3 = Helper.CreateClient("333", "333");

			var order1 = Helper.CreateWhsOrder(org1, data.Whs1, "order1");
			order1.ConsigneePK = org1.PK;
			org1.MainAddress.OA_RN_NKCountryCode = "NZ";

			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "order2");
			order2.ConsigneePK = org2.PK;
			order2.ConsigneeDocAddress.E2_AddressOverride = true;
			order2.ConsigneeDocAddress.E2_RN_NKCountryCode = "UK";

			var order3 = Helper.CreateWhsOrder(org3, data.Whs1, "order3");
			order3.ConsigneePK = org3.PK;
			order3.ConsigneeDocAddress.E2_AddressOverride = true;
			order3.ConsigneeDocAddress.E2_RN_NKCountryCode = "";

			Factory.Save();
			Asserter.AddToScope(order1, order2, order3);
			var orderFilter = GetNewFilterStripBusinessObject();
			var filter = (ModuleNkFilter)orderFilter["Consignee Country / Region"];
			filter.IsActive = true;

			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should find all orders.", orderFilter.Filter, order1, order2, order3);

			filter.Property = "NZ";
			Asserter.AssertMatches("When filter is NZ, should find order 1.", orderFilter.Filter, order1);

			filter.Property = "UK";
			Asserter.AssertMatches("When filter is UK, should find order 2.", orderFilter.Filter, order2);

			filter.Property = "US";
			Asserter.AssertMatches("When filter is US, should find null", orderFilter.Filter);
		}

		#endregion

		#region TestFilterTransportJob

		public void TestFilterTransportJob()
		{
			SetupTestData();

			// PT
			var cartage1 = Helper.CreateCartageJob((WhsOrder)Docket11);
			cartage1.JJ_ConsignmentID = "PT1";

			// TB
			var booking1 = CreateBooking(Docket12.PK, "BOOKING 1");
			var booking2 = CreateBooking(Docket21.PK, "BOOKING PT");
			var booking3 = CreateBooking(Docket22.PK, "BOOKING LT");

			// PT Via TB
			var portTransportViaTB1 = Factory.New<ICommonCartage>();
			portTransportViaTB1.JJ_ParentID = booking2.PK;
			portTransportViaTB1.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			portTransportViaTB1.JJ_ConsignmentID = "PT2";

			// LT via TB
			var consignmentConsol = Factory.New<IDtbConsignmentConsolidation>();
			consignmentConsol.KB_ParentID = booking3.PK;
			consignmentConsol.KB_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var consignment = Factory.New<IDtbBookingConsignment>();
			consignment.KM_KB_Booking = consignmentConsol.PK;
			consignment.KM_JobID = "CSN1";
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, ZString.Empty);
			DocketAssert(true, true, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, new ZString("PT1"));
			DocketAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, new ZString("BOOKING 1"));
			DocketAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, new ZString("PT2"));
			DocketAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, new ZString("CSN1"));
			DocketAssert(false, false, false, true);

			cartage1.JJ_ConsignmentID = "TEST1";
			portTransportViaTB1.JJ_ConsignmentID = "TEST2";
			consignment.KM_JobID = "TEST1";
			booking1.KM_JobID = "TEST2";
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, OrderFilterBusinessObject.Schema.TransportJobNumber, new ZString("TEST"));
			DocketAssert(true, true, true, true);

			var filter = ((ModuleTextFilter)DocketFilter[OrderFilterBusinessObject.Schema.TransportJobNumber]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			DocketAssert(false, false, false, false);

			filter = ((ModuleTextFilter)DocketFilter[OrderFilterBusinessObject.Schema.TransportJobNumber]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			DocketAssert(true, true, true, true);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			DocketAssert(false, false, false, false);

			((BusinessObject)cartage1).Delete();
			((BusinessObject)portTransportViaTB1).Delete(); // TB still exists, so Docket21 still has transport job
			Factory.Save();
			DocketAssert(true, false, false, false);

			var booking2_Consol = Factory.Load<IDtbBookingConsolidation>(booking2.KM_KB_Booking);
			((BusinessObject)booking2_Consol).Delete();
			Factory.Save();
			DocketAssert(true, false, true, false);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			DocketAssert(false, true, false, true);

			Factory.Load<IDtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobType, "BKG")).Cast<BusinessObject>().ForEach(b => b.Delete());
			Factory.Load<IDtbConsignmentConsolidation>(new ZQuery(DtbBookingConsolidationSchema.KB_JobType, "CSN")).Cast<BusinessObject>().ForEach(b => b.Delete());
			Factory.Save();

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			DocketAssert(true, true, true, true);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			DocketAssert(false, false, false, false);
		}

		IDtbBooking CreateBooking(ZGuid parentID, string jobID)
		{
			var consol1 = Factory.New<IDtbBookingConsolidation>();
			consol1.KB_ParentID = parentID;
			consol1.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking1 = Factory.New<IDtbBooking>();
			booking1.KM_KB_Booking = consol1.PK;
			booking1.KM_JobID = jobID;
			return booking1;
		}

		#endregion

		#region TestFilterOrdersWithPackagesOnHold

		public void TestFilterOrdersWithPackagesOnHold()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			Helper.CreatePickNew(order1);
			order1.PackageJob.Packages.AddNew().KP_IsHeld = true;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20);
			Helper.CreatePickNew(order2);
			order2.PackageJob.Packages.AddNew().KP_IsHeld = false;

			Factory.Save();

			var filter = ((ModuleFlagsFilter)FilterStripBizO[OrderFilterBusinessObject.HasHeldPackagesFilterName]);
			filter.Property0 = true;
			filter.IsActive = true;

			var ordersWithHeldPackages = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, ordersWithHeldPackages.Length);
			AssertCollectionContains(order1, ordersWithHeldPackages);

			filter.Property0 = false;

			var ordersWithoutHeldPackages = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals(1, ordersWithoutHeldPackages.Length);
			AssertCollectionContains(order2, ordersWithoutHeldPackages);
		}

		#endregion

		#region TestFilterAuditStatus

		public void TestFilterAuditStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10); //Should be NotRequired
			Helper.CreatePickNew(order1);
			order1.PackageJob.Packages.AddNew().KP_PackageID = "P1";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20); //Should be Audit pending
			Helper.CreatePickNew(order2);
			order2.WD_QualityAuditRequired = true;
			order2.PackageJob.Packages.AddNew().KP_PackageID = "P2";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 20); //Should be Audit Passed
			Helper.CreatePickNew(order3);
			order3.WD_QualityAuditRequired = true;
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P3";
			WhsPackageAuditManager.AuditPackage(package3);

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 20); //Should be Audit Failed when required
			Helper.CreatePickNew(order4);
			order4.WD_QualityAuditRequired = true;
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P4";
			Helper.CreateWhsPackageAuditWithLineFailure(order4, package4.KP_PackageID, data.Part1, 20m, 18m);

			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part2, 20); //Should be Audit pending because all the audits have not been completed
			Helper.CreatePickNew(order5);
			order5.WD_QualityAuditRequired = true;
			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P5_1";
			Helper.CreateWhsPackageAuditWithLineFailure(order5, package5.KP_PackageID, data.Part1, 0m, 5m);
			order5.PackageJob.Packages.AddNew().KP_PackageID = "P5_2";

			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part2, 20); //Should be Audit Failed when not required
			Helper.CreatePickNew(order6);
			var package6 = order6.PackageJob.Packages.AddNew();
			package6.KP_PackageID = "P6";
			Helper.CreateWhsPackageAuditWithLineFailure(order6, package6.KP_PackageID, data.Part1, 0m, 5m);

			var order7 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", data.Part2, 30); // Order without packages but with audit required: Pending Audit
			order7.WD_QualityAuditRequired = true;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO[OrderFilterBusinessObject.AuditStatusFilterName];
			filter.IsActive = true;

			filter.Property = OrderAuditStatus.Codes.NotRequired;
			var ordersWithNoRequiredAuditStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("NotRequiredAudit Orders should have 1 elements.", 1, ordersWithNoRequiredAuditStatus.Length);
			AssertCollectionContains("order1 should be included in NotRequiredAudit orders.", order1, ordersWithNoRequiredAuditStatus);

			filter.Property = OrderAuditStatus.Codes.Failed;
			var ordersWithFailedAuditStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("AuditFailed Orders should have 2 elements.", 2, ordersWithFailedAuditStatus.Length);
			AssertCollectionContains("order4 should be included in FailedAuditOrders.", order4, ordersWithFailedAuditStatus);
			AssertCollectionContains("order6 should be included in FailedAuditOrders.", order6, ordersWithFailedAuditStatus);

			filter.Property = OrderAuditStatus.Codes.Passed;
			var ordersWithPassedAuditStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("PassedAudit Orders should have 1 element.", 1, ordersWithPassedAuditStatus.Length);
			AssertCollectionContains("order3 should be included in PassedAuditOrders.", order3, ordersWithPassedAuditStatus);
			AssertCollectionNotContains("order7 should NOT be included in PassedAuditOrders.", order7, ordersWithPassedAuditStatus);

			filter.Property = OrderAuditStatus.Codes.Pending;
			var ordersWithPendingAuditStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("PendingAudit orders should have 3 elements.", 3, ordersWithPendingAuditStatus.Length);
			AssertCollectionContains("order2 should be included in PendingAuditOrders", order2, ordersWithPendingAuditStatus);
			AssertCollectionContains("order5 should be included in PendingAuditOrders", order5, ordersWithPendingAuditStatus);
			AssertCollectionContains("order7 should be included in PendingAuditOrders", order7, ordersWithPendingAuditStatus);
		}

		#endregion

		#region TestFilterOrderStatus

		public void TestFilterOrderStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "4", transportUnit: truck, startTime: DateTimeOffset.Now);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "9", transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m); // ENT #1
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 10m); // ENT #2
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m); // ATP
			Helper.CreatePickNew(order2);
			Factory.Save();
			AssertEquals("Precondition", DocketStatus.Codes.AttachedToPick, order2.WD_DocketStatus);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m); // STA
			Helper.CreatePickNew(order3);
			var pickLine1 = order3.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 10m); // LOA
			Helper.CreatePickNew(order4);
			var pickLine2 = order4.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order4.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order4.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			Factory.Save();

			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 10m); // DEP
			Helper.CreatePickNew(order5);
			var pickLine3 = order5.Lines.Single().PickLines.Single();
			pickLine3.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package2 = order5.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order5.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			Helper.DepartPackageNow(loadPkgPackagePivot2);
			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Order Status"];
			filter.IsActive = true;

			filter.Property = DocketStatus.Codes.Entered;
			var ordersWithENTStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Entered Orders should have 2 elements.", 2, ordersWithENTStatus.Length);
			AssertCollectionContains("order1 should be included in ENT status orders.", order1, ordersWithENTStatus);
			AssertCollectionContains("order6 should be included in ENT status orders.", order6, ordersWithENTStatus);

			filter.Property = DocketStatus.Codes.AttachedToPick;
			var ordersWithATPStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("AttachedToPick Orders should have 1 element.", 1, ordersWithATPStatus.Length);
			AssertCollectionContains("order4 should be included in FailedAuditOrders.", order2, ordersWithATPStatus);

			filter.Property = WhsOrderStatus.Codes.Staged;
			var ordersWithSTAStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Staged Orders should have 1 element.", 1, ordersWithSTAStatus.Length);
			AssertCollectionContains("order3 should be included in PassedAuditOrders.", order3, ordersWithSTAStatus);

			filter.Property = WhsOrderStatus.Codes.Loaded;
			var ordersWithLOAStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Loaded orders should have 1 elements.", 1, ordersWithLOAStatus.Length);
			AssertCollectionContains("order2 should be included in PendingAuditOrders", order4, ordersWithLOAStatus);

			filter.Property = WhsOrderStatus.Codes.Departed;
			var ordersWithDEPStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Departed orders should have 1 elements.", 1, ordersWithDEPStatus.Length);
			AssertCollectionContains("order5 should be included in PendingAuditOrders", order5, ordersWithDEPStatus);

			filter.Property = WhsOrderStatus.Codes.Loading;
			AssertEquals("Loading orders should have 0 elements.", 0, Factory.Load<WhsOrder>(FilterStripBizO.Filter).Length);
		}

		#endregion

		#region TestFilterDepartedStatus

		public void TestFilterDepartedStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "4", transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m); // ATP
			var pick1 = Helper.CreatePickNew(order1);
			Factory.Save();
			pick1.FinaliseAllOrders();
			var orderstatus1 = Factory.Load<WhsOrderStatusView>(order1.PK);
			AssertEquals(DocketStatus.Codes.AttachedToPick, order1.WD_DocketStatus);
			AssertEquals(DocketStatus.Codes.AttachedToPick, orderstatus1.WOS_OrderStatus);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m); // DEP
			Helper.CreatePickNew(order2);
			var pickLine2 = order2.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load1);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";
			Helper.DepartPackageNow(loadPkgPackagePivot2);
			Factory.Save();
			var orderstatus2 = Factory.Load<WhsOrderStatusView>(order2.PK);
			AssertEquals(DocketStatus.Codes.Picking, order2.WD_DocketStatus);
			AssertEquals(WhsOrderStatus.Codes.Departed, orderstatus2.WOS_OrderStatus);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m); // DEP
			var pick3 = Helper.CreatePickNew(order3);
			Factory.Save();
			pick3.FinaliseAllOrders();
			pick3.FinalisePick();
			Factory.Save();
			var orderstatus3 = Factory.Load<WhsOrderStatusView>(order3.PK);
			AssertEquals(WhsOrderStatus.Codes.Departed, order3.WD_DocketStatus);
			AssertEquals(WhsOrderStatus.Codes.Departed, orderstatus3.WOS_OrderStatus);

			var filter = (ModuleTextFilter)FilterStripBizO["Order Status"];
			filter.IsActive = true;
			filter.Property = WhsOrderStatus.Codes.Departed;
			var ordersWithDEPStatus = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Departed orders should have 2 elements.", 2, ordersWithDEPStatus.Length);
			AssertCollectionContains("order2 should be included in Departed Status Filter", order2, ordersWithDEPStatus);
			AssertCollectionContains("order3 should be included in Departed Status Filter", order3, ordersWithDEPStatus);
		}

		#endregion

		#region TestFilterFinalizedStatus

		public void TestFilterFinalizedStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m); // ENT #1
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 10m); // ENT #2
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 10m); // CAN #1
			order6.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m); // ATP
			Helper.CreatePickNew(order2);
			Factory.Save();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m); // FIN
			var pick1 = Helper.CreatePickNew(order3);
			pick1.FinaliseAllOrders();
			Factory.Save();
			AssertEquals("Order3 is finalised", true, order3.IsFinalised);

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 10m); // FIN
			var pick2 = Helper.CreatePickNew(order4);
			pick2.FinaliseAllOrders();
			Factory.Save();
			AssertEquals("Order4 is finalised", true, order4.IsFinalised);

			var filter = (ModuleTextFilter)FilterStripBizO[OrderFilterBusinessObject.Schema.FinalizedStatus];
			filter.IsActive = true;

			filter.Property = FinalisedStatus.Codes.All;
			var allOrders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("All Orders should have 6 elements.", 6, allOrders.Length);
			AssertCollectionContains("order1 should be included in all orders.", order1, allOrders);
			AssertCollectionContains("order2 should be included in all orders.", order2, allOrders);
			AssertCollectionContains("order3 should be included in all orders.", order3, allOrders);
			AssertCollectionContains("order4 should be included in all orders.", order4, allOrders);
			AssertCollectionContains("order5 should be included in all orders.", order5, allOrders);
			AssertCollectionContains("order6 should be included in all orders.", order6, allOrders);

			filter.Property = FinalisedStatus.Codes.IsFinalised;
			var finalisedOrders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Finalised Orders should have 2 elements.", 2, finalisedOrders.Length);
			AssertCollectionContains("order3 should be included in finalised orders.", order3, finalisedOrders);
			AssertCollectionContains("order4 should be included in finalised orders.", order4, finalisedOrders);
			AssertCollectionNotContains("order6 should NOT be included in finalised orders.", order6, finalisedOrders);

			filter.Property = FinalisedStatus.Codes.IsNotFinalised;
			var notFinalisedOrders = Factory.Load<WhsOrder>(FilterStripBizO.Filter);
			AssertEquals("Not Finalised Orders should have 3 elements.", 3, notFinalisedOrders.Length);
			AssertCollectionContains("order1 should be included in not finalised orders.", order1, notFinalisedOrders);
			AssertCollectionContains("order2 should be included in not finalised orders.", order2, notFinalisedOrders);
			AssertCollectionContains("order5 should be included in not finalised orders.", order5, notFinalisedOrders);
			AssertCollectionNotContains("order6 should be NOT included in not finalised orders.", order6, notFinalisedOrders);
		}

		#endregion

		#region SetupReceive

		void SetupReceiveStockOnHand_100Part1And100Part2(TestDataSimpleEnvironment data)
		{
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "ID123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE2", data.Part2, 100m, data.Whs1.FindLocation("A-2"), "ID456");
			Factory.Save();
		}

		#endregion

		#region TestFilterPackageID

		protected override bool SupportsPackageIdFilter => true;

		#endregion

		#region TestFilterHandlingUnit

		protected override void TestHandlingUnitCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 20m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			Helper.CreatePickNew(order3);
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

			Asserter.AddToScope(order1, order2, order3);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should return only Order1 (Order1 contain a Package with Handling Unit 'HU1').", filter, order1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Equals 'HU2' should return only Order2 (Order2 contain a Package with Handling Unit 'HU2').", filter, order2);
			filter.Property = "HU3";
			Asserter.AssertMatches("Equals 'HU3' should return only Order3 (Order2 contain a Package with Handling Unit 'HU3').", filter, order3);
			filter.Property = "H";
			Asserter.AssertMatches("Equals 'H' should return no Orders.", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1";
			Asserter.AssertMatches("Starts With 'HU1' should return only Order1 (Order1 contain a Package with Handling Unit starts with 'HU1').", filter, order1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Starts With 'HU2' should return only Order1 (Order1 contain a Package with Handling Unit starts with 'HU2').", filter, order2);
			filter.Property = "H";
			Asserter.AssertMatches("Starts With 'H' should return Order1, Order2 and Order3 which contain a Package with Handling Unit starts with 'H'.", filter, order1, order2, order3);
			filter.Property = "1";
			Asserter.AssertMatches("Starts With '1' should return no Orders.", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1";
			Asserter.AssertMatches("Contains 'HU1' should return Order1, Order2 and Order3 which contain a Package with Handling Unit contains 'HU1'.", filter, order1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Contains 'HU2' should return Order2 and Order3 which contain a Package with Handling Unit contains 'HU2'.", filter, order2);
			filter.Property = "U";
			Asserter.AssertMatches("Contains 'U' should return Order2 and Order3 which contain a Package with Handling Unit contains 'ub'.", filter, order1, order2, order3);
			filter.Property = "4";
			Asserter.AssertMatches("Contains '4' should return no Orders.", filter);
		}

		public void TestHandlingUnit_DoesNotReturnUnpacked_ReturnPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
		
			Helper.CreatePickNew(order1);
	
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);
			var handlingUnit1 = PackingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = PackingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			
			PackingHelper.PackHandlingUnitAndUnpack(handlingUnitPackage1, package1);

			Factory.Save();

			Asserter.AddToScope(order1);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal unpacked

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should not return Order1 as it is unpacked from Handling Unit 'HU1').", filter);

			// equal packed
			PackingHelper.PackHandlingUnit(handlingUnitPackage1, package1, handlingUnitPackage1);
			Factory.Save();

			Asserter.AddToScope(order1);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should return Order1 (Order1 contain a Package with Handling Unit 'HU1').", filter, order1);
		}

		public void TestHandlingUnit_PackedInAnotherHandlingUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 20m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			Helper.CreatePickNew(order3);
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

			Asserter.AddToScope(order1, order2, order3);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1";
			Asserter.AssertMatches("Equals 'HU1' should return only Order1 (Order1 contain a Package with Handling Unit 'HU1'.", filter, order1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Equals 'HU2' should return only Order2 (Order2 contain a Package with Handling Unit 'HU2').", filter, order2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Equals 'TopHU1' should return Order1 and Order3 which contain a Package with Handling Unit 'TopHU1'.", filter, order1, order3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Equals 'TopHU2' should return only Order2 (Order2 contain a Package with Handling Unit 'TopHU2').", filter, order2);
			filter.Property = "H";
			Asserter.AssertMatches("Equals 'H' should return no Orders.", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1";
			Asserter.AssertMatches("Starts With 'HU1' should return only Order1 (Order1 contain a Package with Handling Unit starts with 'HU1'.", filter, order1);
			filter.Property = "HU2";
			Asserter.AssertMatches("Starts With 'HU2' should return only Order2 (Order2 contain a Package with Handling Unit starts with 'HU2').", filter, order2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Starts With 'TopHU1' should return Order1 and Order3 which contain a Package with Handling Unit starts with 'TopHU1'.", filter, order1, order3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Starts With 'TopHU2' should return only Order2 (Order2 contain a Package with Handling Unit starts with 'TopHU2').", filter, order2);
			filter.Property = "Top";
			Asserter.AssertMatches("Starts With 'Top' should return Order1, Order2 and Order3 which contain a Package with Handling Unit starts with 'Top'.", filter, order1, order2, order3);
			filter.Property = "U";
			Asserter.AssertMatches("Starts With 'U' should return no Orders.", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1";
			Asserter.AssertMatches("Contains 'HU1' should return Order1 and Order2 which contain a Package with Handling Unit contains 'HU1'.", filter, order1, order3);
			filter.Property = "HU2";
			Asserter.AssertMatches("Contains 'HU2' should return only Order2 (Order2 contain a Package with Handling Unit contains 'HU2').", filter, order2);
			filter.Property = "TopHU1";
			Asserter.AssertMatches("Contains 'TopHU1' should return Order1 Order3 which contain a Package with Handling Unit contains 'TopHU1'.", filter, order1, order3);
			filter.Property = "TopHU2";
			Asserter.AssertMatches("Contains 'TopHU2' should return only Order2 (Order2 contain a Package with Handling Unit contains 'TopHU2').", filter, order2);
			filter.Property = "Top";
			Asserter.AssertMatches("Contains 'Top' should return Order1, Order2 and Order3 which contain a Package with Handling Unit contains 'Top'.", filter, order1, order2, order3);
			filter.Property = "4";
			Asserter.AssertMatches("Contains '4' should return no Orders.", filter);
		}

		public void TestHandlingUnit_MultiplePackageInOneOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 60m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
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

			Factory.Save();

			Asserter.AddToScope(order1, order2);
			var filter = (ModuleTextFilter)FilterStripBizO["Handling Unit"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Equals 'HU1_1' should return only Order1 (Order1 contain a Package with Handling Unit 'HU1_1').", filter, order1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Equals 'HU1_2' should return only Order1 (Order1 contain a Package with Handling Unit 'HU1_2').", filter, order1);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Starts With 'HU1_1' should return only Order1 (Order1 contain a Package with Handling Unit starts with 'HU1_1').", filter, order1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Starts With 'HU1_2' should return only Order1 (Order1 contain a Package with Handling Unit starts with 'HU1_2')').", filter, order1);
			filter.Property = "HU";
			Asserter.AssertMatches("Starts With 'HU' should return Order1 and Order2 which contain a Package with Handling Unit starts with 'HU'.", filter, order1, order2);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "HU1_1";
			Asserter.AssertMatches("Contains 'HU1_1' should return only Order1 (Order1 contain a Package with Handling Unit contains 'HU1_1')'.", filter, order1);
			filter.Property = "HU1_2";
			Asserter.AssertMatches("Contains 'HU1_2' should return only Order1 (Order1 contain a Package with Handling Unit contains 'HU1_2').", filter, order1);
			filter.Property = "HU";
			Asserter.AssertMatches("Contains 'HU' should return Order1 and Order2 which contain a Package with Handling Unit contains 'HU'.", filter, order1, order2);
		}

		protected override bool SupportsHandlingUnitFilter => true;

		#endregion

		#region Test Filter Package Type

		protected override bool SupportsPackageTypeFilter => true;

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode => WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;

		#endregion

		#region TestFilterNotInvoiced

		protected override bool IsNotInvoicedFilterUsed => true;

		protected override AccChargeCode GetJobSpecificChargeCode()
		{
			return Helper.CreateChargeCode("WOUT", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
		}

		protected override WhsPickableDocket CreateFinalisedDocket(TestDataSimpleEnvironment data, ZString reference, ZDateTime finalisedDate)
		{
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + reference, finalisedDate.ToOffset(), data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, reference, finalisedDate.ToOffset(), data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			return order;
		}

		#endregion

		#region TestFilter_TrolleyNumber

		public void TestFilter_TrolleyNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T2
			var pick1 = Helper.CreatePickNew(order1, order2);
			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m); // assigned to T1
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m); // not assigned to any trolley
			var pick2 = Helper.CreatePickNew(order3, order4);
			var package3 = order3.PackageJob.Packages.AddNew();
			order4.PackageJob.Packages.AddNew();
			pick2.FinaliseAllOrders();
			pick2.FinalisePick();
			AssertEquals("Precondition - ensure pick1 is NOT finalised.", false, pick1.IsFinalised);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick2);

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
			Asserter.AddToScope(order1, order2, order3, order4);

			var filter = (ModuleTextFilter)FilterStripBizO[OrderFilterBusinessObject.Schema.TrolleyNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RandomText";
			Asserter.AssertMatches("Should find nothing as none of the trolleys have such name", filter);

			filter.Property = "T1";
			Asserter.AssertMatches("Should find all orders for the trolley, even finalised one.", filter, order1, order3);

			filter.Property = "T2";
			Asserter.AssertMatches("Should find all orders for the trolley.", filter, order2);

			filter.Property = "T";
			Asserter.AssertMatches("Should find orders from both trolleys as their trolley number starts with 'T'.", filter, order1, order2, order3);

			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should not apply any filters, and find all orders.", filter, order1, order2, order3, order4);

			filter.Property = "";
			filter.ComparisonOperator = "is blank";
			Asserter.AssertMatches("Should find orders with Trolley Number is blank.", filter, order4);

			filter.Property = "";
			filter.ComparisonOperator = "is not blank";
			Asserter.AssertMatches("Should find orders with Trolley Number is not blank.", filter, order1, order2, order3);
		}

		public void TestFilter_TrolleyNumber_IgnoresFinalisedTrolleys()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1, but trolley job is finalised
			Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T1
			Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();

			// create 2 trolley Jobs for same trolley
			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley.PK, "FIN");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);

			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package2.PK, 1);
			Factory.Save();

			// assert filters
			Asserter.AddToScope(order1, order2);

			var filter = (ModuleTextFilter)FilterStripBizO[OrderFilterBusinessObject.Schema.TrolleyNumber];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RandomText";
			Asserter.AssertMatches("Should find nothing as none of the trolleys have such name", filter);

			filter.Property = "T1";
			Asserter.AssertMatches("Should find order for the trolley, but only from unfinalised trolley job.", filter, order2);

			filter.Property = "";
			Asserter.AssertMatches("When filter is empty, should not apply any filters, and find all orders.", filter, order1, order2);
		}

		#endregion

		#region TestFilter_LoadId

		public void TestFilter_LoadID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000001");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load1.PK;

			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000002");
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.WD_WLO_PlannedLoad = load2.PK;

			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL20000003");
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			order3.WD_WLO_PlannedLoad = load3.PK;

			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 10m);

			var load4 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL30000004", "CDS", startTime: DateTimeOffset.Now);
			var load5 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL30000005", "CDS", startTime: DateTimeOffset.Now);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order5.WD_DocketStatus = "ENT";
			order5.WD_TotalWeight = 2m;
			order5.WD_TotalWeightUnit = "G";
			order5.WD_TotalCubic = 1.5m;
			order5.WD_TotalCubicUnit = "WW";
			var pick5 = Helper.CreatePickNew(order5);
			var packageJob5 = order5.PackageJob;
			var package1 = packageJob5.Packages.AddNew("CTN", 1);
			var package2 = packageJob5.Packages.AddNew("BOX", 1);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load4);
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load5);

			var load6 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL30000006", "CDS", startTime: DateTimeOffset.Now);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			order6.WD_DocketStatus = "ENT";
			order6.WD_TotalWeight = 2m;
			order6.WD_TotalWeightUnit = "G";
			order6.WD_TotalCubic = 1.5m;
			order6.WD_TotalCubicUnit = "WW";
			var pick6 = Helper.CreatePickNew(order6);
			var packageJob6 = order6.PackageJob;
			var package3 = packageJob6.Packages.AddNew("CTN", 1);
			var package4 = packageJob6.Packages.AddNew("BOX", 1);
			var pivot3 = Helper.CreateLoadPkgPackagePivot(package3.PK, load6);

			Factory.Save();

			// assert filters
			Asserter.AddToScope(order1, order2, order3, order4, order5, order6);

			var filter = (ModuleTextFilter)FilterStripBizO[OrderFilterBusinessObject.Schema.LoadID];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "WL1";
			Asserter.AssertMatches("Should find two orders of the Load id have such name", filter, order1, order2);

			filter.Property = "WJ";
			Asserter.AssertMatches("Should find nothing as none of the Load id have such name", filter);

			filter.ComparisonOperator = "not starting";
			filter.Property = "WL1";
			Asserter.AssertMatches("Should find an order for the Load id have such name", filter, order3, order5, order6);

			filter.ComparisonOperator = "contains";
			filter.Property = "001";
			Asserter.AssertMatches("Should find an order for the Load id have such name.", filter, order1);

			filter.ComparisonOperator = "not contain";
			filter.Property = "RE";
			Asserter.AssertMatches("Should find all orders for the Load id.", filter, order1, order2, order3, order5, order6);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "WL20000003";
			Asserter.AssertMatches("Should find an order for the Load id have such name.", filter, order3);

			filter.Property = "WL10000004"; 
			Asserter.AssertMatches("Should find nothing as none of the Load id have such name", filter);

			filter.Property = "WL30000004";
			Asserter.AssertMatches("Should find an order for the Load id have such name.", filter, order5);

			filter.Property = "WL30000005";
			Asserter.AssertMatches("Should find an order for the Load id have such name.", filter, order5);

			filter.Property = "WL30000006";
			Asserter.AssertMatches("Should find an order for the Load id have such name.", filter, order6);

			filter.ComparisonOperator = "not equal";
			filter.Property = "WL10000001";
			Asserter.AssertMatches("Should find orders with Load ID not equal WL10000001.", filter, order2, order3, order5, order6);

			filter.ComparisonOperator = "not equal";
			filter.Property = "WL30000004";
			Asserter.AssertMatches("Should find orders with Load ID not equal WL30000004.", filter, order1, order2, order3, order5, order6);

			filter.Property = "";
			filter.ComparisonOperator = "is blank";
			Asserter.AssertMatches("Should find orders with Load ID is blank.", filter, order4);

			filter.Property = "";
			filter.ComparisonOperator = "is not blank";
			Asserter.AssertMatches("Should find orders with Load ID is not blank.", filter, order1, order2, order3, order5, order6);
		}

		#endregion

		#region TestFilterPackingRequired

		public void TestFilterPackingRequired()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			order1.WD_PackingAfterPickingRequired = true;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			order2.WD_PackingAfterPickingRequired = true;

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order3");
			order3.WD_PackingAfterPickingRequired = false;

			Factory.Save();

			Asserter.AddToScope(order1, order2, order3);

			var filter = (ModuleFlagsFilter)FilterStripBizO[OrderFilterBusinessObject.Schema.PackingRequired];
			filter.IsActive = true;

			filter.Property0 = false;
			Asserter.AssertMatches("When filter set to false, then only orders without packing required should be found.", filter, order3);

			filter.Property0 = true;
			Asserter.AssertMatches("When filter set to true, then only orders with packing required should be found.", filter, order1, order2);
		}

		#endregion

		#region TestFilterHeldInventory

		public void TestFilterHeldInventory_FilterExists_HeldGoodsForOrdersEnabled()
		{
			TestFilterHeldInventory_FilterExists_Core(enableHeldGoodsForOrders: true);
		}

		public void TestFilterHeldInventory_FilterExists_HeldGoodsForOrdersDisabled()
		{
			TestFilterHeldInventory_FilterExists_Core(enableHeldGoodsForOrders: false);
		}

		void TestFilterHeldInventory_FilterExists_Core(bool enableHeldGoodsForOrders)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				if (enableHeldGoodsForOrders)
				{
					AssertNotNull("Held Inventory Filter exists.", (ModuleFlagsFilter)DocketFilter[OrderFilterBusinessObject.IsOrderForHeldInventoryFilterName]);
				}
				else
				{
					AssertNull("Held Inventory Filter does not exists.", (ModuleFlagsFilter)DocketFilter[OrderFilterBusinessObject.IsOrderForHeldInventoryFilterName]);
				}
			}
		}

		public void TestFilterHeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			orderLine1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			Factory.Save();

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Asserter.AddToScope(order1, order2);

				var filter = (ModuleFlagsFilter)FilterStripBizO[OrderFilterBusinessObject.IsOrderForHeldInventoryFilterName];
				filter.IsActive = true;

				filter.Property0 = true;
				Asserter.AssertMatches("When filter set to true, then only orders with held inventory should be found.", filter, order1);

				filter.Property0 = false;
				Asserter.AssertMatches("When filter set to false, then only orders with available inventory should be found.", filter, order2);
			}
		}

		#endregion

		#region IsServiceLevelUsed

		protected override bool IsServiceLevelUsed => true;

		#endregion

		#region TestFilterMaxLength

		public override void TestFilterMaxLength()
		{
			var expectedPickGroupMaxLength = 2;   // WhsDocketLineSchema.WE_PickGroup.MaxLength == -1, but cdctableconfig.xml sets MaxLength for smallints to 2.
			CombineAssertions(() =>
			{
				AssertEquals("Order filter should contain MaxLength for Consignee State.", OrgAddressSchema.OA_State.MaxLength, FilterStripBizO["Consignee State"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Consignee Company Name.", OrgHeaderSchema.OH_FullName.MaxLength, FilterStripBizO["Consignee Company Name"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Package ID.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, FilterStripBizO["Package ID"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Pick Group.", expectedPickGroupMaxLength, FilterStripBizO["Pick Group"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Pick No.", ModuleNumberFilter.MultiplyMaxLength(WhsPickSchema.WP_PickNo.MaxLength), FilterStripBizO["Pick No"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Trolley Number.", RefEquipmentSchema.RQ_Registration.MaxLength, FilterStripBizO["Trolley Number"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Transport Job Number / Consignment ID.", Math.Min(DtbBookingSchema.KM_JobID.MaxLength, JobCartageSchema.JJ_ConsignmentID.MaxLength), FilterStripBizO["Transport Job Number"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Receive Reference (Pick Slip).", WhsDocketSchema.WD_ExternalReference.MaxLength, FilterStripBizO["Receive Reference (Pick Slip)"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Receive Reference (Cross Dock).", WhsDocketSchema.WD_ExternalReference.MaxLength, FilterStripBizO["Receive Reference (Cross Dock)"].MaxLength);
				AssertEquals("Order filter should contain MaxLength for Load ID.", WhsLoadSchema.WLO_JobID.MaxLength, FilterStripBizO["Load ID"].MaxLength);
			});
		}

		#endregion

		#region TestFilterCarrierBookingAgent

		public void TestFilterCarrierBookingAgent()
		{
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();

			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");

			order1.CarrierBookingAgentDocAddress.E2_OA_Address = agent1.MainAddress.PK;
			order2.CarrierBookingAgentDocAddress.E2_OA_Address = agent2.MainAddress.PK;
			Factory.Save();

			Asserter.AddToScope(order1, order2);

			var filter = (ModuleGuidFilter)FilterStripBizO["Carrier Booking Agent"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			Asserter.AssertMatches("", filter, order1, order2);

			filter.Property = agent1.PK;
			Asserter.AssertMatches("", filter, order1);

			filter.Property = agent2.PK;
			Asserter.AssertMatches("", filter, order2);
		}

		#endregion

		#region TestFilterTransportZone

		public void TestFilterTransportZone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order2");
			var address1 = Helper.SetUpOrgAddress("20 Maxwell Street", "Beaconsfield", "6162", "PERTH", "WA", "AUBYW", order1.Consignee);
			var address2 = Helper.SetUpOrgAddress("72 O'Riordan Street", "Alexandria", "2015", "SYDNEY", "NSW", "AUALX", order2.Consignee);

			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			var rateTransportZoneSydney = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Sydney");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneSydney, "AU", "2015");
			Factory.Save();

			order1.TransportCoPK = data.Org1.PK;
			order1.ConsigneeAddressPK = address1.PK;
			order2.TransportCoPK = data.Org1.PK;
			order2.ConsigneeAddressPK = address2.PK;

			Asserter.AddToScope(order1, order2);

			var filter = (ModuleGuidFilter)FilterStripBizO["Transport Zone"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Should get all orders if filter is left empty", filter, order1, order2);

			var transportZone = RateTransportZone.GetOperationZone(Factory, data.Org1, address1, "AU", null, address1.Postcode, address1.City);
			filter.Property = transportZone.PK;
			Asserter.AssertMatches("Should get first order if filter is populated with first transport zone", filter, order1);

			transportZone = RateTransportZone.GetOperationZone(Factory, data.Org1, address2, "AU", null, address2.Postcode, address2.City);
			filter.Property = transportZone.PK;
			Asserter.AssertMatches("Should get second order if filter is populated with second transport zone", filter, order2);
		}

		public void TestFilterTransportZone_ComparisonOperators()
		{
			var filter = (ModuleGuidFilter)FilterStripBizO["Transport Zone"];
			AssertEquals("Should show comparison operators.", true, filter.HasComparisonOperator);
			AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "is blank", "is not blank", "filters match" }, filter.ComparisonOperator_List.GetAllCodes());
		}

		#endregion

		#region TestFilterOutboundLocation

		public void TestFilterOutboundLocation_Exact()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = orderLoading.PackageJob.Packages.AddNew();
			var package2 = orderLoading.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			//make an order at packing station
			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var orderAtPackingStation = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(orderAtPackingStation);

			var pickLine = orderAtPackingStation.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);
			AssertEquals("Precondition: Order at packing station should have outbound location as packing station", packingLocation.ToLocationString(), orderAtPackingStation.OutboundLocation);

			//try to filter on the three orders
			Asserter.AddToScope(orderAttachedToPick, orderLoading, orderAtPackingStation);
			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Should get all orders if filter is left empty", filter, orderAttachedToPick, orderLoading, orderAtPackingStation);

			filter.Property = packingLocation.PK;
			Asserter.AssertMatches("Should get order at packing station", filter, orderAtPackingStation);

			filter.Property = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Asserter.AssertMatches("Should get order on the truck", filter, orderLoading);
		}

		public void TestFilterOutboundLocation_IsBlankComparator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);

			//try to filter on the order
			Asserter.AddToScope(orderAttachedToPick, orderLoading);
			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Asserter.AssertMatches("Should only get attachecd to pick order if filtering with isblank", filter, orderAttachedToPick);
		}

		public void TestFilterOutboundLocation_IsNotBlankComparator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);

			//try to filter on the order
			Asserter.AddToScope(orderAttachedToPick, orderLoading);
			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			Asserter.AssertMatches("Should only get attachecd to pick order if filtering with isblank", filter, orderLoading);
		}

		public void TestFilterOutboundLocation_NotEqualComparator()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = orderLoading.PackageJob.Packages.AddNew();
			var package2 = orderLoading.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			//make an order at packing station
			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var orderAtPackingStation = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(orderAtPackingStation);

			var pickLine = orderAtPackingStation.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);
			AssertEquals("Precondition: Order at packing station should have outbound location as packing station", packingLocation.ToLocationString(), orderAtPackingStation.OutboundLocation);

			//try to filter on the three orders
			Asserter.AddToScope(orderAttachedToPick, orderLoading, orderAtPackingStation);
			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = packingLocation.PK;
			Asserter.AssertMatches("Should get order on the truck", filter, orderLoading);

			filter.Property = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Asserter.AssertMatches("Should get order at packing station", filter, orderAtPackingStation);
		}

		public void TestFilterOutboundLocation_FiltersMatchComparator_MatchAllLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = orderLoading.PackageJob.Packages.AddNew();
			var package2 = orderLoading.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			//make an order at packing station
			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var orderAtPackingStation = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(orderAtPackingStation);

			var pickLine = orderAtPackingStation.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);
			AssertEquals("Precondition: Order at packing station should have outbound location as packing station", packingLocation.ToLocationString(), orderAtPackingStation.OutboundLocation);

			//try to filter on the three orders
			Asserter.AddToScope(orderAttachedToPick, orderLoading, orderAtPackingStation);

			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;

			Asserter.AssertMatches("Should get order on the truck and at packing station", filter, orderLoading, orderAtPackingStation);
		}

		public void TestFilterOutboundLocation_FiltersMatchComparator_MatchSpecificLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			//make an order in attached to pick state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var orderAttachedToPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(orderAttachedToPick, data.Part1, 10m);
			Helper.CreatePickNew(orderAttachedToPick);
			Factory.Save();

			//make an order in loading state
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var orderLoading = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLineLoading = Helper.CreateWhsOrderLine(orderLoading, data.Part1, 10m);

			var pick = Helper.CreatePickNew(orderLoading);
			orderLineLoading.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = orderLoading.PackageJob.Packages.AddNew();
			var package2 = orderLoading.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;

			Factory.Save();

			//make an order at packing station
			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var orderAtPackingStation = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(orderAtPackingStation);

			var pickLine = orderAtPackingStation.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition: AttachedToPick order should not have outbound location", string.Empty, orderAttachedToPick.OutboundLocation);
			AssertEquals("Precondition: Order in loading state should have outbound location as dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), orderLoading.OutboundLocation);
			AssertEquals("Precondition: Order at packing station should have outbound location as packing station", packingLocation.ToLocationString(), orderAtPackingStation.OutboundLocation);

			//try to filter on the three orders
			Asserter.AddToScope(orderAttachedToPick, orderLoading, orderAtPackingStation);

			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddGuidFilterStrip("Row", packingLocation.Row.PK);
			
			Asserter.AssertMatches("Should get order at Packing Station", filter, orderAtPackingStation);

			filter.Clear();
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddGuidFilterStrip("Row", data.Whs1.DefaultOutboundDockDoorLocation.Row.PK);

			Asserter.AssertMatches("Should get order on truck", filter, orderLoading);
		}

		public void TestFilterOutboundLocation_Validator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			Factory.Save();

			var outboundLocationFilter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			outboundLocationFilter.IsActive = true;

			outboundLocationFilter.Validation.ValidateProperty();
			AssertNoError(outboundLocationFilter.PropertyInfo, "Outbound Location can not be entered without a warehouse.");

			outboundLocationFilter.Property = location.PK;
			AssertHasError(outboundLocationFilter.PropertyInfo, "Outbound Location can not be entered without a warehouse.");

			var warehouseFilter = (ModuleGuidFilter)FilterStripBizO["Warehouse"];
			warehouseFilter.Property = ZGuid.Empty;
			outboundLocationFilter.Property = location.PK;
			AssertHasError(outboundLocationFilter.PropertyInfo, "Outbound Location can not be entered without a warehouse.");

			warehouseFilter.Property = ZGuid.Invalid;
			outboundLocationFilter.Property = location.PK;
			AssertHasError(outboundLocationFilter.PropertyInfo, "Outbound Location can not be entered without a warehouse.");

			warehouseFilter.Property = data.Whs1.PK;
			warehouseFilter.IsActive = true;
			outboundLocationFilter.Property = location.PK;
			AssertNoError(outboundLocationFilter.PropertyInfo, "Outbound Location can not be entered without a warehouse.");
		}

		public void TestFilterOutboundLocation_ComparisonOperators()
		{
			var filter = (ModuleGuidFilter)FilterStripBizO["Outbound Location"];
			AssertEquals("Should show comparison operators.", true, filter.HasComparisonOperator);
			AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "is blank", "is not blank", "filters match" }, filter.ComparisonOperator_List.GetAllCodes());
		}

		#endregion

		#region TestFilterSalesChannel

		public void TestFilterSalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var salesChannelAlibaba = Helper.CreateWhsSalesChannel("ABB", "ALIBABA");
			var salesChannelDirectWebSale = Helper.CreateWhsSalesChannel("DIR", "DIRECT WEB SALE");
			var salesChannelShopify = Helper.CreateWhsSalesChannel("SFY", "SHOPIFY");

			var orderABB_1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			orderABB_1.WD_WSH_SalesChannel = salesChannelAlibaba.PK;

			var orderABB_2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			orderABB_2.WD_WSH_SalesChannel = salesChannelAlibaba.PK;

			var orderSFY_1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order3");
			orderSFY_1.WD_WSH_SalesChannel = salesChannelShopify.PK;

			Factory.Save();

			Asserter.AddToScope(orderABB_1, orderABB_2, orderSFY_1);

			var filter = (ModuleGuidFilter)FilterStripBizO["Sales Channel"];
			AssertNotNull("Precondition: Filter Sales Channel exists", filter);

			filter.Property = salesChannelAlibaba.PK;
			Asserter.AssertMatches("Expect 2 Alibaba orders", filter, orderABB_1, orderABB_2);

			filter.Property = salesChannelShopify.PK;
			Asserter.AssertMatches("Expect 1 Shopify order", filter, orderSFY_1);

			filter.Property = salesChannelDirectWebSale.PK;
			Asserter.AssertMatches("Expect none DirectWebSale order", filter, Array.Empty<WhsOrder>());

			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Expect all orders on empty filter", filter, orderABB_1, orderABB_2, orderSFY_1);
		}

		#endregion

		#region TestCRMSecurityFilters

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<WhsOrder>.AssertFilterStrip(() => new OrderFilterBusinessObject(), Env.Security.WhsOrderCRMSecurity);
		}

		#endregion

		#region Override Tests

		protected override bool SupportsDocketStatus => false;

		// Status filter replaced for Orders
		protected override bool RunTestFilterDocketStatus => false;
		protected override bool RunTestDocketStatusFilter_Description => false;
		protected override bool SupportsFilterForEnteredStatus => false;

		#endregion

		#region Implementation

		void TestServiceTypeFilters(string filterName, ZDate filterDate, string assertionMessage, Action<ServiceTypeDateFilter> setupFilter, Func<WhsOrder, WhsOrder, IEnumerable<WhsOrder>> getExpectedJobs)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			SetupReceiveStockOnHand_100Part1And100Part2(data);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 5m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);
			Helper.CreateWhsOrderLine(order2, data.Part2, 5m);

			var testDate = new ZDate(2023, 12, 08);

			var fumigationService = order1.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			if (filterName.Equals(ServiceTypeDateFilter.ServiceTypeDateCompleted))
			{
				fumigationService.ES_Completed = filterDate;
			}
			else
			{
				fumigationService.ES_Booked = filterDate;
			}

			Factory.Save();
			Asserter.AddToScope(order1, order2);

			var filter = (ServiceTypeDateFilter)FilterStripBizO[filterName];
			setupFilter(filter);

			Asserter.AssertMatches(assertionMessage, filter, getExpectedJobs(order1, order2).ToArray());
		}

		protected override CodeDescriptionPairList ValidOrderTypes => new OrderType();

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrderFilterBusinessObject();

		protected PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));

		PackingTestHelper packingHelper;

		#endregion
	}

	#region OrderFilterBusinessObject_AccountingFilterStripTest

	public class OrderFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsOrder>
	{
		protected override WhsOrder GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override ModuleIdentifier FilterStripModuleID => ModuleIDs.WhsOrder;

		protected override bool ShouldUseBillingFilters => false;
	}

	#endregion
}
