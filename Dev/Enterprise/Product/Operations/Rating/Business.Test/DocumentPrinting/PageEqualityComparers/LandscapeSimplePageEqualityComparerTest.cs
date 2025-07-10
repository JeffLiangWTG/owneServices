using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class LandscapeSimplePageEqualityComparerTest : PageEqualityComparerTest<LandscapeSimplePageEqualityComparer>
	{
		public void TestMatching()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";

			var tariff = Factory.New<CompanyTariff>();

			var entryA1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ROA", "AUBNE", "NZAKL", "STD", "20GP");
			entryA2.TI_RH_NKCommodityCode = "HAZ";

			var entryB1 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryB2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryB2.TI_RH_NKCommodityCode = "HAZ";

			var entryC1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NZAKL", "STD", "20GP");
			entryC2.TI_RH_NKCommodityCode = "HAZ";

			var entryD1 = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryD2 = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryD2.TI_RH_NKCommodityCode = "HAZ";

			var entryE1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryE2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryE2.TI_RH_NKCommodityCode = "HAZ";

			var entryF1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryF2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryF2.TI_RH_NKCommodityCode = "HAZ";

			var entryG1 = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryG2 = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryG2.TI_RH_NKCommodityCode = "HAZ";

			var entryH1 = tariff.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryH2 = tariff.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryH2.TI_RH_NKCommodityCode = "HAZ";

			var entryI1 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryI2 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryI2.TI_RH_NKCommodityCode = "HAZ";

			var entryJ1 = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryJ2 = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "ROA", "AUBNE", "NZAKL", "STD", "");
			entryJ2.TI_RH_NKCommodityCode = "HAZ";

			var entryK1 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "");
			var entryK2 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "SEA", "AUBNE", "", "STD", "");
			entryK2.TI_RH_NKCommodityCode = "HAZ";
			var entryK3 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "");
			entryK3.TI_OH_TransportProvider = principal.PK;

			var entryL1 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "NZAKL", "STD", "");
			var entryL2 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ROA", "", "NZAKL", "STD", "");
			entryL2.TI_RH_NKCommodityCode = "HAZ";
			var entryL3 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "NZAKL", "STD", "");
			entryL3.TI_OH_TransportProvider = principal.PK;

			var entries = new RateEntry[][]
			{
				new RateEntry[] // Forwarding - All
				{
					entryA1,
					entryB1,
					entryE1,
					entryF1, entryF2,
				},
				new RateEntry[] // Forwarding - Road
				{
					entryA2,
				},
				new RateEntry[] // Forwarding - Sea
				{
					entryB2,
					entryE2,
				},
				new RateEntry[] // Shipping - ALL
				{
					entryC1,
					entryD1,
					entryG1, entryG2,
					entryH1, entryH2,
				},
				new RateEntry[] // Shipping - Sea
				{
					entryC2,
					entryD2,
				},
				new RateEntry[] // CFS
				{
					entryI1, entryI2,
					entryJ1, entryJ2,
				},
				new RateEntry[] // Shipping Detention - Export
				{
					entryK1,
					entryK2,
				},
				new RateEntry[]
				{
					entryK3,
				},
				new RateEntry[] // Shipping Detention - Import
				{
					entryL1,
					entryL2,
				},
				new RateEntry[]
				{
					entryL3,
				},
			};

			AssertEqualityGrouping(entries);
		}
	}
}
