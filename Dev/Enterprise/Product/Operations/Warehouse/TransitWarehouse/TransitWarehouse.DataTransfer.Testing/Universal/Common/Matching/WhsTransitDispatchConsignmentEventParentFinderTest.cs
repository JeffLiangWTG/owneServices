using System;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class WhsTransitDispatchConsignmentEventParentFinderTest : WhsTransitConsignmentEventParentFinderTest
	{
		#region TestMRREvent_MatchesDispatchConsignment

		public void TestMRREvent_MatchesDispatchConsignment_MessageTypeIsCIN() => TestMRREvent_MatchesDispatchConsignment(CIN750NotificationConstants.CIN750MessageType);

		public void TestMRREvent_MatchesDispatchConsignment_MessageTypeIsNotCIN() => TestMRREvent_MatchesDispatchConsignment("AAA");

		void TestMRREvent_MatchesDispatchConsignment(string messageType)
		{
			var matchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, jobID: "DC00000001");

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "CIN750DeconsFromDCN";
			documentData.JDD_ParentID = matchingDispatchConsignment.PK;
			documentData.JDD_ParentTableCode = matchingDispatchConsignment.TablePrefix;

			var documentFromLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");
			var documentToLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");

			matchingDispatchConsignment.PopulateAddOnValue("CIN750DeconsHistoryMessageIdPair1", "STR", $"00000000-0000-0000-0000-000000000000|{documentFromLog.PK}");
			matchingDispatchConsignment.PopulateAddOnValue("CIN750DeconsHistoryMessageIdPair2", "STR", $"00000000-0000-0000-0000-000000000000|{documentToLog.PK}");
			TestMRJ_MRREvent_CreateCIN750Note(matchingDispatchConsignment);

			Factory.SaveForTesting();
			documentData.Logs.LoadRelatedElements();
			matchingDispatchConsignment.Logs.LoadRelatedElements();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageReceivedCode, referenceNumber: "DC00000001", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001", reason: "Test Reason", requestNumber: "00000000-0000-0000-0000-000000000000", documentName: "CIN Decons Notification (750)", messageType: messageType));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);

			if (messageType == CIN750NotificationConstants.CIN750MessageType)
			{
				AssertEquals("Should have only found 1 business object", 1, results.Length);
				AssertEquals("Should have no dcn", 0, results.Count(r => r is WhsItemDispatchConsignment));
				AssertEquals("Should have 1 document data", 1, results.Count(r => r is VisualizerDocumentData));

				var note = matchingDispatchConsignment.FindOrCreateCIN750StmNote();
				AssertEquals(@"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been sent successfully.
CIN 750 Notification:
    Message Type       Result     Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Deconsolidation    Succeed    HWB         HSB1        EDIDAT             Header               NOTCIN
From Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    AWB         MAB1        -
To Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    HWB         HSB1        -

", note.ST_NoteText);
			}
			else
			{
				AssertEquals("Should have found no business objects", 0, results.Length);
			}

			AssertEquals("Document From Log should not be cancelled", false, documentFromLog.IsCancelled);
			AssertEquals("Document To Log should not be cancelled", false, documentToLog.IsCancelled);

			var dcnFromLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();
			var dcnToLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

			AssertEquals("DCN From Log should not be cancelled", false, dcnFromLog.IsCancelled);
			AssertEquals("DCN To Log should not be cancelled", false, dcnToLog.IsCancelled);
		}

		#endregion

		#region TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs

		public void TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs_MessageTypeIsCRESA()
		{
			var matchingDispatchConsignment = Helper.CreateDispatchConsignment("MatchingDCN", Data.Warehouse.PK, "STD", "DC00000001");
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "Goods Received (CRESA)";
			documentData.JDD_ParentID = matchingDispatchConsignment.PK;
			documentData.JDD_ParentTableCode = matchingDispatchConsignment.TablePrefix;

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageRejectedCode,
				dataTargetName: "TransitDispatch",
				dataTargetKey: "DC00000001",
				reason: "Test Reason",
				documentName: "Goods Received (CRESA)",
				messageType: TransitWarehouseMessageTypes.CRESAMessageType));

			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);
			AssertEquals("Should have only 1 dcn", 1, results.Count(r => r is WhsItemDispatchConsignment));
		}

		public void TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs_MessageTypeIsCIN() => TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs(CIN750NotificationConstants.CIN750MessageType);

		public void TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs_MessageTypeIsNotCIN() => TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs("AAA");

		void TestMRJEvent_MatchesDispatchConsignmentAndProcessLogs(string messageType)
		{
			var matchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, jobID: "DC00000001");

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_Name = "CIN750DeconsFromDCN";
			documentData.JDD_ParentID = matchingDispatchConsignment.PK;
			documentData.JDD_ParentTableCode = matchingDispatchConsignment.TablePrefix;

			var documentFromLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");
			var documentToLog = Helper.CreateStmALog(documentData, CargoWise.Definitions.EventCodes.MessageSent, $"|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40");

			matchingDispatchConsignment.PopulateAddOnValue("CIN750DeconsHistoryMessageIdPair1", "STR", $"00000000-0000-0000-0000-000000000000|{documentFromLog.PK}");
			matchingDispatchConsignment.PopulateAddOnValue("CIN750DeconsHistoryMessageIdPair2", "STR", $"00000000-0000-0000-0000-000000000000|{documentToLog.PK}");
			TestMRJ_MRREvent_CreateCIN750Note(matchingDispatchConsignment);

			Factory.SaveForTesting();
			documentData.Logs.LoadRelatedElements();
			matchingDispatchConsignment.Logs.LoadRelatedElements();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageRejectedCode, referenceNumber: "DC00000001", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001", reason: "Test Reason", requestNumber: "00000000-0000-0000-0000-000000000000", documentName: "CIN Decons Notification (750)", messageType: messageType));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			AssertNotEquals("Output should not be null", null, results);

			if (messageType == CIN750NotificationConstants.CIN750MessageType)
			{
				AssertEquals("Should have only found 1 business object", 1, results.Length);
				AssertEquals("Should have no dcn", 0, results.Count(r => r is WhsItemDispatchConsignment));
				AssertEquals("Should have 1 document data", 1, results.Count(r => r is VisualizerDocumentData));
				AssertEquals("Document From Log should be cancelled", true, documentFromLog.IsCancelled);
				AssertEquals("Document To Log should be cancelled", true, documentToLog.IsCancelled);

				var dcnFromLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();
				var dcnToLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

				AssertEquals("DCN From Log should be cancelled", true, dcnFromLog.IsCancelled);
				AssertEquals("DCN To Log should be cancelled", true, dcnToLog.IsCancelled);

				var note = matchingDispatchConsignment.FindOrCreateCIN750StmNote();
				AssertEquals(@"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been rejected. Reason: Test Reason.
CIN 750 Notification:
    Message Type       Result     Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Deconsolidation    Succeed    HWB         HSB1        EDIDAT             Header               NOTCIN
From Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    AWB         MAB1        -
To Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    HWB         HSB1        -

", note.ST_NoteText);
			}
			else
			{
				AssertEquals("Should have found no business objects", 0, results.Length);
				AssertEquals("Document From Log should not be cancelled", false, documentFromLog.IsCancelled);
				AssertEquals("Document To Log should not be cancelled", false, documentToLog.IsCancelled);

				var dcnFromLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_From|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();
				var dcnToLog = matchingDispatchConsignment.Logs.Find(l => l.SL_Reference.EndsWith("|HBL=-|JOB=DC00000001|MBL=-|MST=CIN750DeconsNotification_To|OTY=5|PTP=REF|RFN=EDIDATDCN1|WGT=40")).Single();

				AssertEquals("DCN From Log should not be cancelled", false, dcnFromLog.IsCancelled);
				AssertEquals("DCN To Log should not be cancelled", false, dcnToLog.IsCancelled);
			}
		}

		#endregion

		#region TestSHLEvent

		protected override string MessageTypeForSHLEvent => "'Port Notification Export Status'";

		public void TestSHLEvent_MatchesDispatchConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			var nonmatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonmatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			var packageState = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN1SHLNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState.WPS_CustomsStatus);
			});
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SHLNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SHLNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedDCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState2.WPS_CustomsStatus);
			});
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2012_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Import Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			packageState1.Reload();
			packageState2.Reload();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business object", 0, results.Length);
			AssertEquals("Customs Status should not be updated", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState1.WPS_CustomsStatus);
			AssertEquals("Customs Status should not be updated", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState2.WPS_CustomsStatus);
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2012_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "", Facilities.Code.Depot, "CR123", "AUBNE", "Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SHLNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SHLNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedDCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState2.WPS_CustomsStatus);
			});
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SHLNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SHLNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedDCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState2.WPS_CustomsStatus);
			});
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2011_ForImport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Import Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			packageState1.Reload();
			packageState2.Reload();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 0 business object", 0, results.Length);
			AssertEquals("Customs Status should not be updated", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState1.WPS_CustomsStatus);
			AssertEquals("Customs Status should not be updated", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, packageState2.WPS_CustomsStatus);
		}

		public void TestSHLEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2011_ForExport()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.HeldCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|FAC=CFS|LOC=AUBNE|RES=clearance pending|MST=Port Notification Export Status"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SHLNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SHLNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertAdditionalReferece(selectedDCN2SHLNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", "");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState2.WPS_CustomsStatus);
			});
		}

		#endregion

		#region TestSCMEvent

		public void TestSCMEvent_MatchesDispatchConsignment_ByDataTarget()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);

			var packageState = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, eventReference: "|CRF=CR123|DEP=Customs|LOC=AUBNE", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 1 business objects", 1, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN1SCMNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.Cleared, packageState.WPS_CustomsStatus);
			});
		}
		public void TestSCMEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2011()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "|CRF=CR123|DEP=Customs|LOC=AUBNE"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SCMNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SCMNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedDCN2SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.Cleared, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.Cleared, packageState2.WPS_CustomsStatus);
			});
		}

		public void TestSCMEvent_MatchesDispatchConsignments_ByForwardingShipmentNumber_XMLVersion2012()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment1 = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			Helper.CreateAdditionalReference(matchingDispatchConsignment1, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var matchingDispatchConsignment2 = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			matchingDispatchConsignment2.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var nonMatchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000003", Data.Warehouse.PK, "STD", "DC00000003", transportMode: "SEA", direction: "EXP");
			nonMatchingDispatchConsignment.WDC_SystemCreateTimeUtc = DateTime.Now.AddDays(2);
			Helper.CreateAdditionalReference(matchingDispatchConsignment2, "SN321", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, TransitWarehouseReferenceCategories.Codes.AdditionalReference);

			var packageState1 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment1);
			var packageState2 = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment2);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.ClearanceCompletedCode, nameof(DataContextType.ForwardingShipment), "SN123", "Customs", "", "CR123", "AUBNE", ""));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have found 2 business objects", 2, results.Length);

			var selectedDCN1 = (WhsItemDispatchConsignment)results[0];
			var selectedDCN2 = (WhsItemDispatchConsignment)results[1];
			var selectedDCN1SCMNumber = selectedDCN1.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			var selectedDCN2SCMNumber = selectedDCN2.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState1.Reload();
			packageState2.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[1]);
				AssertNotContains(@"Warning - Universal event received could not be used for matching because of missing data. Correct them and try again.", logger.Logs);
				AssertContains(@"Information - Found Dispatch Consignments 'DC00000001, DC00000002' matching shipment number 'SN123'.
Information - Populating matching Dispatch Consignments.", logger.Logs);

				AssertEquals(1, selectedDCN1.PortReferences.Count);
				AssertEquals(1, selectedDCN2.PortReferences.Count);
				AssertAdditionalReferece(selectedDCN1SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertAdditionalReferece(selectedDCN2SCMNumber, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CR123", TransitWarehouseReferenceStatus.Codes.Cleared);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.Cleared, packageState1.WPS_CustomsStatus);
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.Cleared, packageState2.WPS_CustomsStatus);
			});
		}

		#endregion

		#region TestMAAEvent

		public void TestMAAEvent_MatchesDispatchConsignment_ByDataTarget_XMLVersion2012()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			var nonMatchingConsignment = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");
			var packageState = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "CRF123", referenceNumber: "RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedDCN = (WhsItemDispatchConsignment)results[0];
			var selectedDCNPENReference = selectedDCN.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertEquals("Selected the 'only' dispatch consignment where its consignment id matches the data target key.", matchingDispatchConsignment.PK, selectedDCN.PK);

				AssertEquals(1, selectedDCN.PortReferences.Count);
				AssertAdditionalReferece(selectedDCNPENReference, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CRF123", entryLineReference: "RFN123");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState.WPS_CustomsStatus);
			});
		}

		public void TestMAAEvent_MatchesDispatchConsignment_ByDataTarget_XMLVersion2011()
		{
			Data.Warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var rcn = Helper.CreateReceiveConsignment("RC00000001", "STD", Data.Warehouse.PK, "RC00000001");

			var matchingDispatchConsignment = Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");
			var nonMatchingConsignment = Helper.CreateDispatchConsignment("DC00000002", Data.Warehouse.PK, "STD", "DC00000002", transportMode: "SEA", direction: "EXP");

			var packageState = Helper.CreatePackageState(rcn, 10, PackageStateUnitType.Codes.Package, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: matchingDispatchConsignment);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();
			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 1 business object", 1, results.Length);

			var selectedDCN = (WhsItemDispatchConsignment)results[0];
			var selectedDCNPENReference = selectedDCN.PortReferences.First(TransitWarehousePortReferenceTypes.Codes.PortExport);
			packageState.Reload();
			CombineAssertions(() =>
			{
				AssertType<WhsItemDispatchConsignment>("Should be a Dispatch Consignment", results[0]);
				AssertEquals("Selected the 'only' dispatch consignment where its consignment id matches the data target key.", matchingDispatchConsignment.PK, selectedDCN.PK);

				AssertEquals(1, selectedDCN.PortReferences.Count);
				AssertAdditionalReferece(selectedDCNPENReference, TransitWarehouseReferenceCategories.Codes.PortReference, TransitWarehousePortReferenceTypes.Codes.PortExport, "CRF123", entryLineReference: "RFN123");
				AssertEquals("Customs Status has been updated", TransitWarehouseCustomsStatuses.Codes.NotClearedByPortAuthority, packageState.WPS_CustomsStatus);
			});
		}

		public void TestMAAEvent_NoMatchingDispatchConsignment_ByDataTarget_XMLVersion2011()
		{
			Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2011EventXMLWithEventReference(AutoEvents.MessageAcceptedCode, eventReference: "|CRF=CRF123|RFN=RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000002"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 0 business object", 0, results.Length);
		}

		public void TestMAAEvent_NoMatchingDispatchConsignment_ByDataTarget_XMLVersion2012()
		{
			Helper.CreateDispatchConsignment("DC00000001", Data.Warehouse.PK, "STD", "DC00000001", transportMode: "SEA", direction: "EXP");

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.Build2012EventXMLWithEventParameters(AutoEvents.MessageAcceptedCode, customsReferenceNumber: "CR123", referenceNumber: "RFN123", dataTargetName: "TransitDispatch", dataTargetKey: "DC00000002"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);

			AssertNotEquals("Output should not be null", null, results);
			AssertEquals("Should have only found 0 business object", 0, results.Length);
		}

		#endregion

		#region GetFinder 

		protected override EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new WhsTransitDispatchConsignmentEventParentFinder(Factory.BOFactory, new WhsTransitDispatchConsignmentDataContextManager(), logger ?? new TestErrorLogger());
		}

		#endregion

		#region Implementation

		void TestMRJ_MRREvent_CreateCIN750Note(WhsItemDispatchConsignment dcn)
		{
			var note = dcn.FindOrCreateCIN750StmNote();
			note.ST_NoteText = @"User: CargoWise Support
Time: 29-Apr-24 14:34:00 +00:00
Message Status: 00000000-0000-0000-0000-000000000000 has been sent and is waiting for response.
CIN 750 Notification:
    Message Type       Result     Ref Type    Ref Code    Enterprise Code    CFS/TWH Warehouse    CFS/TWH CIN Code
    Deconsolidation    Succeed    HWB         HSB1        EDIDAT             Header               NOTCIN
From Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    AWB         MAB1        -
To Goods Details:
    Quantity    Weight      Description         Ref Type    Ref Code    PNTS
    1           2.300 KG    Test Description    HWB         HSB1        -

";
		}

		#endregion
	}
}
