using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class InterchangeBuilderTest : TestCaseWithFactory
	{
		#region TestTryParse_Transmit

		public void TestTryParse_Transmit()
		{
			var interchangeText = string.Format(interchangeTemplate, universalShipment);

			var builder = new InterchangeBuilder(Factory);
			var res = builder.TryParseUniversalXml(interchangeText, InterchangeBuilder.MessageDirection.Transmit, out var interchange);

			AssertEquals("parsed valid xml", true, res);
			AssertNotNull("created interchange", interchange);

			CombineAssertions(() =>
			{
				AssertNotNull("interchange was created", interchange);
				AssertEquals(nameof(interchange.EI_From), Env.CurrentCompany.GetLicenceCode(), interchange.EI_From);
				AssertEquals(nameof(interchange.EI_To), "eBooking API", interchange.EI_To);
				AssertEquals(nameof(interchange.EI_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals(nameof(interchange.EI_InterchangeType), EDIMessageTypeList.Codes.XMS, interchange.EI_InterchangeType);
				AssertEquals(nameof(interchange.EI_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals(nameof(interchange.EI_GB), GlbBranch.CurrentBranch.PK, interchange.EI_GB);
				AssertEquals(nameof(interchange.EI_Status), EDIInterchangeStatusList.Codes.Sent, interchange.EI_Status);
				AssertEquals(nameof(interchange.EI_TransportType), EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
				AssertMultilineASCIIEquals(nameof(interchange.EI_BodyText), interchangeText, interchange.EI_BodyText);
			});

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<IEDIMessage>(messageQuery);

			AssertEquals("one message was created", 1, messages.Length);

			var message = messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(nameof(message.EM_MessageType), EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals(nameof(message.EM_IsTestMessage), false, message.EM_IsTestMessage);
				AssertEquals(nameof(message.EM_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals(nameof(message.EM_MessageSubType), EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals(nameof(message.EM_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals(nameof(message.EM_GB), GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertEquals(nameof(message.EM_GE), GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(nameof(message.EM_Status), EDIInterchangeStatusList.Codes.Sent, message.EM_Status);
				AssertMultilineASCIIEquals(nameof(message.EM_MessageText), universalShipment, message.EM_MessageText);
			});
		}

		#endregion

		#region TestTryParse_Receive

		public void TestTryParse_Receive()
		{
			var interchangeText = string.Format(interchangeTemplate, universalShipment);

			var builder = new InterchangeBuilder(Factory);
			var res = builder.TryParseUniversalXml(interchangeText, InterchangeBuilder.MessageDirection.Receive, out var interchange);

			AssertEquals("parsed valid xml", true, res);
			AssertNotNull("created interchange", interchange);

			CombineAssertions(() =>
			{
				AssertNotNull("interchange was created", interchange);
				AssertEquals(nameof(interchange.EI_From), "eBooking API", interchange.EI_From);
				AssertEquals(nameof(interchange.EI_To), Env.CurrentCompany.GetLicenceCode(), interchange.EI_To);
				AssertEquals(nameof(interchange.EI_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals(nameof(interchange.EI_InterchangeType), EDIMessageTypeList.Codes.XMS, interchange.EI_InterchangeType);
				AssertEquals(nameof(interchange.EI_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals(nameof(interchange.EI_GB), GlbBranch.CurrentBranch.PK, interchange.EI_GB);
				AssertEquals(nameof(interchange.EI_Status), EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals(nameof(interchange.EI_TransportType), EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
				AssertMultilineASCIIEquals(nameof(interchange.EI_BodyText), interchangeText, interchange.EI_BodyText);
			});

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<IEDIMessage>(messageQuery);

			AssertEquals("one message was created", 1, messages.Length);

			var message = messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(nameof(message.EM_MessageType), EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals(nameof(message.EM_IsTestMessage), false, message.EM_IsTestMessage);
				AssertEquals(nameof(message.EM_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals(nameof(message.EM_MessageSubType), EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals(nameof(message.EM_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals(nameof(message.EM_GB), GlbBranch.CurrentBranch.PK, message.EM_GB);
				AssertEquals(nameof(message.EM_GE), GlbDepartment.CurrentDepartment.PK, message.EM_GE);
				AssertEquals(nameof(message.EM_Status), EDIInterchangeStatusList.Codes.Received, message.EM_Status);
				AssertMultilineASCIIEquals(nameof(message.EM_MessageText), universalShipment, message.EM_MessageText);
			});
		}

		#endregion

		#region TestTryParse_Receive_Multiple

		public void TestTryParse_Receive_Multiple()
		{
			var interchangeText = string.Format(interchangeTemplate, universalShipment + universalEvent);

			var builder = new InterchangeBuilder(Factory);
			var res = builder.TryParseUniversalXml(interchangeText, InterchangeBuilder.MessageDirection.Receive, out var interchange);

			AssertEquals("parsed valid xml", true, res);
			AssertNotNull("created interchange", interchange);

			CombineAssertions(() =>
			{
				AssertNotNull("interchange was created", interchange);
				AssertEquals(nameof(interchange.EI_From), "eBooking API", interchange.EI_From);
				AssertEquals(nameof(interchange.EI_To), Env.CurrentCompany.GetLicenceCode(), interchange.EI_To);
				AssertEquals(nameof(interchange.EI_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals(nameof(interchange.EI_InterchangeType), EDIMessageTypeList.Codes.XMS, interchange.EI_InterchangeType);
				AssertEquals(nameof(interchange.EI_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals(nameof(interchange.EI_GB), GlbBranch.CurrentBranch.PK, interchange.EI_GB);
				AssertEquals(nameof(interchange.EI_Status), EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals(nameof(interchange.EI_TransportType), EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
				AssertMultilineASCIIEquals(nameof(interchange.EI_BodyText), interchangeText, interchange.EI_BodyText);
			});

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<IEDIMessage>(messageQuery);

			AssertEquals("two messages was created", 2, messages.Length);

			var message1 = messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(nameof(message1.EM_MessageType), EDIMessageTypeList.Codes.XDC, message1.EM_MessageType);
				AssertEquals(nameof(message1.EM_IsTestMessage), false, message1.EM_IsTestMessage);
				AssertEquals(nameof(message1.EM_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, message1.EM_ApplicationCode);
				AssertEquals(nameof(message1.EM_MessageSubType), EDIMessageSubTypeList.Codes.XmlUniversalShipment, message1.EM_MessageSubType);
				AssertEquals(nameof(message1.EM_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Receive, message1.EM_ReceiveTransmit);
				AssertEquals(nameof(message1.EM_GB), GlbBranch.CurrentBranch.PK, message1.EM_GB);
				AssertEquals(nameof(message1.EM_GE), GlbDepartment.CurrentDepartment.PK, message1.EM_GE);
				AssertEquals(nameof(message1.EM_Status), EDIInterchangeStatusList.Codes.Received, message1.EM_Status);
				AssertMultilineASCIIEquals(nameof(message1.EM_MessageText), universalShipment, message1.EM_MessageText);
			});

			var message2 = messages[1];

			CombineAssertions(() =>
			{
				AssertEquals(nameof(message2.EM_MessageType), EDIMessageTypeList.Codes.XDC, message2.EM_MessageType);
				AssertEquals(nameof(message2.EM_IsTestMessage), false, message2.EM_IsTestMessage);
				AssertEquals(nameof(message2.EM_ApplicationCode), ApplicationCodeList.Codes.UniversalDataMessaging, message2.EM_ApplicationCode);
				AssertEquals(nameof(message2.EM_MessageSubType), EDIMessageSubTypeList.Codes.XmlUniversalEvent, message2.EM_MessageSubType);
				AssertEquals(nameof(message2.EM_ReceiveTransmit), EDICommunicationsModeCommsDirectionList.Codes.Receive, message2.EM_ReceiveTransmit);
				AssertEquals(nameof(message2.EM_GB), GlbBranch.CurrentBranch.PK, message2.EM_GB);
				AssertEquals(nameof(message2.EM_GE), GlbDepartment.CurrentDepartment.PK, message2.EM_GE);
				AssertEquals(nameof(message2.EM_Status), EDIInterchangeStatusList.Codes.Received, message2.EM_Status);
				AssertMultilineASCIIEquals(nameof(message2.EM_MessageText), universalEvent, message2.EM_MessageText);
			});
		}

		#endregion

		#region TestTryParse_InvalidXml

		public void TestTryParse_InvalidXml()
		{
			var interchangeText = string.Format(interchangeTemplate, universalShipment);

			var builder = new InterchangeBuilder(Factory);
			var res = builder.TryParseUniversalXml("invalid xml", InterchangeBuilder.MessageDirection.Transmit, out var interchange);

			AssertEquals("did not parse invalid xml", false, res);
			AssertNull("did not create interchange", interchange);
		}

		#endregion

		const string interchangeTemplate =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>HYEBNEUAT</SenderID>
	</Header>
	<Body>
		{0}
	</Body>
</UniversalInterchange>";

		const string universalEvent =
@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Event>
    <DataContext>
      <DocumentaryOverride>
        <DocumentName>AirBookingRequest</DocumentName>
      </DocumentaryOverride>
      <DataTargetCollection>
        <DataTarget>
          <Key>CCN1406309</Key>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventType>IRJ</EventType>
    <EventParameters>
      <Department>WiseTech Global</Department>
      <MessageType>Air Booking</MessageType>
      <ReferenceNumber>618-73808291</ReferenceNumber>
      <Reason>[ERROR MESSAGE]</Reason>
    </EventParameters>
    <EventTime>2020-04-08T10:25:21</EventTime>
    <DataContext />
  </Event>
</UniversalEvent>";

		const string universalShipment =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>CHAMDDEE04A000686457</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>
    <BookingConfirmationReference>CHAMDDEE04A000686457</BookingConfirmationReference>
    <PortOfOrigin>HAM</PortOfOrigin>
    <PortOfDestination>HKG</PortOfDestination>
  </Shipment>
</UniversalShipment>";
	}
}
