using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineViewCollection))]
	public class JobComInvoiceLineCollectionTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionViewTestCase<JobComInvoiceLineViewCollection>
	{
		protected override JobComInvoiceLineViewCollection GetCollectionToTest()
		{
			return new JobComInvoiceLineViewCollection(InvoiceHeader, new InvoiceLineCompleteCollection(Declaration));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobComInvoiceLine result = Factory.New<JobComInvoiceLine>();
			result.JI_JZ = InvoiceHeader.PK;
			return result;
		}

		JobDeclaration fDeclaration;
		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}

		JobComInvoiceHeader fInvoiceHeader;
		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.Invoices.AddNew();
				}
				return fInvoiceHeader;
			}
		}
	}
}
