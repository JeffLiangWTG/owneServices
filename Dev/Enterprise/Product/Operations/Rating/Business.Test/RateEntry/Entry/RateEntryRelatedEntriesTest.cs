using CargoWise.Types;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateEntryRelatedEntriesTest : RatingTestCase
	{
		public void TestRelatedEntries()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			#region GIVEN Forwarding RateEntry

			// Forwarding > AIR
			var airULDEntry = AddRateEntry(quote, Category.AIR, Mode.ULD, "AUSYD", "USLAX", container: "LD-7", commodity: ZString.Empty);
			var airLSEEntry = AddRateEntry(quote, Category.AIR, Mode.LSE, "AUSYD", "USLAX", commodity: ZString.Empty);
			// Forwarding > FCL
			var fclSEAEntry = AddRateEntry(quote, Category.FCL, Mode.SEA, "AUSYD", "USLAX", container: "20GP");
			var fclROAEntry = AddRateEntry(quote, Category.FCL, Mode.ROA, "AUSYD", "USLAX", container: "CHIP");
			var fclRAIEntry = AddRateEntry(quote, Category.FCL, Mode.RAI, "AUSYD", "USLAX", container: "20GP");
			// Forwarding > LCL
			var lclLCLEntry = AddRateEntry(quote, Category.LCL, Mode.LCL, "AUSYD", "USLAX");
			var lclLROEntry = AddRateEntry(quote, Category.LCL, Mode.LRO, "AUSYD", "USLAX");
			var lclLRAEntry = AddRateEntry(quote, Category.LCL, Mode.LRA, "AUSYD", "USLAX");
			// Forwarding > ORG > SEA
			var orgSEAEntry = AddRateEntry(quote, Category.ORG, Mode.SEA, "AUSYD", "USLAX");
			var orgFCLEntry = AddRateEntry(quote, Category.ORG, Mode.FCL, "AUSYD", "USLAX", container: "20GP");
			var orgLCLEntry = AddRateEntry(quote, Category.ORG, Mode.LCL, "AUSYD", "USLAX");
			// Forwarding > ORG > AIR
			var orgAIREntry = AddRateEntry(quote, Category.ORG, Mode.AIR, "AUSYD", "USLAX");
			var orgULDEntry = AddRateEntry(quote, Category.ORG, Mode.ULD, "AUSYD", "USLAX", container: "LD-7");
			var orgLSEEntry = AddRateEntry(quote, Category.ORG, Mode.LSE, "AUSYD", "USLAX");
			// Forwarding > ORG > ROA
			var orgROAEntry = AddRateEntry(quote, Category.ORG, Mode.ROA, "AUSYD", "USLAX");
			var orgFROEntry = AddRateEntry(quote, Category.ORG, Mode.FRO, "AUSYD", "USLAX", container: "CHIP");
			var orgLROEntry = AddRateEntry(quote, Category.ORG, Mode.LRO, "AUSYD", "USLAX");
			// Forwarding > ORG > RAI
			var orgRAIEntry = AddRateEntry(quote, Category.ORG, Mode.RAI, "AUSYD", "USLAX");
			var orgFRAEntry = AddRateEntry(quote, Category.ORG, Mode.FRA, "AUSYD", "USLAX", container: "20GP");
			var orgLRAEntry = AddRateEntry(quote, Category.ORG, Mode.LRA, "AUSYD", "USLAX");
			// Forwarding > ORG > ALL
			var orgALLEntry = AddRateEntry(quote, Category.ORG, Mode.ALL, "AUSYD", "USLAX");
			// Forwarding > DST > AIR
			var dstAIREntry = AddRateEntry(quote, Category.DST, Mode.AIR, "AUSYD", "USLAX");
			var dstFCLEntry = AddRateEntry(quote, Category.DST, Mode.FCL, "AUSYD", "USLAX", container: "20GP");
			var dstLCLEntry = AddRateEntry(quote, Category.DST, Mode.LCL, "AUSYD", "USLAX");
			// Forwarding > DST > SEA
			var dstSEAEntry = AddRateEntry(quote, Category.DST, Mode.SEA, "AUSYD", "USLAX");
			var dstULDEntry = AddRateEntry(quote, Category.DST, Mode.ULD, "AUSYD", "USLAX", container: "LD-7");
			var dstLSEEntry = AddRateEntry(quote, Category.DST, Mode.LSE, "AUSYD", "USLAX");
			// Forwarding > DST > ROA
			var dstROAEntry = AddRateEntry(quote, Category.DST, Mode.ROA, "AUSYD", "USLAX");
			var dstFROEntry = AddRateEntry(quote, Category.DST, Mode.FRO, "AUSYD", "USLAX", container: "CHIP");
			var dstLROEntry = AddRateEntry(quote, Category.DST, Mode.LRO, "AUSYD", "USLAX");
			// Forwarding > DST > RAI
			var dstRAIEntry = AddRateEntry(quote, Category.DST, Mode.RAI, "AUSYD", "USLAX");
			var dstFRAEntry = AddRateEntry(quote, Category.DST, Mode.FRA, "AUSYD", "USLAX", container: "20GP");
			var dstLRAEntry = AddRateEntry(quote, Category.DST, Mode.LRA, "AUSYD", "USLAX");
			// Forwarding > DST > ALL
			var dstALLEntry = AddRateEntry(quote, Category.DST, Mode.ALL, "AUSYD", "USLAX");

			#endregion

			#region GIVEN Customs Forwarding RateEntry

			// Customs Forwarding > CAI
			var caiULDEntry = AddRateEntry(quote, Category.CAI, Mode.ULD, "AUSYD", "USLAX", container: "LD-7", commodity: ZString.Empty);
			var caiLSEEntry = AddRateEntry(quote, Category.CAI, Mode.LSE, "AUSYD", "USLAX", commodity: ZString.Empty);
			// Customs Forwarding > CFC
			var cfcSEAEntry = AddRateEntry(quote, Category.CFC, Mode.SEA, "AUSYD", "USLAX", container: "20GP");
			var cfcROAEntry = AddRateEntry(quote, Category.CFC, Mode.ROA, "AUSYD", "USLAX", container: "CHIP");
			var cfcRAIEntry = AddRateEntry(quote, Category.CFC, Mode.RAI, "AUSYD", "USLAX", container: "20GP");
			// Customs Forwarding > CLC
			var clcLCLEntry = AddRateEntry(quote, Category.CLC, Mode.LCL, "AUSYD", "USLAX");
			var clcLROEntry = AddRateEntry(quote, Category.CLC, Mode.LRO, "AUSYD", "USLAX");
			var clcLRAEntry = AddRateEntry(quote, Category.CLC, Mode.LRA, "AUSYD", "USLAX");
			// Customs Forwarding > COR > SEA
			var corSEAEntry = AddRateEntry(quote, Category.COR, Mode.SEA, "AUSYD", "USLAX");
			var corFCLEntry = AddRateEntry(quote, Category.COR, Mode.FCL, "AUSYD", "USLAX", container: "20GP");
			var corLCLEntry = AddRateEntry(quote, Category.COR, Mode.LCL, "AUSYD", "USLAX");
			// Customs Forwarding > COR > AIR
			var corAIREntry = AddRateEntry(quote, Category.COR, Mode.AIR, "AUSYD", "USLAX");
			var corULDEntry = AddRateEntry(quote, Category.COR, Mode.ULD, "AUSYD", "USLAX", container: "LD-7");
			var corLSEEntry = AddRateEntry(quote, Category.COR, Mode.LSE, "AUSYD", "USLAX");
			// Customs Forwarding > COR > ROA
			var corROAEntry = AddRateEntry(quote, Category.COR, Mode.ROA, "AUSYD", "USLAX");
			var corFROEntry = AddRateEntry(quote, Category.COR, Mode.FRO, "AUSYD", "USLAX", container: "CHIP");
			var corLROEntry = AddRateEntry(quote, Category.COR, Mode.LRO, "AUSYD", "USLAX");
			// Customs Forwarding > ORG > ALL
			var corALLEntry = AddRateEntry(quote, Category.COR, Mode.ALL, "AUSYD", "USLAX");
			// Customs Forwarding > COR > RAI
			var corRAIEntry = AddRateEntry(quote, Category.COR, Mode.RAI, "AUSYD", "USLAX");
			var corFRAEntry = AddRateEntry(quote, Category.COR, Mode.FRA, "AUSYD", "USLAX", container: "20GP");
			var corLRAEntry = AddRateEntry(quote, Category.COR, Mode.LRA, "AUSYD", "USLAX");
			// Customs Forwarding > CDS > SEA
			var cdsSEAEntry = AddRateEntry(quote, Category.CDS, Mode.SEA, "AUSYD", "USLAX");
			var cdsFCLEntry = AddRateEntry(quote, Category.CDS, Mode.FCL, "AUSYD", "USLAX", container: "20GP");
			var cdsLCLEntry = AddRateEntry(quote, Category.CDS, Mode.LCL, "AUSYD", "USLAX");
			// Customs Forwarding > CDS > AIR
			var cdsAIREntry = AddRateEntry(quote, Category.CDS, Mode.AIR, "AUSYD", "USLAX");
			var cdsULDEntry = AddRateEntry(quote, Category.CDS, Mode.ULD, "AUSYD", "USLAX", container: "LD-7");
			var cdsLSEEntry = AddRateEntry(quote, Category.CDS, Mode.LSE, "AUSYD", "USLAX");
			// Customs Forwarding > CDS > ROA
			var cdsROAEntry = AddRateEntry(quote, Category.CDS, Mode.ROA, "AUSYD", "USLAX");
			var cdsFROEntry = AddRateEntry(quote, Category.CDS, Mode.FRO, "AUSYD", "USLAX", container: "CHIP");
			var cdsLROEntry = AddRateEntry(quote, Category.CDS, Mode.LRO, "AUSYD", "USLAX");
			// Customs Forwarding > CDS > RAI
			var cdsRAIEntry = AddRateEntry(quote, Category.CDS, Mode.RAI, "AUSYD", "USLAX");
			var cdsFRAEntry = AddRateEntry(quote, Category.CDS, Mode.FRA, "AUSYD", "USLAX", container: "20GP");
			var cdsLRAEntry = AddRateEntry(quote, Category.CDS, Mode.LRA, "AUSYD", "USLAX");
			// Forwarding > DST > ALL
			var cdsALLEntry = AddRateEntry(quote, Category.CDS, Mode.ALL, "AUSYD", "USLAX");

			#endregion

			#region ASSERT Fowarding

			// Forwarding > Air
			AssertRelatedEntries(airULDEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { orgALLEntry, orgAIREntry, orgULDEntry }, expectedDestinations: new RateEntry[] { dstALLEntry, dstAIREntry, dstULDEntry }, "airULDEntry");
			AssertRelatedEntries(airLSEEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { orgALLEntry, orgAIREntry, orgLSEEntry }, expectedDestinations: new RateEntry[] { dstALLEntry, dstAIREntry, dstLSEEntry }, "airLSEEntry");
			// Forwarding > FCL
			AssertRelatedEntries(fclSEAEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { orgALLEntry, orgSEAEntry, orgFCLEntry }, expectedDestinations: new RateEntry[] { dstALLEntry, dstSEAEntry, dstFCLEntry }, "fclSEAEntry");
			// Forwarding > LCL
			AssertRelatedEntries(lclLCLEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { orgALLEntry, orgSEAEntry, orgLCLEntry }, expectedDestinations: new RateEntry[] { dstALLEntry, dstSEAEntry, dstLCLEntry }, "lclLCLEntry");
			AssertRelatedEntries(lclLROEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { orgALLEntry, orgROAEntry, orgLROEntry }, expectedDestinations: new RateEntry[] { dstALLEntry, dstROAEntry, dstLROEntry }, "lclLCLEntry");
			// Forwarding > ORG > SEA
			AssertRelatedEntries(orgSEAEntry, expectedFreights: new RateEntry[] { fclSEAEntry, lclLCLEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstSEAEntry }, "orgSEAEntry");
			AssertRelatedEntries(orgFCLEntry, expectedFreights: new RateEntry[] { fclSEAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstSEAEntry, dstFCLEntry }, "orgFCLEntry");
			AssertRelatedEntries(orgLCLEntry, expectedFreights: new RateEntry[] { lclLCLEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstSEAEntry, dstLCLEntry }, "orgLCLEntry");
			// Forwarding > ORG > AIR
			AssertRelatedEntries(orgAIREntry, expectedFreights: new RateEntry[] { airLSEEntry, airULDEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstAIREntry }, "orgAIREntry");
			AssertRelatedEntries(orgULDEntry, expectedFreights: new RateEntry[] { airULDEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstAIREntry, dstULDEntry }, "orgULDEntry");
			AssertRelatedEntries(orgLSEEntry, expectedFreights: new RateEntry[] { airLSEEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstAIREntry, dstLSEEntry }, "orgLSEEntry");
			// Forwarding > ORG > ROA
			AssertRelatedEntries(orgROAEntry, expectedFreights: new RateEntry[] { fclROAEntry, lclLROEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstROAEntry }, "orgROAEntry");
			AssertRelatedEntries(orgFROEntry, expectedFreights: new RateEntry[] { fclROAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstROAEntry, dstFROEntry }, "orgFROEntry");
			// Forwarding > ORG > RAI
			AssertRelatedEntries(orgRAIEntry, expectedFreights: new RateEntry[] { fclRAIEntry, lclLRAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstRAIEntry }, "orgRAIEntry");
			AssertRelatedEntries(orgFRAEntry, expectedFreights: new RateEntry[] { fclRAIEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry, dstRAIEntry, dstFRAEntry }, "orgFRAEntry");
			// Forwarding > ORG > ALL
			AssertRelatedEntries(orgALLEntry, expectedFreights: new RateEntry[] { airULDEntry, airLSEEntry, fclSEAEntry, fclROAEntry, fclRAIEntry, lclLCLEntry, lclLROEntry, lclLRAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { dstALLEntry }, "orgALLEntry");
			// Forwarding > DST > SEA
			AssertRelatedEntries(dstSEAEntry, expectedFreights: new RateEntry[] { fclSEAEntry, lclLCLEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgSEAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstSEAEntry");
			AssertRelatedEntries(dstFCLEntry, expectedFreights: new RateEntry[] { fclSEAEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgSEAEntry, orgFCLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstFCLEntry");
			AssertRelatedEntries(dstLCLEntry, expectedFreights: new RateEntry[] { lclLCLEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgSEAEntry, orgLCLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstLCLEntry");
			// Forwarding > DST > AIR
			AssertRelatedEntries(dstAIREntry, expectedFreights: new RateEntry[] { airLSEEntry, airULDEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgAIREntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstAIREntry");
			AssertRelatedEntries(dstULDEntry, expectedFreights: new RateEntry[] { airULDEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgAIREntry, orgULDEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstULDEntry");
			AssertRelatedEntries(dstLSEEntry, expectedFreights: new RateEntry[] { airLSEEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgAIREntry, orgLSEEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstLSEEntry");
			// Forwarding > DST > ROA
			AssertRelatedEntries(dstROAEntry, expectedFreights: new RateEntry[] { fclROAEntry, lclLROEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgROAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstROAEntry");
			AssertRelatedEntries(dstFROEntry, expectedFreights: new RateEntry[] { fclROAEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgROAEntry, orgFROEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstFROEntry");
			// Forwarding > DST > RAI
			AssertRelatedEntries(dstRAIEntry, expectedFreights: new RateEntry[] { fclRAIEntry, lclLRAEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgRAIEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstFRAEntry");
			AssertRelatedEntries(dstFRAEntry, expectedFreights: new RateEntry[] { fclRAIEntry }, expectedOrigins: new RateEntry[] { orgALLEntry, orgRAIEntry, orgFRAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstFRAEntry");
			// Forwarding > DST > ALL
			AssertRelatedEntries(dstALLEntry, expectedFreights: new RateEntry[] { airULDEntry, airLSEEntry, fclSEAEntry, fclROAEntry, fclRAIEntry, lclLCLEntry, lclLROEntry, lclLRAEntry }, expectedOrigins: new RateEntry[] { orgALLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "dstALLEntry");

			#endregion

			#region ASSERT Customs Forwarding

			// Customs Forwarding > CAI
			AssertRelatedEntries(caiULDEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { corALLEntry, corAIREntry, corULDEntry }, expectedDestinations: new RateEntry[] { cdsALLEntry, cdsAIREntry, cdsULDEntry }, "caiULDEntry");
			AssertRelatedEntries(caiLSEEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { corALLEntry, corAIREntry, corLSEEntry }, expectedDestinations: new RateEntry[] { cdsALLEntry, cdsAIREntry, cdsLSEEntry }, "caiLSEEntry");
			// Customs Forwarding > CFC
			AssertRelatedEntries(cfcSEAEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { corALLEntry, corSEAEntry, corFCLEntry }, expectedDestinations: new RateEntry[] { cdsALLEntry, cdsSEAEntry, cdsFCLEntry }, "cfcSEAEntry");
			// Customs Forwarding > CLC
			AssertRelatedEntries(clcLCLEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { corALLEntry, corSEAEntry, corLCLEntry }, expectedDestinations: new RateEntry[] { cdsALLEntry, cdsSEAEntry, cdsLCLEntry }, "clcLCLEntry");
			AssertRelatedEntries(clcLROEntry, expectedFreights: System.Array.Empty<RateEntry>(), expectedOrigins: new RateEntry[] { corALLEntry, corROAEntry, corLROEntry }, expectedDestinations: new RateEntry[] { cdsALLEntry, cdsROAEntry, cdsLROEntry }, "clcLCLEntry");
			// Customs Forwarding > COR > SEA
			AssertRelatedEntries(corSEAEntry, expectedFreights: new RateEntry[] { cfcSEAEntry, clcLCLEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsALLEntry, cdsSEAEntry }, "corSEAEntry");
			AssertRelatedEntries(corFCLEntry, expectedFreights: new RateEntry[] { cfcSEAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsSEAEntry, cdsALLEntry, cdsFCLEntry }, "corFCLEntry");
			AssertRelatedEntries(corLCLEntry, expectedFreights: new RateEntry[] { clcLCLEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsSEAEntry, cdsALLEntry, cdsLCLEntry }, "corLCLEntry");
			// Customs Forwarding > COR > AIR
			AssertRelatedEntries(corAIREntry, expectedFreights: new RateEntry[] { caiLSEEntry, caiULDEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsALLEntry, cdsAIREntry }, "corAIREntry");
			AssertRelatedEntries(corULDEntry, expectedFreights: new RateEntry[] { caiULDEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsAIREntry, cdsALLEntry, cdsULDEntry }, "corULDEntry");
			AssertRelatedEntries(corLSEEntry, expectedFreights: new RateEntry[] { caiLSEEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsAIREntry, cdsALLEntry, cdsLSEEntry }, "corLSEEntry");
			// Customs Forwarding > COR > ROA
			AssertRelatedEntries(corROAEntry, expectedFreights: new RateEntry[] { cfcROAEntry, clcLROEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsALLEntry, cdsROAEntry }, "corROAEntry");
			AssertRelatedEntries(corFROEntry, expectedFreights: new RateEntry[] { cfcROAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsROAEntry, cdsALLEntry, cdsFROEntry }, "corFROEntry");
			// Customs Forwarding > COR > RAI
			AssertRelatedEntries(corRAIEntry, expectedFreights: new RateEntry[] { cfcRAIEntry, clcLRAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsALLEntry, cdsRAIEntry }, "corRAIEntry");
			AssertRelatedEntries(corFRAEntry, expectedFreights: new RateEntry[] { cfcRAIEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsRAIEntry, cdsALLEntry, cdsFRAEntry }, "corFRAEntry");
			// Customs Forwarding > COR > ALL
			AssertRelatedEntries(corALLEntry, expectedFreights: new RateEntry[] { caiULDEntry, caiLSEEntry, cfcSEAEntry, cfcROAEntry, cfcRAIEntry, clcLCLEntry, clcLROEntry, clcLRAEntry }, expectedOrigins: System.Array.Empty<RateEntry>(), expectedDestinations: new RateEntry[] { cdsALLEntry }, "corALLEntry");
			// Customs Forwarding > CDS > SEA
			AssertRelatedEntries(cdsSEAEntry, expectedFreights: new RateEntry[] { cfcSEAEntry, clcLCLEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corSEAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsSEAEntry");
			AssertRelatedEntries(cdsFCLEntry, expectedFreights: new RateEntry[] { cfcSEAEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corSEAEntry, corFCLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsFCLEntry");
			AssertRelatedEntries(cdsLCLEntry, expectedFreights: new RateEntry[] { clcLCLEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corSEAEntry, corLCLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsLCLEntry");
			// Customs Forwarding > CDS > AIR
			AssertRelatedEntries(cdsAIREntry, expectedFreights: new RateEntry[] { caiLSEEntry, caiULDEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corAIREntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsAIREntry");
			AssertRelatedEntries(cdsULDEntry, expectedFreights: new RateEntry[] { caiULDEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corAIREntry, corULDEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsULDEntry");
			AssertRelatedEntries(cdsLSEEntry, expectedFreights: new RateEntry[] { caiLSEEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corAIREntry, corLSEEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsLSEEntry");
			// Customs Forwarding > CDS > ROA
			AssertRelatedEntries(cdsROAEntry, expectedFreights: new RateEntry[] { cfcROAEntry, clcLROEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corROAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsROAEntry");
			AssertRelatedEntries(cdsFROEntry, expectedFreights: new RateEntry[] { cfcROAEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corROAEntry, corFROEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsFROEntry");
			// Customs Forwarding > CDS > RAI
			AssertRelatedEntries(cdsRAIEntry, expectedFreights: new RateEntry[] { cfcRAIEntry, clcLRAEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corRAIEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsFRAEntry");
			AssertRelatedEntries(cdsFRAEntry, expectedFreights: new RateEntry[] { cfcRAIEntry }, expectedOrigins: new RateEntry[] { corALLEntry, corRAIEntry, corFRAEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsFRAEntry");
			// Customs Forwarding > CDS > ALL
			AssertRelatedEntries(cdsALLEntry, expectedFreights: new RateEntry[] { caiULDEntry, caiLSEEntry, cfcSEAEntry, cfcROAEntry, cfcRAIEntry, clcLCLEntry, clcLROEntry, clcLRAEntry }, expectedOrigins: new RateEntry[] { corALLEntry }, expectedDestinations: System.Array.Empty<RateEntry>(), "cdsALLEntry");

			#endregion

			void AssertRelatedEntries(RateEntry rateEntry, RateEntry[] expectedFreights, RateEntry[] expectedOrigins, RateEntry[] expectedDestinations, string message = default)
			{
				TestHelper.AssertRateEntries(expectedRateEntries: expectedFreights, rateEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), $"{message}: freight related entries");
				TestHelper.AssertRateEntries(expectedRateEntries: expectedOrigins, rateEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), $"{message}: origin related entries");
				TestHelper.AssertRateEntries(expectedRateEntries: expectedDestinations, rateEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Destination), $"{message}: destination related entries");
			}
		}

		public void TestRelatedEntries_Forwarding()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var airEntry = AddRateEntry(quote, Category.AIR, Mode.LSE, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var originEntry = AddRateEntry(quote, Category.ORG, Mode.AIR, "AUSYD", "", "", "", commodity: ZString.Empty);

			var scoEntry = AddRateEntry(quote, Category.SCO, Mode.SEA, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var sorEntry = AddRateEntry(quote, Category.SOR, Mode.ALL, "AUSYD", "", "", "", commodity: ZString.Empty);

			var caiEntry = AddRateEntry(quote, Category.CAI, Mode.LSE, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var corEntry = AddRateEntry(quote, Category.COR, Mode.AIR, "AUSYD", "", "", "", commodity: ZString.Empty);

			originEntry.TI_DestinationLRC = "USMEM";
			sorEntry.TI_DestinationLRC = "USMEM";
			corEntry.TI_DestinationLRC = "USMEM";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Destination");

			originEntry.TI_DestinationLRC = "USLAX";
			sorEntry.TI_DestinationLRC = "USLAX";
			corEntry.TI_DestinationLRC = "USLAX";
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Same Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Same Destination");

			originEntry.TI_DestinationLRC = "";
			sorEntry.TI_DestinationLRC = "";
			corEntry.TI_DestinationLRC = "";
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Blank Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Blank Destination");

			originEntry.TI_RH_NKCommodityCode = "GEN";
			sorEntry.TI_RH_NKCommodityCode = "GEN";
			corEntry.TI_RH_NKCommodityCode = "GEN";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Commodity Code");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Commodity Code");

			originEntry.TI_RH_NKCommodityCode = "";
			originEntry.TI_RS_NKServiceLevel_NI = "STD";
			sorEntry.TI_RH_NKCommodityCode = "";
			sorEntry.TI_RS_NKServiceLevel_NI = "STD";
			corEntry.TI_RH_NKCommodityCode = "";
			corEntry.TI_RS_NKServiceLevel_NI = "STD";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Service Level");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Service Level");

			originEntry.TI_RS_NKServiceLevel_NI = "";
			originEntry.TI_DestinationLRC = "US";
			sorEntry.TI_RS_NKServiceLevel_NI = "";
			sorEntry.TI_DestinationLRC = "US";
			corEntry.TI_RS_NKServiceLevel_NI = "";
			corEntry.TI_DestinationLRC = "US";
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Service Level");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Service Level");
		}

		public void TestRelatedEntries_Shipping()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var airEntry = AddRateEntry(quote, Category.AIR, Mode.LSE, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var orgEntry = AddRateEntry(quote, Category.ORG, Mode.AIR, "AUSYD", "", "", "", commodity: ZString.Empty);

			var caiEntry = AddRateEntry(quote, Category.CAI, Mode.LSE, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var corEntry = AddRateEntry(quote, Category.COR, Mode.AIR, "AUSYD", "", "", "", commodity: ZString.Empty);

			var scoEntry = AddRateEntry(quote, Category.SCO, Mode.SEA, "AUSYD", "USLAX", "", "", commodity: ZString.Empty);
			var sorEntry = AddRateEntry(quote, Category.SOR, Mode.ALL, "AUSYD", "", "", "", commodity: ZString.Empty);

			orgEntry.TI_DestinationLRC = "USMEM";
			corEntry.TI_DestinationLRC = "USMEM";
			sorEntry.TI_DestinationLRC = "USMEM";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), orgEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Different Destination");

			orgEntry.TI_DestinationLRC = "USLAX";
			corEntry.TI_DestinationLRC = "USLAX";
			sorEntry.TI_DestinationLRC = "USLAX";
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { orgEntry }, airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Same Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Same Destination");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sorEntry }, scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Same Destination");

			orgEntry.TI_RH_NKCommodityCode = "GEN";
			corEntry.TI_RH_NKCommodityCode = "GEN";
			sorEntry.TI_RH_NKCommodityCode = "GEN";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), orgEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Commodity Code");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Commodity Code");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Commodity Code");

			orgEntry.TI_RH_NKCommodityCode = "";
			orgEntry.TI_RS_NKServiceLevel_NI = "STD";
			corEntry.TI_RH_NKCommodityCode = "";
			corEntry.TI_RS_NKServiceLevel_NI = "STD";
			sorEntry.TI_RH_NKCommodityCode = "";
			sorEntry.TI_RS_NKServiceLevel_NI = "STD";
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), orgEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");

			orgEntry.TI_RS_NKServiceLevel_NI = "";
			orgEntry.TI_DestinationLRC = "US";
			corEntry.TI_RS_NKServiceLevel_NI = "";
			corEntry.TI_DestinationLRC = "US";
			sorEntry.TI_RS_NKServiceLevel_NI = "";
			sorEntry.TI_DestinationLRC = "US";
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { orgEntry }, airEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, caiEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sorEntry }, scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Diff Svc Level");
		}

		public void TestRelatedEntries_FCLFreightSeaOrigin()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var fclEntry = testQuote.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "", "20GP");
			var originEntry = testQuote.AddRateEntry(Category.ORG, Mode.SEA, "AUSYD", "USLAX");

			var scoEntry = testQuote.AddRateEntry(Category.SCO, Mode.SEA, "AUSYD", "USLAX", "", "20GP");
			var sorEntry = testQuote.AddRateEntry(Category.SOR, Mode.SEA, "AUSYD", "USLAX");

			var cfcEntry = testQuote.AddRateEntry(Category.CFC, Mode.SEA, "AUSYD", "USLAX", "", "20GP");
			var corEntry = testQuote.AddRateEntry(Category.COR, Mode.SEA, "AUSYD", "USLAX");

			originEntry.TI_Mode = Mode.SEA;
			sorEntry.TI_Mode = Mode.SEA;
			corEntry.TI_Mode = Mode.SEA;
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, fclEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is SEA");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sorEntry }, scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is SEA");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, cfcEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is SEA");

			originEntry.TI_Mode = Mode.FCL;
			sorEntry.TI_Mode = Mode.FCL;
			corEntry.TI_Mode = Mode.FCL;
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, fclEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is FCL");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sorEntry }, scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is FCL");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, cfcEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is FCL");

			originEntry.TI_Mode = Mode.ALL;
			sorEntry.TI_Mode = Mode.ALL;
			corEntry.TI_Mode = Mode.ALL;
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { originEntry }, fclEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is ALL");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sorEntry }, scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is ALL");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { corEntry }, cfcEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is ALL");

			originEntry.TI_Mode = Mode.LCL;
			sorEntry.TI_Mode = Mode.LCL;
			corEntry.TI_Mode = Mode.LCL;
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), fclEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is LCL");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), scoEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is LCL");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), cfcEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Origin), "Customs Forwarding: ORG & COR RateEntry mode is LCL");
		}

		public void TestRelatedEntries_ForOriginFCL()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var originEntry = testQuote.AddRateEntry(Category.ORG, Mode.FCL, "AUSYD", "USLAX", "", "20GP");
			var fclEntry = testQuote.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "", "20GP");

			var sorEntry = testQuote.AddRateEntry(Category.SOR, Mode.FCL, "AUSYD", "USLAX", "", "20GP");
			var scoEntry = testQuote.AddRateEntry(Category.SCO, Mode.SEA, "AUSYD", "USLAX", "", "20GP");

			var corEntry = testQuote.AddRateEntry(Category.COR, Mode.FCL, "AUSYD", "USLAX", "", "20GP");
			var cfcEntry = testQuote.AddRateEntry(Category.CFC, Mode.SEA, "AUSYD", "USLAX", "", "20GP");

			originEntry.TI_RC = GP20.PK;
			sorEntry.TI_RC = GP20.PK;
			corEntry.TI_RC = GP20.PK;
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { fclEntry }, originEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "20GP");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { scoEntry }, sorEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "20GP");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { cfcEntry }, corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "20GP");

			originEntry.TI_RC = GP40.PK;
			sorEntry.TI_RC = GP40.PK;
			corEntry.TI_RC = GP40.PK;
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), originEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "40GP");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), sorEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "40GP");
			TestHelper.AssertRateEntries(expectedRateEntries: System.Array.Empty<RateEntry>(), corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "40GP");

			originEntry.TI_RC = ZGuid.Empty;
			sorEntry.TI_RC = ZGuid.Empty;
			corEntry.TI_RC = ZGuid.Empty;
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { fclEntry }, originEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "No Container");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { scoEntry }, sorEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "No Container");
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { cfcEntry }, corEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight), "No Container");
		}

		public void TestRelatedEntries_ForOriginLCL()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var orgLCLEntry = testQuote.AddRateEntry(Category.ORG, Mode.LCL, "AUSYD", "USLAX", "");
			var lclLCLEntry = testQuote.AddRateEntry(Category.LCL, Mode.LCL, "AUSYD", "USLAX", "");

			var sorLCLEntry = testQuote.AddRateEntry(Category.SOR, Mode.LCL, "AUSYD", "USLAX", "");
			var sncLCLEntry = testQuote.AddRateEntry(Category.SNC, Mode.LCL, "AUSYD", "USLAX", "");

			var corLCLEntry = testQuote.AddRateEntry(Category.COR, Mode.LCL, "AUSYD", "USLAX", "");
			var clcLCLEntry = testQuote.AddRateEntry(Category.CLC, Mode.LCL, "AUSYD", "USLAX", "");

			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { lclLCLEntry }, orgLCLEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight));
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { sncLCLEntry }, sorLCLEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight));
			TestHelper.AssertRateEntries(expectedRateEntries: new RateEntry[] { clcLCLEntry }, corLCLEntry.GetRelatedEntries(QuoteEntry.RelatedEntriesToFindType.Freight));
		}

		#region Implementation

		static RateEntry AddRateEntry(RatingHeader ratingHeader, ZString category, string mode, string origin, string destination, string serviceLevel = "", string container = "", string commodity = "")
		{
			var rateEntry = ratingHeader.AddRateEntry(category, mode, origin, destination, serviceLevel, container);
			rateEntry.TI_RH_NKCommodityCode = commodity; // RatingHeader.AddRateEntry would not set commodity if it is null or empty
			return rateEntry;
		}

		#endregion
	}
}
