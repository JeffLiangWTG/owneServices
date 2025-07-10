using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageManagers.Testing
{
	[TestedType(typeof(GOVGIOMessageManager))]
	sealed class GOVGIOMessageManagerTest : EDIFACTMessageManagerTestCase
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

		public override void TestCanSendThisMessage()
		{
			var messageHeader = (GOVGIOMessageHeader)dataWrapper;
			var header = messageHeader.ManifestHeader;
			GOVGIOMessageManagerForTest manager = new GOVGIOMessageManagerForTest((GOVGIOMessageHeader)dataWrapper, notification);

			ZString reason;

			AssertEquals(false, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals("Job not yet saved, Please save before sending.", reason);

			Factory.Save();

			AssertEquals(true, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals(string.Empty, reason);

			header.GateInOutMessageType = ZString.Empty;
			Factory.Save();

			AssertEquals(false, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals("Gate In/Out Message Type is required.", reason);

			header.GateInOutMessageType = "???";
			Factory.Save();

			AssertEquals(false, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals("'???' is not a valid Gate In/Out Message Type.", reason);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;
			Factory.Save();

			AssertEquals(false, manager.CanSendThisMessage_Exposed(out reason));
			AssertEquals("'ADI' is not a valid Gate In/Out Message Type.", reason);
		}

		public override void TestGetMessageBuilder()
		{
			AssertType(typeof(GOVGIOMessageBuilder), ((GOVGIOMessageManagerForTest)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Create));
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("GOVGIO", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			var messageHeader = (GOVGIOMessageHeader)dataWrapper;
			var manager = new GOVGIOMessageManagerForTest(messageHeader, notification);

			var result = manager.PopulateMessages_Exposed();
			AssertEquals(1, result.Length);

			AssertMultilineASCIIEquals("MessageBody", GOVGIOExamples.GetExpectedSeaDepotGateInText(), result[0].EM_MessageText.Replace("'", "'\r\n"));
		}

		public override void SetTestMode(bool testMode)
		{
			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, testMode);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			var header = GOVGIOExamples.CreateSeaDepotGateIn(Factory);
			return new GOVGIOMessageHeader(header);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new GOVGIOMessageManagerForTest((GOVGIOMessageHeader)dataWrapper, new MessageNotificationCollector_ForTest());
		}

		new readonly MessageNotificationCollector_ForTest notification = new MessageNotificationCollector_ForTest();
	}

	sealed class GOVGIOMessageManagerForTest : GOVGIOMessageManager
	{
		public GOVGIOMessageManagerForTest(GOVGIOMessageHeader source, IMessageNotificationCollector notification) : base(source, notification)
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
