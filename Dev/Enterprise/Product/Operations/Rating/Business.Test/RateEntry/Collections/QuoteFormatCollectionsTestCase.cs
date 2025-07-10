using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal abstract class QuoteFormatCollectionsTestCase : RatingTestCase
	{
		#region Implementation

		protected Quote SetupQuote()
		{
			var air1 = Factory.NewWithValidTestData<RefContainer>();
			air1.RC_ShippingMode = "AIR";
			air1.RC_Code = "AIR1";

			var air2 = Factory.NewWithValidTestData<RefContainer>();
			air2.RC_ShippingMode = "AIR";
			air2.RC_Code = "AIR2";

			var air3 = Factory.NewWithValidTestData<RefContainer>();
			air3.RC_ShippingMode = "AIR";
			air3.RC_Code = "AIR3";

			var truck = Factory.NewWithValidTestData<RefContainer>();
			truck.RC_Code = "TTRK";

			var sea1 = Helper.Containers["20GP"];
			var sea2 = Helper.Containers["40GP"];
			var sea3 = Helper.Containers["40HC"];

			var shippingLine1 = Helper.NewOrgHeader();
			shippingLine1.OH_IsShippingProvider = true;
			shippingLine1.OH_IsShippingLine = true;
			var shippingLine2 = Helper.NewOrgHeader();
			shippingLine2.OH_IsShippingProvider = true;
			shippingLine2.OH_IsShippingLine = true;

			var org = SetupOrgheader();
			var quote = Helper.NewQuote(org);

			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var orgChargeCode = Helper.ChargeCodes["ODOC"];
			var dstChargeCode = Helper.ChargeCodes["DDOC"];

			AIREntry1 = Add("AIR", "LSE", "USNYC", "AUSYD");
			AIREntry2 = Add("AIR", "LSE", "CHZRH", "AUSYD");
			AIREntry3 = Add("AIR", "LSE", "FIHEL", "AUSYD");

			ULDEntry1 = Add("AIR", "ULD", "AUSYD", "USLAX", air1);
			ULDEntry2 = Add("AIR", "ULD", "AUSYD", "USMEM", air2);
			ULDEntry3 = Add("AIR", "ULD", "AUSYD", "USLAX", air3);

			LCLEntry1 = Add("LCL", "LCL", "CNSHA", "AUSYD");
			LCLEntry2 = Add("LCL", "LCL", "CNCAN", "AUSYD");
			LCLEntry3 = Add("LCL", "LCL", "TWKEL", "AUSYD");
			LCLEntry4 = Add("LCL", "LCL", "USNYC", "AUSYD");
			LCLEntry4.TI_RS_NKServiceLevel_NI = "D2D";

			FCLEntry1 = Add("FCL", "SEA", "CNSHA", "AUSYD", sea1);
			FCLEntry2 = Add("FCL", "SEA", "CNSHA", "AUSYD", sea2);
			FCLEntry3 = Add("FCL", "SEA", "CNSHA", "AUSYD", sea3);

			FCLEntry4 = Add("FCL", "SEA", "CNCAN", "AUSYD", sea1);
			FCLEntry5 = Add("FCL", "SEA", "CNCAN", "AUSYD", sea2, true);
			FCLEntry6 = Add("FCL", "SEA", "CNCAN", "AUSYD", sea3);

			FCLEntry7 = Add("FCL", "SEA", "TWKEL", "AUSYD", sea1);
			FCLEntry8 = Add("FCL", "SEA", "DKCPH", "AUSYD", sea2, true);
			FCLEntry9 = Add("FCL", "SEA", "NLRTM", "AUSYD", sea3);

			FTLEntry1 = Add("LCL", "FTL", "AUSYD", "AUMEL", truck);

			FCLEntry10 = Add("FCL", "SEA", "GBLON", "AUSYD", sea1);
			FCLEntry11 = Add("FCL", "SEA", "GBLON", "AUSYD", sea2);
			FCLEntry12 = Add("FCL", "SEA", "GBLON", "AUSYD", sea3);
			FCLEntry13 = Add("FCL", "SEA", "GBLON", "AUSYD", sea2, true);
			FCLEntry10.TI_OH_TransportProvider = shippingLine1.PK;
			FCLEntry11.TI_OH_TransportProvider = shippingLine1.PK;
			FCLEntry12.TI_OH_TransportProvider = shippingLine2.PK;
			FCLEntry13.TI_OH_TransportProvider = shippingLine1.PK;

			FCLEntry14 = Add("FCL", "SEA", "AUSYD", "GBLON", sea1, via: "DKCPH");
			FCLEntry15 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2, via: "DKCPH");
			FCLEntry16 = Add("FCL", "SEA", "AUSYD", "GBLON", sea1, via: "TWKEL");
			FCLEntry17 = Add("FCL", "SEA", "AUSYD", "GBLON", sea1, via: "CNCAN");
			FCLEntry18 = Add("FCL", "SEA", "AUSYD", "GBLON", sea1, via: "CNSHA");

			FCLEntry19 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry20 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry21 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry19.TI_OH_Consignee = Factory.NewWithValidTestData<OrgHeader>().PK;
			FCLEntry20.TI_OH_Consignee = Factory.NewWithValidTestData<OrgHeader>().PK;
			FCLEntry21.TI_OH_Consignee = FCLEntry20.TI_OH_Consignee;

			FCLEntry22 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry23 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry24 = Add("FCL", "SEA", "AUSYD", "GBLON", sea2);
			FCLEntry22.TI_OH_Consignor = Factory.NewWithValidTestData<OrgHeader>().PK;
			FCLEntry23.TI_OH_Consignor = Factory.NewWithValidTestData<OrgHeader>().PK;
			FCLEntry24.TI_OH_Consignor = FCLEntry23.TI_OH_Consignor;

			ORGEntry1 = Add("ORG", "AIR", "USLAX", "AU");
			ORGEntry2 = Add("ORG", "AIR", "USLAX", "AUSYD");

			DSTEntry1 = Add("DST", "FCL", "GBLON", "AUSYD", sea1);

			SCOEntry1 = Add("SCO", "SEA", "AUSYD", "GBLON", sea1);
			SCOEntry2 = Add("SCO", "SEA", "AUSYD", "NLAMS", sea1);
			SCOEntry3 = Add("SCO", "SEA", "AUSYD", "NLAMS", sea2);

			SNCEntry1 = Add("SNC", "LCL", "AUSYD", "GBLON");
			SNCEntry2 = Add("SNC", "LCL", "NLAMS", "AUSYD");

			SOREntry1 = Add("SOR", "ALL", "AUSYD", "");
			SOREntry2 = Add("SOR", "ALL", "NLAMS", "AUSYD");

			SDEEntry1 = Add("SDE", "ALL", "AU", "GBLON");

			RateEntry Add(string category, string mode, string origin, string destination, RefContainer container = null, bool isHaz = false, string via = "")
			{
				var entry = quote.AddRateEntry(category, mode, origin, destination, "STD", "");
				entry.TI_ViaLRC = via;
				entry.TI_RS_NKServiceLevel_NI = "STD";
				entry.TI_RH_NKCommodityCode = isHaz ? "HAZ" : "GEN";

				if (container != null)
				{
					entry.TI_RC = container.PK;
				}

				if (entry.IsFreightEntry())
				{
					entry.RateLines.RemoveAndDeleteAll();
					entry.AddRateLine(frtChargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 100m;
				}
				else
				{
					var chargeCode = entry.IsOriginEntry() ? orgChargeCode : dstChargeCode;
					entry.AddRateLine(chargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 20m;
				}

				return entry;
			}

			return quote;
		}

		public OrgHeader SetupOrgheader()
		{
			var org = Helper.NewOrgHeader();
			org.OH_IsConsignee = true;
			Factory.Save();
			return org;
		}

		protected RateEntry AIREntry1;
		protected RateEntry AIREntry2;
		protected RateEntry AIREntry3;

		protected RateEntry ULDEntry1;
		protected RateEntry ULDEntry2;
		protected RateEntry ULDEntry3;

		protected RateEntry LCLEntry1;
		protected RateEntry LCLEntry2;
		protected RateEntry LCLEntry3;
		protected RateEntry LCLEntry4;

		protected RateEntry FCLEntry1;
		protected RateEntry FCLEntry2;
		protected RateEntry FCLEntry3;
		protected RateEntry FCLEntry4;
		protected RateEntry FCLEntry5;
		protected RateEntry FCLEntry6;
		protected RateEntry FCLEntry7;
		protected RateEntry FCLEntry8;
		protected RateEntry FCLEntry9;
		protected RateEntry FCLEntry10;
		protected RateEntry FCLEntry11;
		protected RateEntry FCLEntry12;
		protected RateEntry FCLEntry13;
		protected RateEntry FCLEntry14;
		protected RateEntry FCLEntry15;
		protected RateEntry FCLEntry16;
		protected RateEntry FCLEntry17;
		protected RateEntry FCLEntry18;
		protected RateEntry FCLEntry19;
		protected RateEntry FCLEntry20;
		protected RateEntry FCLEntry21;
		protected RateEntry FCLEntry22;
		protected RateEntry FCLEntry23;
		protected RateEntry FCLEntry24;
		protected RateEntry FTLEntry1;
		protected RateEntry ORGEntry1;
		protected RateEntry ORGEntry2;
		protected RateEntry DSTEntry1;

		protected RateEntry SCOEntry1;
		protected RateEntry SCOEntry2;
		protected RateEntry SCOEntry3;

		protected RateEntry SNCEntry1;
		protected RateEntry SNCEntry2;

		protected RateEntry SOREntry1;
		protected RateEntry SOREntry2;

		protected RateEntry SDEEntry1;

		#endregion
	}

	internal sealed class QuoteFormatEntryCollectionTest : QuoteFormatCollectionsTestCase
	{
		public void TestLoadFreightEntries()
		{
			var testQuote = SetupQuote();
			var collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals("Count", 33, collection.Count);

			AssertNotNull(collection.FindByPK(FTLEntry1.PK));

			AssertNotNull(collection.FindByPK(AIREntry1.PK));
			AssertNotNull(collection.FindByPK(AIREntry2.PK));
			AssertNotNull(collection.FindByPK(AIREntry3.PK));

			AssertNotNull(collection.FindByPK(LCLEntry1.PK));
			AssertNotNull(collection.FindByPK(LCLEntry2.PK));
			AssertNotNull(collection.FindByPK(LCLEntry3.PK));
			AssertNotNull(collection.FindByPK(LCLEntry4.PK));

			AssertNotNull(collection.FindByPK(ULDEntry1.PK));
			AssertNull(collection.FindByPK(ULDEntry3.PK));

			AssertNotNull(collection.FindByPK(ULDEntry2.PK));

			AssertNotNull(collection.FindByPK(FCLEntry1.PK));
			AssertNull(collection.FindByPK(FCLEntry2.PK));
			AssertNull(collection.FindByPK(FCLEntry3.PK));

			AssertNotNull(collection.FindByPK(FCLEntry4.PK));
			AssertNotNull(collection.FindByPK(FCLEntry5.PK));
			AssertNull(collection.FindByPK(FCLEntry6.PK));

			AssertNotNull(collection.FindByPK(FCLEntry7.PK));
			AssertNotNull(collection.FindByPK(FCLEntry8.PK));
			AssertNotNull(collection.FindByPK(FCLEntry9.PK));

			AssertNotNull(collection.FindByPK(FCLEntry10.PK));
			AssertNull(collection.FindByPK(FCLEntry11.PK));
			AssertNotNull(collection.FindByPK(FCLEntry12.PK));
			AssertNotNull(collection.FindByPK(FCLEntry13.PK));

			AssertNotNull(collection.FindByPK(FCLEntry14.PK));
			AssertNull(collection.FindByPK(FCLEntry15.PK));
			AssertNotNull(collection.FindByPK(FCLEntry16.PK));
			AssertNotNull(collection.FindByPK(FCLEntry17.PK));
			AssertNotNull(collection.FindByPK(FCLEntry18.PK));

			AssertNotNull(collection.FindByPK(ORGEntry1.PK));
			AssertNotNull(collection.FindByPK(ORGEntry2.PK));
			AssertNull(collection.FindByPK(DSTEntry1.PK));

			AssertNotNull("SCOEntry1", collection.FindByPK(SCOEntry1.PK));
			AssertNotNull("SCOEntry2", collection.FindByPK(SCOEntry2.PK));
			AssertNull("SCOEntry3", collection.FindByPK(SCOEntry3.PK));

			AssertNotNull("SNCEntry1", collection.FindByPK(SNCEntry1.PK));
			AssertNotNull("SNCEntry2", collection.FindByPK(SNCEntry1.PK));

			AssertNull("SOREntry1", collection.FindByPK(SOREntry1.PK));
			AssertNull("SOREntry2", collection.FindByPK(SOREntry2.PK));
			AssertNull("SDEEntry1", collection.FindByPK(SDEEntry1.PK));
		}

		public void TestNoFreightEntries()
		{
			var org = SetupOrgheader();
			var testQuote = Helper.NewQuote(org);
			var chinaEntry = testQuote.AddRateEntry("DST", "LCL", "CN", "AU");
			chinaEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 10m;

			var thaiEntry = testQuote.AddRateEntry("DST", "LCL", "TH", "AU");
			thaiEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 11m;

			var blankEntry = testQuote.AddRateEntry("DST", "LCL", "", "AU");
			blankEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 12m;

			var collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals(3, collection.Count);
		}

		public void TestQuoteWithErrorsDoesntLoadEntries()
		{
			var org = SetupOrgheader();
			var testQuote = Helper.NewQuote(org);
			var chinaEntry = testQuote.AddRateEntry("DST", "LCL", "CN", "AU");
			chinaEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 10m;

			var thaiEntry = testQuote.AddRateEntry("DST", "LCL", "TH", "AU");
			thaiEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 11m;

			var blankEntry = testQuote.AddRateEntry("DST", "LCL", "", "AU");
			blankEntry.AddRateLine("DDOC").GetCalculator<FlatCalculator>().BaseRate = 12m;

			testQuote.TH_QuoteEndDate = ZDate.Invalid;
			AssertEquals(true, testQuote.HasErrors);
			var collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals("Quote End Date has errors, so we shouldn't LoadStandard Pricing Pages as we can get exceptions by putting bad data into the RelatedRateEntries ZQuery", 0, collection.Count);

			testQuote.TH_QuoteEndDate = ZDate.Today;
			AssertEquals(false, testQuote.HasErrors);
			collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals("there are no errors so we should load the entries as expected", 3, collection.Count);

			testQuote.TH_QuoteDate = ZDate.Invalid;
			AssertEquals(true, testQuote.HasErrors);
			collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals("Quote Start Date has errors, so we shouldn't LoadStandard Pricing Pages as we can get exceptions by putting bad data into the RelatedRateEntries ZQuery", 0, collection.Count);

			testQuote.TH_QuoteDate = ZDate.Today;
			AssertEquals(false, testQuote.HasErrors);
			collection = new QuoteFormatEntryCollection(Factory, testQuote);
			collection.LoadEntries();
			AssertEquals("there are no errors so we should load the entries as expected", 3, collection.Count);
		}

		public void TestQuoteFormatEntriesLoadEntries()
		{
			var client = SetupOrgheader();
			var quote = Helper.NewQuote(client);

			var airEntry = quote.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 1000m);
			var lclEntry = quote.AddRateEntryWithFlatRateLine("LCL", "LCL", "CN", "AU", "FRT", 200m);
			var dstEntry = quote.AddRateEntryWithFlatRateLine("DST", "LCL", "CN", "AU", "DDOC", 30m);
			var orgEntry = quote.AddRateEntryWithFlatRateLine("ORG", "LRO", "AUSYD", "AUMEL", "ODOC", 20m);
			Factory.Save();

			var collection = new QuoteFormatEntryCollection(Factory, quote);
			collection.LoadEntries();

			var message = "Expect to see all freight entries and the ORG entry (DST should be merged into the LCL quote)";
			var expected = new[] { airEntry.PK, lclEntry.PK, orgEntry.PK };
			var actual = collection.Select(e => e.PK);

			AssertContainsExactElementsInAnyOrder(message, expected, actual);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var reloadedQuote = newFactory.Load<Quote>(quote.PK);

			collection = new QuoteFormatEntryCollection(newFactory, reloadedQuote);
			collection.LoadEntries();

			message = "Loading this collection should load all other Entries on the Quote.";
			actual = collection.Select(e => e.PK);

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		public void TestQuoteFormatEntries_AddAndReLoadEntries()
		{
			var client = SetupOrgheader();
			var quote = Helper.NewQuote(client);
			quote.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USLAX", "FRT", 10m);
			quote.AddRateEntryWithFlatRateLine("ORG", "SEA", "AUSYD", "USLAX", "ODOC", 20m);
			Factory.Save();

			var quoteFormatEntryCollection = new QuoteFormatEntryCollection(Factory, quote);
			quoteFormatEntryCollection.LoadEntries();
			AssertContainsExactElementsInAnyOrder
			(
				"Precondition: ORG isn't shown because printed by freight",
				new[] { "FCL-SEA-FRT", },
				quoteFormatEntryCollection.Select(quoteEntry => $"{quoteEntry.TI_RateCategory}-{quoteEntry.TI_Mode}-{quoteEntry.RateLines[0].ChargeCode.AC_Code}")
			);

			quote.AddRateEntryWithFlatRateLine("DST", "SEA", "AUSYD", "USLAX", "DDOC", 30m);
			quoteFormatEntryCollection.LoadEntries();
			AssertContainsExactElementsInAnyOrder
			(
				"ORG & DST aren't shown because printed by freight",
				new[] { "FCL-SEA-FRT", },
				quoteFormatEntryCollection.Select(quoteEntry => $"{quoteEntry.TI_RateCategory}-{quoteEntry.TI_Mode}-{quoteEntry.RateLines[0].ChargeCode.AC_Code}")
			);
		}
	}

	[TestedType(typeof(QuoteFormatEntryCollection))]
	internal sealed class QuoteFormatEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new QuoteFormatEntryCollection(Factory, Factory.New<Quote>());
		}
	}
}
