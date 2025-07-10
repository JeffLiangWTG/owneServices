using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	sealed class JobComInvoiceLineViewCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return new JobComInvoiceLineViewCollection(InvoiceHeader, new InvoiceLineCompleteCollection(Declaration));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<JobComInvoiceLine>();
			result.JI_JZ = InvoiceHeader.PK;
			return result;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceHeader InvoiceHeader => invoiceHeader ?? (invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew());
	}

	sealed class JobComInvoiceLineCollectionTest : BaseJobComInvoiceLineCollectionTest
	{
		protected override string ValidTariffNumber => "8708.93.25 3";
	}
}
