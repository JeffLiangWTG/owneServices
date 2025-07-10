namespace Enterprise.MasterFiles.Integration.Customs.MX
{
	public interface IMXGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword GlbExternalPassword { get; }
	}
}
