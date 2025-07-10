using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetWhsPickTest : WhsPickingSecureServiceTestCase
	{
		#region TestGetWhsPick

		public void TestGetWhsPick()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var client = Helper.CreateClient("Cient");
			var part = Helper.CreateProduct(client, "Part1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, row11.Locations[0]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part, 20m);
			var order2 = Helper.CreateWhsOrder(client, warehouse, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part, 30m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse, staff);
			var response1 = webService1.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			var pickLogs1 = Helper.FindLogs(pick1.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs1.Length);
				AssertEquals(pick1.WP_PickNo, pickLogs1[0].ReferenceFreeText);
				AssertEquals(pick1.WP_PickNo, response1.Pick.Reference);
			});

			AssertPickLines(new WhsPickLineInfoCollection(pick1.GetAllPickLines(), new WhsPickInfo()), response1.Pick.Lines);

			var webService2 = GetNewWebService(warehouse, staff);
			var response2 = webService2.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			var pickLogs2 = Helper.FindLogs(pick2.Logs, Events.ServiceCommenced);
			CombineAssertions(() =>
			{
				AssertEquals(1, pickLogs2.Length);
				AssertEquals(pick2.WP_PickNo, pickLogs2[0].ReferenceFreeText);
				AssertEquals(pick2.WP_PickNo, response2.Pick.Reference);
			});

			AssertPickLines(new WhsPickLineInfoCollection(pick2.GetAllPickLines(), new WhsPickInfo()), response2.Pick.Lines);
		}

		public void TestGetWhsPick_LiteralizePalletId()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var staff = Helper.CreateGlbStaff("OP1", "ZZZ1");
			var client = Helper.CreateClient("Cient");
			var part = Helper.CreateProduct(client, "Part1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, row11.Locations[0]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part, 20m);
			var order2 = Helper.CreateWhsOrder(client, warehouse, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part, 30m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = WhsInventoryViewSchema.WI_PalletID.Name;
				AssertCollectionContains("WI_PalletID", ParameterSettingsCache.FieldsToLiteralize);

				var webService1 = GetNewWebService(warehouse, staff);
				var response1 = webService1.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response1, webService1);
				AssertNotNull(response1.Pick);
			}
		}

		public void TestGetWhsPick_LiteralizePalletId_WithPalletId()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var staff = Helper.CreateGlbStaff("OP1", "ZZZ1");
			var client = Helper.CreateClient("Cient");
			var part = Helper.CreateProduct(client, "Part1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, row11.Locations[0], "ABC");
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part, 20m);
			var order2 = Helper.CreateWhsOrder(client, warehouse, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part, 30m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = WhsInventoryViewSchema.WI_PalletID.Name;
				AssertCollectionContains("WI_PalletID", ParameterSettingsCache.FieldsToLiteralize);

				var webService1 = GetNewWebService(warehouse, staff);
				var response1 = webService1.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response1, webService1);
				AssertNotNull(response1.Pick);
			}
		}

		#endregion

		#region TestGetWhsPick_PickByBiggestPackType

		public void TestGetWhsPick_PickByBiggestPackType()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			var client = Helper.CreateClient("Cient");
			var part = Helper.CreateProduct(client, "Part1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, row11.Locations[0]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part, 20m);

			var pick1 = Helper.CreatePickNew(order1);

			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.PickByBiggestType.SetTemporaryValue(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				var webService1 = GetNewWebService(warehouse, staff);
				var response1 = webService1.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response1, webService1);
				AssertNotNull(response1.Pick);

				CombineAssertions(() =>
				{
					AssertEquals(true, response1.Pick.IsPickByBiggestPackTypeEnabled);
					AssertEquals(pick1.WP_PickNo, response1.Pick.Reference);
				});
			}

			using (WarehouseDataRegistry.Instance.PickByBiggestType.SetTemporaryValue(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, false))
			{
				var webService2 = GetNewWebService(warehouse, staff);
				var response2 = webService2.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response2, webService2);
				AssertNotNull(response2.Pick);

				CombineAssertions(() =>
				{
					AssertEquals(false, response2.Pick.IsPickByBiggestPackTypeEnabled);
					AssertEquals(pick1.WP_PickNo, response2.Pick.Reference);
				});
			}
		}

		#endregion

		#region TestGetWhsPick_SetIsPicking

		public void TestGetWhsPick_SetIsPicking()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			var picklines = pick.GetAllPickLines();
			var pickline1 = picklines.Single(pl => pl.WZ_Units == 1m);
			var pickline2 = picklines.Single(pl => pl.WZ_Units == 2m);
			pickline2.WZ_GS_NKAssignedTo = "ST2";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals(pick.PK, response.Pick.PK);
				AssertEquals("Pickline returned to the gun should be marked as IsPicking.", true, pickline1.WZ_IsPicking);
				AssertEquals("Pickline *NOT* returned to the gun should *NOT* be marked as IsPickng.", false, pickline2.WZ_IsPicking);
			});
		}

		#endregion

		#region TestGetWhsPick_AcceptEventOnlyCheckForCustomsOrder

		public void TestGetWhsPick_AcceptEventOnlyCheckForCustomsOrder()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			order.Logs.AddNew(Events.HoldTheWarehouseOrder); // should not effect on result
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals(pick.PK, response.Pick.PK);
			AssertPickLines("Holding order should not check for non customs orders.", pick.GetAllPickLines(), response.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_AcceptEventOnlyCheckForNotVirtualWarehouse

		public void TestGetWhsPick_AcceptEventOnlyCheckForNotVirtualWarehouse()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();
			AssertEquals("Precondition", true, data.Whs1.WW_IsVirtualWarehouse);
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			order.Logs.AddNew(Events.HoldTheWarehouseOrder); // should not effect on result
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals(pick.PK, response.Pick.PK);
			AssertPickLines("Holding order should not check for non customs orders.", pick.GetAllPickLines(), response.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_CustomsOrderAcceptEvent

		public void TestGetWhsPick_CustomsOrderAcceptEvent()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			receiveLine1.CustomsData.WB_EntryKey = "EntryKey1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, location);
			receiveLine2.CustomsData.WB_EntryKey = "EntryKey1";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order1.Lines[0], "EntryKey1");
			AddEventToOrder(order1, Events.HoldTheWarehouseOrder, ZDateTime.Now);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			order2.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order2.Lines[0], "EntryKey1");
			AddEventToOrder(order2, Events.HoldTheWarehouseOrder, ZDateTime.Now);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			AssertEquals("If condition in 'IsFinaliseAllowedByCustomsCore' has been changed, please consider to change the Get next pick query as well.", false, order1.IsFinaliseAllowed);
			AssertEquals("If condition in 'IsFinaliseAllowedByCustomsCore' has been changed, please consider to change the Get next pick query as well.", false, order2.IsFinaliseAllowed);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertPickLines("Orders are on hold.", Array.Empty<WhsPickLine>(), response1.Pick.Lines);

			var acceptlog1 = AddEventToOrder(order1, Events.WarehouseJobCanNowBeFinalised, ZDateTime.Now.AddDays(1));

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response2, webService2);
			AssertPickLines("Order1 has been accepted.", order1.Lines[0].PickLines, response2.Pick.Lines);

			acceptlog1.Cancel();
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response3, webService3);
			AssertPickLines("Cancel event should be ignored.", Array.Empty<WhsPickLine>(), response3.Pick.Lines);

			AddEventToOrder(order1, Events.WarehouseJobCanNowBeFinalised, ZDateTime.Now.AddDays(2));
			AddEventToOrder(order2, Events.WarehouseJobCanNowBeFinalised, ZDateTime.Now.AddDays(2));

			var webService4 = GetNewWebService(data.Whs1, staff);
			var response4 = webService4.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response4, webService4);
			AssertPickLines("Both order are accepted.", pick.GetAllPickLines(), response4.Pick.Lines);

			CombineAssertions(() =>
			{
				AssertEquals("If condition in 'IsFinaliseAllowedByCustomsCore' has been changed, please consider to change the Get next pick query as well.", true, order1.IsFinaliseAllowed);
				AssertEquals("If condition in 'IsFinaliseAllowedByCustomsCore' has been changed, please consider to change the Get next pick query as well.", true, order2.IsFinaliseAllowed);
			});

			AddEventToOrder(order1, Events.HoldTheWarehouseOrder, ZDateTime.Now.AddDays(3));

			var webService5 = GetNewWebService(data.Whs1, staff);
			var response5 = webService5.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response5, webService5);
			AssertPickLines("Order1 is on hold again.", order2.Lines[0].PickLines, response5.Pick.Lines);
		}

		StmALog AddEventToOrder(WhsOrder order, Event events, ZDateTime eventTime)
		{
			var log = order.Logs.AddNew(events);
			Helper.SetLogUTCTimeOnFactorySave(TestConnection, log, eventTime);
			Helper.Factory.Save();
			return log;
		}

		#endregion

		#region TestGetWhsPick_SetIsPicking_UnexpectedExit

		public void TestGetWhsPick_SetIsPicking_UnexpectedExit()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive", data.Part1, 10m);
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			Helper.Factory.Save();
			var pick = Helper.CreatePickNew(order);

			AssertOperatorCanLoadPick("First login should not be able to load pick normally.", staff, data.Whs1, expectedToLoadPick: true);
			AssertEquals("Operator1 is picking", true, pick.GetAllPickLines().All(pl => pl.WZ_IsPicking));

			var otherStaff = Helper.CreateGlbStaff("ST2", "ST2");
			Helper.Factory.Save();

			AssertOperatorCanLoadPick("Other Operators should be able to load pick", otherStaff, data.Whs1, expectedToLoadPick: false);
			AssertOperatorCanLoadPick("Operators should be able to login again an countinue their rework", staff, data.Whs1, expectedToLoadPick: true);

			// if first user does not pick
			foreach (var pickline in pick.GetAllPickLines())
			{
				pickline.WZ_GS_NKAssignedTo = "";
				pickline.WZ_IsPicking = false;
			}
			Helper.Factory.Save();

			AssertOperatorCanLoadPick("Other Operators should be able to load pick", otherStaff, data.Whs1, expectedToLoadPick: true);
		}

		void AssertOperatorCanLoadPick(string messege, GlbStaff staff, WhsWarehouse warehouse, bool expectedToLoadPick)
		{
			var webService = GetNewWebService(warehouse, staff);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertEquals(messege, expectedToLoadPick ? 1 : 0, response.Pick.Lines.Count);
		}

		#endregion

		#region TestGetWhsPick_DifferentClients

		public void TestGetWhsPick_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 10m, locationA2, "", false, true);
			Helper.Factory.Save();

			var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m);

			var orderForOrg2 = Helper.CreateWhsOrder(org2, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(orderForOrg2, data.Part1, 1m);

			var pickForOrg1 = Helper.CreatePickNew(orderForOrg1);
			var pickForOrg2 = Helper.CreatePickNew(orderForOrg2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsPick(pickForOrg1.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);
			AssertEquals(pickForOrg1.WP_PickNo, response1.Pick.Reference);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsPick(pickForOrg2.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0, ClientCode = "" });
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);
			AssertEquals(pickForOrg2.WP_PickNo, response2.Pick.Reference);

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.GetWhsPick(pickForOrg1.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0, ClientCode = data.Org1.OH_Code });
			AssertSuccessfulResponse(response3, webService3);
			AssertNotNull(response3.Pick);
			AssertEquals(pickForOrg1.WP_PickNo, response3.Pick.Reference);

			var webService4 = GetNewWebService(data.Whs1);
			var response4 = webService4.GetWhsPick(pickForOrg1.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0, ClientCode = "O2" });
			AssertEquals("No pick should be found.", false, response4.Pick.Lines.Any());
			AssertEquals($"Un-finalized pick could not be found for reference: {pickForOrg1.WP_PickNo}. Possible mismatch on registered equipment, registered pick group, registered area, registered client, pick has been assigned to another operator or customs order is on hold.", response4.ErrorMessage);
		}

		#endregion

		#region TestGetWhsPick_CustomsJobOnHold

		public void TestGetWhsPick_CustomsJobOnHold()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location);
			inv.InDocketLine.CustomsData.WB_EntryKey = "EntryKey";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var holdlog = AddEventToOrder(order, Events.HoldTheWarehouseOrder, ZDateTime.Now.AddDays(1));
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("No pick should be found.", false, response1.Pick.Lines.Any());
				AssertEquals($"Un-finalized pick could not be found for reference: {pick.WP_PickNo}. Possible mismatch on registered equipment, registered pick group, registered area, registered client, pick has been assigned to another operator or customs order is on hold.", response1.ErrorMessage);
			});

			holdlog.Cancel();
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("A pick should be found.", 1, response2.Pick.Lines.Count);
				AssertEquals(pick.WP_PickNo, response2.Pick.Reference);
			});
		}

		#endregion

		#region TestGetWhsPick_IsMultiOrder

		public void TestGetWhsPick_IsMultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);
			var multiOrderPick = Helper.CreatePickNew(order1, order2);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part2, 10m);
			var singleOrderPick = Helper.CreatePickNew(order3);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsPick(multiOrderPick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			var multiOrderPickFromService = response1.Pick;
			CombineAssertions(() =>
			{
				AssertEquals(multiOrderPick.WP_PickNo, multiOrderPickFromService.Reference);
				AssertEquals(true, multiOrderPickFromService.IsMultiOrder);
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsPick(singleOrderPick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "any", PickMethod = "any", PickGroup = 0 }); // testing when any is lower case 
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			var singleOrderPickFromService = response2.Pick;
			CombineAssertions(() =>
			{
				AssertEquals(singleOrderPick.WP_PickNo, singleOrderPickFromService.Reference);
				AssertEquals(false, singleOrderPickFromService.IsMultiOrder);
			});
		}

		#endregion

		#region TestGetWhsPick_OrderLinesAreNotLoaded

		public void TestGetWhsPick_OrderLinesAreNotLoaded()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);

			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;
			row11.Locations[1].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var staff = Helper.CreateGlbStaff("S2", "S2");
			var client = Helper.CreateClient("Cient");

			var part1 = Helper.CreateProduct(client, "Part1");
			var part2 = Helper.CreateProduct(client, "Part2");

			var receive1 = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, part1, 100m, row11.Locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive1, part2, 100m, row11.Locations[1]);
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part1, 20m);
			Helper.CreateWhsOrderLine(order1, part2, 30m);

			var pick = Helper.CreatePickNew(order1);

			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { }); // To check all empty properties
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Precondition: ", true, pick.Orders[0].Lines.Any());

			CombineAssertions(() =>
			{
				AssertEquals(pick.Orders.Count, response.Pick.Orders.Count);
				foreach (var order in response.Pick.Orders)
				{
					AssertEquals("Order lines should not be loaded into pick for performance reasons", false, order.Lines.Any());
				}
			});
		}

		#endregion

		#region TestGetWhsPick_WithPickGroup

		public void TestGetWhsPick_WithPickGroup()
		{
			var operator1 = Helper.CreateGlbStaff("OP1", "Test1");
			var pickGroupCollection = new PickGroupCollection();
			var pickGroup = pickGroupCollection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pickGroupCollection))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals(true, receive.IsFinalised);
				Helper.Factory.Save();

				var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);

				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);
				var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part2, 10m, pickGroup: 1);

				var pick1 = Helper.CreatePickNew(order1);
				var pick2 = Helper.CreatePickNew(order2);

				Helper.Factory.Save();

				var webService1 = GetNewWebService(data.Whs1, operator1);
				var response1 = webService1.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response1, webService1);
				AssertNotNull(response1.Pick);

				CombineAssertions(() =>
				{
					AssertEquals(pick1.WP_PickNo, response1.Pick.Reference);
					AssertEquals(1, response1.Pick.Lines.Count);
					AssertPickLine(response1.Pick.Lines[0], orderLine1.PickLines[0].PK, data.Part1.PK, 4m);
				});

				var webService2 = GetNewWebService(data.Whs1, operator1);
				var response2 = webService2.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response2, webService2);
				AssertNotNull(response2.Pick);

				CombineAssertions(() =>
				{
					AssertEquals(pick2.WP_PickNo, response2.Pick.Reference);
					AssertEquals(2, response2.Pick.Lines.Count);
					AssertPickLine(response2.Pick.Lines[0], orderLine3.PickLines[0].PK, data.Part2.PK, 10m);
					AssertPickLine(response2.Pick.Lines[1], orderLine2.PickLines[0].PK, data.Part1.PK, 6m);
				});

				// clean-up
				orderLine1.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine2.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine3.PickLines[0].WZ_GS_NKAssignedTo = "";
				orderLine1.PickLines[0].WZ_IsPicking = false;
				orderLine2.PickLines[0].WZ_IsPicking = false;
				orderLine3.PickLines[0].WZ_IsPicking = false;

				var webService3 = GetNewWebService(data.Whs1, operator1);
				var response3 = webService3.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 1 });
				AssertSuccessfulResponse(response3, webService3);
				AssertNotNull(response3.Pick);

				CombineAssertions(() =>
				{
					AssertEquals("No pick should be found.", false, response3.Pick.Lines.Any());
					AssertEquals("Un-finalized pick could not be found for reference: P00000001. Possible mismatch on registered equipment, registered pick group, registered area, registered client, pick has been assigned to another operator or customs order is on hold.", response3.ErrorMessage);
				});

				var webService4 = GetNewWebService(data.Whs1, operator1);
				var response4 = webService4.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 1 });
				AssertSuccessfulResponse(response4, webService4);
				AssertNotNull(response4.Pick);

				CombineAssertions(() =>
				{
					AssertEquals(pick2.WP_PickNo, response4.Pick.Reference);
					AssertEquals(1, response4.Pick.Lines.Count);
					AssertPickLine(response4.Pick.Lines[0], orderLine3.PickLines[0].PK, data.Part2.PK, 10m);
				});
			}
		}

		void AssertPickLine(WhsPickLineInfo pickLine, ZGuid expectedPK, ZGuid expectedProductPK, decimal expectedQuantity)
		{
			AssertEquals(expectedPK.ToGuid(), pickLine.PKs[0]);
			AssertEquals(expectedProductPK, pickLine.ProductPK);
			AssertEquals(expectedQuantity, pickLine.Units);
		}

		#endregion

		#region TestGetWhsPick_WithoutWarehouseCode

		public void TestGetWhsPick_WithoutWarehouseCode()
		{
			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Please provide login credentials to use this service.", webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 }));
		}

		#endregion

		#region TestGetWhsPick_PickNotFound

		public void TestGetWhsPick_PickNotFound()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var client = Helper.CreateClient("Cient");
			var part = Helper.CreateProduct(client, "Part1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 100m, row11.Locations[0]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order, part, 20m);

			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 5;

			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse);
			var response1 = webService1.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertEquals(true, response1.NoError());
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(null, response1.ErrorMessage);

			var webService2 = GetNewWebService(warehouse);
			AssertBusinessValidationError(webService2, "Un-finalized pick could not be found for reference: TEST PICK. Possible mismatch on registered equipment, registered pick group, registered area, registered client, pick has been assigned to another operator or customs order is on hold.",
				webService2.GetWhsPick("TEST PICK", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 }));
		}

		#endregion

		#region TestGetWhsPick_ByOrderRef

		public void TestGetWhsPick_ByOrderRef()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;
			row11.Locations[1].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var operator1 = Helper.CreateGlbStaff("OP1", "Test1");
			var operator2 = Helper.CreateGlbStaff("OP2", "Test2");
			var client = Helper.CreateClient("Cient");

			var part1 = Helper.CreateProduct(client, "Part1");
			var part2 = Helper.CreateProduct(client, "Part2");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, 100m, row11.Locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, part2, 100m, row11.Locations[1]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part1, 20m);
			Helper.CreateWhsOrderLine(order1, part2, 30m);
			order1.References.AddNew();
			order1.References[0].WX_RefType = order1.References[0].Lookups.ReferenceTypes[0].Code;
			order1.References[0].WX_Reference = "Order Ref1";

			var order2 = Helper.CreateWhsOrder(client, warehouse, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part1, 30m);
			Helper.CreateWhsOrderLine(order2, part2, 30m);
			order2.References.AddNew();
			order2.References[0].WX_RefType = order2.References[0].Lookups.ReferenceTypes[0].Code;
			order2.References[0].WX_Reference = "Order Ref2";

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse, operator1);
			var response1 = webService1.GetWhsPick("Order1", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			AssertEquals(pick1.WP_PickNo, response1.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick1.GetAllPickLines(), response1.Pick), response1.Pick.Lines);

			var webService2 = GetNewWebService(warehouse, operator1);
			var response2 = webService2.GetWhsPick("Order Ref1", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			AssertEquals(pick1.WP_PickNo, response2.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick1.GetAllPickLines(), response2.Pick), response2.Pick.Lines);

			var webService3 = GetNewWebService(warehouse, operator2);
			var response3 = webService3.GetWhsPick("Order2", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response3, webService3);
			AssertNotNull(response3.Pick);

			AssertEquals(pick2.WP_PickNo, response3.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick2.GetAllPickLines(), response3.Pick), response3.Pick.Lines);

			var webService4 = GetNewWebService(warehouse, operator2);
			var response4 = webService4.GetWhsPick("Order Ref2", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response4, webService4);
			AssertNotNull(response4.Pick);

			AssertEquals(pick2.WP_PickNo, response4.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick2.GetAllPickLines(), response4.Pick), response4.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_OldestUnfinalized

		public void TestGetWhsPick_OldestUnfinalized()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			warehouse.WW_WarehouseCode = "WH1";
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3, 1);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;
			row11.Locations[1].WLV_WA_PickingArea = warehouse.Areas[1].PK;
			row11.Locations[2].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			var operator1 = Helper.CreateGlbStaff("OP1", "Test1");
			var operator2 = Helper.CreateGlbStaff("OP2", "Test2");
			var client = Helper.CreateClient("Cient");

			var part1 = Helper.CreateProduct(client, "Part1");
			var part2 = Helper.CreateProduct(client, "Part2");

			var receive = Helper.CreateWhsReceive(client, warehouse, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part1, 100m, row11.Locations[0]);
			Helper.CreateWhsReceiveInventoryLine(receive, part2, 100m, row11.Locations[1]);
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(client, warehouse, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, part1, 20m);
			Helper.CreateWhsOrderLine(order1, part2, 30m);

			var order2 = Helper.CreateWhsOrder(client, warehouse, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, part1, 30m);
			Helper.CreateWhsOrderLine(order2, part2, 30m);

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(warehouse, operator1);
			var response1 = webService1.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			AssertEquals(pick1.WP_PickNo, response1.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick1.GetAllPickLines(), response1.Pick), response1.Pick.Lines);

			var webService2 = GetNewWebService(warehouse, operator2);
			var response2 = webService2.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			AssertEquals(pick2.WP_PickNo, response2.Pick.Reference);
			AssertPickLines(new WhsPickLineInfoCollection(pick2.GetAllPickLines(), response2.Pick), response2.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets

		public void TestGetWhsPick_GetCompletePallets()
		{
			var client1 = Helper.CreateClient("1");
			var client2 = Helper.CreateClient("2");
			var client3 = Helper.CreateClient("3");

			var whs = Helper.CreateWarehouse("WH1", "A", 1, 10);

			// Part for client 1 and 2
			var part1 = Helper.CreateProduct(client1, "1");
			Helper.CreateProductClientRelationShip(client2, part1);

			// Part for just client2
			var part2 = Helper.CreateProduct(client2, "2");

			// Part for just client3
			var part3 = Helper.CreateProduct(client3, "3");

			// Receive for client1, part1, pallet A11 - FullPick
			var receive1_1 = Helper.CreateWhsReceive(client1, whs, "R1", Notify);
			var inventory1_1 = Helper.CreateWhsReceiveInventoryLine(receive1_1, part1, 10m);
			inventory1_1.WI_PalletID = "A11";
			receive1_1.AllocateLocationsWithMock();
			receive1_1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1_1);

			// Receive for client2, part1, pallet A12 - FullPick
			var receive2_1 = Helper.CreateWhsReceive(client2, whs, "R2", Notify);
			var inventory2_1 = Helper.CreateWhsReceiveInventoryLine(receive2_1, part1, 10m);
			inventory2_1.WI_PalletID = "A12";
			receive2_1.AllocateLocationsWithMock();
			receive2_1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2_1);

			// Receive for client2, part1 and part2, pallet A13 - Not FullPick
			var receive2_2 = Helper.CreateWhsReceive(client2, whs, "R3", Notify);
			var inventory2_2 = Helper.CreateWhsReceiveInventoryLine(receive2_2, part1, 10m);
			inventory2_2.WI_PalletID = "A13";
			var inventory2_3 = Helper.CreateWhsReceiveInventoryLine(receive2_2, part2, 10m);
			inventory2_3.WI_PalletID = "A13";
			receive2_2.AllocateLocationsWithMock();
			receive2_2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2_2);

			// Receive x 2 for client1, part1, pallet A15 - FullPick
			var receive1_2 = Helper.CreateWhsReceive(client1, whs, "R6", Notify);
			var inventory1_2 = Helper.CreateWhsReceiveInventoryLine(receive1_2, part1, 10m);
			inventory1_2.WI_PalletID = "A15";
			receive1_2.AllocateLocationsWithMock();
			receive1_2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1_2);

			var receive1_3 = Helper.CreateWhsReceive(client1, whs, "R7", Notify);
			var inventory1_3 = Helper.CreateWhsReceiveInventoryLine(receive1_3, part1, 10m);
			inventory1_3.WI_PalletID = "A15";
			inventory1_3.WI_WL = inventory1_2.WI_WL;
			receive1_3.AllocateLocationsWithMock();
			receive1_3.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1_3);
			Helper.Factory.Save();

			// Order for client 1 part 1 - FullPick - A11
			var order1_1 = Helper.CreateWhsOrder(client1, whs, "O1");
			var orderLine1_1 = Helper.CreateWhsOrderLine(order1_1, part1, 10m);

			// Order for client 2 part 1 - FullPick - A12
			var order2_1 = Helper.CreateWhsOrder(client2, whs, "O2");
			var orderLine2_1 = Helper.CreateWhsOrderLine(order2_1, part1, 10m);

			// Order for client 2 part 2 - NotFullPick - A13
			var order2_2 = Helper.CreateWhsOrder(client2, whs, "O3");
			var orderLine2_2 = Helper.CreateWhsOrderLine(order2_2, part1, 10m);

			// Order for client 1 part 1 - FullPick - A15
			var order1_2 = Helper.CreateWhsOrder(client1, whs, "O6");
			var orderLine1_2 = Helper.CreateWhsOrderLine(order1_2, part1, 20m);
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(new WhsPickableDocket[] { order1_1, order1_2, order2_1, order2_2 });

			Helper.Factory.Save();

			var webService = GetNewWebService(whs);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals(3, response.Pick.CompletePalletPickingPallets.Count);
				AssertCollectionContains("A11", response.Pick.CompletePalletPickingPallets);
				AssertCollectionContains("A12", response.Pick.CompletePalletPickingPallets);
				AssertCollectionContains("A15", response.Pick.CompletePalletPickingPallets);
				AssertCollectionNotContains("A13", response.Pick.CompletePalletPickingPallets);
			});
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_SamePalletForDifferentOrders

		public void TestGetWhsPick_GetCompletePallets_SamePalletForDifferentOrders()
		{
			var client1 = Helper.CreateClient("CLIENT1");

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var location = whs.FindLocation("A-1");

			var part = Helper.CreateProduct(client1, "P1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R1", part, 10m, location, "PLT-1", finalise: true);
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(client1, whs, "R2", part, 10m, location, "PLT-1", finalise: true);
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client1, whs, "O1", part, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client1, whs, "O2", part, 10m);

			var pick = Helper.CreatePickNew(new WhsPickableDocket[] { order1, order2 });

			Helper.Factory.Save();

			var webService = GetNewWebService(whs);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Two orders for same client with the same pallet id is valid for Full Pallet Picking.", 1, response.Pick.CompletePalletPickingPallets.Count);
			AssertEquals("Should return correct Pallet ID", "PLT-1", response.Pick.CompletePalletPickingPallets.Single());
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_SamePalletIDInDifferentWarehouses

		public void TestGetWhsPick_GetCompletePallets_SamePalletIDInDifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("W2", "A", 2, 1);
			var locationInWhs1 = data.Whs1.FindLocation("A-1");
			var locationInWhs2 = whs2.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationInWhs1, "PLT-1");
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 10m, locationInWhs2, "PLT-1");
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "O2", data.Part1, 10m);

			var pick = Helper.CreatePickNew(order1);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Two warehouses have same Pallet ID is valid for Full Pallet Picking", 1, response.Pick.CompletePalletPickingPallets.Count);
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_MultipleClientsUseSamePalletID

		public void TestGetWhsPick_GetCompletePallets_MultipleClientsUseSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "PLT-1");
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part1, 10m, location, "PLT-1");
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Should not return pallet as another client also used it.", 0, response.Pick.CompletePalletPickingPallets.Count);
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_MultipleProductsOnSamePalletID

		public void TestGetWhsPick_GetCompletePallets_MultipleProductsOnSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location, "PLT-1");
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m, location, "PLT-1");
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Should not return pallet as another product also on it.", 0, response.Pick.CompletePalletPickingPallets.Count);
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_InventoryWithZeroQuantities

		public void TestGetWhsPick_GetCompletePallets_InventoryWithZeroQuantities()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			inventory1.WI_PalletID = "PLT-1";
			inventory2.WI_PalletID = "PLT-1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);

			var pick1 = Helper.CreatePickNew(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10m);

			Helper.Factory.Save();

			var pick2 = Helper.CreatePickNew(order2);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Only 1 Pallet should be available for Complete Pallet Picking", 1, response.Pick.CompletePalletPickingPallets.Count);
				AssertEquals("PLT-1", response.Pick.CompletePalletPickingPallets[0]);
			});
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_PartiallyPicked

		public void TestGetWhsPick_GetCompletePallets_PartiallyPicked()
		{
			TestGetWhsPick_GetCompletePallets_PartiallyPicked_Core(usingInTransitTransfer: false);
		}

		public void TestGetWhsPick_GetCompletePallets_PartiallyPicked_UsingInTransitTransfer()
		{
			TestGetWhsPick_GetCompletePallets_PartiallyPicked_Core(usingInTransitTransfer: true);
		}

		void TestGetWhsPick_GetCompletePallets_PartiallyPicked_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1");
			var location = data.Whs1.DefaultLocation;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 10m, location, "PLT-1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part, 10m, location, "PLT-1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", part, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, part, 10m);

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var pickLine = orderLine1.PickLines.Single();
			if (usingInTransitTransfer)
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_PalletID = "PLT-1";
				Helper.Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Helper.Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine1.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Should not return pallet.", false, response.Pick.CompletePalletPickingPallets.Any());
		}

		#endregion

		#region TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock

		public void TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock()
		{
			TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock_Core(usingInTransitTransfer: false);
		}

		public void TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock_UsingInTransitTransfer()
		{
			TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock_Core(usingInTransitTransfer: true);
		}

		void TestGetWhsPick_GetCompletePallets_OtherPickWithInTransitStock_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var part = Helper.CreateProduct(data.Org1, "PR1");
			var location = data.Whs1.DefaultLocation;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part, 10m, location, "PLT-1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part, 10m, location, "PLT-1");
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", part, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", part, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			Helper.PickAndMakeInTransitTransfer(order1.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			}
			else
			{
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, order2.Lines[0].PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			}

			Helper.Factory.Save();

			// Should not return the pallet as there is other stock for the same pallet in the dock door (Actually, should we maybe support this now?)
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertEquals("Should not find any complete pallets.", false, response.Pick.CompletePalletPickingPallets.Any());
		}

		#endregion

		#region TestGetWhsPick_RequiresPutaway

		public void TestGetWhsPick_RequiresPutaway()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals(pick.PK, response.Pick.PK);
				AssertEquals("Should be a putaway only pick.", false, response.Pick.Lines.Any());
				AssertEquals("Should be a putaway only pick.", true, response.Pick.IsPutawayOnly);
			});
		}

		#endregion

		#region TestGetWhsPick_WithConcurrencyError

		public void TestGetWhsPick_WithConcurrencyError()
		{
			var webService = GetNewWebService();

			// For this test, we need the helper to use the same factory as the webservice
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var staff1 = helper.CreateGlbStaff("ST1", "Staff1");
			var staff2 = helper.CreateGlbStaff("ST2", "Staff2");
			var data = new TestDataSimpleEnvironment(helper.Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			helper.CreatePickNew(order1);

			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = helper.CreatePickNew(order2);

			helper.Factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order1.PK);
				orderInOtherFactory.Lines[0].PickLines[0].WZ_GS_NKAssignedTo = staff2.GS_Code;
				otherFactory.Save();
			};

			SetupSecurityHeader(webService, data.Whs1, staff1);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Second Pick should be returned as it should automatically retry on concurrency errors.", pick2.WP_PickNo, response.Pick.Reference);
				AssertEquals("Second Pick should be returned as it should automatically retry on concurrency errors.", 1, response.Pick.Lines.Count);
				AssertNull("Should have no error message.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetWhsPick_WithSaveError

		public void TestGetWhsPick_WithSaveError()
		{
			var webService = GetNewWebService();

			// For this test, we need the helper to use the same factory as the webservice
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var staff1 = helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(helper.Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			helper.Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			helper.CreatePickNew(order);

			helper.Factory.Saving += delegate
			{
				new DbColumnDependencyRemover(WhsPickLineSchema.Constants.SqlSchemaName, WhsPickLineSchema.Constants.TableName, WhsPickLineSchema.Constants.WZ_GS_NKAssignedTo).DropRelateObjects(TestConnection);
				TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.WhsPickLine
DROP
	COLUMN WZ_GS_NKAssignedTo");
			};

			SetupSecurityHeader(webService, data.Whs1, staff1);
			var response = webService.GetWhsPick("", new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			CombineAssertions(() =>
			{
				AssertEquals("No Pick should be return when there is a factory save error.", false, response.Pick.Lines.Any());
				AssertEndsWith("Error Message should be set.", "Inner Message = Invalid column name 'WZ_GS_NKAssignedTo'.\r\n\r\n", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetWhsPick_ExcludesPickLinesCommittingDockDoorStock

		public void TestGetWhsPick_ExcludesPickLinesCommittingDockDoorStock()
		{
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;

			var splitPickLine = pickLine.Split(4m);
			Helper.PickAndMakeInTransitTransfer(splitPickLine, ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Should find the pick.", pick.WP_PickNo, response.Pick.Reference);
				AssertEquals("Should only find a single line.", 1, response.Pick.Lines.Count);
			});

			AssertPickLines("", pick.GetAllPickLines().Where(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty), response.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_WithPackingDate

		public void TestGetWhsPick_WithPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			var packingDate = ZDate.Today.AddYears(-1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, ZDate.Empty, packingDate, "", "", "", "");
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, ZDate.Empty, packingDate, "", "", "", "", "");

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, picker);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_WithExpiryDate

		public void TestGetWhsPick_WithExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var picker = Helper.CreateGlbStaff("ABC", "Test Picker");

			Helper.SetClientAllAttributeType(data.Org1, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			var expiryDate = ZDate.Today.AddYears(-1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 20m, expiryDate, ZDate.Empty, "", "", "", "", "");

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, picker);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);
		}

		#endregion

		#region TestGetWhsPick_SaveException

		public void TestGetWhsPick_ZSaveException()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			var picklines = pick.GetAllPickLines();
			var pickline1 = picklines.Single(pl => pl.WZ_Units == 1m);
			var pickline2 = picklines.Single(pl => pl.WZ_Units == 2m);
			Helper.Factory.Save();

			var saveCount = 1;
			var webService = GetNewWebService(data.Whs1, staff1);
			var row = ((INeedRow)receive).Row;
			var factory = webService.Factory;
			factory.Saving += f =>
			{
				if (saveCount > 1) // Fail in second save
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Lock request time out period exceeded.", "", 1)
					));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), factory);
					throw exception;
				}
				saveCount++;
			};

			var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertEquals("The save operation timed out\r\nLock request time out period exceeded. Please try again.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestGetWhsPick_ZCannotSaveException

		public void TestGetWhsPick_ZCannotSaveException()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			var picklines = pick.GetAllPickLines();
			var pickline1 = picklines.Single(pl => pl.WZ_Units == 1m);
			var pickline2 = picklines.Single(pl => pl.WZ_Units == 2m);
			Helper.Factory.Save();

			var saveCount = 0;
			var webService = GetNewWebService(data.Whs1, staff1);
			var row = ((INeedRow)receive).Row;
			var factory = webService.Factory;
			factory.Saving += f =>
			{
				if (saveCount > 0) // Fail in second save
				{
					throw new ZCannotSaveException("Test", "Test");
				}
				saveCount++;
			};

			var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Test", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestGetWhsPick_TaskManagement

		#region TestGetWhsPick_BlankReference

		public void TestGetWhsPick_BlankReference()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult =  new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.PickJob);

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>())).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals(task.PK, response.TaskPK);

				AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);
			}
		}

		public void TestGetWhsPick_BlankReference_HasUnpickedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			order2.Lines.SelectMany(l => l.PickLines).ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = staff.GS_Code;
				Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
			});

			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.PickJob);

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>())).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals(task.PK, response.TaskPK);

				AssertPickLines(new WhsPickLineInfoCollection(order1.Lines.SelectMany(l => l.PickLines), new WhsPickInfo()), response.Pick.Lines);
			}
		}

		public void TestGetWhsPick_BlankReference_PutawayOnly()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			pick.GetAllPickLines().ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = staff.GS_Code;
				Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
			});

			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.PickJob);

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>())).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals(task.PK, response.TaskPK);

				AssertEquals(false, response.Pick.Lines.Any());
				AssertEquals(true, response.Pick.IsPutawayOnly);
			}
		}

		public void TestGetWhsPick_BlankReference_IgnoresCompletedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			order2.Lines.SelectMany(l => l.PickLines).ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = staff.GS_Code;
				var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
				transferLine.FinaliseDocketLine();
			});

			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.PickJob);

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>())).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals(task.PK, response.TaskPK);

				AssertPickLines(new WhsPickLineInfoCollection(order1.Lines.SelectMany(l => l.PickLines), new WhsPickInfo()), response.Pick.Lines);
			}
		}

		public void TestGetWhsPick_BlankReference_TaskNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult = new GetNextTaskResult("A task could not be found.");

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>())).Returns(getNextTaskResult);

			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertBusinessValidationError(webService, "A task could not be found.", response);
			}
		}

		public void TestGetWhsPick_BlankReference_IsInvokedInTemporaryUserContext()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			Helper.Factory.Save();

			var serviceMock = new Mock<IWhsTaskManagementService>();
			var getNextTaskResult = new GetNextTaskResult(task.PK.ToGuid(), WarehouseTaskFormFlowTypes.PickJob);

			var glowUserDataManagerMock = new Mock<IGlowUserDataManager>();
			var disposableMock = new Mock<IDisposable>();
			glowUserDataManagerMock.Setup(g => g.IncreaseTempUserCount()).Returns(disposableMock.Object);

			var envBranch = GlbBranch.CurrentBranch.PK.ToGuid();
			var envDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();

			serviceMock.Setup(m => m.GetNextTask(
				It.IsAny<BusinessObjectFactory>(),
				string.Empty,
				staff.PK.ToGuid(),
				data.Whs1.PK.ToGuid(),
				WarehouseTaskFormFlowTypes.PickJob,
				string.Empty,
				Array.Empty<Guid>()))
					.Callback(() =>
					{
						glowUserDataManagerMock.Verify(g => g.IncreaseTempUserCount());

						AssertEquals(staff.PK, GlbStaff.CurrentUser.PK);
						AssertEquals(envBranch, GlbBranch.CurrentBranch.PK);
						AssertEquals(envDepartment, GlbDepartment.CurrentDepartment.PK);
					})
					.Returns(getNextTaskResult);

			using (ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object))
			using (ObjectFactory.Substitute(serviceMock.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.GetWhsPick(string.Empty, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals(task.PK, response.TaskPK);

				AssertPickLines(new WhsPickLineInfoCollection(pick.GetAllPickLines(), new WhsPickInfo()), response.Pick.Lines);

				disposableMock.Verify(d => d.Dispose());
			}
		}

		#endregion

		#region TestGetWhsPick_ByReference

		public void TestGetWhsPick_ByReference_CreatesTask()
			=> TestGetWhsPick_ByReference_CreatesTaskCore(false);

		public void TestGetWhsPick_ByReference_CreatesTask_PutawayOnly()
			=> TestGetWhsPick_ByReference_CreatesTaskCore(true);

		void TestGetWhsPick_ByReference_CreatesTaskCore(bool isPutawayOnlyPick)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var expectedPickLines = pick.GetAllPickLines();

			if (isPutawayOnlyPick)
			{
				var transferLinePickLines = new List<WhsPickLine>();
				expectedPickLines.ForEach(pl =>
				{
					pl.WZ_GS_NKAssignedTo = staff.GS_Code;
					var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					transferLinePickLines.Add(transferLine.PickLines[0]);
				});
				expectedPickLines = transferLinePickLines;
			}

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertNotEquals(Guid.Empty, response.TaskPK);

			var taskPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, response.TaskPK));
			AssertContainsExactElementsInAnyOrder(expectedPickLines.Select(p => p.PK), taskPickLines.Select(p => p.PK));
		}

		public void TestGetWhsPick_ByReference_CreatesTask_MixOfUnpickedAndPickedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var pickLine = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLines = pick.GetAllPickLines();

			var pickedLine = pickLines.First();
			pickedLine.WZ_GS_NKAssignedTo = staff.GS_Code;
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickedLine, ZDateTimeOffset.Now);
			var expectedLines = pickLines.Skip(1).Union([transferLine.PickLines[0]]);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertNotEquals(Guid.Empty, response.TaskPK);

			var taskPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, response.TaskPK));
			AssertContainsExactElementsInAnyOrder(expectedLines.Select(p => p.PK), taskPickLines.Select(p => p.PK));
		}

		public void TestGetWhsPick_ByReference_LoadsExistingTask()
			=> TestGetWhsPick_ByReference_LoadsExistingTaskCore(false);

		public void TestGetWhsPick_ByReference_LoadsExistingTask_PutawayOnly()
			=> TestGetWhsPick_ByReference_LoadsExistingTaskCore(true);

		void TestGetWhsPick_ByReference_LoadsExistingTaskCore(bool isPutawayOnlyPick)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLines = pick.GetAllPickLines();
			var expectedPickLines = pick.GetAllPickLines();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			if (isPutawayOnlyPick)
			{
				var transferLinePickLines = new List<WhsPickLine>();
				pickLines.ForEach(pl =>
				{
					pl.WZ_GS_NKAssignedTo = staff.GS_Code;
					var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					transferLinePickLines.Add(transferLine.PickLines[0]);
				});
				expectedPickLines = transferLinePickLines;
			}

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertEquals(task.PK.ToGuid(), response.TaskPK);

			var taskPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, response.TaskPK));
			AssertContainsExactElementsInAnyOrder(expectedPickLines.Select(p => p.PK), taskPickLines.Select(p => p.PK));
		}

		public void TestGetWhsPick_ByReference_DeletesUnstartedOrphanTasks()
			=> TestGetWhsPick_ByReference_DeletesUnstartedOrphanTasksCore(false);

		public void TestGetWhsPick_ByReference_DeletesUnstartedOrphanTasks_PutawayOnly()
			=> TestGetWhsPick_ByReference_DeletesUnstartedOrphanTasksCore(true);

		void TestGetWhsPick_ByReference_DeletesUnstartedOrphanTasksCore(bool isPutawayOnlyPick)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var expectedPickLines = pick.GetAllPickLines();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);

			foreach (var pickLine in order2.Lines.SelectMany(l => l.PickLines))
			{
				pickLine.WZ_P9_Task = Guid.Empty;
			}

			if (isPutawayOnlyPick)
			{
				var transferLinePickLines = new List<WhsPickLine>();
				expectedPickLines.ForEach(pl =>
				{
					pl.WZ_GS_NKAssignedTo = staff.GS_Code;
					var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					transferLinePickLines.Add(transferLine.PickLines[0]);
				});
				expectedPickLines = transferLinePickLines;
			}

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertTaskIsSplitAppropriately(
				webService.Factory,
				response,
				expectedPickLinesOnNewTask: expectedPickLines,
				task,
				expectedOriginalTaskStatus: null,
				expectedPickLinesOnOriginalTask: null);
		}

		public void TestGetWhsPick_ByReference_ClosesTasksWithPickedLines()
			=> TestGetWhsPick_ByReference_ClosesTasksWithPickedLinesCore(false);

		public void TestGetWhsPick_ByReference_ClosesTasksWithPickedLines_PutawayOnly()
			=> TestGetWhsPick_ByReference_ClosesTasksWithPickedLinesCore(true);

		void TestGetWhsPick_ByReference_ClosesTasksWithPickedLinesCore(bool isPutawayOnlyPick)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 30m);

			var order1PickLines = order1.Lines.SelectMany(l => l.PickLines);
			var order2PickLines = order2.Lines.SelectMany(l => l.PickLines);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLines = pick.GetAllPickLines();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			if (isPutawayOnlyPick)
			{
				var transferLinePickLines = new List<WhsPickLine>();
				order1PickLines.ForEach(pl =>
				{
					pl.WZ_GS_NKAssignedTo = staff.GS_Code;
					var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					transferLinePickLines.Add(transferLine.PickLines[0]);
				});
				order1PickLines = transferLinePickLines;
			}

			var finalisedPickLines = new List<WhsPickLine>();
			order2PickLines.ForEach(pl =>
			{
				pl.WZ_GS_NKAssignedTo = staff.GS_Code;
				var transferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
				transferLine.WE_WL = data.Whs1.WW_DefaultOutboundDockDoor;
				transferLine.FinaliseDocketLine();
				finalisedPickLines.Add(transferLine.PickLines[0]);
			});

			Helper.Factory.Save();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertTaskIsSplitAppropriately(
				webService.Factory,
				response,
				expectedPickLinesOnNewTask: order1PickLines,
				task,
				expectedOriginalTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
				expectedPickLinesOnOriginalTask: finalisedPickLines);
		}

		public void TestGetWhsPick_ByReference_DoesNotModifyTasksWithPendingLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var org2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(org2, data.Part2);

			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m);
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m);
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			AssertEquals(true, receive1.IsFinalised);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(org2, data.Whs1, "Order2", Notify);
			Helper.CreateWhsOrderLine(order2, data.Part2, 30m);

			var pick = Helper.CreatePickNew(order1, order2);
			var pickLines = pick.GetAllPickLines();

			var order1PickLines = order1.Lines.SelectMany(l => l.PickLines);
			var order2PickLines = order2.Lines.SelectMany(l => l.PickLines);

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var searchCriteria = new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0, ClientCode = data.Org1.OH_Code };
			var response = webService.GetWhsPick(pick.WP_PickNo, searchCriteria);

			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			AssertTaskIsSplitAppropriately(
				webService.Factory,
				response,
				expectedPickLinesOnNewTask: order1PickLines,
				task,
				expectedOriginalTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
				expectedPickLinesOnOriginalTask: order2PickLines);
		}

		void AssertTaskIsSplitAppropriately(
			BusinessObjectFactory factory,
			WhsPickWebServiceResponse response,
			IEnumerable<WhsPickLine> expectedPickLinesOnNewTask,
			ProcessTask originalTask,
			string expectedOriginalTaskStatus,
			IEnumerable<WhsPickLine> expectedPickLinesOnOriginalTask)
		{
			AssertNotEquals(Guid.Empty, response.TaskPK);

			var newTaskInNewFactory = factory.Load<ProcessTask>(response.TaskPK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, newTaskInNewFactory.P9_Status);

			var taskPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, response.TaskPK));
			AssertContainsExactElementsInAnyOrder(expectedPickLinesOnNewTask.Select(p => p.PK), taskPickLines.Select(p => p.PK));

			var originalTaskInNewFactory = factory.Load<ProcessTask>(originalTask.PK);
			if (expectedOriginalTaskStatus != null)
			{
				var originalTaskPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, originalTask.PK));
				AssertNotNull(originalTaskInNewFactory);
				AssertEquals(expectedOriginalTaskStatus, originalTaskInNewFactory.P9_Status);
				AssertContainsExactElementsInAnyOrder(expectedPickLinesOnOriginalTask.Select(p => p.PK), originalTaskPickLines.Select(p => p.PK));
			}
			else
			{
				AssertNull(originalTaskInNewFactory);
			}
		}

		[TestDate(2024, 05, 05)]
		public void TestGetWhsPick_ByReference_TaskManagementDBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (var i = 0; i < 20; i++)
			{
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			Helper.Factory.Save();

			var orders = new List<WhsOrder>();
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order" + i, Notify);
				Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				orders.Add(order);
			}
			Helper.Factory.Save();

			var pick = Helper.CreatePickNew(orders.ToArray());
			var pickLines = pick.GetAllPickLines();

			var finalisedPickLines = new List<WhsPickLine>();
			foreach (var order in orders)
			{
				var task = Helper.CreateProcessTaskForPickJob(pick, staff, setPickLineFKs: false);
				var orderPickLines = order.Lines.SelectMany(l => l.PickLines).ToList();
				orderPickLines.ForEach(pl => pl.WZ_P9_Task = task.PK);

				var transferLine = Helper.PickAndMakeInTransitTransfer(orderPickLines[0], ZDateTimeOffset.Now);
				transferLine.FinaliseDocketLine();
			}
			Helper.Factory.Save();

			// We have:
			// 10 Unpicked Lines
			// 10 Picked and Putaway Lines
			// 10 Orders + Order Lines
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ ProcessTasksSchema.Constants.TableName, 3 }, // CreatePickTask + HandleOrphanedTask + Factory Save
				{ StmALogSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 5 }, // 2x PickLinesLoader + GetCompletePallets + DDA + WhsPickLineInfoCollection
				{ WhsDocketLineSchema.Constants.TableName, 5 }, // 4x PickLinesLoader + GetCompletePallets
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 4 }, // PickLinesLoader + GetCompletePallets + GetPickedLinesToPutaway + HandleOrphanedTask
			};

			var webService = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });

				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		#endregion

		#endregion
	}
}
