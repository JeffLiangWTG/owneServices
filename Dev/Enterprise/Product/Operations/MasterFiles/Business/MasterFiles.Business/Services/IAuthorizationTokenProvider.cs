namespace Enterprise.MasterFiles.Business
{
	public interface IAuthorizationTokenProvider
	{
		string GetToken(AuthenticationCertificateInfo certificateInfo);
	}
}
