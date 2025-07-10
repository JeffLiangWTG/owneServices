namespace Enterprise.Customs.TW.Business.N5167
{
	public class N5167MessageSendingObjectValidation : MessageSendingObjectValidation
	{
		public N5167MessageSendingObjectValidation(MessageSendingObject parent) : base(parent)
		{
		}

		public new N5167MessageSendingObject Parent => base.Parent as N5167MessageSendingObject;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.EntryStatus != EntryStatusCodeList.Codes.IEM)
			{
				Parent.ShouldSendInfo.AddMessageError(ValidationConstants.MessageSendingObject.EntryStatusShouldBeIEM);
			}
		}
	}
}
