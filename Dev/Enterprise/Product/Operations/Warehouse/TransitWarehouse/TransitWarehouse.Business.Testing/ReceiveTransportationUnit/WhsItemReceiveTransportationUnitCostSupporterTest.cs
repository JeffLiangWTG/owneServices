using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemReceiveTransportationUnitCostSupporterTest : WhsItemTransportationUnitCostSupporterTest<WhsItemReceiveTransportationUnit>
	{
		public void TestShipmentList()
		{
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", Warehouse.PK, stageLocation.PK);
			var costSupporter = GetCostSupporter(rtu);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<IJobInvoicingPlugIn>(), costSupporter.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), costSupporter.ShipmentsListPKs);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", Warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", Warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", Warehouse.PK, stageLocation.PK);
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", Warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn3, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu2);
			Helper.CreatePackageState(rtu3, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Arrived);

			var costSupporter1 = GetCostSupporter(rtu1);
			var costSupporter2 = GetCostSupporter(rtu2);
			var costSupporter3 = GetCostSupporter(rtu3);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { rcn1, rcn2 }, costSupporter1.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { rcn1.PK, rcn2.PK }, costSupporter1.ShipmentsListPKs);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { rcn3 }, costSupporter2.ShipmentsList);
			AssertContainsExactElementsInAnyOrder(new[] { rcn3.PK }, costSupporter2.ShipmentsListPKs);

			AssertEquals(0, costSupporter3.ShipmentsList.Length);
			AssertEquals(0, costSupporter3.ShipmentsListPKs.Length);
		}

		public void TestCreditorPK()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var costSupporter = GetCostSupporter(rtu);
			foreach (var chargeCode in rtu.DefaultChargeGroups)
			{
				AssertEquals(rtu.CreditorPK, costSupporter.GetCreditorPK(chargeCode, ZGuid.Empty));
			}
		}
		public void TestCreditorPK_TransportCompanyIsOverridden()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			rtu.TransportCompany.E2_AddressOverride = true;
			rtu.TransportCompany.E2_CompanyName = "Transport Co";

			var costSupporter = GetCostSupporter(rtu);
			foreach (var chargeCode in rtu.DefaultChargeGroups)
			{
				AssertEquals(ZGuid.Empty, costSupporter.GetCreditorPK(chargeCode, ZGuid.Empty));
			}
		}

		public void TestMasterBillNum()
		{
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var costSupporter = GetCostSupporter(rtu);

			AssertEquals(ZString.Empty, costSupporter.MasterBillNum);

			var additionalReference = rtu.AdditionalReferenceNumbers.AddNew();
			additionalReference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReference.CE_EntryNum = "MAB Test";

			AssertEquals("MAB Test", costSupporter.MasterBillNum);
		}

		public void TestETAAndETD()
		{
			var stageLocation = Warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", Warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rtu2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived);

			var testDate = ZDateTime.Now;
			rcn1.WRC_ExpectedArrivalTime = testDate;
			rcn1.WRC_ExpectedDispatchTime = testDate.AddHours(1);
			rcn2.WRC_ExpectedArrivalTime = testDate.AddHours(2);
			rcn2.WRC_ExpectedDispatchTime = testDate.AddHours(3);

			var costSupporter1 = GetCostSupporter(rtu1);
			var costSupporter2 = GetCostSupporter(rtu2);

			AssertEquals(testDate, costSupporter1.ETA);
			AssertEquals(testDate.AddHours(3), costSupporter1.ETD);
			AssertEquals(ZDateTime.Empty, costSupporter2.ETA);
			AssertEquals(ZDateTime.Empty, costSupporter2.ETD);
		}

		#region Implementation

		protected override IGenericJobCostSupporter GetCostSupporter(WhsItemReceiveTransportationUnit transportationUnit) => new WhsItemTransportationUnitCostSupporter<WhsItemReceiveTransportationUnit>(transportationUnit);

		#endregion
	}
}
