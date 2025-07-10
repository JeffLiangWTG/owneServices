using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefApplicabilityViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZT_ZZA_TradeGroup()
		{
			var applicabilityView = Factory.New<CusRefApplicabilityView>();
			var tradeGroupInfo = applicabilityView.ZZT_ZZA_TradeGroupInfo;
			var error = "The Trade Group should not be empty.";

			CombineAssertions(() =>
			{
				applicabilityView.ZZT_IsSystem = true;
				applicabilityView.Validation.ValidateZZT_ZZA_TradeGroup();
				AssertNoErrors("No error when ZZT_ZZA_TradeGroup is empty and ZZT_IsSystem is true", tradeGroupInfo);

				applicabilityView.ZZT_IsSystem = false;
				applicabilityView.Validation.ValidateZZT_ZZA_TradeGroup();
				AssertHasError("Has error when ZZT_ZZA_TradeGroup is empty and ZZT_IsSystem is false", tradeGroupInfo, error);

				applicabilityView.ZZT_ZZA_TradeGroup = ZGuid.NewZGuid();
				AssertNoError("No empty error when ZZT_ZZA_TradeGroup has value", tradeGroupInfo, error);
			});
		}

		public void TestCheckZZT_StartDate()
		{
			var message = "Start Date cannot be later than End Date.";
			var message2 = "Start Date cannot be earlier than Rate's Start Date.";
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.ZZT_IsSystem = false;
			applicabilityView.ZZT_EndDate = new ZDateTime(2021, 03, 01);
			applicabilityView.ZZT_StartDate = new ZDateTime(2021, 03, 02);

			CombineAssertions(() =>
			{
				AssertHasError("Has error message", applicabilityView.ZZT_StartDateInfo, message);

				applicabilityView.ZZT_StartDate = new ZDateTime(2019, 01, 01);
				AssertHasError("Has error message2", applicabilityView.ZZT_StartDateInfo, message2);

				applicabilityView.ZZT_StartDate = new ZDateTime(2021, 02, 22);
				AssertNoError("No error message", applicabilityView.ZZT_StartDateInfo, message);
				AssertNoError("No error message2", applicabilityView.ZZT_StartDateInfo, message2);
			});
		}

		public void TestCheckZZT_StartDateIsValidZDateTimeRange()
		{
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.Rate.ZZ2_StartDate = ZDateTime.MinSmallDateTimeValue;
			applicabilityView.ZZT_IsSystem = false;
			applicabilityView.ZZT_StartDate = new ZDateTime(1999, 01, 01);

			AssertNoErrors(applicabilityView.ZZT_StartDateInfo);
		}

		public void TestCheckZZT_EndDate()
		{
			var message = "End Date cannot be earlier than Start Date.";
			var message2 = "End Date cannot be later than Rate's End Date.";
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.ZZT_IsSystem = false;
			applicabilityView.ZZT_StartDate = new ZDateTime(2021, 02, 22);
			applicabilityView.ZZT_EndDate = new ZDateTime(2021, 02, 20);

			CombineAssertions(() =>
			{
				AssertHasError("Has error message", applicabilityView.ZZT_EndDateInfo, message);

				applicabilityView.ZZT_EndDate = new ZDateTime(2023, 01, 01);
				AssertHasError("Has error message2", applicabilityView.ZZT_EndDateInfo, message2);

				applicabilityView.ZZT_EndDate = new ZDateTime(2021, 02, 24);
				AssertNoError("No error message", applicabilityView.ZZT_EndDateInfo, message);
				AssertNoError("No error message2", applicabilityView.ZZT_EndDateInfo, message2);
			});
		}

		public void TestZZT_OrderNumberIsNotMandatory()
		{
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.ZZT_OrderNumber = ZString.Empty;
			AssertNoNotifications("ZZT_OrderNumber is not mandatory.", applicabilityView.ZZT_OrderNumberInfo);
		}

		public void TestCheckZZT_OrderNumber_TradeGroupAndOrderNumberAndStartDateIsUnique()
		{
			var message = "The Applicability with same Start Date, Trade Group and Order already exists.";
			var applicabilityView = CreateCusRefApplicabilityView();
			var cusRefTradeGroup = Factory.New<CusRefTradeGroup>();

			applicabilityView.ZZT_StartDate = new ZDateTime(2021, 01, 01);
			applicabilityView.ZZT_ZZA_TradeGroup = cusRefTradeGroup.PK;
			applicabilityView.ZZT_OrderNumber = "001";

			var applicabilityView2 = applicabilityView.Rate.FilteredRateApplicabilities.AddNew();
			applicabilityView2.ZZT_StartDate = applicabilityView.ZZT_StartDate;
			applicabilityView2.ZZT_ZZA_TradeGroup = applicabilityView.ZZT_ZZA_TradeGroup;
			applicabilityView2.ZZT_OrderNumber = applicabilityView.ZZT_OrderNumber;

			CombineAssertions(() =>
			{
				AssertHasError("Same Start Date, Trade Group and Order", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_ZZA_TradeGroup = ZGuid.Empty;
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertNoError("Same Start Date and Order, different Trade Group", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_ZZA_TradeGroup = applicabilityView.ZZT_ZZA_TradeGroup;
				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 02, 02);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertNoError("Same Trade Group and Order, different Start Date", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_StartDate = applicabilityView.ZZT_StartDate;
				applicabilityView2.ZZT_OrderNumber = "002";
				AssertNoError("Same Start Date Trade Group, different Order", applicabilityView2.ZZT_OrderNumberInfo, message);
			});
		}

		public void TestCheckZZT_OrderNumber_TradeGroupAndOrderNumberIsSameAndNotOverlap()
		{
			var message = "The date range of this Applicability overlaps with another Applicability with same Trade Group and Order.";
			var applicabilityView = CreateCusRefApplicabilityView();
			var cusRefTradeGroup = Factory.New<CusRefTradeGroup>();

			applicabilityView.ZZT_StartDate = new ZDateTime(2021, 02, 01);
			applicabilityView.ZZT_EndDate = new ZDateTime(2021, 03, 01);
			applicabilityView.ZZT_ZZA_TradeGroup = cusRefTradeGroup.PK;
			applicabilityView.ZZT_OrderNumber = "001";

			var applicabilityView2 = applicabilityView.Rate.FilteredRateApplicabilities.AddNew();
			applicabilityView2.ZZT_ZZA_TradeGroup = applicabilityView.ZZT_ZZA_TradeGroup;
			applicabilityView2.ZZT_OrderNumber = applicabilityView.ZZT_OrderNumber;

			CombineAssertions(() =>
			{
				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 01, 01);
				applicabilityView2.ZZT_EndDate = new ZDateTime(2021, 01, 10);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertNoError("ZZT_StartDate < ZZT_EndDate < Existing StartDate", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 01, 01);
				applicabilityView2.ZZT_EndDate = new ZDateTime(2021, 02, 01);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertHasError("ZZT_StartDate < Existing StartDate <= ZZT_EndDate", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 02, 10);
				applicabilityView2.ZZT_EndDate = new ZDateTime(2021, 03, 01);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertHasError("ZZT_StartDate < Existing EndDate <= ZZT_EndDate", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 03, 01);
				applicabilityView2.ZZT_EndDate = new ZDateTime(2021, 03, 10);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertHasError("ZZT_StartDate <= Existing EndDate < ZZT_EndDate", applicabilityView2.ZZT_OrderNumberInfo, message);

				applicabilityView2.ZZT_StartDate = new ZDateTime(2021, 03, 10);
				applicabilityView2.ZZT_EndDate = new ZDateTime(2021, 03, 31);
				applicabilityView2.Validation.ValidateZZT_OrderNumber();
				AssertNoError("Existing EndDate < ZZT_StartDate < ZZT_EndDate", applicabilityView2.ZZT_OrderNumberInfo, message);
			});
		}

		[TestDate(2021, 02, 23)]
		public void TestCheckZZT_EndDateIsValidZDateTimeRange()
		{
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.Rate.ZZ2_EndDate = ZDateTime.MaxSmallDateTime;
			applicabilityView.ZZT_IsSystem = false;
			applicabilityView.ZZT_EndDate = new ZDateTime(2027, 02, 23);

			AssertNoErrors(applicabilityView.ZZT_EndDateInfo);
		}

		public void TestCheckZZT_AdditionalCodeIsNotEmpty()
		{
			var applicabilityView = CreateCusRefApplicabilityView();
			applicabilityView.ZZT_IsSystem = false;
			applicabilityView.ZZT_AdditionalCode = ZString.Empty;

			AssertNoErrors(applicabilityView.ZZT_AdditionalCodeInfo);
		}

		CusRefApplicabilityView CreateCusRefApplicabilityView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01));
			Factory.Save();
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK, isSystem: true);
			Factory.Save();
			var cusRate = helper.CreateRate(cusTariff, rateCode.PK, new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01), "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea, isSystem: true);
			Factory.Save();

			return cusRate.FilteredRateApplicabilities.AddNew();
		}
	}
}
