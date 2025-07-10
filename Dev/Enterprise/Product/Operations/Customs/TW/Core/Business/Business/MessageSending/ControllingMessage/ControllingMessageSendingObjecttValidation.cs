using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageSendingObjectValidation : AutoControllingMessageSendingObjectValidation
	{
		public ControllingMessageSendingObjectValidation(AutoControllingMessageSendingObject parent) : base(parent)
		{
		}

		public new ControllingMessageSendingObject Parent => base.Parent as ControllingMessageSendingObject;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateShouldSend();
		}

		public void ValidateShouldSend()
		{
			((IValidationInternals)this).Validate(Parent.ShouldSendInfo, () => { CheckShouldSend(); });
		}

		void CheckShouldSend()
		{
			if (Parent.ShouldSend)
			{
				CheckShouldSendWhenTrue();
			}
		}

		protected virtual void CheckShouldSendWhenTrue()
		{
			var parent = Parent;
			if (parent.MessageType.IsEmpty)
			{
				parent.ShouldSendInfo.AddError(Res.GetString("0B0F44EB-8D14-4FAC-B720-EEF341EC083B", "A valid message type is missing. The message cannot be sent."));
			}
		}
	}
}
