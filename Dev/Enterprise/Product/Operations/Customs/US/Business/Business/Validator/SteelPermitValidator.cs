using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class SteelPermitValidator : PermitValidator
	{
		public SteelPermitValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._01, invoiceLine)
		{
		}

		protected override bool MandatoryForEntry(string entryType)
		{
			return base.MandatoryForEntry(entryType) && !EntryTypeList.IsInformal(entryType) && entryType != EntryTypeList.Codes.ConsumptionFTZ;
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
