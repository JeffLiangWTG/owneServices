using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CompactPageEqualityComparerTest : PageEqualityComparerTest<CompactPageEqualityComparer>
	{
		public void TestMatching()
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";

			var tariff = Factory.New<CompanyTariff>();

			var entryA1 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryA2 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ROA", "AUBNE", "NZAKL", "STD", "20GP");
			entryA2.TI_RH_NKCommodityCode = "HAZ";
			var entryA3 = tariff.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AUBNE", "NZAKL", "D2D", "20GP");

			var entryB1 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryB2 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryB2.TI_RH_NKCommodityCode = "HAZ";
			var entryB3 = tariff.AddRateEntry(RatingConstants.RateCategory.LCL, "ALL", "AUBNE", "NZAKL", "D2D", "");

			var entryC1 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "NZAKL", "STD", "20GP");
			var entryC2 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NZAKL", "STD", "20GP");
			entryC2.TI_RH_NKCommodityCode = "HAZ";
			var entryC3 = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "SEA", "AUBNE", "NZAKL", "D2D", "20GP");
			entryC3.TI_RH_NKCommodityCode = "HAZ";

			var entryD1 = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryD2 = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryD2.TI_RH_NKCommodityCode = "HAZ";
			var entryD3 = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, "SEA", "AUBNE", "NZAKL", "D2D", "");
			entryD3.TI_RH_NKCommodityCode = "HAZ";

			var entryE1 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryE2 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUBNE", "NZAKL", "STD", "");
			entryE2.TI_RH_NKCommodityCode = "HAZ";
			var entryE3 = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUBNE", "NZAKL", "D2D", "");

			var entryF1 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryF2 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryF2.TI_RH_NKCommodityCode = "HAZ";
			var entryF3 = tariff.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "AUBNE", "NZAKL", "D2D", "");

			var entryG1 = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryG2 = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryG2.TI_RH_NKCommodityCode = "HAZ";
			var entryG3 = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "SEA", "AUBNE", "NZAKL", "D2D", "");
			entryG3.TI_RH_NKCommodityCode = "HAZ";

			var entryH1 = tariff.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryH2 = tariff.AddRateEntry(RatingConstants.RateCategory.SDE, "ALL", "AUBNE", "NZAKL", "STD", "");
			entryH2.TI_RH_NKCommodityCode = "HAZ";
			var entryH3 = tariff.AddRateEntry(RatingConstants.RateCategory.SDE, "SEA", "AUBNE", "NZAKL", "D2D", "");
			entryH3.TI_RH_NKCommodityCode = "HAZ";

			var entryI1 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryI2 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "ROA", "AUBNE", "NZAKL", "STD", "");
			entryI2.TI_RH_NKCommodityCode = "HAZ";
			var entryI3 = tariff.AddRateEntry(RatingConstants.RateCategory.PAC, "SEA", "AUBNE", "NZAKL", "D2D", "");
			entryI3.TI_RH_NKCommodityCode = "HAZ";

			var entryJ1 = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "ALL", "AUBNE", "NZAKL", "STD", "");
			var entryJ2 = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "ROA", "AUBNE", "NZAKL", "STD", "");
			entryJ2.TI_RH_NKCommodityCode = "HAZ";
			var entryJ3 = tariff.AddRateEntry(RatingConstants.RateCategory.UNP, "SEA", "AUBNE", "NZAKL", "D2D", "");
			entryJ3.TI_RH_NKCommodityCode = "HAZ";

			var entryK1 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "");
			var entryK2 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "SEA", "AUBNE", "", "STD", "");
			entryK2.TI_RH_NKCommodityCode = "HAZ";
			var entryK3 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ALL", "AUBNE", "", "STD", "");
			var entryK4 = tariff.AddRateEntry(RatingConstants.RateCategory.SED, "ROA", "AUBNE", "", "D2D", "");
			entryK3.TI_OH_TransportProvider = principal.PK;

			var entryL1 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "NZAKL", "STD", "");
			var entryL2 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ROA", "", "NZAKL", "STD", "");
			entryL2.TI_RH_NKCommodityCode = "HAZ";
			var entryL3 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ALL", "", "NZAKL", "STD", "");
			var entryL4 = tariff.AddRateEntry(RatingConstants.RateCategory.SID, "ROA", "", "NZAKL", "D2D", "");
			entryL3.TI_OH_TransportProvider = principal.PK;

			var entries = new RateEntry[][]
			{
				new RateEntry[] // Forwarding - Sea - GEN
				{
					entryA1,
					entryB1,
					entryE1,
					entryF1,
				},
				new RateEntry[] // Forwarding - Sea - HAZ
				{
					entryB2,
					entryE2,
				},
				new RateEntry[]
				{
					entryF2,
				},
				new RateEntry[] // Forwarding - Road - HAZ
				{
					entryA2,
				},
				new RateEntry[]
				{
					entryA3,
					entryB3,
					entryE3,
					entryF3,
				},
				new RateEntry[]
				{
					entryC3,
					entryD3,
					entryG3,
					entryH3,
				},
				new RateEntry[]
				{
					entryI3,
					entryJ3,
				},
				new RateEntry[]
				{
					entryK4,
				},
				new RateEntry[]
				{
					entryL4,
				},
				new RateEntry[] // Shipping - ALL - GEN
				{
					entryC1,
					entryD1,
					entryG1,
					entryH1,
				},
				new RateEntry[] // Shipping - ALL - HAZ
				{
					entryG2,
					entryH2,
				},
				new RateEntry[] // Shipping - Sea - HAZ
				{
					entryC2,
					entryD2,
				},
				new RateEntry[] // CFS - GEN
				{
					entryI1,
					entryJ1,
				},
				new RateEntry[] // CFS - HAZ
				{
					entryI2,
					entryJ2,
				},
				new RateEntry[]
				{
					entryK1,
				},
				new RateEntry[]
				{
					entryL1,
				},
				new RateEntry[]
				{
					entryK2,
				},
				new RateEntry[]
				{
					entryK3,
				},
				new RateEntry[]
				{
					entryL3,
				},
				new RateEntry[]
				{
					entryL2,
				},
			};

			AssertEqualityGrouping(entries);
		}
	}
}
