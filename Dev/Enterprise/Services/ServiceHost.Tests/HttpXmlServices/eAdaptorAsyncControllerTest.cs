using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static NUnit.Framework.XmlAssertions;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class eAdaptorAsyncControllerTest : TestCaseWithFactory
	{
		eAdaptorSenderHelper sender;

		protected IEDICommunicationPartyConfig config;
		protected IDisposable disposableMessagingContext;
		Mock<IMessagingContext> mockMessagingContext;
		GlbStaff partyConfigSecurityProxy;

		protected override void SetUp()
		{
			var controller = new eAdaptorNextController();
			sender = new eAdaptorSenderHelper(controller, controller.PostAsynchronous);
			config = AuthenticationTestHelper.SetUpBasicAuthenticationUser(string.Empty, string.Empty);
			config = AuthenticationTestHelper.SetConfigBranchDepComp(config);
			partyConfigSecurityProxy = AuthenticationTestHelper.SetSeurityProxyUser(config);
			mockMessagingContext = new Mock<IMessagingContext>();
			mockMessagingContext.SetupGet(c => c.CurrentInboundConfig).Returns(config);
			disposableMessagingContext = ObjectFactory.Substitute(mockMessagingContext.Object);

			base.SetUp();
		}

		protected override void TearDown()
		{
			disposableMessagingContext.Dispose();
			base.TearDown();
		}

		string PostAsyncRequest(string requestXml, HttpStatusCode assertStatusCode = HttpStatusCode.OK)
		{
			var (statusCode, headers, result) = sender.SendRequest(requestXml);
			AssertEquals("HttpStatusCode differs from expected", assertStatusCode, statusCode);
			return result;
		}

		void AssertUniversalResponse(string status, string shouldContain, string actualResponse)
		{
			AssertContains($"<Status>{status}", actualResponse);
			AssertContains(shouldContain, actualResponse);
		}

		void AssertExternalReferenceNumberForRequestAndResponse(string response, string externalNum)
		{
			AssertNotNull("Request message with correct EM_ExternalReferenceNumber", Factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive).AddToFilter(EDIMessageSchema.EM_ExternalReferenceNumber, externalNum)));

			AssertIsXml(response)
			.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "External").WithValue(externalNum))
			.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection", collection => collection
				.HavingExactlyOneChildNode("MessageNumber", node => node.WithAttribute("Type", "External"))
				.HavingExactlyOneChildNode("MessageNumber", node => node
					.WithValue(externalNum)
					.WithAttribute("Type", "External")
				)
			);
		}

		#region Successful Processing Cases

		void AssertUniversalShipmentImportResults(string postResponse)
		{
			var senderID = "ABCDEFGHI";
			var recipientID = "EDIDEMDAT";
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			CombineAssertions(delegate
			{
				AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals(senderID, interchange.EI_From);
				AssertEquals(recipientID, interchange.EI_To);
				AssertEquals(EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals(ReceiveTransmitList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals(EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
			});

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];
			CombineAssertions(delegate
			{
				AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals(EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals(ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals(EDIInterchangeTransportTypeList.Codes.eAdaptor, message.EM_TransportType);

				AssertContains("<Key>S00001117</Key>", message.EM_MessageText);
			});

			CombineAssertions(delegate
			{
				AssertIsXml(postResponse)
					.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "TrackingID").WithValue(interchanges[0].EI_SessionGUID.ToString()))
					.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "InterchangeNumber").WithValue(interchanges[0].EI_InterchangeNum.ToString()))
					.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[0].EM_MessageNum))
					.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "TrackingID").WithValue(interchanges[0].EI_SessionGUID.ToString()))
					.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "InterchangeNumber").WithValue(interchanges[0].EI_InterchangeNum.ToString()))
					.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[0].EM_MessageNum));
			});
		}

		public void TestSendUniversalInterchangeXML()
		{
			var response = PostAsyncRequest(GetValidUniversalInterchangeWithXUSMessage("http://www.cargowise.com/Schemas/Universal/2011/11"));
			AssertUniversalShipmentImportResults(response);
		}

		public void TestUniversalInterchangeWithNative()
		{
			var senderID = "ABCDEFGHI";
			var recipientID = "EDIDEMDAT";
			var universalInterchangeWithNativeXml = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Native#UniversalInterchange"" version=""1.1"">
  <Header>
    <SenderID>{senderID}</SenderID>
    <RecipientID>{recipientID}</RecipientID>
  </Header>
  <Body>
	<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
	  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
		<OwnerCode>EDIDATEDI</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	  </Header>
	  <Body>
		<Organization version=""1.0"">
		  <OrgHeader Action=""MERGE"">
			<FullName>JR</FullName>
			<ClosestPort TableName=""RefUNLOCO"">
				<Code>BGSOF</Code>
			</ClosestPort>
		  </OrgHeader>
		</Organization>
	  </Body>
	</Native>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(universalInterchangeWithNativeXml);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			CombineAssertions(delegate
			{
				AssertEquals("Native application code should be NDM", ApplicationCodeList.Codes.NativeDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals(senderID, interchange.EI_From);
				AssertEquals(recipientID, interchange.EI_To);
				AssertEquals(EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals(ReceiveTransmitList.Codes.Receive, interchange.EI_ReceiveTransmit);
				AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
				AssertEquals(EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
			});

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];
			CombineAssertions(delegate
			{
				AssertEquals("Native application code should be NDM", ApplicationCodeList.Codes.NativeDataMessaging, message.EM_ApplicationCode);
				AssertEquals(EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals(EDIMessageSubTypeList.Codes.XmlNativeOrganization, message.EM_MessageSubType);
				AssertEquals(ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			});
		}

		public void TestSendUniversalInterchangeXMLWithServiceTask()
		{
			var requestXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipment>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestXml);

			AssertContains("Message Response", "<MessageNumber Type=\"MessageNumber\">", response);

			var serviceTask = new UniversalDataBuss.ServiceTasks.UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertEquals("Message should be processed by UMI", "PRS", messages[0].EM_Status);
			AssertEquals("SystemLastEditUser should be set to PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, messages[0].EM_SystemLastEditUser);
		}

		public void TestInterchangeWithMultipleMessages()
		{
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals(0, interchanges.Length);

			var requestXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipment>

	<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipment>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestXml);

			interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals(1, interchanges.Length);
			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchanges[0].PK));
			AssertEquals(2, messages.Length);

			AssertIsXml(response)
				.HavingExactlyOneChildNode("MessageNumberCollection", collection => collection
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "TrackingID").WithValue(interchanges[0].EI_SessionGUID.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "InterchangeNumber").WithValue(interchanges[0].EI_InterchangeNum.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[0].EM_MessageNum))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[1].EM_MessageNum))
				)
				.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection", collection => collection
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber"))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "TrackingID").WithValue(interchanges[0].EI_SessionGUID.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "InterchangeNumber").WithValue(interchanges[0].EI_InterchangeNum.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[0].EM_MessageNum))
				)
				.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection", collection => collection
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber"))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "TrackingID").WithValue(interchanges[0].EI_SessionGUID.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "InterchangeNumber").WithValue(interchanges[0].EI_InterchangeNum.ToString()))
					.HavingExactlyOneChildNode("MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(messages[1].EM_MessageNum))
				);
		}

		public void TestGetAsynchronous()
		{
			var result = string.Empty;
			using (var controller = new eAdaptorNextController())
			using (var response = controller.GetAsynchronous())
			{
				result = response.Content.ReadAsStringAsync().Result;
			}

			AssertContains("Message Response", "Welcome to the eAdaptor Next Async Service", result);
		}

		public void TestCustomHeaders()
		{
			var (statusCode, headers, result) = sender.SendRequest(GetValidUniversalInterchangeWithXUSMessage("http://www.cargowise.com/Schemas/Universal/2011/11"));
			var interchange = Factory.LoadTop1<IEDIInterchange>(new ZQuery());
			Assert("Expect containing the header", headers.TryGetValues("eAdaptor-InterchangeNumber", out var headerValue));
			AssertEquals("Expect the value of the header", interchange.EI_InterchangeNum, headerValue.FirstOrDefault());
		}

		public void TestUniversalInterchangeWithExternalReferenceNumber()
		{
			var externalNum = "8a64619e-9a9b-40b8-aa52-330511287dc6";
			var requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   		<Event>
			<DataContext>
				<DataTargetCollection>
 					<DataTarget>
						<Type>ForwardingShipment</Type>
						<Key>S00001000</Key>
					</DataTarget>
				</DataTargetCollection>
			</DataContext>
			<EventTime>2022-12-06T12:12:00</EventTime>
			<EventType>ARV</EventType>
			<MessageNumberCollection>
				<MessageNumber Type=""External"">{externalNum}</MessageNumber>
      		</MessageNumberCollection>
		</Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestXml);

			AssertExternalReferenceNumberForRequestAndResponse(response, externalNum);
		}

		public void TestUniversalInterchangeWithMulitpleExternalReferenceNumbers()
		{
			var externalNum = "8a64619e-9a9b-40b8-aa52-330511287dc6";
			var externalNum2 = "fa4782c6-1dda-4cd5-a867-ab6ec7d701d4";
			var requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   		<Event>
			<DataContext>
				<DataTargetCollection>
 					<DataTarget>
						<Type>ForwardingShipment</Type>
						<Key>S00001000</Key>
					</DataTarget>
				</DataTargetCollection>
			</DataContext>
			<EventTime>2022-12-06T12:12:00</EventTime>
			<EventType>ARV</EventType>
			<MessageNumberCollection>
				<MessageNumber Type=""External"">{externalNum}</MessageNumber>
      		</MessageNumberCollection>
		</Event>
    </UniversalEvent>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   		<Event>
			<DataContext>
				<DataTargetCollection>
 					<DataTarget>
						<Type>ForwardingShipment</Type>
						<Key>S00001001</Key>
					</DataTarget>
				</DataTargetCollection>
			</DataContext>
			<EventTime>2022-12-06T12:11:00</EventTime>
			<EventType>ARV</EventType>
			<MessageNumberCollection>
				<MessageNumber Type=""External"">{externalNum2}</MessageNumber>
      		</MessageNumberCollection>
		</Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestXml);

			AssertExternalReferenceNumberForRequestAndResponse(response, externalNum);
			AssertExternalReferenceNumberForRequestAndResponse(response, externalNum2);
		}
		#endregion

		#region Failure Cases

		public void TestMissingHeader()
		{
			var xmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlMessage, HttpStatusCode.BadRequest);

			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - XML does not contain a valid Header element.", response);
		}

		public void TestNoSenderID()
		{
			var xmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlMessage, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Header does not contain a valid SenderID.", response);
		}

		public void TestNoRecipientID()
		{
			var xmlWithNoSenderID = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
  </Header>
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlWithNoSenderID, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Header does not contain a valid RecipientID.", response);
		}

		public void TestEmptySenderID()
		{
			var xmlMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <RecipientID>EDIDEMDAT</RecipientID>
	<SenderID></SenderID>
  </Header>
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlMessage, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Header does not contain a valid SenderID.", response);
		}

		public void TestEmptyRecipientID()
		{
			var xmlWithNoSenderID = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
	<RecipientID></RecipientID>
  </Header>
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlWithNoSenderID, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Header does not contain a valid RecipientID.", response);
		}

		public void TestRecipientIDHasToBe9Chars()
		{
			var xmlWithNoSenderID = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
	<RecipientID>ABC</RecipientID>
  </Header>
</UniversalInterchange>";
			var response = PostAsyncRequest(xmlWithNoSenderID, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - RecipientID should be 9 characters in length", response);
		}

		public void TestRecipientIDDoesNotContainAValidCompany()
		{
			var xmlWithNoSenderID = GetValidUniversalInterchangeWithXUSMessage("http://www.cargowise.com/Schemas/Universal/2011/11")
				.Replace("<RecipientID>EDIDEMDAT</RecipientID>", "<RecipientID>AQQBQQCQQ</RecipientID>");
			var response = PostAsyncRequest(xmlWithNoSenderID, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "no company with this code found in the database", response);
		}

		public void TestUnsupportedRootElement()
		{
			var xmlMessage = @"
<ns0:TelematicsInterchange xmlns:ns0=""http://cargowise.com/ehub/products/telematics/2013/05"">
  <Header>
    <SenderID>TELEMATIC</SenderID>
    <RecipientID>TELEMATIC</RecipientID>
  </Header>
  <Body>
    <ProtobufData>CAEQARpDH4sIAAAAAAAEANNiYGDgSBFS4xI4tXLy3hNnEl2an8d/CZmbclGIMzMgIz8vVcE/WIm9wsIs3syEAQAKS62WLgAAAA==</ProtobufData>
  </Body>
</ns0:TelematicsInterchange>";

			var response = PostAsyncRequest(xmlMessage, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Message contains an unsupported element TelematicsInterchange", response);
		}

		public void TestInvalidXml()
		{
			var xmlMessage = @"?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
	<RecipientID>ABC</RecipientID>
  </Header>
</UniversalInterchange>";

			var response = PostAsyncRequest(xmlMessage, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "Error - Invalid XML.", response);
		}

		public void TestUniversalInterchangeStatusFailed()
		{
			var universalInterchangeWithNativeXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Native#UniversalInterchange"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<Apple xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
	  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
		<OwnerCode>EDIDATEDI</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	  </Header>
	  <Body>
		<Organization version=""1.0"">
		  <OrgHeader Action=""MERGE"">
			<FullName>JR</FullName>
			<ClosestPort TableName=""RefUNLOCO"">
				<Code>BGSOF</Code>
			</ClosestPort>
		  </OrgHeader>
		</Organization>
	  </Body>
	</Native>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(universalInterchangeWithNativeXml, HttpStatusCode.BadRequest);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);

			AssertContains("Status should be error", "<Status>ERR</Status>", response);
			AssertContains("Response should contain the ", "The 'Apple' start tag", response);
		}

		public void TestUniversalInterchangeStatusFailedAndNoEdiMessageIsCreated_FailureCausedByException()
		{
			void CheckNoEDIMessageWasCreated()
			{
				var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, ediMessages.Length);
			}

			CheckNoEDIMessageWasCreated();
			var universalInterchangeWithNativeXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Native#UniversalInterchange"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
	  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
		<OwnerCode>EDIDATEDI</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	  </Header>
	  <Body>
		<Wrong version=""1.0"">
		  <OrgHeader Action=""MERGE"">
			<FullName>JR</FullName>
			<ClosestPort TableName=""RefUNLOCO"">
				<Code>BGSOF</Code>
			</ClosestPort>
		  </OrgHeader>
		</Wrong>
	  </Body>
	</Native>
  </Body>
</UniversalInterchange>";

			PostAsyncRequest(universalInterchangeWithNativeXml, HttpStatusCode.BadRequest);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			CheckNoEDIMessageWasCreated();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipmentRequest>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.BadRequest);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			AssertContains("Status should be error", "<Status>ERR</Status>", response);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeWithoutOutboundConfig_GeneratesWarningProcessingLog()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];

			AssertNull(Factory.Load<EDICommunicationPartyConfig>(interchange.EI_ECC_CommunicationPartyConfig).Party.Configs.Cast<EDICommunicationPartyConfig>().FirstOrDefault(c => c.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound && c.ECC_IsActive));
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);
			AssertContains("Warning - Acknowledgement message requested, but acknowledgement will not be generated as outbound configuration is missing.", response);
		}

		public void TestUniversalInterchangeWithOutboundConfig_DoesNotGenerateWarningProcessingLog()
		{
			var outboundConfig = AuthenticationTestHelper.AddOutboundConfiguration(config.Party);

			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];

			AssertNotNull(Factory.Load<EDICommunicationPartyConfig>(interchange.EI_ECC_CommunicationPartyConfig).Party.Configs.Cast<EDICommunicationPartyConfig>().FirstOrDefault(c => c.PK == outboundConfig.PK));
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);
			AssertNotContains("Warning - Acknowledgement message requested, but acknowledgement will not be generated as outbound configuration is missing.", response);
		}

		public void TestSuccessfulInterchangeProcessing()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipment>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);
			AssertEquals(1, ediMessages.Length);

			AssertEquals(config.PK, interchange.EI_ECC_CommunicationPartyConfig);
			AssertEquals(config.PK, ediMessages[0].EM_ECC_CommunicationPartyConfig);
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipmentRequest>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
	  </Shipment>
	</UniversalShipment>
  </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags_RequestTagAppearsLast()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001411</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDA</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001412</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>Z01</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(2, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags_AllRequests()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001411</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDA</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001412</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>Z01</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.BadRequest);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>ERR</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags_RequestsAroundEvent()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001411</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDA</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001412</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>Z01</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags_TwoRequestTagAppearsLast()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001411</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDA</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001412</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>Z01</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestUniversalInterchangeInvalidRequestFailed_MultipleUniversalTags_TwoRequestTagAppearsFirst()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001411</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDA</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>
        <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001412</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>Z01</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalEvent>
    </Body>
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.OK);
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertContains("Status should be Processed", "<Status>PRS</Status>", response);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, ediMessages.Length);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestResponseNoStackTrace()
		{
			var requestMessageXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
    </Header>
    <Body>
        <UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
            <Event>
                <DataContext>
                    <DataTargetCollection>
                        <DataTarget>
                            <Type>ForwardingShipment</Type>
                            <Key>S00001410</Key>
                        </DataTarget>
                    </DataTargetCollection>
                </DataContext>
                <EventType>DDI</EventType>
                <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
                <IsEstimate>False</IsEstimate>
            </Event>
        </UniversalShipmentRequest>'||'
</UniversalInterchange>";

			var response = PostAsyncRequest(requestMessageXml, HttpStatusCode.BadRequest);
			AssertUniversalResponse(UniversalResponseStatus.Error, "does not match the end tag of", response);
			Assert(!ContainsStackTrace(response));
			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			var failureLogNote = ((EDIInterchange)interchange).Notes.FindByDescription("Failure Log")?.MaxBySafe(stmNote => stmNote.ST_CreatedDateUtc)?.ST_NoteDataAsText;
			Assert(!ContainsStackTrace(failureLogNote));
		}

		bool ContainsStackTrace(string input)
		{
			var pattern = @"\bat\s+\w+.*\s+in\s+.*:\s*line\s+\d+";

			return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
		}
		#endregion

		#region Xml Samples

		string GetValidUniversalInterchangeWithXUSMessage(string schemaName)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?><UniversalInterchange xmlns=""{schemaName}"" version=""1.0"">
  <Header>
    <SenderID>ABCDEFGHI</SenderID>
    <RecipientID>EDIDEMDAT</RecipientID>
  </Header>
  <Body>
	  <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001117</Key>
        </DataSource>
      </DataSourceCollection>
      <DataTargetCollection>
	<DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001117</Key>
	</DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DUS</Code>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Name>U.S. Demo Company</Name>
      </Company>
      <EnterpriseID>HYE</EnterpriseID>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2012-01-10T10:47:22.067</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
    </DataContext>

    <ActualChargeable>1500.000</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LSE</Code>
      <Description>Loose</Description>
    </ContainerMode>
    <DocumentedChargeable>1500.000</DocumentedChargeable>
    <DocumentedVolume>0.000</DocumentedVolume>
    <DocumentedWeight>1500.000</DocumentedWeight>
    <FreightRate>0.0000</FreightRate>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>USD</Code>
      <Description>United States of America, Dollars</Description>
    </GoodsValueCurrency>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <InsuranceValue>0.0000</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>USD</Code>
      <Description>United States of America, Dollars</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsBuyersConsolMaster>false</IsBuyersConsolMaster>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsCoload>false</IsCoload>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsSellersConsolMaster>false</IsSellersConsolMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <JobCosting>
      <Branch>
        <Code>CHI</Code>
        <Name>Chicago</Name>
      </Branch>
      <AccrualNotRecognized>0</AccrualNotRecognized>
      <AccrualRecognized>0</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Currency>
        <Code>USD</Code>
        <Description>United States of America, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>0</TotalAccrual>
      <TotalCost>0</TotalCost>
      <TotalJobProfit>0</TotalJobProfit>
      <TotalRevenue>0</TotalRevenue>
      <TotalWIP>0</TotalWIP>
      <WIPNotRecognized>0</WIPNotRecognized>
      <WIPRecognized>0</WIPRecognized>
    </JobCosting>
    <ManifestedChargeable>1500.000</ManifestedChargeable>
    <ManifestedVolume>0.000</ManifestedVolume>
    <ManifestedWeight>1500.000</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code>AUALX</Code>
      <Name>Alexandria</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USORD</Code>
      <Name>O'Hare Apt/Chicago</Name>
    </PortOfOrigin>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>1500.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>AIR</Code>
      <Description>Air Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber>S00001117</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryEquipmentNeeded>
        <Code>PSL</Code>
        <Description>Premise Supplies Lift</Description>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupEquipmentNeeded>
        <Code>PSL</Code>
        <Description>Premise Supplies Lift</Description>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2011-07-16T13:00:00</Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>
";
		}

		#endregion

		#region Messages Without Interchange

		public void TestSendUniversalShipmentXMLWithoutInterchangeProcessedOK()
		{
			var requestXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
   <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Shipment>
	<DataContext>
		<DataTargetCollection>
		<DataTarget>
			<Type>ForwardingShipment</Type>
		</DataTarget>
		</DataTargetCollection>
	</DataContext>
	<ShipmentType>
		<Code>AGT</Code>
		<Description>Agent</Description>
	</ShipmentType>
	<TransportMode>
		<Code>SEA</Code>
		<Description>Sea Freight</Description>
	</TransportMode>
	</Shipment>
</UniversalShipment>";

			AssertMessageWithNoInterchangeQueued(requestXml);

			var serviceTask = new UniversalDataBuss.ServiceTasks.UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();

			var messages = new BusinessObjectFactory().Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertEquals("Message should be processed by UMI", EDIMessageStatusList.Codes.ProcessedOK, messages[0].EM_Status);
		}

		public void TestSendUniversalEventWithoutInterchangeRecieved()
		{
			var requestXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
 				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S00001000</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2022-12-06T12:12:00</EventTime>
		<EventType>ARV</EventType>
		<MessageNumberCollection>
			<MessageNumber Type=""External"">630976cb-a852-4f95-ad6c-539b576ada3b</MessageNumber>
		</MessageNumberCollection>
	</Event>
</UniversalEvent>";

			AssertMessageWithNoInterchangeQueued(requestXml, true);
		}

		void AssertMessageWithNoInterchangeQueued(string requestXml, bool hasExternalNumber = false)
		{
			var response = PostAsyncRequest(requestXml);

			var xmlAssertion = AssertIsXml(response).WithName("UniversalResponse")
				.HavingExactlyOneChildNode("Status", st => st.WithValue("PRS"));

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			var message = messages[0];
			AssertEquals("Message should be Queued", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals(EDIInterchangeTransportTypeList.Codes.eAdaptor, message.EM_TransportType);

			var interchanges = Factory.Load<IEDIInterchange>(new ZQuery());
			AssertEquals("No EDI Interchange should be created", 0, interchanges.Length);

			xmlAssertion.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "MessageNumber").WithValue(message.EM_MessageNum))
				.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection", collection => collection
					.HavingExactlyOneChildNode("MessageNumber", node => node.WithAttribute("Type", "MessageNumber").WithValue(message.EM_MessageNum)));

			if (hasExternalNumber)
			{
				xmlAssertion.HavingExactlyOneChildNode("MessageNumberCollection/MessageNumber", n => n.WithAttribute("Type", "External").WithValue(message.EM_ExternalReferenceNumber))
					.HavingExactlyOneChildNode("MessageNumberCollections/MessageNumberCollection", collection => collection
						.HavingExactlyOneChildNode("MessageNumber", node => node.WithAttribute("Type", "External").WithValue(message.EM_ExternalReferenceNumber)));
			}
		}

		public void TestXmlMessageWithIdentifierWrapsInInterchange()
		{
			var requestXml = @"<?xml version=""1.0"" encoding=""utf-8""?><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1""><Event>
		<DataContext>
			<DataTargetCollection>
 				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S00001000</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2022-12-06T12:12:00</EventTime>
		<EventType>ARV</EventType>
	</Event>
</UniversalEvent>";

			AssertMessageWithNoInterchangeQueued(requestXml);
		}
		#endregion Messages Without Interchange

		public void TestAcknowledgementMessageContextIs_Party_ECP_GS_SecurityProxy()
		{
			var requestMessageXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Header>
        <SenderID>ABCDEFGHI</SenderID>
        <RecipientID>EDIDEMDAT</RecipientID>
		<Acknowledgement>
		  <Required>OnAll</Required>
		  <Channel>eAdaptor</Channel>
		  <RecipientID>ABCDEFGHI</RecipientID>
		</Acknowledgement>
    </Header>
    <Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
   			<Event>
				<DataContext>
					<DataTargetCollection>
 						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S00001000</Key>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<EventTime>2022-12-06T12:12:00</EventTime>
				<EventType>ARV</EventType>
			</Event>
   		</UniversalEvent>
	</Body>
</UniversalInterchange>";

			AuthenticationTestHelper.ConfigureOutboundConfig(config.Party);

			var response = PostAsyncRequest(requestMessageXml);
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);

			var serviceTask = new UniversalDataBuss.ServiceTasks.UMIServiceTask { ServiceLogger = new TestServiceLogger() };
			serviceTask.RunTask();
			interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 2, interchanges.Length);
			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 2, messages.Length);
			var sentMessage = messages.Single(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			AssertEquals("Sent message should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, sentMessage.EM_SystemCreateUser);
			var sentInterchange = interchanges.Single(i => i.EI_ReceiveTransmit == EDIInterchange.Direction.Transmit);
			AssertEquals("Sent interchange should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, sentInterchange.EI_SystemCreateUser);
			var receiveMessage = messages.Single(m => m.EM_ReceiveTransmit == EDIMessage.Direction.Receive);
			AssertEquals("Receive message should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, receiveMessage.EM_SystemCreateUser);
			var receiveInterchange = interchanges.Single(i => i.EI_ReceiveTransmit == EDIInterchange.Direction.Receive);
			AssertEquals("Receive interchange should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, receiveInterchange.EI_SystemCreateUser);
		}

		public void TestUniversalInterchangeWithNativeMessageContextIs_Party_ECP_GS_SecurityProxy()
		{
			var senderID = "ABCDEFGHI";
			var recipientID = "EDIDEMDAT";
			var universalInterchangeWithNativeXml = @$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Native#UniversalInterchange"" version=""1.1"">
  <Header>
    <SenderID>{senderID}</SenderID>
    <RecipientID>{recipientID}</RecipientID>
  </Header>
  <Body>
	<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
	  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
		<OwnerCode>EDIDATEDI</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	  </Header>
	  <Body>
		<Organization version=""1.0"">
		  <OrgHeader Action=""MERGE"">
			<FullName>JR</FullName>
			<ClosestPort TableName=""RefUNLOCO"">
				<Code>BGSOF</Code>
			</ClosestPort>
		  </OrgHeader>
		</Organization>
	  </Body>
	</Native>
  </Body>
</UniversalInterchange>";
			AuthenticationTestHelper.ConfigureOutboundConfig(config.Party);
			var response = PostAsyncRequest(universalInterchangeWithNativeXml);

			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);
			var interchange = interchanges[0];
			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];
			AssertEquals("Receive message should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, message.EM_SystemCreateUser);
			AssertEquals("Receive interchange should be created by PartyConfig.ECP_GS_SecurityProxy", partyConfigSecurityProxy.GS_Code, interchange.EI_SystemCreateUser);
		}
	}
}
