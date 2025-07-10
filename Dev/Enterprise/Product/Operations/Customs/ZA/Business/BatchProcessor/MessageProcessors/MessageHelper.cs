using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public abstract class MessageHelper : NonPersistentBusinessObject
	{
		protected MessageHelper(ZAMessage message)
			: base(message.Factory)
		{
			this.Message = message;
		}

		public EDIInterchange Interchange => Message.Interchange;
		public readonly ZAMessage Message;

		public static string ApplicationCode => "ZAC";

		public ZDateTime InterchangeTime => interchangeTime;
		protected ZDateTime interchangeTime;

		public ZQuery GetOutgoingMessageQuery(ZString outGoingMessageNum)
		{
			ZQuery result = null;
			if (!outGoingMessageNum.IsEmpty)
			{
				result = new ZQuery();
				result.AddToFilter(EDIMessageSchema.EM_MessageNum, outGoingMessageNum);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
				result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			}
			return result;
		}
	}
}
