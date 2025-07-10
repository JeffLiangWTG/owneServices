using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	sealed class RefExchangeRateDataImportTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData()
		{
			FileInfo testFileInfo = new FileInfo(PathToTestFile);
			ExchangeRateDataImport.ImportData(testFileInfo.FullName);
			ZQuery exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_StartDate, startDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, endDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			BusinessObject[] exchangeRates = Factory.Load(typeof(RefExchangeRate), exchangeRateQuery);
			AssertEquals("Should have been 24 exchange rates loaded", 24, exchangeRates.Length);
			Assert(RateLoaded("AUD", 1.314m));
			Assert(RateLoaded("CAD", 1.502m));
			Assert(RateLoaded("CHF", 1.24162m));
			Assert(RateLoaded("CNY", 0.1969m));
			Assert(RateLoaded("DKK", 0.2797m));
			Assert(RateLoaded("EUR", 2.081m));
			Assert(RateLoaded("GBP", 2.986m));
			Assert(RateLoaded("HKD", 0.18906m));
			Assert(RateLoaded("IDR", 0.00016m));
			Assert(RateLoaded("INR", 0.03848m));
			Assert(RateLoaded("JPY", 0.01252m));
			Assert(RateLoaded("KRW", 0.00164m));
			Assert(RateLoaded("LKR", 0.01333m));
			Assert(RateLoaded("MYR", 0.43395m));
			Assert(RateLoaded("NOK", 0.27188m));
			Assert(RateLoaded("NZD", 1.115m));
			Assert(RateLoaded("PHP", 0.03478m));
			Assert(RateLoaded("PKR", 0.02458m));
			Assert(RateLoaded("SAR", 0.40007m));
			Assert(RateLoaded("SEK", 0.22829m));
			Assert(RateLoaded("THB", 0.04656m));
			Assert(RateLoaded("TWD", 0.04609m));
			Assert(RateLoaded("USD", 1.466m));
			Assert(RateLoaded("ZAR", 0.21695m));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRatesImportOnlyOnce()
		{
			FileInfo testFileInfo = new FileInfo(PathToTestFile);
			ExchangeRateDataImport.ImportData(testFileInfo.FullName);
			ZQuery exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_StartDate, startDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, endDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			BusinessObject[] exchangeRates = Factory.Load(typeof(RefExchangeRate), exchangeRateQuery);
			AssertEquals("Should have been 24 exchange rates loaded", 24, exchangeRates.Length);
			ExchangeRateDataImport.ImportData(testFileInfo.FullName);
			exchangeRates = Factory.Load(typeof(RefExchangeRate), exchangeRateQuery);
			AssertEquals("Should still only be 24 exchange rates loaded for this date range", 24, exchangeRates.Length);
		}

		bool RateLoaded(string currencyCode, decimal exchangeRate)
		{
			bool rateLoaded = false;
			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
			ZQuery exchangeRateQuery = new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_StartDate, startDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, endDate);
			exchangeRateQuery.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.CustomsRate);
			List<RefExchangeRate> exchangeRates = new List<RefExchangeRate>(currency.ExchangeRates.Find(exchangeRateQuery));
			if (exchangeRates.Count == 1)
			{
				RefExchangeRate thisRate = exchangeRates[0];
				rateLoaded = thisRate.RE_SellRate == exchangeRate;
			}

			return rateLoaded;
		}

		RefExchangeRateDataImport exchangeRateDataImport;
		RefExchangeRateDataImport ExchangeRateDataImport => exchangeRateDataImport ?? (exchangeRateDataImport = new RefExchangeRateDataImport());

		string PathToTestFile => BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business\Business\RefCurrency\ExchangeRate.xls";

		readonly DateTime startDate = new DateTime(2007, 10, 23);
		readonly DateTime endDate = new DateTime(2007, 10, 29, 23, 59, 0);
	}
}
