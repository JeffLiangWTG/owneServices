using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(PricingPage))]
	sealed class PricingPageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRollUpDisplayAndStyle()
		{
			var quote = Factory.NewWithValidTestData<Quote>();

			var entry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 10);
			var pricingPage = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			CombineAssertions(() =>
			{
				foreach (var rateCategory in new[] { RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.DST })
				{
					foreach (CodeDescriptionPair rateMode in RateEntryLookups.GetTransportModesByRateCategory(rateCategory))
					{
						AssertRollUpDisplayAndStyle(pricingPage, rateCategory, rateMode.Code, expectedRollUpDisplay: DocRollupOrSortDisplayList.Codes.RollUpCharges, expectedRollUpStyle: DocRollupOrSortStyleList.Codes.NoGrouping);
					}
				}
			});
		}

		static void AssertRollUpDisplayAndStyle(PricingPage pricingPage, string rateCategory, string rateMode, string expectedRollUpDisplay, string expectedRollUpStyle)
		{
			pricingPage.FirstRateEntry.TI_RateCategory = rateCategory;
			pricingPage.FirstRateEntry.TI_Mode = rateMode;

			AssertEquals($"{rateCategory}-{rateMode} RollUpDisplay", expectedRollUpDisplay, pricingPage.RollUpDisplay);
			AssertEquals($"{rateCategory}-{rateMode} RollUpStyle", expectedRollUpStyle, pricingPage.RollUpStyle);
		}

		public void TestHasFCLWithEmptyContainer()
		{
			var quote = Factory.NewWithValidTestData<Quote>();

			var entry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100, container: "20GP");
			var pricingPage1 = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			AssertEquals("FCL rate with container", false, pricingPage1.HasFCLWithEmptyContainer);

			var entry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100, container: "");
			var pricingPage2 = new PricingPage(entry2, Factory, PricingPageStyle.Landscape);
			AssertEquals("FCL rate with empty container", true, pricingPage2.HasFCLWithEmptyContainer);

			var entry3 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100, container: "");
			var pricingPage3 = new PricingPage(entry3, Factory, PricingPageStyle.Landscape);
			AssertEquals("LCL rate with empty container", false, pricingPage3.HasFCLWithEmptyContainer);
		}

		public void TestPropertyIndexer()
		{
			PricingPage.FirstRateEntry.TI_RS_NKServiceLevel_NI = "D2D";

			AssertEquals("Service Level", "D2D", PricingPage[RateEntrySchema.TI_RS_NKServiceLevel_NI.Name]);
			AssertEquals("FreightType", "Sea", PricingPage["FreightType"]);

			PricingPage.FirstRateEntry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			AssertEquals("FreightType", "Air", PricingPage["FreightType"]);
		}

		public void TestIVisualizerNoteSupporter()
		{
			var entry = Factory.New<QuoteEntry>();
			var page = new PricingPage(entry, Factory, PricingPageStyle.Landscape);
			AssertEquals(entry.PK, ((IVisualizerNoteSupporter)page).PK);
			AssertEquals(RateEntrySchema.Constants.Prefix, ((IVisualizerNoteSupporter)page).TableCode);
			AssertEquals(Guid.Empty, ((IVisualizerNoteSupporter)page).ChildBusinessObjectPK);
		}

		/// <summary>
		/// This test tries to check the hashing that occurs when adding
		/// new rate entries to the pricing page. However, this really 
		/// depends on how the values vary in the items of this test. 
		/// </summary>
		[TestDate(2300, 11, 11)]
		[StressTest]
		public void TestAddEntry_CheckAddingHash()
		{
			var mockComparer = new Mock<PricingPageRateEntryComparer>();
			mockComparer.CallBase = true;

			var entry = Factory.NewWithValidTestData<RateEntry>();
			var page = new PricingPage(entry, Factory, PricingPageStyle.Landscape, mockComparer.Object, false);
			var startDate = ZDate.Today;

			// Add one random entry and one just like the original (according
			// to its hash code)
			for (int i = 0; i < 200; i++)
			{
				// Add a unique entry with a unique hash. This should be added
				// to the set without calling .Equals
				var anotherEntry = Factory.NewWithValidTestData<RateEntry>();
				anotherEntry.TI_RateStartDate = startDate;
				anotherEntry.TI_RateEndDate = startDate.AddMonths(2);
				startDate = startDate.AddDays(1);
				page.AddRateEntry(anotherEntry);

				// Add a unique entry with a non-unique hash. This will still
				// be added to the set, but .Equals will be called against all
				// other entries with the same hash
				anotherEntry = Factory.NewWithValidTestData<RateEntry>();
				anotherEntry.TI_OriginLRC = entry.TI_OriginLRC;
				anotherEntry.TI_DestinationLRC = entry.TI_DestinationLRC;
				anotherEntry.TI_RateStartDate = entry.TI_RateStartDate;
				anotherEntry.TI_RateEndDate = entry.TI_RateEndDate;
				anotherEntry.TI_Mode = entry.TI_Mode;
				anotherEntry.TI_RateCategory = entry.TI_RateCategory;
				anotherEntry.TI_MatchContainerRateClass = entry.TI_MatchContainerRateClass;
				page.AddRateEntry(anotherEntry);
			}

			// Loop 200 times, creating 2x entries each time; plus 1 more
			// initial entry when the page is created. So all up 401.
			// Out of the 400, 200 will never call Equals (as hash is different)
			// Out of the 400, 200 will always call equals (as hash is same)
			//
			// The first loop [1 unique hash, 2 same hash], it calls Equals once 
			// The second loop [2 unique hash, 3 same hash], it calls Equals twice
			// The third loop [3 unique hash, 4 same hash], it calls Equals thrice
			//
			// So we expect (in Python syntax):
			// sum(range(1, 201)) == 20100
			mockComparer.Verify(p =>
				p.Equals(
					It.IsAny<RateEntry>(),
					It.IsAny<RateEntry>()
				),
				Times.AtMost(20100));

			Assert(true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			var entry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.SEA, "AU", "NL", "FRT", 100);

			return new PricingPage(entry, Factory, PricingPageStyle.Landscape);
		}

		PricingPage PricingPage => pricingPage ?? (pricingPage = (PricingPage)GetNewBusinessObject());
		PricingPage pricingPage;

		#endregion
	}
}

