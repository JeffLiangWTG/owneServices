namespace Enterprise.MasterFiles.Integration.Customs.BE
{
	public interface IBEGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword BrokerageCredentials { get; }
	}
}
