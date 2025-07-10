using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeaderMessageDataProvider))]
sealed class CusEntryHeaderMessageDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new CusEntryHeaderMessageDataProvider(null));

		var entryHeader = Factory.New<CusEntryHeader>();
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new CusEntryHeaderMessageDataProvider(entryHeader));

		var declaration = Factory.New<JobDeclaration>();
		entryHeader.CH_JE = declaration.PK;
		AssertNoExceptionThrown("When all required objects are present in entryHeader", () => new CusEntryHeaderMessageDataProvider(entryHeader));
	});

	public void TestGetTotalInvoiceAmount() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_InvoiceAmount = 100m;
		invoiceLine.JI_LinePrice = 20m;
		var provider1 = GetNewDataProvider();
		AssertEquals("CommercialInvoiceAmount, when single InvoiceHeader with single InvoiceLine", 100m, provider1.GetTotalInvoiceAmount());

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 30.00m;
		invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
		var provider2 = GetNewDataProvider();
		AssertEquals("CommercialInvoiceAmount, when single InvoiceHeader with multiple InvoiceLines", 50m, provider2.GetTotalInvoiceAmount());

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine3.JI_LinePrice = 150.50m;
		invoiceLine3.JI_CL = entryHeader.MergedLines.AddNew().PK;
		var provider3 = GetNewDataProvider();
		AssertEquals("CommercialInvoiceAmount, when multiple InvoiceHeader with same Currency", 200.5m, provider3.GetTotalInvoiceAmount());

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.08m);
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
		var provider4 = GetNewDataProvider();
		AssertEquals("CommercialInvoiceAmount, when multiple InvoiceHeader with different Currency", 1931.25m, provider4.GetTotalInvoiceAmount());
	});

	public void TestGetGoodsNumberPosition() => CombineAssertions(() =>
	{
		declaration.JE_Position = "123";
		var provider1 = GetNewDataProvider();
		AssertEquals("GoodsNumberPosition, when SubPosition is empty", "123", provider1.GetGoodsNumberPosition());

		entryInstruction.CEI_SubPosition = "42";
		var provider2 = GetNewDataProvider();
		AssertEquals("GoodsNumberPosition, when SubPosition is not empty", "123/42", provider2.GetGoodsNumberPosition());

		entryHeader.CH_CEI_Instruction = ZGuid.Empty;
		var provider3 = GetNewDataProvider();
		AssertEquals("GoodsNumberPosition, when EntryInstruction is empty", "123", provider3.GetGoodsNumberPosition());
	});

	public void TestHasMultipleCurrencies() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		AssertEquals("When single invoice header with NOK currency", expected: false, GetNewDataProvider().HasMultipleCurrencies);

		var invoice2 = declaration.Invoices.AddNew();
		invoice2.InvoiceLines.AddNew().JI_CL = entryLine.PK;
		invoice2.JZ_RX_NKInvoice_Currency = "NOK";
		entryLine.InvoiceLines.ReloadFromLocalCache();
		AssertEquals("When two invoices with same currency - NOK", expected: false, GetNewDataProvider().HasMultipleCurrencies);

		var invoice3 = declaration.Invoices.AddNew();
		invoice3.InvoiceLines.AddNew().JI_CL = entryLine.PK;
		invoice3.JZ_RX_NKInvoice_Currency = "USD";
		entryLine.InvoiceLines.ReloadFromLocalCache();
		AssertEquals("When three invoices with different currencies - NOK, USD", expected: true, GetNewDataProvider().HasMultipleCurrencies);
	});

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	CusEntryInstruction entryInstruction;

	CusEntryHeaderMessageDataProvider GetNewDataProvider() => new(entryHeader);
}
