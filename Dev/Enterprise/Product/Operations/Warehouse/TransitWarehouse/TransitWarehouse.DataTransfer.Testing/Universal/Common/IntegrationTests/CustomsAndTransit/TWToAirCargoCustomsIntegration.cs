using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class TWToAirCargoCustomsIntegration : IntegrationTestCaseWithFactory
	{
		#region Universal Shipments Tests

		public void TestSendAirCargoReportToTW_CreatesReceiveConsignmentsAndPackages_FromUnderbondOfHAWB()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");
			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");
			var cusHAWB2 = CreateAirCargoHouse(cusMAWB, "HB2", "MB1234", "NZAKL", "AUSYD", 1, consignor, consignee, "");
			CreateUnderbond(cusHAWB1, "U0000001");
			CreateUnderbond(cusHAWB2, "U0000002");
			AssertEquals("Precondition", (ZShort)0, cusHAWB1.CS_PiecesLanded);
			AssertEquals("Precondition", (ZShort)0, cusHAWB2.CS_PiecesLanded);

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Two receive consignments should have been created for two house bills.", 2, receiveConsignmentsAfterImport.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "HB1", "HB2" }, receiveConsignmentsAfterImport.Select(r => r.WRC_ConsignmentID));
			var receiveConsignmentHSB1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var receiveConsignmentHSB2 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignmentHSB1.WRC_RL_NKNextDischargePort);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignmentHSB2.WRC_RL_NKNextDischargePort);
			AssertEquals(TransportModes.Air, receiveConsignmentHSB1.WRC_TransportMode);
			AssertEquals(TransportModes.Air, receiveConsignmentHSB2.WRC_TransportMode);

			AssertConsignmentAdditionalRefs(receiveConsignmentHSB1, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertConsignmentAdditionalRefs(receiveConsignmentHSB2, houseBill: "HB2", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN1 Destination", "AUSYD", receiveConsignmentHSB1.WRC_RL_NKDestination);
			AssertEquals("RCN2 Destination", "AUSYD", receiveConsignmentHSB2.WRC_RL_NKDestination);

			AssertEquals("1 package created for the 2 pieces on HB1.", 1, receiveConsignmentHSB1.PackageStates.Count);
			AssertEquals("1 package created for 1 piece on HB1.", 1, receiveConsignmentHSB2.PackageStates.Count);

			var receiveConsignmentHSB1Package = receiveConsignmentHSB1.PackageStates.Single();
			var receiveConsignmentHSB2Package = receiveConsignmentHSB2.PackageStates.Single();
			AssertPackageProperties(receiveConsignmentHSB1Package.Package, qty: 2, packType: "PCE", "Air cargo pieces");
			AssertPackageProperties(receiveConsignmentHSB2Package.Package, qty: 1, packType: "PCE", "");

			var universalLinkForRCN1 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignmentHSB1, DataContextType.AirManifestLine, null).Single();
			var universalLinkForRCN2 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignmentHSB2, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWB1.CS_MessageReference, universalLinkForRCN1.Key);
			AssertEquals(cusHAWB2.CS_MessageReference, universalLinkForRCN2.Key);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: now, unLoadCompleteTime: now.AddDays(+2));
			UnloadAndLabelPackage(receiveConsignmentHSB1Package, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			UnloadAndLabelPackage(receiveConsignmentHSB2Package, rtu2, "BOX1", setDetails: true, weight: 3.1m, weightUQ: "KG", volume: 3.79m, volumeUQ: "M3", isPillaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);

			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cusHAWB1.PK);
			var cusHAWB2AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cusHAWB2.PK);
			AssertPackageOutturned(cusHAWB1AfterSendingOutturn, 2, 1, 2.1m, "KG", 2.79m, "CU", false, false, "SH", now.AddDays(+2).ToZDateTime(), now.AddDays(+2).ToZDateTime(), "YC");
			AssertPackageOutturned(cusHAWB2AfterSendingOutturn, 1, 1, 3.1m, "KG", 3.79m, "CU", false, true, "NIL", null, null, "YC");

			UnloadAndLabelPackage(receiveConsignmentHSB1Package, rtu1, "PLT2", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", isDamaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);

			var afterUpdatingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterUpdatingOutturn = afterUpdatingOutturn.Load<CusHAWB>(cusHAWB1.PK);
			var cusHAWB2AfterUpdatingOutturn = afterUpdatingOutturn.Load<CusHAWB>(cusHAWB2.PK);

			AssertPackageOutturned(cusHAWB1AfterUpdatingOutturn, 2, 2, 4.2m, "KG", 5.58m, "CU", true, false, "NIL", now.AddDays(+2).ToZDateTime(), now.AddDays(+2).ToZDateTime(), "YC");
			AssertPackageOutturned(cusHAWB2AfterUpdatingOutturn, 1, 1, 3.1m, "KG", 3.79m, "CU", false, true, "NIL", null, null, "YC");

			var surplusPackageState = Helper.CreatePackageState(receiveConsignmentHSB1, 1, PkgUnit.Piece, "SUPackage", TransitWarehouseStatuses.Codes.Arrived);
			UnloadAndLabelPackage(surplusPackageState, rtu1, "SUPackage", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", isDamaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);

			var afterUpdatingOutturn2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterUpdatingOutturn2 = afterUpdatingOutturn2.Load<CusHAWB>(cusHAWB1.PK);
			var cusHAWB2AfterUpdatingOutturn2 = afterUpdatingOutturn2.Load<CusHAWB>(cusHAWB2.PK);

			AssertPackageOutturned(cusHAWB1AfterUpdatingOutturn2, 2, 3, 6.3m, "KG", 8.37m, "CU", true, false, "SU", now.AddDays(+2).ToZDateTime(), now.AddDays(+2).ToZDateTime(), "YC");
			AssertPackageOutturned(cusHAWB2AfterUpdatingOutturn2, 1, 1, 3.1m, "KG", 3.79m, "CU", false, true, "NIL", null, null, "YC");
		}

		public void TestSendAirCargoReportToTW_CreatesReceiveConsignmentsAndPackages_FromUnderbondOfMAWB()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");
			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");
			var cusHAWB2 = CreateAirCargoHouse(cusMAWB, "HB2", "MB1234", "NZAKL", "AUSYD", 1, consignor, consignee, "");
			CreateUnderbond(cusMAWB, "U0000001");
			AssertEquals("Precondition", (ZShort)0, cusHAWB1.CS_PiecesLanded);
			AssertEquals("Precondition", (ZShort)0, cusHAWB2.CS_PiecesLanded);

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Two receive consignments should have been created for two house bills.", 2, receiveConsignmentsAfterImport.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "HB1", "HB2" }, receiveConsignmentsAfterImport.Select(r => r.WRC_ConsignmentID));
			var receiveConsignmentHSB1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var receiveConsignmentHSB2 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignmentHSB1.WRC_RL_NKNextDischargePort);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignmentHSB2.WRC_RL_NKNextDischargePort);
			AssertEquals(TransportModes.Air, receiveConsignmentHSB1.WRC_TransportMode);
			AssertEquals(TransportModes.Air, receiveConsignmentHSB2.WRC_TransportMode);

			AssertConsignmentAdditionalRefs(receiveConsignmentHSB1, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertConsignmentAdditionalRefs(receiveConsignmentHSB2, houseBill: "HB2", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN1 Destination", "AUSYD", receiveConsignmentHSB1.WRC_RL_NKDestination);
			AssertEquals("RCN2 Destination", "AUSYD", receiveConsignmentHSB2.WRC_RL_NKDestination);

			AssertEquals("1 package created for the 2 pieces on HB1.", 1, receiveConsignmentHSB1.PackageStates.Count);
			AssertEquals("1 package created for 1 piece on HB1.", 1, receiveConsignmentHSB2.PackageStates.Count);

			var receiveConsignmentHSB1Package = receiveConsignmentHSB1.PackageStates.Single();
			var receiveConsignmentHSB2Package = receiveConsignmentHSB2.PackageStates.Single();
			AssertPackageProperties(receiveConsignmentHSB1Package.Package, qty: 2, packType: "PCE", "Air cargo pieces");
			AssertPackageProperties(receiveConsignmentHSB2Package.Package, qty: 1, packType: "PCE", "");

			var universalLinkForRCN1 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignmentHSB1, DataContextType.AirManifestLine, null).Single();
			var universalLinkForRCN2 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignmentHSB2, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWB1.CS_MessageReference, universalLinkForRCN1.Key);
			AssertEquals(cusHAWB2.CS_MessageReference, universalLinkForRCN2.Key);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: now, unLoadCompleteTime: now.AddDays(+2));
			UnloadAndLabelPackage(receiveConsignmentHSB1Package, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			UnloadAndLabelPackage(receiveConsignmentHSB2Package, rtu2, "BOX1", setDetails: true, weight: 3.1m, weightUQ: "KG", volume: 3.79m, volumeUQ: "M3", isPillaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);

			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cusHAWB1.PK);
			var cusHAWB2AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cusHAWB2.PK);
			AssertPackageOutturned(cusHAWB1AfterSendingOutturn, 2, 1, 2.1m, "KG", 2.79m, "CU", false, false, "SH", now.AddDays(+2).ToZDateTime(), now.AddDays(+2).ToZDateTime(), "YC", true);
			AssertPackageOutturned(cusHAWB2AfterSendingOutturn, 1, 1, 3.1m, "KG", 3.79m, "CU", false, true, "NIL", null, null, "YC", true);

			UnloadAndLabelPackage(receiveConsignmentHSB1Package, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", isDamaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);

			var afterUpdatingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterUpdatingOutturn = afterUpdatingOutturn.Load<CusHAWB>(cusHAWB1.PK);
			var cusHAWB2AfterUpdatingOutturn = afterUpdatingOutturn.Load<CusHAWB>(cusHAWB2.PK);

			AssertPackageOutturned(cusHAWB1AfterUpdatingOutturn, 2, 2, 4.2m, "KG", 5.58m, "CU", true, false, "NIL", now.AddDays(+2).ToZDateTime(), now.AddDays(+2).ToZDateTime(), "YC", true);
			AssertPackageOutturned(cusHAWB2AfterUpdatingOutturn, 1, 1, 3.1m, "KG", 3.79m, "CU", false, true, "NIL", null, null, "YC", true);
		}

		public void TestSendAirCargoReportToTW_CreatesReceiveConsignmentsAndPackages_FromUnderbond_WhenCannotFindByCusMAWB()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "9922W", CountryCodes.Australia);

			CreateWorkflowTemplateForAirCargoDepotOutturn();

			var outturn1 = CreateCusOutturn("HB3", "BG", 4);
			outturn1.C5_MasterBill = "1111111";
			var aplVessel = CreateVessel("APL VESSEL", "9832343");

			var underbond = CreateCusUnderbond("1111111", "OR123", "9922W", aplVessel, new[] { outturn1 });

			TriggerAndFireTransitRequestForRelease(underbond);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("one receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignment = receiveConsignmentsAfterImport.FirstOrDefault();

			var packageState = receiveConsignment.PackageStates.Single();

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: now, unLoadCompleteTime: now.AddDays(+2));
			UnloadAndLabelPackage(packageState, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");
			Factory.Save();

			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();
			TriggerAndFireOutturn(receiveConsignment);

			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var outturns = afterSendingOutturn.Load<CusOutturn>(new ZQuery());
			Assert("should update outturn", outturns.Length == 1);
			AssertEquals("1111111", outturns.FirstOrDefault().C5_MasterBill);
			var outturn = outturns.FirstOrDefault();
			AssertEquals("Outturn package qty must be correct.", 4, outturn.C5_OuterPacks);
			AssertEquals("Outturn outturned qty must be correct.", 1, outturn.C5_PackagesOutturned);
			AssertEquals("Outturn pack type must be correct.", "", outturn.C5_PackagesUnits);
			AssertEquals("Outturn weight must be correct.", 2.1m, outturn.C5_WeightOutturned);
			AssertEquals("Outturn weight unit must be correct.", "KG", outturn.C5_WeightOutturnedUQ);
			AssertEquals("Outturn weight must be correct.", 2.79m, outturn.C5_VolumeOutturned);
			AssertEquals("Outturn weight unit must be correct.", "CU", outturn.C5_VolumeOutturnedUQ);
		}

		public void TestSendAirCargoReportToTW_PopulateUnderbondWithLatestRTUUnloadTimeWhenRelateMutipleRTU()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");
			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");
			var cusHAWB2 = CreateAirCargoHouse(cusMAWB, "HB2", "MB1234", "NZAKL", "AUSYD", 1, consignor, consignee, "");
			var cusHAWB3 = CreateAirCargoHouse(cusMAWB, "HB3", "MB1234", "NZAKL", "AUSYD", 1, consignor, consignee, "");
			var underbond = CreateUnderbond(cusMAWB, "U0000001");
			AssertEquals("Precondition", (ZShort)0, cusHAWB1.CS_PiecesLanded);
			AssertEquals("Precondition", (ZShort)0, cusHAWB2.CS_PiecesLanded);

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Two receive consignments should have been created for two house bills.", 3, receiveConsignmentsAfterImport.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "HB1", "HB2", "HB3" }, receiveConsignmentsAfterImport.Select(r => r.WRC_ConsignmentID));
			var receiveConsignmentHSB1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var receiveConsignmentHSB2 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			var receiveConsignmentHSB3 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB3");

			var receiveConsignmentHSB1Package = receiveConsignmentHSB1.PackageStates.Single();
			var receiveConsignmentHSB2Package = receiveConsignmentHSB2.PackageStates.Single();
			var receiveConsignmentHSB3Package = receiveConsignmentHSB3.PackageStates.Single();
			AssertPackageProperties(receiveConsignmentHSB1Package.Package, qty: 2, packType: "PCE", "Air cargo pieces");
			AssertPackageProperties(receiveConsignmentHSB2Package.Package, qty: 1, packType: "PCE", "");

			var today = DateTime.Today;
			var yesterday = today.AddDays(-1);
			var gateInTime = today.AddDays(-1);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: gateInTime, unLoadCompleteTime: today);
			UnloadAndLabelPackage(receiveConsignmentHSB1Package, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2", gateIn: gateInTime, unLoadCompleteTime: yesterday);
			UnloadAndLabelPackage(receiveConsignmentHSB2Package, rtu2, "BOX1", setDetails: true, weight: 3.1m, weightUQ: "KG", volume: 3.79m, volumeUQ: "M3", isPillaged: true);

			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, warehouse.DefaultLocation.PK, "V2", gateIn: gateInTime);
			UnloadAndLabelPackage(receiveConsignmentHSB3Package, rtu3, "BOX1", setDetails: true, weight: 3.1m, weightUQ: "KG", volume: 3.79m, volumeUQ: "M3", isPillaged: true);

			TriggerAndFireOutturn(receiveConsignmentHSB1);
			TriggerAndFireOutturn(receiveConsignmentHSB2);
			TriggerAndFireOutturn(receiveConsignmentHSB3);

			var afterUpdating = new BusinessObjectFactory() { RefreshEnabled = false };
			var underbondAfterUpdating = afterUpdating.Load<CusUnderbond>(underbond.PK);

			AssertEquals("Should populate underbond with latest date", today, underbondAfterUpdating.C4_Outurned);
		}

		public void TestSendTWToAirCargoReport_CreateNewUnderbond_WhenRCNHasMasterBillButNoMatchUnderbond()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();

			var masterBill = "111-1111";

			var cusMAWB = CreateAirCargoReport(warehouse, masterBill);
			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Two receive consignments should have been created for one house bills.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentHSB1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			AssertEquals(TransportModes.Air, receiveConsignmentHSB1.WRC_TransportMode);
			var nowDateTime = new ZDateTime(2024, 10, 27);
			receiveConsignmentHSB1.WRC_ExpectedArrivalTime = nowDateTime;
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var localCTO = data.Org1;
			Helper.CreateJobDocAddressFromAddress(receiveConsignmentHSB1, DocAddressTypes.Codes.LocalCartageCTO, localCTO.MainAddress);

			var packageState = receiveConsignmentHSB1.PackageStates.Single();

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: now, unLoadCompleteTime: now.AddDays(+2));
			UnloadAndLabelPackage(packageState, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");
			TriggerAndFireOutturn(receiveConsignmentHSB1);

			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cusHAWB1.PK);
			var underbonds = afterSendingOutturn.Load<CusUnderbond>(new ZQuery());
			AssertEquals("underbond should have been created.", 1, underbonds.Length);
			var underbond = underbonds.Single();
			AssertEquals("Should create with master bill", "1111111", underbond.C4_MAWB);
			AssertEquals("Should create with Arrival Date", nowDateTime, underbond.C4_ArrivalDate);
			AssertEquals("Should create with Movement Reason", "DCL", underbond.C4_MovementReason);
			AssertEquals("Should create with Mode Of Movement", "AIR", underbond.C4_ModeOfMovement);
			AssertEquals("Should create with Is Move From Discharge", true, underbond.C4_IsMoveFromDischarge);
			AssertEquals("Should create with Pieces Manifested", 2, underbond.C4_PiecesManifested);
			AssertEquals("Should create with Origin Address", localCTO.MainAddress.PK, underbond.C4_OA_OriginAddress);
			AssertEquals("Should create with Destination Address", receiveConsignmentHSB1.Warehouse.WarehouseAddress.PK, underbond.C4_OA_DestinationAddress);

			var outturn = underbond.Outturns[0];
			AssertEquals("Outturn weight", 2.1m, outturn.C5_WeightOutturned);
			Factory.Save();

			// Send instruction from Underbond to RCN again
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			var factoryAfterExport = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusMAWBAfterExport = factoryAfterExport.Load<CusMAWB>(cusMAWB.PK);
			var ediMessageAfterExport = UniversalHelper.GetEDIMessageFromDB(cusMAWBAfterExport, AutoEvents.DataExportCode);
			var notesAfterExport = ediMessageAfterExport.GetNotes().GetAllNotes();
			AssertContains("Should print log", "Receive Consignment has been partially received. Package level data will be ignored and not read in.", notesAfterExport.Cast<StmNote>().First().ST_NoteDataAsText);
		}

		#endregion

		#region TestSendTWToAirCargoReport_CreateCusMAWBAndCusHAWB

		public void TestSendTWToAirCargoReport_CreateCusMAWBAndCusHAWB()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			CreateWorkflowTemplateForReceiveASNToAirCargo();
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "9922W", CountryCodes.Australia);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_HouseBillNumber = "HB1";

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			asn.WRP_TransportMode = TransportModes.Air;
			var masterBill = asn.AdditionalReferenceNumbers.AddNew();
			masterBill.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			masterBill.CE_EntryNum = "891111111110";
			var voyageFlightNumber = asn.AdditionalReferenceNumbers.AddNew();
			voyageFlightNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber;
			voyageFlightNumber.CE_EntryNum = "F0001";

			var packageState = Helper.CreatePackageState(rcn, 1, PkgUnit.Pallet, "Package1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			packageState.Package.KP_Weight = 2.1m;
			packageState.Package.KP_PackageQty = 1;

			Factory.Save();

			TriggerAndSendASN(asn);

			var afterSendingASN = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusMAWBs = afterSendingASN.Load<CusMAWB>(new ZQuery());
			var cusHAWBs = afterSendingASN.Load<CusHAWB>(new ZQuery());
			Assert("should create cusMAWB", cusMAWBs.Length == 1);
			Assert("should create cusHAWB", cusHAWBs.Length == 1);

			var cusMAWB = cusMAWBs.FirstOrDefault();
			var cusHAWB = cusHAWBs.FirstOrDefault();

			AssertEquals("891111111110", cusMAWB.CM_MAWB);
			AssertEquals("F0001", cusMAWB.CM_FlightNo);
			AssertEquals(warehouse.WarehouseAddress.PK, cusMAWB.CM_OA_UnpackDepotAddress);

			AssertEquals("HB1", cusHAWB.CS_HAWB);
			AssertEquals("PLT", cusHAWB.CS_PackType);
			AssertEquals(2.1m, cusHAWB.CS_Weight);
			AssertEquals("KG", cusHAWB.CS_WeightUQ);
			AssertEquals(new ZShort(1), cusHAWB.CS_PiecesManifested);

			var asnfterExport = afterSendingASN.Load<WhsItemReceiveASN>(asn.PK);
			var ediMessageAfterExport = UniversalHelper.GetEDIMessageFromDB(asnfterExport, AutoEvents.DataExportCode);
			var notesAfterExport = ediMessageAfterExport.GetNotes().GetAllNotes();
			AssertContains("Should print log", @"No matching CusMAWB found, creating new CusMAWB.
Populating CusMAWB...
Successfully matched organization with code 'EDICUSCHC'.
No matching CusHAWB found, creating new CusHAWB.
Populating CusHAWB...
Added Air Cargo House (HAWB: HB1) from UniversalShipment.
Added AirCargo Report (MAWB: 891-111111110) from UniversalShipment.
Successfully saved AirCargo Report (MAWB: 891-111111110) with 1 x CusHAWB.", notesAfterExport.Cast<StmNote>().First().ST_NoteDataAsText);

			// Now try to send outturn from rcn
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, gateIn: now, unLoadCompleteTime: now.AddDays(+2));
			UnloadAndLabelPackage(packageState, rtu1, "PLT1", setDetails: true, weight: 2.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");
			TriggerAndFireOutturn(rcn);

			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var outturns = afterSendingOutturn.Load<CusOutturn>(new ZQuery());
			var underBonds = afterSendingOutturn.Load<CusUnderbond>(new ZQuery());
			Assert("should update outturn", outturns.Length == 1);
			Assert("should update underBond", underBonds.Length == 1);
			AssertEquals("891111111110", outturns.FirstOrDefault().C5_MasterBill);

			var outturn = outturns.FirstOrDefault();
			var underBond = underBonds.FirstOrDefault();

			AssertEquals("Outturn package qty must be correct.", 1, outturn.C5_OuterPacks);
			AssertEquals("Outturn outturned qty must be correct.", 1, outturn.C5_PackagesOutturned);
			AssertEquals("Outturn pack type must be correct.", "PF", outturn.C5_PackagesUnits);
			AssertEquals("Outturn weight must be correct.", 2.1m, outturn.C5_WeightOutturned);
			AssertEquals("Outturn weight unit must be correct.", "KG", outturn.C5_WeightOutturnedUQ);
			AssertEquals("Outturn weight must be correct.", 2.79m, outturn.C5_VolumeOutturned);
			AssertEquals("Outturn weight unit must be correct.", "CU", outturn.C5_VolumeOutturnedUQ);

			AssertEquals("underBond masterBill", "891111111110", underBond.C4_MAWB);
			AssertEquals("underBond C4_FlightNo", "", underBond.C4_FlightNo);
			AssertEquals("underBond C4_PiecesManifested", 1, underBond.C4_PiecesManifested);
			AssertEquals("underBond C4_ModeOfMovement", "", underBond.C4_ModeOfMovement);
			AssertEquals("underBond C4_MovementReason", "DCL", underBond.C4_MovementReason);
			AssertEquals("underBond C4_IsMoveFromDischarge", true, underBond.C4_IsMoveFromDischarge);
			AssertEquals("underBond C4_ParentID", cusMAWB.PK, underBond.C4_ParentID);
			AssertEquals("underBond C4_ParentTableCode", "CM", underBond.C4_ParentTableCode);
			AssertEquals("Should create with Destination Address", rcn.Warehouse.WarehouseAddress.PK, underBond.C4_OA_DestinationAddress);
		}

		#endregion

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory);
	}
}
