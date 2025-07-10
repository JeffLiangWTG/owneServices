using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class MandatoryValidationAbstractTest : TestCaseWithFactory
	{
		public void TestWarnIfAnExchangeRateExpandsOverMoreThanOneWorkingDay()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ExportDate = new ZDateTime(2006, 5, 1);
			AssertEquals("DateOfValuation", new ZDateTime(2006, 5, 1), declaration.DateOfValuation);

			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = "Z~Z";
			RefExchangeRate rate = currency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = new ZDateTime(2006, 4, 28);
			rate.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate.RE_SellRate = 0.541m;

			AssertEquals("PreCondition:StartDate is Friday", DayOfWeek.Friday, rate.RE_StartDate.DayOfWeek);
			AssertEquals("PreCondition:EndDate is Monday", DayOfWeek.Monday, rate.RE_ExpiryDate.DayOfWeek);

			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;

			AssertEquals("Exchange rate", 0.541m, invoice.JZ_InvoiceCurrExRate);
			AssertHasWarningContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "The exchange rate applied for this currency covers more than one working day");

			rate.RE_ExpiryDate = new ZDateTime(2006, 4, 30);
			RefExchangeRate rate2 = currency.ExchangeRates.AddNew();
			rate2.RE_ExRateType = "CUS";
			rate2.RE_StartDate = new ZDateTime(2006, 5, 1);
			rate2.RE_ExpiryDate = new ZDateTime(2006, 5, 1);
			rate2.RE_SellRate = 0.542m;

			invoice.RunPreSaveValidation();
			AssertNoWarningContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "The exchange rate applied for this currency covers more than one working day");
		}
	}
}
