using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class AluminumImportLicenseValidator : PermitValidator
	{
		public AluminumImportLicenseValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._28, invoiceLine)
		{
		}

		protected override bool MandatoryForEntry(string entryType)
		{
			return base.MandatoryForEntry(entryType) && entryType != EntryTypeList.Codes.ConsumptionFTZ;
		}

		public override bool IsPermitNoNotRequired()
		{
			return InvoiceLine.ImportEntryType == EntryTypeList.Codes.ConsumptionFTZ;
		}

		public override ZString GetErrorTextForPermitNotRequired()
		{
			return $"The {PermitDescription} number is not permitted for FTZ withdrawals.";
		}
	}
}
