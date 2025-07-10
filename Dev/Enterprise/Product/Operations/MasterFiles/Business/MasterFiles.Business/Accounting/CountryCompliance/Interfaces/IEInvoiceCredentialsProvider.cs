using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.CountryCompliance
{
	public interface IEInvoiceCredentialsProvider
	{
		bool ShouldShowCredentialsTab(ICompany company);

		bool ShouldShowCertificatesTab(ICompany company);

		string GetAuthorizationURL(ICompany company);

		void CreateOrUpdateEInvoicingCredential(ICompany company);

		EInvoiceOAuthData CreateOAuthData(ICompany company);
	}
}
