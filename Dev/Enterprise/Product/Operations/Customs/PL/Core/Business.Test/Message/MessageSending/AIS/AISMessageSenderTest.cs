using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AISMessageSenderTest : AbstractMessageSenderTest
{
	public void TestIsCorrectionAmendment()
	{
		AssertSendMessageSubType(ImportMessageSendingObjectActionList.Codes.ZC415, AISMessageCodes.Descriptions.ZC415);

		void AssertSendMessageSubType(ZString actionType, ZString expectedSubType)
		{
			sendingObject.Action = actionType;
			var messageSender = new AISMessageSender(factory, sendingObject, sendingObjectParent);
			var message = messageSender.Send();

			AssertEquals($"EM_MessageSubType for {actionType}:", expectedSubType, message.EM_MessageSubType);
		}
	}

	public void TestMessageText_ZC415()
	{
		sendingObject.Action = ImportMessageSendingObjectActionList.Codes.ZC415;
		var messageSender = new AISMessageSender(factory, sendingObject, sendingObjectParent);
		var message = messageSender.Send();
		factory.Save();

		CombineAssertions(() =>
		{
			AssertNotEquals("EM_MessageData should contain ZC415 xml Message", 0, message.EM_MessageData.Length);

			var expectedNodes = Array.Empty<ZString>();
			var expectedRootElement = "zc415:ZC415";
			AssertXmlMessage(message.EM_MessageText, expectedRootElement, expectedNodes);
		});
	}
}
