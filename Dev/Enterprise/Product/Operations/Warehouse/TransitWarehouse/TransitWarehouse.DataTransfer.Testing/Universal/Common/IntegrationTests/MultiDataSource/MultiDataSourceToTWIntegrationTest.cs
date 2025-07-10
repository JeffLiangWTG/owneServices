using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class MultiDataSourceToTWIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region From different priority source

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenSendForwardingShipment_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendSeaCargo_ThenSendForwardingShipment_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_ThenSendAirCargo_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB2", "S00001000",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_ThenSendSeaCargo_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenSendForwardingShipment_ThenSeaCargo_OverrideRCNWhenSendForwardingShipment()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenSendForwardingShipment_ThenSeaCargo_ThenUnderBond_OverrideRCNWhenSendForwardingShipment()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
			(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
			(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
			(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"),
			(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenSendForwardingShipment_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenSendOutturn_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenSendAirCargo_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"));

		#endregion

		#region From same priority source

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_ThenForwardingShipmentFromSameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "", "S00001001",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_ThenForwardingShipmentFromDifferentSender_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "", "S00001000",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_ThenForwardingShipmentFromDifferentSenderTwice_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "", "S00001000",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderB"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendForwardingShipment_FromDifferentSender_FromTheSameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(3, "", "S00001002",
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderB"),
				(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenAirCargo_SameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenAirCargo_DifferentSender_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenAirCargo_DifferentSenderTwice_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_FromDifferentSender_FromTheSameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(3, "MB1", "S00001000",
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"),
				(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendSeaCargo_ThenSeaCargo_SameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"),
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"));

		//We don't create universal link for seacargo so the RCN would always be overriden
		[TestDate(2018, 1, 1)]
		public void TestSendSeaCargo_ThenSeaCargo_DifferentSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"),
				(DataSourceType: DataContextType.Outturn, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenUnderBond_DifferentSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenUnderBond_SameSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderB"),
				(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenSeaCargo_DifferentSender_NotOverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
		(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
		(DataSourceType: DataContextType.Outturn, SenderName: "SenderB"));

		//We don't create universal link for seacargo so the RCN would always be overriden
		[TestDate(2018, 1, 1)]
		public void TestSendSeaCargo_ThenAirCargo_DifferentSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
		(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"),
		(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_ThenUnderBond_DifferentSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
		(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"),
		(DataSourceType: DataContextType.UnderBond, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_ThenAirCargo_DifferentSender_OverrideRCN()
			=> TestImportRCNFromMultiSource(2, "MB1", "S00001000",
		(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"),
		(DataSourceType: DataContextType.AirManifest, SenderName: "SenderB"));

		[TestDate(2018, 1, 1)]
		public void TestSendShipment_Base()
			=> TestImportRCNFromMultiSource(1, "", "S00001000",
		(DataSourceType: DataContextType.ForwardingShipment, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendSeaCargo_Base()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
		(DataSourceType: DataContextType.Outturn, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendAirCargo_Base()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
		(DataSourceType: DataContextType.AirManifest, SenderName: "SenderA"));

		[TestDate(2018, 1, 1)]
		public void TestSendUnderBond_Base()
			=> TestImportRCNFromMultiSource(1, "MB1", "S00001000",
		(DataSourceType: DataContextType.UnderBond, SenderName: "SenderA"));

		#endregion

		#region TestImportRCNFromMultiSource_PublishOutturn_SeaCargoAndForwarding

		public void TestImportRCNFromMultiSource_PublishOutturn_SeaCargoAndForwarding()
		{
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			var testData = CreateTestData(true, parentProcessIsConsol: true);
			Factory.Save();

			var cargoLines = ImportSeaCargo(testData);
			AssertReceiveConsignmentAndPackageAndASNCreated_SeaCargo("HB1", "MB1", "PKG", 2, "CNT1", "SAFETY EQUIPMENT", "DRIVE1", 1333.1m, "KG", 2.79m, "M3");
			ImportForwardingConsol(testData);
			// Package details has been overridden
			AssertReceiveConsignmentAndPackageAndASNCreated_SeaCargo("HB1", "MB1", "PLT", 1, "CNT1", "", "", 0m, "KG", 0m, "M3");

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Only 1 receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentAfterImport = receiveConsignmentsAfterImport.Single(r => r.WRC_ConsignmentID == "HB1");
			AssertEquals(1, receiveConsignmentAfterImport.PackageStates.Count);

			var packageStateForHSB1 = receiveConsignmentAfterImport.PackageStates.Single();
			var containerRTU = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("CNT1", containerRTU.ContainerNumber);

			containerRTU.WRH_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(containerRTU);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1, containerRTU, "PKG1");
			Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: containerRTU, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: containerRTU, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");

			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(containerRTU);

			Factory.Save();

			CombineAssertions(() =>
			{
				TriggerAndFireToOutturn(containerRTU);
				var factoryToLoadOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
				var fclCargoLine = factoryToLoadOutturn.Load<DepotCusOutturn>(cargoLines.fclCargoLine.PK);
				AssertEquals(ZDateTime.Empty, fclCargoLine.C5_CargoUnpackDate);
				AssertEquals(3, fclCargoLine.C5_PackagesOutturned);
				AssertEquals(CMRPackageTypes.Codes.Package, fclCargoLine.C5_PackagesUnits);
				AssertEquals(false, fclCargoLine.C5_SealIntactIndicator);
				AssertEquals("SU", fclCargoLine.C5_OutturnResultType);

				var hb1CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(cargoLines.lclCargoLine.PK);
				AssertCargoPackLine(hb1CargoLineInAnotherFactory, 3, 3m, 4m, isDamaged: false, isPillaged: false, "SU", packType: "PK");
			});
		}

		public void TestImportRCNFromMultiSource_PublishOutturn_ForwardingAndSeaCargo()
		{
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			var testData = CreateTestData(true, parentProcessIsConsol: true);
			Factory.Save();

			ImportForwardingConsol(testData);
			AssertReceiveConsignmentAndPackageAndASNCreated_SeaCargo("HB1", "MB1", "PLT", 1, "CNT1", "", "", 0m, "KG", 0m, "M3", false);
			var cargoLines = ImportSeaCargo(testData);
			// packages information should not be overridden, since forwarding has higher priority
			AssertReceiveConsignmentAndPackageAndASNCreated_SeaCargo("HB1", "MB1", "PLT", 1, "CNT1", "", "", 0m, "KG", 0m, "M3", false);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Only 1 receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentAfterImport = receiveConsignmentsAfterImport.Single(r => r.WRC_ConsignmentID == "HB1");
			AssertEquals(1, receiveConsignmentAfterImport.PackageStates.Count);

			var packageStateForHSB1 = receiveConsignmentAfterImport.PackageStates.Single();
			var containerRTU = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("CNT1", containerRTU.ContainerNumber);

			containerRTU.WRH_WL_StagingLocation = testData.warehouse.DefaultLocation.PK;
			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(containerRTU);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1, containerRTU, "PKG1");
			Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: containerRTU, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: containerRTU, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");

			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(containerRTU);

			Factory.Save();

			CombineAssertions(() =>
			{
				TriggerAndFireToOutturn(containerRTU);
				var factoryToLoadOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
				var fclCargoLine = factoryToLoadOutturn.Load<DepotCusOutturn>(cargoLines.fclCargoLine.PK);
				AssertEquals(ZDateTime.Empty, fclCargoLine.C5_CargoUnpackDate);
				AssertEquals(3, fclCargoLine.C5_PackagesOutturned);
				AssertEquals(CMRPackageTypes.Codes.Package, fclCargoLine.C5_PackagesUnits);
				AssertEquals(false, fclCargoLine.C5_SealIntactIndicator);
				AssertEquals("SU", fclCargoLine.C5_OutturnResultType);
				var rcnJobLinks = newFactory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentTableCode, "WRC"));
				var rtuJobLinks = newFactory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentTableCode, "WRH"));
				AssertEquals(1, rcnJobLinks.Length);
				AssertEquals(nameof(DataContextType.ForwardingShipment), rcnJobLinks[0].UCL_SourceType);
				AssertEquals(rcnJobLinks[0].UCL_SourceKey, "S00001000");
				AssertEquals(rtuJobLinks.Length, 1);
				AssertEquals(nameof(DataContextType.SeaCargoOutturn), rtuJobLinks[0].UCL_SourceType);
				AssertEquals(rtuJobLinks[0].UCL_SourceKey, "O00000001");

				var hb1CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(cargoLines.lclCargoLine.PK);
				AssertCargoPackLine(hb1CargoLineInAnotherFactory, 3, 3m, 4m, isDamaged: false, isPillaged: false, "SU", packType: "PK");
			});
		}

		public void TestImportRCNFromMultiSource_PublishOutturn_AirCargoAndForwarding()
		{
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			var gateInTime = now.AddDays(+2);
			var unloadTime = now.AddDays(+2);
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			var testData = CreateTestData(true, parentProcessIsConsol: true);
			Factory.Save();

			var cargoLines = ImportAirCargo(testData);
			AssertReceiveConsignmentAndPackage_AirCargo("HB1", "MB1", "PCE", 10, "Air cargo pieces", "", 0m, "KG", 0m, "M3");
			ImportForwardingConsol(testData, "AIR");
			// Package details has been overridden
			AssertReceiveConsignmentAndPackage_AirCargo("HB1", "MB1", "PLT", 1, "", "", 0m, "KG", 0m, "M3");

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Only 1 receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentAfterImport = receiveConsignmentsAfterImport.Single(r => r.WRC_ConsignmentID == "HB1");
			AssertEquals(1, receiveConsignmentAfterImport.PackageStates.Count);
			AssertEquals(TransportModes.Air, receiveConsignmentAfterImport.WRC_TransportMode);
			AssertConsignmentAdditionalRefs(receiveConsignmentAfterImport, houseBill: "HB1", shipmentID: "", masterbill: "MB1");
			AssertEquals("RCN Destination", "AUSYD", receiveConsignmentAfterImport.WRC_RL_NKDestination);

			var packageStateForHSB1 = receiveConsignmentAfterImport.PackageStates.Single();
			var rtu = Helper.CreateReceiveTransportationUnit("ULD1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(rtu);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1, rtu, "PKG1", weight: 1.1m, volume: 1.79m, weightUQ: "KG", volumeUQ: "M3", setDetails: true);
			var packageState1 = Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			var packageState2 = Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageBookedDetail(packageState1.Package, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageBookedDetail(packageState2.Package, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();
			rtu.WRH_GateInTime = gateInTime;
			rtu.WRH_UnloadCompleteTime = unloadTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			Factory.Save();

			TriggerAndFireOutturn(receiveConsignmentAfterImport);
			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cargoLines.cusHAWB.PK);
			AssertPackageOutturned(cusHAWB1AfterSendingOutturn, 3, 3, 4.1m, "KG", 5.79m, "CU", false, false, "NIL", unloadTime.ToZDateTime(), gateInTime.ToZDateTime(), "PF", true);
		}

		public void TestImportRCNFromMultiSource_PublishOutturn_ForwardingAndAirCargo()
		{
			var now = ZDateTimeOffset.Now.WithoutSeconds();
			var gateInTime = now.AddDays(+2);
			var unloadTime = now.AddDays(+2);
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			var testData = CreateTestData(true, parentProcessIsConsol: true);
			Factory.Save();

			ImportForwardingConsol(testData, "AIR");
			// Package details has been overridden
			AssertReceiveConsignmentAndPackage_AirCargo("HB1", "MB1", "PLT", 1, "", "", 0m, "KG", 0m, "M3");
			var cargoLines = ImportAirCargo(testData);
			AssertReceiveConsignmentAndPackage_AirCargo("HB1", "MB1", "PLT", 1, "", "", 0m, "KG", 0m, "M3");

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Only 1 receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentAfterImport = receiveConsignmentsAfterImport.Single(r => r.WRC_ConsignmentID == "HB1");
			AssertEquals(1, receiveConsignmentAfterImport.PackageStates.Count);
			AssertEquals(TransportModes.Air, receiveConsignmentAfterImport.WRC_TransportMode);
			AssertConsignmentAdditionalRefs(receiveConsignmentAfterImport, houseBill: "HB1", shipmentID: "", masterbill: "MB1");
			AssertEquals("RCN Destination", "AUSYD", receiveConsignmentAfterImport.WRC_RL_NKDestination);

			var packageStateForHSB1 = receiveConsignmentAfterImport.PackageStates.Single();
			var rtu = Helper.CreateReceiveTransportationUnit("ULD1", testData.warehouse.PK, testData.warehouse.DefaultLocation.PK);
			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(rtu);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1, rtu, "PKG1", weight: 1.1m, volume: 1.79m, weightUQ: "KG", volumeUQ: "M3", setDetails: true);
			var packageState1 = Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			var packageState2 = Helper.CreatePackageState(receiveConsignmentAfterImport, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageBookedDetail(packageState1.Package, weight: 1m, volume: 1m, weightUQ: "KG", volumeUQ: "M3");
			Helper.CreatePackageBookedDetail(packageState2.Package, weight: 2m, volume: 3m, weightUQ: "KG", volumeUQ: "M3");
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();
			rtu.WRH_GateInTime = gateInTime;
			rtu.WRH_UnloadCompleteTime = unloadTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			Factory.Save();

			TriggerAndFireOutturn(receiveConsignmentAfterImport);
			var afterSendingOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
			var cusHAWB1AfterSendingOutturn = afterSendingOutturn.Load<CusHAWB>(cargoLines.cusHAWB.PK);
			AssertPackageOutturned(cusHAWB1AfterSendingOutturn, 3, 3, 4.1m, "KG", 5.79m, "CU", false, false, "NIL", unloadTime.ToZDateTime(), gateInTime.ToZDateTime(), "PF", true);
			var rcnJobLinks = newFactory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentTableCode, "WRC"));
			AssertEquals(2, rcnJobLinks.Length);

			AssertEquals(1, rcnJobLinks.Where(r => r.UCL_SourceType == nameof(DataContextType.AirManifestLine)).Count());
			AssertEquals(1, rcnJobLinks.Where(r => r.UCL_SourceType == nameof(DataContextType.ForwardingShipment)).Count());

			AssertEquals("A00000001", rcnJobLinks.Where(r => r.UCL_SourceType == nameof(DataContextType.AirManifestLine)).Single().UCL_SourceKey);
			AssertEquals("S00001000", rcnJobLinks.Where(r => r.UCL_SourceType == nameof(DataContextType.ForwardingShipment)).Single().UCL_SourceKey);
		}

		#endregion

		//import index begins with 1 in order to create proper test data
		void TestImportRCNFromMultiSource(int expectedImportIndex, string expectedMasterBillNumber = "MB1", string expectedShipmentNumber = "S00001000", params (DataContextType DataSourceType, string SenderName)[] imports)
		{
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			var testData = CreateTestData(false, parentProcessIsConsol: false);

			var senderNames = imports.Select(i => i.SenderName).Distinct().ToList();
			var senderDictionary = new Dictionary<string, (GlbStaff staff, GlbBranch branch, GlbDepartment department)>();
			for (var n = 0; n < senderNames.Count; n++)
			{
				senderDictionary.Add(senderNames[n], CreateSender(n + 1, senderNames[n]));
			}
			Factory.Save();

			(UniversalShipment shipment, Transport outboundLegInShipment) shipmentDataToAssert = (null, null);
			var shouldSetPremiseID = true;

			#region Simulate Imports

			for (var index = 1; index < imports.Length + 1; index++)
			{
				var import = imports[index - 1];
				var senderInfo = senderDictionary[import.SenderName];
				using (Env.SetTemporaryUserContext(senderInfo.staff.PK.ToGuid(), senderInfo.branch.PK.ToGuid(), senderInfo.department.PK.ToGuid()))
				{
					switch (import.DataSourceType)
					{
						case DataContextType.AirManifest:
							{
								ImportAirCargo(testData, index);
								break;
							}
						case DataContextType.Outturn:
							{
								ImportSeaCargo(testData, index, shouldSetPremiseID);
								//We can only set PrimiseID once
								shouldSetPremiseID = false;
								SetConsignor(testData.consignor);
								break;
							}
						case DataContextType.ForwardingShipment:
							{
								var shipmentData = ImportForwardingShipment(testData, index);

								if (index == expectedImportIndex)
								{
									shipmentDataToAssert = shipmentData;
								}
								break;
							}
						case DataContextType.UnderBond:
							{
								ImportUnderBond(testData, shouldSetPremiseID);
								shouldSetPremiseID = false;
								break;
							}
					}
				}
			}

			#endregion

			#region Assertions

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var receiveConsignmentsAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Only 1 receive consignment should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignmentAfterImport = receiveConsignmentsAfterImport.Single(r => r.WRC_ConsignmentID == "HB1");

			var targetType = imports[expectedImportIndex - 1].DataSourceType;

			// We want the remain RCN data matches the import of expected index
			// We use different assertions for the result from different expected import DataSource
			switch (targetType)
			{
				case DataContextType.AirManifest:
					{
						AssertAirCargo(receiveConsignmentAfterImport, expectedImportIndex);
						break;
					}
				case DataContextType.Outturn:
					{
						AssertSeaCargo(receiveConsignmentAfterImport, expectedImportIndex);
						break;
					}
				case DataContextType.ForwardingShipment:
					{
						AssertForwardingShipment(newFactory, expectedImportIndex, receiveConsignmentAfterImport, testData, shipmentDataToAssert.shipment, shipmentDataToAssert.outboundLegInShipment, expectedMasterBillNumber, expectedShipmentNumber);
						break;
					}
				case DataContextType.UnderBond:
					{
						AssertUnderBond(receiveConsignmentAfterImport, "TestOrg" + senderNames[expectedImportIndex - 1]);
						break;
					}
			}

			#endregion
		}

		void SetConsignor(OrgHeader consignor)
		{
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();

			if (receiveConsignmentsAfterImport != null && !Helper.HasJobDocAddressOfType(receiveConsignmentsAfterImport, DocAddressTypes.Codes.LocalCartageExporter))
			{
				Helper.CreateJobDocAddressFromAddress(receiveConsignmentsAfterImport, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
				Factory.Save();
			}
		}

		#region AirCargo

		void ImportAirCargo((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, int n)
		{
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(testData.warehouse, $"MB{n}");
			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1", "NZAKL", "AUSYD", 10, testData.consignor, testData.consignee, "Air cargo pieces");

			TriggerAndFireTransitRequestForRelease(cusMAWB);
		}

		(CusMAWB cusMAWB, CusHAWB cusHAWB) ImportAirCargo((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData)
		{
			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(testData.warehouse, "MB1");
			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1", "NZAKL", "AUSYD", 10, testData.consignor, testData.consignee, "Air cargo pieces");
			CreateUnderbond(cusMAWB, "U0000001");

			TriggerAndFireTransitRequestForRelease(cusMAWB);

			return (cusMAWB, cusHAWBWithPieces);
		}

		void AssertAirCargo(WhsItemReceiveConsignment rcn, int expectRCNFromWhichImport = 1)
		{
			AssertEquals("Next discharge port imported.", "AUSYD", rcn.WRC_RL_NKNextDischargePort);

			AssertConsignmentAdditionalRefs(rcn, houseBill: "HB1", shipmentID: "", masterbill: $"MB{expectRCNFromWhichImport}");
			AssertEquals("RCN Destination", "AUSYD", rcn.WRC_RL_NKDestination);
			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, rcn.PackageStates.Count);

			var receiveConsignment1Package = rcn.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1Package, qty: 10, packType: "PCE", "Air cargo pieces");
		}

		#endregion

		#region UnderBond

		void ImportUnderBond((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, bool shouldSetPremiseID)
		{
			var premiseId = "PREMISEID";
			if (shouldSetPremiseID)
			{
				WhsTransitTestHelper.SetPremiseIDForWarehouse(testData.warehouse, premiseId, CountryCodes.Australia);
			}

			CreateWorkflowTemplateForAirCargoDepotOutturn();

			var outturn = CreateCusOutturn("HB1", "BG", 4);
			var underbond = CreateCusUnderbond("MB1", "OR123", premiseId, testData.vessel, new[] { outturn });

			TriggerAndFireTransitRequestForRelease(underbond);
		}

		void AssertUnderBond(WhsItemReceiveConsignment rcn, string senderName)
		{
			AssertEquals("Booking Party Company Name:", senderName, rcn.BookingPartyDocAddress.CompanyName);
			AssertConsignmentAdditionalRefs(rcn, houseBill: "HB1", shipmentID: "", masterbill: "MB1");
			AssertEquals("1 packline created for the 4 pieces on HB1.", 1, rcn.PackageStates.Count);

			var receiveConsignment1Package = rcn.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1Package, qty: 4, packType: "BG", "");
		}

		#endregion

		#region SeaCargo

		(DepotCusOutturn fclCargoLine, DepotCusOutturn lclCargoLine) ImportSeaCargo((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, int n = 1, bool shouldSetPremiseID = true)
		{
			if (shouldSetPremiseID)
			{
				WhsTransitTestHelper.SetPremiseIDForWarehouse(testData.warehouse, "PREMISEID");
			}
			var outturnHeader = Factory.New<CusOutturnHeader>();

			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, testData.warehouse, "PREMISEID", $"Lloyds{n}", $"Voyage{n}", $"Vessel{n}");

			var fclCargoLine = CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, $"CNT{n}", "", "", 2, CMRPackageTypes.Codes.Package, "CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
				19026, "KG", "Marks And Numbers", "SEA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			var lclCargoLine = CreateCargoPackLine(outturnHeader, "LCL", $"CNT{n}", "HB1", "MB1", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
				1333.1m, "KG", $"DRIVE{n}", "SEA", 1333.1m, "KG", 2.79m, "CU", $"COMPANY NAME {n}");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			return (fclCargoLine, lclCargoLine);
		}

		void AssertSeaCargo(WhsItemReceiveConsignment rcn, int expectRCNFromWhichImport = 1)
		{
			AssertEquals($"COMPANY NAME {expectRCNFromWhichImport}", rcn.ConsigneeDocAddress.CompanyName);
			AssertEquals(TransportModes.Sea, rcn.WRC_TransportMode);

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault(r => r.WRH_VehicleReference == $"CNT{expectRCNFromWhichImport}");
			AssertNotNull($"Container number should be CNT{expectRCNFromWhichImport}.", rtu);

			var packageState = rcn.PackageStates.Single();
			var pivot = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == packageState.ReceiveASN.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK);
			var receiveASN = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault(a => a.PK == packageState.ReceiveASN.PK);
			AssertNotNull(pivot);
			AssertNotNull(receiveASN);

			var package = packageState.Package;
			AssertPackageProperties(package, qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: $"DRIVE{expectRCNFromWhichImport}", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			AssertConsignmentAdditionalRefs(rcn, houseBill: "HB1", shipmentID: "", masterbill: "MB1");

			AssertEquals($"Voyage{expectRCNFromWhichImport}", receiveASN.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber).CE_EntryNum);
			AssertEquals($"Lloyds{expectRCNFromWhichImport}", receiveASN.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds).CE_EntryNum);
		}

		#endregion

		#region ForwardingShipment

		(UniversalShipment shipment, Transport outboundLegInShipment) ImportForwardingShipment((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, int n = 1)
		{
			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			var shipment = CreateShipment("HB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			shipment.JS_OA_ExportReceivingDepot = testData.cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			var outboundLegInShipment = CreateTransport(shipment, 1, "SEA", "A", $"AA{n}", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));
			CreateOuterPackline(shipment, n, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);
			return (shipment, outboundLegInShipment);
		}

		void ImportForwardingConsol((OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, string transportMode = "SEA", bool includeHouseBill = true, bool includeContainer = true)
		{
			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Consol.Code);

			var consol = CreateConsol("MB1", null, "NZAKL", "AUSYD", transportMode: transportMode);
			var shipment = includeHouseBill ? CreateShipment(consol, "HB1", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee) : CreateShipment(consol, "", "NZAKL", "AUSYD", testData.today.AddDays(-1), testData.today.AddDays(9), testData.consignor, testData.consignee);
			consol.JK_OA_UnpackDepotAddress = testData.cfs.MainAddress.PK;
			var outboundLegInShipment = CreateTransport(shipment, 1, "SEA", "A", $"AA1", "NZAKL", "AUSYD", testData.today, testData.today.AddDays(2));
			if (includeContainer)
			{
				var container = CreateContainer(consol, "CNT1", 1, "20GP");
				CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);
			}

			TriggerAndFireTransitRequestUsingBookingRequested(consol);
		}

		void AssertForwardingShipment(BusinessObjectFactory factory, int expectRCNFromWhichImport, WhsItemReceiveConsignment rcn, (OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) testData, UniversalShipment shipment, Transport outboundLegInShipment, string expectedMasterBillNumber = "MB1", string expectedShipmentNumber = "S00001000")
		{
			AssertConsignment(factory, "HB1", testData.today.AddDays(-1), testData.today, "AUSYD", "STD", testData.warehouse, shipment.PK, outboundLegInShipment);
			AssertConsignmentAdditionalRefs(rcn, "HB1", expectedShipmentNumber, expectedMasterBillNumber, ZString.Empty, $"AA{expectRCNFromWhichImport}", "01-Jan-18 00:00");
			AssertEquals("RCN Destination", "AUSYD", rcn.WRC_RL_NKDestination);
			AssertConsignmentPackages(rcn, 1);
		}

		#endregion

		(GlbStaff staff, GlbBranch branch, GlbDepartment department) CreateSender(int n, string senderName)
		{
			CreateOrgHeader($"EDIDATGC{n}", $"TestOrg{senderName}");

			var orgHeaderProxy = Factory.NewWithValidTestData<OrgHeader>();
			var companyNew = Factory.NewWithValidTestData<GlbCompany>();
			companyNew.CompanyName = $"Company{senderName}";
			companyNew.GC_Code = $"GC{n}";
			companyNew.GC_OH_OrgProxy = orgHeaderProxy.PK;

			var orgHeaderNew = Factory.NewWithValidTestData<OrgHeader>();
			var branchNew = Factory.NewWithValidTestData<GlbBranch>();
			branchNew.GB_BranchName = $"homeBranch{senderName}";
			branchNew.GB_Code = $"GB{n}";
			branchNew.GB_GC = companyNew.PK;
			branchNew.GB_OH_OrgProxy = orgHeaderNew.PK;

			var staffNew = Factory.NewWithValidTestData<GlbStaff>();
			staffNew.GS_FullName = $"Staff{senderName}";
			staffNew.GS_Code = $"GS{n}";
			staffNew.GS_GB_HomeBranch = branchNew.PK;

			var departmentNew = Factory.NewWithValidTestData<GlbDepartment>();
			departmentNew.GE_Code = $"DP{n}";

			return (staffNew, branchNew, departmentNew);
		}

		static void AssertReceiveConsignmentAndPackage_AirCargo(string houseBill, string masterBill, string packType, int packageQty, string goodsDescription, string marksAndNumbers, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignment = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, houseBill)).Single();
			AssertNotNull(receiveConsignment);
			var packageState = receiveConsignment.PackageStates.Single();
			var package = packageState.Package;
			AssertPackageProperties(package, qty: packageQty, packType, goodsDescription: goodsDescription,
				marksAndNumbers: marksAndNumbers, weight: weight, weightUQ: weightUQ, volume: volume, volumeUQ: volumeUQ);
			AssertConsignmentAdditionalRefs(receiveConsignment, houseBill: houseBill, shipmentID: "", masterbill: masterBill);
		}

		static void AssertReceiveConsignmentAndPackageAndASNCreated_SeaCargo(string houseBill, string masterBill, string packType, int packageQty, string containerNum, string goodsDescription, string marksAndNumbers, decimal weight, string weightUQ, decimal volume, string volumeUQ, bool checkVesselLloyds = true)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignment = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, houseBill)).Single();
			AssertNotNull(receiveConsignment);
			var receiveASNs = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Single ASN must be created for the container", 1, receiveASNs.Length);
			var packageState = receiveConsignment.PackageStates.Single();
			var asn = receiveASNs.Where(a => a.WRP_VehicleReference == containerNum).Single();
			AssertEquals(asn, packageState.ReceiveASN);
			if (checkVesselLloyds)
			{
				AssertAdditionalReference(asn, asn.AdditionalReferenceNumbers.Cast<CusEntryNumber>().ToArray(), WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, "Lloyds1");
			}
			var package = packageState.Package;
			AssertPackageProperties(package, qty: packageQty, packType, goodsDescription: goodsDescription,
				marksAndNumbers: marksAndNumbers, weight: weight, weightUQ: weightUQ, volume: volume, volumeUQ: volumeUQ);
			AssertEquals(TransportModes.Sea, receiveConsignment.WRC_TransportMode);
			AssertConsignmentAdditionalRefs(receiveConsignment, houseBill: houseBill, shipmentID: "", masterbill: masterBill);
		}

		static void AssertPackageProperties(PkgPackage package, int qty, string packType, string goodsDescription, string marksAndNumbers, decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			AssertEquals(qty, package.KP_PackageQty);
			AssertEquals(packType, package.KP_F3_NKPackType);
			AssertEquals(goodsDescription, package.KP_GoodsDescription);
			AssertEquals(marksAndNumbers, package.KP_MarksAndNumbers);
			AssertEquals(weight, package.KP_Weight);
			AssertEquals(weightUQ, package.KP_WeightUQ);
			AssertEquals(volume, package.KP_Volume);
			AssertEquals(volumeUQ, package.KP_VolumeUQ);
		}

		OrgHeader CreateOrgHeader(string code, string fullName)
		{
			var client = Helper.CreateClient(code, fullName);
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "Address fullName";
			address.OA_City = "SYD";

			return client;
		}
	}
}
