using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(COSTCOMessageManager))]
	sealed class COSTCOMessageManagerTest : EDIFACTMessageManagerTestCase
	{
		public override void TestIsWaitingForResponse()
		{
			dataWrapper.MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			Assert("IsWaitingForResponse", messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = ZAMessageStatusList.Codes.Error;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = ZString.Empty;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
		}

		public void TestAMA_ManifestRegistrationDate_MandatoryValidationOnSending()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var testWrapper = new COSTCOHeader(header);
			var manager = new COSTCOMessageManagerForTest(testWrapper, notification);

			manager.SendMessage(MessageSubTypes.Create);
			AssertHasMessageErrorContaining(header.AMA_IssueDateInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_IssueDate = ZDate.Today;

			manager.SendMessage(MessageSubTypes.Create);
			AssertNoMessageErrorContaining(header.AMA_IssueDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public new void TestBusinessObject()
		{
			AssertType<AsycudaManifestHeader>(messageManager.BusinessObject);
		}

		public override void TestCanSendThisMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			var testWrapper = new COSTCOHeader(header);
			var manager = new COSTCOMessageManagerForTest(testWrapper, notification);
			AssertEquals(false, manager.CanSendThisMessage_Exposed(out var reason));
			AssertEquals("Job not yet saved, Please save before sending.", reason);

			Factory.Save();
			manager = new COSTCOMessageManagerForTest(testWrapper, notification);
			AssertEquals(true, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals(string.Empty, reason);
		}

		public override void TestGetMessageBuilder()
		{
			AssertType(typeof(COSTCOMessageBuilder), ((COSTCOMessageManagerForTest)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Create));
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("COSTCO", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			Factory.Save();

			var testWrapper = new COSTCOHeader(header);
			var manager = new COSTCOMessageManagerForTest(testWrapper, notification);
			var result = manager.PopulateMessages_Exposed();
			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", @"UNH+<<MSGNO PLACEHOLDER>>+COSTCO:D:16A:UN:RCG001
BGM+788+<<SYSCAR>>+9
FTX+ADI
TDT+20++++:172:20+++:103
RFF+ACL
NAD+MS+::ZZZ
NAD+RL+::ZZZ
EQD+BB+1
SEL
CNT+8:0
UNT+11+<<MSGNO PLACEHOLDER>>
", result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		public override void SetTestMode(bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, testMode);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new COSTCOHeader(header);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new COSTCOMessageManagerForTest((COSTCOHeader)dataWrapper, new MessageNotificationCollector_ForTest());
		}

		new readonly MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();
	}

	sealed class COSTCOMessageManagerForTest : COSTCOMessageManager
	{
		public COSTCOMessageManagerForTest(COSTCOHeader source, IMessageNotificationCollector notification) : base(source, notification) { }

		public IMessageBuilder GetMessageBuilder_Exposed(MessageSubTypes actionCode) => GetMessageBuilder(actionCode);

		public Messaging.Business.EDIMessage[] PopulateMessages_Exposed()
		{
			return PopulateMessage(MessageSubTypes.Create);
		}

		public bool CanSendThisMessage_Exposed(out ZString messageText)
		{
			return CanSendThisMessage(MessageSubTypes.Undefined, out messageText);
		}
	}
}
