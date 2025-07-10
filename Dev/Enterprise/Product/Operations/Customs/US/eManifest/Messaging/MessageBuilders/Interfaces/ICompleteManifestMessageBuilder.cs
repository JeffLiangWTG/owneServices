using System;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface ICompleteManifestMessageBuilder : Enterprise.Messaging.MessageBuilders.IMessageBuilder
	{
		void GenerateMessageContent(Enterprise.Messaging.Business.EDIMessage message);
	}

	public static class ICompleteManifestMessageBuilderHelper
	{
		public static void GenerateMessageContent(this ICompleteManifestMessageBuilder builder, Action populateEdifactMessage, Func<string> getMessageText, Func<string> getMessageInterpretation, Enterprise.Messaging.Business.EDIMessage message)
		{
			populateEdifactMessage();
			message.EM_MessageText = getMessageText().Replace(EDIMessage.MessageNumberPlaceHolder, message.EM_MessageNum);
			message.EM_MessageInterpretation = getMessageInterpretation();
		}
	}
}
