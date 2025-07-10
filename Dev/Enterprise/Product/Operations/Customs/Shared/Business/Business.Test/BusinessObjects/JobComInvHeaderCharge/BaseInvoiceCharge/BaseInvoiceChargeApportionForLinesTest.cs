using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseInvoiceChargeApportionForLinesTest : TestCaseWithFactory
	{
		public void TestChangingAmountLeadsToReapportion()
		{
			BaseJobComInvHeaderCharge invoiceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("PreCondition: OTH amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceCharge.J7_Amount = 900m;
			testDec.ResumeApportionment();
			AssertEquals("Reapportioned OTH", 900m, invoiceLine.ApportionedCharges[0].J7_Amount);
		}

		public void TestChangingCurrencyLeadsToReapportion()
		{
			BaseJobComInvHeaderCharge invoiceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("PreCondition: OTH amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceCharge.J7_RX_NKCurrency = "USD";
			testDec.ResumeApportionment();
			AssertEquals("Reapportioned OTH", "USD", invoiceLine.ApportionedCharges[0].J7_RX_NKCurrency);
		}

		public void TestApportionZeroAmountWithValidCurrency()
		{
			BaseJobComInvHeaderCharge invoiceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("PreCondition: OTH amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceCharge.J7_Amount = 0m;
			invoiceCharge.J7_RX_NKCurrency = invoice.JobDeclaration.LocalCurrencyCode;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned charge still there", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("Zero amount with a valid currency still needs to be apportioned", 0m, invoiceLine.ApportionedCharges[0].J7_Amount);
		}

		public void TestClearingCurrencyRemovesApportionedCharges()
		{
			BaseJobComInvHeaderCharge invoiceCharge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			invoiceCharge.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned", 1, invoiceLine.ApportionedCharges.Count);
			AssertEquals("PreCondition: OTH amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceCharge.J7_RX_NKCurrency = ZString.Empty;
			testDec.ResumeApportionment();
			AssertEquals("Apportioned charge for invoice line is cleared now", 0, invoiceLine.ApportionedCharges.Count);
		}

		public void TestChangingChargeTypeWhenValidCurrencyIsThere()
		{
			BaseJobComInvHeaderCharge oTH1 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH1.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge oTH2 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH2.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned to line 1", 1, invoiceLine.ApportionedCharges.Count);

			oTH1.J7_ChargeType = CustomsChargeTypeList.Codes.ExWorks;
			testDec.ResumeApportionment();
			AssertEquals("There are two apportioned charges now", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("First amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("Second amount", 1000m, invoiceLine.ApportionedCharges[1].J7_Amount);
		}

		public void TestChangingIsDutiableWhenValidCurrencyChargeTypeAreThere()
		{
			BaseJobComInvHeaderCharge oTH1 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH1.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge oTH2 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH2.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned to line 1", 1, invoiceLine.ApportionedCharges.Count);

			oTH1.J7_IsDutiable = !oTH1.J7_IsDutiable;
			testDec.ResumeApportionment();
			AssertEquals("There are two apportioned charges now", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("First amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("Second amount", 1000m, invoiceLine.ApportionedCharges[1].J7_Amount);
		}

		public void TestChangingIsGSTWhenValidCurrencyAndChargeTypeAreThere()
		{
			BaseJobComInvHeaderCharge oTH1 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH1.J7_IsIncludedInITOT = true;

			BaseJobComInvHeaderCharge oTH2 = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 1000m, invoice.JobDeclaration.LocalCurrencyCode);
			oTH2.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
			testDec.ResumeApportionment();
			AssertEquals("PreCondition: OTH is apportioned to line 1", 1, invoiceLine.ApportionedCharges.Count);

			oTH1.J7_IsGSTApplicable = !oTH1.J7_IsGSTApplicable;
			testDec.ResumeApportionment();
			AssertEquals("There are two apportioned charges now", 2, invoiceLine.ApportionedCharges.Count);
			AssertEquals("First amount", 1000m, invoiceLine.ApportionedCharges[0].J7_Amount);
			AssertEquals("Second amount", 1000m, invoiceLine.ApportionedCharges[1].J7_Amount);
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
			distributeByForExport = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;
	}
}
