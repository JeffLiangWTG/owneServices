using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobComInvHeaderChargeTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoadingForInvoiceCharge()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();

			var charge = Factory.New<BaseInvoiceCharge>();
			charge.J7_IsApportionedCharge = false;
			charge.J7_ParentTableCode = "JZ";
			charge.J7_ParentID = invoice.PK;

			TypeDecider chargeDecider = new BaseJobComInvHeaderChargeTypeDecider();
			Assert("Type for InvCharge", typeof(BaseInvoiceCharge).IsAssignableFrom(chargeDecider.GetTypeForLoad(((INeedRow)charge).Row, Factory)));
		}

		public void TestGetTypeForLoadingForInvoiceApportionedCharge()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();

			var charge = Factory.New<BaseApportionedCharge>();
			charge.J7_IsApportionedCharge = true;
			charge.J7_ParentTableCode = "JZ";
			charge.J7_ParentID = invoice.PK;

			TypeDecider chargeDecider = new BaseJobComInvHeaderChargeTypeDecider();
			AssertEquals("Type for InvCharge", typeof(BaseApportionedCharge), chargeDecider.GetTypeForLoad(((INeedRow)charge).Row, Factory));
		}

		public void TestGetTypeForLoadingForInvoiceLineCharge()
		{
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var charge = Factory.New<BaseInvoiceLineCharge>();
			charge.J7_IsApportionedCharge = false;
			charge.J7_ParentTableCode = "JI";
			charge.J7_ParentID = invoiceLine.PK;

			TypeDecider chargeDecider = new BaseJobComInvHeaderChargeTypeDecider();
			AssertEquals("Type for InvCharge", typeof(BaseInvoiceLineCharge), chargeDecider.GetTypeForLoad(((INeedRow)charge).Row, Factory));
		}

		public void TestGetTypeForLoadingForInvoiceLineApportionedCharge()
		{
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var charge = Factory.New<BaseInvoiceLineApportionedCharge>();
			charge.J7_IsApportionedCharge = true;
			charge.J7_ParentTableCode = "JI";
			charge.J7_ParentID = invoiceLine.PK;

			TypeDecider chargeDecider = new BaseJobComInvHeaderChargeTypeDecider();
			AssertEquals("Type for InvCharge", typeof(BaseInvoiceLineApportionedCharge), chargeDecider.GetTypeForLoad(((INeedRow)charge).Row, Factory));
		}

		public void TestGetTypeForLoadingForGroupInvoice()
		{
			BaseJobComInvoiceGroupHeader groupHeader = Factory.New<BaseJobComInvoiceGroupHeader>();

			var charge = Factory.New<BaseGroupInvoiceCharge>();
			charge.J7_IsApportionedCharge = true;
			charge.J7_ParentTableCode = "JZ";
			charge.J7_ParentID = groupHeader.PK;

			TypeDecider chargeDecider = new BaseJobComInvHeaderChargeTypeDecider();
			AssertEquals("Type for InvCharge", typeof(BaseGroupInvoiceCharge), chargeDecider.GetTypeForLoad(((INeedRow)charge).Row, Factory));
		}
	}
}
