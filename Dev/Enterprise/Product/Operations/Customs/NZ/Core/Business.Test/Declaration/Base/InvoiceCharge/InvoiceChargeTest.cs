using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		protected override void SetupAllTestObjects()
		{
			TestDec = Factory.New<JobDeclaration>();
			Invoice = TestDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			TestCharge = Invoice.Charges.AddNew();
		}

		protected JobDeclaration TestDec
		{
			get { return (JobDeclaration)base.testDec; }
			set { base.testDec = value; }
		}
		protected JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.invoice; }
			set { base.invoice = value; }
		}
		protected InvoiceCharge TestCharge
		{
			get { return (InvoiceCharge)base.testCharge; }
			set { base.testCharge = value; }
		}
	}
	public class InvoiceChargePrepaidCollectTest : Customs.Business.Testing.BaseInvoiceChargePrepaidCollectTest
	{
		#region Implementation
		protected JobDeclaration TestDec
		{
			get { return (JobDeclaration)base.testDec; }
			set { base.testDec = value; }
		}
		protected JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.groupHeader; }
			set { base.groupHeader = value; }
		}
		protected JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.invoice; }
			set { base.invoice = value; }
		}

		protected override void SetupAllTestObjects()
		{
			TestDec = Factory.New<JobDeclaration>();
			GroupHeader = TestDec.JobComInvoiceGroupHeaders[0];
			Invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			Invoice.JZ_InvoiceAmount = 10000;
			Invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
		}

		protected override Customs.Business.BaseJobComInvHeaderCharge GetNewInvoiceChargeOnInvoice(string chargeName)
		{
			return Invoice.Charges.AddNew(chargeName);
		}
		#endregion
	}
}
