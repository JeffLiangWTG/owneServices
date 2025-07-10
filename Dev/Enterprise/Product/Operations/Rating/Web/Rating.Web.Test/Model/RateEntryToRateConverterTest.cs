using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using CustomField = WiseRates.Api.Model.CustomField;
using RefServiceLevel = Enterprise.MasterFiles.Business.RefServiceLevel;
using WiseRate = WiseRates.Api.Model.Rate;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateEntryToRateConverterTest : RatingTestCase
	{
		public void TestConvert_TransportMode_ContainerMode()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			void AssertTransportModeAndContainerMode(string rateCategory, string rateMode, string expectedTransportMode, string expectedContainerMode)
			{
				var entry = rate.AddRateEntry(rateCategory, rateMode, "AUSYD", "USLAX", removeLines: true);
				var convertedRate = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

				AssertEquals(expectedTransportMode, convertedRate.TransportMode);
				AssertEquals(expectedContainerMode, convertedRate.ContainerMode);
			}

			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "AIR", "AIR", "");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "LSE", "AIR", "LSE");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "ULD", "AIR", "ULD");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "SEA", "SEA", "");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "FCL", "SEA", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "LCL", "SEA", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "ROA", "ROA", "");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "FRO", "ROA", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "LRO", "ROA", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "RAI", "RAI", "");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "FRA", "RAI", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "LRA", "RAI", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "ALL", "ALL", "");

			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "FTL", "ROA", "FTL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "FWL", "RAI", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "COU", "COU", "OBC");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.ORG, "ABC", "OTH", "OTH");

			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.FCL, "SEA", "SEA", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.FCL, "ROA", "ROA", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.FCL, "RAI", "RAI", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.FCL, "AIR", "AIR", "FCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.FCL, "ALL", "ALL", "FCL");

			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.LCL, "SEA", "SEA", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.LCL, "AIR", "AIR", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.LCL, "ALL", "ALL", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.LCL, "ROA", "ROA", "LCL");
			AssertTransportModeAndContainerMode(RatingConstants.RateCategory.LCL, "RAI", "RAI", "LCL");
		}

		public void TestConvert_Locations()
		{
			var auZone = Factory.NewWithValidTestData<RefZoneHeader>();
			auZone.FZ_Code = "AURZ";
			auZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			auZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			auZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			var usZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usZone.FZ_Code = "USRZ";
			usZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			usZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			usZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates)));

			Factory.Save();

			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var unlocoEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", removeLines: true);
			unlocoEntry.TI_RateOrigin = "AUMEL";
			unlocoEntry.TI_RateDestination = "USLGA";

			unlocoEntry.TI_PlannedLoadLRC = "AUABX";
			unlocoEntry.TI_PlannedDischargeLRC = "AUALD";

			unlocoEntry.TI_ViaLRC = "AUALX";

			var convertedUNLOCO = converter.Convert(new[] { unlocoEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type of Origin for UNLOCO locations should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.Origin.Type);
			AssertEquals("Value of Origin for UNLOCO locations should be converted correctly", "AUSYD", convertedUNLOCO.Origin.Value);

			AssertEquals("Type of Destination for UNLOCO locations should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.Destination.Type);
			AssertEquals("Value of Destination for UNLOCO locations should be converted correctly", "USLAX", convertedUNLOCO.Destination.Value);

			AssertEquals("Type of Rate Origin should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.RateOrigin.Type);
			AssertEquals("Value of Rate Origin should be converted correctly", "AUMEL", convertedUNLOCO.RateOrigin.Value);

			AssertEquals("Type of Rate Destination should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.RateDestination.Type);
			AssertEquals("Value of Rate Origin should be converted correctly", "USLGA", convertedUNLOCO.RateDestination.Value);

			AssertEquals("Type of Planned Load  should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.PlannedLoad.Type);
			AssertEquals("Value of Planned Load  should be converted correctly", "AUABX", convertedUNLOCO.PlannedLoad.Value);

			AssertEquals("Type of Planned Discharge should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.PlannedDischarge.Type);
			AssertEquals("Value of Planned Discharge should be converted correctly", "AUALD", convertedUNLOCO.PlannedDischarge.Value);

			AssertEquals("Type of Via should be converted correctly", Location.Types.UNLOCO, convertedUNLOCO.Via.Type);
			AssertEquals("Value of Via should be converted correctly", "AUALX", convertedUNLOCO.Via.Value);

			var cityEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "SYD", "LAX", removeLines: true);
			var convertedCity = converter.Convert(new[] { cityEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type of Origin for City locations should be converted correctly", Location.Types.City, convertedCity.Origin.Type);
			AssertEquals("Value of Origin for City locations should be converted correctly", "SYD", convertedCity.Origin.Value);

			AssertEquals("Type of Destination for City locations should be converted correctly", Location.Types.City, convertedCity.Destination.Type);
			AssertEquals("Value of Destination for City locations should be converted correctly", "LAX", convertedCity.Destination.Value);

			var countryEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AU", "US", removeLines: true);
			var convertedCountry = converter.Convert(new[] { countryEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type of Origin for Country locations should be converted correctly", Location.Types.Country, convertedCountry.Origin.Type);
			AssertEquals("Value of Origin for Country locations should be converted correctly", "AU", convertedCountry.Origin.Value);

			AssertEquals("Type of Destination for Country locations should be converted correctly", Location.Types.Country, convertedCountry.Destination.Type);
			AssertEquals("Value of Destination for Country locations should be converted correctly", "US", convertedCountry.Destination.Value);

			var zoneEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AURZ", "USRZ", removeLines: true);
			var convertedZone = converter.Convert(new[] { zoneEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type of Origin for Zone locations should be converted correctly", Location.Types.Zone, convertedZone.Origin.Type);
			AssertEquals("Value of Origin for Zone locations should be converted correctly", "AURZ", convertedZone.Origin.Value);

			AssertEquals("Type of Destination for Zone locations should be converted correctly", Location.Types.Zone, convertedZone.Destination.Type);
			AssertEquals("Value of Destination for Zone locations should be converted correctly", "USRZ", convertedZone.Destination.Value);

			var emptyEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "", "", removeLines: true);
			var convertedEmpty = converter.Convert(new[] { emptyEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertNull("Empty Origin should be converted correctly", convertedEmpty.Origin);
			AssertNull("Empty Destination should be converted correctly", convertedEmpty.Destination);
		}

		public void TestConvert_Locations_FirstLoad()
			=> AssertLocationConverted(RateEntryLookups.LocationSourceOption.FirstLoad.Code, RateEntrySchema.TI_FirstLoadLRC);

		public void TestConvert_Locations_LastDischarge()
			=> AssertLocationConverted(RateEntryLookups.LocationSourceOption.LastDischarge.Code, RateEntrySchema.TI_LastDischargeLRC);

		public void TestConvert_Locations_FirstRouteSetLoadPort()
			=> AssertLocationConverted(RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code, RateEntrySchema.TI_FirstRouteSetLoadPortLRC);

		public void TestConvert_Locations_LastRouteSetDischargePort()
			=> AssertLocationConverted(RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code, RateEntrySchema.TI_LastRouteSetDischargePortLRC);

		void AssertLocationConverted(ZString code, SchemaColumn column)
		{
			var auZone = Factory.NewWithValidTestData<RefZoneHeader>();
			auZone.FZ_Code = "AURZ";
			auZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;
			auZone.FZ_ZoneMode = Core.Constants.RateMode.AIR;
			auZone.Countries.Add(Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia)));

			Factory.Save();

			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", removeLines: true);

			entry[column] = "AUSYD";

			var converted = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals("Locations.Length", 1, converted.Locations.Length);
			AssertEquals("Locations.RelatedField", code, converted.Locations[0].RelatedField);

			AssertEquals("Type for UNLOCO locations", Location.Types.UNLOCO, converted.Locations[0].Type);
			AssertEquals("Value for UNLOCO locations", "AUSYD", converted.Locations[0].Value);

			entry[column] = "AU";
			converted = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type for Country locations", Location.Types.Country, converted.Locations[0].Type);
			AssertEquals("Value for Country locations", "AU", converted.Locations[0].Value);

			entry[column] = "SYD";
			converted = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type for City locations", Location.Types.City, converted.Locations[0].Type);
			AssertEquals("Value for City locations", "SYD", converted.Locations[0].Value);

			entry[column] = "AURZ";
			converted = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("Type for Zone locations", Location.Types.Zone, converted.Locations[0].Type);
			AssertEquals("Value for Zone locations", "AURZ", converted.Locations[0].Value);
		}

		#region Convert ServiceProvider

		public void TestConvert_ServiceProvider_CW1Costing()
			=> AssertConvert_ServiceProvider_CW1Rates_ServiceProviderIsTH_OH((OrgHeader provider) => Helper.NewCosting(provider), RatingConstants.RatingHeaderTypes.Costing);

		public void TestConvert_ServiceProvider_CW1IntercompanyTariff()
			=> AssertConvert_ServiceProvider_CW1Rates_ServiceProviderIsTH_OH((OrgHeader provider) => Helper.NewIntercompanyTariff(provider), RatingConstants.RatingHeaderTypes.IntercompanyTariff);

		void AssertConvert_ServiceProvider_CW1Rates_ServiceProviderIsTH_OH(Func<OrgHeader, RatingHeader> getRatingHeader, string rateType)
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var airLine = Factory.NewWithValidTestData<RefAirline>();
			airLine.RM_TwoCharacterCode = "A1";
			airLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";
			provider1.MiscServ.OM_RM_Airline = airLine.PK;

			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "SCC1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var csl1 = provider1.MiscServ.CarrierServiceLevels.AddNew();
			csl1.PL_Code = "SL1";
			csl1.PL_CarrierServiceLevelDescription = "DESC1";
			var csl2 = provider1.MiscServ.CarrierServiceLevels.AddNew();
			csl2.PL_Code = "SL2";
			csl2.PL_CarrierServiceLevelDescription = "DESC2";

			var provider2 = Helper.NewOrgHeader();

			Factory.Save();

			var rate1 = getRatingHeader(provider1);
			var entry1 = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var convertedEntry = converter.Convert(new[] { entry1 }, rateType, logger).Single();

			AssertEquals("OHC", convertedEntry.ServiceProvider.CWCode);
			AssertEquals("A1", convertedEntry.ServiceProvider.IATACode);
			AssertEquals("SCC1", convertedEntry.ServiceProvider.SCAC);
			AssertEquals("C1C1", convertedEntry.ServiceProvider.C1CCode);

			var rate2 = getRatingHeader(provider2);
			var entry2 = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			convertedEntry = converter.Convert(new[] { entry2 }, rateType, logger).Single();

			AssertNotNull(convertedEntry.ServiceProvider);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.IATACode);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.SCAC);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.C1CCode);

			//For IntercompnayTariff, TH_OH is mandatory, so following case is not applicable
			if (rateType != RatingConstants.RatingHeaderTypes.IntercompanyTariff)
			{
				var rate3 = getRatingHeader(null);
				var entry3 = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

				convertedEntry = converter.Convert(new[] { entry3 }, rateType, logger).Single();

				AssertNull(convertedEntry.ServiceProvider);
			}
		}

		public void TestConvert_ServiceProvider_CW1ClientRate()
			=> AssertConvert_ServiceProvider_CW1Rates_ServiceProviderIsTI_OH_Supplier((OrgHeader orgHeader) => Helper.NewClientRate(orgHeader), RatingConstants.RatingHeaderTypes.ClientRate);

		void AssertConvert_ServiceProvider_CW1Rates_ServiceProviderIsTI_OH_Supplier(Func<OrgHeader, RatingHeader> getRatingHeader, string rateType)
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var airLine = Factory.NewWithValidTestData<RefAirline>();
			airLine.RM_TwoCharacterCode = "A1";
			airLine.RM_EagleAddedAirlinePrefixOrAccountingCode = "AA1";

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";
			provider1.MiscServ.OM_RM_Airline = airLine.PK;

			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "SCC1";
			shippingLine1.RSL_CargoWiseOneCode = "C1C1";
			provider1.OH_RSL_ShippingLine = shippingLine1.PK;

			var csl1 = provider1.MiscServ.CarrierServiceLevels.AddNew();
			csl1.PL_Code = "SL1";
			csl1.PL_CarrierServiceLevelDescription = "DESC1";
			var csl2 = provider1.MiscServ.CarrierServiceLevels.AddNew();
			csl2.PL_Code = "SL2";
			csl2.PL_CarrierServiceLevelDescription = "DESC2";

			var provider2 = Helper.NewOrgHeader();

			Factory.Save();

			var rate = getRatingHeader(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry1.TI_OH_Supplier = provider1.PK;

			var convertedEntry = converter.Convert(new[] { entry1 }, rateType, logger).Single();

			AssertEquals("OHC", convertedEntry.ServiceProvider.CWCode);
			AssertEquals("A1", convertedEntry.ServiceProvider.IATACode);
			AssertEquals("SCC1", convertedEntry.ServiceProvider.SCAC);
			AssertEquals("C1C1", convertedEntry.ServiceProvider.C1CCode);

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry2.TI_OH_Supplier = provider2.PK;

			convertedEntry = converter.Convert(new[] { entry2 }, rateType, logger).Single();

			AssertNotNull(convertedEntry.ServiceProvider);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.IATACode);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.SCAC);
			AssertNullOrEmpty(convertedEntry.ServiceProvider.C1CCode);

			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			convertedEntry = converter.Convert(new[] { entry3 }, rateType, logger).Single();

			AssertNull(convertedEntry.ServiceProvider);
		}

		#endregion

		public void TestConvert_ServiceProvider_WiseRates()
		{
			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.WiseCarrier = new RefCarrier() { C1Code = "C1C", IATACode = "IATA", SCACCode = "SCAC" };

			var wiseEntry = new WiseEntry(new WiseRate(), Factory);
			wiseEntry.ParentRatingHeader = wiseHeader;

			AssertEquals(ZGuid.Empty, wiseEntry.ServiceProviderPK());

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("C1C", convertedEntry.ServiceProvider.C1CCode);
			AssertEquals("IATA", convertedEntry.ServiceProvider.IATACode);
			AssertEquals("SCAC", convertedEntry.ServiceProvider.SCAC);

			wiseHeader.WiseCarrier = null;

			convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertNull(convertedEntry.ServiceProvider);
		}

		public void TestConvert_ControllingCustomer()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var controllingCustomer = Helper.NewOrgHeader();
			controllingCustomer.OH_Code = "OHC";

			Factory.Save();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals("OHC", convertedEntry.ControllingCustomer);

			entry.TI_OH_ControllingCustomer = ZGuid.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.ControllingCustomer);
		}

		public void TestConvert_Consignor()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var consignor = Helper.NewOrgHeader();
			consignor.OH_Code = "OHC";

			Factory.Save();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_OH_Consignor = consignor.PK;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals("OHC", convertedEntry.Consignor);

			entry.TI_OH_Consignor = ZGuid.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.Consignor);
		}

		public void TestConvert_Consignee()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var consignee = Helper.NewOrgHeader();
			consignee.OH_Code = "OHC";

			Factory.Save();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_OH_Consignee = consignee.PK;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals("OHC", convertedEntry.Consignee);

			entry.TI_OH_Consignee = ZGuid.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.Consignee);
		}

		public void TestConvert_CarrierContract()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_ContractNumber = "CNumber";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals("CNumber", convertedEntry.CarrierContract);

			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.ClientRate, logger).Single();
			AssertNullOrEmpty(convertedEntry.CarrierContract);
		}

		public void TestConvert_ClientContract()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewClientRate(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_ContractNumber = "CNumber";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.ClientRate, logger).Single();
			AssertEquals("CNumber", convertedEntry.ClientContract);

			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNullOrEmpty(convertedEntry.ClientContract);
		}

		public void TestConvert_CarrierServiceLevel_CWRates()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_PL_NKCarrierServiceLevel = "CSL";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals(CarrierServiceLevel.Types.CargoWise, convertedEntry.CarrierServiceLevel.Type);
			AssertEquals("CSL", convertedEntry.CarrierServiceLevel.Value);

			entry.TI_PL_NKCarrierServiceLevel = ZString.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.CarrierServiceLevel);
		}

		public void TestConvert_CarrierServiceLevel_WiseRates()
		{
			var wiseRate = new WiseRate()
			{
				ServiceLevel = "CSX",
			};

			var wiseEntry = new WiseEntry(wiseRate, Factory);
			wiseEntry.TI_PL_NKCarrierServiceLevel = string.Empty;

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("UC", convertedEntry.CarrierServiceLevel.Type);
			AssertEquals("CSX", convertedEntry.CarrierServiceLevel.Value);

			wiseEntry.TI_PL_NKCarrierServiceLevel = "CSL";

			convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("CW", convertedEntry.CarrierServiceLevel.Type);
			AssertEquals("CSL", convertedEntry.CarrierServiceLevel.Value);
		}

		public void TestConvert_ServiceLevel()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_RS_NKServiceLevel_NI = "SL";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("SL", convertedEntry.ServiceLevel);
		}

		public void TestConvert_GatewayServiceLevel()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var costing = Helper.NewCosting(null);
			var entryCosting = costing.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var convertedEntry = converter.Convert(new[] { entryCosting }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("", convertedEntry.GatewayServiceLevel);

			var ict = Helper.NewIntercompanyTariff(null);
			var entryIct = ict.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entryIct.TI_RS_NKGatewayServiceLevel = "DIR";

			convertedEntry = converter.Convert(new[] { entryIct }, RatingConstants.RatingHeaderTypes.IntercompanyTariff, logger).Single();

			AssertEquals("DIR", convertedEntry.GatewayServiceLevel);
		}

		public void TestConvert_ShipmentGatewayServiceLevel()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;

			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var costing = Helper.NewCosting(null);
			var entryCosting = costing.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var convertedEntry = converter.Convert(new[] { entryCosting }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals("", convertedEntry.ShipmentGatewayServiceLevel);

			var ict = Helper.NewIntercompanyTariff(null);
			var entryIct = ict.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entryIct.TI_RS_NKShipmentGatewayServiceLevel = "DIR";

			convertedEntry = converter.Convert(new[] { entryIct }, RatingConstants.RatingHeaderTypes.IntercompanyTariff, logger).Single();

			AssertEquals("DIR", convertedEntry.ShipmentGatewayServiceLevel);
		}

		public void TestConvert_ContainerType()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_RC = GP40.PK;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals(ContainerType.Types.CargoWise, convertedEntry.ContainerType.Type);
			AssertEquals("40GP", convertedEntry.ContainerType.Value);

			entry.TI_RC = ZGuid.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.ContainerType);
		}

		public void TestConvert_GWAgentType()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewIntercompanyTariff(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_GatewayAgentType = GatewayAgentType.Codes.ReceivingAgent;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals(GatewayAgentType.Codes.ReceivingAgent, convertedEntry.GWAgentType);
		}

		public void TestConvert_Commodity_CWRates()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_RH_NKCommodityCode = "HAZ";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertEquals(CommodityInfo.Types.CargoWise, convertedEntry.Commodity.Type);
			AssertEquals("HAZ", convertedEntry.Commodity.Value);
			AssertEquals("HAZARDOUS GOODS", convertedEntry.Commodity.Description);

			entry.TI_RH_NKCommodityCode = ZString.Empty;
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.Commodity);
		}

		public void TestConvert_Commodity_WiseRates()
		{
			var wiseRate = new WiseRate()
			{
				Commodity = "CMX",
			};

			var wiseEntry = new WiseEntry(wiseRate, Factory);
			wiseEntry.TI_RH_NKCommodityCode = string.Empty;

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("UCG", convertedEntry.Commodity.Type);
			AssertEquals("CMX", convertedEntry.Commodity.Value);

			wiseEntry.TI_RH_NKCommodityCode = "HAZ";

			convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("CW", convertedEntry.Commodity.Type);
			AssertEquals("HAZ", convertedEntry.Commodity.Value);
		}

		public void TestConvert_TransitTime()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_TransitTime = "55";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("55", convertedEntry.TransitTime);
		}

		public void TestConvert_Frequency()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_Frequency = 9;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(9, convertedEntry.Frequency);
		}

		public void TestConvert_FrequencyUnit()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_FrequencyUnit = "DAILY";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("DAILY", convertedEntry.FrequencyUnit);
		}

		public void TestConvert_StartDate()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_RateStartDate = ZDate.BrettsBirthday;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(ZDate.BrettsBirthday.ToISO8601ShortDateString(), convertedEntry.StartDate);
		}

		public void TestConvert_ExpiryDate()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var expiryDate = ZDate.Today;

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_RateEndDate = expiryDate;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(expiryDate.ToISO8601ShortDateString(), convertedEntry.ExpiryDate);
		}

		public void TestConvert_PayTermOverride_CWRates()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_PaymentTerm = "PPD";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("PPD", convertedEntry.PayTermOverride);
		}

		public void TestConvert_PayTermOverride_WiseRates()
		{
			var wiseEntry = new WiseEntry(new WiseRate(), Factory);
			wiseEntry.TI_PaymentTerm = "PPD";

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertNullOrEmpty(convertedEntry.PayTermOverride);
		}

		public void TestConvert_AircraftType()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_AircraftType = "ACT";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("ACT", convertedEntry.AircraftType);
		}

		public void TestConvert_RateProvider()
		{
			var wiseEntry = new WiseEntry(new WiseRate(), Factory);
			wiseEntry.RateProvider = "RP";

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("RP", convertedEntry.RateProvider);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("CW", convertedEntry.RateProvider);
		}

		public void TestConvert_IsCWGlobal()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals(false, convertedEntry.IsCWGlobal);

			var globalRate = Helper.NewGlobalCosting(null);
			var globalEntry = globalRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);

			var convertedGlobalEntry = converter.Convert(new[] { globalEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals(true, convertedGlobalEntry.IsCWGlobal);
		}

		public void TestConvertCharges_ChargeCode_CWRates()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var subgroup = ChargeCodeSubGroupList.Storage;
			var chargeGroup = ChargeCodeGroupList.Codes.Origin;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CNTHR1", "", UnitCalculator.Code, chargeGroup, subgroup);
			chargeCode1.AC_Desc = "ChargeDesc1";

			var universalMapping1 = chargeCode1.UniversalChargeCodeMappingsCollection.AddNew();
			universalMapping1.AUP_Code = "CAF";

			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("BNTHR2", "", UnitCalculator.Code, chargeGroup, subgroup);
			chargeCode2.AC_Desc = "ChargeDesc2";

			var universalMapping2 = chargeCode2.UniversalChargeCodeMappingsCollection.AddNew();
			universalMapping2.AUP_Code = "BAF";

			var costing = Helper.NewCosting(null);
			var entry = costing.AddRateEntry(chargeGroup, RateMode.AIR, "NZ", "AUMEL");
			entry.TI_RX_NKCurrency = CurrencyCodes.Australia;

			var line1 = entry.AddRateLine(chargeCode1, UnitCalculator.Code, QuantityUnit.KG);
			line1.GetCalculator<UnitCalculator>().PerUnit = 5;
			line1.ChargeInternalNoteText = "Internal Notes 1";
			line1.ConversionFactor = new ZArchitecture.Business.ConversionFactor(1.5m, Weight.Kilograms, Volume.CubicMetres);
			line1.TL_IsOnPallets = true;
			line1.TL_IsWhsJobLevelCharge = true;
			line1.TL_Condition = "BRK";
			line1.TL_ConditionalExpression = "\"<Blah>\" == \"ABC\"";
			line1.TL_ConditionalExpressionDescription = "This is a macro";
			line1.TL_RateStartDate = new ZDate(2023, 8, 5);
			line1.TL_RateEndDate = new ZDate(2023, 9, 5);
			line1.TL_FeeChargeType = "DWY";
			line1.TL_FeeChargeLevel = "STD";
			line1.TL_CompanyTariffLevel = 1;
			line1.TL_ContainerOwnership = "SHP";
			line1.ChargeInformationNoteText = "Who needs information note";
			line1.OverrideChargeDescription = true;
			line1.TL_RateDescLocal = "Charge local description";
			line1.TL_RateDesc = "Overridden Charge description";
			line1.UnitMultipleAsString = "ABC";

			var line2 = entry.AddRateLine(chargeCode2, UnitCalculator.Code, QuantityUnit.KG);
			line2.GetCalculator<UnitCalculator>().PerUnit = 8;
			line2.ChargeInternalNoteText = "Internal Notes 2";
			line2.ConversionFactor = new ZArchitecture.Business.ConversionFactor(1.7m, Weight.Kilograms, Volume.CubicMetres);

			var line3 = entry.AddRateLine((AccChargeCode)null, FlatCalculator.Code, QuantityUnit.KG);
			line3.TL_ActualPercentage = 100;
			line3.TL_UnitFactor = UnitFactorList.Codes.PacksWeight;
			line3.TL_Rounding = RatingRoundingTypes.UpTo1;

			var expectedConvertedCharges = new[]
			{
				new Web.Model.ChargeInfo()
				{
					ChargeCode = new ChargeCodeInfo { CWCode = "CNTHR1", UniversalCodes = new [] { "CAF" } },
					ChargeGroup = "ORG",
					ChargeSubGroup = "STG",
					Calculators = new [] { new CalculatorInfo() { CWCode = "UNT" , Attributes = new [] { new CalculatorAttribute()  { Name = "PerUnit", Type = "Decimal", Value = 5M } } } },
					ChargeDesc = "ChargeDesc1",
					ConversionFactor = new ConversionFactor() { Factor = "1.5", NumeratorUnit = Weight.Kilograms, DenominatorUnit = Volume.CubicMetres },
					Currency = "AUD",
					InternalNote = "Internal Notes 1",
					Unit = QuantityUnit.KG,
					IsOptional = false,
					IsOnPallets = true,
					IsWhsJobLevelCharge = true,
					Rounding = "DEF",
					Condition = "BRK",
					ConditionExp = "\"<Blah>\" == \"ABC\"",
					ConditionExpDesc = "This is a macro",
					UnitFactor = "",
					StartDate  = "2023-08-05",
					EndDate = "2023-09-05",
					FeeChargeLevel = "STD",
					FeeChargeType = "DWY",
					CompanyTariffLevel = 1,
					ContainerOwnership = "SHP",
					PublicNote = "Who needs information note",
					IsChargeDescOverride = true,
					ActualWgtVolOnly = false,
					ChargeLocalDesc = "Charge local description",
					UnitMultiple = "ABC",
					OverriddenChargeDesc = "Overridden Charge description"
				},
				new Web.Model.ChargeInfo()
				{
					ChargeCode = new ChargeCodeInfo { CWCode = "BNTHR2", UniversalCodes = new [] { "BAF" } },
					ChargeGroup = "ORG",
					ChargeSubGroup = "STG",
					Calculators = new [] { new CalculatorInfo() { CWCode = "UNT" , Attributes = new [] { new CalculatorAttribute()  { Name = "PerUnit", Type = "Decimal", Value = 8M } } } },
					ChargeDesc = "ChargeDesc2",
					ConversionFactor = new ConversionFactor() { Factor = "1.7", NumeratorUnit = Weight.Kilograms, DenominatorUnit = Volume.CubicMetres },
					Currency = "AUD",
					InternalNote = "Internal Notes 2",
					Unit = QuantityUnit.KG,
					IsOptional = false,
					Rounding = "DEF",
					IsChargeDescOverride = false,
					UnitFactor = "",
					StartDate  = "",
					EndDate = "",
					FeeChargeLevel = "",
					FeeChargeType = "",
					ActualWgtVolOnly = false,
					Condition = "",
					ConditionExp = "",
					ConditionExpDesc = "",
					ContainerOwnership = "",
					PublicNote = "",
					ChargeLocalDesc = "ChargeDesc2",
					UnitMultiple = "",
					OverriddenChargeDesc = "ChargeDesc2"
				},
				new Web.Model.ChargeInfo()
				{
					ChargeCode = new ChargeCodeInfo { CWCode = null, UniversalCodes = null },
					ChargeGroup = null,
					ChargeSubGroup = null,
					Calculators = new [] { new CalculatorInfo() { CWCode = "FLT" , Attributes = new [] { new CalculatorAttribute()  { Name = "BaseRate", Type = "Decimal", Value = 0m } } } },
					ChargeDesc = null,
					ConversionFactor = new ConversionFactor() { Factor = "0", NumeratorUnit = null, DenominatorUnit = null },
					Currency = "AUD",
					InternalNote = "",
					Unit = QuantityUnit.KG,
					IsOptional = false,
					ActualWgtVolOnly = true,
					ActualPercentage = 100,
					UnitFactor = UnitFactorList.Codes.PacksWeight,
					IsChargeDescOverride = false,
					StartDate  = "",
					EndDate = "",
					FeeChargeLevel = "",
					FeeChargeType = "",
					Condition = "",
					ConditionExp = "",
					ConditionExpDesc = "",
					ContainerOwnership = "",
					PublicNote = "",
					Rounding = RatingRoundingTypes.UpTo1,
					RoundingFactor = 1m,
					ChargeLocalDesc = "",
					UnitMultiple = "",
					OverriddenChargeDesc = ""
				}
			};

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(expectedConvertedCharges.Length, convertedEntry.Charges.Length);
			for (int i = 0; i < expectedConvertedCharges.Length; i++)
			{
				AssertChargeInfo(expectedConvertedCharges[i], convertedEntry.Charges[i]);
			}
		}

		public void TestConvertCharges_ChargeCode_WiseRates()
		{
			var wiseEntry = new WiseEntry(new WiseRate(), Factory);

			var wiseLine = new WiseLine(Factory, new Charge() { ChargeCode = "FRT", IsOptional = false }); // FRT is not mapped to any universal charge code, but we have FRT charge itself in CW1.
			wiseLine.ParentRateEntry = wiseEntry;
			wiseEntry.ChildRateLines = new[] { wiseLine };

			var expectedChargeCode = new ChargeCodeInfo { CWCode = "FRT", UniversalCodes = new[] { "FRT" } };

			var converter = new RateEntryToRateConverter(Factory);
			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();
			var chargeCode = convertedEntry.Charges.Single();
			AssertEquals(expectedChargeCode.CWCode, chargeCode.ChargeCode.CWCode);
			AssertContainsExactElementsInAnyOrder(expectedChargeCode.UniversalCodes, chargeCode.ChargeCode.UniversalCodes);
			AssertEquals("FRT", chargeCode.ChargeGroup);
			AssertEquals(string.Empty, chargeCode.ChargeSubGroup);
			Assert(!chargeCode.IsOptional);

			wiseLine = new WiseLine(Factory, new Charge() { ChargeCode = "XYZ", IsOptional = true }); // XYZ is not a valid charge
			wiseLine.ParentRateEntry = wiseEntry;
			wiseEntry.ChildRateLines = new[] { wiseLine };

			expectedChargeCode = new ChargeCodeInfo { CWCode = null, UniversalCodes = new[] { "XYZ" } };

			convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();
			chargeCode = convertedEntry.Charges.Single();

			AssertEquals(expectedChargeCode.CWCode, chargeCode.ChargeCode.CWCode);
			AssertContainsExactElementsInAnyOrder(expectedChargeCode.UniversalCodes, chargeCode.ChargeCode.UniversalCodes);
			AssertNull(chargeCode.ChargeGroup);
			AssertNull(chargeCode.ChargeSubGroup);
			Assert(chargeCode.IsOptional);
		}

		public void TestConvertCargoSphere()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var wiseRate = new WiseRate()
			{
				Provider = WRConstants.RateProviders.CargoSphere,
				ContainerMode = "FCL",
				TransportMode = "SEA",
				Commodity = "GENL",
				NamedAccounts = new string[] { "NAC1", "NAC2" },
				Charges = new List<Charge>(),
				ProviderCustomFields = new[]
				{
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.ArbitraryPermission,
						Value = "ADN"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.RateType,
						Value = "RT007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.RateType2,
						Value = "RT2007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Common.Routing,
						Value = "Route1 -> Route2"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.ServiceString,
						Value = "SST007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.TradeLane,
						Value = "TL007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.CargoSphere.Vessel,
						Value = "VL99080"
					},
				}
			};

			var logger = new ElementaryLogger();
			var wiseResponse = new RatesSearchResponse
			{
				Rates = new[] { wiseRate },
				Carriers = Array.Empty<RefCarrier>(),
				ChargeCodes = Array.Empty<RefChargeCode>(),
			};

			var wiseRatesConverter = new WiseRatesConverter(Factory, logger);
			var wiseEntry = wiseRatesConverter.Convert(wiseResponse, null).First();

			var expectedCSInfo = new CSRateInfo()
			{
				AddOn = "ADN",
				Commodity = "GENL",
				NamedAccounts = new string[] { "NAC1", "NAC2" },
				RateType = "RT007",
				RateType2 = "RT2007",
				Routing = "Route1 -> Route2",
				ServiceString = "SST007",
				TradeLane = "TL007",
				Vessel = "VL99080"
			};

			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(expectedCSInfo.AddOn, convertedEntry.CargoSphere.AddOn);
			AssertEquals(expectedCSInfo.Commodity, convertedEntry.CargoSphere.Commodity);
			AssertEquals(expectedCSInfo.RateType, convertedEntry.CargoSphere.RateType);
			AssertEquals(expectedCSInfo.RateType2, convertedEntry.CargoSphere.RateType2);
			AssertEquals(expectedCSInfo.Routing, convertedEntry.CargoSphere.Routing);
			AssertEquals(expectedCSInfo.ServiceString, convertedEntry.CargoSphere.ServiceString);
			AssertEquals(expectedCSInfo.TradeLane, convertedEntry.CargoSphere.TradeLane);
			AssertEquals(expectedCSInfo.Vessel, convertedEntry.CargoSphere.Vessel);
			AssertContainsExactElementsInAnyOrder(expectedCSInfo.NamedAccounts, convertedEntry.CargoSphere.NamedAccounts);
			AssertNull(convertedEntry.CargoGuide);
		}

		public void TestConvertCargoGuide()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var issueDate = DateTime.Now.Date;
			var wiseRate = new WiseRate()
			{
				Provider = WRConstants.RateProviders.CargoGuide,
				ContainerMode = "ULD",
				TransportMode = "AIR",
				IssueDate = issueDate,
				Origin = "AU",
				Destination = "USLAX",
				PaymentTerm = "PPD",
				Charges = new List<Charge>(),
				NamedAccounts = new string[] { "NA1", null, "", "NA2" },
				ProviderCustomFields = new[]
				{
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.Via,
						Value = new JArray(new JValue("AUMEL"), new JValue("AUSYD" ), new JValue(228)),
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.DeckType,
						Value = "DKT"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductClass,
						Value = "CS007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductCode,
						Value = "PC007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductId,
						Value = "PID007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductName,
						Value = "PN007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.RateClass,
						Value = "RC007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.Ratio,
						Value = "1.8"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.Reference,
						Value = "RFN007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.Remarks,
						Value = "RMK007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductDeckType,
						Value = "PDK007"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.CarrierGSAName,
						Value = "GSA Name"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.OriginName,
						Value = "Origin Name"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.OriginCity,
						Value = "AUSYD"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.OriginCityName,
						Value = "Sydney"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.PortOfLoading,
						Value = "AUSYD"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.DestinationName,
						Value = "Destination Name"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.DestinationCity,
						Value = "USLAX"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.DestinationCityName,
						Value = "Los Angles"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.PortOfDischarge,
						Value = "USLAX"
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.CargoAircraftOnly,
						Value = true
					},
					new CustomField()
					{
						Code = WiseRate.CustomFields.Cargoguide.ProductTemperatureRange,
						Value = new TemperatureRange { Minimum = -10M, Maximum = 50.7M }
					},
				}
			};

			var wiseResponse = new RatesSearchResponse
			{
				Rates = new[] { wiseRate },
				Carriers = Array.Empty<RefCarrier>(),
				ChargeCodes = Array.Empty<RefChargeCode>(),
			};

			var wiseRatesConverter = new WiseRatesConverter(Factory, logger);
			var wiseEntry = wiseRatesConverter.Convert(wiseResponse, null).First();

			var expectedCGInfo = new CGRateInfo()
			{
				Deck = "DKT",
				Destination = "USLAX",
				IssueDate = new ZDateTime(issueDate).ToISO8601ShortDateString(),
				Origin = "AU",
				PaymentTerm = "PPD",
				ProductClass = "CS007",
				ProductCode = "PC007",
				ProductDeck = "PDK007",
				ProductId = "PID007",
				ProductName = "PN007",
				RateClass = "RC007",
				Ratio = "1.8",
				Reference = "RFN007",
				Remarks = "RMK007",
				Via = new string[] { "AUMEL", "AUSYD" },
				NamedAccounts = new string[] { "NA1", null, "", "NA2" },
				GSAName = "GSA Name",
				OriginName = "Origin Name",
				OriginCity = "AUSYD",
				OriginCityName = "Sydney",
				POL = "AUSYD",
				DestinationName = "Destination Name",
				DestinationCity = "USLAX",
				DestinationCityName = "Los Angles",
				POD = "USLAX",
				CargoAircraftOnly = true,
				TemperatureRange = new TemperatureRange { Minimum = -10M, Maximum = 50.7M }
			};

			var convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();

			AssertEquals(expectedCGInfo.Deck, convertedEntry.CargoGuide.Deck);
			AssertEquals(expectedCGInfo.Destination, convertedEntry.CargoGuide.Destination);
			AssertEquals(expectedCGInfo.IssueDate, convertedEntry.CargoGuide.IssueDate);
			AssertEquals(expectedCGInfo.Origin, convertedEntry.CargoGuide.Origin);
			AssertEquals(expectedCGInfo.PaymentTerm, convertedEntry.CargoGuide.PaymentTerm);
			AssertEquals(expectedCGInfo.Ratio, convertedEntry.CargoGuide.Ratio);
			AssertEquals(expectedCGInfo.ProductClass, convertedEntry.CargoGuide.ProductClass);
			AssertEquals(expectedCGInfo.ProductCode, convertedEntry.CargoGuide.ProductCode);
			AssertEquals(expectedCGInfo.ProductDeck, convertedEntry.CargoGuide.ProductDeck);
			AssertEquals(expectedCGInfo.ProductId, convertedEntry.CargoGuide.ProductId);
			AssertEquals(expectedCGInfo.ProductName, convertedEntry.CargoGuide.ProductName);
			AssertEquals(expectedCGInfo.RateClass, convertedEntry.CargoGuide.RateClass);
			AssertEquals(expectedCGInfo.Reference, convertedEntry.CargoGuide.Reference);
			AssertEquals(expectedCGInfo.Remarks, convertedEntry.CargoGuide.Remarks);
			AssertContainsExactElementsInAnyOrder(expectedCGInfo.Via, convertedEntry.CargoGuide.Via);
			AssertContainsExactElementsInAnyOrder(expectedCGInfo.NamedAccounts, convertedEntry.CargoGuide.NamedAccounts);
			AssertEquals(expectedCGInfo.GSAName, convertedEntry.CargoGuide.GSAName);
			AssertEquals(expectedCGInfo.OriginName, convertedEntry.CargoGuide.OriginName);
			AssertEquals(expectedCGInfo.OriginCity, convertedEntry.CargoGuide.OriginCity);
			AssertEquals(expectedCGInfo.OriginCityName, convertedEntry.CargoGuide.OriginCityName);
			AssertEquals(expectedCGInfo.POL, convertedEntry.CargoGuide.POL);
			AssertEquals(expectedCGInfo.DestinationName, convertedEntry.CargoGuide.DestinationName);
			AssertEquals(expectedCGInfo.DestinationCity, convertedEntry.CargoGuide.DestinationCity);
			AssertEquals(expectedCGInfo.DestinationCityName, convertedEntry.CargoGuide.DestinationCityName);
			AssertEquals(expectedCGInfo.POD, convertedEntry.CargoGuide.POD);
			AssertEquals(expectedCGInfo.CargoAircraftOnly, convertedEntry.CargoGuide.CargoAircraftOnly);
			AssertEquals(expectedCGInfo.TemperatureRange.Minimum, convertedEntry.CargoGuide.TemperatureRange.Minimum);
			AssertEquals(expectedCGInfo.TemperatureRange.Maximum, convertedEntry.CargoGuide.TemperatureRange.Maximum);

			AssertNull(convertedEntry.CargoSphere);

			wiseRate.IssueDate = default;
			convertedEntry = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, logger).Single();
			AssertNull(convertedEntry.CargoGuide.IssueDate);
		}

		public void TestConvertWillNotConvertWiseEntryWithError()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var logger = new ElementaryLogger();

			var issueDate = DateTime.Now.Date;
			var wiseRate = new WiseRate()
			{
				Provider = WRConstants.RateProviders.CargoGuide,
				ContainerMode = "ULD",
				TransportMode = "AIR",
				IssueDate = issueDate,
				Origin = "AU",
				Destination = "USLAX",
				PaymentTerm = "PPD",
				Charges = new List<Charge>()
			};

			var wiseResponse = new RatesSearchResponse
			{
				Rates = new[] { wiseRate },
				Carriers = Array.Empty<RefCarrier>(),
				ChargeCodes = Array.Empty<RefChargeCode>(),
			};

			var wiseRatesConverter = new WiseRatesConverter(Factory, logger);
			var wiseEntry = wiseRatesConverter.Convert(wiseResponse, null).First();

			wiseEntry.Errors.Add(RateEntrySchema.TI_Mode, "TI_Mode Is Invalid"); //Fake Erroe

			var convertedEntries = converter.Convert(new[] { wiseEntry }, RatingConstants.RatingHeaderTypes.Costing, logger);
			AssertEquals("We will convert WiseEntry even if it is not valid", 1, convertedEntries.Count);

			Assert(logger.GetErrorsAndWarnings().Contains("An entry returned from 'Rate Service' could not be considered as a valid rate. Reason: TI_Mode Is Invalid\r\n"));
		}

		public void TestConvert_MultipleRates()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", removeLines: true);
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUMEL", "USLAX", removeLines: true);
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.FCL, "AIR", "AUPER", "USLAX", removeLines: true);

			var convertedRates = converter.Convert(new[] { entry1, entry2, entry3 }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger());
			AssertEquals(3, convertedRates.Count);
			var actualOrigins = convertedRates.Select(c => c.Origin.Value).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { "AUSYD", "AUMEL", "AUPER" }, actualOrigins);
		}

		public void TestConvert_Carrier()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var iATAAirLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty));

			var testAirline = Factory.New<OrgHeader>();
			testAirline.OH_Code = "AIR123";
			testAirline.OH_IsShippingProvider = true;
			testAirline.OH_IsAirLine = true;
			testAirline.MiscServ.OM_RM_Airline = iATAAirLine.PK;

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_OH_TransportProvider = testAirline.PK;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(testAirline.OH_Code, convertedEntry.Carrier);
			AssertEquals("Correct 2 char code", iATAAirLine.RM_TwoCharacterCode, convertedEntry.CarrierCode);
		}

		public void TestConvert_ContainerClass()
		{
			// if you are changing this, make sure we use a container that has value other than empty/null for container class
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.CST, removeLines: true);
			entry.TI_RC = GP40.PK;
			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();
			AssertEquals("40F", entry.Container.RC_StorageClass);
			AssertEquals(entry.Container.RC_StorageClass, convertedEntry.ContainerClass);
		}

		public void TestConvert_DestinationPostCode()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_CartageDeliveryAddressPostCode = "2114";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("2114", convertedEntry.DestPostCode);
		}

		public void TestConvert_OriginPostCode()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_CartagePickupAddressPostCode = "2114";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("2114", convertedEntry.OriginPostCode);
		}

		public void TestConvert_CreationSource()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_CreationSource = RateEntryCreator.Sources.FromQuotation;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(RateEntryCreator.Sources.FromQuotation, convertedEntry.CreationSource);
		}

		public void TestConvert_IsContainerClassMatch()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_MatchContainerRateClass = true;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(true, convertedEntry.IsCntrClassMatch);
		}

		public void TestConvert_IsCrossTrade()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_IsCrossTrade = true;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(true, convertedEntry.IsCrossTrade);
		}

		public void TestConvert_IsExcludedFromAutoRating()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_IsExcludedFromAutoRating = true;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(true, convertedEntry.ExcludeAutorate);
		}

		public void TestConvert_ShipmentConsolidationStatus()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_ShipmentConsolidationStatus = "STS";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("STS", convertedEntry.SHPConsolStatus);
		}

		public void TestConvert_HBLDeliveryMode()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider = Helper.NewOrgHeader();
			provider.OH_Code = "OHC";

			var clientRate = Helper.NewClientRate(provider);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_HBLDeliveryMode = "DOOR/DOOR";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("DOOR/DOOR", convertedEntry.HBLDeliveryMode);
		}

		public void TestConvert_FMCTariffID()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider = Helper.NewOrgHeader();
			provider.OH_Code = "OHC";

			var clientRate = Helper.NewClientRate(provider);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_FMCTariffID = "1234";

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("1234", convertedEntry.FMCTariffID);
		}

		public void TestConvert_ContractNumberLinked()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_ContractNumberLinked = true;

			var convertedEntry = converter.Convert(new[] { entry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals(true, convertedEntry.ContractNumberLinked);
		}

		public void TestConvert_PortTransportAddress_ForOrigin()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var unlocoEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", removeLines: true);
			var pickupFromOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupFromOrg.MainAddress.OA_City = "Pickup City";
			pickupFromOrg.MainAddress.OA_Code = "1 pickup address";
			unlocoEntry.TI_OA_CartagePickupAddressOverride = pickupFromOrg.MainAddress.PK;

			var convertedUNLOCO = converter.Convert(new[] { unlocoEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("1 pickup address", convertedUNLOCO.PortTrpAddr);
		}

		public void TestConvert_PortTransportAddress_ForDestination()
		{
			var converter = new RateEntryToRateConverter(Factory);
			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var unlocoEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, "AIR", "AUSYD", "USLAX", removeLines: true);

			var deliveryToOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryToOrg.MainAddress.OA_City = "Delivery City";
			deliveryToOrg.MainAddress.OA_Code = "1 delivery address";
			unlocoEntry.TI_OA_CartageDeliveryAddressOverride = deliveryToOrg.MainAddress.PK;

			var convertedUNLOCO = converter.Convert(new[] { unlocoEntry }, RatingConstants.RatingHeaderTypes.Costing, new ElementaryLogger()).Single();

			AssertEquals("1 delivery address", convertedUNLOCO.PortTrpAddr);
		}

		public void TestConvertIncotermPayer_EmptyAutoRateInfos()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";

			var subgroup = ChargeCodeSubGroupList.Storage;
			var chargeGroupOrigin = ChargeCodeGroupList.Codes.Origin;
			var chargeCodeOrigin = Helper.ChargeCodes.NewConsolChargeCode("CNTHR1", "", UnitCalculator.Code, chargeGroupOrigin, subgroup);

			var rate = Helper.NewClientRate(provider1);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var lineOrigin = entry.AddRateLine(chargeCodeOrigin, FlatCalculator.Code, "KG", "AUD");
			lineOrigin.RateLineItems.AddNew();

			var criteria = new TestRatingCriteria();

			criteria.ConsumerType = JobInvoicingConsumerTypes.OneOffQuotation;
			criteria.Consignee = Factory.New<OrgHeader>();
			criteria.Consignor = Factory.New<OrgHeader>();
			criteria.JobDirection = Directions.Export;
			criteria.PaymentTerm = new PaymentTermInfos();
			criteria.PaymentTerm.AddOrReplace(new PaymentTermInfo(PaymentTermType.Incoterm, CostSell.Revenue, "EXW"));

			var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger());

			AssertNull("IncotermPayer should be null when no autorateinfo", convertedEntry.Charges[0].IncotermPayer);
		}

		public void TestConvertRateModule()
		{
			var provider = Helper.NewOrgHeader();
			provider.OH_Code = "OHC";

			var clientRate = Helper.NewClientRate(provider);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var clientRateLine = clientRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			clientRateLine.RateLineItems.AddNew();

			AssertConvertRateModule(clientRateEntry, clientRateLine, "CLR");

			var costing = Helper.NewCosting(provider);
			var costingRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var costingRateLine = costingRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			costingRateLine.RateLineItems.AddNew();

			AssertConvertRateModule(costingRateEntry, costingRateLine, "CST");

			var companyTariff = Helper.NewCompanyTariff();
			var companyTariffRateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var companyTariffRateLine = companyTariffRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			companyTariffRateLine.RateLineItems.AddNew();

			AssertConvertRateModule(companyTariffRateEntry, companyTariffRateLine, "CTR");

			var intercompanyTariff = Helper.NewIntercompanyTariff(provider);
			var intercompanyTariffRateEntry = intercompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var intercompanyTariffRateLine = intercompanyTariffRateEntry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			intercompanyTariffRateLine.RateLineItems.AddNew();

			AssertConvertRateModule(intercompanyTariffRateEntry, intercompanyTariffRateLine, "ICT");
		}

		void AssertConvertRateModule(RateEntry entry, RateLine line, string expectedValue)
		{
			var converter = new RateEntryToRateConverter(Factory);
			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new TestRatingCriteria();

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

			AssertEquals(expectedValue, convertedEntry.RateModule);
		}

		public void TestConvertRateParty()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";

			var rate = Helper.NewClientRate(provider1);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var lineOrigin = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			lineOrigin.RateLineItems.AddNew();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new TestRatingCriteria();

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(lineOrigin, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

			AssertEquals("OHC", convertedEntry.RateParty);
		}

		public void TestConvertRateParty_NoParentRatingHeader()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";

			var rate = Helper.NewClientRate(provider1);
			var wiseEntry = new WiseEntry(new WiseRate(), Factory);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var lineOrigin = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			lineOrigin.RateLineItems.AddNew();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new TestRatingCriteria();

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(lineOrigin, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var convertedEntry = converter.Convert(wiseEntry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

			AssertNull(convertedEntry.RateParty);
		}

		public void TestConvertIncotermPayer()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";

			var rate = Helper.NewClientRate(provider1);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			entry.TI_PaymentTerm = "EXW";

			var subgroup = ChargeCodeSubGroupList.Storage;
			var chargeGroupOrigin = ChargeCodeGroupList.Codes.Origin;
			var chargeCodeOrigin = Helper.ChargeCodes.NewConsolChargeCode("CNTHR1", "", UnitCalculator.Code, chargeGroupOrigin, subgroup);

			var line = entry.AddRateLine(chargeCodeOrigin, FlatCalculator.Code, "KG", "AUD");
			line.RateLineItems.AddNew();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new TestRatingCriteria();

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(line, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			var incotermListConsignee = GetIncotermRegistry(PaymentParty.Consignee, IncoTerms.ExWorks);
			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incotermListConsignee))
			{
				var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

				AssertContainsExactElementsInAnyOrder("Consignee should return CNE", new[] { "CNE" }, convertedEntry.Charges.Select(x => x.IncotermPayer));
			}

			var incotermListConsignor = GetIncotermRegistry(PaymentParty.Consignor, IncoTerms.ExWorks);
			using (RatingDataRegistry.Instance.IncoTermDefinition.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, incotermListConsignor))
			{
				var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

				AssertContainsExactElementsInAnyOrder("Consignor should return CNR", new[] { "CNR" }, convertedEntry.Charges.Select(x => x.IncotermPayer));
			}
		}

		IncoTermChargeCodesCollection GetIncotermRegistry(string paymentParty, string incoterms)
		{
			var incoTermDefinitions = new IncoTermChargeCodesCollection();
			var def = incoTermDefinitions.AddNew();
			def.Origin = paymentParty;
			def.Loading = paymentParty;
			def.Freight = paymentParty;
			def.Insurance = paymentParty;
			def.Unloading = paymentParty;
			def.Destination = paymentParty;
			def.Brokerage = paymentParty;
			def.CustomsDuty = paymentParty;
			def.OriginBrokerage = paymentParty;
			def.IncoTerm = incoterms;
			return incoTermDefinitions;
		}

		public void TestConvertRateParty_CompanyTariff()
		{
			var converter = new RateEntryToRateConverter(Factory);

			var provider1 = Helper.NewOrgHeader();
			provider1.OH_Code = "OHC";

			var rate = Helper.NewCompanyTariff();
			rate.Company.GC_OH_OrgProxy = provider1.PK;
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var lineOrigin = entry.AddRateLine("FRT", FlatCalculator.Code, "KG", "AUD");
			lineOrigin.RateLineItems.AddNew();

			var query = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var businessObject = new RateQueryBusinessObject(Factory, query, SourceEndpoint.JobCharges);
			var criteria = new TestRatingCriteria();

			var autoRatingParameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var calculationResult = CalculationResult.CreateForTest(lineOrigin, 50, 0, 50, autoRatingParameters.Criteria);

			var rateInfo = new AutoRateInfo(calculationResult, autoRatingParameters, Factory);

			Factory.Save();

			var convertedEntry = converter.Convert(entry, RatingConstants.RatingHeaderTypes.ClientRate, new ElementaryLogger(), (RateQueryRatingAdapter)businessObject.RatingAdapter, new[] { rateInfo });

			AssertEquals("OHC", convertedEntry.RateParty);
		}

		void AssertChargeInfo(Web.Model.ChargeInfo expected, Web.Model.ChargeInfo actual)
		{
			AssertEquals(expected.ChargeGroup, actual.ChargeGroup);
			AssertEquals(expected.ChargeSubGroup, actual.ChargeSubGroup);
			AssertEquals(expected.Calculators.Length, actual.Calculators.Length);
			AssertEquals(expected.ChargeDesc, actual.ChargeDesc);
			AssertEquals(expected.ConversionFactor.Factor, actual.ConversionFactor.Factor);
			AssertEquals(expected.ConversionFactor.NumeratorUnit, actual.ConversionFactor.NumeratorUnit);
			AssertEquals(expected.ConversionFactor.DenominatorUnit, actual.ConversionFactor.DenominatorUnit);
			AssertEquals(expected.Currency, actual.Currency);
			AssertEquals(expected.InternalNote, actual.InternalNote);
			AssertEquals(expected.Unit, actual.Unit);
			AssertEquals(expected.IsOptional, actual.IsOptional);
			AssertEquals(expected.IsOnPallets, actual.IsOnPallets);
			AssertEquals(expected.IsWhsJobLevelCharge, actual.IsWhsJobLevelCharge);
			AssertEquals(expected.Rounding, actual.Rounding);
			AssertEquals(expected.Condition, actual.Condition);
			AssertEquals(expected.ConditionExp, actual.ConditionExp);
			AssertEquals(expected.ConditionExpDesc, actual.ConditionExpDesc);
			AssertEquals(expected.UnitFactor, actual.UnitFactor);
			AssertEquals(expected.StartDate, actual.StartDate);
			AssertEquals(expected.EndDate, actual.EndDate);
			AssertEquals(expected.FeeChargeLevel, actual.FeeChargeLevel);
			AssertEquals(expected.FeeChargeType, actual.FeeChargeType);
			AssertEquals(expected.CompanyTariffLevel, actual.CompanyTariffLevel);
			AssertEquals(expected.ContainerOwnership, actual.ContainerOwnership);
			AssertEquals(expected.PublicNote, actual.PublicNote);
			AssertEquals(expected.IsChargeDescOverride, actual.IsChargeDescOverride);
			AssertEquals(expected.ActualWgtVolOnly, actual.ActualWgtVolOnly);
			AssertEquals(expected.ChargeLocalDesc, actual.ChargeLocalDesc);
			AssertEquals(expected.UnitMultiple, actual.UnitMultiple);
			AssertEquals(expected.OverriddenChargeDesc, actual.OverriddenChargeDesc);
		}
	}
}
