using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestsSubclassesOf(typeof(InvoiceLineChargeLookups))]
	abstract class InvoiceLineChargeLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
		where T : InvoiceLineChargeLookups
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			charge = invoiceLine.Charges.AddNew();
			lookups = charge.Lookups as T;
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
		protected InvoiceLineCharge charge;
		protected T lookups;

		protected abstract string MessageType { get; }

		public void TestLookupsType()
		{
			AssertType<T>(charge.Lookups);
		}
	}
}
