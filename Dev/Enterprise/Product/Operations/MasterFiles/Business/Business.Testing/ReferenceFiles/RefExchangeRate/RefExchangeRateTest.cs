using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRate))]
	sealed class RefExchangeRateTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestOnLoaded()
		{
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = false;
			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			ExchangeRate.OnLoaded();
			Assert("Exchange rate fields should be Read Only", ExchangeRate.RE_ExpiryDateInfo.ReadOnly);

			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			ExchangeRate.OnLoaded();
			Assert("Exchange rate fields should not be Read Only", !ExchangeRate.RE_ExpiryDateInfo.ReadOnly);

			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			ExchangeRate.OnLoaded();
			Assert("Exchange rate fields should not be Read Only", !ExchangeRate.RE_ExpiryDateInfo.ReadOnly);

			Env.Security.GCBExchangeRateUpdate.IsAllowed = false;
			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl;
			ExchangeRate.OnLoaded();
			Assert("Exchange rate fields should not be Read Only", ExchangeRate.RE_ExpiryDateInfo.ReadOnly);

			Env.Security.GCBExchangeRateUpdate.IsAllowed = true;
			ExchangeRate.OnLoaded();
			Assert("Exchange rate fields should not be Read Only", !ExchangeRate.RE_ExpiryDateInfo.ReadOnly);
		}

		public void TestDeleteFailsWithSystemRecord()
		{
			CombineAssertions(() =>
			{
				RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
				exchangeRate.RE_ExRateType = "CUS";
				exchangeRate.RE_IsSystem = true;
				Assert("Cannot delete", !exchangeRate.CanDelete);
				AssertEquals("Cannot delete a System Defined Customs exchange rate.", exchangeRate.ReasonForNotAbleToDelete);

				exchangeRate.RE_IsSystem = false;
				Assert("Can delete", exchangeRate.CanDelete);
			});
		}

		public void TestDeleteSucceedsWithSufficientAccess()
		{
			RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
			exchangeRate.RE_ExRateType = "CUS";
			exchangeRate.ReadOnly = true;
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			exchangeRate.Delete();
			AssertEquals("Deleted", true, exchangeRate.IsDeleted);
		}

		public void TestSystemRecordIsReadOnlyAndCannotDelete()
		{
			ExchangeRate.RE_RX_NKExCurrency = "EUR";
			ExchangeRate.RE_ExRateType = "SEL";
			ExchangeRate.RE_StartDate = ZDateTime.BrettsBirthday;
			ExchangeRate.RE_ExpiryDate = ZDateTime.BrettsBirthday;
			ExchangeRate.RE_SellRate = 2m;
			ExchangeRate.RE_IsSystem = true;
			Env.Security.CustomsExchangeRateUpdate.IsAllowed = true;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Prereq: exchangeRate is system", true, ExchangeRate.RE_IsSystem);
				AssertEquals("Cannot modify a system record", true, ExchangeRate.ReadOnly);
				AssertEquals("Cannot delete a system record", false, ExchangeRate.CanDelete);
			});
		}

		public void TestSavingRefExchangeRateWithInvalidDate()
		{
			ExchangeRate.RE_RX_NKExCurrency = "AUD";
			ExchangeRate.RE_ExRateType = "SEL";
			ExchangeRate.RE_StartDate = ZDateTime.Empty;
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2011, 11, 11);
			ExchangeRate.RE_SellRate = 2m;

			Factory.Save();
			AssertNull("Should not save to database when RE_StartDate is empty", new BusinessObjectFactory().Load<RefExchangeRate>(ExchangeRate.PK));
			AssertEquals("Saving_Invalide_DateTime", ErrorReporter.LastKeyReported);
			AssertStartsWith("Should report stack trace of saving", "Saving invalid DateTime on Exchange Rate, saving stack trace:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			ExchangeRate.RE_StartDate = new ZDateTime(2011, 11, 11);
			ExchangeRate.RE_ExpiryDate = ZDateTime.Empty;

			Factory.Save();
			AssertNull("Should not save to database when RE_ExpiryDate is empty", new BusinessObjectFactory().Load<RefExchangeRate>(ExchangeRate.PK));
			AssertEquals("Saving_Invalide_DateTime", ErrorReporter.LastKeyReported);
			AssertStartsWith("Should report stack trace of saving", "Saving invalid DateTime on Exchange Rate, saving stack trace:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			ExchangeRate.RE_StartDate = new ZDateTime(2011, 11, 11);
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2011, 11, 12);
			Factory.Save();
			ExchangeRate.RE_StartDate = ZDateTime.Empty;
			ExchangeRate.IsNull = true;

			Assert("Current BizO will not be saved", !ExchangeRate.IsSavedByFactory);
			Factory.Save();
			AssertNullOrEmpty("No error report will be generated", ErrorReporter.LastMessageReported);
		}

		public void TestLoadWithCacheAddObjectFilterForRefExchangeRate()
		{
			var countryCode = "r1";
			var pk = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefExchangeRateZZ (ZZN_PK, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_RX_NKExCurrency, ZZN_RN_NKCountry, ZZN_AsPublished)
VALUES ('{pk}', 'CUS', '2017-01-11 00:00:00', '2079-06-06 23:59:00', 10, 'USD', '{countryCode}', '')");

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_RN_NKCountryCode = countryCode;
			Factory.Save();

			var filter = new ZQuery(RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, company.PK);
			var reader = new FilteredBusinessObjectReader<RefExchangeRate>(filter);
			AssertEquals("Should load filtered rows and no ConstraintException thrown", 1, reader.Count());
		}

		public void TestIsIndiaCustomsRate()
		{
			CombineAssertions(() =>
			{
				string GetMessage() => $"Country={ExchangeRate.Company.Country.Code}, RateType={ExchangeRate.RE_ExRateType}";

				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				AssertEquals(GetMessage(), false, ExchangeRate.IsIndiaCustomsRate);
				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				AssertEquals(GetMessage(), false, ExchangeRate.IsIndiaCustomsRate);
				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				AssertEquals(GetMessage(), false, ExchangeRate.IsIndiaCustomsRate);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					AssertEquals(GetMessage(), false, ExchangeRate.IsIndiaCustomsRate);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					AssertEquals(GetMessage(), true, ExchangeRate.IsIndiaCustomsRate);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					AssertEquals(GetMessage(), true, ExchangeRate.IsIndiaCustomsRate);
				}
			});
		}

		public void TestAsPublishedIsApplicable()
		{
			CombineAssertions(() =>
			{
				string GetMessage() => $"Country={ExchangeRate.Company.Country.Code}, RateType={ExchangeRate.RE_ExRateType}";

				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				AssertEquals(GetMessage(), false, ExchangeRate.AsPublishedIsApplicable);
				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				AssertEquals(GetMessage(), true, ExchangeRate.AsPublishedIsApplicable);
				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
				AssertEquals(GetMessage(), false, ExchangeRate.AsPublishedIsApplicable);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					AssertEquals(GetMessage(), false, ExchangeRate.AsPublishedIsApplicable);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					AssertEquals(GetMessage(), true, ExchangeRate.AsPublishedIsApplicable);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					AssertEquals(GetMessage(), true, ExchangeRate.AsPublishedIsApplicable);
				}
			});
		}

		public void TestPopulateRE_AsPublished()
		{
			var helper = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.Universal.IUniversalReferenceTestDataHelper>("Universal.IUniversalReferenceTestDataHelper", Factory);
			helper.CreateNewOrGetExistingDataGrouping("IN", "India");
			helper.CreateNewOrGetExistingCusCodeType("SDCUR", "IN Customs Standard Currency List", "IN");
			var codeList1PK = helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "USD", "United States Dollar", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6)).PK;
			var codeList2PK = helper.CreateNewOrGetExistingCusCodeList("IN", "SDCUR", "KRW", "South Korean Won", new ZDateTime(2019, 1, 1), new ZDateTime(2079, 6, 6)).PK;
			Factory.Save();
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList1PK, "Multiplier", "1");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2PK, "Multiplier", "100");
			Factory.Save();

			string GetMessage() => $"Currency={ExchangeRate.RE_RX_NKExCurrency}, RateType={ExchangeRate.RE_ExRateType}, Rate={ExchangeRate.RE_SellRate}";

			CombineAssertions(() =>
			{
				ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				ExchangeRate.RE_SellRate = 0m;
				AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
				ExchangeRate.RE_SellRate = 30m;
				AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
				{
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
					ExchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
					ExchangeRate.RE_SellRate = 0.561000m;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);

					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
					AssertEquals("100 KRW = 56.100000 INR", ExchangeRate.RE_AsPublished);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					AssertEquals("100 KRW = 56.100000 INR", ExchangeRate.RE_AsPublished);

					ExchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
					AssertEquals("1 USD = 0.561000 INR", ExchangeRate.RE_AsPublished);

					ExchangeRate.RE_SellRate = 6.000000m;
					AssertEquals("1 USD = 6.000000 INR", ExchangeRate.RE_AsPublished);

					ExchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.China;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
					ExchangeRate.RE_AsPublished = "xxx";
					ExchangeRate.RE_SellRate = 0.661000m;
					AssertEquals(GetMessage(), ZString.Empty, ExchangeRate.RE_AsPublished);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
					ExchangeRate.RE_AsPublished = "yyy";
					ExchangeRate.RE_SellRate = 0.500000m;
					AssertEquals("Should not clear RE_AsPublished for non-India Customs Rate", "yyy", ExchangeRate.RE_AsPublished);

					ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
					AssertEquals("Should clear RE_AsPublished for non Customs Rate", ZString.Empty, ExchangeRate.RE_AsPublished);
				}
			});
		}

		public void TestLocalClientIsApplicable()
		{
			CombineAssertions(() =>
			{
				ExchangeRate.RE_OH_Client = ZGuid.Empty;
				Assert(!ExchangeRate.LocalClientIsApplicable);

				ExchangeRate.RE_OH_Client = ZGuid.BrettsGuid;
				Factory.ClearCachedValue<bool>("IsLocalClientExchangeRatefieldNeeded");
				Assert(ExchangeRate.LocalClientIsApplicable);
			});
		}

		#region Property Tests

		public void TestRE_StartDate()
		{
			ZDateTime expectedStartDate = new ZDateTime(2004, 2, 12, 0, 0, 0);
			ExchangeRate.RE_StartDate = new ZDateTime(2004, 2, 12, 12, 10, 30);
			AssertEquals("Expecting Start date to revert to very first moment of day (00:00:00)", expectedStartDate, ExchangeRate.RE_StartDate);

			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.PeriodEndRate;
			ExchangeRate.RE_ExpiryDate = ZDateTime.BrettsBirthday;
			ExchangeRate.RE_StartDate = new ZDateTime(2004, 2, 12, 12, 10, 30);
			AssertEquals("Start date should be automatically updated to the end of Accounting Period when Rate Type is Period End Rate", PeriodEndDate, ExchangeRate.RE_StartDate);
			AssertEquals("Setting Start date must update Expiry Date when Rate Type is Period End Rate", PeriodEndDate + RefExchangeRate.EndOfDay, ExchangeRate.RE_ExpiryDate);
		}

		public void TestRE_ExpiryDate()
		{
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2004, 2, 12, 12, 10, 30);
			ZDateTime expectedExpiryDate = new ZDateTime(2004, 2, 12, 23, 59, 0);

			AssertEquals("Expecting Expiry date to revert to very last moment of day (23:59:00)", expectedExpiryDate, ExchangeRate.RE_ExpiryDate);
		}

		public void TestRE_ExpiryDateInfo()
		{
			ExchangeRate.RE_ExRateType = "PER";
			Assert("Should be readonly when Rate Type is Period End Rate", ExchangeRate.RE_ExpiryDateInfo.ReadOnly);
		}

		public void TestRE_ExRateType()
		{
			ExchangeRate.RE_StartDate = new ZDateTime(2004, 2, 12, 0, 0, 0);
			ExchangeRate.RE_ExpiryDate = new ZDateTime(2007, 11, 15, 23, 59, 0);
			ExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.PeriodEndRate;
			AssertEquals("Setting Rate Type must update Start Date when Rate Type is Period End Rate", PeriodEndDate, ExchangeRate.RE_StartDate);
			AssertEquals("Setting Rate Type must update Expiry Date when Rate Type is Period End Rate", PeriodEndDate + RefExchangeRate.EndOfDay, ExchangeRate.RE_ExpiryDate);
		}

		#endregion

		#region Implementation

		RefExchangeRate ExchangeRate;

		protected override void SetUp()
		{
			base.SetUp();
			var allowCustomsExchangeRateUpdate = Env.Security.CustomsExchangeRateUpdate.IsAllowed;
			ExchangeRate = RefExchangeRate.New(Factory);
			AccountingPeriodTestHelper accountingPeriodTestHelper = new AccountingPeriodTestHelper();
			accountingPeriodTestHelper.SetupSinglePeriod(200402, PeriodStartDate, PeriodEndDate.EndOfDay());
		}

		readonly ZDateTime PeriodStartDate = new ZDateTime(2004, 2, 1, 0, 0, 0);
		readonly ZDateTime PeriodEndDate = new ZDateTime(2004, 2, 29, 0, 0, 0);
		#endregion
	}
}
