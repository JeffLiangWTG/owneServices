using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoiceOAuthCredentialCollection : NonPersistentBusinessObjectCollection<EInvoiceOAuthCredential>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EInvoiceOAuthCredential();
		}

		protected override bool AllowNewCore => false;
	}
}
