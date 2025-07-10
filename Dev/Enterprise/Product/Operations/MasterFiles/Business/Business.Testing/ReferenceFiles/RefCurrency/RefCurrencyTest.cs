using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCurrency))]
	sealed class RefCurrencyTest : EnterpriseBusinessObjectTestCase
	{
		#region TestRX_IsExcludedCFXCalculation

		public void TestRX_IsExcludedCFXCalculation()
		{
			Currency.RX_Code = "ABC";
			AssertEquals("Default", false, Currency.RX_IsExcludedCFXCalculation);

			Currency.RX_IsExcludedCFXCalculation = true;
			AssertEquals("Value is set to true", true, Currency.RX_IsExcludedCFXCalculation);
			AssertEquals("Registry not set yet", false, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode(Currency.RX_Code));

			Factory.Save();

			AssertEquals("Value is set to true", true, Currency.RX_IsExcludedCFXCalculation);
			AssertEquals("Registry is set on save", true, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode(Currency.RX_Code));

			Currency.RX_Code = "XYZ";
			Currency.RX_IsExcludedCFXCalculation = false;
			AssertEquals("Value is set to false", false, Currency.RX_IsExcludedCFXCalculation);
			AssertEquals("Registry not set yet", true, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("ABC"));
			AssertEquals("Registry not set yet", false, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("XYZ"));

			Factory.Save();

			AssertEquals("Value is set to false", false, Currency.RX_IsExcludedCFXCalculation);
			AssertEquals("Registry is set on save", false, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("XYZ"));
			AssertEquals("Registry not set yet", false, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("ABC"));
		}

		public void TestRX_IsExcludedCFXCalculationWithLogs()
		{
			RefCurrency currency = null;
			Func<IEnumerable<string>> loadLogs = () =>
			{
				return currency.Logs.GetAllLogs().Cast<StmALog>().Select(log => log.SL_SE_NKEvent + ": " + log.SL_Reference);
			};

			var expectedLogs = new List<string>();

			//1. create new currency, it should have the log
			currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "A09";
			var sellRate = currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 0.1m;

			currency.RX_IsExcludedCFXCalculation = true;
			AssertContainsExactElementsInAnyOrder("logs before saving", expectedLogs, loadLogs());

			Factory.Save();
			expectedLogs.Add("ADD: Exclude from CFX Calculations Flag set to 'Yes', Company: EDI");
			AssertContainsExactElementsInAnyOrder("logs after ticking and saving a new currency", expectedLogs, loadLogs());

			//2. modify RX_IsExcludedCFXCalculation on saved currency, it should have a new log
			currency.RX_IsExcludedCFXCalculation = false;
			Factory.Save();
			expectedLogs.Add("EDT: Exclude from CFX Calculations Flag set to 'No', Company: EDI");
			AssertContainsExactElementsInAnyOrder("logs after unticking 'exclude cfx' and saving a currency for the second time", expectedLogs, loadLogs());

			//3. edit rates on saved currency, it should not have any new logs
			currency.ExchangeRates[0].RE_SellRate = 1.92m;
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("logs after modifying exchange rates and saving", expectedLogs, loadLogs());

			//4. edit desc. only, it shoud not have the log
			currency.RX_Desc = "RX_Desc1";
			Factory.Save();
			expectedLogs.Add("EDT: ");
			AssertContainsExactElementsInAnyOrder("logs after modifying description and saving", expectedLogs, loadLogs());

			//5. edit desc. and RX_IsExcludedCFXCalculation, it shoud have the log
			currency.RX_Desc = "RX_Desc2";
			currency.RX_IsExcludedCFXCalculation = !currency.RX_IsExcludedCFXCalculation;
			Factory.Save();
			expectedLogs.Add("EDT: Exclude from CFX Calculations Flag set to 'Yes', Company: EDI");
			AssertContainsExactElementsInAnyOrder("logs afer changing description and exclude flag and saving currency", expectedLogs, loadLogs());
		}

		public void TestModifyingOneCurrencyDoesNotModifyOther()
		{
			var currency1 = Factory.NewWithValidTestData<RefCurrency>();
			currency1.RX_Code = "AAA";
			currency1.RX_IsExcludedCFXCalculation = true;

			var currency2 = Factory.NewWithValidTestData<RefCurrency>();
			currency2.RX_Code = "BBB";
			currency2.RX_IsExcludedCFXCalculation = false;
			Factory.Save();

			AssertEquals("Initial value set to true", true, currency1.RX_IsExcludedCFXCalculation);
			AssertEquals("Initial value set to false", false, currency2.RX_IsExcludedCFXCalculation);

			var cmd = TestConnection.Command("Delete From dbo.StmData Where SD_Name = 'ExcludeCurrencyFromCFXCalculation'");
			cmd.ExecuteNonQuery(); // this will delete all excluded currency code from the registry (e.g. some other user set currency1.RX_IsExcludedCFXCalculation = false;)

			currency2.RX_IsExcludedCFXCalculation = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var currency1InNewFactory = newFactory.Load<RefCurrency>(currency1.PK);
			var currency2InNewFactory = newFactory.Load<RefCurrency>(currency2.PK);
			AssertEquals("Should set to false", false, currency1InNewFactory.RX_IsExcludedCFXCalculation);
			AssertEquals("Should set to true", true, currency2InNewFactory.RX_IsExcludedCFXCalculation);
		}

		public void TestCurrencyInitialisedInOneCompanyDoesNotAffectValuesStoredInCurrentCompany()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newCompanybranch = newCompany.Branches.AddNew();

			var currency1 = Factory.NewWithValidTestData<RefCurrency>();
			currency1.RX_Code = "AAA";
			currency1.RX_IsExcludedCFXCalculation = true;
			Factory.Save();

			AssertEquals("RX_IsExcludedCFXCalculation when logged into the original company", true, currency1.RX_IsExcludedCFXCalculation);

			var newFactory = new BusinessObjectFactory();
			RefCurrency currency1Reloaded = null;

			// this simulates when you import sister company invoices.
			// the currency object is constructed while you're logged into another company.
			// using the old code, this would populate the value of RX_IsExcludedCFXCalculation based on the other company's registry settings.
			// 
			using (newCompanybranch.SetAsTemporaryContext())
			{
				currency1Reloaded = newFactory.Load<RefCurrency>(currency1.PK);
				AssertEquals("RX_IsExcludedCFXCalculation when logged into a different company", false, currency1Reloaded.RX_IsExcludedCFXCalculation);
			}

			AssertEquals("RX_IsExcludedCFXCalculation when logged into the original company", true, currency1Reloaded.RX_IsExcludedCFXCalculation);
			var logsBeforeSave = currency1Reloaded.Logs.GetAllLogs().Count;
			newFactory.Save();
			var logsAfterSave = currency1Reloaded.Logs.GetAllLogs().Count;
			AssertEquals("should be no new logs because nothing has been changed", logsBeforeSave, logsAfterSave);
		}

		#endregion

		#region Decimals

		public void TestDecimals()
		{
			RefCurrency currency = (RefCurrency)GetNewBusinessObject();
			currency.RX_SubUnitRatio = 0;
			AssertEquals(0, currency.Decimals);
			currency.RX_SubUnitRatio = 100;
			AssertEquals(2, currency.Decimals);
		}

		#endregion

		#region Decimals

		public void TestISODecimals()
		{
			RefCurrency currency = (RefCurrency)GetNewBusinessObject();
			currency.RX_ISOSubUnitRatio = 0;
			AssertEquals(0, currency.ISODecimals);
			currency.RX_ISOSubUnitRatio = 100;
			AssertEquals(2, currency.ISODecimals);
		}

		#endregion

		#region Loading

		public void TestLoadFromForeignCode()
		{
			OrgHeader orgWithOverrides = Factory.NewWithValidTestData<OrgHeader>();

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency kRWCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");

			OrgPatternMatchOverride matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = aUDCurrency.PK;
			matchOverride.OO_ForeignCode = "AD";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = kRWCurrency.PK;
			matchOverride.OO_ForeignCode = "KR";
			Factory.Save();

			AssertEquals("AUD", RefCurrency.LoadFromForeignCode(Factory, "AD", orgWithOverrides).RX_Code);
			AssertEquals("KRW", RefCurrency.LoadFromForeignCode(Factory, "KR", orgWithOverrides).RX_Code);
			AssertNull(RefCurrency.LoadFromForeignCode(Factory, "AUD", orgWithOverrides));
			AssertNull(RefCurrency.LoadFromForeignCode(Factory, "XXX", orgWithOverrides));
		}

		public void TestLoadFromCountryCode()
		{
			AssertEquals("ZAR", RefCurrency.LoadFromCurrencyCode(new BusinessObjectFactory(), "ZAR").RX_Code);
			AssertNull(RefCurrency.LoadFromCurrencyCode(new BusinessObjectFactory(), "XXX"));
		}

		public void TestOnLoaded()
		{
			Currency.RX_IsSystem = false;
			Currency.OnLoaded();
			Assert("RX_Code should not be read only", !Currency.RX_CodeInfo.ReadOnly);
			Currency.RX_IsSystem = true;
			Currency.OnLoaded();
			Assert("RX_Code should be read only", Currency.RX_CodeInfo.ReadOnly);
		}

		#endregion

		#region Exchange Rates

		public void TestRateValidationSuspendedWhenCurrencyValidationSuspended()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CAD");
			using (currency.GetValidationSuspender())
			{
				var rate1 = currency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now.AddDays(-1), 0.79m);
				Assert("Rate validation has been suspended", !rate1.RE_ExpiryDateInfo.HasNotifications());
			}

			var rate2 = currency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now.AddDays(-1), 0.9m);
			Assert("Rate validation has not been suspended", rate2.RE_ExpiryDateInfo.HasNotifications());
		}

		public void TestExchangeRates()
		{
			RefExchangeRate e1 = Factory.New(typeof(RefExchangeRate)) as RefExchangeRate;
			RefExchangeRate e2 = Factory.New(typeof(RefExchangeRate)) as RefExchangeRate;
			RefExchangeRate e3 = Factory.New(typeof(RefExchangeRate)) as RefExchangeRate;
			RefExchangeRate e4 = Factory.New(typeof(RefExchangeRate)) as RefExchangeRate;
			RefCurrency currency2 = Factory.New(typeof(RefCurrency)) as RefCurrency;
			Currency.RX_Code = "AAA";
			currency2.RX_Code = "XXX";

			e1.RE_RX_NKExCurrency = Currency.RX_Code;
			e2.RE_RX_NKExCurrency = Currency.RX_Code;
			e3.RE_RX_NKExCurrency = Currency.RX_Code;
			e4.RE_RX_NKExCurrency = currency2.RX_Code;

			AssertEquals("Currency exchange rate collection count should be 3", 3, Currency.ExchangeRates.Count);

			Currency.ExchangeRates.RemoveFromRelationship(e1);
			AssertEquals("Currency exchange rate collection count should be 2", 2, Currency.ExchangeRates.Count);
		}

		public void TestCurrentExchangeRates()
		{
			Currency.RX_Code = "XYZ";
			RefExchangeRate sellRate = Currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 0.8m;

			RefExchangeRate sellRateOld = Currency.ExchangeRates.AddNew();
			sellRateOld.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRateOld.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRateOld.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
			sellRateOld.RE_SellRate = 0.7m;

			RefExchangeRate buyRate = Currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			buyRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			buyRate.RE_SellRate = 0.6m;

			RefExchangeRate customsRateOld = Currency.ExchangeRates.AddNew();
			customsRateOld.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			customsRateOld.RE_StartDate = ZDateTime.Today.AddDays(-10);
			customsRateOld.RE_ExpiryDate = ZDateTime.Today.AddDays(-9);
			customsRateOld.RE_SellRate = 0.5m;

			AssertEquals("Precondition: Currency exchange rate collection count should be 4", 4, Currency.ExchangeRates.Count);

			AssertEquals("Current Sell Rate returned", 0.8m, Currency.CurrentSellRate);
			AssertEquals("Current Buy Rate returned", 0.6m, Currency.CurrentBuyRate);
			AssertEquals("No current Customs rate so 0 returned", 0m, Currency.CurrentCustomsRate);

			RefExchangeRate customsRate = Currency.ExchangeRates.AddNew();
			customsRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			customsRate.RE_StartDate = ZDateTime.Today.AddDays(-10);
			customsRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			customsRate.RE_SellRate = 0.4m;

			AssertEquals("Currency exchange rate now has 1 more item", 5, Currency.ExchangeRates.Count);
			AssertEquals("Current Customs Rate returned", 0.4m, Currency.CurrentCustomsRate);

			customsRate.RE_SellRate = 0.33m;
			AssertEquals("Updated Customs Rate returned - current rates are not cached", 0.33m, Currency.CurrentCustomsRate);
		}

		public void ConvertUsingCustomsRate()
		{
			RefExchangeRate sellRate = Currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 0.8m;

			RefExchangeRate buyRate = Currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			buyRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			buyRate.RE_SellRate = 0.6m;

			RefExchangeRate customsRate = Currency.ExchangeRates.AddNew();
			customsRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			customsRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			customsRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			customsRate.RE_SellRate = 0.5m;

			RefExchangeRate customsRateOld = Currency.ExchangeRates.AddNew();
			customsRateOld.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			customsRateOld.RE_StartDate = ZDateTime.Today.AddDays(-5);
			customsRateOld.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
			customsRateOld.RE_SellRate = 0.7m;

			RefCurrency targetCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			AssertEquals("Expecting convert using the current customs rate",
				new Money(0.5m * 5m, targetCurrency),
				Currency.ConvertUsingSellRate(ZDateTime.Now, 5m, targetCurrency));
		}

		public void ConvertUsingSellRate()
		{
			RefExchangeRate sellRate = Currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 0.8m;

			RefExchangeRate sellRateOld = Currency.ExchangeRates.AddNew();
			sellRateOld.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRateOld.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRateOld.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
			sellRateOld.RE_SellRate = 0.7m;

			RefExchangeRate buyRate = Currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			buyRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			buyRate.RE_SellRate = 0.6m;

			RefExchangeRate customsRate = Currency.ExchangeRates.AddNew();
			customsRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			customsRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			customsRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			customsRate.RE_SellRate = 0.5m;

			RefCurrency targetCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			AssertEquals("Expecting convert using the current sell rate",
				new Money(0.8m * 5m, targetCurrency),
				Currency.ConvertUsingSellRate(ZDateTime.Now, 5m, targetCurrency));
		}

		public void TestSetCustomsRate()
		{
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			ZDateTime dateStart = new ZDateTime(2004, 9, 18);
			ZDateTime dateEnd = new ZDateTime(2004, 9, 22);
			currency.SetCustomsRate(dateStart, dateEnd, 0.79m);
			AssertEquals(0.79m, currency.GetCustomsRate(dateStart));
		}

		public void TestUniqueIndexOfExchangeRate()
		{
			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			ZDateTime dateStart = new ZDateTime(2004, 9, 18);
			ZDateTime dateEnd1 = new ZDateTime(2004, 9, 22);
			ZDateTime dateEnd2 = new ZDateTime(2004, 10, 22);
			currency.SetCustomsRate(dateStart, dateEnd1, 0.79m);
			currency.SetCustomsRate(dateStart, dateEnd2, 0.89m);
			AssertEquals(0.89m, currency.GetCustomsRate(dateStart));
		}

		public void TestCurrentExchangeRateByCurrentCompany()
		{
			RefExchangeRate sellRate = Currency.ExchangeRates.AddNew();
			sellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			sellRate.RE_SellRate = 0.1m;

			RefExchangeRate buyRate = Currency.ExchangeRates.AddNew();
			buyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			buyRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			buyRate.RE_SellRate = 0.2m;
			buyRate.RE_GC = ZGuid.Empty;

			AssertEquals("Current SellRate should be 0.1m", 0.1m, Currency.CurrentSellRate);
			AssertEquals("Current BuyRate should  be Zero", 0m, Currency.CurrentBuyRate);
		}

		#endregion

		#region Logging

		public void TestLogging()
		{
			AssertEquals("No Logs", 0, Currency.Logs.DatabaseCount);
			Currency.RX_Code = "XDB";
			Currency.RX_Desc = "DESC";
			Factory.Save();
			AssertEquals("Log (ADD)", 1, Currency.Logs.DatabaseCount);
		}

		#endregion

		#region Delete

		public void TestCannotDeleteSystemCurrency()
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			Assert("Australian dollar was not system defined.", currency.RX_IsSystem);
			Assert("Australian (system) dollar was deletable!", !currency.CanDelete);
			currency.RX_IsSystem = false;
			Assert("Australian dollar should not be a system currency after manual override", !currency.RX_IsSystem);
			Assert("Australian dollar should now be deletable but is isn't", currency.CanDelete);
		}

		public void TestDeleteRemovesCodeFromRegistry()
		{
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ABC";
			currency.RX_IsSystem = false;
			currency.RX_IsExcludedCFXCalculation = true;
			Factory.Save();

			AssertEquals("Registry is set on save", true, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("ABC"));
			currency.Delete();

			AssertEquals("ABC code is removed on delete", false, AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).ContainsCode("ABC"));
		}

		[ExpectException(typeof(ZCannotSaveException))]
		public void TestDelete_WithMatchingCompany_ThrowsZCannotSaveException()
		{
			Assert(GlbCompany.CurrentCompany != null);
			var company = GlbCompany.CurrentCompany;
			var currencycode = company.GC_RX_NKLocalCurrency;
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = currencycode;
			currency.Delete();
		}
		#endregion

		#region Translatable

		public void TestRX_Desc_Translatable()
		{
			var bizO = Factory.NewWithValidTestData<RefCurrency>();
			bizO.RX_Desc = "Australian Dollar";

			var translateEnglish = Factory.New<RefLanguageText>();
			translateEnglish.RLT_ColumnName = "RX_Desc";
			translateEnglish.RLT_Language = Core.SharedConstants.Languages.English;
			translateEnglish.RLT_ParentId = bizO.PK;
			translateEnglish.RLT_ParentTableCode = "RX";
			translateEnglish.RLT_Text = "Australian Dollar";

			var translateChinese = Factory.New<RefLanguageText>();
			translateChinese.RLT_ColumnName = "RX_Desc";
			translateChinese.RLT_Language = Core.SharedConstants.Languages.Afrikaans;
			translateChinese.RLT_ParentId = bizO.PK;
			translateChinese.RLT_ParentTableCode = "RX";
			translateChinese.RLT_Text = "Australiaanse Dollar";

			Factory.Save();

			AssertEquals("Australian Dollar", bizO.RX_DescMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Afrikaans))
			{
				AssertEquals("Australiaanse Dollar", bizO.RX_DescMultilingual);
			}
		}

		public void TestRX_UnitName_Translatable()
		{
			var bizO = Factory.NewWithValidTestData<RefCurrency>();
			bizO.RX_UnitName = "Australian Dollar";

			var translateEnglish = Factory.New<RefLanguageText>();
			translateEnglish.RLT_ColumnName = "RX_UnitName";
			translateEnglish.RLT_Language = Core.SharedConstants.Languages.English;
			translateEnglish.RLT_ParentId = bizO.PK;
			translateEnglish.RLT_ParentTableCode = "RX";
			translateEnglish.RLT_Text = "Australian Dollar";

			var translateChinese = Factory.New<RefLanguageText>();
			translateChinese.RLT_ColumnName = "RX_UnitName";
			translateChinese.RLT_Language = Core.SharedConstants.Languages.Afrikaans;
			translateChinese.RLT_ParentId = bizO.PK;
			translateChinese.RLT_ParentTableCode = "RX";
			translateChinese.RLT_Text = "Australiaanse Dollar";

			Factory.Save();

			AssertEquals("Australian Dollar", bizO.RX_UnitNameMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Afrikaans))
			{
				AssertEquals("Australiaanse Dollar", bizO.RX_UnitNameMultilingual);
			}
		}

		public void TestRX_SubUnitName_Translatable()
		{
			var bizO = Factory.NewWithValidTestData<RefCurrency>();
			bizO.RX_SubUnitName = "Australian Dollar";

			var translateEnglish = Factory.New<RefLanguageText>();
			translateEnglish.RLT_ColumnName = "RX_SubUnitName";
			translateEnglish.RLT_Language = Core.SharedConstants.Languages.English;
			translateEnglish.RLT_ParentId = bizO.PK;
			translateEnglish.RLT_ParentTableCode = "RX";
			translateEnglish.RLT_Text = "Australian Dollar";

			var translateChinese = Factory.New<RefLanguageText>();
			translateChinese.RLT_ColumnName = "RX_SubUnitName";
			translateChinese.RLT_Language = Core.SharedConstants.Languages.Afrikaans;
			translateChinese.RLT_ParentId = bizO.PK;
			translateChinese.RLT_ParentTableCode = "RX";
			translateChinese.RLT_Text = "Australiaanse Dollar";

			Factory.Save();

			AssertEquals("Australian Dollar", bizO.RX_SubUnitNameMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Afrikaans))
			{
				AssertEquals("Australiaanse Dollar", bizO.RX_SubUnitNameMultilingual);
			}
		}

		#endregion

		#region Implementation

		RefCurrency Currency;

		protected override void SetUp()
		{
			base.SetUp();
			Currency = Factory.New<RefCurrency>();
		}

		#endregion
	}
}
