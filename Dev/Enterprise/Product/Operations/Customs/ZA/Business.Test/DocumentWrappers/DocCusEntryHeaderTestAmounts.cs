using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DocCusEntryHeaderTestAmounts : TestDataClassForZAJobDeclaration
	{
		public void TestCustomsValue()
		{
			mockCusEntryHeader.Setup(m => m.CustomsValue).Returns(new ZDecimal(391692.80m));
			AssertEquals("Customs value", 391693m, entryHeaderWrapper.CustomsValue);
		}

		public void TestTransactionValue()
		{
			Assert("There must be at least one invoiceheader attached to the EntryHeader the test is working with", entryHeader.InvoiceHeaders.Length > 0);
			JobComInvoiceHeader header = entryHeader.InvoiceHeaders[0];
			header.JZ_InvoiceAmount = 391692.80m;
			header.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("TransactionValue", 391692.80m, entryHeader.TransactionValue);
		}

		public void TestDutySch1P2B()
		{
			entryLine.Fees.AddOrUpdate("12B", 391692.80m);
			AssertEquals("DutySch1P2B", 391692.80m, entryHeaderWrapper.DutySch1P2B);
		}

		public void TestVAT()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 300.10m);
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 392.24m);
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals("VAT", 692.34M, entryHeaderWrapper.TotalVAT);
		}

		public void TestDuty()
		{
			entryLine.Fees.AddOrUpdate("1P1", 3058.7m);
			AssertEquals("Temporary fix. It should be fixed properly byCustoms team. Duty Amount", 0m, entryHeaderWrapper.CustomsDuty);
		}

		public void TestValueAddedTax()
		{
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 300.10m);
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, 392.24m);
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals("ValueAddedTax", 692.34m, entryHeaderWrapper.ValueAddedTax);
		}

		[TestDate(2003, 7, 1)]
		public void TestTodaysDateFormat()
		{
			AssertEquals("2003 07 01", entryHeaderWrapper.TodaysDate);
		}

		#region Implmentation

		DocCusEntryHeader entryHeaderWrapper;
		Mock<CusEntryHeader> mockCusEntryHeader;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			var declaration = Factory.New<JobDeclaration>();
			mockCusEntryHeader = Factory.NewMoq<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(mockCusEntryHeader.Object);
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			var invoiceHeaders = declaration.Invoices.ToArray<JobComInvoiceHeader>();
			Assert(invoiceHeaders.Length > 0);
			entryHeader = mockCusEntryHeader.Object;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Assert(entryHeader.InvoiceHeaders.Length > 0);
			entryHeaderWrapper = DocCusEntryHeader.New(mockCusEntryHeader.Object, Factory);
		}

		#endregion
	}
}
