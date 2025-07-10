namespace Enterprise.Rating.Business.Testing
{
	internal sealed class StandardPageEqualityComparerTest : PageEqualityComparerTest<StandardPageEqualityComparer>
	{
		public void TestMatching()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entryA1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryA3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA4 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryA5 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryA6 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryA7 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryA7.TI_RH_NKCommodityCode = "HAZ";

			var entryB1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryB2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryB3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryB4 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryB5 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryB6 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryB7 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryB7.TI_RH_NKCommodityCode = "HAZ";

			var entryC1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryC3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC4 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryC5 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryC6 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryC7 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryC7.TI_RH_NKCommodityCode = "HAZ";

			var entryD1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryD2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryD3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryD4 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryD5 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryD6 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryD7 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryD7.TI_RH_NKCommodityCode = "HAZ";

			var entries = new RateEntry[][]
			{
				new RateEntry[] { entryA1, entryA2 },
				new RateEntry[] { entryA3 },
				new RateEntry[] { entryA4 },
				new RateEntry[] { entryA5 },
				new RateEntry[] { entryA6 },
				new RateEntry[] { entryA7 },

				new RateEntry[] { entryB1, entryB2, entryD1, entryD2 },
				new RateEntry[] { entryB3, entryD3 },
				new RateEntry[] { entryB4, entryD4 },
				new RateEntry[] { entryB5, entryD5 },
				new RateEntry[] { entryB6, entryD6 },
				new RateEntry[] { entryB7, entryD7 },

				new RateEntry[] { entryC1, entryC1 },
				new RateEntry[] { entryC3 },
				new RateEntry[] { entryC4 },
				new RateEntry[] { entryC5 },
				new RateEntry[] { entryC6 },
				new RateEntry[] { entryC7 },
			};

			AssertEqualityGrouping(entries);
		}

		public void TestGrouping_GivenRateEntriesWithDifferentIncoterms_ThenShouldBeGroupedIntoDifferentGroups()
		{
			var tariff = Factory.New<CompanyTariff>();

			var entry1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "SGSIN", "USLAX", "STD", "20GP");
			entry1.TI_QuotePageIncoTerm = "CFR";
			var entry2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "SGSIN", "USLAX", "STD", "20GP");
			entry2.TI_QuotePageIncoTerm = "FOB";

			AssertEqualityGrouping
			(
				new RateEntry[][]
				{
					new RateEntry[] { entry1 },
					new RateEntry[] { entry2 },
				}
			);
		}
	}
}
