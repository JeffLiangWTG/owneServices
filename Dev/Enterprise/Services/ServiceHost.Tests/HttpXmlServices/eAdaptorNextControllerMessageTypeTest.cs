using System.Net;
using System.Net.Http.Headers;
using System.Web;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Services.ServiceHost.Tests
{
	class eAdaptorNextControllerMessageTypeTest : BaseEAdaptorControllerTest
	{
		protected eAdaptorSenderHelper sender;

		protected override void SetUp()
		{
			var controller = new eAdaptorNextController();
			sender = new eAdaptorSenderHelper(controller, controller.Post);
			base.SetUp();
		}

		(HttpStatusCode statusCode, HttpHeaders headers, string result) SendWithMessageType(string message, string messageType)
		{
			sender.Send = () => (sender.Controller as eAdaptorNextController).PostWithMessageType(messageType);
			return sender.SendRequest(message);
		}

		public void TestNext_PostWithMessageTypeUniversalEvent_200()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			var result = SendWithMessageType(xml, "UniversalEvent");

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.ProcessedOK);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_PostWithMessageTypeUniversalShipment_200()
		{
			var xml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			var result = SendWithMessageType(xml, "UniversalShipment");

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.ProcessedOK);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_PostWithMessageTypeNative_200()
		{
			var xml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var result = SendWithMessageType(xml, "Native");

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Received);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_PostWithMessageTypeUniversalEventIncorrectBody()
		{
			var xml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			var result = SendWithMessageType(xml, "UniversalEvent");

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.BadRequest, result.statusCode);

				var resultDecode = HttpUtility.HtmlDecode(result.result);
				AssertContains("<Status>PRS</Status>", resultDecode);
				AssertContains("<ProcessingLog>Error - Line 1: Invalid Root element found. Expected <UniversalEvent> but found <UniversalShipment> instead.</ProcessingLog>", resultDecode);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Rejected);
			});
		}

		public void TestNext_PostWithMessageType_400()
		{
			var fakeMessageType = "NotADoctorShhh!";
			var (statusCode, headers, content) = SendWithMessageType("", fakeMessageType);
			AssertEquals(HttpStatusCode.BadRequest, statusCode);
			AssertEquals($"{fakeMessageType} is not a valid message type", content);
		}
	}
}
