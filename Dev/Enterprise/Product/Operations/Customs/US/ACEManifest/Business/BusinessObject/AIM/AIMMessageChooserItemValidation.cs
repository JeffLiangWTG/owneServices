using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessageChooserItemValidation : MessageChooserItemValidation
	{
		public AIMMessageChooserItemValidation(AutoMessageChooserItem parent)
			: base(parent)
		{
		}

		protected override void CheckChecked()
		{
			base.CheckChecked();
			if (Parent.Checked && Parent.Bill is ASYCUDA.Business.AsycudaBill bill)
			{
				var messageStatus = bill.ABL_MessageStatus;
				var messageChooser = (AIMMessageChooser)Parent.Chooser;

				if (messageChooser.IsSending && (messageStatus == MessageStatusCodeList.Codes.Sent || messageStatus == MessageStatusCodeList.Codes.Registered))
				{
					Parent.CheckedInfo.AddMessageError($"Bill {bill.ABL_BillNumber} has already been sent.");
				}
				else if ((messageChooser.IsChanging || messageChooser.IsCancelling) && messageStatus.IsEmpty)
				{
					Parent.CheckedInfo.AddMessageError($"Bill {bill.ABL_BillNumber} has not been sent to Customs yet.");
				}
			}
		}
	}
}
