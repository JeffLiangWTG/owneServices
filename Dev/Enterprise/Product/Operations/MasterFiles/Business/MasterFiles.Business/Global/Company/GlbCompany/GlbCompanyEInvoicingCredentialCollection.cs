using CargoWise.Application;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyEInvoicingCredentialCollection : EInvoicingCertificateCollection<GlbCompanyEInvoicingCertificateCredential, GlbCompany>
	{
		public GlbCompanyEInvoicingCredentialCollection(GlbCompany master) : base(master)
		{
			var objectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			var countryEInvoicingSettings = objectFactory.GetCountryEInvoicingObjectFactorySettings(master.GC_RN_NKCountryCode);
			CredentialSettings = countryEInvoicingSettings?.Credentials as IEInvoicingCertificateCredentialSettings;
		}

		protected override bool IsEditAllowedCore => CredentialSettings?.IsCompanyCredentialsRequired ?? false;
	}
}
