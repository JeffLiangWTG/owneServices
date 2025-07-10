using Enterprise.Customs.GUI;
using Enterprise.Customs.US.DIS.Business;

namespace Enterprise.Customs.US.DIS.GUI
{
	public class MessageSendingForm : MessageSendingFormBase<MessageSendingAction, DISDocument>
	{
		public MessageSendingForm(MessageSendingActionCollection coll) : base(coll)
		{
		}
	}
}
