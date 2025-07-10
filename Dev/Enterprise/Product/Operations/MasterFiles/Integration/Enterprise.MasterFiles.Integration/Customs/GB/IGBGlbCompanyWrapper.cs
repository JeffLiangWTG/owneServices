namespace Enterprise.MasterFiles.Integration.Customs.GB
{
	public interface IGBGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPasswordCollection_GB GBBPasswordCollection { get; }
	}
}
