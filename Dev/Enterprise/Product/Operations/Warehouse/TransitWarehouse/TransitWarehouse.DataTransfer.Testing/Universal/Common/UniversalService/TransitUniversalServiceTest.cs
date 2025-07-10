using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class TransitUniversalServiceTest : IntegrationTestCaseWithFactory
	{
		#region TestPublishUniversal_FromRCN_SuccessfullyUpdatedShipment

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_SuccessfullyUpdated()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			//Shipment 1
			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			UnloadAndLabelPackage(packageStateForBox, rtu1, "BOX1");

			//Received Overs
			Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, Constants.PkgUnit.Case, "CAS2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);

			//Shipment 2
			var consol2 = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol2.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment2 = CreateShipment(consol2, "HSB2", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline3 = CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Pallet);
			var packline4 = CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Box);
			var packline5 = CreateOuterPackline(shipment2, 1, Constants.PkgUnit.Carton);

			TriggerAndFireTransitRequestUsingBookingRequested(consol2);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline3.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline4.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline5.JL_PackLineId);

			var receiveConsignment2 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB2")).Single();
			AssertEquals(3, receiveConsignment2.PackageStates.Count);

			packageStateForPallet = receiveConsignment2.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			packageStateForBox = receiveConsignment2.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			var packageStateForCarton = receiveConsignment2.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Carton);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(2, packageStateForBox.Package.KP_PackageQty);
			AssertEquals(1, packageStateForCarton.Package.KP_PackageQty);

			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, warehouse.DefaultLocation.PK, "V3");
			UnloadAndLabelPackage(packageStateForPallet, rtu3, "PLT2");
			UnloadAndLabelPackage(packageStateForPallet, rtu3, "PLT3");

			var rtu4 = Helper.CreateReceiveTransportationUnit("RTU4", warehouse.PK, warehouse.DefaultLocation.PK, "V4");
			UnloadAndLabelPackage(packageStateForBox, rtu3, "BOX2");
			UnloadAndLabelPackage(packageStateForBox, rtu4, "BOX3");
			UnloadAndLabelPackage(packageStateForCarton, rtu4, "CARTON1");

			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK, receiveConsignment2.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Success, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder successfully updated related job.\r\n{receiveConsignment2.WRC_ConsignmentID}: Outturn to Forwarder successfully updated related job.", errorMessage.Message);
			var factoryToLoadShipment = new BusinessObjectFactory() { RefreshEnabled = false };

			//Asssert Shipment 1
			var shipmentInAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment.PK);
			var packLinesInShipment = shipmentInAnotherFactory.OuterPackLines;
			AssertEquals(3, packLinesInShipment.Count);

			var packLinesForPLT = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packLinesForBOX = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCAS = packLinesInShipment.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Case);
			AssertEquals("No new packline should be generated.", packLinesForPLT.PK, packline1.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForPLT.PkgPackageCollection.Count);

			AssertEquals("No new packline should be generated.", packLinesForBOX.PK, packline2.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForBOX.PkgPackageCollection.Count);

			AssertEquals("new packline should be created with pkgpackages attached", 2, packLinesForCAS.PkgPackageCollection.Count);

			//Asssert Shipment 2
			var shipment2InAnotherFactory = factoryToLoadShipment.Load<ForwardingShipment>(shipment2.PK);
			var packLinesInShipment2 = shipment2InAnotherFactory.OuterPackLines;
			AssertEquals(3, packLinesInShipment2.Count);

			packLinesForPLT = packLinesInShipment2.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Pallet);
			packLinesForBOX = packLinesInShipment2.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Box);
			var packLinesForCarton = packLinesInShipment2.Cast<ForwardingPackLine>().Single(p => p.JL_F3_NKPackType == Constants.PkgUnit.Carton);

			AssertEquals("No new packline should be generated.", packLinesForPLT.PK, packline3.PK);
			AssertEquals("PkgPackage should created and attached to packline", 2, packLinesForPLT.PkgPackageCollection.Count);

			AssertEquals("No new packline should be generated.", packLinesForBOX.PK, packline4.PK);
			AssertEquals("PkgPackage should created and attached to packline", 2, packLinesForBOX.PkgPackageCollection.Count);

			AssertEquals("No new packline should be generated.", packLinesForCarton.PK, packline5.PK);
			AssertEquals("PkgPackage should created and attached to packline", 1, packLinesForCarton.PkgPackageCollection.Count);
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromDCN_SuccessfullySent()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			//Shipment 1
			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "AUSYD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline1 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			var packline2 = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline1.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline2.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(2, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForBox = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(1, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "V2");
			UnloadAndLabelPackage(packageStateForBox, rtu2, "BOX1");

			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			//Shipment 2
			var consol2 = CreateConsol("", vessel, "NZCHC", "AUADL", "AUSYD");
			consol2.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment2 = CreateShipment(consol2, "HSB2", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline3 = CreateOuterPackline(shipment2, 2, Constants.PkgUnit.Pallet);
			var packline4 = CreateOuterPackline(shipment2, 3, Constants.PkgUnit.Box);

			TriggerAndFireTransitRequestUsingBookingRequested(consol2);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline3.JL_PackLineId);
			AssertNotNullOrEmpty("Precondition: Packline id is set", packline4.JL_PackLineId);

			var receiveConsignment2 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB2")).Single();
			AssertEquals(2, receiveConsignment2.PackageStates.Count);

			packageStateForPallet = receiveConsignment2.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			packageStateForBox = receiveConsignment2.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(3, packageStateForBox.Package.KP_PackageQty);

			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, warehouse.DefaultLocation.PK, "V3");
			UnloadAndLabelPackage(packageStateForPallet, rtu3, "PLT2");
			UnloadAndLabelPackage(packageStateForPallet, rtu3, "PLT3");

			var rtu4 = Helper.CreateReceiveTransportationUnit("RTU4", warehouse.PK, warehouse.DefaultLocation.PK, "V4");
			UnloadAndLabelPackage(packageStateForBox, rtu4, "BOX2");
			UnloadAndLabelPackage(packageStateForBox, rtu4, "BOX3");
			UnloadAndLabelPackage(packageStateForBox, rtu4, "BOX4");

			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol2);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			//Assert Shipment 1
			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB1")).Single();
			AssertEquals("Should have created 1 Dispatch Consignment", "HSB1", dispatchConsignment.WDC_ConsignmentID);

			packageStateForPallet = dispatchConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			packageStateForBox = dispatchConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(1, packageStateForPallet.Package.KP_PackageQty);
			AssertEquals(1, packageStateForBox.Package.KP_PackageQty);

			//Assert Shipment 2
			var dispatchConsignment2 = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "HSB2")).Single();
			AssertEquals("Should have created 1 Dispatch Consignment", "HSB2", dispatchConsignment2.WDC_ConsignmentID);

			var packageStatesForPallet = dispatchConsignment2.PackageStates.Where(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStatesForBox = dispatchConsignment2.PackageStates.Where(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Box);
			AssertEquals(2, packageStatesForPallet.Count());
			AssertEquals(3, packageStatesForBox.Count());

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { dispatchConsignment.PK, dispatchConsignment2.PK }, WhsItemDispatchConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Success, errorMessage.ErrorType);

			AssertEquals($"{dispatchConsignment.WDC_ConsignmentID}: Outturn to Forwarder successfully updated related job.\r\n{dispatchConsignment2.WDC_ConsignmentID}: Outturn to Forwarder successfully updated related job.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_UnsupportedJobType_ReturnsError

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_UnsupportedJobType_ReturnsError()
		{
			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { Guid.NewGuid() }, WhsItemDispatchLoadListSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals("The job type WhsItemDispatchLoadList is not yet supported.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_ErrorInDEXEvent

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromRCN_ErrorInDEXEvent()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			CreateJobLink(receiveConsignment.PK, WhsItemReceiveConsignmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.OrgProxy.OH_Code, nameof(DataContextType.ForwardingConsol), false);
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder had an issue while processing (DCD - Discarded). Check DEX logs for details.", errorMessage.Message);
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromDCN_ErrorInDEXEvent()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			CreateJobLink(dispatchConsignment.PK, WhsItemDispatchConsignmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.OrgProxy.OH_Code, nameof(DataContextType.ForwardingConsol), false);
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { dispatchConsignment.PK }, WhsItemDispatchConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{dispatchConsignment.WDC_ConsignmentID}: Outturn to Forwarder had an issue while processing (DCD - Discarded). Check DEX logs for details.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_WarningWhenNoJobLink

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_WarningWhenNoJobLink()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Warning, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn/Packs cannot update Forwarding Shipment or Customs Declaration as no match could be found.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_CorrectDEXLogIsReturned

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromRCN_CorrectDEXLogIsReturned_2011Version()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2011_11))
			{
				AssertCorrectDEXLogIsReturned(WhsItemReceiveConsignmentSchema.Constants.TableName);
			}
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromRCN_CorrectDEXLogIsReturned_2012Version()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				AssertCorrectDEXLogIsReturned(WhsItemReceiveConsignmentSchema.Constants.TableName);
			}
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromDCN_CorrectDEXLogIsReturned_2011Version()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2011_11))
			{
				AssertCorrectDEXLogIsReturned(WhsItemDispatchConsignmentSchema.Constants.TableName);
			}
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromDCN_CorrectDEXLogIsReturned_2012Version()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				AssertCorrectDEXLogIsReturned(WhsItemDispatchConsignmentSchema.Constants.TableName);
			}
		}

		void AssertCorrectDEXLogIsReturned(string tableName)
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			WhsItemReceiveConsignment receiveConsignment = null;
			WhsItemDispatchConsignment dispatchConsignment = null;
			ZGuid jobPK = Guid.Empty;

			if (tableName == WhsItemReceiveConsignmentSchema.Constants.TableName)
			{
				receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
				receiveConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
				jobPK = receiveConsignment.PK;
				CreateJobLink(jobPK, WhsItemReceiveConsignmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.OrgProxy.OH_Code, nameof(DataContextType.ForwardingConsol), false);
			}
			else if (tableName == WhsItemDispatchConsignmentSchema.Constants.TableName)
			{
				dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
				dispatchConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
				jobPK = dispatchConsignment.PK;
				CreateJobLink(jobPK, WhsItemDispatchConsignmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.OrgProxy.OH_Code, nameof(DataContextType.ForwardingConsol), false);
			}

			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { jobPK }, tableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{(tableName == WhsItemReceiveConsignmentSchema.Constants.TableName ? receiveConsignment.WRC_ConsignmentID : dispatchConsignment.WDC_ConsignmentID)}: Outturn to Forwarder had an issue while processing (DCD - Discarded). Check DEX logs for details.", errorMessage.Message);

			TestDateAttribute.AddSeconds(-10); // To simulate concurrency issue. Publish the message in the past so that system has to select the correct StmALog

			if (tableName == WhsItemReceiveConsignmentSchema.Constants.TableName)
			{
				receiveConsignment.BookingPartyDocAddress.E2_OA_Address = Helper.CreateClient("AAB").MainAddress.PK;
			}
			else if (tableName == WhsItemDispatchConsignmentSchema.Constants.TableName)
			{
				dispatchConsignment.BookingPartyDocAddress.E2_OA_Address = Helper.CreateClient("AAB").MainAddress.PK;
			}

			Factory.Save();
			errorMessage = service.PublishUniversal(new ZGuid[] { jobPK }, tableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{(tableName == WhsItemReceiveConsignmentSchema.Constants.TableName ? receiveConsignment.WRC_ConsignmentID : dispatchConsignment.WDC_ConsignmentID)}: Outturn to Forwarder failed to send. Check Booking Party EDI Communications.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_NoForwarderOnRCN

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_NoForwarderOnRCN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = ZGuid.Empty;
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder failed to send. Check Booking Party EDI Communications.\r\n{receiveConsignment.WRC_ConsignmentID}: Forwarder: You selected 'FOR' for the Recipient Type, but no Organization has been entered for this Recipient Type on Receive Consignment RC00000001.\r\nPlease enter an Organization for this Recipient Type, or select another Recipient Type.", errorMessage.Message.Trim());
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_NoForwarderOnDCN()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(1, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			Factory.Save();

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Should have created 1 Dispatch Consignment", "HSB1", dispatchConsignment.WDC_ConsignmentID);
			AssertEquals(1, dispatchConsignment.PackageStates.Count);
			dispatchConsignment.BookingPartyDocAddress.E2_OA_Address = ZGuid.Empty;
			newBizOFactoryAfterRunningLogWalker.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { dispatchConsignment.PK }, WhsItemDispatchConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{dispatchConsignment.WDC_ConsignmentID}: Outturn to Forwarder failed to send. Check Booking Party EDI Communications.\r\n{dispatchConsignment.WDC_ConsignmentID}: Forwarder: You selected 'FOR' for the Recipient Type, but no Organization has been entered for this Recipient Type on Dispatch Consignment DC00000001.\r\nPlease enter an Organization for this Recipient Type, or select another Recipient Type.", errorMessage.Message.Trim());
		}

		#endregion

		#region TestPublishUniversal_QueuedMessage

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromRCN_QueuedMessage()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 2, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(2, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");
			var bookingParty = Helper.CreateClient("TES");
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			var mode = bookingParty.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "Shipment_Destination";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			mode.EK_Module = WorkflowDescriptors.TransitReceiveConsignment;
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Warning, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder has been queued to send. Check DEX logs for details.", errorMessage.Message);
		}

		[TestDate(2021, 09, 10)]
		public void TestPublishUniversal_FromDCN_QueuedMessage()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("", vessel, "NZCHC", "AUADL", "ABCD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			AssertNotNullOrEmpty("Precondition: Packline id is set", packline.JL_PackLineId);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1")).Single();
			AssertEquals(1, receiveConsignment.PackageStates.Count);

			var packageStateForPallet = receiveConsignment.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			AssertEquals(1, packageStateForPallet.Package.KP_PackageQty);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(packageStateForPallet, rtu1, "PLT1");

			var bookingParty = Helper.CreateClient("TES");
			var mode = bookingParty.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "Shipment_Destination";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			mode.EK_Module = WorkflowDescriptors.TransitDispatchConsignment;

			TriggerAndFireTransitRequestForRelease(consol);

			var newBizOFactoryAfterRunningLogWalker = new BusinessObjectFactory() { RefreshEnabled = false };

			var dispatchConsignment = newBizOFactoryAfterRunningLogWalker.Load<WhsItemDispatchConsignment>(new ZQuery()).Single();
			AssertEquals("Should have created 1 Dispatch Consignment", "HSB1", dispatchConsignment.WDC_ConsignmentID);
			AssertEquals(1, dispatchConsignment.PackageStates.Count);

			dispatchConsignment.BookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			newBizOFactoryAfterRunningLogWalker.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { dispatchConsignment.PK }, WhsItemDispatchConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Warning, errorMessage.ErrorType);
			AssertEquals($"{dispatchConsignment.WDC_ConsignmentID}: Outturn to Forwarder has been queued to send. Check DEX logs for details.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_CustomsOutturnAgentOnRCN_COASuccess

		[TestDate(2023, 09, 19)]
		public void TestPublishUniversal_CustomsOutturnAgentOnRCN_COASuccess()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse();

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");
			CreateUnderbond(cusHAWB1, "U0000001");
			AssertEquals("Precondition", (ZShort)0, cusHAWB1.CS_PiecesLanded);

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("One receive consignments should have been created.", 1, receiveConsignmentsAfterImport.Length);
			var rcn = receiveConsignmentsAfterImport[0];

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { rcn.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Success, errorMessage.ErrorType);
			AssertEquals($"{rcn.WRC_ConsignmentID}: Outturn to Customs Outturn Agent successfully updated related job.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_CustomsOutturnAgentOnRCN_COAFailed

		[TestDate(2023, 09, 19)]
		public void TestPublishUniversal_CustomsOutturnAgentOnRCN_COAFailed()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			CreateJobLink(receiveConsignment.PK, WhsItemReceiveConsignmentSchema.Constants.Prefix, GlbBranch.CurrentBranch.OrgProxy.OH_Code, nameof(DataContextType.AirManifestLine), false);
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Customs Outturn Agent had an issue while processing (ERR - Processed with Errors). Check DEX logs for details.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCN_COAFailed_FORSuccess

		public void TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCN_COAFailed_FORSuccess()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);
			var cusMAWB = CreateAirCargoReport(warehouse, "U0000001");

			var cusHAWB1 = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 2, consignor, consignee, "Air cargo pieces");
			CreateUnderbond(cusHAWB1, "U0000001");

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			var consol = CreateConsol("MB1234", vessel, "NZAKL", "AUSYD", "AUSYD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;
			var container = CreateContainer(consol, "CNT1", 1, "20GP");

			var shipment = CreateShipment(consol, "HB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Customs Outturn Agent had an issue while processing (DCD - Discarded). Check DEX logs for details.\r\n{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder successfully updated related job.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCN_COASuccess_FORFailed

		public void TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCN_COASuccess_FORFailed()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(warehouse, "MB1");
			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1", "NZAKL", "AUSYD", 10, consignor, consignee, "Air cargo pieces");
			CreateUnderbond(cusMAWB, "U0000001");

			TriggerAndFireTransitRequestForRelease(cusMAWB);

			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			var shipment = CreateShipment("HB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			var outboundLegInShipment = CreateTransport(shipment, 1, "SEA", "A", "AA1", "NZAKL", "AUSYD", today, today.AddDays(2));
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Error, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Customs Outturn Agent successfully updated related job.\r\n{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder had an issue while processing (DCD - Discarded). Check DEX logs for details.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCN_COASuccess_FORWarning

		public void TestPublishUniversal_ForwarderAndCustomsOutturnAgentOnRCNForWarningButCOAIsSuccess()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(warehouse, "MB1");
			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1", "NZAKL", "AUSYD", 10, consignor, consignee, "Air cargo pieces");
			CreateUnderbond(cusMAWB, "U0000001");

			TriggerAndFireTransitRequestForRelease(cusMAWB);

			CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(JobInvoicingConsumerTypes.Shipment.Code);

			var shipment = CreateShipment("HB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = ZGuid.Empty;
			var outboundLegInShipment = CreateTransport(shipment, 1, "SEA", "A", "AA1", "NZAKL", "AUSYD", today, today.AddDays(2));
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);

			TriggerAndFireTransitRequestUsingBookingRequested(shipment);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			UnloadAndLabelPackage(receiveConsignment.PackageStates[0], rtu1, "PLT1");
			var bookingParty = Helper.CreateClient("TES");
			receiveConsignment.BookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			var mode = bookingParty.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			mode.EK_Destination = "Shipment_Destination";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			mode.EK_Module = WorkflowDescriptors.TransitReceiveConsignment;
			Factory.Save();

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Warning, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Customs Outturn Agent successfully updated related job.\r\n{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder has been queued to send. Check DEX logs for details.", errorMessage.Message);
		}

		#endregion

		#region TestPublishUniversal_ForwarderAndExternalForwarderOnRCN_Success

		public void TestPublishUniversal_ForwarderAndExternalForwarderOnRCNForSuccess()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(false, "NZCHC");

			var consol = CreateConsol("MB1234", vessel, "NZAKL", "AUSYD", "AUSYD");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var shipment = CreateShipment(consol, "HB1", "NZAKL", "AUSYD", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet);
			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			CreateJobLink(receiveConsignment.PK, WhsItemReceiveConsignmentSchema.Constants.Prefix, cfs.OH_Code, nameof(DataContextType.ForwardingConsol), false);

			var service = new TransitUniversalService();
			var errorMessage = service.PublishUniversal(new ZGuid[] { receiveConsignment.PK }, WhsItemReceiveConsignmentSchema.Constants.TableName);
			AssertEquals(MessageTypes.Success, errorMessage.ErrorType);
			AssertEquals($"{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder successfully updated related job.\r\n{receiveConsignment.WRC_ConsignmentID}: Outturn to Forwarder successfully updated related job.", errorMessage.Message);
		}

		#endregion

		void CreateJobLink(ZGuid parentPK, string prefix, string orgHeaderCode, string sourceType, bool isEmptyOwner)
		{
			var orgHeader = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgHeaderCode)).FirstOrDefault();

			var jobLink = Factory.NewWithValidTestData<StmUniversalJobLink>();
			jobLink.UCL_ParentID = parentPK;
			jobLink.UCL_ParentTableCode = prefix;
			jobLink.UCL_SourceType = sourceType;
			jobLink.UCL_OH_Owner = isEmptyOwner || orgHeader == null ? ZGuid.Empty : orgHeader.PK;

			Factory.Save();
		}

		#region TestGetOutturnMessage

		public void TestGetNotificationForInstruction()
		{
			var service = new TransitUniversalService();

			var msgForTest = "test UNDG Class one";
			service.InstructionMapper.AddMessageFormatsForTesting(msgForTest);

			var failureReason = "Error log test,NotificationForInstruction error message test,test UNDG Class one,test end";
			var successMessage = "Instruction successfully updated related job.";
			var failureMessage = "Error: Instruction had an issue while processing ({0} - {1}). Check DEX logs for details.";
			var expectedReason = "\r\nThe reason is: " + msgForTest;

			var instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.ProcessedOK, failureReason);
			AssertEquals(typeof(InfoNotification), instructionMessage.GetType());
			AssertEquals(successMessage, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Warning, failureReason);
			AssertEquals(typeof(InfoNotification), instructionMessage.GetType());
			AssertEquals(successMessage, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Error, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Error, EDIMessageStatusList.Descriptions.Error) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Discarded, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Discarded, EDIMessageStatusList.Descriptions.Discarded) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Cancelled, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Cancelled, EDIMessageStatusList.Descriptions.Cancelled) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Withdrawn, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Withdrawn, EDIMessageStatusList.Descriptions.Withdrawn) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Failed, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Failed, EDIMessageStatusList.Descriptions.Failed) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Rejected, failureReason);
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Rejected, EDIMessageStatusList.Descriptions.Rejected) + expectedReason, instructionMessage.Message);

			instructionMessage = service.GetNotificationForInstruction(null, failureReason);
			AssertEquals(typeof(WarningNotification), instructionMessage.GetType());
			AssertEquals("Warning: Instruction has been queued to send. Check DEX logs for details.", instructionMessage.Message);

			var emptyReason = string.Empty;
			var message = service.GetNotificationForInstruction(EDIMessageStatusList.Codes.Discarded, emptyReason);
			AssertEquals(typeof(ErrorNotification), message.GetType());
			AssertEquals(string.Format(failureMessage, EDIMessageStatusList.Codes.Discarded, EDIMessageStatusList.Descriptions.Discarded), message.Message);
		}

		#endregion

		#region TestGetStopLoadMessage

		public void TestGetNotificationForStopLoadInstructionEvent()
		{
			TestGetNotificationForStopLoadInstructionEventCore(false);
		}

		public void TestGetNotificationForCancelStopLoadInstructionEvent()
		{
			TestGetNotificationForStopLoadInstructionEventCore(true);
		}

		void TestGetNotificationForStopLoadInstructionEventCore(bool isCancel)
		{
			var service = new TransitUniversalService();
			CombineAssertions(() =>
			{
				var instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.ProcessedOK, isCancel);
				AssertInfoNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Warning, isCancel);
				AssertInfoNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Error, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Discarded, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Cancelled, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Withdrawn, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Failed, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(EDIMessageStatusList.Codes.Rejected, isCancel);
				AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage, isCancel);

				instructionMessage = service.GetNotificationForStopLoadInstructionEvent(null, isCancel);
				AssertWarningNotification_GetNotificationForStopLoadInstructionEvent(instructionMessage);
			});
		}

		void AssertInfoNotification_GetNotificationForStopLoadInstructionEvent(INotification instructionMessage, bool isCancel)
		{
			AssertEquals(typeof(InfoNotification), instructionMessage.GetType());
			AssertEquals(isCancel ? "Successfully canceled Stop Load." : "Successfully stopped Load.", instructionMessage.Message);
		}

		void AssertErrorNotification_GetNotificationForStopLoadInstructionEvent(INotification instructionMessage, bool isCancel)
		{
			AssertEquals(typeof(ErrorNotification), instructionMessage.GetType());
			AssertEquals(isCancel ? "Error: Could not cancel Stop Load. Check DEX logs for details." : "Error: Could not stop Load. Check DEX logs for details.", instructionMessage.Message);
		}

		void AssertWarningNotification_GetNotificationForStopLoadInstructionEvent(INotification instructionMessage)
		{
			AssertEquals(typeof(WarningNotification), instructionMessage.GetType());
			AssertEquals("Warning: Instruction has been queued to send. Check DEX logs for details.", instructionMessage.Message);
		}

		#endregion
	}
}
