using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public abstract class ExportUnionMessageProcessorBaseTest<TProcessor> : TestCaseWithFactory where TProcessor : ExportUnionMessageProcessorBase
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "TR Export Union Message Processor", Processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, Processor.ApplicationCode);
		}

		protected abstract TProcessor Processor { get; }

		protected abstract ZString DefaultMessageType { get; }

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		protected LoggingInformation logger;

		protected (EDIInterchange originalInterchagne, EDIMessage originalMessage, EDIInterchange incomingInterchagne, EDIMessage incomingMessage, JobDeclaration messageAttachee, ZGuid sessionID) CreateMessageWithOrigins(string messageText, string messageNumber, string messageType = TRMessageTypes.Codes.EUT)
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "3-B00001000";
			var sessionID = ZGuid.NewZGuid();
			var originalInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.EUT, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var originalMessage = TRMessageTestHelper.CreateMessage<ExportUnionMessage>(Factory, TRMessageTypes.Codes.EUT, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, "DKOTRX001", string.Empty);
			originalMessage.EM_EI = originalInterchange.PK;
			originalMessage.EM_LinkedObject = cusEntryHeader;

			Factory.Save();

			var incomingInterchange = TRMessageTestHelper.CreateInterchange(Factory, messageType, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var incomingMessage = TRMessageTestHelper.CreateMessage<ExportUnionMessage>(Factory, messageType, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, "DKORCV001", string.Empty);
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_MessageNum = messageNumber;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			return (originalInterchange, originalMessage, incomingInterchange, incomingMessage, declaration, sessionID);
		}

		protected class TestMessageContext
		{
			public EDIInterchange OriginalInterchange { get; set; }
			public EDIMessage OriginalMessage { get; set; }
			public EDIInterchange IncomingInterchange { get; set; }
			public EDIMessage IncomingMessage { get; set; }
			public JobDeclaration MessageAttachee { get; set; }
			public ZGuid SessionID { get; set; }
		}
	}
}
