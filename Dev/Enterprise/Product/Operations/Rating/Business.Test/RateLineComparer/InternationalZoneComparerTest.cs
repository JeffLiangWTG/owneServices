using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	public class InternationalZoneComparerTest : TestCaseWithFactory
	{
		public void TestGetZoneRankRanksByZoneType()
		{
			var importZone = Factory.NewWithValidTestData<RefZoneHeader>();
			importZone.FZ_Code = "AUIM";
			importZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.RatingImport;
			importZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var ratingZone = Factory.NewWithValidTestData<RefZoneHeader>();
			ratingZone.FZ_Code = "AURA";
			ratingZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			ratingZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));
			Factory.Save();

			var supplier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(supplier);
			var importCriteria = new TestRatingCriteria("CNSHA", "AU", FreightMode.LSE, 15M, 1M, supplier);
			var importLine = importCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, importZone));
			var ratingLine = importCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, ratingZone));

			var result = new InternationalZoneComparer(false, importCriteria).Compare(importLine, ratingLine);

			Assert("Result should be positive as import line is more specific than rating line", result > 0);

			result = new InternationalZoneComparer(false, importCriteria).Compare(ratingLine, importLine);

			Assert("Result is reversed, result cannot be 0 as there is a difference", result < 0);
		}

		public void TestGetZoneRankRanksByZoneTypeAndZoneMode()
		{
			var uldZone = Factory.NewWithValidTestData<RefZoneHeader>();
			uldZone.FZ_Code = "AULD";
			uldZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.RatingExport;
			uldZone.FZ_ZoneMode = Core.Constants.RateMode.ULD;
			uldZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var airZone = Factory.NewWithValidTestData<RefZoneHeader>();
			airZone.FZ_Code = "BAIR";
			airZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.RatingImport;
			airZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			airZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var allZone = Factory.NewWithValidTestData<RefZoneHeader>();
			allZone.FZ_Code = "CAIR";
			allZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			allZone.FZ_ZoneMode = Core.Constants.RateMode.ALL;
			allZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			Factory.Save();

			var supplier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(supplier);
			var ratingCriteria = new TestRatingCriteria("CNSHA", "AU", FreightMode.ULD, 15M, 1M, supplier);
			var uldLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, uldZone, Core.Constants.RateMode.ULD));
			var airLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, airZone, Core.Constants.RateMode.AIR));
			var allLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, allZone, Core.Constants.RateMode.AIR));

			var result = new InternationalZoneComparer(false, ratingCriteria).Compare(airLine, uldLine);
			Assert("Result should be positive as air line is matched", result > 0);

			result = new InternationalZoneComparer(false, ratingCriteria).Compare(allLine, airLine);
			Assert("Result is reversed, result cannot be 0 as there is a difference", result < 0);
		}

		public void TestGetZoneRankRanksByZoneMode()
		{
			var uldZone = Factory.NewWithValidTestData<RefZoneHeader>();
			uldZone.FZ_Code = "AULD";
			uldZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			uldZone.FZ_ZoneMode = Core.Constants.RateMode.ULD;
			uldZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var airZone = Factory.NewWithValidTestData<RefZoneHeader>();
			airZone.FZ_Code = "BAIR";
			airZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			airZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			airZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var allZone = Factory.NewWithValidTestData<RefZoneHeader>();
			allZone.FZ_Code = "CAIR";
			allZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			allZone.FZ_ZoneMode = Core.Constants.RateMode.ALL;
			allZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			Factory.Save();

			var supplier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(supplier);
			var ratingCriteria = new TestRatingCriteria("CNSHA", "AU", FreightMode.ULD, 15M, 1M, supplier);
			var uldLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, uldZone, Core.Constants.RateMode.ULD));
			var airLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, airZone, Core.Constants.RateMode.AIR));
			var allLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, allZone, Core.Constants.RateMode.AIR));

			var result = new InternationalZoneComparer(false, ratingCriteria).Compare(uldLine, airLine);
			Assert("Result should be positive as air line is matched", result > 0);

			result = new InternationalZoneComparer(false, ratingCriteria).Compare(airLine, allLine);
			Assert("Result should be positive as air line is matched", result > 0);
		}

		public void TestGetZoneRankRanksByCarrier()
		{
			var supplier = Helper.NewOrgHeader();

			var uldZone = Factory.NewWithValidTestData<RefZoneHeader>();
			uldZone.FZ_Code = "AULD";
			uldZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.RatingImport;
			uldZone.FZ_ZoneMode = Core.Constants.RateMode.ULD;
			uldZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var airZone = Factory.NewWithValidTestData<RefZoneHeader>();
			airZone.FZ_Code = "BAIR";
			airZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			airZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			airZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var allZone = Factory.NewWithValidTestData<RefZoneHeader>();
			allZone.FZ_Code = "CAIR";
			allZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			allZone.FZ_ZoneMode = Core.Constants.RateMode.ALL;
			allZone.FZ_OH_RelatedParty = supplier.PK;
			allZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			Factory.Save();

			var costing = Helper.NewCosting(supplier);
			var ratingCriteria = new TestRatingCriteria("CNSHA", "AU", FreightMode.ULD, 15M, 1M, supplier);
			var uldLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, uldZone, Core.Constants.RateMode.ULD));
			var airLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, airZone, Core.Constants.RateMode.AIR));
			var allLine = ratingCriteria.Cache.GetOrCreateFastLine(GetCostRateLineWithZone(costing, allZone, Core.Constants.RateMode.AIR));

			var result = new InternationalZoneComparer(false, ratingCriteria).Compare(uldLine, airLine);
			Assert("Result should be positive as air line is matched", result > 0);

			result = new InternationalZoneComparer(false, ratingCriteria).Compare(allLine, uldLine);
			Assert("Result should be positive as air line is matched", result > 0);
		}

		public void TestGetZoneRankDoesNotThrowException()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "AUUA";
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			zone.UNLOCOs.Add(Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBTB")));
			Factory.Save();

			var fastLineProvider = new FastLineProvider(null);
			var standardCostLine = fastLineProvider.GetOrCreate(GetCostRateLineWithZone(Helper.NewCosting(null), zone));
			var supplierCostLine = fastLineProvider.GetOrCreate(GetCostRateLineWithZone(Helper.NewCosting(Helper.NewOrgHeader()), zone));

			var message = "Comparer is looking at origin, rate lines will be compared by country (CN) and should not throw an exception";

			AssertNoExceptionThrown(message, () => new InternationalZoneComparer(true, null).Compare(standardCostLine, supplierCostLine));

			message = "Comparer is looking at dest, rate lines will be compared by zone and should not throw an exception";

			AssertNoExceptionThrown(message, () => new InternationalZoneComparer(false, null).Compare(standardCostLine, supplierCostLine));
		}

		RateLine GetCostRateLineWithZone(Costing costing, RefZoneHeader zone)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, Core.Constants.CountryCodes.China, zone.Code);
			var rateLine = entry.AddRateLine("DDOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 15m;

			return rateLine;
		}

		RateLine GetCostRateLineWithZone(Costing costing, RefZoneHeader zone, string rateMode)
		{
			var entry = costing.AddRateEntry(RatingConstants.RateCategory.DST, rateMode, Core.Constants.CountryCodes.China, zone.Code);
			var rateLine = entry.AddRateLine("DDOC", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 15m;

			return rateLine;
		}

		TestHelper helper;
		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
	}
}
