namespace Enterprise.MasterFiles.Integration.Customs.UY
{
	public interface IUYGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword GlbExternalPassword { get; }
	}
}
