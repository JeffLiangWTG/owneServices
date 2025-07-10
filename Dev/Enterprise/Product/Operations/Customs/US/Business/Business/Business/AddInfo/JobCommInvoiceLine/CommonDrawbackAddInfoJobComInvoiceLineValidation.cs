
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CommonDrawbackAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public CommonDrawbackAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent
		{
			get { return base.Parent.InvoiceLine; }
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return Parent; }
		}

		protected override void CheckUS_DRWExportAction()
		{
			base.CheckUS_DRWExportAction();
			if (Parent.US_DRWIsForExportSection)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_DRWExportActionInfo, Parent.Lookups.DRWExportActionList);
			}
		}

		protected override void CheckUS_ImportEntryNo()
		{
			base.CheckUS_ImportEntryNo();
			if (Parent.US_DRWIsForImportSection)
			{
				if (Parent.US_ImportEntryNo.Length > 0 && Parent.US_ImportEntryNo.Length != 11)
				{
					Parent.US_ImportEntryNoInfo.AddMessageError(ImportEntryLength);
				}
			}
		}
		internal const string ImportEntryLength = "The Import Entry Number should be 11 characters long";

		protected override void CheckUS_ExportTariff()
		{
			base.CheckUS_ExportTariff();
			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ExportTariffInfo);
				if (ShouldCheckTariffIfInvalid)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_ExportTariffInfo, Parent.Lookups.ExportTariffs);
				}
				if (Parent.Declaration.US_PetroleumClaimInd && Parent.US_ExportTariff.Length < 8)
				{
					Parent.US_ExportTariffInfo.AddMessageError(CommonDrawbackJobComInvoiceLineValidation.PetroliumTariffValidation);
				}
			}
		}

		protected virtual bool ShouldCheckTariffIfInvalid
		{
			get { return Parent.US_ExportTariff.Length >= 8; }
		}

		protected override void CheckUS_DRWExportDate()
		{
			base.CheckUS_DRWExportDate();
			if (Parent.US_DRWIsForExportSection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(InvoiceLine.US_DRWExportDateInfo);

				if (InvoiceLine.US_DRWExportDate.IsValid)
				{
					var declaration = InvoiceLine.Declaration;
					if (declaration.IsDrawbackTFTEA)
					{
						if (declaration.US_EstimatedEntryDate.IsValid && declaration.US_EstimatedEntryDate > InvoiceLine.US_DRWExportDate.AddYears(5))
						{
							InvoiceLine.US_DRWExportDateInfo.AddMessageError(ExportDateGreaterThanClaimDateWhenTFTEA);
						}
						if (InvoiceLine.US_DRWEntryDate.IsValid && InvoiceLine.US_DRWExportDate > InvoiceLine.US_DRWEntryDate.AddYears(5))
						{
							InvoiceLine.US_DRWExportDateInfo.AddMessageError(ExportDateGreaterThanImportDateWhenTFTEA);
						}
					}
					else
					{
						if (declaration.US_EstimatedEntryDate.IsValid && declaration.US_EstimatedEntryDate > InvoiceLine.US_DRWExportDate.AddYears(3))
						{
							InvoiceLine.US_DRWExportDateInfo.AddMessageError(ExportDateGreaterThanClaimDate);
						}
						if (InvoiceLine.US_DRWEntryDate.IsValid && InvoiceLine.US_DRWExportDate > InvoiceLine.US_DRWEntryDate.AddYears(3))
						{
							InvoiceLine.US_DRWExportDateInfo.AddMessageError(ExportDateGreaterThanImportDate);
						}
					}
				}
			}
		}
		internal const string ExportDateGreaterThanClaimDateWhenTFTEA = "Export Date should be no more than 5 years older than claim date.";
		internal const string ExportDateGreaterThanClaimDate = "Export Date should be no more than 3 years older than claim date.";
		internal const string ExportDateGreaterThanImportDateWhenTFTEA = "Export Date greater than 5 years from Import Date.";
		internal const string ExportDateGreaterThanImportDate = "Export Date greater than 3 years from Import Date.";
	}
}
