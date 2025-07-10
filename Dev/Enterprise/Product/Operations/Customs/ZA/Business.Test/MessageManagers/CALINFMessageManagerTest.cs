using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(CALINFMessageManager))]
	sealed class CALINFMessageManagerTest : EDIFACTMessageManagerTestCase
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

		public new void TestBusinessObject()
		{
			AssertType<JobVoyage>(messageManager.BusinessObject);
		}

		public override void TestCanSendThisMessage()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "VF999";

			var testWrapper = new CALINFMessageData(jobVoyage);
			var manager = new CALINFMessageManagerForTest(testWrapper, notification);

			AssertEquals(false, manager.CanSendThisMessage_Exposed(out var reason));
			AssertEquals("Job not yet saved, Please save before sending.", reason);

			Factory.Save();
			manager = new CALINFMessageManagerForTest(testWrapper, notification);
			AssertEquals(true, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals(string.Empty, reason);
		}

		public override void TestGetMessageBuilder()
		{
			AssertType(typeof(CALINFMessageBuilder), ((CALINFMessageManagerForTest)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Create));
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("CALINF", messageManager.MessageFriendlyName);
		}

		[TestDate(2020, 06, 16, 14, 35, 0)]
		public override void TestPopulateMessages()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			var testWrapper = new CALINFMessageData(jobVoyage);
			var manager = new CALINFMessageManagerForTest(testWrapper, notification);
			var result = manager.PopulateMessages_Exposed();

			AssertEquals(1, result.Length);
			AssertMultilineASCIIEquals("MessageBody", @"UNH+<<MSGNO PLACEHOLDER>>+CALINF:D:16A:UN:RCG001
BGM+96:::SCH+<<SYSCAR>>+9
DTM+137:202006161435:203
NAD+MS+::ZZZ
TDT+20++1++:172:20+++:103
RFF+ACL
UNT+7+<<MSGNO PLACEHOLDER>>
", result[0].EM_MessageText.Replace("'", "\r\n"));
		}

		public override void SetTestMode(bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, testMode);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			return new CALINFMessageData(jobVoyage);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new CALINFMessageManagerForTest((CALINFMessageData)dataWrapper, new MessageNotificationCollector_ForTest());
		}

		new readonly MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();
	}

	sealed class CALINFMessageManagerForTest : CALINFMessageManager
	{
		public CALINFMessageManagerForTest(CALINFMessageData source, IMessageNotificationCollector notification)
			: base(source, notification)
		{
		}

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
