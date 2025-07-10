namespace Enterprise.Customs.US.Business
{
	public class ReconAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public ReconAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader addInfo)
			: base(addInfo)
		{
		}

		protected override void CheckUS_CH_ReconEntry()
		{
			base.CheckUS_CH_ReconEntry();

			if (Parent.ReconOriginalEntry == null)
			{
				Parent.US_CH_ReconEntryInfo.AddMessageError(ReconEntryIsMandatory);
			}
		}

		internal const string ReconEntryIsMandatory = "You should select a valid original import entry this invoice is attached to.";
	}
}
