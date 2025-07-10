using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	abstract class InvoiceChargeLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			charge = declaration.Invoices.AddNew().Charges.AddNew();
		}

		protected JobDeclaration declaration;
		protected InvoiceCharge charge;

		protected abstract string MessageType { get; }
	}
}
