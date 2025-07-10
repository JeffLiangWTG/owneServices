using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public static class ETradeMessageSenderHelper
	{
		public static ZString CreateIncrementedReferenceId(IMessageSender sender, ZString userId)
		{
			var messageCount = sender.Messages.Cast<EDIMessage>().Count(x => Array.IndexOf(new ZString[] { TRMessageTypes.Codes.TRE, TRMessageTypes.Codes.TRS, TRMessageTypes.Codes.TRD, TRMessageTypes.Codes.TCD }, x.EM_MessageType) >= 0 && x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit);
			messageCount += 1;

			return sender.JobReference + "|" + userId + "|" + messageCount.ToString();
		}
	}
}
