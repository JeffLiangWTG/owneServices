using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StandaloneCommonInvoiceProvider))]
	class StandaloneCommonInvoiceProviderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestICommonInvoiceDataProviderMembers()
		{
			ICommonInvoiceDataProvider provider = new StandaloneCommonInvoiceProvider(Invoice);
			AssertSame("CustomsFileParent", Invoice, provider.CustomsFileParent);
		}

		BaseJobComInvoiceHeader Invoice => invoice ??= Factory.New<BaseJobComInvoiceHeader>();
		BaseJobComInvoiceHeader invoice;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StandaloneCommonInvoiceProvider(Invoice);
		}
	}
}
