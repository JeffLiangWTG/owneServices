namespace Enterprise.Customs.US.Business
{
	class CottonShirtingFabricLicenseNumberValidator : PermitValidator
	{
		public CottonShirtingFabricLicenseNumberValidator(JobComInvoiceLine invoiceLine)
			: base(LicencePermitTypeList.Codes._12, invoiceLine)
		{
		}

		protected override bool MandatoryForEntry(string entryType)
		{
			return entryType != EntryTypeList.Codes.Warehouse && entryType != EntryTypeList.Codes.ReWarehouse;
		}
	}
}
