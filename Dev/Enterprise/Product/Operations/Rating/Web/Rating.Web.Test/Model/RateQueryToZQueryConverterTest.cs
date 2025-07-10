using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateQueryToZQueryConverterTest : RatingTestCase
	{
		public void TestConvert_RateProviders()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costingEntryPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true).PK;
			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.RateProviders = new[] { RatesAPIsConstants.RateProviders.CGCS };
			AssertQueryConvert(rateQuery, Array.Empty<ZGuid>(), sourceEndpoint: SourceEndpoint.Costing);

			rateQuery.RateProviders = new[] { RatesAPIsConstants.RateProviders.CargoWise, RatesAPIsConstants.RateProviders.CGCS };
			AssertQueryConvert(rateQuery, new[] { costingEntryPK }, sourceEndpoint: SourceEndpoint.Costing);
		}

		public void TestConvert_Company()
		{
			var currentCompanyPK = Env.CurrentCompanyPK;
			var anotherCompanyPK = Factory.NewWithValidTestData<GlbCompany>().PK;

			var localCosting = Helper.NewCosting(Helper.NewOrgHeader());
			var localEntryPK = localCosting.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true).PK;
			AssertEquals(currentCompanyPK, localCosting.TH_GC);

			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			var globalEntryPK = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true).PK;

			var standardCosting = Helper.NewCosting(null);
			AssertEquals(currentCompanyPK, standardCosting.TH_GC);
			var standardEntryPK = standardCosting.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true).PK;

			var anotherCompanyCosting = Helper.NewCosting(Helper.NewOrgHeader());
			anotherCompanyCosting.TH_GC = anotherCompanyPK;
			anotherCompanyCosting.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { globalEntryPK, localEntryPK, standardEntryPK };

			var rateQuery = GetValidRateQuery();

			AssertQueryConvert(rateQuery, expectedRateEntries, null, Env.CurrentCompanyPK);
		}

		public void TestConvert_RateType()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			var companyTariffEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			companyTariff.Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var intercompanyTariff = Helper.NewIntercompanyTariff();
			var intercompanyTariffEntry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var expectedRateEntries = Array.Empty<ZGuid>();
			var expectedLogs = new[] { "Only 'Costing', 'Client Rates' , 'Company Tariff' and 'Intercompany Tariff' are supported by now." };

			var rateQuery = GetValidRateQuery();

			AssertQueryConvert(rateQuery, new[] { companyTariffEntry.PK }, Enumerable.Empty<string>(), sourceEndpoint: SourceEndpoint.CompanyTariffs);
			AssertQueryConvert(rateQuery, new[] { costingEntry.PK }, Enumerable.Empty<string>(), sourceEndpoint: SourceEndpoint.Costing);
			AssertQueryConvert(rateQuery, new[] { clientRateEntry.PK }, Enumerable.Empty<string>(), sourceEndpoint: SourceEndpoint.ClientRates);
			AssertQueryConvert(rateQuery, new[] { intercompanyTariffEntry.PK }, Enumerable.Empty<string>(), sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_Origin_ISO3166CountryCode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var auPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true).PK;
			var ausydPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true).PK;

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { auPK, ausydPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.Country, Value = "AU" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Origin_UNLOCODE()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var ausydPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true).PK;

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { ausydPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Origin_Postcode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			var rateEntryWithOriginPostCode = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			rateEntryWithOriginPostCode.TI_CartagePickupAddressPostCode = "2070";

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { rateEntryWithOriginPostCode.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.Postcode, Value = "2070" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Origin_Zone()
		{
			var zone = Helper.NewInternationalZone("AUXX", null, "AUSYD", "AUMEL");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var zoneRateEntryPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUXX", "USLAX", removeLines: true).PK;
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { zoneRateEntryPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.Zone, Value = "AUXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Origin_Other()
		{
			var zone = Helper.NewInternationalZone("AUXX", null, "AUSYD", "AUMEL");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var zoneRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUXX", "USLAX", removeLines: true);
			var unlocoRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			var countryRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { zoneRateEntry.PK, unlocoRateEntry.PK, countryRateEntry.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Origin = new Location() { Type = Location.Types.IATACity, Value = "SYD" }; // When Origin is not Country, UNLOCO or Zone, it will be ignored

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Destination_ISO3166CountryCode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var usPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true).PK;
			var uslaxPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true).PK;

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "UK", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { usPK, uslaxPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Destination_UNLOCODE()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var uslaxPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true).PK;

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "US", "UK", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { uslaxPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Destination_Postcode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			var rateEntryWithDestinationPostCode = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			rateEntryWithDestinationPostCode.TI_CartageDeliveryAddressPostCode = "2070";

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { rateEntryWithDestinationPostCode.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.Postcode, Value = "2070" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Destination_Zone()
		{
			var zone = Helper.NewInternationalZone("USXX", null, "USLAX", "USLAH");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var zoneRateEntryPK = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USXX", removeLines: true).PK;
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAH", removeLines: true);
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { zoneRateEntryPK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.Zone, Value = "USXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Destinatio_Other()
		{
			var zone = Helper.NewInternationalZone("USXX", null, "USLAX", "USLAH");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var zoneRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USXX", removeLines: true);
			var unlocoRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			var countryRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "US", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { zoneRateEntry.PK, unlocoRateEntry.PK, countryRateEntry.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Destination = new Location() { Type = Location.Types.IATACity, Value = "LAX" }; // When Destination is not Country, UNLOCO or Zone, it will be ignored

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Via_ISO3166CountryCode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry1.TI_ViaLRC = "HK";

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry2.TI_ViaLRC = "HKHKG";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Via = new Location() { Type = Location.Types.Country, Value = "HK" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Via_UNLOCODE()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry1.TI_ViaLRC = "HK";

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry2.TI_ViaLRC = "HKHKG";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Via = new Location() { Type = Location.Types.UNLOCO, Value = "HKHKG" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Via_Zone()
		{
			var zone = Helper.NewInternationalZone("HKXX", null, "HKHKG", "HKGOM");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry1.TI_ViaLRC = "HKXX";

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry2.TI_ViaLRC = "HKHKG";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry3.TI_ViaLRC = "HKGOM";

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Via = new Location() { Type = Location.Types.Zone, Value = "HKXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Via_NullOrEmpty()
		{
			var zone = Helper.NewInternationalZone("HKXX", null, "HKHKG", "HKGOM");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry1.TI_ViaLRC = "HKXX";

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry2.TI_ViaLRC = "HKHKG";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entry3.TI_ViaLRC = "HKGOM";

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.Via = null;
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.Via = new Location();
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.Via = new Location() { Value = "" };
			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Locations_FirstLoad()
			=> AssertFilteredLocation(RateEntryLookups.LocationSourceOption.FirstLoad.Code, RateEntrySchema.TI_FirstLoadLRC);

		public void TestConvert_Locations_LastDischarge()
			=> AssertFilteredLocation(RateEntryLookups.LocationSourceOption.LastDischarge.Code, RateEntrySchema.TI_LastDischargeLRC);

		public void TestConvert_Locations_FirstRouteSetLoadPort()
			=> AssertFilteredLocation(RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code, RateEntrySchema.TI_FirstRouteSetLoadPortLRC);

		public void TestConvert_Locations_LastRouteSetDischargePort()
			=> AssertFilteredLocation(RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code, RateEntrySchema.TI_LastRouteSetDischargePortLRC);

		void AssertFilteredLocation(ZString relatedFieldCode, SchemaColumn column)
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var zone = Helper.NewInternationalZone("HKXX");

			var entryCountry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entryCountry[column] = "HK";

			var entryUnloco = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entryUnloco[column] = "HKHKG";

			var entryCity = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entryCity[column] = "HKG";

			var entryZone = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "US", removeLines: true);
			entryZone[column] = "HKXX";

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.Locations = new[] { new Location { Type = Location.Types.UNLOCO, Value = "HKHKG", RelatedField = relatedFieldCode } };
			AssertQueryConvert(rateQuery, new[] { entryUnloco.PK });

			rateQuery.Locations = new[] { new Location { Type = Location.Types.Country, Value = "HK", RelatedField = relatedFieldCode } };
			AssertQueryConvert(rateQuery, new[] { entryCountry.PK, entryUnloco.PK, entryCity.PK, entryZone.PK });

			rateQuery.Locations = new[] { new Location { Type = Location.Types.City, Value = "HKG", RelatedField = relatedFieldCode } };
			AssertQueryConvert(rateQuery, new[] { entryCity.PK });

			rateQuery.Locations = new[] { new Location { Type = Location.Types.Zone, Value = "HKXX", RelatedField = relatedFieldCode } };
			AssertQueryConvert(rateQuery, new[] { entryZone.PK });
		}

		public void TestConvert_TransportMode_ContainerMode()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var airEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", removeLines: true);
			var uldEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "ULD", "AUSYD", "USLAX", removeLines: true);
			var seaEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUSYD", "USLAX", removeLines: true);
			var fclEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "AUSYD", "USLAX", removeLines: true);
			var roadEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "ROA", "AUSYD", "USLAX", removeLines: true);
			var railEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "RAI", "AUSYD", "USLAX", removeLines: true);
			var allEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "USLAX", removeLines: true);
			var airLSEEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			var fclSEAEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", removeLines: true);
			var fclROAEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "ROA", "AUSYD", "USLAX", removeLines: true);
			var fclRAIEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "RAI", "AUSYD", "USLAX", removeLines: true);
			var fclBCNEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "BCN", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = string.Empty;

			var expectedRateEntries = new[] { airEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "LSE";

			expectedRateEntries = new[] { airLSEEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "ULD";
			expectedRateEntries = new[] { uldEntry.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = string.Empty;

			expectedRateEntries = new[] { seaEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			expectedRateEntries = new[] { fclEntry.PK, fclSEAEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "ALL";
			rateQuery.ContainerMode = string.Empty;

			expectedRateEntries = new[] { allEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "ROA";
			rateQuery.ContainerMode = string.Empty;

			expectedRateEntries = new[] { roadEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "ROA";
			rateQuery.ContainerMode = "FCL";

			expectedRateEntries = new[] { fclROAEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "RAI";
			rateQuery.ContainerMode = string.Empty;

			expectedRateEntries = new[] { railEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.TransportMode = "RAI";
			rateQuery.ContainerMode = "FCL";

			expectedRateEntries = new[] { fclRAIEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			// We do not support AIR-FCL combination
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "FCL";

			expectedRateEntries = Array.Empty<ZGuid>();
			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Commodity()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var stdEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", commodity: "STD", removeLines: true);
			var xyzEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", commodity: "XYZ", removeLines: true);
			var abcEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", commodity: "ABC", removeLines: true);
			var otherEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			otherEntry.TI_RH_NKCommodityCode = string.Empty;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "STD" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "XYZ" },
				new CommodityInfo() { Type = CommodityInfo.Types.UniversalCommodityGroup, Value = "ZZZ" },
			};

			var expectedRateEntries = new[] { stdEntry.PK, xyzEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "abc" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "" }
			};

			expectedRateEntries = new[] { abcEntry.PK, otherEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "abc" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = null }
			};

			expectedRateEntries = new[] { abcEntry.PK, otherEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = null }
			};

			expectedRateEntries = new[] { otherEntry.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_EffectiveDate()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_RateStartDate = new ZDate(2013, 1, 1);
			entry1.TI_RateEndDate = ZDate.Empty;

			var entry2 = rate.AddRateEntry("AIR", "LSE", "AUBNE", "SGSIN", removeLines: true);
			entry2.TI_RateStartDate = new ZDate(2012, 1, 1);
			entry2.TI_RateEndDate = new ZDate(2013, 12, 31);

			var entry3 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "GB", removeLines: true);
			entry3.TI_RateStartDate = new ZDate(2012, 1, 1);
			entry3.TI_RateEndDate = new ZDate(2012, 9, 22);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.EffectiveDate = new DateTimeOffset(2012, 10, 23, 0, 0, 0, TimeSpan.Zero);

			var expectedRateEntries = new[] { entry2.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Client()
		{
			var org1 = Helper.NewOrgHeader("ORG1");
			var org2 = Helper.NewOrgHeader("ORG2");

			var entry1 = AddClientRateEntry(org1);
			var entry2 = AddClientRateEntry(org2);
			var entry3 = AddCostingRateEntry(org1);
			var entry4 = AddCostingRateEntry(org2);

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.Client = "ORG1";
			AssertQueryConvert(rateQuery, new[] { entry1.PK }, sourceEndpoint: SourceEndpoint.ClientRates);
			AssertQueryConvert(rateQuery, Array.Empty<ZGuid>(), sourceEndpoint: SourceEndpoint.Costing);

			rateQuery.Client = "ORG2";
			AssertQueryConvert(rateQuery, new[] { entry2.PK }, sourceEndpoint: SourceEndpoint.ClientRates);
			AssertQueryConvert(rateQuery, Array.Empty<ZGuid>(), sourceEndpoint: SourceEndpoint.Costing);

			rateQuery.Client = "";
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.ClientRates);
			AssertQueryConvert(rateQuery, new[] { entry3.PK, entry4.PK }, sourceEndpoint: SourceEndpoint.Costing);

			RateEntry AddClientRateEntry(OrgHeader org)
			{
				var rate = Helper.NewClientRate(org);
				return rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			}

			RateEntry AddCostingRateEntry(OrgHeader org)
			{
				var rate = Helper.NewCosting(org);
				return rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			}
		}

		public void TestConvert_CarrierContractNumber()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_ContractNumber = "ABC";

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_ContractNumber = "XYZ";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.CarrierContracts = new[] { "ABC", "XYZ" };

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierContracts = new[] { "" };
			expectedRateEntries = new[] { entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierContracts = new string[] { null };
			expectedRateEntries = new[] { entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierContracts = new string[] { "ABC", null };
			expectedRateEntries = new[] { entry1.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_CarrierContractNumber_OtherThanCosting()
		{
			var rate = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_ContractNumber = "ABC"; // Intercompany Tariff's rate entry doesn't accept contract number, and seting it here is just for test purposes. 

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_ContractNumber = "XYZ";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.CarrierContracts = new[] { "ABC", "XYZ" };

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_ClientContractNumber()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_ContractNumber = "ABC";

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_ContractNumber = "XYZ";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ClientContracts = new[] { "ABC", "XYZ" };

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			rateQuery.ClientContracts = new[] { "" };
			expectedRateEntries = new[] { entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			rateQuery.ClientContracts = new string[] { null };
			expectedRateEntries = new[] { entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			rateQuery.ClientContracts = new string[] { "ABC", null };
			expectedRateEntries = new[] { entry1.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			rateQuery.ClientContracts = new string[] { "", null };
			expectedRateEntries = new[] { entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);
		}

		public void TestConvert_ClientContractNumber_OtherThanClientRate()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_ContractNumber = "ABC"; // Costing rate entry accepts contract number, but it's used as Carrier Contract Number. 

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_ContractNumber = "XYZ";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ClientContracts = new[] { "PQR", "XYZ" };

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.Costing);
		}

		public void TestConvert_CarrierServiceLevel()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry1.TI_PL_NKCarrierServiceLevel = "ABC";
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry2.TI_PL_NKCarrierServiceLevel = "XYZ";
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry3.TI_PL_NKCarrierServiceLevel = "OPT";
			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry4.TI_PL_NKCarrierServiceLevel = "";

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "XYZ" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "OPT" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "ZZZ" },
			};

			var expectedRateEntries = new[] { entry2.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "XYZ" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "" },
			};

			expectedRateEntries = new[] { entry2.PK, entry4.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "XYZ" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = null },
			};

			expectedRateEntries = new[] { entry2.PK, entry4.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = null },
			};

			expectedRateEntries = new[] { entry4.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ServiceLevel()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry1.TI_RS_NKServiceLevel_NI = "ABC";
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry2.TI_RS_NKServiceLevel_NI = "XYZ";
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry3.TI_RS_NKServiceLevel_NI = "OPT";
			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry4.TI_RS_NKServiceLevel_NI = string.Empty;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.ServiceLevels = new[] { "XYZ", "OPT" };
			var expectedRateEntries = new[] { entry2.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.ServiceLevels = new string[] { null };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.ServiceLevels = new[] { string.Empty };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.ServiceLevels = new[] { "OPT", string.Empty };
			expectedRateEntries = new[] { entry3.PK, entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_GatewayServiceLevel()
		{
			Helper.ChargeCodes.CreateGlobalCharge("TTT");

			var rate = Helper.NewIntercompanyTariff();

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry1.TI_RS_NKGatewayServiceLevel = "ABC";
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry2.TI_RS_NKGatewayServiceLevel = "XYZ";
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry3.TI_RS_NKGatewayServiceLevel = "OPT";
			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry4.TI_RS_NKGatewayServiceLevel = string.Empty;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.GatewayServiceLevels = new[] { "XYZ", "OPT" };
			var expectedRateEntries = new[] { entry2.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.GatewayServiceLevels = new string[] { null };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.GatewayServiceLevels = new[] { string.Empty };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.GatewayServiceLevels = new[] { "OPT", string.Empty };
			expectedRateEntries = new[] { entry3.PK, entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_ShipmentGatewayServiceLevel()
		{
			Helper.ChargeCodes.CreateGlobalCharge("TTT");

			var rate = Helper.NewIntercompanyTariff();

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry1.TI_RS_NKShipmentGatewayServiceLevel = "ABC";
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry2.TI_RS_NKShipmentGatewayServiceLevel = "XYZ";
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry3.TI_RS_NKShipmentGatewayServiceLevel = "OPT";
			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", removeLines: true);
			entry4.TI_RS_NKShipmentGatewayServiceLevel = string.Empty;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			rateQuery.ShipmentGatewayServiceLevels = new[] { "XYZ", "OPT" };
			var expectedRateEntries = new[] { entry2.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.ShipmentGatewayServiceLevels = new string[] { null };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.ShipmentGatewayServiceLevels = new[] { string.Empty };
			expectedRateEntries = new[] { entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.ShipmentGatewayServiceLevels = new[] { "OPT", string.Empty };
			expectedRateEntries = new[] { entry3.PK, entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_ContainerType_CWCode()
		{
			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "40GP", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.ContainerTypes = new[] {
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "ISO0" },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = null },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "" },
				null
			};

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ContainerType_ISOType()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "40GP", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.ContainerTypes = new[] {
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "22G0" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = null },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "" },
				null,
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "40HC" },
			};

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ContainerType_ContainerTypesCanNotBeResolved()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "40GP", removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.ContainerTypes = new[] {
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "GP2X" }, // No Such Code in CW1
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "40GX" }, // No Such ISO in CW1
			};

			var expectedRateEntries = Array.Empty<ZGuid>();

			var expectedLogs = new string[]
			{
				"Provided Container Type with Code 'GP2X' does not exists in CargoWise.",
				"Provided Container Type with ISO Type '40GX' does not exists in CargoWise."
			};

			AssertQueryConvert(rateQuery, expectedRateEntries, expectedLogs);

			rateQuery.ContainerTypes = Array.Empty<ContainerType>();
			AssertQueryConvert(rateQuery, new ZGuid[] { entry1.PK, entry2.PK }, Array.Empty<string>());
		}

		public void TestConvert_ServiceProvider_CWCode_Costing()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var entry3 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry3.TI_OH_TransportProvider = provider1.PK;

			var standardRate = Helper.NewCosting(null);
			var standardEntry1 = standardRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			standardEntry1.TI_OH_TransportProvider = provider1.PK;

			var standardEntry2 = standardRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			standardEntry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK, standardEntry1.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ServiceProvider_CWCode_CompanyTariff()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";
			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			var entry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			entry1.TI_OH_Supplier = provider1.PK;
			var entry2 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry2.TI_OH_Supplier = provider2.PK;
			companyTariff.Factory.Save();

			var companyTariffLevel2 = Helper.NewCompanyTariff();
			companyTariffLevel2.TH_GC = Env.CurrentCompanyPK;
			companyTariffLevel2.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			var entry3 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "UK", "USLAX", removeLines: true);
			entry3.TI_OH_Supplier = provider1.PK;
			var entry4 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "CN", "USLAX", removeLines: true);
			entry4.TI_OH_Supplier = provider2.PK;
			companyTariffLevel2.Factory.Save();

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		public void TestConvert_ServiceProvider_CWCode_IntercompanyTariff()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewIntercompanyTariff(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewIntercompanyTariff(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var entry3 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry3.TI_OH_TransportProvider = provider1.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_ServiceProvider_CWCode_ClientRate()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_Supplier = provider1.PK;

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_Supplier = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);
		}

		public void TestConvert_ServiceProvider_SCAC()
		{
			var provider1 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "SCC1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "SCC2";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			provider2.OH_RSL_ShippingLine = shippingLine2.PK;

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { SCAC = "SCC1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ServiceProvider_C1CCode()
		{
			var provider1 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "STD1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "STD2";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			provider2.OH_RSL_ShippingLine = shippingLine2.PK;

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { C1CCode = "C1C1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ServiceProvider_IATACode()
		{
			var airLine1 = Factory.NewWithValidTestData<RefAirline>();
			airLine1.RM_TwoCharacterCode = "A1";
			airLine1.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";

			var airLine2 = Factory.NewWithValidTestData<RefAirline>();
			airLine2.RM_TwoCharacterCode = "A2";
			airLine2.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA2";

			var provider1 = Helper.NewOrgHeader();
			provider1.MiscServ.OM_RM_Airline = airLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			provider2.MiscServ.OM_RM_Airline = airLine2.PK;

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] { new Organisation() { IATACode = "A2" } };

			var expectedRateEntries = new[] { entry2.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_ServiceProvider_ServiceProvidersCanNotBeResolved()
		{
			var airLine = Factory.NewWithValidTestData<RefAirline>();
			airLine.RM_TwoCharacterCode = "AA";
			airLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "AAA";

			var provider1 = Helper.NewOrgHeader();
			provider1.MiscServ.OM_RM_Airline = airLine.PK;

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "PCW";

			var provider3 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "SCC1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider4 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "SCC2";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			provider2.OH_RSL_ShippingLine = shippingLine2.PK;

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate3 = Helper.NewCosting(provider3);
			var entry3 = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate4 = Helper.NewCosting(provider4);
			var entry4 = rate4.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.ServiceProviders = new[] {
				new Organisation() { IATACode = "AX" },
				new Organisation() { C1CCode = "C1CX" },
				new Organisation() { SCAC = "SCCX" },
				new Organisation() { CWCode = "XXX" },
				new Organisation() { },
			};

			var expectedRateEntries = Array.Empty<ZGuid>();

			var expectedLogs = new string[]
			{
				"Provided Service Provider with CWCode 'XXX' does not exists in CargoWise.",
				"Provided Service Provider with IATA Code 'AX' does not exists in CargoWise.",
				"Provided Service Provider with SCAC Code 'SCCX' does not exists in CargoWise.",
				"Provided Service Provider with C1C Code 'C1CX' does not exists in CargoWise."
			};

			AssertQueryConvert(rateQuery, expectedRateEntries, expectedLogs); // Query Should Return No Results
		}

		public void TestConvert_Carriers_CWCode_Costing()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_TransportProvider = provider2.PK;
			var entry3 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry3.TI_OH_TransportProvider = provider1.PK;

			var standardRate = Helper.NewCosting(null);
			var standardEntry1 = standardRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			standardEntry1.TI_OH_TransportProvider = provider1.PK;

			var standardEntry2 = standardRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			standardEntry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK, standardEntry1.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Carriers_CWCode_CompanyTariff()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";
			Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			var entry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			entry1.TI_OH_Supplier = provider1.PK;
			entry1.TI_OH_TransportProvider = provider1.PK;
			var entry2 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry2.TI_OH_Supplier = provider2.PK;
			entry2.TI_OH_TransportProvider = provider2.PK;
			companyTariff.Factory.Save();

			var companyTariffLevel2 = Helper.NewCompanyTariff();
			companyTariffLevel2.TH_GC = Env.CurrentCompanyPK;
			companyTariffLevel2.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			var entry3 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "UK", "USLAX", removeLines: true);
			entry3.TI_OH_Supplier = provider1.PK;
			entry3.TI_OH_TransportProvider = provider1.PK;
			var entry4 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "CN", "USLAX", removeLines: true);
			entry4.TI_OH_Supplier = provider2.PK;
			entry4.TI_OH_TransportProvider = provider2.PK;
			companyTariffLevel2.Factory.Save();

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		public void TestConvert_Carriers_CWCode_IntercompanyTariff()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewIntercompanyTariff(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewIntercompanyTariff(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_Carriers_CWCode_ClientRate()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_Supplier = provider1.PK;
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_Supplier = provider2.PK;
			entry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { CWCode = "P1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);
		}

		public void TestConvert_Carriers_SCAC()
		{
			var provider1 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "SCC1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "SCC2";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			provider2.OH_RSL_ShippingLine = shippingLine2.PK;

			var rate1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { SCAC = "SCC1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Carriers_C1CCode()
		{
			var provider1 = Helper.NewOrgHeader();
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "STD1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "STD2";
			shippingLine2.RSL_CargoWiseOneCode = "C1C2";
			provider2.OH_RSL_ShippingLine = shippingLine2.PK;

			var rate1 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { C1CCode = "C1C1" } };

			var expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_Carriers_IATACode()
		{
			var airLine1 = Factory.NewWithValidTestData<RefAirline>();
			airLine1.RM_TwoCharacterCode = "A1";
			airLine1.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";

			var airLine2 = Factory.NewWithValidTestData<RefAirline>();
			airLine2.RM_TwoCharacterCode = "A2";
			airLine2.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA2";

			var provider1 = Helper.NewOrgHeader();
			provider1.MiscServ.OM_RM_Airline = airLine1.PK;

			var provider2 = Helper.NewOrgHeader();
			provider2.MiscServ.OM_RM_Airline = airLine2.PK;

			var rate1 = Helper.NewCosting(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_TransportProvider = provider1.PK;

			var rate2 = Helper.NewCosting(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.Carriers = new[] { new Organisation { IATACode = "A2" } };

			var expectedRateEntries = new[] { entry2.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_NamedAccount()
		{
			var orgHeader1 = Helper.NewOrgHeader();
			orgHeader1.OH_Code = "OH1";

			var orgHeader2 = Helper.NewOrgHeader();
			orgHeader2.OH_Code = "OH2";

			var orgHeader3 = Helper.NewOrgHeader();
			orgHeader3.OH_Code = "OH3";

			var rate = Helper.NewCosting(null);
			var entry0 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true); // no Consignor, Consignee, Controlling Customer or NAC

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_Consignor = orgHeader1.PK;

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_Consignee = orgHeader1.PK;

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry3.TI_OH_ControllingCustomer = orgHeader1.PK;

			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry4.TI_OH_ControllingCustomer = orgHeader2.PK;

			var entry5 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry5.TI_OH_ControllingCustomer = orgHeader3.PK;

			var entry6 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry6.TI_OH_Consignor = orgHeader3.PK;

			var entry7 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry7.TI_OH_Consignee = orgHeader3.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.NamedAccounts = new[]
			{
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "OH1" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "OH2" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "007" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC007" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "OH3" }, // When type is NamedAccount, we will ignore it for CW1 rates. 
			};

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK, entry4.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_NamedAccount_NamedAccountsCantNotBeResolved()
		{
			var orgHeader1 = Helper.NewOrgHeader();
			orgHeader1.OH_Code = "OH1";

			var orgHeader2 = Helper.NewOrgHeader();
			orgHeader2.OH_Code = "OH2";

			var orgHeader3 = Helper.NewOrgHeader();
			orgHeader3.OH_Code = "OH3";

			var rate = Helper.NewCosting(null);
			var entry0 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true); // no Consignor, Consignee, Controlling Customer or NAC

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_Consignor = orgHeader1.PK;

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_Consignee = orgHeader1.PK;

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry3.TI_OH_ControllingCustomer = orgHeader1.PK;

			var entry4 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry4.TI_OH_ControllingCustomer = orgHeader2.PK;

			var entry5 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry5.TI_OH_ControllingCustomer = orgHeader3.PK;

			var entry6 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry6.TI_OH_Consignor = orgHeader3.PK;

			var entry7 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry7.TI_OH_Consignee = orgHeader3.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.NamedAccounts = new[]
			{
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "OHX" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "OHY" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "" },
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "OHZ" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC007" }, // When type is NamedAccount, we will ignore it for CW1 rates.
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "OH3" },
			};

			var expectedRateEntries = Array.Empty<ZGuid>();

			var expectedLogs = new string[]
			{
				"Provided NamedAccount organization with Code 'OHX' does not exists in CargoWise.",
				"Provided NamedAccount organization with Code 'OHY' does not exists in CargoWise.",
				"Provided NamedAccount organization with Code 'OHZ' does not exists in CargoWise."
			};

			AssertQueryConvert(rateQuery, expectedRateEntries, expectedLogs);
		}

		public void TestConvert_Costing_FilterCombination()
		{
			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "P1";

			var provider2 = Helper.NewOrgHeader();
			provider2.OH_Code = "P2";

			var rate1 = Helper.NewCosting(provider1);

			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			entry1.TI_ContractNumber = "ABC";

			var entry2 = rate1.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			entry2.TI_ContractNumber = "XYZ";

			var entry3 = rate1.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "40GP", removeLines: true);

			var rate2 = Helper.NewCosting(provider2);
			var rate2entry1 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var rate2entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			rate2entry2.TI_ContractNumber = "ABC";

			var standardRate = Helper.NewCosting(null);
			var standardEntry1 = standardRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			standardEntry1.TI_ContractNumber = "ABC";
			standardEntry1.TI_OH_TransportProvider = provider1.PK;

			var standardEntry2 = standardRate.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AU", "NZ", "", "20GP", removeLines: true);
			standardEntry2.TI_ContractNumber = "ABC";
			standardEntry2.TI_OH_TransportProvider = provider2.PK;

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";
			rateQuery.ServiceProviders = new[] { new Organisation() { CWCode = "P1" } };
			rateQuery.ContainerTypes = new[] { new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" } };
			rateQuery.CarrierContracts = new[] { "ABC" };

			var expectedRateEntries = new[] { entry1.PK, standardEntry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);

			rateQuery.ServiceProviders = Array.Empty<Organisation>();
			expectedRateEntries = new[] { entry1.PK, standardEntry1.PK, standardEntry2.PK, rate2entry2.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_PlannedLoad_EmptyValue()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.PlannedLoad = new Location() { Type = "", Value = "" };
			AssertQueryConvert(rateQuery, new[] { entry1.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.PlannedLoad = new Location() { };
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.PlannedLoad = null;
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedLoad_ISO3166CountryCode()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "AU";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.Country, Value = "AU" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedLoad_UNLOCODE()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "AU";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedLoad_Zone()
		{
			var zone = Helper.NewInternationalZone("AUXX", null, "AUSYD", "AUMEL");

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "AUXX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.Zone, Value = "AUXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedLoad_Other()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "SYD";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK }; // When Planned Load is not Country, UNLOCO or Zone, it will be ignored

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.IATACity, Value = "SYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedLoad_Costing()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedLoadLRC = "AU"; // Costing's rate entry doesn't accept Planned Load, and seting it here is just for test purposes.

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_PlannedLoadLRC = "AUSYD";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_PlannedDischarge_EmptyValue()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "AUSYD";

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.PlannedDischarge = new Location() { Type = "", Value = "" };
			AssertQueryConvert(rateQuery, new[] { entry1.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.PlannedDischarge = new Location() { };
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.PlannedDischarge = null;
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedDischarge_ISO3166CountryCode()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedDischarge_UNLOCODE()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedDischarge_Zone()
		{
			var zone = Helper.NewInternationalZone("USXX", null, "USLAX", "USKDD");

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "USXX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.Zone, Value = "USXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedDischarge_Other()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "LAX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK }; // When Planned Discharge is not Country, UNLOCO or Zone, it will be ignored

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.IATACity, Value = "SYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_PlannedDischarge_Costing()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_PlannedDischargeLRC = "US"; // Costing's rate entry doesn't accept Planned Discharge, and seting it here is just for test purposes.

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_PlannedDischargeLRC = "USLAX";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_RateOrigin_EmptyValue()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "AUSYD";

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.RateOrigin = new Location() { Type = "", Value = "" };
			AssertQueryConvert(rateQuery, new[] { entry1.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.RateOrigin = new Location() { };
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.RateOrigin = null;
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateOrigin_ISO3166CountryCode()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateOrigin_UNLOCODE()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateOrigin_Zone()
		{
			var zone = Helper.NewInternationalZone("USXX", null, "USLAX", "USKDD");

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "USXX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { Type = Location.Types.Zone, Value = "USXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateOrigin_Other()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "LAX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK }; // When Rate Origin is not Country, UNLOCO or Zone, it will be ignored

			var rateQuery = GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { Type = Location.Types.IATACity, Value = "SYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateOrigin_Costing()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateOrigin = "US"; // Costing's rate entry doesn't accept Rate Origin, and seting it here is just for test purposes.

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_RateOrigin = "USLAX";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_RateDestination_EmptyValue()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "AUSYD";

			Factory.Save();

			var rateQuery = GetValidRateQuery();

			rateQuery.RateDestination = new Location() { Type = "", Value = "" };
			AssertQueryConvert(rateQuery, new[] { entry1.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.RateDestination = new Location() { };
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			rateQuery.RateDestination = null;
			AssertQueryConvert(rateQuery, new[] { entry1.PK, entry2.PK }, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateDestination_ISO3166CountryCode()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateDestination = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateDestination_UNLOCODE()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "US";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry2.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateDestination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateDestination_Zone()
		{
			var zone = Helper.NewInternationalZone("USXX", null, "USLAX", "USKDD");

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "USXX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateDestination = new Location() { Type = Location.Types.Zone, Value = "USXX" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateDestination_Other()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "LAX";

			var entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "USLAX";

			var entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK }; // When Rate Destination is not Country, UNLOCO or Zone, it will be ignored

			var rateQuery = GetValidRateQuery();
			rateQuery.RateDestination = new Location() { Type = Location.Types.IATACity, Value = "SYD" };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);
		}

		public void TestConvert_RateDestination_Costing()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AU", "USLAX", removeLines: true);
			entry1.TI_RateDestination = "US"; // Costing's rate entry doesn't accept Rate Destination, and seting it here is just for test purposes.

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_RateDestination = "USLAX";

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);

			Factory.Save();

			var expectedRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK };

			var rateQuery = GetValidRateQuery();
			rateQuery.RateDestination = new Location() { Type = Location.Types.Country, Value = "US" };

			AssertQueryConvert(rateQuery, expectedRateEntries);
		}

		public void TestConvert_CTLevel()
		{
			var companyTariffBase = Helper.NewCompanyTariff();
			companyTariffBase.TH_GlobalRateLevel = 1;

			var entry1 = companyTariffBase.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			var entry2 = companyTariffBase.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			var entry3 = companyTariffBase.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "UK", "USLAX", removeLines: true);
			var entry4 = companyTariffBase.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "CN", "USLAX", removeLines: true);
			companyTariffBase.Factory.Save();

			var companyTariffLevel2 = Helper.NewCompanyTariff();
			companyTariffLevel2.TH_GlobalRateLevel = 2;

			var entry5 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			var entry6 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			var entry7 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "UK", "USLAX", removeLines: true);
			var entry8 = companyTariffLevel2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "CN", "USLAX", removeLines: true);
			companyTariffLevel2.Factory.Save();

			Factory.Save();

			var expectedCompanyTariffBaseRateEntries = new[] { entry1.PK, entry2.PK, entry3.PK, entry4.PK };
			var rateQueryLevelBase = GetValidRateQuery();
			rateQueryLevelBase.CTLevel = 1;
			AssertQueryConvert(rateQueryLevelBase, expectedCompanyTariffBaseRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);

			var expectedCompanyTariffLevel2RateEntries = new[] { entry5.PK, entry6.PK, entry7.PK, entry8.PK };
			var rateQueryLevel2 = GetValidRateQuery();
			rateQueryLevel2.CTLevel = 2;
			AssertQueryConvert(rateQueryLevel2, expectedCompanyTariffLevel2RateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		public void TestConvert_PaymentTermOverride()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_PaymentTerm = "PPD";

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_PaymentTerm = "CCX";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry3.TI_PaymentTerm = "";

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.PaymentTermOverride = "PPD";

			var expectedRateEntries = new[] { entry1.PK, entry3.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			entry1 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry1.TI_PaymentTerm = "CCX";

			entry2 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry2.TI_PaymentTerm = "PPD";

			entry3 = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "UK", "USLAX", removeLines: true);
			entry3.TI_PaymentTerm = "";

			Factory.Save();

			expectedRateEntries = new[] { entry2.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.IntercompanyTariffs);

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			entry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			entry1.TI_PaymentTerm = "PPD";

			entry2 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry2.TI_PaymentTerm = "CCX";

			entry3 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry3.TI_PaymentTerm = "";

			companyTariff.Factory.Save();

			expectedRateEntries = new[] { entry1.PK, entry3.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		public void TestConvert_HBLDeliveryMode()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_HBLDeliveryMode = "DOOR/DOOR";

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_HBLDeliveryMode = "CFS/CFS";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry3.TI_HBLDeliveryMode = "";

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.HBLDeliveryMode = "DOOR/DOOR";

			var expectedRateEntries = new[] { entry1.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			entry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			entry1.TI_HBLDeliveryMode = "DOOR/DOOR";

			entry2 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry2.TI_HBLDeliveryMode = "CFS/CFS";

			entry3 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry3.TI_HBLDeliveryMode = "";

			companyTariff.Factory.Save();

			expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		public void TestConvert_FMCTariffID()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry1.TI_FMCTariffID = "1234";

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry2.TI_FMCTariffID = "1111";

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "LSE", "AUSYD", "USLAX", removeLines: true);
			entry3.TI_FMCTariffID = "";

			Factory.Save();

			var rateQuery = GetValidRateQuery();
			rateQuery.FMCTariffID = "1234";

			var expectedRateEntries = new[] { entry1.PK };
			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.ClientRates);

			var companyTariff = Helper.NewCompanyTariff();
			companyTariff.TH_GC = Env.CurrentCompanyPK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			entry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "USLAX", removeLines: true);
			entry1.TI_FMCTariffID = "1234";

			entry2 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry2.TI_FMCTariffID = "1111";

			entry3 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", removeLines: true);
			entry3.TI_FMCTariffID = "";

			companyTariff.Factory.Save();

			expectedRateEntries = new[] { entry1.PK };

			AssertQueryConvert(rateQuery, expectedRateEntries, sourceEndpoint: SourceEndpoint.CompanyTariffs);
		}

		void AssertQueryConvert(RateQuery query, ZGuid[] expectedEntryPKs, IEnumerable<string> expectedLogs = null, ZGuid companyPK = default, SourceEndpoint sourceEndpoint = SourceEndpoint.Costing)
		{
			var companyPKToUse = companyPK != default ? companyPK : Env.CurrentCompanyPK;
			var logger = new ElementaryLogger();
			var converter = new RateQueryToZQueryConverter(Factory, logger);
			var rates = new SimpleRateEntryCollection(Factory);
			var zQuery = converter.Convert(companyPKToUse, query, sourceEndpoint);
			rates.Load(zQuery);

			AssertContainsExactElementsInAnyOrder(expectedEntryPKs.ToArray(), rates.GetPKs().ToArray());

			if (expectedLogs != null)
			{
				AssertContainsExactElementsInAnyOrder(expectedLogs.ToArray(), logger.GetAllLogs().ToArray());
			}
		}

		RateQuery GetValidRateQuery()
		{
			var rateQuery = new RateQuery();
			rateQuery.RateProviders = new[] { RatesAPIsConstants.RateProviders.CargoWise };
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "LSE";

			return rateQuery;
		}
	}
}
