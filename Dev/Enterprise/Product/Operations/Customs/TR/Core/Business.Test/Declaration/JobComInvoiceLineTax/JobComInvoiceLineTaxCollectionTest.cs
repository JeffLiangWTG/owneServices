using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	sealed class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasType()
		{
			var invoiceLineTaxCollection = ComInvoiceLineTaxCollection;
			var invoiceLineTax = invoiceLineTaxCollection.AddNew();
			invoiceLineTax.JLT_Type = "D10";
			Assert(invoiceLineTaxCollection.Cast<JobComInvoiceLineTax>().Any(x => x.JLT_Type == "D10"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return ComInvoiceLineTaxCollection;
		}

		JobComInvoiceLineTaxCollection ComInvoiceLineTaxCollection
		{
			get
			{
				return fJobComInvoiceLineTaxCollection ?? (fJobComInvoiceLineTaxCollection = new JobComInvoiceLineTaxCollection(InvoiceLine));
			}
		}

		JobComInvoiceLineTaxCollection fJobComInvoiceLineTaxCollection;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
