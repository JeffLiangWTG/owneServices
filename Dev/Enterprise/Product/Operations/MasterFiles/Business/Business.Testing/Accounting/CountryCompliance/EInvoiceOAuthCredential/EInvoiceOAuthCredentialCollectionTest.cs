using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.CountryCompliance
{
	[TestedType(typeof(EInvoiceOAuthCredentialCollection))]
	sealed class EInvoiceOAuthCredentialCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EInvoiceOAuthCredentialCollection>
	{
		protected override EInvoiceOAuthCredentialCollection GetCollectionToTest()
		{
			return new EInvoiceOAuthCredentialCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EInvoiceOAuthCredential();
		}
	}
}
