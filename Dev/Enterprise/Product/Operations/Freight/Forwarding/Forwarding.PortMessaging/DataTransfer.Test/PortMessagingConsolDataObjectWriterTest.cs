using System.Linq;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer.Testing
{
	sealed class PortMessagingConsolDataObjectWriterTest : ConsolDataObjectWriterTest
	{
		public void TestExportEORICode_SendingForwarder()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234", Constants.CountryCodes.Germany);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var writer = new PortMessagingConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var consolDataObject = writer.GetDataObject(consol);

			AssertNotNull("Expected to have a consol data object created", consolDataObject);

			var organizationAddress = consolDataObject.OrganizationAddressCollection.Find(address => address.AddressType.GetValueOrDefault() == AddressTypes.SendingForwarderAddress);
			AssertNotNull("Expected to export the consol's SendingForwarder", organizationAddress);

			AssertEquals("Expected the Eori number to be included with the consol", 1, organizationAddress.RegistrationNumberCollection.Count);

			var registrationNumber = organizationAddress.RegistrationNumberCollection[0];
			AssertEquals("Expected the Eori number to be included with the consol", "EOR", registrationNumber.Type.Code.GetValueOrDefault());
			AssertEquals("Expected the Eori number to be included with the consol", "1234", registrationNumber.Value.GetValueOrDefault());
			AssertEquals("Expected the Eori number to be included with the consol", "DE", registrationNumber.CountryOfIssue.Code.GetValueOrDefault());
		}

		public void TestConsolPortMessagingRemarksNotes()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.PortMessageRemarks.Description, "Port Messaging Remarks Note is carried though ja.");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "This note should not appear in port messaging");

			var writer = new PortMessagingConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var consolDataObject = writer.GetDataObject(consol);

			AssertNotNull("Expected to have a consol data object created", consolDataObject);
			AssertNotNull("Expected consol data object to have a note collection", consolDataObject.NoteCollection);
			AssertEquals("Expected both notes to be included with the consol", 2, consolDataObject.NoteCollection.Count);

			AssertNotNull("Expected consol data object to have a Port Messaging", consolDataObject.PortMessaging);
			AssertEquals("Expected only the right type of note was included in the Port Messaging Remarks", "Port Messaging Remarks Note is carried though ja.", consolDataObject.PortMessaging.Remarks);
		}

		public void TestCancellationReasonNote()
		{
			AssertCancellationReason(PortMessagingManager.MessageType.PortOrderWithHDSCancellationBecauseOfErrors, "CancellationBecauseOfErrors");
			AssertCancellationReason(PortMessagingManager.MessageType.PortOrderWithHDSCancellationOnExit, "CancellationOnExit");
			AssertCancellationReason(PortMessagingManager.MessageType.PortOrderWithHDSForwardingCancellation, "ForwardingCancellation");
			AssertCancellationReason(PortMessagingManager.MessageType.PortOrderWithHDS, null);
			AssertCancellationReason(PortMessagingManager.MessageType.GatePass, null);
		}

		void AssertCancellationReason(PortMessagingManager.MessageType messageType, string expectedNote)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var writer = new PortMessagingConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), messageType, "Some Purpose");
			var dataObject = writer.GetDataObject(consol);

			var note = dataObject.NoteCollection != null
				? dataObject.NoteCollection.FirstOrDefault(x => x.Description.HasValue && x.Description.Value == "DAKOSYPortMessageCancellationType")
				: null;

			if (expectedNote == null)
			{
				AssertNull("Should not create cancellation note", note);
			}
			else
			{
				AssertNotNull("Should create cancellation note", note);
				AssertEquals(expectedNote, note.NoteText.Value);
			}
		}

		public void TestPackingLine_ShipmentHasRelatedShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CON1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON2";
			var bcnShipment = consol.Shipments.AddNew();
			var subShipment = consol.Shipments.AddNew();

			bcnShipment.JS_ShipmentType = "BCN";
			bcnShipment.CoLoadShipments.Add(subShipment);

			var bcnShipPackLine = bcnShipment.OuterPackLines.AddNew();
			bcnShipPackLine.SetContainer(container1.PK);
			bcnShipPackLine.JL_PackageCount = 3;

			var subShipPackLine = subShipment.OuterPackLines.AddNew();
			subShipPackLine.SetContainer(container2.PK);
			subShipPackLine.JL_PackageCount = 6;

			var writer = new PortMessagingConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consol)), PortMessagingManager.MessageType.PortOrderWithHDS, "Some Purpose");
			var consolDataObject = writer.GetDataObject(consol);

			AssertNotNull("Expected to have a consol data object created", consolDataObject);
			AssertNotNull(consolDataObject.SubShipmentCollection);
			AssertEquals(1, consolDataObject.SubShipmentCollection.Count);

			var subShipmentDataObject = consolDataObject.SubShipmentCollection[0];
			AssertNotNull(subShipmentDataObject);
			AssertEquals("Included both packingline from bcn and sub", 2, subShipmentDataObject.PackingLineCollection.Count);

			var packLine1Data = subShipmentDataObject.PackingLineCollection.First(line => line.ContainerNumber.Value == "CON1");
			var packLine2Data = subShipmentDataObject.PackingLineCollection.First(line => line.ContainerNumber.Value == "CON2");
			AssertEquals(3, packLine1Data.PackQty.Value);
			AssertEquals(6, packLine2Data.PackQty.Value);
		}
	}
}
