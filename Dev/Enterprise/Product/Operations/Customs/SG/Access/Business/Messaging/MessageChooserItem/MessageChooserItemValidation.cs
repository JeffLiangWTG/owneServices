namespace Enterprise.Customs.SG.Access.Business
{
	public class MessageChooserItemValidation : ASYCUDA.Business.MessageChooserItemValidation
	{
		public MessageChooserItemValidation(MessageChooserItem parent)
			: base(parent)
		{
		}

		protected override void CheckChecked()
		{
			base.CheckChecked();
			if (Parent.RequiresCycleFields)
			{
				var chooser = Parent.Chooser;
				if (chooser != null)
				{
					var bill = Parent.Bill as AsycudaBill;
					if (bill != null && (chooser.CycleDate != bill.CycleDate || chooser.CycleNumber != bill.CycleNumber))
					{
						Parent.CheckedInfo.AddWarning("The Bill Cycle details do not match the Submission Cycle details.");
					}
				}
			}
		}

		protected new MessageChooserItem Parent => (MessageChooserItem)base.Parent;
	}
}
