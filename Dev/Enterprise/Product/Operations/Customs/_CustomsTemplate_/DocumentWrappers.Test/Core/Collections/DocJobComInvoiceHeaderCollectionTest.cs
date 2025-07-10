using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeaderCollection))]
	sealed class DocJobComInvoiceHeaderCollectionTests : DocBaseJobComInvoiceHeaderCollectionTest<DocJobComInvoiceHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
			return DocJobComInvoiceHeader.New(jobComInvoiceHeader, Factory);
		}

		protected override DocJobComInvoiceHeaderCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceHeaderCollection(Factory);
		}
	}
}
