using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MoneyTest : TestCaseWithFactory
	{
		public void TestComparingMoniesWithNullCurrenciesThrowsNoExceptions()
		{
			Money nullBuggerMoney = new Money(15m, null);
			Money notherNullBuggerMoney = new Money(15m, null);
			Money realBuggerMoney = new Money(12m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.NewZealand));
			Money playBuggerMoney = new Money(12m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia));
			Money fakeBuggerMoney = new Money(realBuggerMoney);
			AssertEquals(nullBuggerMoney, notherNullBuggerMoney);
			AssertNotEquals(nullBuggerMoney, realBuggerMoney);
			AssertNotEquals(notherNullBuggerMoney, realBuggerMoney);
			AssertNotEquals(playBuggerMoney, realBuggerMoney);
			AssertEquals(fakeBuggerMoney, realBuggerMoney);
		}

		#region TestMoniesWithDifferentICurrenciesThatHaveTheSameInnerValuesAreSeenAsEqual

		public void TestMoniesWithDifferentICurrenciesThatHaveTheSameInnerValuesAreSeenAsEqual()
		{
			CurrencyOne currency1 = new CurrencyOne();
			CurrencyTwo currency2 = new CurrencyTwo();
			Money money1 = new Money(100m, currency1);
			Money money2 = new Money(100m, currency2);
			AssertEquals(money1, money2);
		}
		static readonly Guid StaticGuid = Guid.NewGuid();
		class CurrencyOne : ICurrency
		{
			public string Code
			{
				get { return "ZAZ"; }
			}

			public int Decimals
			{
				get { return 2; }
			}

			public Guid PK
			{
				get { return StaticGuid; }
			}
		}
		class CurrencyTwo : ICurrency
		{
			public string Code
			{
				get { return "ZAZ"; }
			}

			public int Decimals
			{
				get { return 2; }
			}

			public Guid PK
			{
				get { return StaticGuid; }
			}
		}
		#endregion

		public void TestInvalidMoneyChangesWhenCountryChanges()
		{
			ZString originalCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
			try
			{
				AssertEquals(GlbCompany.CurrentCompany.LocalCurrency.Code, Money.Invalid.Currency.Code);

				GlbCompany.CurrentCompany.SetCurrency(Core.Constants.CurrencyCodes.Angola);
				AssertEquals(Constants.CurrencyCodes.Angola, Money.Invalid.Currency.Code);

				GlbCompany.CurrentCompany.SetCurrency(Core.Constants.CurrencyCodes.Aruba);
				AssertEquals(Constants.CurrencyCodes.Aruba, Money.Invalid.Currency.Code);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCurrency(originalCurrency);
			}
		}

		public void TestEmptyMoneyAlwaysHasTheCurrencyOfTheCurrentCountry()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				AssertEquals("Money.Empty.Currency.Code", Core.Constants.CurrencyCodes.NewZealand, Money.Empty.Currency.Code);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				AssertEquals("Money.Empty.Currency.Code", Core.Constants.CurrencyCodes.Australia, Money.Empty.Currency.Code);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestEqualsOnZeroValuesWithDifferentCurrencies()
		{
			Money money1 = new Money(0m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia));
			Money money2 = new Money(0m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.NewZealand));
			AssertEquals("0.00AUD == 0.00NZD", money1, money2);
		}

		public void TestEqualsOnSameObject()
		{
			Money money = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Assert(money.Equals(money));
		}

		public void TestEqualsOnEquivalentObject()
		{
			Money money1 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Money money2 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Assert(money1.Equals(money2));
		}

		public void TestEqualsOnDifferentObject()
		{
			Money money1 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Money money2 = new Money(101m, GlbCompany.CurrentCompany.LocalCurrency);
			Assert(!money1.Equals(money2));
		}

		public void TestGetHashCodeOnEquivalentObjects()
		{
			Money money1 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Money money2 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			AssertEquals(money1.GetHashCode(), money2.GetHashCode());
		}

		public void TestGetHashCodeOnDifferentObjects()
		{
			Money money1 = new Money(100m, GlbCompany.CurrentCompany.LocalCurrency);
			Money money2 = new Money(101m, GlbCompany.CurrentCompany.LocalCurrency);
			Assert(money1.GetHashCode() != money2.GetHashCode());
		}

		public void TestMultiplierOverload()
		{
			Money originalMoney = new Money(100, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney * 10;
			AssertEquals(1000m, newMoney.Amount);
		}

		public void TestMultiplierOverloadDoesNotRound()
		{
			Money originalMoney = new Money(1.23456m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney * 10;
			AssertEquals(12.3456m, newMoney.Amount);
		}

		public void TestDivisorOverload()
		{
			Money originalMoney = new Money(1000, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney / 10;
			AssertEquals(100m, newMoney.Amount);
		}

		[ExpectException(typeof(DivideByZeroException))]
		public void TestDivisorOverloadThrowsExceptionOnDivideByZero()
		{
			Money originalMoney = new Money(1000, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney / 0;
		}

		public void TestDivisorOverloadDoesNotRound()
		{
			Money originalMoney = new Money(1.23456m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney / 10;
			AssertEquals(.123456m, newMoney.Amount);
		}

		public void TestRoundDown()
		{
			Money originalMoney = new Money(100.459m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney.RoundDown(2);
			AssertEquals(100.45m, newMoney.Amount);
		}

		public void TestRoundUp()
		{
			Money originalMoney = new Money(100.459m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney.RoundUp(2);
			AssertEquals(100.46m, newMoney.Amount);
		}

		public void TestRound()
		{
			Money originalMoney = new Money(100.459m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney.Round(2);
			AssertEquals("Round Up", 100.46m, newMoney.Amount);

			originalMoney = new Money(100.454m, Env.CurrentCompany.LocalCurrency);
			newMoney = originalMoney.Round(2);
			AssertEquals("Round Up", 100.45m, newMoney.Amount);

			originalMoney = new Money(100.449m, Env.CurrentCompany.LocalCurrency);
			newMoney = originalMoney.Round(2);
			AssertEquals("Round Up", 100.45m, newMoney.Amount);
		}

		public void TestRoundToSameDecimalCountHasNoEffect()
		{
			Money originalMoney = new Money(100.449m, Env.CurrentCompany.LocalCurrency);
			Money newMoney = originalMoney.Round(3);
			AssertEquals("Round to three decimals on a 3 decimal number should have no effect", 100.449m, newMoney.Amount);
		}

		public void TestBadMoneyMadeFromValueWithoutCurrency()
		{
			Money badMoney = new Money(1, null);
			Assert(!badMoney.IsValid);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestBadMoneyCannotBeMadeGoodViaCopyMoney()
		{
			Money badMoney = new Money(1, null);
			Assert(!badMoney.IsValid);
			Money copiedMoney = new Money(badMoney, true);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestBadMoneyCannotBeMadeGoodViaAmountCurrencyValid()
		{
			Money badMoney = new Money(1, null, true);
		}

		public void TestGetTrimmedDecimalAmountString()
		{
			Money testMoney = new Money(100.40m, Env.CurrentCompany.LocalCurrency);
			AssertEquals("TrimmedAmount", "100.40", testMoney.GetTrimmedDecimalAmountString(10, false));
			AssertEquals("TrimmedAmount", "100.40", testMoney.GetTrimmedDecimalAmountString(6, false));
			AssertEquals("TrimmedAmount", "100.4", testMoney.GetTrimmedDecimalAmountString(5, false));
			AssertEquals("TrimmedAmount", "100.4", testMoney.GetTrimmedDecimalAmountString(4, false));
			AssertEquals("TrimmedAmount", "100", testMoney.GetTrimmedDecimalAmountString(4, true));
		}

		public void TestTrimmedDecimalAmount2()
		{
			decimal amount1 = 16958876.00m;
			decimal amount2 = 244745.00m;
			decimal amount3 = 42397.00m;
			Money testMoney = new Money(amount1 + amount2 + amount3, Env.CurrentCompany.LocalCurrency);
			AssertEquals("TrimmedAmount", "17246018.0", testMoney.GetTrimmedDecimalAmountString(10, false));
		}

		public void TestTrimmedDecimalAmountWithNoCurrency()
		{
			Money testMoney = new Money(123.12m, null);
			AssertEquals("TrimmedAmount", "123.12", testMoney.GetTrimmedDecimalAmountString(10, false));
		}

		public void TestNewMoneyWithEmptyCurrency()
		{
			Money testMoneyWithAmount = new Money(100, null);
			Assert(!testMoneyWithAmount.IsValid);

			Money testMoneyWithoutAmount = new Money(0, null);
			Assert(!testMoneyWithoutAmount.IsValid);
		}

		public void TestFuzzyRoundDown()
		{
			Money testMoney = new Money(99.999999999999999m, Env.CurrentCompany.LocalCurrency);
			AssertEquals(99.99m, testMoney.RoundDown(2).Amount);
			AssertEquals(100m, testMoney.FuzzyRoundDown(2).Amount);
		}

		public void TestToStringWithoutCurrency()
		{
			AssertEquals("12.00", new Money(12m, null).ToString());
			AssertEquals("12.35", new Money(12.346m, null).ToString());
			AssertEquals("123456789.35", new Money(123456789.346m, null).ToString());
		}

		public void TestToString()
		{
			var currency1 = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Japan);
			var currency2 = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.China);
			var currency3 = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Oman);
			AssertEquals("Pre-condition", 1, currency1.RX_SubUnitRatio);
			AssertEquals("Pre-condition", 100, currency2.RX_SubUnitRatio);
			AssertEquals("Pre-condition", 1000, currency3.RX_SubUnitRatio);

			var originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				var money1 = new Money(12345.6789m, currency1, "");
				var money2 = new Money(12345.6789m, currency2, "");
				var money3 = new Money(12345.6789m, currency3, "");

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Germany);
				AssertMultilineASCIIEquals("12.346 JPY", money1.ToString());
				AssertMultilineASCIIEquals("12.345,68 CNY", money2.ToString());
				AssertMultilineASCIIEquals("12.345,679 OMR", money3.ToString());

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.SouthAfrica);
				AssertMultilineASCIIEquals(money1.ToString(), money1.ToString());
				AssertMultilineASCIIEquals(money2.ToString(), "12 345,68 CNY", money2.ToString());
				AssertMultilineASCIIEquals(money3.ToString(), "12 345,679 OMR", money3.ToString());

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				AssertMultilineASCIIEquals("12,346 JPY", money1.ToString());
				AssertMultilineASCIIEquals("12,345.68 CNY", money2.ToString());
				AssertMultilineASCIIEquals("12,345.679 OMR", money3.ToString());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestRightToLeft()
		{
			var usd = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			var money = new Money(0.99m, usd);
			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Arabic))
			{
				AssertEquals(money.ToString(), "USD 0.99");
			}
		}

		public void TestMoneySource()
		{
			var money = new Money(100m, AUDCurrency, "FRT");

			AssertEquals("FRT", money.Source);

			var clonedMoney = new Money(money);
			AssertEquals(money.Amount, clonedMoney.Amount);
			AssertEquals(money.Source, clonedMoney.Source);
			AssertEquals(money.Currency, clonedMoney.Currency);

			money = new Money(20m, AUDCurrency);

			AssertEquals(string.Empty, money.Source);

			money = new Money(20m, USDCurrency, "BAF");

			AssertEquals("BAF", money.Source);

			clonedMoney = new Money(money);
			AssertEquals(money.Amount, clonedMoney.Amount);
			AssertEquals(money.Source, clonedMoney.Source);
			AssertEquals(money.Currency, clonedMoney.Currency);
		}

		RefCurrency AUDCurrency
		{
			get { return audCurrency ?? (audCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Australia)); }
		}

		RefCurrency audCurrency;

		RefCurrency USDCurrency
		{
			get { return usdCurrency ?? (usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.UnitedStates)); }
		}

		RefCurrency usdCurrency;
	}
}
