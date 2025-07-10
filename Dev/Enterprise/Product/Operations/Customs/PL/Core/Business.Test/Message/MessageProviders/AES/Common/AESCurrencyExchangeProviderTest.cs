using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESCurrencyExchangeProviderTest : DataProviderTestCase<AESCurrencyExchangeProvider>
{
	public void TestInternalCurrencyUnit() => AssertEquals("Should be null", null, Provider.InternalCurrencyUnit);

	public void TestExchangeRate() => CombineAssertions(() =>
	{
		var invoiceHeader = jobDeclaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
		invoiceHeader.JZ_InvoiceCurrExRate = 0m;
		AssertNull("Exchange rate should be null if JZ_InvoiceCurrExRate equals 0.", GetProvider().ExchangeRate);

		invoiceHeader.JZ_InvoiceCurrExRate = 1m;
		AssertEquals("Exchange rate should be JZ_InvoiceCurrExRate if JZ_InvoiceCurrExRate is not 0.", 1m, GetProvider().ExchangeRate);

		entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.A;
		AssertNull("Exchange rate should be null if SubStyle is A.", GetProvider().ExchangeRate);

		entryInstruction.CEI_SubStyle = Constants.SubStyleCodes.D;
		AssertNull("Exchange rate should be null if SubStyle is D.", GetProvider().ExchangeRate);
	});

	protected override AESCurrencyExchangeProvider GetProvider() => new AESCurrencyExchangeProvider(entryInstruction);

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
	}

	JobDeclaration jobDeclaration;
	CusEntryInstruction entryInstruction;
}
