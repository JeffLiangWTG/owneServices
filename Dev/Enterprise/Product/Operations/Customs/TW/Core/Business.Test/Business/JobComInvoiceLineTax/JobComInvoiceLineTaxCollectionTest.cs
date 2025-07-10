using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTaxCollection))]
	sealed class JobComInvoiceLineTaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasType()
		{
			var invoiceLineTaxCollection = ComInvoiceLineTaxCollection;
			var invoiceLineTax = invoiceLineTaxCollection.AddNew();
			invoiceLineTax.JLT_Type = "CT";
			invoiceLineTax.JLT_Tariff = "xxx";
			Assert(invoiceLineTaxCollection.HasType("CT"));
			Assert(!invoiceLineTaxCollection.HasType("XX"));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return ComInvoiceLineTaxCollection;
		}

		#region ComInvoiceLineTaxCollection
		JobComInvoiceLineTaxCollection ComInvoiceLineTaxCollection
		{
			get
			{
				return fJobComInvoiceLineTaxCollection ?? (fJobComInvoiceLineTaxCollection = new JobComInvoiceLineTaxCollection(InvoiceLine));
			}
		}

		JobComInvoiceLineTaxCollection fJobComInvoiceLineTaxCollection;
		#endregion
		#region InvoiceLine
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
	}
}
