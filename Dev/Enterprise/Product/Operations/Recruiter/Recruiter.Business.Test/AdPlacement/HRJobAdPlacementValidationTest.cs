using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobAdPlacementValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHQ_RX_NKAdCostCurrency()
		{
			HRJobAdPlacement ad = Factory.New<HRJobAdPlacement>();
			ad.HQ_RX_NKAdCostCurrency = "AUD";
			AssertNoErrors("Valid cost currency entered, should not have errors", ad.HQ_RX_NKAdCostCurrencyInfo);
			ad.HQ_RX_NKAdCostCurrency = "///";
			AssertHasErrors("Invalid cost currency entered, should have errors", ad.HQ_RX_NKAdCostCurrencyInfo);
		}

		[TestDate(2008, 3, 24)]
		public void TestCheckHQ_AdBookedDate()
		{
			HRJobAdPlacement ad = Factory.New<HRJobAdPlacement>();
			ad.HQ_AdBookedDate = ZDateTime.Empty;
			AssertHasErrors("Ad Booked date should be mandatory", ad.HQ_AdBookedDateInfo);

			ad.HQ_AdBookedDate = new ZDateTime(2003, 1, 1, 1, 1, 1);
			AssertNoErrors("Date entered, should not have errors", ad.HQ_AdBookedDateInfo);
		}

		[TestDate(2008, 3, 24)]
		public void TestCheckHQ_EffectiveStartDate()
		{
			HRJobAdPlacement ad = Factory.New<HRJobAdPlacement>();
			ZDateTime startDate = new ZDateTime(2004, 1, 1, 1, 1, 1);
			ZDateTime endDate = new ZDateTime(2004, 2, 2, 2, 2, 2);

			ad.HQ_EffectiveEndDate = startDate;
			ad.HQ_EffectiveStartDate = endDate;

			AssertHasErrors("End date is before start date, should have errors", ad.HQ_EffectiveStartDateInfo);

			ad.HQ_EffectiveEndDate = endDate;
			ad.HQ_EffectiveStartDate = startDate;

			AssertNoErrors("Dates valid, should not have errors", ad.HQ_EffectiveStartDateInfo);
		}

		[TestDate(2008, 3, 24)]
		public void TestCheckHQ_EffectiveEndDate()
		{
			HRJobAdPlacement ad = Factory.New<HRJobAdPlacement>();
			ZDateTime startDate = new ZDateTime(2004, 1, 1, 1, 1, 1);
			ZDateTime endDate = new ZDateTime(2004, 2, 2, 2, 2, 2);

			ad.HQ_EffectiveStartDate = endDate;
			ad.HQ_EffectiveEndDate = startDate;

			AssertHasErrors("End date is before start date, should have errors", ad.HQ_EffectiveEndDateInfo);

			ad.HQ_EffectiveStartDate = startDate;
			ad.HQ_EffectiveEndDate = endDate;

			AssertNoErrors("Dates valid, should not have errors", ad.HQ_EffectiveEndDateInfo);
		}

		public void TestCheckHQ_AdBookedIn()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("***", "Booked in");
			RecruiterDataRegistry.Instance.AdPlacementPublicationsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			HRJobAdPlacement ad = Factory.New<HRJobAdPlacement>();
			ad.HQ_AdBookedIn = "***";
			AssertNoErrors("Publication booked in is valid, should not have errors", ad.HQ_AdBookedInInfo);
			ad.HQ_AdBookedIn = ";;;";
			AssertHasErrors("Publication booked in is NOT valid, should have errors", ad.HQ_AdBookedInInfo);
		}

		public void TestCheckHQ_EffectiveStartDateLocal()
		{
			HRJobAdPlacement hrJobAdPlacement = Factory.New<HRJobAdPlacement>();
			ZDateTime startDate = ZDateTime.Now.AddYears(-1);
			ZDateTime endDate = ZDateTime.Now.AddYears(-1).AddDays(12);

			hrJobAdPlacement.HQ_EffectiveEndDateLocal = startDate;
			hrJobAdPlacement.HQ_EffectiveStartDateLocal = endDate;

			hrJobAdPlacement.Validation.ValidateHQ_EffectiveStartDate();
			AssertHasErrors("End date is before start date, should have errors", hrJobAdPlacement.HQ_EffectiveStartDateLocalInfo);

			hrJobAdPlacement.HQ_EffectiveEndDateLocal = endDate;
			hrJobAdPlacement.HQ_EffectiveStartDateLocal = startDate;

			hrJobAdPlacement.Validation.ValidateHQ_EffectiveStartDate();
			AssertNoErrors("Dates valid, should not have errors", hrJobAdPlacement.HQ_EffectiveStartDateLocalInfo);
		}

		public void TestCheckHQ_EffectiveEndDateLocal()
		{
			HRJobAdPlacement hrJobAdPlacement = Factory.New<HRJobAdPlacement>();
			ZDateTime startDate = ZDateTime.Now.AddYears(-1);
			ZDateTime endDate = ZDateTime.Now.AddYears(-1).AddDays(30);

			hrJobAdPlacement.HQ_EffectiveStartDateLocal = endDate;
			hrJobAdPlacement.HQ_EffectiveEndDateLocal = startDate;

			hrJobAdPlacement.Validation.ValidateHQ_EffectiveEndDate();
			AssertHasErrors("End date is before start date, should have errors", hrJobAdPlacement.HQ_EffectiveEndDateLocalInfo);

			hrJobAdPlacement.HQ_EffectiveStartDateLocal = startDate;
			hrJobAdPlacement.HQ_EffectiveEndDateLocal = endDate;

			hrJobAdPlacement.Validation.ValidateHQ_EffectiveEndDate();
			AssertNoErrors("Dates valid, should not have errors", hrJobAdPlacement.HQ_EffectiveEndDateLocalInfo);
		}
	}
}
