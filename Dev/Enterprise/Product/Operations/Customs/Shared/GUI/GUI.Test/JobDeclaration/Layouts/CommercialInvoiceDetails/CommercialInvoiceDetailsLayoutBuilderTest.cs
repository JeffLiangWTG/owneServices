using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>))]
	sealed class CommercialInvoiceDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>, BaseJobComInvoiceHeader, CommercialInvoiceDetailsControlBag>
	{
		protected override CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader> GetColumnLayoutBuilderForTesting()
		{
			return new CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>();
		}

		protected override int ExpectedMaxColumns => 1;
	}
}
