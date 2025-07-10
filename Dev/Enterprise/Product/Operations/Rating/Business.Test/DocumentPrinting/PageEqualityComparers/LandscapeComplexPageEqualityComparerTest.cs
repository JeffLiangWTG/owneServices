using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class LandscapeComplexPageEqualityComparerTest : PageEqualityComparerTest<LandscapeComplexPageEqualityComparer>
	{
		public void TestMatching()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";

			var tariff = Factory.New<Quote>();

			var entryA1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryA3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA4 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryA5 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryA6 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryA7 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryA7.TI_RH_NKCommodityCode = "HAZ";

			var entryB1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryB2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZAKL", "AUBNE", "STD", "40GP");
			var entryB3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "FCL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryB4 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZAKL", "AU", "STD", "20GP");
			var entryB5 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZ", "AUBNE", "STD", "20GP");
			var entryB6 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZAKL", "AUBNE", "D2D", "20GP");
			var entryB7 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			entryB7.TI_RH_NKCommodityCode = "HAZ";

			var entryC1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryC3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC4 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryC5 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryC6 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryC7 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryC7.TI_RH_NKCommodityCode = "HAZ";

			var entryD1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryD2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZAKL", "AUBNE", "STD", "40GP");
			var entryD3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "FCL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryD4 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZAKL", "AU", "STD", "20GP");
			var entryD5 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZ", "AUBNE", "STD", "20GP");
			var entryD6 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZAKL", "AUBNE", "D2D", "20GP");
			var entryD7 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			entryD7.TI_RH_NKCommodityCode = "HAZ";

			var entryE1 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryE2 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "STD", "40GP");
			var entryE3 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryE4 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AU", "NZAKL", "STD", "20GP");
			var entryE5 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZ", "STD", "20GP");
			var entryE6 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");
			var entryE7 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			entryE7.TI_RH_NKCommodityCode = "HAZ";

			var entryF1 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryF2 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZAKL", "AUBNE", "STD", "40GP");
			var entryF3 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "NZAKL", "AUBNE", "STD", "20GP");
			var entryF4 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZAKL", "AU", "STD", "20GP");
			var entryF5 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZ", "AUBNE", "STD", "20GP");
			var entryF6 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZAKL", "AUBNE", "D2D", "20GP");
			var entryF7 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "NZAKL", "AUBNE", "STD", "20GP");
			entryF7.TI_RH_NKCommodityCode = "HAZ";

			var entryG1 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "20GP");
			var entryG2 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "40GP");
			var entryG3 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "FCL", "AUBNE", "", "STD", "20GP");
			var entryG4 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AU", "", "STD", "20GP");
			var entryG5 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "20GP");
			var entryG6 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "D2D", "20GP");
			var entryG7 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "20GP");
			entryG7.TI_RH_NKCommodityCode = "HAZ";
			var entryG8 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "20GP");
			entryG8.TI_OH_TransportProvider = principal.PK;

			var entryH1 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "STD", "20GP");
			var entryH2 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "STD", "40GP");
			var entryH3 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "FCL", "", "AUBNE", "STD", "20GP");
			var entryH4 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AU", "STD", "20GP");
			var entryH5 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "STD", "20GP");
			var entryH6 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "D2D", "20GP");
			var entryH7 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "STD", "20GP");
			entryH7.TI_RH_NKCommodityCode = "HAZ";
			var entryH8 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "AUBNE", "STD", "20GP");
			entryH8.TI_OH_TransportProvider = principal.PK;

			var entries = new RateEntry[][]
			{
				new RateEntry[] // Forwarding - Export
				{
					entryA1,
					entryA2,
					entryA5,
				},
				new RateEntry[]
				{
					entryA3,
				},
				new RateEntry[]
				{
					entryA4,
				},
				new RateEntry[]
				{
					entryA6,
				},
				new RateEntry[]
				{
					entryA7,
				},
				new RateEntry[] // Forwarding - Import
				{
					entryB1,
					entryB2,
					entryB5,
				},
				new RateEntry[]
				{
					entryB3,
				},
				new RateEntry[]
				{
					entryB4,
				},
				new RateEntry[]
				{
					entryB6,
				},
				new RateEntry[]
				{
					entryB7,
				},
				new RateEntry[] // Shipping - Export
				{
					entryC1,
					entryC2,
					entryC5,
				},
				new RateEntry[]
				{
					entryC6,
				},
				new RateEntry[]
				{
					entryC3,
				},
				new RateEntry[]
				{
					entryC4,
				},
				new RateEntry[]
				{
					entryC7,
				},
				new RateEntry[] // Shipping - Import
				{
					entryD1,
					entryD2,
					entryD5,
				},
				new RateEntry[]
				{
					entryD6,
				},
				new RateEntry[]
				{
					entryD3,
				},
				new RateEntry[]
				{
					entryD4,
				},
				new RateEntry[]
				{
					entryD7,
				},
				new RateEntry[] // CFS
				{
					entryE1,
					entryE2,
					entryE3,
					entryE4,
					entryE5,
					entryE6,
					entryE7,

					entryF1,
					entryF2,
					entryF3,
					entryF4,
					entryF5,
					entryF6,
					entryF7,
				},
				new RateEntry[] // Shipping Detention - Export
				{
					entryG1,
					entryG2,
					entryG3,
					entryG4,
					entryG5,
					entryG6,
					entryG7,
				},
				new RateEntry[]
				{
					entryG8,
				},
				new RateEntry[] // Shipping Detention - Import
				{
					entryH1,
					entryH2,
					entryH3,
					entryH4,
					entryH5,
					entryH6,
					entryH7,
				},
				new RateEntry[]
				{
					entryH8,
				},
			};

			AssertEqualityGrouping(entries);
		}
	}
}
