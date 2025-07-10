using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Common.IntegrationTests.ForwardingAndTransit
{
	public class ForwardingToTWGoverningEvents : IntegrationTestCaseWithFactory
	{
		#region Clearance Completed

		public void TestPortAuthorityUXMLEvent_SCM_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithMatchingShipmentNumber2 = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber2, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithMatchingShipmentNumber2.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ClearanceCompletedCode);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber2.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber2.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Receive Consignment is not closed.", false, receiveConsignmentWithMatchingShipmentNumber.WRC_CompleteTime.IsValid);
			AssertEquals("Receive Consignment is not closed.", false, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CompleteTime.IsValid);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.ClearanceCompleted, $"|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE");

			var factoryAfterPublishingSCMEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var nonMatchingConsignmentAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSCMEvent2 = factoryAfterPublishingSCMEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber2.PK);
			var nonMatchingPackageStateAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingConsignmentAfterPublishingSCMEvent.WRC_CustomsStatus);
				AssertEquals("Blind consignment with matching packages Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.Cleared, receiveConsignmentForBlindPackagesAfterPublishingSCMEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingSCMEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingPackageStateAfterPublishingSCMEvent.WPS_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingPackageStateAfterPublishingSCMEvent2.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingSCMEvent.WPS_CustomsStatus);
				AssertEquals("Receive Consignment is not closed.", false, matchingConsignmentAfterPublishingSCMEvent.WRC_CompleteTime.IsValid);
				AssertEquals("Receive Consignment is not closed.", false, nonMatchingConsignmentAfterPublishingSCMEvent.WRC_CompleteTime.IsValid);

				AssertEquals("SCM event is added to matching consignment.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingConsignmentAfterPublishingSCMEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingPackageStateAfterPublishingSCMEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingPackageStateAfterPublishingSCMEvent2.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingSCMEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SCM event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingSCMEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
					Factory,
					matchingConsignmentAfterPublishingSCMEvent.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					TransitWarehouseReferenceStatus.Codes.Cleared);

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingSCMEvent.Package.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					TransitWarehouseReferenceStatus.Codes.Cleared);

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingSCMEvent2.Package.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		public void TestPortAuthorityUXMLEvent_SCM_ShipmentAndConsol_PopulatesMatchingReceiveConsignments__WarehouseIsPortAuthorityControlled()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var rcnWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();

			var rcnWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(rcnWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ClearanceCompletedCode);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, rcnWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Completed event from shipment yet.", 0, rcnWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, rcnWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Receive Consignment is not closed.", false, rcnWithMatchingShipmentNumber.WRC_CompleteTime.IsValid);
			AssertEquals("Receive Consignment is not closed.", false, rcnWithNonMatchingShipmentNumber.WRC_CompleteTime.IsValid);

			warehouse.WW_IsPortAuthorityControlled = true;
			warehouse.WW_IsCustomsControlled = true;
			Helper.CreateGoverningReference(rcnWithMatchingShipmentNumber.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: CusEntryNumber.EntryType.CustomsNumber);
			Helper.CreateGoverningReference(rcnWithMatchingShipmentNumber.PK, WhsItemReceiveConsignmentSchema.Constants.TableName, entryType: CusEntryNumber.EntryType.CustomsReleaseNumber);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.ClearanceCompleted, $"|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE");

			var factoryAfterPublishingSCMEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingRCNAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemReceiveConsignment>(rcnWithMatchingShipmentNumber.PK);
			var nonMatchingRCNAfterPublishingSCMEvent = factoryAfterPublishingSCMEvent.Load<WhsItemReceiveConsignment>(rcnWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment is not cleared by Port Authority.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingRCNAfterPublishingSCMEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingRCNAfterPublishingSCMEvent.WRC_CustomsStatus);
				AssertEquals("Receive Consignment is closed.", true, matchingRCNAfterPublishingSCMEvent.WRC_CompleteTime.IsValid);
				AssertEquals("Receive Consignment is not closed.", false, nonMatchingRCNAfterPublishingSCMEvent.WRC_CompleteTime.IsValid);
				AssertEquals("SCM event is added to matching consignment.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingRCNAfterPublishingSCMEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is not added to non matching consignment.", 0, nonMatchingRCNAfterPublishingSCMEvent.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
					Factory,
					matchingRCNAfterPublishingSCMEvent.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					TransitWarehouseReferenceStatus.Codes.Cleared);
			});
		}

		#endregion

		#region Clearance Hold

		public void TestPortAuthorityUXMLEvent_SCH_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithMatchingShipmentNumber2 = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber2, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithMatchingShipmentNumber2.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.HeldCode);
			AssertEquals("No Clearance Hold event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Hold event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Hold event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Hold event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber2.Logs.Find(eventQuery).Length);
			AssertEquals("No Clearance Hold event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber2.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.Held, $"|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE");

			var factoryAfterPublishingSCHEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingSCHEvent = factoryAfterPublishingSCHEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingSCHEvent = factoryAfterPublishingSCHEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var nonMatchingConsignmentAfterPublishingSCHEvent = factoryAfterPublishingSCHEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSCHEvent = factoryAfterPublishingSCHEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSCHEvent2 = factoryAfterPublishingSCHEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber2.PK);

			var nonMatchingPackageStateAfterPublishingSCHEvent = factoryAfterPublishingSCHEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingConsignmentAfterPublishingSCHEvent.WRC_CustomsStatus);
				AssertEquals("Blind consignment with matching packages Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, receiveConsignmentForBlindPackagesAfterPublishingSCHEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingSCHEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingPackageStateAfterPublishingSCHEvent.WPS_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingPackageStateAfterPublishingSCHEvent2.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingSCHEvent.WPS_CustomsStatus);

				AssertEquals("SCH event is added to matching consignment.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingConsignmentAfterPublishingSCHEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCH event is added to matching package.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingPackageStateAfterPublishingSCHEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCH event is added to matching package.", "|CRF=CLRCOMP1|DEP=CUSTOMS|LOC=AUBNE", matchingPackageStateAfterPublishingSCHEvent2.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCH event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingSCHEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SCH event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingSCHEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
				Factory,
				matchingConsignmentAfterPublishingSCHEvent.PK,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority,
				TransitWarehouseReferenceCategories.Codes.PortReference,
				"CLRCOMP1",
				"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingSCHEvent.Package.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
				Factory,
				matchingPackageStateAfterPublishingSCHEvent2.Package.PK,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority,
				TransitWarehouseReferenceCategories.Codes.PortReference,
				"CLRCOMP1",
				"");
			});
		}

		public void TestPortAuthorityUXMLEvent_SHL_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates_ForImport()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.HeldCode);
			AssertEquals("No Held event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.Held, $"|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Import Status");

			var factoryAfterPublishingSHLEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var nonMatchingConsignmentAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var nonMatchingPackageStateAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingConsignmentAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Blind consignment with matching packages Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, receiveConsignmentForBlindPackagesAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingPackageStateAfterPublishingSHLEvent.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingSHLEvent.WPS_CustomsStatus);

				AssertEquals("SHL event is added to matching consignment.", "|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Import Status", matchingConsignmentAfterPublishingSHLEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SHL event is added to matching package.", "|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Import Status", matchingPackageStateAfterPublishingSHLEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SHL event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingSHLEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SHL event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingSHLEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
				Factory,
				matchingConsignmentAfterPublishingSHLEvent.PK,
				TransitWarehousePortReferenceTypes.Codes.PortAuthority,
				TransitWarehouseReferenceCategories.Codes.PortReference,
				"CLRCOMP1",
				"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingSHLEvent.Package.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					"");
			});
		}

		public void TestPortAuthorityUXMLEvent_SHL_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates_ForExport()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.HeldCode);
			AssertEquals("No Held event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No Held event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.Held, $"|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Export Status");

			var factoryAfterPublishingSHLEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var nonMatchingConsignmentAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var nonMatchingPackageStateAfterPublishingSHLEvent = factoryAfterPublishingSHLEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingConsignmentAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Blind consignment with matching packages Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, receiveConsignmentForBlindPackagesAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingSHLEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, matchingPackageStateAfterPublishingSHLEvent.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingSHLEvent.WPS_CustomsStatus);

				AssertEquals("SHL event is added to matching consignment.", "|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Export Status", matchingConsignmentAfterPublishingSHLEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SHL event is added to matching package.", "|CRF=CLRCOMP1|FAC=CFS|LOC=AUBNE|MST=Port Notification Export Status", matchingPackageStateAfterPublishingSHLEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SHL event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingSHLEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SHL event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingSHLEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
					Factory,
					matchingConsignmentAfterPublishingSHLEvent.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingSHLEvent.Package.PK,
					TransitWarehousePortReferenceTypes.Codes.PortAuthority,
					TransitWarehouseReferenceCategories.Codes.PortReference,
					"CLRCOMP1",
					"");
			});
		}

		#endregion

		#region Customs Number Entered

		public void TestCustomsUXMLEvent_CEN_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithMatchingShipmentNumber2 = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber2, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithMatchingShipmentNumber2.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsNumberEnteredCode);
			AssertEquals("No CEN event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CEN event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CEN event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CEN event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber2.Logs.Find(eventQuery).Length);
			AssertEquals("No CEN event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber2.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.CustomsNumberEntered, $"|CRF=CLRCOMP1|LOC=AUBNE");

			var factoryAfterPublishingCENEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingCENEvent = factoryAfterPublishingCENEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var nonMatchingConsignmentAfterPublishingCENEvent = factoryAfterPublishingCENEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingCENEvent = factoryAfterPublishingCENEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var matchingPackageStateAfterPublishingCENEvent = factoryAfterPublishingCENEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingCENEvent2 = factoryAfterPublishingCENEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber2.PK);
			var nonMatchingPackageStateAfterPublishingCENEvent = factoryAfterPublishingCENEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, matchingConsignmentAfterPublishingCENEvent.WRC_CustomsStatus);
				AssertEquals("Consignment for matching packages Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, receiveConsignmentForBlindPackagesAfterPublishingCENEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingCENEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, matchingPackageStateAfterPublishingCENEvent.WPS_CustomsStatus);
				AssertEquals("Matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NotClearedByCustoms, matchingPackageStateAfterPublishingCENEvent2.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingCENEvent.WPS_CustomsStatus);

				AssertEquals("SCM event is added to matching consignment.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingConsignmentAfterPublishingCENEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingPackageStateAfterPublishingCENEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingPackageStateAfterPublishingCENEvent2.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingCENEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SCM event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingCENEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
					Factory,
					matchingConsignmentAfterPublishingCENEvent.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingCENEvent.Package.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingCENEvent2.Package.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");
			});
		}

		#endregion

		#region Customs Release Number Entered

		public void TestCustomsUXMLEvent_CRN_ShipmentAndConsol_PopulatesMatchingReceiveConsignmentsAndPackageStates()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(arrivalWarehouse: false);

			var consol = CreateConsol("MSB1", vessel, "NZCHC", "AUADL");
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var container = CreateContainer(consol, "CONT1", 1, "20GP");

			CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder();
			var shipment = CreateShipment(consol, "HSB1", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor, consignee);
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;
			var packline = CreateOuterPackline(shipment, 1, Constants.PkgUnit.Pallet, container);

			// Set warehouse port code for matching
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			Factory.Save();

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var receiveConsignmentWithMatchingShipmentNumber = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).Single();
			AssertEquals("Precondition", 1, receiveConsignmentWithMatchingShipmentNumber.PackageStates.Count);
			var packageStateForPallet = receiveConsignmentWithMatchingShipmentNumber.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveConsignmentWithNonMatchingShipmentNumber = Helper.CreateReceiveConsignment("Non-matching", warehouse.PK);
			Helper.CreateAdditionalReference(receiveConsignmentWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			// Blind packages need to be manually created
			var receiveConsignmentForBlindPackages = Helper.CreateReceiveConsignment("RCNBlind", warehouse.PK);
			var blindackageStateWithMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithMatchingShipmentNumber2 = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithMatchingShipmentNumber2, shipment.JobNumber, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var blindackageStateWithNonMatchingShipmentNumber = Helper.CreatePackageState(receiveConsignmentForBlindPackages, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, location: warehouse.DefaultInboundDockDoorLocation);
			Helper.CreateAdditionalReference(blindackageStateWithNonMatchingShipmentNumber, "Non-matching Number", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			// Set warehouse port code for matching
			blindackageStateWithMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithMatchingShipmentNumber2.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			blindackageStateWithNonMatchingShipmentNumber.LastLocation.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();

			var eventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsReleaseNumberEnteredCode);
			AssertEquals("No CRN event from shipment yet.", 0, receiveConsignmentWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CRN event from shipment yet.", 0, receiveConsignmentWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CRN event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("No CRN event from shipment yet.", 0, blindackageStateWithMatchingShipmentNumber2.Logs.Find(eventQuery).Length);
			AssertEquals("No CRN event from shipment yet.", 0, blindackageStateWithNonMatchingShipmentNumber.Logs.Find(eventQuery).Length);
			AssertEquals("Customs Status is NON", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentWithNonMatchingShipmentNumber.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, receiveConsignmentForBlindPackages.WRC_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithMatchingShipmentNumber2.WPS_CustomsStatus);
			AssertEquals("Customs Status hasn't been populated yet.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, blindackageStateWithNonMatchingShipmentNumber.WPS_CustomsStatus);

			TriggerAndFireUniversalEventFromShipment(shipment, Events.CustomsReleaseNumberEntered, $"|CRF=CLRCOMP1|LOC=AUBNE");

			var factoryAfterPublishingCRNEvent = new BusinessObjectFactory() { RefreshEnabled = false };
			var matchingConsignmentAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithMatchingShipmentNumber.PK);
			var receiveConsignmentForBlindPackagesAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentForBlindPackages.PK);
			var nonMatchingConsignmentAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemReceiveConsignment>(receiveConsignmentWithNonMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber.PK);
			var matchingPackageStateAfterPublishingCRNEvent2 = factoryAfterPublishingCRNEvent.Load<WhsItemPackageState>(blindackageStateWithMatchingShipmentNumber2.PK);
			var nonMatchingPackageStateAfterPublishingCRNEvent = factoryAfterPublishingCRNEvent.Load<WhsItemPackageState>(blindackageStateWithNonMatchingShipmentNumber.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Matching consignment Customs Status should be Cleared", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingConsignmentAfterPublishingCRNEvent.WRC_CustomsStatus);
				AssertEquals("Blind Consignment for matching Packages Customs Status should be Cleared.", TransitWarehouseCustomsStatuses.Codes.Cleared, receiveConsignmentForBlindPackagesAfterPublishingCRNEvent.WRC_CustomsStatus);
				AssertEquals("Non matching consignment Customs Status has not been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingConsignmentAfterPublishingCRNEvent.WRC_CustomsStatus);
				AssertEquals("Matching package state Customs Status should be Cleared", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingPackageStateAfterPublishingCRNEvent.WPS_CustomsStatus);
				AssertEquals("Matching package state Customs Status should be Cleared", TransitWarehouseCustomsStatuses.Codes.Cleared, matchingPackageStateAfterPublishingCRNEvent2.WPS_CustomsStatus);
				AssertEquals("Non matching package state Customs Status has been recalculated.", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, nonMatchingPackageStateAfterPublishingCRNEvent.WPS_CustomsStatus);

				AssertEquals("SCM event is added to matching consignment.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingConsignmentAfterPublishingCRNEvent.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingPackageStateAfterPublishingCRNEvent.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is added to matching package.", "|CRF=CLRCOMP1|LOC=AUBNE", matchingPackageStateAfterPublishingCRNEvent2.Package.Logs.Find(eventQuery).Single().SL_Reference);
				AssertEquals("SCM event is not added to non matching consignment.", 0, nonMatchingConsignmentAfterPublishingCRNEvent.Logs.Find(eventQuery).Length);
				AssertEquals("SCM event is not added to non matching package.", 0, nonMatchingPackageStateAfterPublishingCRNEvent.Package.Logs.Find(eventQuery).Length);

				AssertEntryNumber(
					Factory,
					matchingConsignmentAfterPublishingCRNEvent.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingCRNEvent.Package.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");

				AssertEntryNumber(
					Factory,
					matchingPackageStateAfterPublishingCRNEvent2.Package.PK,
					TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber,
					TransitWarehouseReferenceCategories.Codes.CustomsReference,
					"CLRCOMP1",
					"");
			});
		}

		#endregion

		#region Assertion

		static void AssertEntryNumber(BusinessObjectFactory factory, ZGuid parentPK, string entryType, string category, string entryNum, string entryStatus)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_Category, category);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			query.AddToFilter(CusEntryNumSchema.CE_ParentID, parentPK);
			var additionalReference = factory.Load<CusEntryNumber>(query).Single();
			AssertEquals(entryNum, additionalReference.CE_EntryNum);
			AssertEquals(entryStatus, additionalReference.CE_EntryStatus);
		}

		#endregion
	}
}
