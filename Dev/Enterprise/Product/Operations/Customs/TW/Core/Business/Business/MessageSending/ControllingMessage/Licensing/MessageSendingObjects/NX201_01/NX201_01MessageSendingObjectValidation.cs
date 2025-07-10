
namespace Enterprise.Customs.TW.Business
{
	public class NX201_01MessageSendingObjectValidation : LicensingMessageSendingObjectValidation
	{
		public NX201_01MessageSendingObjectValidation(NX201_01MessageSendingObject parent) : base(parent)
		{
		}

		protected new NX201_01MessageSendingObject Parent => (NX201_01MessageSendingObject)base.Parent;

		protected override void CheckShouldSendWhenTrue()
		{
			base.CheckShouldSendWhenTrue();
			var parent = Parent;
			if (parent.ReasonDescription.IsEmpty && parent.ActionForReasonDescriptions.Contains(parent.Action))
			{
				parent.ShouldSendInfo.AddError(Res.GetString("109f542e-39ba-45a2-bc8b-5377a1cd6d1a", "Reason Description is required when Action is '5' or '17' or '52'."));
			}
		}

		protected override void CheckAction()
		{
			base.CheckAction();
			var parent = Parent;
			var header = parent.Header;
			var targetInfo = parent.ActionInfo;
			var action = parent.Action;
			if (parent.ActionForReasonDescriptions.Contains(action) && header.PermitNumber.IsEmpty)
			{
				targetInfo.AddError(Res.GetString("32CB0CE6-1AD0-4FE0-B365-BCA79A48B156", "Please enter a Permit Number."));
			}

			if (parent.ActionForProcessingNumbers.Contains(action) && header.ProcessingNumber.IsEmpty)
			{
				targetInfo.AddError(Res.GetString("54B6FA3D-FFA7-4078-BEA2-7035AE8ABD90", "Please enter a Processing Number."));
			}
		}
	}
}
