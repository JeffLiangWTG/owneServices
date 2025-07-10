namespace Enterprise.MasterFiles.Integration.Customs.IE
{
	public interface IIEGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPasswordWithCertificate GetGlbExternalPassword();
		IGlbExternalPasswordWithCertificate GetGlbExternalPasswordOrCreateNew();
		IGlbExternalPasswordCollection_IEEMCS GetEMCSGlbExternalPasswordCollectionOrCreateNew();
	}
}
