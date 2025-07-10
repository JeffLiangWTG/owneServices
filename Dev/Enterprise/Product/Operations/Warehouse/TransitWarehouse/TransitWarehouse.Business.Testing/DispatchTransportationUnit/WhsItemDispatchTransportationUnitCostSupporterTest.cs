using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemDispatchTransportationUnitCostSupporterTest : WhsItemTransportationUnitCostSupporterTest<WhsItemDispatchTransportationUnit>
	{
		public void TestShipmentList()
		{
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", Warehouse.PK);
			var costSupporter = GetCostSupporter(dtu);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IJobInvoicingPlugIn>(), costSupporter.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), costSupporter.ShipmentsListPKs);

			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", Warehouse.PK);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", Warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", Warehouse.PK);

			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn1, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn3, dtu2, dll);

			var costSupporter1 = GetCostSupporter(dtu1);
			var costSupporter2 = GetCostSupporter(dtu2);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn1, dcn2 }, costSupporter1.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { dcn1.PK, dcn2.PK }, costSupporter1.ShipmentsListPKs);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn3 }, costSupporter2.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { dcn3.PK }, costSupporter2.ShipmentsListPKs);
		}

		public void TestCreditorPK()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			var costSupporter = GetCostSupporter(dtu);
			foreach (var chargeCode in dtu.DefaultChargeGroups)
			{
				AssertEquals(dtu.CreditorPK, costSupporter.GetCreditorPK(chargeCode, ZGuid.Empty));
			}
		}

		public void TestCreditorPK_TransportCompanyIsOverridden()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", Warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", Warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			dtu.TransportCompany.E2_AddressOverride = true;
			dtu.TransportCompany.E2_CompanyName = "Transport Co";

			var costSupporter = GetCostSupporter(dtu);
			foreach (var chargeCode in dtu.DefaultChargeGroups)
			{
				AssertEquals(ZGuid.Empty, costSupporter.GetCreditorPK(chargeCode, ZGuid.Empty));
			}
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
			var costSupporter = GetCostSupporter(dtu);

			AssertEquals(ZString.Empty, costSupporter.MasterBillNum);

			var additionalReference = dtu.AdditionalReferenceNumbers.AddNew();
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
			var costSupporter = GetCostSupporter(dtu);

			AssertEquals(ZDateTime.Empty, costSupporter.ETA);
			AssertEquals(ZDateTime.Empty, costSupporter.ETD);
		}

		#region Implementation

		protected override IGenericJobCostSupporter GetCostSupporter(WhsItemDispatchTransportationUnit transportationUnit) => new WhsItemTransportationUnitCostSupporter<WhsItemDispatchTransportationUnit>(transportationUnit);

		#endregion
	}
}
