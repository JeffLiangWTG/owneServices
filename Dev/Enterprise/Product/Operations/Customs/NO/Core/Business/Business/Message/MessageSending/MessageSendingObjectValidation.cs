using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business
{
	public class MessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent)
			: base(parent)
		{
		}

		public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

		protected override void CheckMessageType()
		{
			base.CheckMessageType();

			if (!Parent.EntryStatus.In(new ZString[] { MessageSendingStatusCodes.Codes.FinalApproval, MessageSendingStatusCodes.Codes.RefusalOfDeclaration }))
			{
				if (Parent.ShouldSend)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.MessageTypeInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.MessageTypeInfo);
			}
		}
	}
}
