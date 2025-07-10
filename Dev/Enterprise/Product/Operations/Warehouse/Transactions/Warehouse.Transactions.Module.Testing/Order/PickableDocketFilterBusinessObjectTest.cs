using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class PickableDocketFilterBusinessObjectTest<TFilterBusinessObject> : ExtendedDocketFilterBusinessObjectTest<TFilterBusinessObject, WhsPickableDocket>
			where TFilterBusinessObject : PickableDocketFilterBusinessObject, new()
	{
		#region TestFilterByDocketSubType

		public void TestFilterByDocketSubType()
		{
			var filter = (ModuleTextFilter)DocketFilter["Order Type"];
			filter.IsActive = true;

			// test the filtering
			SetupTestData();
			Docket11.WD_DocketSubType = "ORD";
			Docket12.WD_DocketSubType = "ORD";
			Docket21.WD_DocketSubType = "ASS";
			Docket22.WD_DocketSubType = "";

			filter.Property = "ORD";
			DocketAssert(true, true, false, false);

			filter.Property = "ASS";
			DocketAssert(false, false, true, false);

			// test the Order Types list
			foreach (CodeDescriptionPair validOrderType in ValidOrderTypes)
			{
				AssertCollectionContains(validOrderType, filter.List);
			}
		}

		protected abstract CodeDescriptionPairList ValidOrderTypes { get; }

		#endregion

		#region TestFilterByPickNo

		public void TestFilterByPickNo()
		{
			SetupTestData();
			var pick1 = Factory.NewWithValidTestData<WhsPick>();
			var pick2 = Factory.NewWithValidTestData<WhsPick>();
			pick1.WP_PickNo = "P00000001";
			pick2.WP_PickNo = "P00000002";
			pick1.WP_WW_Whs = Docket12.WD_WW_Whs;
			pick2.WP_WW_Whs = Docket21.WD_WW_Whs;
			Docket12.WD_WP = pick1.PK;
			Docket21.WD_WP = pick2.PK;
			var docketStatus = DocketStatus.Codes.AttachedToPick;
			Docket12.WD_DocketStatus = docketStatus;
			Docket21.WD_DocketStatus = docketStatus;
			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", pick1.WP_PickNo);
			DocketAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"1");
			DocketAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"0001");
			DocketAssert(false, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", pick2.WP_PickNo);
			DocketAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"2");
			DocketAssert(false, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Pick No", (ZString)"0002");
			DocketAssert(false, false, true, false);
		}

		#endregion

		#region TestFilterByConsignee

		public abstract void TestFilterByConsignee();

		#endregion

		#region TestFilterPalletID

		protected override void TestPalletIDCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			// setup 2 receives
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE1", data.Part1, 20m, locationA1, "ID123");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RECEIVE2", data.Part2, 10m, locationA2, "ID456");
			Factory.Save();

			// setup 3 orders, 2 picked, 1 with reserved stock
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order2", data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order3");
			var orderLine = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			AssertNotNull("Precondition - Divot was created.", orderLine.ReserveStockIfAbleTo(receive1.Inventory[0]));
			AssertEquals("Precondition - Order1 is picked.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Order2 is picked.", true, order2.IsAttachedToPickButNotFinalised);

			Factory.Save();

			Asserter.AddToScope(order1);
			Asserter.AddToScope(order2);
			Asserter.AddToScope(order3);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ID123";
			Asserter.AssertMatches("Equals 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, order1); // inventory reserved to orders should not be considered
			filter.Property = "ID456";
			Asserter.AssertMatches("Equals 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, order2);
			filter.Property = "ID";
			Asserter.AssertMatches("Equals 'ID' should return no dockets (each has Pallet ID 'ID123' and 'ID456' respectively).", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ID123";
			Asserter.AssertMatches("Starts With 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, order1); // inventory reserved to orders should not be considered
			filter.Property = "ID456";
			Asserter.AssertMatches("Starts With 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, order2);
			filter.Property = "ID";
			Asserter.AssertMatches("Starts With 'ID' should return both Docket1 and Docket2 (each has Pallet ID 'ID123' and 'ID456' respectively).", filter, order1, order2);
			filter.Property = "D";
			Asserter.AssertMatches("Starts With 'D' should return no dockets (each has Pallet ID 'ID123' and 'ID456' respectively).", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ID123";
			Asserter.AssertMatches("Contains 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, order1); // inventory reserved to orders should not be considered
			filter.Property = "ID456";
			Asserter.AssertMatches("Contains 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, order2);
			filter.Property = "ID";
			Asserter.AssertMatches("Contains 'ID' should return both Docket1 and Docket2 (each has Pallet ID 'ID123' and 'ID456' respectively).", filter, order1, order2);

			// not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "ID123";
			Asserter.AssertMatches("Not contains 'ID123' should return only Docket2 & Docket3 (Docket1 has a PalletID 'ID123').", filter, order2, order3);
			filter.Property = "ID456";
			Asserter.AssertMatches("Not contains 'ID456' should return only Docket1 & Docket3 (Docket2 has a PalletID 'ID456').", filter, order1, order3);
			filter.Property = "D";
			Asserter.AssertMatches("Not contains 'D' should return only Docket3 (Docket21 & Docket2 has PalletID 'ID123' and 'ID456' respectively).", filter, order3);

			var pick3 = Helper.CreatePickNew(order3);
			var availableInventory = pick3.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PickLineQuantity > 0m);
			availableInventory.PickLineQuantity = 0m;
			Factory.Save();

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ID123";
			Asserter.AssertMatches("Equals 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, order1); // should not consider 0 unit Pick Lines.
		}

		public void TestPalletID_PickedUsingInTransitTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m, data.Whs1.DefaultLocation, "PLT123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 35m, data.Whs1.DefaultLocation, "PLT456");
			Factory.Save();

			var orderForReceive1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m);
			var orderForReceive2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 30m);
			var shortOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			var pick = Helper.CreatePickNew(orderForReceive1, orderForReceive2, shortOrder);

			var unpickedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part2, 5m);

			foreach (var pickLine in pick.GetAllPickLines().ToArray())
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_PalletID = transferLine.WE_PalletID.Replace("PLT", "TFR");
			}

			Factory.Save();

			Asserter.AddToScope(orderForReceive1, orderForReceive2, shortOrder, unpickedOrder);

			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "TFR";
			Asserter.AssertMatches("Should not find based on the transfer reference", filter);

			filter.Property = "PLT";
			Asserter.AssertMatches("Should find both orders with stock allocated from receives.", filter, orderForReceive1, orderForReceive2);

			filter.Property = "PLT123";
			Asserter.AssertMatches("Should find order1.", filter, orderForReceive1);
		}

		#endregion

		#region TestFilterByDeliveryRouteAndSequence

		void GetOrderWithDummyAddress(WhsDocket docket, string address, JobDocAddress docAddress, string deliveryRoute, ZShort deliveryRouteSequence)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Address1 = address;
			org.MainAddress.OA_DeliveryRoute = deliveryRoute;
			org.MainAddress.OA_DeliveryRouteSequence = deliveryRouteSequence;

			docAddress.E2_OA_Address = org.MainAddress.PK;
		}

		public void TestFilterByDeliveryRoute()
		{
			if (SupportsDeliveryRouteFilters)
			{
				SetupTestData();
				GetOrderWithDummyAddress(Docket11, "Paris, just under the bridge of Alexander III.", ((WhsPickableDocket)Docket11).ConsigneeDocAddress, "ABC", 1);
				GetOrderWithDummyAddress(Docket12, "Sydney, just under the Harbor bridge.", ((WhsPickableDocket)Docket12).ConsigneeDocAddress, "ABC", 2);
				GetOrderWithDummyAddress(Docket21, "Sydney, just above the Harbor bridge.", ((WhsPickableDocket)Docket21).GoodsBillToDocAddress, "ABC", 3);
				GetOrderWithDummyAddress(Docket22, "Sydney, just above the Harbor bridge.", ((WhsPickableDocket)Docket22).ConsigneeDocAddress, "XXX", 1);
				Factory.Save();

				var clearFactory = new BusinessObjectFactory();

				var filter = (ModuleTextFilter)FilterStripBizO["Delivery Route"];
				filter.IsActive = true;
				filter.Property = "ABC";

				DocketCollection.AdditionalFilter = filter.Query;
				AssertContainsExactElementsInAnyOrder(DocketCollection.Select(o => o.PK), new[] { Docket11.PK, Docket12.PK });
			}
			else
			{
				Assert("FilterBusinessObject does not support Delivery Route Filters.", true);
			}
		}

		public void TestFilterByDeliveryRouteSequence()
		{
			if (SupportsDeliveryRouteFilters)
			{
				SetupTestData();
				GetOrderWithDummyAddress(Docket11, "Paris, just under the bridge of Alexander III.", ((WhsPickableDocket)Docket11).ConsigneeDocAddress, "ABC", 1);
				GetOrderWithDummyAddress(Docket12, "Sydney, just under the Harbor bridge.", ((WhsPickableDocket)Docket12).ConsigneeDocAddress, "ABC", 2);
				GetOrderWithDummyAddress(Docket21, "Sydney, just above the Harbor bridge.", ((WhsPickableDocket)Docket21).GoodsBillToDocAddress, "ABC", 3);
				GetOrderWithDummyAddress(Docket22, "Sydney, just above the Harbor bridge.", ((WhsPickableDocket)Docket22).ConsigneeDocAddress, "ABC", 4);
				Factory.Save();

				var clearFactory = new BusinessObjectFactory();
				var filter = (ModuleNumberRangeFilter)FilterStripBizO["Delivery Route Sequence"];
				filter.IsActive = true;
				filter.Property1 = 1;
				filter.Property2 = 3;

				var dockCollection = new WhsOrderCollection(clearFactory, filter.Query);
				DocketCollection.AdditionalFilter = filter.Query;

				AssertContainsExactElementsInAnyOrder("Assert should not fail under normal conditions", DocketCollection.Select(o => o.PK), new[] { Docket11.PK, Docket12.PK });

				filter.Property1 = 2 * short.MinValue;
				filter.Property2 = 2 * short.MaxValue;

				Assert(filter.HasNotifications());
				AssertEquals(2, filter.Notifications.Count());
				if (filter.Notifications.Count() == 2)
				{
					AssertEquals("Error - Property1: Please enter a value greater than or equal to 0.", filter.Notifications.ElementAt(0).Message);
					AssertEquals("Error - Property2: Please enter a value less than or equal to 32,767.", filter.Notifications.ElementAt(1).Message);
				}
			}
			else
			{
				Assert("FilterBusinessObject does not support Delivery Route Filters.", true);
			}
		}

		protected virtual bool SupportsDeliveryRouteFilters => true;

		#endregion

		#region TestFilterByPriority

		public void TestFilterByPriority()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order1 = Factory.NewWithValidTestData<WhsOrder>();
			var order2 = Factory.NewWithValidTestData<WhsOrder>();
			var order3 = Factory.NewWithValidTestData<WhsOrder>();
			var order4 = Factory.NewWithValidTestData<WhsOrder>();
			order1.WD_PickPriority = 0;
			order2.WD_PickPriority = 1;
			order3.WD_PickPriority = 2;
			order4.WD_PickPriority = 3;

			Factory.Save();

			var filter = (ModuleNumberRangeFilter)FilterStripBizO["Priority"];
			filter.IsActive = true;
			filter.Property1 = 1;
			filter.Property2 = 2;

			var dockCollection = new WhsOrderCollection(Factory, filter.Query);
			dockCollection.AdditionalFilter = filter.Query;
			AssertContainsExactElementsInAnyOrder(dockCollection.Select(o => o.PK), new[] { order2.PK, order3.PK });
		}

		#endregion

		#region Overrides

		protected override ZString ExternalReferenceFieldName => "Order No.";

		protected override ZString GetStatusFilterCaption() => "Order Status";

		protected override MultilingualString GetStatusFilterMultilingualDescription() => (NoResString)"Order Status";

		#endregion

		#region Implementation

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref) => Helper.CreateWhsOrder(org, whs, @ref);

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units) => Helper.CreateWhsOrderLine((WhsOrder)docket, part, units);

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection() => new WhsOrderCollection(Factory);

		#endregion
	}
}
