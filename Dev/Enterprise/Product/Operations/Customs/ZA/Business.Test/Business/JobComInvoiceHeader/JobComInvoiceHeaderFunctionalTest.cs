using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderFunctionalTest : BaseJobComInvoiceHeaderFunctionalTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
		#endregion

		public override void TestCalculateFOB_CIFNonDutiablePreFOB()
		{
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV1";
			invoice.JZ_InvoiceAmount = 180848.58m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

			BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			oFT.J7_Amount = 9280m;

			BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight);
			nonDutiableFIFT.J7_Amount = 2755.90m;
			nonDutiableFIFT.J7_IsDutiable = false;
			nonDutiableFIFT.J7_IsGSTApplicable = true;
			nonDutiableFIFT.J7_IsIncludedInITOT = true;

			BaseJobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
			invoiceLine.JI_LinePrice = 171568.58m;

			AssertEquals("FOB value", 168812.68m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("FOB line", 168812.68m, invoiceLine.JI_Calc_FOB);
		}

		[TestDate(2016, 01, 01)]
		public override void TestExceptionInSettingExchangeRate()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
				declaration.JE_MasterBillIssuedDate = ZDateTime.Invalid;
				// todo: should it really be empty? CusEntryHeaderTest.TestFOBInLocalCurrency requires setting JZ_ValuationDateOverride because of it
				AssertEquals("DateForRate 01", ZDateTime.Empty, invoiceHeader.CurrencyConverter.DateForRate);
				declaration.JE_MasterBillIssuedDate = ZDateTime.Today;
				AssertEquals("DateForRate 02", ZDateTime.Today, invoiceHeader.CurrencyConverter.DateForRate);

				declaration.JE_MessageType = "EXP";
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Invalid;
				AssertEquals("For EXP, DateForRate is from dbo.CusEntryInstruction/JobDeclaration 03", ZDateTime.Today.AddDays(-1), invoiceHeader.CurrencyConverter.DateForRate);
				invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Today.AddDays(-3);
				AssertEquals("For EXP, DateForRate is from dbo.CusEntryInstruction/JobDeclaration 04", ZDateTime.Today.AddDays(-3), invoiceHeader.CurrencyConverter.DateForRate);
			});
		}
	}
}
