using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	[TestedType(typeof(CFGInterchangeUnpacker))]
	sealed class CFGInterchangeUnpackerTest : InterchangeUnpackerTest<CFGInterchangeUnpacker>
	{
		public void TestUnpack()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var outgoingMessage = Factory.New<EDIMessageForTest>();
			outgoingMessage.EM_LinkTable = "JobDeclaration";
			outgoingMessage.EM_LinkUniqueID = declaration.PK;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_ApplicationCode = "TST";
			outgoingMessage.EM_MessageType = "ZZZ";

			var sessionGUID = ZGuid.NewZGuid();
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "OUT123456";
			outgoingInterchange.EI_BodyText = "Outgoing Test Data";
			outgoingInterchange.EI_SessionGUID = sessionGUID;
			outgoingInterchange.EI_ReceiveTransmit = "TRX";
			outgoingInterchange.EI_Status = "SNT";
			outgoingInterchange.EI_ApplicationCode = "TST";
			outgoingInterchange.EI_InterchangeType = "ZZZ";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "ICS22023001";
			incomingInterchange.EI_BodyText = "Incoming Test Data";
			incomingInterchange.EI_SessionGUID = sessionGUID;
			incomingInterchange.EI_ReceiveTransmit = "RCV";
			incomingInterchange.EI_Status = "QUE";
			incomingInterchange.EI_ApplicationCode = "TST";
			incomingInterchange.EI_InterchangeType = "XER";
			Factory.Save();

			var logger = new LoggingInformation();
			var unpacker = new CFGInterchangeUnpacker();
			var unpackResult = unpacker.Unpack(incomingInterchange, outgoingInterchange, outgoingMessage, logger);

			CombineAssertions(() =>
			{
				Assert("Successful unpacking", unpackResult.IsSuccess);
				AssertEquals(1, incomingInterchange.ContainedMessages.Count);
				AssertEquals(1, unpackResult.EdiMessages.Count);
				var message = unpackResult.EdiMessages.Single();
				AssertSame(message, incomingInterchange.ContainedMessages[0]);

				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_GB", outgoingMessage.EM_GB, message.EM_GB);
				AssertEquals("EM_MessageNum", "ICS22023001", message.EM_MessageNum);
				AssertEquals("EM_MessageType", "ZZZ", message.EM_MessageType);
				AssertEquals("EM_MessageText", "Incoming Test Data", message.EM_MessageText);
			});
		}

		public void TestUnpack_NoOutgoingMessage()
		{
			var sessionGUID = ZGuid.NewZGuid();
			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "OUT123456";
			outgoingInterchange.EI_BodyText = "Outgoing Test Data";
			outgoingInterchange.EI_SessionGUID = sessionGUID;
			outgoingInterchange.EI_ReceiveTransmit = "TRX";
			outgoingInterchange.EI_Status = "SNT";
			outgoingInterchange.EI_ApplicationCode = "TST";
			outgoingInterchange.EI_InterchangeType = "ZZ1";

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "ICS22023001";
			incomingInterchange.EI_BodyText = "Incoming Test Data";
			incomingInterchange.EI_SessionGUID = sessionGUID;
			incomingInterchange.EI_ReceiveTransmit = "RCV";
			incomingInterchange.EI_Status = "QUE";
			incomingInterchange.EI_ApplicationCode = "TST";
			incomingInterchange.EI_InterchangeType = "XER";
			Factory.Save();

			var logger = new LoggingInformation();
			var unpacker = new CFGInterchangeUnpacker();
			var unpackResult = unpacker.Unpack(incomingInterchange, outgoingInterchange, null, logger);

			CombineAssertions(() =>
			{
				Assert("Successful unpacking", unpackResult.IsSuccess);
				AssertEquals(1, incomingInterchange.ContainedMessages.Count);
				AssertEquals(1, unpackResult.EdiMessages.Count);
				var message = unpackResult.EdiMessages.Single();
				AssertSame(message, incomingInterchange.ContainedMessages[0]);

				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_GB", outgoingInterchange.EI_GB, message.EM_GB);
				AssertEquals("EM_MessageNum", "ICS22023001", message.EM_MessageNum);
				AssertEquals("EM_MessageType", "ZZ1", message.EM_MessageType);
				AssertEquals("EM_MessageText", "Incoming Test Data", message.EM_MessageText);
			});
		}

		protected override string[] ApplicationCodes => new[] { ApplicationCodeList.Codes.XHCredentialConfig };
	}
}
