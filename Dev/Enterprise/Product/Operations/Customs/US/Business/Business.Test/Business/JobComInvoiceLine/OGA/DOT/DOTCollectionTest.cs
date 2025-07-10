using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOTCollection))]
	public class DOTCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			InvoiceLine.JI_Description = "TEST";

			DOT dot = DOTs.AddNew();
			AssertEquals(InvoiceLine.JI_Description, dot.US_DOTCommercialDesc);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DOTs;
		}

		DOTCollection DOTs
		{
			get { return dots ?? (dots = InvoiceLine.DOTs); }
		}
		DOTCollection dots;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
