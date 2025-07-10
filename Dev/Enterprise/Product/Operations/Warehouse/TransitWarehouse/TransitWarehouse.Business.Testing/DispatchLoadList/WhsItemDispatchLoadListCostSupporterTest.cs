using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemDispatchLoadListCostSupporterTest : TestCaseWithFactory
	{
		public void TestShipmentList()
		{
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var costSupporter = GetCostSupporter(dll);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IJobInvoicingPlugIn>(), costSupporter.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), costSupporter.ShipmentsListPKs);

			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", Warehouse.PK);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", Warehouse.PK);

			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn1, dtu, dll1);
			Helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu, dll1);
			Helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu, dll1);
			Helper.CreatePackageState(rcn, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn3, dtu, dll2);

			var costSupporter1 = GetCostSupporter(dll1);
			var costSupporter2 = GetCostSupporter(dll2);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn1, dcn2 }, costSupporter1.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { dcn1.PK, dcn2.PK }, costSupporter1.ShipmentsListPKs);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn3 }, costSupporter2.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { dcn3.PK }, costSupporter2.ShipmentsListPKs);
		}

		public void TestMasterBillNum()
		{
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
			var costSupporter = GetCostSupporter(dll);

			AssertEquals(ZString.Empty, costSupporter.MasterBillNum);

			var additionalReference = dll.AdditionalReferenceNumbers.AddNew();
			additionalReference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReference.CE_EntryNum = "MAB Test";

			AssertEquals("MAB Test", costSupporter.MasterBillNum);
		}

		public void TestETAAndETD()
		{
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
			var costSupporter = GetCostSupporter(dll);

			AssertEquals(ZDateTime.Empty, costSupporter.ETA);
			AssertEquals(ZDateTime.Empty, costSupporter.ETD);
		}

		public void TestGetCreditorPK()
		{
			var creditor = Helper.CreateClient("CDT");
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK, creditor: creditor);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
			var costSupporter = GetCostSupporter(dll);

			AssertEquals(ZGuid.Empty, costSupporter.GetCreditorPK("", ZGuid.Empty));
			AssertEquals(creditor.PK, costSupporter.GetCreditorPK(ChargeCodeGroupList.Codes.TRWDispatchLoadList, ZGuid.Empty));
		}

		#region Implementation

		IGenericJobCostSupporter GetCostSupporter(WhsItemDispatchLoadList loadList) => new WhsItemDispatchLoadListCostSupporter(loadList);

		WhsWarehouse Warehouse => warehouse ?? (warehouse = Helper.CreateTRWWarehouse());
		WhsWarehouse warehouse;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
