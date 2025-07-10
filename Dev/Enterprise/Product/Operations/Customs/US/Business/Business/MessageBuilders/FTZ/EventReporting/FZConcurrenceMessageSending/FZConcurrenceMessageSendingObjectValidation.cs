namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZConcurrenceMessageSendingObjectValidation : AutoFZConcurrenceMessageSendingObjectValidation
	{
		public FZConcurrenceMessageSendingObjectValidation(AutoFZConcurrenceMessageSendingObject parent)
			: base(parent)
		{
		}

		#region Implementation

		public new AutoFZConcurrenceMessageSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Parent; }
		}

		#endregion
	}
}
