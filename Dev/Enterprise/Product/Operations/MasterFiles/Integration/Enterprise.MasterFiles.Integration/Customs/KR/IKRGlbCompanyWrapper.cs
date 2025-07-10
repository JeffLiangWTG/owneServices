namespace Enterprise.MasterFiles.Integration.Customs.KR
{
	public interface IKRGlbCompanyWrapper : IGlbCompanyWrapper
	{
		IGlbExternalPassword CertificateForUnipass { get; }
	}
}
