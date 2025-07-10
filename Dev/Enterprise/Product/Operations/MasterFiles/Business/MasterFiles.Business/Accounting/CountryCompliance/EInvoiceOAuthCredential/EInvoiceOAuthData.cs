using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoiceOAuthData
	{
		public EInvoiceOAuthData(ICompany company, EInvoiceOAuthCredentialCollection oAuthCredentials)
		{
			Company = company;
			OAuthCredentials = oAuthCredentials;
		}

		public ICompany Company
		{
			get;
		}

		public EInvoiceOAuthCredentialCollection OAuthCredentials
		{
			get;
		}
	}
}
