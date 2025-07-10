using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHInvoice))]
sealed class NODocSADHInvoiceTest : DocBaseWrapperTest
{
	public void TestCurrencyCode()
	{
		AssertEquals("[PRE-CONDITION] CurrencyCode", ZString.Empty, CreateNewDocSADHInvoiceWrapper().CurrencyCode);
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.EuropeanUnion;
		AssertEquals("CurrencyCode", "EUR", CreateNewDocSADHInvoiceWrapper().CurrencyCode);
	}

	[TestDate(2025, 3, 1)]
	public void TestCurrencyExchangeRate()
	{
		var strategy = CurrencyConverterTestHelper.StrategyIfExchangeRateAlreadyExists.SplitTimePeriod;
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.Australia, ZDecimal.Zero, ZDateTime.Today.AddDays(-50), "CUS", strategy);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.Australia, 1.23m, ZDateTime.Today.AddDays(-10), "CUS", strategy);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.Australia, 1.45m, ZDateTime.Today, "CUS", strategy);
		var currency = CurrencyConverterTestHelper.GetCurrency(Factory, Constants.CurrencyCodes.Australia);
		AssertEquals("[PRE-CONDITION] Exchange rate at today", 1.45m, currency.GetCustomsRate(ZDateTime.Today));
		AssertEquals("[PRE-CONDITION] Exchange rate at 10 days ago", 1.23m, currency.GetCustomsRate(ZDateTime.Today.AddDays(-10)));
		AssertEquals("[PRE-CONDITION] Exchange rate at 50 days ago", ZDecimal.Zero, currency.GetCustomsRate(ZDateTime.Today.AddDays(-50)));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.Invoices.Add(InvoiceHeader);
		var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_LinePrice = 100m;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var wrapper = CreateNewDocSADHInvoiceWrapper();

		AssertEquals("[PRE-CONDITION] When no exchange rate or currency has been provided", "1,000", wrapper.CurrencyExchangeRate);
		CombineAssertions(() =>
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Australia;
			InvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			InvoiceHeader.JZ_InvoiceCurrExRate = 1.2m;
			AssertEquals("When exchange rate is manually overridden, it must be shown in the wrapper", "1,200", wrapper.CurrencyExchangeRate);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Sweden;
			InvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = true;
			InvoiceHeader.JZ_InvoiceCurrExRate = 1.2m;
			AssertEquals("When exchange rate is manually overridden, and the currency should be multiplied with 100, it must be shown in the wrapper", "120,000", wrapper.CurrencyExchangeRate);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Australia;
			InvoiceHeader.IsJZ_InvoiceCurrExRateUserEnterable = false;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-10);
			AssertEquals("When requested process date is set, use this date for exchange rate", "1,230", wrapper.CurrencyExchangeRate);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("When requested process date is NOT set, use current date for exchange rate", "1,450", wrapper.CurrencyExchangeRate);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(-50);
			AssertEquals("When requested process date is set, and we don't have exchange rate for date", "1,000", wrapper.CurrencyExchangeRate);
		});
	}

	public void TestIncoTerm()
	{
		AssertEquals("[PRE-CONDITION] IncoTerm", ZString.Empty, CreateNewDocSADHInvoiceWrapper().IncoTerm);
		InvoiceHeader.JZ_IncoTerm = Constants.IncoTerms.CostInsuranceAndFreight;
		AssertEquals("IncoTerm", "CIF", CreateNewDocSADHInvoiceWrapper().IncoTerm);
	}

	public void TestIncoTermPlace()
	{
		AssertEquals("[PRE-CONDITION] IncoTermPlace", ZString.Empty, CreateNewDocSADHInvoiceWrapper().IncoTermPlace);
		InvoiceHeader.JZ_IncoTermPlace = "place";
		AssertEquals("IncoTermPlace", "place", CreateNewDocSADHInvoiceWrapper().IncoTermPlace);
	}

	public void TestInvoiceAmount()
	{
		ZDecimal expectedInvoiceAmount = 999_350_246.46m;
		AssertEquals("[PRE-CONDITION] InvoiceAmount", ZDecimal.Zero.ToStringRounded(2), CreateNewDocSADHInvoiceWrapper().InvoiceAmount);
		InvoiceHeader.JZ_InvoiceAmount = expectedInvoiceAmount;
		AssertEquals("InvoiceAmount", expectedInvoiceAmount.ToStringRounded(2), CreateNewDocSADHInvoiceWrapper().InvoiceAmount);
	}

	public void TestInvoiceAmountNOK()
	{
		AssertEquals("[PRE-CONDITION] InvoiceAmountNOK", ZDecimal.Zero.ToStringRounded(2), CreateNewDocSADHInvoiceWrapper().InvoiceAmountNOK);
		InvoiceHeader.JZ_InvoiceAmount = 42;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.Norway;
		AssertEquals("[PRE-CONDITION] LocalCurrencyCode", "NOK", InvoiceHeader.LocalCurrencyCode);
		AssertEquals("[PRE-CONDITION] JZ_InvoiceAmountInLocalCurrency", 42.0m, InvoiceHeader.JZ_InvoiceAmountInLocalCurrency);
		AssertEquals("When InvoiceCurrency is 'NOK', InvoiceAmountNOK", "42,00", CreateNewDocSADHInvoiceWrapper().InvoiceAmountNOK);
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Constants.CurrencyCodes.EuropeanUnion, 0.08m);
		var declaration = Factory.New<JobDeclaration>();
		declaration.Invoices.Add(InvoiceHeader);
		declaration.JE_ExportDate = ZDateTime.Today;
		InvoiceHeader.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.EuropeanUnion;
		AssertEquals("[PRE-CONDITION] JZ_InvoiceAmountInLocalCurrency", 525.0m, InvoiceHeader.JZ_InvoiceAmountInLocalCurrency);
		AssertEquals("When InvoiceCurrency is 'EUR', InvoiceAmountNOK", "525,00", CreateNewDocSADHInvoiceWrapper().InvoiceAmountNOK);
	}

	public void TestInvoiceDate()
	{
		AssertEquals("[PRE-CONDITION] InvoiceDate", ZDateTime.Today.ToCustomsFormatString("yyyy.MM.dd"), CreateNewDocSADHInvoiceWrapper().InvoiceDate);
		InvoiceHeader.JZ_InvoiceDate = ZDateTime.BrettsBirthday;
		AssertEquals("InvoiceDate", "1971.09.18", CreateNewDocSADHInvoiceWrapper().InvoiceDate);
	}

	public void TestInvoiceNumber()
	{
		AssertEquals("[PRE-CONDITION] InvoiceNumber", ZString.Empty, CreateNewDocSADHInvoiceWrapper().InvoiceNumber);
		InvoiceHeader.JZ_InvoiceNumber = "6234965231270300";
		AssertEquals("InvoiceNumber", "6234965231270300", CreateNewDocSADHInvoiceWrapper().InvoiceNumber);
	}

	public void TestSupplierName()
	{
		AssertEquals("[PRE-CONDITION] SupplierName", ZString.Empty, CreateNewDocSADHInvoiceWrapper().SupplierName);
		InvoiceHeader.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
		InvoiceHeader.Supplier.OH_FullName = "Awesome stuff Inc.";
		AssertEquals("SupplierName", "Awesome stuff Inc.", CreateNewDocSADHInvoiceWrapper().SupplierName);
	}

	public void TestValuationCode()
	{
		AssertEquals("[PRE-CONDITION] ValuationCode", "01", CreateNewDocSADHInvoiceWrapper().ValuationCode);
		InvoiceHeader.JZ_ValuationCode = "11";
		AssertEquals("ValuationCode", "11", CreateNewDocSADHInvoiceWrapper().ValuationCode);
	}

	public void TestHasMultipleEntryHeaders()
	{
		AssertEquals("[PRE-CONDITION] NumberOfEntryHeaders", 0, CreateNewDocSADHInvoiceWrapper().NumberOfEntryHeaders);

		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.Invoices.Add(InvoiceHeader);
			var invoiceLine1 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			AssertEquals("With one entryInstruction -> false", 1, CreateNewDocSADHInvoiceWrapper().NumberOfEntryHeaders);

			var invoiceLine2 = InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			AssertEquals("With two entryInstructions -> true", 2, CreateNewDocSADHInvoiceWrapper().NumberOfEntryHeaders);
		});
	}

	public void TestConstructor()
	{
		var e = AssertExceptionThrown<ArgumentNullException>("invoiceHeader", () => NODocSADHInvoice.New(null, Factory));
		AssertContains("invoiceHeader", e.ParamName);
	}
	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Factory.New<JobComInvoiceHeader>();
	JobComInvoiceHeader invoiceHeader;

	NODocSADHInvoice CreateNewDocSADHInvoiceWrapper() => NODocSADHInvoice.New(InvoiceHeader, Factory);
	protected override DocBaseWrapper GetNewDocumentWrapper() => CreateNewDocSADHInvoiceWrapper();
}
