using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class ResumeApportionmentTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestResumeApportionmentSetsCurrentApportionmentCountToZero()
		{
			//BaseJobDeclaration TestDec = BaseJobDeclaration.New(Factory);
			testDec.currentApportionment = 100;
			AssertEquals("PreCondition:CurrentApportionment", 100, testDec.currentApportionment);
			testDec.ResumeApportionment();//Nothing to apportion
			AssertEquals("CurrentApportionment", 0, testDec.currentApportionment);
		}

		public void TestCalculateTotalApportionments()
		{
			//BaseJobDeclaration TestDec = BaseJobDeclaration.New(Factory);

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, testDec.LocalCurrencyCode);
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, testDec.LocalCurrencyCode);

			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100, testDec.LocalCurrencyCode);

			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			if (!testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Contains(invoice))
			{
				testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Add(invoice);
			}

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100, testDec.LocalCurrencyCode);

			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			int invoiceChargeExpected = 1 * 3;
			int groupChargeExpected = 2 * (1 + 3);
			AssertEquals("Total Apportionments that should happen", invoiceChargeExpected + groupChargeExpected, testDec.TotalApportionments);
		}

		//jobs can be saved via data import - in this case, validation does not run - so it is important to run apportionment.
		public void TestApportionmentRunsEvenWhenValidationDoesNotRun()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				var invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";
				var invoiceCharge = invoice.Charges.AddNew();
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				invoiceCharge.J7_Amount = 1000m;
				invoiceCharge.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 9000m;

				AssertEquals("Pre-condition", 0, invoiceLine.ApportionedCharges.Count);
				AssertEquals("Pre-condition", true, testDec.ApportionmentDirty);
				Factory.Save();
				AssertEquals(false, testDec.ApportionmentDirty);
				AssertEquals(1, invoiceLine.ApportionedCharges.Count);

				//test that apportionment occurs whether the job is in the DB or not
				invoiceCharge.J7_Amount = 500m;
				AssertEquals("Pre-condition", true, testDec.ApportionmentDirty);
				Factory.Save();
				AssertEquals(false, testDec.ApportionmentDirty);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
		}
		BaseJobDeclaration testDec;
	}
}
