using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackJobComInvoiceLineValidation : CommonDrawbackJobComInvoiceLineValidation
	{
		public ACEDrawbackJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateUS_OA_DRWExporterOrDestroyer();
		}

		protected override void CheckJI_Description()
		{
			if (Parent.US_DRWIsForImportSection || Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_DescriptionInfo);
			}

			if (Parent.JI_Description.Length > 50)
			{
				Parent.JI_DescriptionInfo.AddWarning(DescriptionIsTooLong);
			}
		}
		internal const string DescriptionIsTooLong = "Description is too long. Message will be sent using the first 50 characters only.";

		public void ValidateUS_OA_DRWExporterOrDestroyer()
		{
			ValidateCalculatedProperty(Parent.US_OA_DRWExporterOrDestroyerInfo);
		}

		protected void CheckUS_OA_DRWExporterOrDestroyer()
		{
			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_DRWExporterOrDestroyerInfo);
			}
			if (!Parent.US_OA_DRWExporterOrDestroyer.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_DRWExporterOrDestroyerInfo, Parent.ExporterOrDestroyer.Address);
			}
		}
	}
}
