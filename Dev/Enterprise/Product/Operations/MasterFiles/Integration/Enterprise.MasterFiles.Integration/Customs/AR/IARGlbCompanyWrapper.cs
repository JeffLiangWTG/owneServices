namespace Enterprise.MasterFiles.Integration.Customs.AR
{
	public interface IARGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword GlbExternalPassword { get; }
	}
}
