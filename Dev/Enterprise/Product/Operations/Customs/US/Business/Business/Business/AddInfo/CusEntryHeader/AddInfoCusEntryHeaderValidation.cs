namespace Enterprise.Customs.US.Business
{
	public class AddInfoCusEntryHeaderValidation : USAddInfoValidation
	{
		public AddInfoCusEntryHeaderValidation(AddInfoCusEntryHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoCusEntryHeader Parent
		{
			get { return (AddInfoCusEntryHeader)base.Parent; }
		}

		protected AddInfoCusEntryHeaderLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckUS_IsDeactivated()
		{
			base.CheckUS_IsDeactivated();
			Parent.EntryHeader.Validation.ValidateCH_BGMReference();
		}

		protected override void CheckUS_ShouldBeReportToCustoms()
		{
			base.CheckUS_ShouldBeReportToCustoms();

			var declaration = Parent.Declaration;

			if (Parent.US_ShouldBeReportToCustoms && declaration != null && declaration.ExportMessageStatus.IsWaitingForResponse(Parent.EntryHeader.CH_Status))
			{
				Parent.US_ShouldBeReportToCustomsInfo.AddMessageError(PendingResponse);
			}
		}
		internal const string PendingResponse = "There is a pending response. Please wait until it is received.";
	}
}
