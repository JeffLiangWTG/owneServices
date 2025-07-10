using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	[TestedType(typeof(BaseConfigurationMessageProcessor<ConfigurationRequest, ConfigurationMessageResponse>))]
	sealed class BaseConfigurationMessageProcessorForConfigurationTest : TestCaseWithFactory
	{
		public void TestProcessMessage_Invalid()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration_InvalidMessage();

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			AssertNull(processor.OutgoingInterchange_Exposed);
			AssertNull(processor.RequestConfigurationMessage_Exposed);
			AssertNull(processor.ConfigurationMessageResponse_Exposed);
		}

		public void TestProcessMessage_NoOutgoingInterchange_MessageDiscarded()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration();
			var message = Factory.New<EDIMessage>();

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			AssertNull(processor.OutgoingInterchange_Exposed);
			AssertNull(processor.RequestConfigurationMessage_Exposed);
			AssertNull(processor.ConfigurationMessageResponse_Exposed);
		}

		public void TestProcessMessage_Success()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.wisetechglobal.com/Schemas/ConfigurationResponse"">
	<Group Type=""System"" Reference=""WTLDPL"">
		<Group Type=""Company"" Reference=""CH1"" Status=""VAL"">
			<Credential>
				<CredentialPath>123|WTLDPL.CH1</CredentialPath>
			</Credential>
		</Group>
	</Group>
</Configuration>";

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("CustomsConfiguration", processor.OutgoingInterchange_Exposed.EI_To);
			AssertNotNull(processor.RequestConfigurationMessage_Exposed);
			AssertEquals("CH1", processor.CompanyCode_Exposed);
			var parseConfigurationMessageResult = processor.ConfigurationMessageResponse_Exposed;
			Assert(parseConfigurationMessageResult.IsSuccessful);
			AssertEquals(null, parseConfigurationMessageResult.ErrorReason);
			AssertEquals(null, parseConfigurationMessageResult.ErrorUniversalEventWrapper);
			var expectedXml = XDocument.Parse(message.EM_MessageText);
			AssertEquals(expectedXml.ToString(SaveOptions.DisableFormatting), parseConfigurationMessageResult.SuccessfulXML.ToString(SaveOptions.DisableFormatting));
		}

		public void TestProcessMessage_ErrorUniversalEventWrapper()
		{
			var logger = new LoggingInformation();
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration();
			var universalEvent = UCMPTestHelper.GetTestUniversalEvent("IRJ", "XER", "PreProcessingError", "Value cannot be null. (Parameter 'UserName')");
			message.EM_MessageText = @$"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Header>
    <SenderID>WTLDPL</SenderID>
    <RecipientID>CustomsConfiguration</RecipientID>
  </Header>
  <Body>
    {universalEvent}
  </Body>
</UniversalInterchange>";

			processor.ProcessMessage(message, logger);

			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("CustomsConfiguration", processor.OutgoingInterchange_Exposed.EI_To);
			AssertNotNull(processor.RequestConfigurationMessage_Exposed);
			AssertEquals("CH1", processor.CompanyCode_Exposed);
			var parseConfigurationMessageResult = processor.ConfigurationMessageResponse_Exposed;
			AssertEquals(expected: false, parseConfigurationMessageResult.IsSuccessful);
			AssertEquals("Value cannot be null. (Parameter 'UserName')", parseConfigurationMessageResult.ErrorReason);
			AssertEquals("PreProcessingError", parseConfigurationMessageResult.ErrorUniversalEventWrapper.ResponseType);
			AssertEquals(null, parseConfigurationMessageResult.SuccessfulXML);
		}

		public void TestGetLinkedBusinessObjectMetaData_NoOutgoingInterchange_ReturnEmptyLinkedObjectWithDiscardReason()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageNum = "123456";

			var processor = new BaseConfigurationMessageProcessorForTest_Configuration();

			var result = processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
			AssertEquals("LinkedBusinessObjectMetaData should be empty", LinkedBusinessObjectMetaData.Empty, result.ReturnValue);

			var discardReasonUnresolvedString = result.DiscardReason.GetUnresolvedString();
			AssertContains("Error message should indicate no outgoing interchange", "No outgoing Interchange for CFG message", discardReasonUnresolvedString);
			AssertContains("Error message should contains message num", message.EM_MessageNum, discardReasonUnresolvedString);
		}

		public void TestGetLinkedBusinessObjectMetaData_ReturnsFromCoreImplementation()
		{
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration();

			var result = processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
			AssertEquals(
				"LinkTableName should match the value returned by GetLinkedBusinessObjectMetaDataCore",
				message.EM_LinkTable,
				result.ReturnValue.LinkTableName);
			AssertEquals(
				"LinkUniqueId should match the value returned by GetLinkedBusinessObjectMetaDataCore",
				message.EM_LinkUniqueID,
				result.ReturnValue.LinkUniqueID);
			AssertEquals(
				"BranchPk should match the value returned by GetLinkedBusinessObjectMetaDataCore",
				message.EM_GB,
				result.ReturnValue.BranchPk);
			AssertEquals(
				"JobNumber should match the value returned by GetLinkedBusinessObjectMetaDataCore",
				"123456",
				result.ReturnValue.JobNumber);
		}

		public void TestGetLinkedBusinessObjectMetaData_CoreImplementationReturnsNull_ReturnsDefault()
		{
			var processor = new BaseConfigurationMessageProcessorForTest_Configuration_InvalidMessage();

			var result = processor.GetLinkedBusinessObjectMetaData(message, new LoggingInformation());
			AssertEquals("LinkTableName should match the message.EM_LinkTable", message.EM_LinkTable, result.ReturnValue.LinkTableName);
			AssertEquals("LinkUniqueId should match the message.EM_LinkUniqueId", message.EM_LinkUniqueID, result.ReturnValue.LinkUniqueID);
			AssertEquals("BranchPk should match the message.EM_GB", message.EM_GB, result.ReturnValue.BranchPk);
			AssertEquals("JobNumber should be empty", ZString.Empty, result.ReturnValue.JobNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var sessionGuid = Guid.NewGuid();
			outgoingInterchange = Factory.New<EDIInterchange>();
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_SessionGUID = sessionGuid;
			incomingInterchange.EI_ApplicationCode = "CFG";
			incomingInterchange.EI_To = "WTLDCHCTU";
			incomingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			incomingInterchange.EI_From = "CustomsConfiguration";
			outgoingInterchange.EI_SessionGUID = sessionGuid;
			outgoingInterchange.EI_ApplicationCode = incomingInterchange.EI_ApplicationCode;
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			outgoingInterchange.EI_From = "WTLDCHCTU";
			outgoingInterchange.EI_To = "CustomsConfiguration";
			outgoingInterchange.EI_InterchangeType = EDIMessage.ApplicationCodes.CHCustomsEdec;
			outgoingInterchange.EI_BodyText = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""CHCustomId"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">
  <Group Type=""System"" Reference=""WTLXYZ"">
    <Group Type=""Company"" Reference=""CH1"" Status=""VAL"">
      <Credential>
        <UserName>customsID</UserName>
        <Password />
      </Credential>
    </Group>
  </Group>
</Configuration>";

			message = Factory.New<EDIMessageForTest>();
			message.EM_EI = incomingInterchange.PK;
			message.EM_LinkTable = "ABC";
			message.EM_LinkUniqueID = ZGuid.NewZGuid();

			Factory.Save();
		}
		EDIMessage message;
		EDIInterchange outgoingInterchange;

		class BaseConfigurationMessageProcessorForTest_Configuration : BaseConfigurationMessageProcessor<ConfigurationRequest, ConfigurationMessageResponse>
		{
			public EDIInterchange OutgoingInterchange_Exposed { get; set; }
			public Configuration RequestConfigurationMessage_Exposed { get; set; }
			public ConfigurationMessageResponse ConfigurationMessageResponse_Exposed { get; set; }
			public string CompanyCode_Exposed { get; set; }

			protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, ConfigurationRequest requestMessage, EDIMessage message)
			{
				OutgoingInterchange_Exposed = outgoingInterchange;
				RequestConfigurationMessage_Exposed = requestMessage.Request;
				CompanyCode_Exposed = GetCompanyCodeFromConfiguration(RequestConfigurationMessage_Exposed);
				return true;
			}

			protected override void ProcessMessageCore(EDIMessage message, ConfigurationMessageResponse responseMessage, ILoggingInformation logger)
			{
				ConfigurationMessageResponse_Exposed = responseMessage;
			}
			protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(EDIMessage message, object linkedObject, ILoggingInformation logger)
			{
				return new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, "123456");
			}

			string GetCompanyCodeFromConfiguration(Configuration configuration)
			{
				return configuration.Group.Cast<Group>()
							.SelectMany(g => g.Items.Cast<Group>())
							.FirstOrDefault(i => i.Type == Constants.GroupTypes.CompanyType)?.Reference;
			}
		}

		class BaseConfigurationMessageProcessorForTest_Configuration_InvalidMessage : BaseConfigurationMessageProcessorForTest_Configuration
		{
			protected override bool IsValidMessageCore(EDIInterchange outgoingInterchange, ConfigurationRequest requestMessage, EDIMessage message)
			{
				return false;
			}

			protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(EDIMessage message, object linkedObject, ILoggingInformation logger)
			{
				return null;
			}
		}
	}
}
