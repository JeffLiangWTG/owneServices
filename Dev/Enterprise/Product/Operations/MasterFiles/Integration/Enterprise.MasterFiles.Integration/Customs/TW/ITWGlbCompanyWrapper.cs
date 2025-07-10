namespace Enterprise.MasterFiles.Integration.Customs.TW
{
	public interface ITWGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword ForwarderCertificate { get; }
		IGlbExternalPassword LicensingCertificate { get; }
	}
}
