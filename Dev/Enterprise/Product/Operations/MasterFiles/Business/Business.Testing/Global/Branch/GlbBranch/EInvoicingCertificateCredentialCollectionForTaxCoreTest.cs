using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateCredentialCollectionForTaxCore))]
	public sealed class EInvoicingCertificateCredentialCollectionForTaxCoreTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			return new EInvoicingCertificateCredentialCollectionForTaxCore(branch);
		}

		public void TestIsEditAllowed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var collection = (EInvoicingCertificateCredentialCollectionForTaxCore)GetCollectionToTest();
				Assert(nameof(collection.IsEditAllowed), !collection.IsEditAllowed);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Fiji))
			{
				var collection = (EInvoicingCertificateCredentialCollectionForTaxCore)GetCollectionToTest();
				Assert(nameof(collection.IsEditAllowed), collection.IsEditAllowed);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.WesternSamoa))
			{
				var collection = (EInvoicingCertificateCredentialCollectionForTaxCore)GetCollectionToTest();
				Assert(nameof(collection.IsEditAllowed), collection.IsEditAllowed);
			}
		}

		public void TestNewCollection_WithNullCompany_DoesNotThrow()
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = ZGuid.Empty;
			AssertNoExceptionThrown(() => new EInvoicingCertificateCredentialCollectionForTaxCore(branch));
		}
	}
}
