using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public abstract class DeclarationMessageProcessorBaseTest<TProcessor> : TestCaseWithFactory where TProcessor : DeclarationMessageProcessorBase
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "TR Import-Export Message Processor", Processor.MessageFriendlyName);
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

		protected void AssertStatusesMessage(EDIMessage incomingMessage, ZString messageStatus, ZString customsStatus, ZString ediMessageStatus)
		{
			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var messageAttachee = (cusEntryHeader as IMessageAttachee);
			CombineAssertions("Statuses of Message", () =>
			{
				AssertEquals("Message Status", messageStatus, messageAttachee.MessageStatus);
				AssertEquals("Customs Status", customsStatus, messageAttachee.CustomsStatus);
				AssertEquals("message.EM_Status", ediMessageStatus, incomingMessage.EM_Status);
			});
		}

		protected (EDIInterchange originalInterchagne, EDIMessage originalMessage, EDIInterchange incomingInterchagne, EDIMessage incomingMessage, JobDeclaration messageAttachee, ZGuid sessionID) CreateMessageWithOrigins(string messageText, string messageNumber, string messageType = TRMessageTypes.Codes.DKO)
		{
			var originMessageType = messageType == TRMessageTypes.Codes.DKO || messageType == TRMessageTypes.Codes.DK1  ? TRMessageTypes.Codes.DKO : TRMessageTypes.Codes.DTE;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = newBranch.PK;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "3-B00001000";
			var sessionID = ZGuid.NewZGuid();
			var originalInterchange = TRMessageTestHelper.CreateInterchange(Factory, originMessageType, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var originalMessage = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, originMessageType, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, string.Empty, "DKOTRX001", string.Empty);
			originalMessage.EM_EI = originalInterchange.PK;
			originalMessage.EM_LinkedObject = cusEntryHeader;

			Factory.Save();

			var incomingInterchange = TRMessageTestHelper.CreateInterchange(Factory, messageType, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, sessionID);
			var incomingMessage = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, messageType, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, "DKORCV001", string.Empty);
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_MessageNum = messageNumber;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			incomingMessage.EM_LinkedObject = cusEntryHeader;

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
