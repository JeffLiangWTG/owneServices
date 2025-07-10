using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchEInvoicingCredentialCollection : EInvoicingCertificateCollection<GlbBranchEInvoicingCertificateCredential, GlbBranch>
	{
		public GlbBranchEInvoicingCredentialCollection(GlbBranch master) : base(master)
		{
			var objectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			var countryEInvoicingSettings = objectFactory.GetCountryEInvoicingObjectFactorySettings(master.Company?.GC_RN_NKCountryCode ?? string.Empty);
			CredentialSettings = countryEInvoicingSettings?.Credentials as IEInvoicingCertificateCredentialSettings;
		}

		protected override bool IsEditAllowedCore => CredentialSettings?.IsBranchCredentialsRequired ?? false;

		protected override bool AllowNewCore => IsEditAllowedCore;

		protected override bool AllowRemoveCore => IsEditAllowedCore;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is EInvoicingCertificateCredential credentialBizo)
			{
				credentialBizo.GP_GB = Master.PK;
				credentialBizo.GP_GC = Master.GB_GC;
			}
		}
	}
}
