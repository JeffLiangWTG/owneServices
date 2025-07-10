using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FCCCollection))]
	public class FCCCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			InvoiceLine.JI_Description = "TEST";

			FCC fcc = FCCs.AddNew();
			AssertEquals(InvoiceLine.JI_Description, fcc.US_FCCCommercialDesc);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return FCCs;
		}

		FCCCollection FCCs
		{
			get { return fccs ?? (fccs = InvoiceLine.FCCs); }
		}
		FCCCollection fccs;

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
