namespace Enterprise.MasterFiles.Integration.Customs.CH
{
	public interface ICHGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword GlbExternalPassword { get; }
	}
}
