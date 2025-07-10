namespace Enterprise.MasterFiles.Integration.Customs.PL
{
	public interface IPLGlbStaffWrapper : IGlbStaffWrapper
	{
		IGlbExternalPasswordWithCertificate PLBPassword { get; }
	}
}
