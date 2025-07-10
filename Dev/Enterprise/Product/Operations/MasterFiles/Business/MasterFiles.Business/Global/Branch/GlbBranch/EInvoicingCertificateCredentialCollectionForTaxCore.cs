using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoicingCertificateCredentialCollectionForTaxCore : EInvoicingCertificateCollection<EInvoicingCertificateCredentialForTaxCore, GlbBranch>
	{
		public EInvoicingCertificateCredentialCollectionForTaxCore(GlbBranch master)
			: base(master)
		{
			var objectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			var countryEInvoicingSettings = objectFactory.GetCountryEInvoicingObjectFactorySettings(master.Company?.GC_RN_NKCountryCode ?? string.Empty);
			CredentialSettings = countryEInvoicingSettings?.Credentials as IEInvoicingCertificateCredentialSettings;
		}

		protected override ZQuery CreateAdditionalFilter()
			=> new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.FPC);

		protected override bool IsEditAllowedCore => Master.Company.IsInTaxCoreSupportedCountry();
	}
}
