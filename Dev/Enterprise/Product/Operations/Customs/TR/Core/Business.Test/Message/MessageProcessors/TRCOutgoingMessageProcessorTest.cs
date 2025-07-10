using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRCOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestCreateNewInterchangeProvider()
		{
			var message1 = CreateMessage(Factory, "Message 1", EDIMessage.ApplicationCodes.TRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, TRMessageTypes.Codes.TRE);
			var message2 = CreateMessage(Factory, "Message 2", EDIMessage.ApplicationCodes.TRCustoms, EDIMessage.Direction.Transmit, EDIMessageStatusList.Codes.Queued, TRMessageTypes.Codes.TRO);

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new TRCOutgoingMessageProcessor(logger);
			processor.ProcessMessage(CancellationToken.None);

			CombineAssertions(() =>
			{
				var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("NumberOfInterchanges", 2, interchangesCreated.Length);

				message1.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message1.EM_Status);
				var interchange1 = interchangesCreated.Single(i => i.PK == message1.EM_EI);
				AssertEquals("EI_InterchangeNum", message1.EM_MessageNum, interchange1.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "00000000000001", interchange1.EI_InterchangeNum);
				AssertEquals(EDIMessageStatusList.Codes.Sent, message1.EM_Status);

				message2.Reload();
				AssertEquals("EM_Status", EDIMessage.Status.Sent, message2.EM_Status);
				var interchange2 = interchangesCreated.Single(i => i.PK == message2.EM_EI);
				AssertEquals("EI_InterchangeNum", message2.EM_MessageNum, interchange2.EI_InterchangeNum);
				AssertEquals("EI_InterchangeNum", "00000000000002", interchange2.EI_InterchangeNum);
				AssertEquals(EDIMessageStatusList.Codes.Sent, message2.EM_Status);
			});
		}

		EDIMessage CreateMessage(BusinessObjectFactory factory, ZString messageText, ZString applicationCode, ZString direction, ZString status, ZString type, string subType = null)
		{
			var message = factory.New<TRBaseMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_MessageText = messageText;
			message.EM_MessageType = type;
			message.EM_MessageSubType = subType;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			return message;
		}
	}
}
