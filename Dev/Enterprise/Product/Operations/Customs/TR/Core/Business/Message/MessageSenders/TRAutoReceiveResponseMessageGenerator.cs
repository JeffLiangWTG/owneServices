using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public abstract class TRAutoReceiveResponseMessageGenerator<T> : TRBaseMessageGenerator<T> where T : TRBaseMessage
	{
		protected readonly ZString queryGuid;
		protected readonly EDIMessage originalMessage;

		protected TRAutoReceiveResponseMessageGenerator(IMessageSender sender, EDIMessage originalMessage) : base(sender)
		{
			this.originalMessage = originalMessage;
		}

		protected TRAutoReceiveResponseMessageGenerator(IMessageSender sender, ZString queryGuid, EDIMessage originalMessage) : this(sender, originalMessage)
		{
			this.queryGuid = queryGuid;
		}

		protected override ZString ApplicationReference => queryGuid;

		protected override GlbStaff WhoSendThisMessage => originalMessage.UserWhoQueuedThisRecord;

		protected override ZString MessageSubType => originalMessage.EM_MessageSubType;
	}
}

