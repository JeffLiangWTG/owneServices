using System;
using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveASNDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitReceiveASN,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region Integration Tests

		#region Test_EventExport_Writes_XML_WithoutError
		public void Test_EventExport_Writes_XML_WithoutError()
		{
			var receiveASN = helper.CreateReceiveASN("ASN", Data.Warehouse.PK);

			var trigger = helper.GetEventTrigger(receiveASN,
				Core.Constants.Workflow.WorkflowTriggerType,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"Export UXML From ASN");

			var notification = helper.GetEventNotification(trigger,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				MessageRecipientPartyTypeList.Codes.OrgProxy);
			Factory.SaveForTesting();

			using (var tempDirectory = new TempDirectory())
			{
				var asnReference = receiveASN[WhsItemReceiveASNSchema.WRP_ReferenceNumber].ToString();

				var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(notification, receiveASN);
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", $"UniversalShipment from [{asnReference}] saved", exportResult.Message);
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

		#endregion

		#region PopulateDataObject Tests

		#region TestBasicFieldMappings

		public void TestBasicFieldMappings()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			receiveASN.WRP_ETA = new ZDateTime(2020, 1, 28);

			var writer = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveASN);

			var expectedEta = receiveASN.WRP_ETA;
			var arrivalDate = dataObject.DateCollection?.FirstOrDefault(d => d.Type == DateType.Arrival);

			CombineAssertions(() =>
			{
				AssertNotNull(dataObject);
				AssertEquals("A Shipping Notice arrival time is an estimate", true, arrivalDate?.IsEstimate);
				AssertEquals("Should be the advanced shipping notice's eta", receiveASN.WRP_ETA, arrivalDate?.Value);
			});
		}

		#endregion

		#region TestPopulateDataObject_PackagesOnConsignments

		public void TestPopulateDataObject_PackagesOnConsignments()
		{
			var receiveASN1 = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var receiveASN2 = helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var jobId2 = receiveConsignment2.WRC_JobID;

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, receiveASN: receiveASN1);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: receiveASN1);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, receiveASN: receiveASN2);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Booked);

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN1)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN1);

			var subShipmentKeys = asnDataObject.SubShipmentCollection?.Select(s => s.DataContext.DataSourceCollection.Single().Key.Value);
			AssertContainsExactElementsInAnyOrder("RCNs found through packages on the ASN should be included as subshipments.", new string[] { jobId1, jobId2 }, subShipmentKeys);

			var rcn1 = asnDataObject.SubShipmentCollection.Single(s => s.DataContext.DataSourceCollection.Single().Key.Value == jobId1);
			var rcn2 = asnDataObject.SubShipmentCollection.Single(s => s.DataContext.DataSourceCollection.Single().Key.Value == jobId2);
			const string packageErrorMessage = "RCNs should only include packages on their package job that also link to the ASN.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1" }, rcn1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG2" }, rcn2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNull("All ASN packages should be on its subshipments", asnDataObject.PackingLineCollection);
		}

		#endregion

		#region TestPopulateDataObject_Addresses

		public void TestPopulateDataObject_With_BookingParty_Consignee_Consignor()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var transportCompany = data.Org2;
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK, bookedByParty, transportCompany);

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);

			var expectedAddresses = new (string, string)[]
			{
				("BookingPartyDocumentaryAddress", bookedByParty.OH_Code.ToString()),
				("TransportCompanyDocumentaryAddress", transportCompany.OH_Code.ToString()),
				("LocalCartageCFS", Data.Orgs.WUFSHIJNB.OH_Code.ToString()),
				("ArrivalCFSAddress", Data.Orgs.WUFSHIJNB.OH_Code.ToString()),
			};
			var actualAddresses = asnDataObject.OrganizationAddressCollection.Select(a => (a.AddressType.Value.ToString(), a.OrganizationCode.ToString()));

			AssertContainsExactElementsInAnyOrder(expectedAddresses, actualAddresses);
		}

		public void TestPopulateDataObject_WithOnlyWarehouseAddress()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);

			var expectedAddresses = new (string, string)[] { ("LocalCartageCFS", Data.Orgs.WUFSHIJNB.OH_Code.ToString()) };
			var actualAddresses = asnDataObject.OrganizationAddressCollection.Select(a => (a.AddressType.Value.ToString(), a.OrganizationCode.ToString()));

			AssertContainsExactElementsInAnyOrder(expectedAddresses, [actualAddresses.First() ]);
		}

		#endregion

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var reference1 = helper.CreateAdditionalReference(receiveASN, "REF1");
			var reference2 = helper.CreateAdditionalReference(receiveASN, "REF2");

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);

			AssertEquals(CollectionContent.Partial, asnDataObject.AdditionalReferenceCollection.Content);

			var expectedReferences = new string[] { "REF1", "REF2" };
			var actualReferences = asnDataObject.AdditionalReferenceCollection.Select(r => r.ReferenceNumber.ToString());

			AssertContainsExactElementsInAnyOrder(expectedReferences, actualReferences);
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var note1 = receiveASN.Notes.AddNew(true, "Custom Description", "Test Note 1");
			var note2 = receiveASN.Notes.AddNew(false, "Non-custom Description", "Test Note 2");

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);

			var expectedNotes = new (string, string)[] { ("Custom Description", "Test Note 1"), ("Non-custom Description", "Test Note 2") };
			var actualNotes = asnDataObject.NoteCollection.Select(n => (n.Description.ToString(), n.NoteText.ToString()));

			AssertContainsExactElementsInAnyOrder(expectedNotes, actualNotes);
		}

		#endregion

		#region TestPopulateDataObject_TransportRoutings

		public void TestPopulateDataObject_TransportRoutings()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			var routing1 = helper.CreateTransportRouting(receiveASN, "ROUTING1", "AUSYD", "NZAKL");
			var routing2 = helper.CreateTransportRouting(receiveASN, "ROUTING2", "AUPER", "NZAKL");

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);

			var expectedRoutings = new (string, string, string)[] { ("ROUTING1", "AUSYD", "NZAKL"), ("ROUTING2", "AUPER", "NZAKL") };
			var actualRoutings = asnDataObject.TransportLegCollection.Select(t => (t.VoyageFlightNo.Value.ToString(), t.PortOfDischarge.Code.ToString(), t.PortOfLoading.Code.ToString()));

			AssertContainsExactElementsInAnyOrder(expectedRoutings, actualRoutings);
		}

		#endregion

		#region TestPopulateDataObject_TransportMode

		public void TestPopulateDataObject_TransportMode_Sea() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Sea, "Sea Freight");

		public void TestPopulateDataObject_TransportMode_Air() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Air, "Air Freight");

		public void TestPopulateDataObject_TransportMode_AirSea() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.AirSea, null);

		public void TestPopulateDataObject_TransportMode_Rail() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Rail, "Rail Freight");

		public void TestPopulateDataObject_TransportMode_Pedestrian() => TestPopulateDataObject_TransportMode(Core.Constants.TransportModes.Pedestrian, null);

		void TestPopulateDataObject_TransportMode(string transportMode, string outputDescription)
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			receiveASN.WRP_TransportMode = transportMode;

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);
			AssertEquals(transportMode, asnDataObject.TransportMode.Code);
			AssertEquals(outputDescription, asnDataObject.TransportMode.Description);

			receiveASN.WRP_TransportMode = "";
			var asnDataObjectWriterForEmptyTransportMode = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveASN)));
			var asnDataObjectForEmptyTransportMode = asnDataObjectWriterForEmptyTransportMode.GetDataObject(receiveASN);
			AssertNull("TransportMode property must be null.", asnDataObjectForEmptyTransportMode.TransportMode);
		}

		#endregion

		#region TestPopulateDataObject_AirCargo

		public void TestPopulateDataObject_AirCargoRequired()
		{
			var receiveASN = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			receiveASN.WRP_TransportMode = Core.Constants.TransportModes.Air;
			var masterBill = receiveASN.AdditionalReferenceNumbers.AddNew();
			masterBill.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			masterBill.CE_EntryNum = "111123";

			var voyageNumber = receiveASN.AdditionalReferenceNumbers.AddNew();
			voyageNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber;
			voyageNumber.CE_EntryNum = "VN123";

			var asnDataObjectWriter = new WhsTransitReceiveASNDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ARP, receiveASN)));
			var asnDataObject = asnDataObjectWriter.GetDataObject(receiveASN);
			AssertEquals(Core.Constants.TransportModes.Air, asnDataObject.TransportMode.Code);
			AssertEquals("111123", asnDataObject.WayBillNumber);
			AssertEquals("MWB", asnDataObject.WayBillType.Code);
			AssertEquals("VN123", asnDataObject.VoyageFlightNo);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTransitTestHelper helper => new WhsTransitTestHelper(Factory.BOFactory);

		#endregion
	}
}
