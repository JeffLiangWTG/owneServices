using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveConsignmentDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitReceive,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region Integration Tests

		#region Test_EventExport_Writes_XML_WithoutError

		public void Test_EventExport_Writes_XML_WithoutError()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);

			var trigger = Helper.GetEventTrigger(receiveConsignment,
				Core.Constants.Workflow.WorkflowTriggerType,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"Export UXML From RCN");

			var notification = Helper.GetEventNotification(trigger,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				MessageRecipientPartyTypeList.Codes.OrgProxy);
			Factory.SaveForTesting();

			using (var tempDirectory = new TempDirectory())
			{
				var rcnReference = receiveConsignment[WhsItemReceiveConsignmentSchema.WRC_JobID].ToString();

				var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(notification, receiveConsignment);
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", $"UniversalShipment from [{rcnReference}] saved", exportResult.Message);
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var uxmlFileName = messageLines[1];
					AssertEquals($"File.Exists(\"{uxmlFileName}\")", true, File.Exists(uxmlFileName));
					AssertContains("fileName", tempDirectory.DirectoryName, uxmlFileName);
					AssertEquals("fileName.EndsWith(\".xml\") failed on: " + uxmlFileName, true, uxmlFileName.EndsWith(".xml"));
					var uxmlText = File.ReadAllText(uxmlFileName);
					AssertContains("Trigger Count is never zero even for samples.", "<TriggerCount>1</TriggerCount>", uxmlText);
				});
			}
		}

		#endregion

		#region Test_Import_ForwardingShipment_With_New_Consignment

		public void Test_Import_ForwardingShipment_With_New_Consignment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var pickup = data.Org4;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", warehouse.PK, bookedByParty, consignor, consignee, serviceLevel: "STD", pickup: pickup);
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PACKAGEID", TransitWarehouseStatuses.Codes.Booked);
			receiveConsignment.WRC_HouseBillNumber = "REF";
			var note = receiveConsignment.Notes.AddNew(true, "Custom Description", "Test Note");
			note.ST_NoteType = StmNoteDescription.Pub;
			var routing = (BusinessObject)Helper.CreateTransportRouting(receiveConsignment, "ROUTING", "AUBNE", "NZAKL");
			var service = receiveConsignment.Services.AddNew();
			service.ES_ServiceCode = "S1";
			var houseBill = receiveConsignment.WRC_HouseBillNumber;
			AssertEquals("Precondition", "REF", houseBill);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			//delete business objects to create new ones
			receiveConsignment.Delete();
			packageState.Package.Delete();
			packageState.Delete();
			note.Delete();
			routing.Delete();
			service.Delete();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				AssertNull(Factory.BOFactory.LoadTop1<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "RCN")));
				AssertNull(Factory.BOFactory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_Status, TransitWarehouseStatuses.Codes.Booked)));
				AssertNull(Factory.BOFactory.LoadTop1<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_EntryNum, "REF")));
				AssertNull(Factory.BOFactory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_Table, "Custom Description")));
				AssertNull(Factory.BOFactory.LoadTop1<ITransport>(new ZQuery(JobConsolTransportSchema.JW_VoyageFlight, "ROUTING")));
				AssertNull(Factory.BOFactory.LoadTop1<JobService>(new ZQuery(JobServiceSchema.ES_ServiceCode, "S1")));
			});

			//process EDI message to import UXML
			SetupDataContextWithTargetForImport(consignmentDataObject, DataContextType.TransitReceive, "", ServiceCodeType.TWR);
			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(consignmentDataObjectEDIMessage);

			var newFactory = new UniversalObjectFactory();
			var newRCN = newFactory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "REF")).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Receive Consignment RC00000001 from UniversalShipment.
Successfully saved Receive Consignment RC00000001 with 1 x Transport, 1 x PkgPackageJob, 1 x PkgPackage, 1 x WhsItemPackageState, 1 x StmNote.".Trim(), serviceTaskLog.ToString());
				AssertEquals("STD", newRCN.WRC_RS_NKServiceLevel);
				AssertEquals(TransitWarehouseStatuses.Codes.Booked, newRCN.PackageStates.Single().WPS_Status);
				AssertEquals("PKG", newRCN.PackageStates.Single().Package.KP_F3_NKPackType);
				AssertEquals("PACKAGEID", newRCN.PackageStates.Single().Package.GetPackageHeader().KPH_PackageID);
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, newRCN.BookingPartyDocAddress.OrganisationPK);
				AssertEquals(consignor.PK, newRCN.ConsignorDocAddress.OrganisationPK);
				AssertEquals(consignee.PK, newRCN.ConsigneeDocAddress.OrganisationPK);
				AssertEquals(pickup.PK, newRCN.ConsignorPickupDeliveryAddress.OrganisationPK);
				AssertEquals("ROUTING", newRCN.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, newRCN.PK)).SingleOrDefault().JW_VoyageFlight);
			});
		}

		#endregion

		#region Test_Import_ForwardingShipment_Updates_Existing_Consignment

		public void Test_Import_ForwardingShipment_Updates_Existing_Consignment()
		{
			var receiveConsignment = SetupReceiveConsignmentWithTestDataForTesting();
			var houseBill = receiveConsignment.HouseBillNumber;
			AssertEquals("Precondition:", "REF", houseBill);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);
			consignmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_WUFSHIJNB });
			SetupDataContextWithTargetForImport(consignmentDataObject, DataContextType.TransitReceive, "RC00000001", ServiceCodeType.TWR);
			Factory.SaveForTesting();

			consignmentDataObject.PackingLineCollection.First().PackType.Code = "MOD";
			consignmentDataObject.ServiceLevel = new ServiceLevel() { Code = "MOD" };
			consignmentDataObject.LocalProcessing.AdditionalServiceCollection.First().ServiceCode.Code = "MODIFIED";
			consignmentDataObject.AdditionalReferenceCollection.First().ReferenceNumber = "MODIFIED";
			consignmentDataObject.NoteCollection.First().Description = "MODIFIED";
			consignmentDataObject.TransportLegCollection.First().VoyageFlightNo = "MODIFIED";

			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(consignmentDataObjectEDIMessage);

			var newFactory = new UniversalObjectFactory();
			var modifiedRCN = newFactory.BOFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "REF")).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Updated Receive Consignment RC00000001 from UniversalShipment.
Successfully saved Receive Consignment RC00000001 with 1 x Transport, 1 x PkgPackageJob, 1 x PkgPackage, 1 x WhsItemPackageState, 1 x StmNote.".Trim(), serviceTaskLog.ToString());
				AssertEquals("MOD", modifiedRCN.WRC_RS_NKServiceLevel);
				AssertEquals("MOD", modifiedRCN.PackageStates.Single().Package.KP_F3_NKPackType);
				AssertEquals("MODIFIED", modifiedRCN.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, modifiedRCN.PK)).SingleOrDefault().JW_VoyageFlight);
			});
		}

		#endregion

		#region Test_Export_To_Forwarding_Creates_Shipment

		public void Test_Export_To_Forwarding_Creates_Shipment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", warehouse.PK, bookedByParty, consignor, consignee, serviceLevel: "STD");
			var jobId = receiveConsignment.WRC_JobID;
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			packageState.Package.BookedDimensions.KPB_PackageQty = 0;
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked);
			var note = receiveConsignment.Notes.AddNew(true, "Custom Description", "Test Note");
			var routing = (BusinessObject)Helper.CreateTransportRouting(receiveConsignment, "ROUTING", "AUBNE", "NZAKL");
			var service = receiveConsignment.Services.AddNew();
			service.ES_ServiceCode = "S1";

			// relabel PKG2
			packageState2.Package.KP_PreviousPackageID = "PKG2";

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertNull("Should not write previous Package Id to AddInfoCollection if empty.",
				consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection);
			AssertEquals("Should write previous Package Id to AddInfoCollection if not empty.",
				"PKG2", consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.PreviousPackageID)).Value);
			AssertEquals("Should write manifested package to AddInfoCollection if booked qty is not 0.",
				true.ToString(), consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.IsManifestedPackage)).Value);

			SetupDataContextWithTargetRecipientForExport(consignmentDataObject, RecipientRoleType.FOR);
			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			//process UXML to create forwarding shipment
			manager.Process(consignmentDataObjectEDIMessage);

			var newFactory = new UniversalObjectFactory();
			var newJobShipment = newFactory.BOFactory.Load<CommonShipment>(new ZQuery()).Single();
			var shipmentCusEntryNum = Factory.BOFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_EntryNum, jobId)).Single(c => c.CE_ParentID == newJobShipment.PK);
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Shipment from UniversalShipment.
Successfully saved Shipment S00001000 with 1 x ForwardingShipmentStmNote, 1 x Transport, 2 x ForwardingPackLine, 1 x CusEntryNumber, 1 x JobService.".Trim(), serviceTaskLog.ToString());
				AssertEquals(2, newJobShipment.OuterPackLines.Count);
				AssertEquals(consignor.PK, newJobShipment.ConsignorPK);
				AssertEquals(consignee.PK, newJobShipment.ConsigneePK);
				AssertEquals(newJobShipment.PK, shipmentCusEntryNum.CE_ParentID);
				AssertEquals("TWR", shipmentCusEntryNum.CE_EntryType);
				AssertEquals("STD", newJobShipment.JS_RS_NKServiceLevel);
				AssertEquals("Custom Description", newJobShipment.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault(n => n.ST_NoteText == "Test Note").ST_Description);
				AssertEquals("ROUTING", newJobShipment.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, newJobShipment.PK)).SingleOrDefault().JW_VoyageFlight);
			});
		}

		#endregion

		#region Test_Export_To_Forwarding_Updates_Shipment

		public void Test_Export_To_Forwarding_Updates_Shipment()
		{
			var receiveConsignment = SetupReceiveConsignmentWithTestDataForTesting();
			var houseBill = receiveConsignment.HouseBillNumber;
			AssertEquals("Precondition", "REF", houseBill);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ATW, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);
			SetupDataContextWithTargetRecipientForExport(consignmentDataObject, RecipientRoleType.FOR);
			Factory.SaveForTesting();

			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			//process UXML to create new forwarding shipment
			manager.Process(consignmentDataObjectEDIMessage);
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Shipment (House Bill='REF') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='REF') with 1 x ForwardingShipmentStmNote, 1 x Transport, 1 x ForwardingPackLine, 1 x CusEntryNumber, 1 x JobService.".Trim(), serviceTaskLog.ToString());
			});

			//Modify records and resend message to update shipment
			consignmentDataObject.PackingLineCollection.First().PackType.Code = "MOD";
			consignmentDataObject.ServiceLevel = new ServiceLevel() { Code = "MOD" };
			consignmentDataObject.LocalProcessing.AdditionalServiceCollection.First().ServiceCode.Code = "MODIFIED";
			consignmentDataObject.LocalProcessing.AdditionalServiceCollection.Content = CollectionContent.Complete;
			consignmentDataObject.AdditionalReferenceCollection.First().ReferenceNumber = "MODIFIED";
			consignmentDataObject.NoteCollection.Single(n => n.NoteText.Equals("Test Note")).Description = "MODIFIED";
			consignmentDataObject.TransportLegCollection.First().VoyageFlightNo = "MODIFIED";
			consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			serviceTaskLog.ClearLogs();
			manager.Process(consignmentDataObjectEDIMessage);

			var newFactory = new UniversalObjectFactory();
			var newJobShipment = newFactory.BOFactory.Load<CommonShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001000")).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Updated Shipment S00001000 (House Bill='REF') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='REF') with 1 x ForwardingShipmentStmNote, 1 x Transport, 1 x ForwardingPackLine, 1 x CusEntryNumber, 1 x JobService.".Trim(), serviceTaskLog.ToString());
				AssertEquals("MOD", newJobShipment.JS_RS_NKServiceLevel);
				AssertNotNull(newJobShipment.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault(n => n.ST_Description == "MODIFIED"));
				AssertEquals("MODIFIED", newJobShipment.Factory.Load<ITransport>(new ZQuery(JobConsolTransportSchema.JW_ParentGUID, newJobShipment.PK)).SingleOrDefault().JW_VoyageFlight);
				AssertEquals("MOD", ((JobService)newJobShipment.DocsAndCartage.Services.Single()).ES_ServiceCode);
			});
		}

		#endregion

		#region Test_Export_To_Forwarding_Sets_PackageScreeningResult_FailedResult

		public void Test_Export_To_Forwarding_Sets_PackageScreeningResult_FailedResult()
		{
			var receiveConsignment = SetupReceiveConsignmentWithTestDataForTesting();
			var houseBill = receiveConsignment.HouseBillNumber;
			AssertEquals("Precondition", "REF", houseBill);

			var package1 = receiveConsignment.PackageStates.Single();
			var package2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PACKAGE2ID", TransitWarehouseStatuses.Codes.Booked);
			AssertEquals("Package1 does not have any screening", 0, package1.Package.Screenings.Count);
			AssertEquals("Package2 does not have any screening", 0, package2.Package.Screenings.Count);

			var package1Screening = Helper.CreatePackageScreening(package1.Package, "XRY");
			var package2Screening = Helper.CreatePackageScreening(package2.Package, "XRY", true);
			AssertEquals("Package1 has 1 screening", 1, package1.Package.Screenings.Count);
			AssertEquals("Package2 has 1 screening", 1, package2.Package.Screenings.Count);
			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("Universal Shipment Screening Status should be Unknown", ScreeningStatusesList.Codes.Unknown, consignmentDataObject.ScreeningStatus.Code);

			consignmentDataObject.TransportMode = new CodeDescriptionPair() { Code = "AIR", Description = "Air Freight" };
			SetupDataContextWithTargetRecipientForExport(consignmentDataObject, RecipientRoleType.FOR);
			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			//process UXML to create new forwarding shipment
			manager.Process(consignmentDataObjectEDIMessage);
			var newFactory = new UniversalObjectFactory();
			var newJobShipment = newFactory.BOFactory.Load<CommonShipment>(new ZQuery()).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Shipment (House Bill='REF') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='REF') with 1 x ForwardingShipmentStmNote, 1 x Transport, 2 x ForwardingPackLine, 2 x CusEntryNumber, 1 x JobService.".Trim(), serviceTaskLog.ToString());
				// waiting on Forwarding to read ScreeningStatus from Universal Shipment
				//AssertEquals("Forwarding Shipment Screening Status should be Unknown", "UNK", newJobShipment.JS_ScreeningStatus);
				AssertEquals("Forwarding Shipment Inspection Type should be empty", "UNK", newJobShipment.JS_InspectionTypeCode);
			});
		}

		#endregion

		#region Test_Export_To_Forwarding_Sets_PackageScreeningResult_PassedResult

		public void Test_Export_To_Forwarding_Sets_PackageScreeningResult_PassedResult()
		{
			var receiveConsignment = SetupReceiveConsignmentWithTestDataForTesting();
			var houseBill = receiveConsignment.HouseBillNumber;
			AssertEquals("Precondition", "REF", houseBill);

			var package1 = receiveConsignment.PackageStates.Single();
			var package2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PACKAGE2ID", TransitWarehouseStatuses.Codes.Booked);
			AssertEquals("Package1 does not have any screening", 0, package1.Package.Screenings.Count);
			AssertEquals("Package2 does not have any screening", 0, package2.Package.Screenings.Count);

			var package1Screening = Helper.CreatePackageScreening(package1.Package, "XRY", true);
			var package2Screening = Helper.CreatePackageScreening(package2.Package, "XRY", true);
			AssertEquals("Package1 has 1 screening", 1, package1.Package.Screenings.Count);
			AssertEquals("Package2 has 1 screening", 1, package2.Package.Screenings.Count);
			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("Universal Shipment Screening Status should be Clear", ScreeningStatusesList.Codes.Clear, consignmentDataObject.ScreeningStatus.Code);
			AssertNull("Universal Shipment Inspection Type should be null", consignmentDataObject.AviationSecurityInspectionType);

			consignmentDataObject.TransportMode = new CodeDescriptionPair() { Code = "AIR", Description = "Air Freight" };
			SetupDataContextWithTargetRecipientForExport(consignmentDataObject, RecipientRoleType.FOR);
			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			//process UXML to create new forwarding shipment
			manager.Process(consignmentDataObjectEDIMessage);
			var newFactory = new UniversalObjectFactory();
			var newJobShipment = newFactory.BOFactory.Load<CommonShipment>(new ZQuery()).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Shipment (House Bill='REF') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='REF') with 1 x ForwardingShipmentStmNote, 1 x Transport, 3 x CusEntryNumber, 2 x ForwardingPackLine, 1 x JobService.".Trim(), serviceTaskLog.ToString());
				// waiting on Forwarding to read ScreeningStatus from Universal Shipment
				//AssertEquals("Forwarding Shipment Screening Status should be Clear", "CLR", newJobShipment.JS_ScreeningStatus);
				AssertEquals("Forwarding Shipment Inspection Type should be empty", "UNK", newJobShipment.JS_InspectionTypeCode);
			});
		}

		#endregion

		#region Test_Export_To_Forwarding_Sets_PackageScreeningResult_NotScreened

		public void Test_Export_To_Forwarding_Sets_PackageScreeningResult_NotScreened()
		{
			var receiveConsignment = SetupReceiveConsignmentWithTestDataForTesting();
			var houseBill = receiveConsignment.HouseBillNumber;
			AssertEquals("Precondition", "REF", houseBill);

			var package1 = receiveConsignment.PackageStates.Single();
			var package2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PACKAGE2ID", TransitWarehouseStatuses.Codes.Booked);
			AssertEquals("Package1 does not have any screening", 0, package1.Package.Screenings.Count);
			AssertEquals("Package2 does not have any screening", 0, package2.Package.Screenings.Count);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("Universal Shipment Screening Status should be Not Screened", ScreeningStatusesList.Codes.NotScreened, consignmentDataObject.ScreeningStatus.Code);
			AssertNull("Universal Shipment Inspection Type should be null", consignmentDataObject.AviationSecurityInspectionType);

			consignmentDataObject.TransportMode = new CodeDescriptionPair() { Code = "AIR", Description = "Air Freight" };
			SetupDataContextWithTargetRecipientForExport(consignmentDataObject, RecipientRoleType.FOR);
			var consignmentDataObjectEDIMessage = GetQueuedUniversalShipmentMessage(consignmentDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			//process UXML to create new forwarding shipment
			manager.Process(consignmentDataObjectEDIMessage);
			var newFactory = new UniversalObjectFactory();
			var newJobShipment = newFactory.BOFactory.Load<CommonShipment>(new ZQuery()).Single();
			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Service Task Log", @"Added Shipment (House Bill='REF') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='REF') with 1 x ForwardingShipmentStmNote, 1 x Transport, 2 x ForwardingPackLine, 1 x CusEntryNumber, 1 x JobService.".Trim(), serviceTaskLog.ToString());
				// waiting on Forwarding to read ScreeningStatus from Universal Shipment
				//AssertEquals("Forwarding Shipment Screening Status should be Not Screened", "NOT", newJobShipment.JS_ScreeningStatus);
				AssertEquals("Forwarding Shipment Inspection Type should be empty", "UNK", newJobShipment.JS_InspectionTypeCode);
			});
		}

		#endregion

		#region TestExportReceiveConsignmentWithPortReferences

		public void TestExportReceiveConsignmentWithPortReferences_OnlyHasPEN() => TestExportReceiveConsignmentWithPortReferencesCore(true, false);

		public void TestExportReceiveConsignmentWithPortReferences_OnlyHasPAN() => TestExportReceiveConsignmentWithPortReferencesCore(false, true);

		public void TestExportReceiveConsignmentWithPortReferences_HasPANAndPEN() => TestExportReceiveConsignmentWithPortReferencesCore(true, true);

		void TestExportReceiveConsignmentWithPortReferencesCore(bool hasPEN, bool hasPAN)
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			if (hasPEN)
			{
				Helper.CreateCustomsAdditionalReference(receiveConsignment, TransitWarehousePortReferenceTypes.Codes.PortExport, "BBE001", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			}
			if (hasPAN)
			{
				Helper.CreateCustomsAdditionalReference(receiveConsignment, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "BBE002", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			}

			var expectedPortReferenceNumber = hasPEN ? "BBE001" : "BBE002";
			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var references = consignmentDataObject.PortReferenceCollection;
			if (hasPAN || hasPEN)
			{
				CombineAssertions(() =>
				{
					if (hasPAN && hasPEN)
					{
						AssertNull("Should not poplulate old PAN if PEN exists", references.FirstOrDefault(r => r.Reference.Value == "BBE002"));
					}

					var panReference = references.First(r => r.Type.Code.Value == TransitWarehousePortReferenceTypes.Codes.PortAuthority);
					Helper.AssertPortReference(panReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, expectedPortReferenceNumber, "CLS", "AU");

					var rcnPortReferences = receiveConsignment.PortReferences.Cast<CusEntryNumber>().ToList();
					if (hasPEN)
					{
						AssertEquals(1, rcnPortReferences.Count(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport));
						AssertEquals("Should not modify the existing PEN", "BBE001", rcnPortReferences.Single(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport).CE_EntryNum);
					}
					if (hasPAN)
					{
						AssertEquals(1, rcnPortReferences.Count(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority));
						AssertEquals("Should not modify the existing PEN", "BBE002", rcnPortReferences.Single(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority).CE_EntryNum);
					}
				});
			}
		}

		#endregion

		#endregion

		#region TestPopulateDataObject_RTUConstructor

		public void TestPopulateDataObject_RTUConstructor()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var receiveTransportationUnit = SetupReceiveTransportationUnitForTesting("Good Vehicle");
			var receiveTransportationUnit2 = SetupReceiveTransportationUnitForTesting("Not As Good");

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreatePackageState(receiveTransportationUnit, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)), receiveTransportationUnit, container: null);
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertContainsExactElementsInAnyOrder("Should write all packages on the RCN", new string[] { "PKG1", "PKG2", "PKG3" }, consignmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.ToString()));
		}

		#endregion

		#region TestPopulateDataObject_ASNConstructor

		public void TestPopulateDataObject_ASNConstructor()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var receiveASN = Helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var receiveASN2 = Helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, receiveASN: receiveASN);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: receiveASN2);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)), receiveASN);
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertContainsExactElementsInAnyOrder("Should only write packages on the supplied ASN", new string[] { "PKG1" }, consignmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.ToString()));
		}

		#endregion

		#region PopulateDataObject Tests

		#region TestPopulateDataObject_ReceiveConsignment_PackagesAndServiceLevel

		public void TestPopulateDataObject_ReceiveConsignment_PackagesAndServiceLevel()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			CombineAssertions(() =>
			{
				Assert(consignmentDataObject.ServiceLevel.Code.Equals("STD"));
				AssertContainsExactElementsInAnyOrder(new[] { "PKG1", "PKG2" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			});
		}

		public void TestPopulateDataObject_ReceiveConsignment_NestedOverpackHandlingUnitsAndInners()
		{
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var topLevelOverpackPackageState = Helper.CreateOverpackPackage("OuterOverpack", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var onceNestedOverpackPackageState = Helper.CreateOverpackPackage("firstInnerOverpack", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var twiceNestedsOverpackPackageState = Helper.CreateOverpackPackage("secondInnerOverpack", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var singleLevelOverpackInner1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "OuterPack_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpackInner2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "OuterPack_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var firstNestedOverpackInner = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "firstNested_InnerPackage", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var secondNestedOverpackInner = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "secondNested_InnerPackage", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, onceNestedOverpackPackageState, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(onceNestedOverpackPackageState, twiceNestedsOverpackPackageState, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, singleLevelOverpackInner1, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, singleLevelOverpackInner2, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(onceNestedOverpackPackageState, firstNestedOverpackInner, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(twiceNestedsOverpackPackageState, secondNestedOverpackInner, ZDateTimeOffset.Now, "JEF");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var topLevelPackingLineCollection = consignmentDataObject.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new[] { "OuterOverpack" }, topLevelPackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var outerOverPack = topLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack_InnerPackage1", "OuterPack_InnerPackage2", "firstInnerOverpack" }, outerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var firstInnerOverPack = outerOverPack.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "firstInnerOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "firstNested_InnerPackage", "secondInnerOverpack" }, firstInnerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var secondInnerOverPack = firstInnerOverPack.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "secondInnerOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "secondNested_InnerPackage" }, secondInnerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_ReceiveConsignment_LastScreeningMethodForOverPack()
		{
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");

			var topLevelOverpackPackageState1 = Helper.CreateOverpackPackage("OuterOverpack1", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var topLevelOverpackPackageState2 = Helper.CreateOverpackPackage("OuterOverpack2", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var topLevelOverpackPackageState3 = Helper.CreateOverpackPackage("OuterOverpack3", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var topLevelOverpackPackageState4 = helper.CreateOverpackPackage("OuterOverpack4", receiveConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);
			var singleLevelOverpack1Inner1 = Helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack1_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpack1Inner2 = Helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack1_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpack2Inner1 = Helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack2_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpack3Inner1 = Helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack3_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpack4Inner1 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack4_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var singleLevelOverpack4Inner2 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack4_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var ovp1Child1Screening = Helper.CreatePackageScreening(singleLevelOverpack1Inner1.Package, "SC1", true);
			var ovp1Child2Screening = Helper.CreatePackageScreening(singleLevelOverpack1Inner2.Package, "SC1", true);
			ovp1Child1Screening.KPS_Time = DateTime.Now;
			ovp1Child2Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			var ovp2Screening = Helper.CreatePackageScreening(topLevelOverpackPackageState2.Package, "OV2", true);
			var ovp2Child1Screening = Helper.CreatePackageScreening(singleLevelOverpack2Inner1.Package, "SC8", true);
			ovp2Screening.KPS_Time = DateTime.Now;
			ovp2Child1Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			var ovp3Screening = Helper.CreatePackageScreening(topLevelOverpackPackageState3.Package, "OV3", true);
			ovp3Screening.KPS_Time = DateTime.Now;

			var ovp4Child1Screening = helper.CreatePackageScreening(singleLevelOverpack4Inner1.Package, "SC2", true);
			var ovp4Child2Screening = helper.CreatePackageScreening(singleLevelOverpack4Inner2.Package, "SC3", true);
			ovp1Child1Screening.KPS_Time = DateTime.Now;
			ovp1Child2Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState1, singleLevelOverpack1Inner1, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState1, singleLevelOverpack1Inner2, ZDateTimeOffset.Now, "JEF");
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState2, singleLevelOverpack2Inner1, ZDateTimeOffset.Now, "GDS");
			Helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState3, singleLevelOverpack3Inner1, ZDateTimeOffset.Now, "ABC");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState4, singleLevelOverpack4Inner1, ZDateTimeOffset.Now, "ABC");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState4, singleLevelOverpack4Inner2, ZDateTimeOffset.Now, "ABC");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var outerLevelPackingLineCollection = consignmentDataObject.PackingLineCollection;
			var outerOverPack1 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack1");
			var outerOverPack2 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack2");
			var outerOverPack3 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack3");
			var outerOverPack4 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack4");

			AssertContainsExactElementsInAnyOrder(new[] { "OuterOverpack1", "OuterOverpack2", "OuterOverpack3", "OuterOverpack4" }, outerLevelPackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack1_InnerPackage1", "OuterPack1_InnerPackage2" }, outerOverPack1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNull(outerOverPack1.ScreeningMethod);
			AssertNull(outerOverPack1.AviationSecurityInspectionType);
			AssertContainsExactElementsInAnyOrder(new[] { "SC1", "SC1" }, outerOverPack1.PackingLineCollection.Select(p => (string)p.ScreeningMethod));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack2_InnerPackage1" }, outerOverPack2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertEquals("OV2", outerOverPack2.ScreeningMethod);
			AssertEquals("OV2", outerOverPack2.AviationSecurityInspectionType.Code);
			AssertContainsExactElementsInAnyOrder(new[] { "SC8" }, outerOverPack2.PackingLineCollection.Select(p => (string)p.ScreeningMethod));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack3_InnerPackage1" }, outerOverPack3.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertEquals("OV3", outerOverPack3.ScreeningMethod);
			AssertEquals("OV3", outerOverPack3.AviationSecurityInspectionType.Code);
			var ovp3PacklineScreeningMethods = outerOverPack3.PackingLineCollection.Select(p => (string)p.ScreeningMethod).ToList();
			AssertEquals(1, ovp3PacklineScreeningMethods.Count);
			AssertNull(ovp3PacklineScreeningMethods[0]);

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack4_InnerPackage1", "OuterPack4_InnerPackage2" }, outerOverPack4.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNull(outerOverPack4.ScreeningMethod);
			AssertNull(outerOverPack4.AviationSecurityInspectionType);
			AssertContainsExactElementsInAnyOrder(new[] { "SC2", "SC3" }, outerOverPack4.PackingLineCollection.Select(p => (string)p.ScreeningMethod));
		}

		#endregion

		#region TestPopulateDataObject_Addresses

		public void TestPopulateDataObject_With_BookingParty_Consignee_Consignor_Pickup()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var pickup = data.Org4;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK, bookedByParty, consignor, consignee, pickup: pickup);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var organizationAddresses = consignmentDataObject.OrganizationAddressCollection;
			CombineAssertions(() =>
			{
				AssertEquals(5, organizationAddresses.Count);
				AssertEquals("Booking Party", bookedByParty.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "BookingPartyDocumentaryAddress").OrganizationCode);
				AssertEquals("Consignor", consignor.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageExporter").OrganizationCode);
				AssertEquals("Consignee", consignee.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "ConsigneeDocumentaryAddress").OrganizationCode);
				AssertEquals("Consignor Pickup", pickup.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "ConsignorPickupDeliveryAddress").OrganizationCode);
				AssertEquals(Data.Orgs.WUFSHIJNB.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageCFS").OrganizationCode);
			});
		}

		public void TestPopulateDataObject_WithOnlyWarehouseAddress()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var organizationAddresses = consignmentDataObject.OrganizationAddressCollection;
			CombineAssertions(() =>
			{
				AssertEquals(1, organizationAddresses.Count);
				AssertEquals(1, organizationAddresses.Count);
				AssertEquals(Data.Orgs.WUFSHIJNB.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageCFS").OrganizationCode);
			});
		}

		#endregion

		#region TestPopulateDataObject_ContainerDates

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Container_Dates()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_VehicleReference = "VEH1";
			var currentTime = ZDateTimeOffset.Now;
			receiveTransportationUnit.WRH_UnloadCompleteTime = currentTime;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertNotNull(consignmentDataObject?.ContainerCollection);
			AssertContainsExactElementsInAnyOrder("Should be the receive transportation unit's unload time", new[] { currentTime.ToZDateTime() }, consignmentDataObject.ContainerCollection.Select(c => c.LCLUnpack.Value));
		}

		#endregion

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCNExternalRef", "STD", warehouse.PK, jobID: "RCNJOBID");
			var reference1 = Helper.CreateAdditionalReference(receiveConsignment, "REF1");
			var reference2 = Helper.CreateAdditionalReference(receiveConsignment, "REF2");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var references = consignmentDataObject.AdditionalReferenceCollection;
			CombineAssertions(() =>
			{
				AssertEquals(CollectionContent.Partial, consignmentDataObject.AdditionalReferenceCollection.Content);
				AssertEquals(4, references.Count);
				AssertEquals("AAS", references.Single(r => r.ReferenceNumber.Equals(reference1.CE_EntryNum)).Type.Code);
				AssertEquals("AAS", references.Single(r => r.ReferenceNumber.Equals(reference2.CE_EntryNum)).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceive, references.Single(r => r.ReferenceNumber.Equals("RCNJOBID")).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveReference, references.Single(r => r.ReferenceNumber.Equals("RCNExternalRef")).Type.Code);
			});
		}

		public void TestPopulateDataObject_AdditionalReferences_WithCustomsReference()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCNExternalRef", "STD", warehouse.PK, jobID: "RCNJOBID");
			var reference1 = Helper.CreateCustomsAdditionalReference(receiveConsignment, "CRN", "Customs Release Number");
			var reference2 = Helper.CreateCustomsAdditionalReference(receiveConsignment, "CEN", "Customs Entry Number");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var references = consignmentDataObject.AdditionalReferenceCollection;
			CombineAssertions(() =>
			{
				AssertEquals(CollectionContent.Partial, consignmentDataObject.AdditionalReferenceCollection.Content);
				AssertEquals(4, references.Count);
				AssertEquals("CRN", references.Single(r => r.ReferenceNumber.Equals(reference1.CE_EntryNum)).Type.Code);
				AssertEquals("CEN", references.Single(r => r.ReferenceNumber.Equals(reference2.CE_EntryNum)).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceive, references.Single(r => r.ReferenceNumber.Equals("RCNJOBID")).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveReference, references.Single(r => r.ReferenceNumber.Equals("RCNExternalRef")).Type.Code);
			});
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var note1 = receiveConsignment.Notes.AddNew(true, "Custom Description", "Test Note 1");
			var note2 = receiveConsignment.Notes.AddNew(false, "Non-custom Description", "Test Note 2");
			
			var noteTypesNotPopulate = TransitWarehouseNoteHelper.GetNoteTypesNotPopulate();
			foreach (var noteTypeDescription in noteTypesNotPopulate)
			{
				receiveConsignment.Notes.AddNew(false, noteTypeDescription, $"Note Text for {noteTypeDescription}");
			}

			Factory.SaveForTesting();
			var notesInRCN = Factory.BOFactory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, WhsItemReceiveConsignmentSchema.Constants.TableName));
			CombineAssertions(() =>
			{
				AssertEquals(2 + noteTypesNotPopulate.Count, notesInRCN.Length);
				AssertEquals("Test Note 1", note1.ST_NoteText, notesInRCN.Single(r => r.ST_Description.Equals(note1.ST_Description)).ST_NoteText);
				AssertEquals("Test Note 2", note2.ST_NoteText, notesInRCN.Single(r => r.ST_Description.Equals(note2.ST_Description)).ST_NoteText);
				foreach (var noteTypeDescription in noteTypesNotPopulate)
				{
					AssertEquals($"Note Text for {noteTypeDescription}", notesInRCN.Single(r => r.ST_Description.Equals(noteTypeDescription)).ST_NoteText);
				}
			});

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var notesInDataObject = consignmentDataObject.NoteCollection;
			CombineAssertions(() =>
			{
				AssertEquals(2, notesInDataObject.Count);
				AssertEquals("Test Note 1", note1.ST_NoteText, notesInDataObject.Single(r => r.Description.HasValue && r.NoteText.HasValue && r.Description.Value == note1.ST_Description).NoteText.Value);
				AssertEquals("Test Note 2", note2.ST_NoteText, notesInDataObject.Single(r => r.Description.HasValue && r.NoteText.HasValue && r.Description.Value == note2.ST_Description).NoteText.Value);
			});
		}

		#endregion

		#region TestPopulateDataObject_EmptyTransportMode

		public void TestPopulateDataObject_EmptyTransportMode()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			receiveConsignment.WRC_TransportMode = "";
			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertNull("Transport Mode property must be null for empty Transport modes.", consignmentDataObject.TransportMode);
		}

		#endregion

		#region TestPopulateDataObject_TransportRoutings

		public void TestPopulateDataObject_TransportRoutings()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);

			var routing1 = Helper.CreateTransportRouting(receiveConsignment, "ROUTING1", "AUSYD", "NZAKL");
			var routing2 = Helper.CreateTransportRouting(receiveConsignment, "ROUTING2", "AUPER", "NZAKL");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			CombineAssertions(() =>
			{
				AssertEquals(2, consignmentDataObject.TransportLegCollection.Count);
				AssertTransportRouting(consignmentDataObject.TransportLegCollection.Single(l => l.VoyageFlightNo.Value == "ROUTING1"), "AUSYD", "NZAKL");
				AssertTransportRouting(consignmentDataObject.TransportLegCollection.Single(l => l.VoyageFlightNo.Value == "ROUTING2"), "AUPER", "NZAKL");
			});
		}

		void AssertTransportRouting(TransportLeg routingLeg, string discPort, string loadPort)
		{
			AssertEquals(discPort, routingLeg.PortOfDischarge.Code);
			AssertEquals(loadPort, routingLeg.PortOfLoading.Code);
		}

		#endregion

		#region TestPopulateDataObject_AdditionalServices

		public void TestPopulateDataObject_AdditionalServices()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);

			var service1 = receiveConsignment.Services.AddNew();
			service1.ES_ServiceCode = "S1";
			var service2 = receiveConsignment.Services.AddNew();
			service2.ES_ServiceCode = "S2";

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var services = consignmentDataObject.LocalProcessing.AdditionalServiceCollection;

			CombineAssertions(() =>
			{
				AssertEquals(2, services.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { service1.ES_ServiceCode, service2.ES_ServiceCode }, consignmentDataObject.LocalProcessing.AdditionalServiceCollection.Select(x => x.ServiceCode.Code.Value));
			});
		}

		#endregion

		#region TestAllPackagesWithinRCN_HasBeenReceived_Containerised_AllPackedIntoOneContainer

		public void TestAllPackagesWithinRCN_HasBeenReceived_Containerised_AllPackedIntoOneContainer()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "CNT1");
			AssertEquals("Precondition", "RTU1", rtu.ContainerNumber);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);
			var containerOnTopLevelDO = consignmentDataObject.ContainerCollection.Single();

			AssertEquals("RTU1", rtu.WRH_VehicleReference); // for the moment container number is stored on vehicle reference and not on container's package header
			AssertEquals("RTU1", containerOnTopLevelDO.ContainerNumber);
			AssertEquals(0, containerOnTopLevelDO.Link);
			AssertEquals(2, consignmentDataObject.PackingLineCollection.Count);
			var packLineForPkg1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPkg2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			AssertEquals(0, packLineForPkg1.ContainerLink);
			AssertEquals(0, packLineForPkg2.ContainerLink);
		}

		#endregion

		#region TestUXMLContainTargetWhenRecipientRoleIsCOA

		public void TestUXMLContainTargetWhenRecipientRoleIsCOA()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "CNT1");
			AssertEquals("Precondition", "RTU1", rtu.ContainerNumber);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.COA, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var relatedDataTarget = consignmentDataObject.DataContext.DataTargetCollection.Single();
			AssertEquals("UnderBond", relatedDataTarget.Type);
		}

		#endregion

		#region TestAllPackagesWithinRCN_HasBeenReceived_Containerised_AllPackedIntoMultipleContainers

		public void TestAllPackagesWithinRCN_HasBeenReceived_Containerised_AllPackedIntoMultipleContainers()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "CNT1");
			var rtu2 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU2", "CNT2");
			AssertEquals("Precondition", "RTU1", rtu1.ContainerNumber);
			AssertEquals("Precondition", "RTU2", rtu2.ContainerNumber);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals(2, consignmentDataObject.ContainerCollection.Count);
			var cnt1OnTopLevelDO = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "RTU1");
			var cnt2OnTopLevelDO = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "RTU2");
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 0, 1 }, consignmentDataObject.ContainerCollection.Select(c => c.Link.Value));
			AssertEquals(2, consignmentDataObject.PackingLineCollection.Count);
			var packLineForPkg1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPkg2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			AssertEquals(cnt1OnTopLevelDO.Link, packLineForPkg1.ContainerLink);
			AssertEquals(cnt2OnTopLevelDO.Link, packLineForPkg2.ContainerLink);
		}

		#endregion

		#region TestSomePackagesWithinRCN_HasBeenReceived_Containerised_SomePackedIntoContainers

		public void TestSomePackagesWithinRCN_HasBeenReceived_Containerised_SomePackedIntoContainers()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "CNT1");
			var rtu2 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU2", "CNT2");
			AssertEquals("Precondition", "RTU1", rtu1.ContainerNumber);
			AssertEquals("Precondition", "RTU2", rtu2.ContainerNumber);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals(2, consignmentDataObject.ContainerCollection.Count);
			var cnt1OnTopLevelDO = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "RTU1");
			var cnt2OnTopLevelDO = consignmentDataObject.ContainerCollection.Single(c => c.ContainerNumber.Value == "RTU2");
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 0, 1 }, consignmentDataObject.ContainerCollection.Select(c => c.Link.Value));

			AssertEquals(3, consignmentDataObject.PackingLineCollection.Count);
			var packLineForPkg1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPkg2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packLineForPkg3 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3");
			AssertEquals(cnt1OnTopLevelDO.Link, packLineForPkg1.ContainerLink);
			AssertEquals(cnt2OnTopLevelDO.Link, packLineForPkg2.ContainerLink);
			AssertNull(packLineForPkg3.ContainerLink);
		}

		#endregion

		#region TestSomePackagesWithinRCN_Received_UnPackedFromVehicles

		public void TestSomePackagesWithinRCN_Received_UnPackedFromVehicles()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu1 = SetupReceiveTransportationUnitForTesting("RTU1", "V1");
			var rtu2 = SetupReceiveTransportationUnitForTesting("RTU2", "V2");
			AssertEquals("Precondition", "V1", rtu1.WRH_VehicleReference);
			AssertEquals("Precondition", "V2", rtu2.WRH_VehicleReference);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertNull(consignmentDataObject.ContainerCollection);
			AssertEquals(3, consignmentDataObject.PackingLineCollection.Count);
			var packLineForPkg1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPkg2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packLineForPkg3 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3");
			var transportReferenceForPkg1 = packLineForPkg1.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit);
			var transportReferenceForPkg2 = packLineForPkg2.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit);

			AssertEquals("RTU1", transportReferenceForPkg1.ReferenceNumber);
			AssertEquals("RTU2", transportReferenceForPkg2.ReferenceNumber);
			AssertNull(packLineForPkg3.ReferenceNumberCollection);
			AssertNull(packLineForPkg1.ContainerLink);
			AssertNull(packLineForPkg2.ContainerLink);
			AssertNull(packLineForPkg3.ContainerLink);
		}

		#endregion

		#region TestSomePackagesWithinRCN_Received_UnPackedFromVehicles

		public void TestSomePackagesWithinRCN_Received_UnPackedFromVehiclesAndContainers()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var rtu1 = SetupReceiveTransportationUnitForTesting("RTU1", "V1");
			var rtu2 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU2", "C2");
			AssertEquals("Precondition", "V1", rtu1.WRH_VehicleReference);
			AssertEquals("Precondition", "RTU2", rtu2.ContainerNumber);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var container = consignmentDataObject.ContainerCollection.Single();
			AssertEquals("Precondition", 0, container.Link);
			AssertEquals("Precondition", "RTU2", container.ContainerNumber);

			AssertEquals(2, consignmentDataObject.PackingLineCollection.Count);
			var packLineForPkg1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPkg2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var transportReferenceForPkg1 = packLineForPkg1.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit);
			var transportReferenceForPkg2 = packLineForPkg2.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit);
			AssertEquals("RTU1", transportReferenceForPkg1.ReferenceNumber);
			AssertEquals("RTU2", transportReferenceForPkg2.ReferenceNumber);
			AssertNull(packLineForPkg1.ContainerLink);
			AssertEquals(0, packLineForPkg2.ContainerLink);
		}

		#endregion

		#region TestPopulateDataObject_ReceiveTransportationUnits

		public void TestPopulateDataObject_ReceiveTransportationUnits()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var receiveTransportationUnit1 = SetupReceiveTransportationUnitForTesting("RTU1", "Vehicle1");
			var receiveTransportationUnit2 = SetupReceiveTransportationUnitForTesting("RTU2", "Vehicle2");
			var receiveTransportationUnitWithContainer = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU3", "Container1");

			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit1);
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit2);
			var packageState3 = Helper.CreatePackageState(receiveConsignment, 1, "BOX", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnitWithContainer);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var receiveTransportationUnits = consignmentDataObject.RelatedShipmentCollection;
			var rtu1 = receiveTransportationUnits.Single(rtu => rtu.VesselName.Equals("Vehicle1"));
			var rtu2 = receiveTransportationUnits.Single(rtu => rtu.VesselName.Equals("Vehicle2"));
			var rtu3 = receiveTransportationUnits.Single(rtu => rtu.VesselName.Equals("RTU3"));
			CombineAssertions(() =>
			{
				AssertEquals(3, receiveTransportationUnits.Count);
				AssertNull("Receive Transportation Unit 1 should not have a container collection", rtu1.ContainerCollection);
				AssertNull("Receive Transportation Unit 2 should not have a container collection", rtu2.ContainerCollection);
				AssertEquals("Receive Transportation Unit 3 with Container should have one container collection", "Container1", rtu3.ContainerCollection.Single().ContainerNumber);

				var packLines = consignmentDataObject.PackingLineCollection;
				AssertEquals("Each package must have a pack line.", 3, packLines.Count);
				var packLineInRTU1 = packLines.Single(p => p.ReferenceNumber.Value == "PKG1");
				var packLineInRTU2 = packLines.Single(p => p.ReferenceNumber.Value == "PKG2");
				var packLineInRTU3 = packLines.Single(p => p.ReferenceNumber.Value == "PKG3");
				AssertPackLineReference(packLineInRTU1, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, "RTU1");
				AssertPackLineReference(packLineInRTU2, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, "RTU2");
				AssertPackLineReference(packLineInRTU3, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, "RTU3");
			});
		}

		#endregion

		#region TestPopulateDataObject_ReceiveASNs

		public void TestPopulateDataObject_ReceiveASNs()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asnWithoutPivots = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtuTruck = SetupReceiveTransportationUnitForTesting("Vehicle1", "Vehicle1");
			var rtuContainer = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Container1", "Container1");

			var booked = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asnWithoutPivots);
			var bookedWithoutASN = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked);
			var arrivedWithASN = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuTruck, receiveASN: asnWithoutPivots);
			var arrivedWithoutASN = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuContainer);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			CombineAssertions(() =>
			{
				var relatedShipments = consignmentDataObject.RelatedShipmentCollection;
				var asn = GetMatchingShipment(relatedShipments, DataContextType.TransitReceiveASN, "ASN1");
				var rtuForTruck = GetMatchingShipment(relatedShipments, DataContextType.TransitReceiveHeader, "Vehicle1");
				var rtuForContainer = GetMatchingShipment(relatedShipments, DataContextType.TransitReceiveHeader, "Container1");
				AssertEquals(3, relatedShipments.Count);
				AssertNull("ASN must not have a container collection since there are no pivots.", asn.ContainerCollection);
				AssertNull("RTU for truck must not have a container collection.", rtuForTruck.ContainerCollection);
				AssertEquals("RTU for Container must have a container collection.", "Container1", rtuForContainer.ContainerCollection.Single().ContainerNumber);

				var packLines = consignmentDataObject.PackingLineCollection;
				AssertEquals("Each package must have a pack line.", 4, packLines.Count);
				var bookedPackLine = packLines.Single(p => p.ReferenceNumber.Value == "PKG1");
				var bookedPackLineWithoutASN = packLines.Single(p => p.ReferenceNumber.Value == "PKG2");
				var arrivedPackLineWithASN = packLines.Single(p => p.ReferenceNumber.Value == "PKG3");
				var arrivedPackLineWithoutASN = packLines.Single(p => p.ReferenceNumber.Value == "PKG4");
				AssertPackLineReference(bookedPackLine, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveASN, "ASN1");
				AssertNull("No reference number collections since there are no ASNs or RTUs.", bookedPackLineWithoutASN.ReferenceNumberCollection);
				AssertPackLineReference(arrivedPackLineWithASN, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, "Vehicle1");
				AssertPackLineReference(arrivedPackLineWithASN, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveASN, "ASN1");
				AssertPackLineReference(arrivedPackLineWithoutASN, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, "Container1");
			});
		}

		static void AssertPackLineReference(PackingLine packLine, string refType, string referenceNumber)
		{
			var referenceNumberForASN = packLine.ReferenceNumberCollection.Single(r => r.Type.Code.Value == refType);
			AssertEquals(referenceNumber, referenceNumberForASN.ReferenceNumber.Value);
		}

		public void TestPopulateDataObject_ReceiveASNs_Containers()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtuContainer1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Container1", "Container1");
			var rtuContainer2 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Container2", "Container2");
			Helper.CreateReceiveASNRTUPivot(rtuContainer1.PK, asn.PK);
			Helper.CreateReceiveASNRTUPivot(rtuContainer2.PK, asn.PK);

			var package = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			CombineAssertions(() =>
			{
				var asnRelatedShipment = consignmentDataObject.RelatedShipmentCollection.Single();
				AssertEquals(nameof(DataContextType.TransitReceiveASN), asnRelatedShipment.DataContext.DataSourceCollection.Single().Type.Value);
				AssertEquals("ASN1", asnRelatedShipment.DataContext.DataSourceCollection.Single().Key.Value);
				var containers = asnRelatedShipment.ContainerCollection;
				AssertEquals("ASN must have containers from pivot.", 2, containers.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "Container1", "Container2" }, containers.Select(c => c.ContainerNumber.Value));
			});
		}

		static Shipment GetMatchingShipment(List<Shipment> relatedShipments, DataContextType contextType, string key)
		{
			return relatedShipments.Single(shipment => shipment.DataContext.DataSourceCollection.Single().Type.Value == contextType.ToString() && shipment.DataContext.DataSourceCollection.Single().Key.Value == key);
		}

		#endregion

		#region TestPopulateDataObject_PopulateOutturnDetail

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPopulateDataObject_PopulateOutturnDetail()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var receiveTransportationUnit = SetupReceiveTransportationUnitForTesting("RTU1", "Vehicle1");

			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-27 12:00:00 +08:00");
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-27 04:00:00 +00:00");
			var packageState3 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);

			var ovpPackageHasUnloadedTime = helper.CreateOverpackPackage("OVPPKG1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment);
			ovpPackageHasUnloadedTime.WPS_UnloadedNotYetProcessedTime = ovpPackageHasUnloadedTime.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-27 04:00:00 +00:00");
			var ovpPackageNoUnloadedTimeNoInner = helper.CreateOverpackPackage("OVPPKG2", receiveConsignment, null, rcn: receiveConsignment);
			var ovpPackageNoUnloadedTimeHasInner = helper.CreateOverpackPackage("OVPPKG3", receiveConsignment, null, rcn: receiveConsignment);
			ovpPackageNoUnloadedTimeHasInner.WPS_SystemCreateTimeUtc = new ZDateTime(2025, 4, 27, 4, 0, 0);
			var innerPackage = helper.CreatePackageState(receiveConsignment, 1, "PKG", "InnerPKG", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			helper.PackPackageIntoHandlingUnit(ovpPackageNoUnloadedTimeHasInner, innerPackage, ZDateTimeOffset.Now, "KL6", ovpPackageNoUnloadedTimeHasInner);

			Factory.SaveForTesting();

			Helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			Helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			Helper.SetupPackageForOutturn(packageState3.Package, bookedQty: 10, damagedQty: 0, height: 1, length: 2, volume: 100, weight: 4, width: 5);
			Helper.SetupPackageForOutturn(ovpPackageHasUnloadedTime.Package, bookedQty: 10, damagedQty: 0, height: 1, length: 2, volume: 100, weight: 4, width: 5);
			Helper.SetupPackageForOutturn(ovpPackageNoUnloadedTimeNoInner.Package, bookedQty: 10, damagedQty: 0, height: 1, length: 2, volume: 100, weight: 4, width: 5);
			Helper.SetupPackageForOutturn(ovpPackageNoUnloadedTimeHasInner.Package, bookedQty: 10, damagedQty: 0, height: 1, length: 2, volume: 100, weight: 4, width: 5);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2", "PKG3", "OVPPKG1", "OVPPKG2", "OVPPKG3" }, consignmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var packingLine1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packingLine2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packingLine3 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3");
			var ovpHasUnloadTimePackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG1");
			var ovpNoUnloadTimeNoInnerPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG2");
			var ovpNoUnloadTimeHasInnerPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG3");

			AssertOutturnDetail(packingLine1, 1, new ZDateTime(2025, 4, 27, 14, 0, 0), null, 0, 1, 2, 10, 4, 5);
			AssertOutturnDetail(packingLine2, 1, new ZDateTime(2025, 4, 27, 14, 0, 0), null, 1, 1, 2, 10, 4, 5);
			AssertOutturnDetail(packingLine3, 0, null, null, 0, 0, 0, 0, 0, 0);
			AssertOutturnDetail(ovpHasUnloadTimePackingLine, 1, new ZDateTime(2025, 4, 27, 14, 0, 0), null, 0, 1, 2, 10, 4, 5);
			AssertOutturnDetail(ovpNoUnloadTimeNoInnerPackingLine, 0, null, null, 0, 0, 0, 0, 0, 0);
			AssertOutturnDetail(ovpNoUnloadTimeHasInnerPackingLine, 1, new ZDateTime(2025, 4, 27, 14, 0, 0), null, 0, 0, 0, 0, 0, 0);
		}

		public void TestPopulateDataObject_PalletizedPackline()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "Vehicle1");

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreateAndAttachArrivedInners(packageState, 10, "CTN");
			Helper.SetupPackageForOutturn(packageState.Package, bookedQty: 10, damagedQty: 10, height: 0, length: 0, volume: 0, weight: 0, width: 0);
			Helper.SetupPackageForOutturn(packageState.Package.Packages.Single(), bookedQty: 0, damagedQty: 0, height: 0, length: 0, volume: 0, weight: 0, width: 0);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1" }, consignmentDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var packingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			AssertEquals("PKG1", packingLine.ReferenceNumber.Value);
			AssertEquals("PLT", packingLine.PackType.Code.Value);
			AssertEquals(0, packingLine.ContainerLink);
			AssertOutturnDetail(packingLine, 1, packageState.WPS_UnloadedTime.ToLocalZDateTime(), null, 1, 0, 0, 0, 0, 0);

			AssertEquals(1, packingLine.PackingLineCollection.Count);
			var innerPackingLine = packingLine.PackingLineCollection.Single();
			AssertEquals("CTN", innerPackingLine.PackType.Code.Value);
			AssertNull(innerPackingLine.ContainerLink);
			AssertOutturnDetail(innerPackingLine, 1, packageState.WPS_UnloadedTime.ToLocalZDateTime(), null, 0, 0, 0, 0, 0, 0);
		}

		#endregion

		#region TestPopulateDataObject_PackagesWithAttachedShipmentID

		public void TestPopulateDataObject_PackagesWithAttachedShipmentID()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN", warehouse.PK, bookedByParty, consignor, consignee, serviceLevel: "STD");
			var jobId = receiveConsignment.WRC_JobID;

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			helper.CreateAdditionalReference(packageState, "S0000001", AdditionalReferenceTypes.Codes.BookingPartyReference);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.FOR, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("Packages on the RCN's package job should have AddInfo of ShipmentID.",
				"S0000001", consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.ForwardingShipment)).Value);
		}

		#endregion

		#region TestPopulateDataObject_OrderReferences

		public void TestPopulateDataObject_OrderReferences()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCNExternalRef", "STD", warehouse.PK, jobID: "RCNJOBID");
			var packageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var packageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked);
			var orderReference1 = helper.PackingHelper.CreatePackageOrderReference(packageState1.Package, "ORN1", "BAT1", "CIN1", new ZDate(2024, 01, 01), "LNE1", "SKU1", "SRN1");
			var orderReference2 = helper.PackingHelper.CreatePackageOrderReference(packageState2.Package, "ORN2", "BAT2", "CIN2", new ZDate(2024, 01, 02), "LNE2", "SKU2", "SRN2");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var references = consignmentDataObject.RelatedShipmentCollection;
			var packlines = consignmentDataObject.PackingLineCollection;
			CombineAssertions(() =>
			{
				AssertEquals(2, references.Count);
				var orderLine1 = references.Single(r => r.Order.OrderNumber.Value == "ORN1").Order.OrderLineCollection.First();
				var orderLine2 = references.Single(r => r.Order.OrderNumber.Value == "ORN2").Order.OrderLineCollection.First();
				AssertOrderReference(orderLine1, "BAT1", "CIN1", new ZDateTime(2024, 01, 01), "LNE1", "SKU1", "SRN1", 0);
				AssertOrderReference(orderLine2, "BAT2", "CIN2", new ZDateTime(2024, 01, 02), "LNE2", "SKU2", "SRN2", 1);
				var packedItem1 = packlines.Single(p => p.ReferenceNumber.Value == "PKG1").PackedItemCollection.First();
				var packedItem2 = packlines.Single(p => p.ReferenceNumber.Value == "PKG2").PackedItemCollection.First();
				AssertEquals(0, packedItem1.OrderLineLink);
				AssertEquals(1, packedItem2.OrderLineLink);
			});
		}

		void AssertOrderReference(OrderLine orderLine, string batchNumber, string commercialInvoiceNumber, ZDateTime expiryDate, string lineReference, string skuPartNumber, string serialNumber, int link)
		{
			AssertEquals(batchNumber, orderLine.BatchNumber);
			AssertEquals(commercialInvoiceNumber, orderLine.CommercialInvoiceNumber);
			AssertEquals(expiryDate, orderLine.ExpiryDate);
			AssertEquals(lineReference, orderLine.LineReference);
			AssertEquals(link, orderLine.Link);
			AssertEquals(skuPartNumber, orderLine.Product.Code);
			AssertEquals(serialNumber, orderLine.SerialNumber);
		}

		#endregion

		#region TestPopulateDataObject_ValidationRuleCollection

		public void TestPopulateDataObject_ValidationRuleCollection()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCNExternalRef", "STD", warehouse.PK, jobID: "RCNJOBID");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals(0, consignmentDataObject.ValidationRuleCollection.Count);
		}

		#endregion

		#region TestPopulateDataObject_Destination

		public void TestPopulateDataObject_Destination()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			receiveConsignment.WRC_RL_NKDestination = "ARFLO";

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("ARFLO", consignmentDataObject.PortOfDestination.Code);
			AssertEquals("Florida", consignmentDataObject.PortOfDestination.Name);
		}

		#endregion

		#region TestPopulateDataObject_OrderNumber

		public void TestPopulateDataObject_OrderNumber()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "RCNJOBID");
			var order1 = Helper.CreateWhsItemConsignmentOrderReference("ORDER1", receiveConsignment);
			var order2 = Helper.CreateWhsItemConsignmentOrderReference("ORDER2", receiveConsignment);
			var order3 = Helper.CreateWhsItemConsignmentOrderReference("ORDER3", receiveConsignment);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);
			var consignmentOrderReferenceCollection = consignmentDataObject.LocalProcessing.OrderNumberCollection;

			AssertEquals(3, consignmentDataObject.LocalProcessing.OrderNumberCollection.Count);
			AssertCollectionContains("ORDER1", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
			AssertCollectionContains("ORDER2", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
			AssertCollectionContains("ORDER3", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
		}

		#endregion

		#endregion

		#region TestPopulateDataObject_AdjustedOutPackage

		public void TestPopulateDataObject_AdjustedOutPackage()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1", "Vehicle1");

			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("RCN should only 1 Package", 1, consignmentDataObject.PackingLineCollection.Count);
			AssertEquals("RCN should include a package PKG1.", "PKG1", consignmentDataObject.PackingLineCollection.Single().ReferenceNumber);
		}

		#endregion

		#region TestPopulateDataObject_EmptyPackingLineCollection

		public void TestPopulateDataObject_EmptyPackingLineCollection()
		{
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var writer = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(rcn);

			AssertNotNull(dataObject.PackingLineCollection);
			AssertEquals("Content should be completed", CollectionContent.Complete, dataObject.PackingLineCollection.Content);
		}

		#endregion

		#region TestPopulateDataObject_LocalCartageCTO

		public void TestPopulateDataObject_LocalCartageCTO()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var localCTO = data.Org1;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(receiveConsignment, DocAddressTypes.Codes.LocalCartageCTO, localCTO.MainAddress);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var organizationAddresses = consignmentDataObject.OrganizationAddressCollection;
			AssertEquals(localCTO.OH_Code, organizationAddresses.First(o => o.AddressType.Value == "LocalCartageCTO").OrganizationCode);
		}

		#endregion

		#region TestPopulateDataObject_DateCollection

		public void TestPopulateDataObject_DateCollection()
		{
			var now = ZDateTime.Now;
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.WRC_ExpectedArrivalTime = now;

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			var dateCollection = consignmentDataObject.DateCollection;
			AssertEquals(now, dateCollection.Single(o => o.Type == DateType.Arrival).Value);
		}

		#endregion

		#region TestPopulateDataObject_FlightNo

		public void TestPopulateDataObject_FlightNo()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RC00000001", warehouse.PK);
			Helper.CreateTransportRouting(receiveConsignment, "QF846", "AUBNE", "NZAKL");
			Helper.CreateTransportRouting(receiveConsignment, "QF845", "AUSYD", "AUBNE");

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("QF846", consignmentDataObject.VoyageFlightNo);
		}

		#endregion

		#region TestPopulateDataObject_AirCargo

		public void TestPopulateDataObject_AirCargoRequired()
		{
			var receiveConsignment = Helper.CreateReceiveConsignment("RC00000001", warehouse.PK);
			receiveConsignment.WRC_HouseBillNumber = "H0001";
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, weight: 1);
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, weight: 2);

			var consignmentDataObjectWriter = new WhsTransitReceiveConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(receiveConsignment);

			AssertEquals("H0001", consignmentDataObject.WayBillNumber);
			AssertEquals(WayBillTypeList.Codes.House, consignmentDataObject.WayBillType.Code);
			AssertEquals("PKG", consignmentDataObject.TotalNoOfPacksPackageType.Code);
			AssertEquals(3m, consignmentDataObject.TotalWeight);
			AssertEquals("KG", consignmentDataObject.TotalWeightUnit.Code);
			AssertEquals(2, consignmentDataObject.TotalNoOfPieces);
		}

		#endregion

		#region Implementation

		void AssertOutturnDetail(PackingLine packingLine, int expectedOutturnQty, ZDateTime? expectedUnloadTime, ZDateTime? expectedLoadTime, int expectedOutturnDamagedQty, decimal expectedOutturnedHeight, decimal expectedOtturnedLength, decimal expectedOutturnedVolume, decimal expectedOutturnedWeight, decimal expectedOutturnedWidth)
		{
			AssertEquals(expectedOutturnQty, packingLine.OutturnQty);
			AssertEquals(expectedUnloadTime, packingLine.UnloadDate);
			AssertEquals(expectedLoadTime, packingLine.LoadDate);
			AssertEquals(expectedOutturnDamagedQty, packingLine.OutturnDamagedQty);
			AssertEquals(expectedOutturnedHeight, packingLine.OutturnedHeight);
			AssertEquals(expectedOtturnedLength, packingLine.OutturnedLength);
			AssertEquals(expectedOutturnedVolume, packingLine.OutturnedVolume);
			AssertEquals(expectedOutturnedWeight, packingLine.OutturnedWeight);
			AssertEquals(expectedOutturnedWidth, packingLine.OutturnedWidth);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
		WhsWarehouse warehouse => Data.Warehouse;

		void SetupDataContextWithTargetForImport(Shipment dataObject, DataContextType dataContextType, string targetKey, ServiceCodeType serviceCode)
		{
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(dataContextType, targetKey);
			dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbBranch.CurrentBranch.Company);
			dataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { ServiceCode = serviceCode } } });
		}

		void SetupDataContextWithTargetRecipientForExport(Shipment dataObject, RecipientRoleType recipientRoleType)
		{
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.SetCompanyAndDataProviderDetails(GlbBranch.CurrentBranch.Company);
			dataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { recipientRoleType }.ToRecipientRoleDetails() });
		}

		WhsItemReceiveConsignment SetupReceiveConsignmentWithTestDataForTesting()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var receiveConsignment = Helper.CreateReceiveConsignment("RC00000001", warehouse.PK, bookedByParty, consignor, consignee, serviceLevel: "STD");
			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PACKAGEID", TransitWarehouseStatuses.Codes.Booked);
			receiveConsignment.WRC_HouseBillNumber = "REF";
			var note = receiveConsignment.Notes.AddNew(true, "Custom Description", "Test Note");
			note.ST_NoteType = StmNoteDescription.Pub;
			Helper.CreateTransportRouting(receiveConsignment, "ROUTING", "AUBNE", "NZAKL");
			var service = receiveConsignment.Services.AddNew();
			service.ES_ServiceCode = "S1";

			return receiveConsignment;
		}

		WhsItemReceiveTransportationUnit SetupReceiveTransportationUnitForTesting(ZString reference, string vehicleNumber = "V1")
		{
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit(reference, warehouse.PK, location.PK, vehicleNumber);
			receiveTransportationUnit.WRH_VehicleReference = vehicleNumber;

			return receiveTransportationUnit;
		}

		WhsItemReceiveTransportationUnit SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting(string reference, string containerID = "CNT1")
		{
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType(reference, warehouse.PK, location.PK, containerID);
			receiveTransportationUnit.WRH_VehicleReference = reference;
			return receiveTransportationUnit;
		}

		#endregion
	}
}
