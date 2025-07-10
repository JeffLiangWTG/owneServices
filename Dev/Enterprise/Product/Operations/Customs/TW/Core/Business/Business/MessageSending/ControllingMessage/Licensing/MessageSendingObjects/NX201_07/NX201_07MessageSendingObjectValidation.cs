
namespace Enterprise.Customs.TW.Business
{
	public class NX201_07MessageSendingObjectValidation : LicensingMessageSendingObjectValidation
	{
		public NX201_07MessageSendingObjectValidation(NX201_07MessageSendingObject parent) : base(parent)
		{
		}

		protected override void CheckShouldSendWhenTrue()
		{
			base.CheckShouldSendWhenTrue();
			if (Parent is NX201_07MessageSendingObject parent && parent.ReasonDescription.IsEmpty && parent.IsReasonDescriptionRequired)
			{
				parent.ShouldSendInfo.AddError(Res.GetString("b1a2c435-6c88-482d-8209-df3a16b984af", "Reason Description is required when Action is '1' or '50'."));
			}
		}
	}
}
