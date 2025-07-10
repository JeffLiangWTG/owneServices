using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CostsComparerEntry))]
	public class CostsComparerEntryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMatchesFilter()
		{
			var entry = Factory.New<RateEntry>();
			entry.TI_ContractNumber = "TI123";
			var costsComparerEntry = new CostsComparerEntry(new CostsComparer(), entry, new List<RateLine>());

			var query = new ZQuery(RateEntrySchema.TI_ContractNumber, SQLComparisonOperator.StartsWith, "TI");
			Assert(costsComparerEntry.MatchesFilter(query));
		}

		public void TestDecimalPlaces()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = costing.AddRateEntry("DST", "LSE", "", "USLAX");
			entry.AddRateLine("DDOC", FlatCalculator.Code);
			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Destination = "USLAX";
			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();

			var costsComparerEntry = new CostsComparerEntry(TestComparer, entry, new List<RateLine>(entry.RateLines.ToArray<RateLine>()));
			TestComparer.Costs.Add(costsComparerEntry);
			var summ1 = new ChargesSummaryItem("CMB", 45m, 69.69m, 99.99m);
			var summ2 = new ChargesSummaryItem("CMB", 45m, 68.68m, 88.88m);
			costsComparerEntry.SummaryItems.Add(summ1, summ2);

			TestComparer.Currency = "AUD";
			string summary = (ZString)costsComparerEntry.SummaryColumn("SummaryColumn1", typeof(ZString));
			AssertEquals("68.68/88.88", summary);

			TestComparer.Currency = "HUF";
			summary = (ZString)costsComparerEntry.SummaryColumn("SummaryColumn1", typeof(ZString));
			AssertEquals("69/89", summary);  // If it fails here... has someone entered a rate for Hungarian Forints into youtr test DB?  

			TestComparer.Currency = "IQD";
			summary = (ZString)costsComparerEntry.SummaryColumn("SummaryColumn1", typeof(ZString));
			AssertEquals("68.680/88.880", summary);  // If it fails here... has someone entered a rate for Iraqi Dinars into youtr test DB?  
		}

		public void TestRateLines()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1a.TI_RH_NKCommodityCode = "GEN";
			entry1a.TI_ViaLRC = "SGSIN";

			var entry1b = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1b.TI_ViaLRC = "SGSIN";
			entry1b.RateLines.RemoveAndDeleteAll();
			entry1b.AddRateLine("WAR", FlatCalculator.Code);

			var entry1c = costing1.AddRateEntry("FCL", "SEA", "AU", "US");
			entry1c.RateLines.RemoveAndDeleteAll();
			entry1c.AddRateLine("BAF", FlatCalculator.Code);

			var entry1d = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "US");
			entry1d.RateLines.RemoveAndDeleteAll();
			entry1d.AddRateLine("WAR", FlatCalculator.Code);
			entry1d.AddRateLine("BAF", FlatCalculator.Code);
			entry1d.AddRateLine("CAF", FlatCalculator.Code);

			var entry1e = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			entry1e.TI_RH_NKCommodityCode = ZString.Empty;
			entry1e.RateLines.RemoveAndDeleteAll();
			entry1e.AddRateLine("CAF", FlatCalculator.Code);

			var entry1f = costing1.AddRateEntry("ORG", "ALL", "AUSYD", "", "", "");
			entry1f.TI_RH_NKCommodityCode = ZString.Empty;
			entry1f.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1g = costing1.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "");
			entry1g.TI_RH_NKCommodityCode = ZString.Empty;
			entry1g.AddRateLine("ODOC", FlatCalculator.Code);
			entry1g.AddRateLine("OTHC", FlatCalculator.Code);

			var entry1h = costing1.AddRateEntry("ORG", "SEA", "AU", "", "", "");
			entry1h.TI_RH_NKCommodityCode = ZString.Empty;
			entry1h.AddRateLine("OPCH", FlatCalculator.Code);

			var entry1i = costing1.AddRateEntry("ORG", "SEA", "AUSYD", "", "", "");
			entry1i.TI_RH_NKCommodityCode = "GEN";
			entry1i.AddRateLine("OTHC", FlatCalculator.Code);

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			var costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			AssertEquals(7, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1b.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1d.RateLines[1]));
			Assert(costEntry.RateLines.Contains(entry1e.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1g.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1h.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1i.RateLines[0]));
		}

		public void TestRateLines_ConsigneeConsignorSpecific()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1a.TI_OH_Consignee = org1.PK;
			entry1a.RateLines.RemoveAndDeleteAll();
			entry1a.AddRateLine("CAF", FlatCalculator.Code);

			var entry1b = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1b.TI_OH_Consignee = org2.PK;
			entry1b.RateLines.RemoveAndDeleteAll();
			entry1b.AddRateLine("CAF", FlatCalculator.Code);

			Factory.Save();

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();

			AssertEquals(2, TestComparer.Costs.Count);
			AssertEquals(1, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(1, TestComparer.Costs[1].RateLines.Count);
			AssertNotEquals(TestComparer.Costs[0].RateLines[0], TestComparer.Costs[1].RateLines[0]);
		}

		public void TestRateLinesTransitTimeOrFrequencyOverride()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1a.TI_TransitTime = "10";
			entry1a.TI_Frequency = 4;
			entry1a.TI_FrequencyUnit = "MONTHLY";
			entry1a.AddRateLine("BAF", FlatCalculator.Code);
			entry1a.AddRateLine("CAF", FlatCalculator.Code);

			var entry1b = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1b.TI_TransitTime = "10";
			entry1b.AddRateLine("BAF", FlatCalculator.Code);
			entry1b.AddRateLine("CAF", FlatCalculator.Code);
			entry1b.AddRateLine("WAR", FlatCalculator.Code);

			var entry1c = costing1.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry1c.TI_TransitTime = "5";
			entry1c.AddRateLine("BAF", FlatCalculator.Code);
			entry1c.AddRateLine("CAF", FlatCalculator.Code);
			entry1c.AddRateLine("WAR", FlatCalculator.Code);

			Factory.Save();
			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			var costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().First(x => x.Entry.PK == entry1a.PK);
			AssertEquals(4, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[1]));
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[2]));
			Assert(costEntry.RateLines.Contains(entry1b.RateLines[3]));
		}

		public void TestRateLinesOriginOnly()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("ORG", "LSE", "AUSYD", "");
			entry1a.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1b = costing1.AddRateEntry("ORG", "LCL", "AUSYD", "");
			entry1b.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1c = costing1.AddRateEntry("ORG", "FCL", "AUSYD", "");
			entry1c.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1d = costing1.AddRateEntry("ORG", "SEA", "AUSYD", "");
			entry1d.AddRateLine("ONOTE", FlatCalculator.Code);

			var entry1e = costing1.AddRateEntry("ORG", "ALL", "AU", "");
			entry1e.AddRateLine("ONOTE", FlatCalculator.Code);

			var entry1f = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.Origin = "AU";
			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1a.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1e.RateLines[0]));

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1c.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1d.RateLines[0]));

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1b.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1d.RateLines[0]));
		}

		public void TestRateLinesDestinationOnly()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1a = costing1.AddRateEntry("DST", "LSE", "", "USLAX");
			entry1a.AddRateLine("DDOC", FlatCalculator.Code);

			var entry1b = costing1.AddRateEntry("DST", "LCL", "", "USLAX");
			entry1b.AddRateLine("DDOC", FlatCalculator.Code);

			var entry1c = costing1.AddRateEntry("DST", "FCL", "", "USLAX");
			entry1c.AddRateLine("DDOC", FlatCalculator.Code);

			var entry1d = costing1.AddRateEntry("DST", "SEA", "", "USLAX");
			entry1d.AddRateLine("DNOTE", FlatCalculator.Code);

			var entry1e = costing1.AddRateEntry("DST", "ALL", "", "US");
			entry1e.AddRateLine("DNOTE", FlatCalculator.Code);

			var entry1f = costing1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Destination = "USLAX";
			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1a.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1e.RateLines[0]));

			TestComparer.Mode = "FCL";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1c.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1d.RateLines[0]));

			TestComparer.Mode = "LCL";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1b.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1d.RateLines[0]));
		}

		public void TestRateLinesOriginOrDestinationOnlyShowsSummaryItemIfAllSameWeightVolume()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			var rateline1 = entry1.RateLines[0];
			rateline1.TL_WeightVolume = "CN";
			rateline1.TL_RateCalculator = UnitCalculator.Code;
			rateline1.TL_RX_NKCurrency = "AUD";
			rateline1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var entry2 = costing.AddRateEntry("ORG", "FCL", "AUSYD", "");
			var rateline2a = entry2.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateline2a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			var rateline2b = entry2.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateline2b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)15m;

			var entry3 = costing.AddRateEntry("DST", "FCL", "", "USLAX");
			var rateline3a = entry3.AddRateLine("DCART", FlatCalculator.Code, currencyCode: "AUD");
			rateline3a.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)30m;
			var rateline3b = entry3.AddRateLine("DDOC", FlatCalculator.Code, currencyCode: "AUD");
			rateline3b.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)25m;

			var entry4 = costing.AddRateEntry("DST", "FCL", "", "NZAKL");
			var rateline4a = entry4.AddRateLine("DDOC", FlatCalculator.Code, currencyCode: "AUD");
			rateline4a.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)40m;
			var rateline4b = entry4.AddRateLine("DCART", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateline4b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ShowAllCharges = false;
			TestComparer.ShowDestinationChargesOnly = false;
			TestComparer.SingleChargeCodeComparisonOnly = false;
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.Origin = "AUSYD";
			TestComparer.LoadCosts();

			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(1, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "UNT", 65m);

			TestComparer.ShowAllCharges = false;
			TestComparer.SingleChargeCodeComparisonOnly = false;
			TestComparer.ShowOriginChargesOnly = false;
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Origin = "";
			TestComparer.Destination = "USLAX";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(1, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "BAS", 55m);

			TestComparer.Destination = "NZAKL";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(2, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "BAS", 40m);
			AssertSummaryItem(2, "UNT", 10m);
		}

		public void TestRateLinesOriginOrDestinationOnlyShowsSummaryItemIfSameWeightVolumeAsFreight()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			var rateline1 = entry1.RateLines[0];
			rateline1.TL_WeightVolume = "CN";
			rateline1.TL_RateCalculator = UnitCalculator.Code;
			rateline1.TL_RX_NKCurrency = "AUD";
			rateline1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)200m;

			var entry2 = costing.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var rateline2 = entry2.RateLines[0];
			rateline2.TL_WeightVolume = "CN";
			rateline2.TL_RateCalculator = UnitCalculator.Code;
			rateline2.TL_RX_NKCurrency = "AUD";
			rateline2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)120m;

			var entry3 = costing.AddRateEntry("ORG", "FCL", "AUSYD", "");
			var rateline3a = entry3.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateline3a.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			var rateline3b = entry3.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateline3b.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)15m;

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ShowAllCharges = false;
			TestComparer.ShowDestinationChargesOnly = false;
			TestComparer.SingleChargeCodeComparisonOnly = false;
			TestComparer.ShowOriginChargesOnly = true;
			TestComparer.Origin = "AUSYD";
			TestComparer.LoadCosts();

			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(2, TestComparer.Costs[0].RateLines.Count);
			AssertEquals(1, TestComparer.SummaryItems.Count);
			AssertSummaryItem(1, "UNT", 15m);
		}

		void AssertSummaryItem(int columnNumber, ZString type, ZDecimal value)
		{
			if (columnNumber <= TestComparer.SummaryItems.Count)
			{
				var item = TestComparer.SummaryItems.Keys[columnNumber - 1];
				AssertEquals(type, item.Type + (item.Break.IsEmpty ? "" : item.Break.ToString("f0")));
				AssertEquals(value.ToString("f2"), TestComparer.Costs[0].SummaryColumn("SummaryColumn" + columnNumber.ToString("f0"), typeof(ZString)));
			}
			else
			{
				AssertEquals("", type);
				AssertEquals("", TestComparer.Costs[0].SummaryColumn("SummaryColumn" + columnNumber.ToString("f0"), typeof(ZString)));
			}
		}

		public void TestRateLinesForRegionsCountries()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1a = costing1.AddRateEntry("AIR", "LSE", "AU", "US");

			var entry1b = costing1.AddRateEntry("AIR", "LSE", "AUEC", "USCA");

			var entry1c = costing1.AddRateEntry("ORG", "AIR", "AUSYD", "");
			entry1c.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1d = costing1.AddRateEntry("ORG", "AIR", "AUPER", "");
			entry1d.AddRateLine("ODOC", FlatCalculator.Code);

			var entry1e = costing1.AddRateEntry("DST", "AIR", "", "USLAX");
			entry1e.AddRateLine("DDOC", FlatCalculator.Code);

			var entry1f = costing1.AddRateEntry("DST", "AIR", "", "USNYC");
			entry1f.AddRateLine("DDOC", FlatCalculator.Code);

			Factory.Save();

			TestComparer.Origin = "AUSYD";
			TestComparer.Destination = "USLAX";
			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			AssertEquals(2, TestComparer.Costs.Count);

			var costEntry1 = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			var costEntry2 = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1b.PK);
			AssertEquals(3, costEntry1.RateLines.Count);
			Assert(costEntry1.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry1.RateLines.Contains(entry1c.RateLines[0]));
			Assert(costEntry1.RateLines.Contains(entry1e.RateLines[0]));

			AssertEquals(3, costEntry2.RateLines.Count);
			Assert(costEntry2.RateLines.Contains(entry1b.RateLines[0]));
			Assert(costEntry2.RateLines.Contains(entry1c.RateLines[0]));
			Assert(costEntry2.RateLines.Contains(entry1e.RateLines[0]));

			TestComparer.Origin = "AUPER";
			TestComparer.Destination = "USNYC";
			TestComparer.LoadCosts();
			AssertEquals(1, TestComparer.Costs.Count);
			AssertEquals(3, TestComparer.Costs[0].RateLines.Count);
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1a.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1d.RateLines[0]));
			Assert(TestComparer.Costs[0].RateLines.Contains(entry1f.RateLines[0]));
		}

		[TestDate(2012, 1, 1)]
		public void TestRateLinesDates()
		{
			var costing1 = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1a = costing1.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			entry1a.RateLines.RemoveAndDeleteAll();
			entry1a.AddRateLine("WAR", FlatCalculator.Code);
			entry1a.TI_RateStartDate = new ZDate(2012, 1, 1);
			entry1a.TI_RateEndDate = new ZDate(2012, 12, 30);

			var entryBaf0 = costing1.AddRateEntry("FCL", "SEA", "AU", "US");
			entryBaf0.RateLines.RemoveAndDeleteAll();
			entryBaf0.AddRateLine("BAF", FlatCalculator.Code);
			entryBaf0.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			entryBaf0.TI_RateEndDate = new ZDate(2012, 8, 30);

			var entryBaf1 = costing1.AddRateEntry("FCL", "SEA", "AU", "US");
			entryBaf1.RateLines.RemoveAndDeleteAll();
			entryBaf1.AddRateLine("BAF", FlatCalculator.Code);
			entryBaf1.TI_RateStartDate = new ZDate(2012, 9, 1);
			entryBaf1.TI_RateEndDate = new ZDate(2012, 9, 30);

			var entryBaf2 = costing1.AddRateEntry("FCL", "SEA", "AU", "US");
			entryBaf2.RateLines.RemoveAndDeleteAll();
			entryBaf2.AddRateLine("BAF", FlatCalculator.Code);
			entryBaf2.TI_RateStartDate = new ZDate(2012, 10, 1);
			entryBaf2.TI_RateEndDate = new ZDate(2012, 10, 30);

			var entryBaf3 = costing1.AddRateEntry("FCL", "SEA", "AU", "US");
			entryBaf3.RateLines.RemoveAndDeleteAll();
			entryBaf3.AddRateLine("BAF", FlatCalculator.Code);
			entryBaf3.TI_RateStartDate = new ZDate(2012, 11, 1);
			entryBaf3.TI_RateEndDate = new ZDate(2012, 11, 30);

			Factory.Save();

			TestComparer.Mode = "FCL";
			TestComparer.ValidFromDate = new ZDateTime(2012, 9, 1);
			TestComparer.ValidToDate = new ZDateTime(2012, 9, 30);
			TestComparer.LoadCosts();
			AssertEquals("WAR + only BAF1 in date range", 2, TestComparer.Costs.Count);
			var costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			AssertEquals("main entry + 1 related BAF in date range", 2, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf1.RateLines[0]));

			TestComparer.ValidFromDate = ZDateTime.Empty;
			TestComparer.ValidToDate = new ZDateTime(2012, 9, 30);
			TestComparer.LoadCosts();
			AssertEquals("WAR + BAF0 + BAF1 in date range", 3, TestComparer.Costs.Count);
			costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			AssertEquals("main entry + 2 related BAF in date range", 3, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf0.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf1.RateLines[0]));

			TestComparer.ValidFromDate = new ZDateTime(2012, 10, 1);
			TestComparer.ValidToDate = ZDateTime.Empty;
			TestComparer.LoadCosts();
			AssertEquals("WAR + BAF2 + BAF3 in date range", 3, TestComparer.Costs.Count);
			costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			AssertEquals("main entry + 2 related BAF in date range", 3, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf2.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf3.RateLines[0]));

			TestComparer.ValidFromDate = ZDateTime.Empty;
			TestComparer.ValidToDate = ZDateTime.Empty;
			TestComparer.LoadCosts();
			AssertEquals("WAR + all BAFs in range", 5, TestComparer.Costs.Count);
			costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == entry1a.PK);
			AssertEquals("main entry + 5 related BAF in date range", 5, costEntry.RateLines.Count);
			Assert(costEntry.RateLines.Contains(entry1a.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf0.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf1.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf2.RateLines[0]));
			Assert(costEntry.RateLines.Contains(entryBaf3.RateLines[0]));
		}

		public void TestIsDeletedDependsOnRateEntry()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var rateEntry = costing.AddRateEntry("DST", "LSE", "", "USLAX");
			rateEntry.AddRateLine("DDOC", FlatCalculator.Code);

			Factory.Save();

			TestComparer.ShowAllCharges = false;
			TestComparer.ShowDestinationChargesOnly = true;
			TestComparer.Destination = "USLAX";
			TestComparer.Mode = "LSE";
			TestComparer.LoadCosts();
			var costEntry = TestComparer.Costs.Cast<CostsComparerEntry>().FirstOrDefault(x => x.Entry.PK == rateEntry.PK);

			AssertEquals(false, rateEntry.IsDeleted);
			AssertEquals(false, costEntry.IsDeleted);

			rateEntry.Delete();
			Factory.Save();

			AssertEquals(true, rateEntry.IsDeleted);
			AssertEquals(true, costEntry.IsDeleted);
		}

		#region Implementation

		CostsComparer TestComparer
		{
			get
			{
				if (fTestComparer == null)
				{
					fTestComparer = new CostsComparer();
				}

				return fTestComparer;
			}
		}

		CostsComparer fTestComparer;

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<RateEntry>();
			return new CostsComparerEntry(TestComparer, entry, new List<RateLine>());
		}

		#endregion

	}
}
