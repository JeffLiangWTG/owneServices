using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Testing
{
	public partial class FreightInclusiveCalculatorTest
	{
		public void TestGetRelatedFreightRateLines()
		{
			var airLSERateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AU", "", "FRT", 10m).RateLines[0];
			var fclSEARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AU", "", "FRT", 10m).RateLines[0];
			var fclROARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.FCL, Mode.ROA, "AU", "", "FRT", 10m).RateLines[0];
			var fclRAIRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.FCL, Mode.RAI, "AU", "", "FRT", 10m).RateLines[0];
			var lclROARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.ROA, "AU", "", "FRT", 10m).RateLines[0];
			var lclLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];
			var lclLRORateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.LRO, "AU", "", "FRT", 10m).RateLines[0];
			var lclFTLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.FTL, "AU", "", "FRT", 10m).RateLines[0];
			var lclLRARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.LRA, "AU", "", "FRT", 10m).RateLines[0];
			var orgFTLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.ORG, Mode.FTL, "AU", "", "ONOTE", 10m).RateLines[0];
			var orgLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.ORG, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];

			var scoSEARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SCO, Mode.SEA, "AU", "", "FRT", 10m).RateLines[0];
			var sncLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SNC, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];
			var sorLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SOR, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];
			var sorFCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SOR, Mode.FCL, "AU", "", "FRT", 10m).RateLines[0];
			var sdeLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SDE, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];
			var sdeFCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.SDE, Mode.FCL, "AU", "", "FRT", 10m).RateLines[0];

			var caiLSERateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CAI, Mode.LSE, "AU", "", "FRT", 10m).RateLines[0];
			var cfcSEARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CFC, Mode.SEA, "AU", "", "FRT", 10m).RateLines[0];
			var cfcROARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CFC, Mode.ROA, "AU", "", "FRT", 10m).RateLines[0];
			var cfcRAIRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CFC, Mode.RAI, "AU", "", "FRT", 10m).RateLines[0];
			var clcROARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.ROA, "AU", "", "FRT", 10m).RateLines[0];
			var clcLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];
			var clcLRORateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.LRO, "AU", "", "FRT", 10m).RateLines[0];
			var clcFTLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.FTL, "AU", "", "FRT", 10m).RateLines[0];
			var clcLRARateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.LRA, "AU", "", "FRT", 10m).RateLines[0];
			var corFTLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.COR, Mode.FTL, "AU", "", "ONOTE", 10m).RateLines[0];
			var corLCLRateLine = ClientRate.AddRateEntryWithFlatRateLine(Category.COR, Mode.LCL, "AU", "", "FRT", 10m).RateLines[0];

			var allRateLines = new List<RateLine>() { airLSERateLine, fclSEARateLine, fclROARateLine, fclRAIRateLine, lclROARateLine, lclLCLRateLine, lclLRORateLine, lclFTLRateLine, lclLRARateLine, orgFTLRateLine, orgLCLRateLine, scoSEARateLine, sncLCLRateLine, sorLCLRateLine, sorFCLRateLine, sdeLCLRateLine, sdeFCLRateLine, caiLSERateLine, cfcSEARateLine, cfcROARateLine, cfcRAIRateLine, clcROARateLine, clcLCLRateLine, clcLRORateLine, clcFTLRateLine, clcLRARateLine, corFTLRateLine, corLCLRateLine, };

			AssertNotEquals(Env.Registry.FreightChargeCode, Helper.ChargeCodes["ONOTE"]);

			CombineAssertions(() =>
			{
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.FTL, new[] { lclFTLRateLine, clcFTLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.LSE, new[] { airLSERateLine, caiLSERateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.AIR, new[] { airLSERateLine, caiLSERateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.LCL, new[] { lclLCLRateLine, sncLCLRateLine, clcLCLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.FCL, new[] { fclSEARateLine, scoSEARateLine, cfcSEARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.FRO, new[] { fclROARateLine, cfcROARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.FRA, new[] { fclRAIRateLine, cfcRAIRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.SEA, new[] { fclSEARateLine, lclLCLRateLine, scoSEARateLine, sncLCLRateLine, cfcSEARateLine, clcLCLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.ROA, new[] { fclROARateLine, lclLRORateLine, lclFTLRateLine, cfcROARateLine, clcLRORateLine, clcFTLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.RAI, new[] { fclRAIRateLine, lclLRARateLine, cfcRAIRateLine, clcLRARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.ORG, Mode.ALL, new[] { airLSERateLine, fclSEARateLine, fclROARateLine, fclRAIRateLine, lclROARateLine, lclLCLRateLine, lclLRORateLine, lclFTLRateLine, lclLRARateLine, scoSEARateLine, sncLCLRateLine, caiLSERateLine, cfcSEARateLine, cfcROARateLine, cfcRAIRateLine, clcROARateLine, clcLCLRateLine, clcLRORateLine, clcFTLRateLine, clcLRARateLine });

				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.FTL, new[] { lclFTLRateLine, clcFTLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.LSE, new[] { airLSERateLine, caiLSERateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.AIR, new[] { airLSERateLine, caiLSERateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.LCL, new[] { lclLCLRateLine, sncLCLRateLine, clcLCLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.FCL, new[] { fclSEARateLine, scoSEARateLine, cfcSEARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.FRO, new[] { fclROARateLine, cfcROARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.FRA, new[] { fclRAIRateLine, cfcRAIRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.SEA, new[] { fclSEARateLine, lclLCLRateLine, scoSEARateLine, sncLCLRateLine, cfcSEARateLine, clcLCLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.ROA, new[] { fclROARateLine, lclLRORateLine, lclFTLRateLine, cfcROARateLine, clcLRORateLine, clcFTLRateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.RAI, new[] { fclRAIRateLine, lclLRARateLine, cfcRAIRateLine, clcLRARateLine });
				AssertGetRelatedFreightRateLines(allRateLines, Category.COR, Mode.ALL, new[] { airLSERateLine, fclSEARateLine, fclROARateLine, fclRAIRateLine, lclROARateLine, lclLCLRateLine, lclLRORateLine, lclFTLRateLine, lclLRARateLine, scoSEARateLine, sncLCLRateLine, caiLSERateLine, cfcSEARateLine, cfcROARateLine, cfcRAIRateLine, clcROARateLine, clcLCLRateLine, clcLRORateLine, clcFTLRateLine, clcLRARateLine });
			});

			void AssertGetRelatedFreightRateLines(IEnumerable<IRateLine> rateLines, string category, string mode, IEnumerable<IRateLine> expectedRateLines)
			{
				var frtInclusiveCalculatorRateLine = ClientRate
					.AddRateEntry(category, mode, "AU", "")
					.AddRateLine("ODOC", FreightInclusiveCalculator.Code);
				TestHelper.AssertRateLines(expectedRateLines, rateLines.GetRelatedFreightRateLines(frtInclusiveCalculatorRateLine), $"{category}-{mode}");
			}
		}

		public void TestGetRelatedFreightRateLines_RateCategoryAndModeFilter()
		{
			var frtInclusiveCalculatorRateEntry = ClientRate.AddRateEntry(Category.ORG, Mode.FTL, "AU", "");
			var frtInclusiveCalculatorRateLine = frtInclusiveCalculatorRateEntry.AddRateLine("ODOC", FreightInclusiveCalculator.Code);

			var freightRateEntry = ClientRate.AddRateEntry(Category.LCL, Mode.FTL, "AU", "");
			var freightRateLine = freightRateEntry.AddRateLine("FRT", FlatCalculator.Code);

			var originLclEntry = ClientRate.AddRateEntry(Category.ORG, Mode.LCL, "AU", "");
			var originLclRateLine = originLclEntry.AddRateLine("FRT", FlatCalculator.Code);

			var airEntry = ClientRate.AddRateEntry(Category.AIR, Mode.LSE, "AU", "");
			var airRateLine = airEntry.AddRateLine("FRT", FlatCalculator.Code);

			var lines = new List<IRateLine>
			{
				freightRateLine,
				originLclRateLine,
				airRateLine
			};

			AssertGetIncludedLines(lines, freightRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: new[] { freightRateLine }, lines.GetRelatedFreightRateLines(frtInclusiveCalculatorRateLine));
		}

		public void TestGetRelatedFreightRateLines_ServiceLevel()
		{
			var orgFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.ORG, Mode.FTL, "AU", "");
			var orgFTLWithFRTCalculatorRateLine = orgFTLWithFRTCalculatorRateEntry.AddRateLine("ODOC", FreightInclusiveCalculator.Code);

			var corFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.COR, Mode.FTL, "AU", "");
			var corFTLWithFRTCalculatorRateLine = corFTLWithFRTCalculatorRateEntry.AddRateLine("OCART", FreightInclusiveCalculator.Code);

			var lclFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.FTL, "AU", "", "FRT", 10m);
			var clcFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.FTL, "AU", "", "FRT", 11m);

			var lclFTLRateLine = lclFTLRateEntry.RateLines[0];
			var clcFTLRateLine = clcFTLRateEntry.RateLines[0];

			var rateLines = new List<IRateLine>
			{
				lclFTLRateLine,
				orgFTLWithFRTCalculatorRateLine,
				clcFTLRateLine,
				corFTLWithFRTCalculatorRateLine
			};

			orgFTLWithFRTCalculatorRateEntry.TI_RS_NKServiceLevel_NI = "TT";
			corFTLWithFRTCalculatorRateEntry.TI_RS_NKServiceLevel_NI = "TT";
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));

			lclFTLRateEntry.TI_RS_NKServiceLevel_NI = "STD";
			orgFTLWithFRTCalculatorRateEntry.TI_RS_NKServiceLevel_NI = "STD";
			clcFTLRateEntry.TI_RS_NKServiceLevel_NI = "STD";
			corFTLWithFRTCalculatorRateEntry.TI_RS_NKServiceLevel_NI = "STD";
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));
		}

		public void TestGetRelatedFreightRateLines_TransportProvider()
		{
			var orgFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.ORG, Mode.FTL, "AU", "");
			var orgFTLWithFRTCalculatorRateLine = orgFTLWithFRTCalculatorRateEntry.AddRateLine("ODOC", FreightInclusiveCalculator.Code);

			var corFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.COR, Mode.FTL, "AU", "");
			var corFTLWithFRTCalculatorRateLine = corFTLWithFRTCalculatorRateEntry.AddRateLine("OCART", FreightInclusiveCalculator.Code);

			var lclFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.FTL, "AU", "", "FRT", 10m);
			var clcFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.FTL, "AU", "", "FRT", 11m);

			var lclFTLRateLine = lclFTLRateEntry.RateLines[0];
			var clcFTLRateLine = clcFTLRateEntry.RateLines[0];

			var rateLines = new List<IRateLine>
			{
				lclFTLRateLine,
				orgFTLWithFRTCalculatorRateLine,
				clcFTLRateLine,
				corFTLWithFRTCalculatorRateLine
			};

			lclFTLRateEntry.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
			orgFTLWithFRTCalculatorRateEntry.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
			clcFTLRateEntry.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
			corFTLWithFRTCalculatorRateEntry.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));

			orgFTLWithFRTCalculatorRateEntry.TI_OH_TransportProvider = ZGuid.Empty;
			corFTLWithFRTCalculatorRateEntry.TI_OH_TransportProvider = ZGuid.Empty;
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));
		}

		public void TestGetRelatedFreightRateLines_Container()
		{
			var orgFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.ORG, Mode.FTL, "AU", "");
			var orgFTLWithFRTCalculatorRateLine = orgFTLWithFRTCalculatorRateEntry.AddRateLine("ODOC", FreightInclusiveCalculator.Code);

			var corFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.COR, Mode.FTL, "AU", "");
			var corFTLWithFRTCalculatorRateLine = corFTLWithFRTCalculatorRateEntry.AddRateLine("OCART", FreightInclusiveCalculator.Code);

			var lclFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.FTL, "AU", "", "FRT", 10m);
			var clcFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.FTL, "AU", "", "FRT", 11m);

			var lclFTLRateLine = lclFTLRateEntry.RateLines[0];
			var clcFTLRateLine = clcFTLRateEntry.RateLines[0];

			var rateLines = new List<IRateLine>
			{
				lclFTLRateLine,
				orgFTLWithFRTCalculatorRateLine,
				clcFTLRateLine,
				corFTLWithFRTCalculatorRateLine
			};

			orgFTLWithFRTCalculatorRateEntry.TI_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			corFTLWithFRTCalculatorRateEntry.TI_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));

			lclFTLRateEntry.TI_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			clcFTLRateEntry.TI_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));

			orgFTLWithFRTCalculatorRateEntry.TI_RC = ZGuid.Empty;
			corFTLWithFRTCalculatorRateEntry.TI_RC = ZGuid.Empty;
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));
		}

		public void TestGetRelatedFreightRateLines_Commodity()
		{
			var orgFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.ORG, Mode.FTL, "AU", "");
			var orgFTLWithFRTCalculatorRateLine = orgFTLWithFRTCalculatorRateEntry.AddRateLine("ODOC", FreightInclusiveCalculator.Code);

			var corFTLWithFRTCalculatorRateEntry = ClientRate.AddRateEntry(Category.COR, Mode.FTL, "AU", "");
			var corFTLWithFRTCalculatorRateLine = corFTLWithFRTCalculatorRateEntry.AddRateLine("OCART", FreightInclusiveCalculator.Code);

			var lclFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.LCL, Mode.FTL, "AU", "", "FRT", 10m);
			var clcFTLRateEntry = ClientRate.AddRateEntryWithFlatRateLine(Category.CLC, Mode.FTL, "AU", "", "FRT", 11m);

			var lclFTLRateLine = lclFTLRateEntry.RateLines[0];
			var clcFTLRateLine = clcFTLRateEntry.RateLines[0];

			var rateLines = new List<IRateLine>
			{
				lclFTLRateLine,
				orgFTLWithFRTCalculatorRateLine,
				clcFTLRateLine,
				corFTLWithFRTCalculatorRateLine
			};

			orgFTLWithFRTCalculatorRateEntry.TI_RH_NKCommodityCode = null;
			lclFTLRateEntry.TI_RH_NKCommodityCode = "OCHM";
			corFTLWithFRTCalculatorRateEntry.TI_RH_NKCommodityCode = null;
			clcFTLRateEntry.TI_RH_NKCommodityCode = "OCHM";
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: new string[] { "ODOC", "OCART" });
			TestHelper.AssertRateLines(expectedRateLines: new IRateLine[] { lclFTLRateLine, clcFTLRateLine }, rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));

			orgFTLWithFRTCalculatorRateEntry.TI_RH_NKCommodityCode = "AFAT";
			corFTLWithFRTCalculatorRateEntry.TI_RH_NKCommodityCode = "AFAT";
			AssertGetIncludedLines(rateLines, lclFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(orgFTLWithFRTCalculatorRateLine));
			AssertGetIncludedLines(rateLines, clcFTLRateLine, expectedCharges: System.Array.Empty<string>());
			TestHelper.AssertRateLines(expectedRateLines: System.Array.Empty<IRateLine>(), rateLines.GetRelatedFreightRateLines(corFTLWithFRTCalculatorRateLine));
		}

		#region Implementation

		static void AssertGetIncludedLines(IEnumerable<IRateLine> rateLines, IRateLine freightRateLine, string[] expectedCharges)
			=> AssertContainsExactElementsInAnyOrder
			(
				expectedCharges,
				rateLines.GetIncludedLines(freightRateLine).Select(rateLine => rateLine.ChargeCode.AC_Code)
			);

		#endregion
	}
}
