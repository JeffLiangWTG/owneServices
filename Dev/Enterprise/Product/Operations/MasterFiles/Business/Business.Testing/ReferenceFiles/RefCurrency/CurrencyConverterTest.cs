using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CurrencyConverterTest : CurrencyConverterTestCase
	{
		public void TestAddWithEmptyMoneyIsValid()
		{
			RefCurrency foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, LocalCurrency.RX_Code));
			Money moneyInForeignCurrency = new Money(100, foreignCurrency);

			ZDateTime dateOfValuation = new ZDateTime(2005, 3, 15);
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", dateOfValuation, dateOfValuation.AddDays(1), 0.5m, foreignCurrency);
			CurrencyConverter converter = CurrencyConverter.New(Factory, dateOfValuation, ZArchitecture.Core.ExchangeRateType.Customs, 1);

			Money added = converter.Add(moneyInForeignCurrency, Money.Empty);
			AssertEquals("Added money is valid", true, added.IsValid);

			added = converter.Add(Money.Empty, moneyInForeignCurrency);
			AssertEquals("Added money is valid", true, added.IsValid);
		}

		[ExpectNoExceptions]
		public void TestConvertAcceptsNullDestinationCurrency()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money foreignAmount1 = new Money(1000, DecoratedCurrency);
			currencyConverter.ConvertExact(foreignAmount1, (RefCurrency)null);
		}

		public void TestAddWhenForeignCurrencyInvolved()
		{
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.9862m, usd);
			var converter = CurrencyConverter.New(Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, 1);

			var amountInUSD = new Money(187.04m, usd);
			var amountInLocalCurrency = converter.ConvertExact(amountInUSD, GlbCompany.CurrentCompany.LocalCurrency);

			var amountToAdd1 = new Money(1.78m, usd);
			var amountToAdd2 = new Money(0.07m, GlbCompany.CurrentCompany.LocalCurrency);
			var amountToAdd3 = new Money(0.10m, GlbCompany.CurrentCompany.LocalCurrency);

			var total1 = converter.Add(amountInUSD, amountToAdd1);
			total1 = converter.Add(total1, amountToAdd2);
			total1 = converter.Add(total1, amountToAdd3);

			var total2 = converter.Add(amountToAdd1, amountToAdd2);
			total2 = converter.Add(total2, amountToAdd3);
			total2 = converter.Add(amountInLocalCurrency, total2);

			var convertedTotal1 = converter.ConvertExact(total1, GlbCompany.CurrentCompany.LocalCurrency);
			var convertedTotal2 = converter.ConvertExact(total2, GlbCompany.CurrentCompany.LocalCurrency);

			AssertEquals(convertedTotal1.Amount, convertedTotal2.Amount);
		}

		public void TestAddingNullToNull()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money nullMoney = currencyConverter.Add(null, null);
			Assert(nullMoney != null);
			AssertEquals(0m, nullMoney.Amount);
			AssertEquals(null, nullMoney.Currency);
			Assert(!nullMoney.IsValid);
		}

		public void TestAddingNullToMoney()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money originalMoney = new Money(100, DecoratedCurrency);
			Money result = currencyConverter.Add(originalMoney, null);
			Assert(result != null);
			AssertEquals(originalMoney.Amount, result.Amount);
			AssertEquals(originalMoney.Currency, result.Currency);
			AssertEquals("Added result is valid", true, result.IsValid);
		}

		public void TestAddingMoneyToNullValid()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money originalMoney = new Money(100, DecoratedCurrency);
			Money result = currencyConverter.Add(Money.Empty, originalMoney);
			Assert(result != null);
			AssertEquals(originalMoney.Currency.Code, result.Currency.Code);
			AssertEquals(originalMoney.Amount, result.Amount);
			AssertEquals("Added result is valid", true, result.IsValid);
		}

		public void TestAddMoneyWIthForeignCurrency()
		{
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefCurrency nZD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");

			Money uSDAmount = new Money(1000, uSD);
			Money nZDAmount = new Money(1000, nZD);

			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money plainAddingResult = currencyConverter.Add(uSDAmount, nZDAmount);
			AssertEquals("Adding two amounts in foreign currecies result in local currency", LocalCurrency.RX_Code, plainAddingResult.Currency.Code);

			Money preferredAddingResult = currencyConverter.Add(uSDAmount, uSDAmount);
			AssertEquals("Result in USD", "USD", preferredAddingResult.Currency.Code);

			preferredAddingResult = currencyConverter.Add(Money.Empty, nZDAmount);
			AssertEquals("Result in NZD", "NZD", preferredAddingResult.Currency.Code);
		}

		public void TestAddingLocalToLocal()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money localMoney1 = new Money(100, LocalCurrency);
			Money localMoney2 = new Money(200, LocalCurrency);
			Money result = currencyConverter.Add(localMoney1, localMoney2);
			Assert(result != null);
			AssertEquals(localMoney1.Amount + localMoney2.Amount, result.Amount);
			AssertEquals(localMoney1.Currency, result.Currency);
		}

		public void TestAddingForeignToForeign()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money foreignMoney1 = new Money(100, DecoratedCurrency);
			Money foreignMoney2 = new Money(200, DecoratedCurrency);
			Money result = currencyConverter.Add(foreignMoney1, foreignMoney2);
			Assert(result != null);
			AssertEquals(foreignMoney1.Amount + foreignMoney2.Amount, result.Amount);
			AssertEquals(foreignMoney2.Currency, result.Currency);
		}

		public void TestAddingTwoForeignCurrenciesDoesNotRoundAfterOneConversion()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			//GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", new ZDateTime(2003, 11, 14), new ZDateTime(2003, 11, 14), 0.9862m, usd);

			var nzd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			SetExchangeRate(GlbCompany.CurrentCompany, "CUS", new ZDateTime(2003, 11, 14), new ZDateTime(2003, 11, 14), 1.10m, nzd);

			var foreignAmount1 = new Money(100.5m, usd);
			var foreignAmount2 = new Money(10.5m, nzd);

			var result = currencyConverter.Add(foreignAmount1, foreignAmount2);
			AssertEquals(111.46m, result.Amount);
		}

		public void TestAddingForeignToLocal()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money localMoney1 = new Money(100, LocalCurrency);
			Money foreignMoney2 = new Money(200, DecoratedCurrency);
			Money result = currencyConverter.Add(localMoney1, foreignMoney2);
			Assert("Money object should be created", result != null);
			AssertEquals("Result money object currency code", localMoney1.Currency.Code, result.Currency.Code);
		}

		public void TestAddingWithMissingInitialCurrency()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money initial = new Money(100, null);
			Money amountToAdd = new Money(200, DecoratedCurrency);
			Money result = currencyConverter.Add(initial, amountToAdd);
			Assert(result != null);
			AssertEquals(amountToAdd.Amount, result.Amount);
			AssertEquals(amountToAdd.Currency, result.Currency);
		}

		public void TestAddWithoutRounding()
		{
			var currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, false);
			var localMoney = new Money(100.0001, LocalCurrency);
			var alienMoney = new Money(200.0004, DecoratedCurrency);
			var result = currencyConverter.Add(localMoney, alienMoney);

			Assert("Should not have rounded", 385.71m < result.Amount && result.Amount < 385.72m);
		}

		public void TestAddingWithMissingAdditionalCurrency()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money initial = new Money(100, DecoratedCurrency);
			Money amountToAdd = new Money(200, null);
			Money result = currencyConverter.Add(initial, amountToAdd);
			Assert(result != null);
			AssertEquals(initial.Amount, result.Amount);
			AssertEquals(initial.Currency, result.Currency);
		}

		public void TestSubtract()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money initial = new Money(100, DecoratedCurrency);
			Money amountToSubtract = new Money(50, DecoratedCurrency);
			Money result = currencyConverter.Subtract(initial, amountToSubtract);
			AssertEquals(50m, result.Amount);
		}

		public void TestMin()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money money1 = new Money(100, DecoratedCurrency);
			Money money2 = new Money(50, DecoratedCurrency);
			Money moneyMin = currencyConverter.Min(money1, money2);
			AssertEquals(50m, moneyMin.Amount);
		}

		public void TestMax()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 14), ExchangeRateType.Customs, 0);
			Money money1 = new Money(100, DecoratedCurrency);
			Money money2 = new Money(50, DecoratedCurrency);
			Money moneyMax = currencyConverter.Max(money1, money2);
			AssertEquals(100m, moneyMax.Amount);
		}

		public void TestFallback()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 16), ExchangeRateType.Customs, 0);
			AssertEquals("0 Conversion", 0m, currencyConverter.GetExchangeRate(DecoratedCurrency));
			currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 16), ExchangeRateType.Customs, 1);
			AssertEquals("0.7 Conversion", 0.7m, currencyConverter.GetExchangeRate(DecoratedCurrency));
		}

		public void TestAccumulatedInvoiceAmountWithNullCurrency()
		{
			CurrencyConverter currencyConverter = CurrencyConverter.New(Factory, new ZDateTime(2003, 11, 16), ExchangeRateType.Customs, 0);
			AssertEquals("ExchangeRate", 0m, currencyConverter.GetExchangeRate(null));
		}

		public void TestConversionWithDifferentGlbCompanies()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_RN_NKCountryCode = "UA";
			company1.GC_RX_NKLocalCurrency = company1.Country.RN_RX_NKLocalCurrency;
			SetExchangeRate(company1, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 0.2m, DecoratedCurrency);

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_RN_NKCountryCode = "RU";
			company2.GC_RX_NKLocalCurrency = company2.Country.RN_RX_NKLocalCurrency;
			company2.GC_IsReciprocal = true;
			SetExchangeRate(company2, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 25m, DecoratedCurrency);

			CurrencyConverter currencyConverter1 = CurrencyConverter.New(company1, Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			AssertEquals(0.2m, currencyConverter1.GetExchangeRate(DecoratedCurrency));
			AssertEquals(500m, currencyConverter1.ConvertExact(new Money(100m, DecoratedCurrency), company1.LocalCurrency).Amount);
			AssertEquals(0m, currencyConverter1.ConvertExact(new Money(100m, DecoratedCurrency), company2.LocalCurrency).Amount);

			CurrencyConverter currencyConverter2 = CurrencyConverter.New(company2, Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			AssertEquals(25m, currencyConverter2.GetExchangeRate(DecoratedCurrency));
			AssertEquals(2500m, currencyConverter2.ConvertExact(new Money(100m, DecoratedCurrency), company2.LocalCurrency).Amount);
			AssertEquals(0m, currencyConverter2.ConvertExact(new Money(100m, DecoratedCurrency), company1.LocalCurrency).Amount);
		}

		public void TestConvertExactWithoutRoundingToDestinationCurrencyDecimals()
		{
			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			RefCurrency nZD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 0.8573m, uSD);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 2m, nZD);
			Money uSDAmount = new Money(1, uSD);
			Money nZDAmount = new Money(2, nZD);
			Money localAmount = new Money(1, GlbCompany.CurrentCompany.LocalCurrency);

			CurrencyConverter converter = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			AssertEquals(0.8573m, converter.ConvertExact(nZDAmount, uSD, false).Amount);
			AssertEquals(0.86m, converter.ConvertExact(nZDAmount, uSD, true).Amount);

			var converterNoRounging = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			AssertEquals(0.86m, converterNoRounging.ConvertExact(nZDAmount, uSD, true).Amount);
			AssertEquals(0.8573m, converterNoRounging.ConvertExact(nZDAmount, uSD, false).Amount);
		}

		public void TestConverterDefaultValidity()
		{
			CurrencyConverter converter = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, 0);
			Assert("The default value of IsConverterValid should be true", converter.IsConverterValid);
		}
	}
}
