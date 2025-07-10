using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static WiseRates.Api.Model.Rate;
using static WiseRates.Constants.WRConstants;
using ApiModel = WiseRates.Api.Model;

namespace Enterprise.Rating.Business.Testing
{
	public class ContainerQualityComparerTest : RatingTestCase
	{
		public ContainerQualityComparerTest()
		{
			wiseEntry1 = null;
			wiseEntry2 = null;
		}

		public void TestComparingWhenJobIsNotContainerised()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, false);
			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var comapere = new ContainerQualityComparer(criteria);

			CompareAndAssert(comapere, entry1, entry2, 0, "Don't compare when the job is not containerised");
			CompareAndAssert(comapere, wiseEntry1, wiseEntry2, 0, "Don't compare when the job is not containerised");
			CompareAndAssert(comapere, entry1, wiseEntry2, 0, "Don't compare when the job is not containerised");
			CompareAndAssert(comapere, wiseEntry2, entry2, 0, "Don't compare when the job is not containerised");
		}

		public void TestComparingWhenContainerQualityListIsEmpty()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, true);

			var expectedQualities = new[] { "" };
			var actualQualities = criteria.RateableMeasures.GetDistinctContainerQualities();
			AssertContainsExactElementsInAnyOrder("Prerequisites", expectedQualities, actualQualities);

			var comparer = new ContainerQualityComparer(criteria);

			CompareAndAssert(comparer, entry1, entry2, 0, "Don't compare when the Container Quality list is empty");
			CompareAndAssert(comparer, wiseEntry1, wiseEntry2, 0, "Don't compare when the Container Quality list is empty");
			CompareAndAssert(comparer, entry1, wiseEntry2, 0, "Don't compare when the Container Quality list is empty");
			CompareAndAssert(comparer, wiseEntry2, entry2, 0, "Don't compare when the Container Quality list is empty");
		}

		public void TestComparingWhenContainerQualityNotSpecified()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, true);
			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var comapere = new ContainerQualityComparer(criteria);

			SetAndAssertContainerQuality(wiseEntry1, "");
			SetAndAssertContainerQuality(wiseEntry2, "");

			CompareAndAssert(comapere, entry1, entry2, 0, "CW1 Rates naturaly doesn't have Container Quality feild. Therefore, same priority");
			CompareAndAssert(comapere, entry1, wiseEntry1, 0, "Same priority when Container Quality not specified");
			CompareAndAssert(comapere, wiseEntry1, wiseEntry2, 0, "Same priority when Container Quality not specified");
		}

		public void TestComparingCW1RateAndWiseRateWithQuality()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, true);
			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var comapere = new ContainerQualityComparer(criteria);

			SetAndAssertContainerQuality(wiseEntry1, "GOH");

			CompareAndAssert(comapere, entry1, wiseEntry1, 0, "Since there is a fallback logic when we have a blank Container Quality comparer retuens 0");
			CompareAndAssert(comapere, wiseEntry1, entry1, 0, "Since there is a fallback logic when we have a blank Container Quality comparer retuens 0");
		}

		public void TestComparingWiseRateWithQualityAndWiseRateWithEmptyQulity()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, true);
			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var comapere = new ContainerQualityComparer(criteria);

			SetAndAssertContainerQuality(wiseEntry1, "GOH");
			SetAndAssertContainerQuality(wiseEntry2, "");

			CompareAndAssert(comapere, wiseEntry1, wiseEntry2, 0, "Since there is a fallback logic when we have a blank Container Quality comparer retuens 0");
			CompareAndAssert(comapere, wiseEntry2, wiseEntry1, 0, "Since there is a fallback logic when we have a blank Container Quality comparer retuens 0");
		}

		public void TestComparingTwoWiseRateWithContainerQualitySpecified()
		{
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 1, GP20, null);
			SetAndAssertCriteriaIsContainerised(criteria, true);
			AddAndAssertContainerQuality(criteria, GP20, "GOH");
			AddAndAssertContainerQuality(criteria, GP20, "GAH");

			var comapere = new ContainerQualityComparer(criteria);

			SetAndAssertContainerQuality(wiseEntry1, "GOH");
			SetAndAssertContainerQuality(wiseEntry2, "GAH");

			CompareAndAssert(comapere, wiseEntry1, wiseEntry2, 0, "Same priority when both Wise Rates have Container Quality specified");
		}

		void CompareAndAssert(ContainerQualityComparer comparer, IRateEntry entry1, IRateEntry entry2, int expectedResult, string message = null)
		{
			var line1 = entry1.ChildRateLines.First();
			var line2 = entry2.ChildRateLines.First();

			AssertEquals(message, expectedResult, comparer.Compare(new FastLine(line1), new FastLine(line2)));
		}

		void AddAndAssertContainerQuality(RatingCriteria criteria, RefContainer container, string quality)
		{
			criteria.RateableMeasures.AddContainerGroup(container.PK, new[] {
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345", containerQuality: quality)
			});

			Assert("Setup Container Quality List", criteria.RateableMeasures.GetDistinctContainerQualities().Contains(quality));
		}

		void SetAndAssertCriteriaIsContainerised(RatingCriteria criteria, bool isContainerised)
		{
			if (isContainerised)
			{
				criteria.FreightMode = criteria.FreightMode | MasterFiles.Business.FreightMode.Containerised;
			}
			else
			{
				criteria.FreightMode = criteria.FreightMode ^ MasterFiles.Business.FreightMode.Containerised;
			}

			AssertEquals("Setup criteria.IsContainerised", isContainerised, criteria.IsContainerised);
		}

		void SetAndAssertContainerQuality(WiseEntry wiseEntry, string quality)
		{
			var customFields =
				wiseEntry.CustomFields?.Where(x => x.Code != CustomFields.CargoSphere.ContainerQuality).ToList()
				?? new List<ApiModel.CustomField>();

			if (quality != null)
			{
				customFields.Add(new ApiModel.CustomField
				{
					Code = CustomFields.CargoSphere.ContainerQuality,
					Value = quality
				});
			}

			wiseEntry.CustomFields = customFields;

			AssertEquals("Setup wiseEntry.ContainerQuality", new ZString(quality), wiseEntry.ContainerQuality);
		}

		protected override void SetUp()
		{
			base.SetUp();

			#region CW1 Costing setup

			var costing = Helper.NewCosting(null);
			entry1 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USLAX", "FRT", 100m, container: "20GP");
			entry1.TI_ContractNumber = "123";
			entry2 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USLAX", "FRT", 200m, container: "20GP");
			entry2.TI_ContractNumber = "321";

			#endregion

			#region CS Rate/Cost setup

			var wiseRate1 = new ApiModel.Rate
			{
				ProviderRateId = "R_123456",
				Carrier = "EMR",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new ApiModel.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 1000m },
				},
				ProviderCustomFields = new[]
				{
					new ApiModel.CustomField
					{
						Code = CustomFields.CargoSphere.ContainerQuality,
						Value = "GOH",
					},
				},
			};

			var wiseRate2 = new ApiModel.Rate
			{
				ProviderRateId = "Z_222333",
				Carrier = "EMR",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new ApiModel.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 2000m },
				},
			};

			var frt = Helper.ChargeCodes["FRT"];

			wiseEntry1 = new WiseEntry(wiseRate1, Factory);
			wiseEntry1.TI_RateCategory = "FCL";
			wiseEntry1.TI_Mode = "SEA";
			wiseEntry1.TI_RateStartDate = new ZDate(2020, 01, 01);
			wiseEntry1.ChildRateLines = new[]
			{
				new WiseLine(Factory, wiseRate1.Charges[0]) { TL_AC = frt.PK }
			};

			wiseEntry2 = new WiseEntry(wiseRate2, Factory);
			wiseEntry2.TI_RateCategory = "FCL";
			wiseEntry2.TI_Mode = "SEA";
			wiseEntry2.TI_RateStartDate = new ZDate(2020, 01, 01);
			wiseEntry2.ChildRateLines = new[]
			{
				new WiseLine(Factory, wiseRate2.Charges[0]) { TL_AC = frt.PK }
			};

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = TransportProvider1.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry1, wiseEntry2 };

			#endregion
		}

		RateEntry entry1;
		RateEntry entry2;
		WiseEntry wiseEntry1;
		WiseEntry wiseEntry2;
	}
}
