using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	[TestedType(typeof(BaseConfigurationMessageProcessor<CredentialChangesRequest, UniversalEventWrapper>))]
	sealed class BaseConfigurationMessageProcessorForCredentialTest : TestCaseWithFactory
	{
		public void TestProcessMessage_Invalid()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_CredentialChange_InvalidMessage();

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			AssertNull(processor.CredentialChangesCredentialChange_Exposed);
			AssertNull(processor.UniversalEventWrapper_Exposed);
		}

		public void TestProcessMessage_NoOutgoingInterchange_MessageDiscarded()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_CredentialChange();
			var message = Factory.New<EDIMessage>();

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			AssertNull(processor.CredentialChangesCredentialChange_Exposed);
			AssertNull(processor.UniversalEventWrapper_Exposed);
		}

		public void TestProcessMessage_IAK()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_CredentialChange();
			var universalEvent = UCMPTestHelper.GetTestUniversalEvent("IAK", "CLC", "", "");
			message.EM_MessageText = @$"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
                            <Header>
                                <SenderID>HYECLTCM2</SenderID>
                                <RecipientID>CustomsCredentialChange</RecipientID>
                            </Header>
                            <Body>
                                {universalEvent}
                            </Body>
                          </UniversalInterchange>";

			processor.ProcessMessage(message, logger);
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			var credentialChange = processor.CredentialChangesCredentialChange_Exposed;
			AssertEquals("WTLCTU_JPC", credentialChange.EnterpriseCode);
			AssertEquals(string.Empty, credentialChange.DatabaseCode);
			var universalEventWrapper = processor.UniversalEventWrapper_Exposed;
			Assert(universalEventWrapper.IsAcknowledgement);
			AssertEquals(string.Empty, universalEventWrapper.Reason);
		}

		public void TestProcessMessage_IRJ()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_CredentialChange();
			message.EM_MessageText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID>CustomsConfiguration</SenderID>
		<RecipientID>HYEDUSCM2</RecipientID>
	</Header>
	<Body>
		<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
			<Event>
				<EventTime>2024-05-30 01:23:02.098</EventTime>
				<EventType>IRJ</EventType>
				<EventParameters>
					<MessageType>IC2</MessageType>
					<Reason>An item with the same key has already been added. Key: CertificateURI</Reason>
				</EventParameters>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			var credentialChange = processor.CredentialChangesCredentialChange_Exposed;
			AssertEquals("WTLCTU_JPC", credentialChange.EnterpriseCode);
			AssertEquals(string.Empty, credentialChange.DatabaseCode);
			var universalEventWrapper = processor.UniversalEventWrapper_Exposed;
			AssertEquals(expected: false, universalEventWrapper.IsAcknowledgement);
			AssertEquals("An item with the same key has already been added. Key: CertificateURI", universalEventWrapper.Reason);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var sessionGuid = Guid.NewGuid();
			outgoingInterchange = Factory.New<EDIInterchange>();
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_SessionGUID = sessionGuid;
			incomingInterchange.EI_ApplicationCode = "CFG";
			incomingInterchange.EI_To = "WTLDJPCTU";
			incomingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			incomingInterchange.EI_From = "CustomsCredentialChange";
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_ApplicationCode = incomingInterchange.EI_ApplicationCode;
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			outgoingInterchange.EI_From = "WTLDJPCTU";
			outgoingInterchange.EI_To = "CustomsCredentialChange";
			outgoingInterchange.EI_InterchangeType = EDIMessage.ApplicationCodes.JPCustoms;
			outgoingInterchange.EI_BodyText = @"<CredentialChanges xmlns=""http://cargowise.com/xhub/credentials"">
  <CredentialChange>
    <ChangeType>Update</ChangeType>
    <ChangeDateTime>2024-05-31T07:13:48.177+10:00</ChangeDateTime>
    <EnterpriseCode>WTLCTU_JPC</EnterpriseCode>
    <DatabaseCode />
    <DatabaseNumber />
    <LicenceType>TST</LicenceType>
    <Password>80-32-2A-9D-8E-56-C3-3D-F1-CC-A0-2A-69-AF-3E-88-A6-A9-3B-4B-EE-B1-2E-06-00-BB-51-0A-40-42-0A-5E-19-E0-F8-9E-DC-4C-14-E8-E1-AA-9D-D1-EF-51-0A-A3-06-8B-BA-9B-85-F9-4B-57-9F-92-31-EE-EF-73-22-F6</Password>
    <OldPassword />
  </CredentialChange>
</CredentialChanges>";

			message = Factory.New<EDIMessageForTest>();
			message.EM_EI = incomingInterchange.PK;
			Factory.Save();
		}

		EDIMessage message;
		EDIInterchange outgoingInterchange;

		class BaseConfigurationMessageProcessorForTest_CredentialChange : BaseConfigurationMessageProcessor<CredentialChangesRequest, UniversalEventWrapper>
		{
			public CredentialChangesCredentialChange CredentialChangesCredentialChange_Exposed { get; set; }
			public UniversalEventWrapper UniversalEventWrapper_Exposed { get; set; }

			protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, CredentialChangesRequest requestMessage, EDIMessage message)
			{
				CredentialChangesCredentialChange_Exposed = requestMessage.Request;
				return true;
			}

			protected override void ProcessMessageCore(EDIMessage message, UniversalEventWrapper responseMessage, ILoggingInformation logger)
			{
				UniversalEventWrapper_Exposed = responseMessage;
			}

			protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(EDIMessage message, object linkedObject, ILoggingInformation logger)
			{
				return null;
			}
		}

		class BaseConfigurationMessageProcessorForTest_CredentialChange_InvalidMessage : BaseConfigurationMessageProcessorForTest_CredentialChange
		{
			protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, CredentialChangesRequest requestMessage, EDIMessage message)
			{
				return false;
			}
		}
	}
}
