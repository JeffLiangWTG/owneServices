using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobComInvoiceLineCollection))]
	sealed class DocJobComInvoiceLineCollectionTests : DocBaseJobComInvoiceLineCollectionTest<DocJobComInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			return DocJobComInvoiceLine.New(jobComInvoiceLine, Factory);
		}

		protected override DocJobComInvoiceLineCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceLineCollection(Factory);
		}
	}
}
