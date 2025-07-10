namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IEInvoicingRegistryProvider
	{
		public bool ShouldAutoSetEReportingComplianceDate { get; }
	}
}
