using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	sealed class SGInboundXMLMessageProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessErrorMessage()
		{
			ProcessWithFile("ErrorMessage.xml", "199702247W202005208000");
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessApprovalMessage()
		{
			ProcessWithFile("ApprovalMessage.xml", "199702247W202005208000");
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRejectionMessage()
		{
			ProcessWithFile("RejectionMessage.xml", "199702247W202005208000");
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessCertificateOfOriginApprovalMessage()
		{
			ProcessWithFile("CertificateOfOriginApproval.xml", "199702247W202005208000");
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessPermitMessage()
		{
			ProcessWithFile("INPPMT.xml", "199702247W202005208000");
			ProcessWithFile("IPTPMT.xml", "199702247W202005208000");
			ProcessWithFile("OUTPMT.xml", "199702247W202005208000");
			ProcessWithFile("TNPPMT.xml", "199702247W202005208000");
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessUpdatePermitMessage()
		{
			ProcessWithFile("INPUPT.xml", "199702247W202005208000");
			ProcessWithFile("IPTUPT.xml", "199702247W202005200800");
			ProcessWithFile("OUTUPT.xml", "199702247W202005208000");
			ProcessWithFile("TNPUPT.xml", "199702247W202005208000");
		}

		public void TestInvalidMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageText = "THIS MESSAGE TEXT IS SO WRONG IT IS NOT FUNNY";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SGCustomsTradenetXML;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var logger = new LoggingInformation();
			var processor = new SGInboundXMLMessageProcessor(logger, EDIMessage.ApplicationCodes.SGCustomsTradenetXML);
			processor.ProcessMessage(message);
			AssertEquals("Invalid XML Format.", EDIMessage.Status.Failed, message.EM_Status);
			Assert("Invalid XML Format.", logger.Logs.Any(c => c.Message == "Invalid XML Format."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmptyTradenetResponse()
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\EmptyTradenetResponse.xml");
			var message = Factory.New<SGXmlEDIMessage>();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SGCustomsTradenetXML;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkedObject = null;
			var logger = new LoggingInformation();
			var processor = new SGInboundXMLMessageProcessor(logger, EDIMessage.ApplicationCodes.SGCustomsTradenetXML);
			processor.ProcessMessage(message);
			AssertEquals("Empty Tradenet Response.", EDIMessage.Status.Failed, message.EM_Status);
			Assert("Empty Tradenet Response.", logger.Logs.Any(c => c.Message == "Can't find a valid processor for this message."));
		}

		void ProcessWithFile(string fileName, string applicationReference)
		{
			var messageText = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\" + fileName);
			var factory = NewFactory();
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var transmitMessage = factory.New<SGXmlEDIMessage>();
			transmitMessage.EM_ApplicationReference = applicationReference;
			transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(transmitMessage);
			var message = factory.New<SGXmlEDIMessage>();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SGCustomsTradenetXML;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkedObject = null;
			var logger = new LoggingInformation();
			var processor = new SGInboundXMLMessageProcessor(logger, EDIMessage.ApplicationCodes.SGCustomsTradenetXML);
			processor.ProcessMessage(message);
			AssertEquals("Process Message Without Any Exceptions.", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Should attach the received message.", entryHeader.PK, message.EM_LinkUniqueID);
		}
	}
}
