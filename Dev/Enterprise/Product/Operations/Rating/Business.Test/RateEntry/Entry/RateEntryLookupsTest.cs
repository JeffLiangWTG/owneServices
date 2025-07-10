using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using HBLDeliveryModes = Enterprise.Registry.Business.HBLDeliveryModes;

namespace Enterprise.Rating.Business.Testing
{
	public class RateEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Transport Zones

		public void TestGetTransportZonesByOrg()
		{
			var owner1 = Factory.NewWithValidTestData<OrgHeader>();
			var owner2 = Factory.NewWithValidTestData<OrgHeader>();
			var owner3 = Factory.NewWithValidTestData<OrgHeader>();
			var owner4 = Factory.NewWithValidTestData<OrgHeader>();
			var owner5 = Factory.NewWithValidTestData<OrgHeader>();

			var zoneGroupS = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);
			var zoneGroup1 = Helper.CreateRateTransportZoneSet(owner1, CountryCodes.Belarus, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);
			var zoneGroup2 = Helper.CreateRateTransportZoneSet(owner2, CountryCodes.China, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);
			var zoneGroup3 = Helper.CreateRateTransportZoneSet(owner3, CountryCodes.Malaysia, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);
			var zoneGroup4 = Helper.CreateRateTransportZoneSet(owner4, CountryCodes.Ukraine, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);
			var zoneGroup5 = Helper.CreateRateTransportZoneSet(owner5, CountryCodes.UnitedKingdom, RatingConstants.RatingZoneTypes.Rating, RateMode.LRO);

			var zoneS1 = zoneGroupS.CreateRateTransportZoneForTest("zoneS1");
			var zoneS2 = zoneGroupS.CreateRateTransportZoneForTest("zoneS2");
			var zone11 = zoneGroup1.CreateRateTransportZoneForTest("zone11");
			var zone12 = zoneGroup1.CreateRateTransportZoneForTest("zone12");
			var zone21 = zoneGroup2.CreateRateTransportZoneForTest("zone21");
			var zone22 = zoneGroup2.CreateRateTransportZoneForTest("zone22");
			var zone31 = zoneGroup3.CreateRateTransportZoneForTest("zone31");
			var zone32 = zoneGroup3.CreateRateTransportZoneForTest("zone32");
			var zone41 = zoneGroup4.CreateRateTransportZoneForTest("zone41");
			var zone42 = zoneGroup4.CreateRateTransportZoneForTest("zone42");
			var zone51 = zoneGroup5.CreateRateTransportZoneForTest("zone51");
			var zone52 = zoneGroup5.CreateRateTransportZoneForTest("zone52");

			var rate = Factory.NewWithValidTestData<ClientRate>() as RatingHeader;
			rate.TH_QuoteNumber = "";

			var ct = Factory.NewWithValidTestData<CompanyTariff>() as RatingHeader;
			ct.TH_QuoteNumber = "";

			var costing = Factory.NewWithValidTestData<Costing>() as RatingHeader;
			costing.TH_QuoteNumber = "";

			var quote = Factory.NewWithValidTestData<Quote>() as RatingHeader;
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			Factory.Save();

			foreach (var header in new[] { rate, ct, costing, quote })
			{
				var entry = header.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LRO, "AUSYD", "AUBNE");
				entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);

				if (header.IsTariff())
				{
					AssertZones(header.GetType().ToString(), new[] { zoneS1, zoneS2 }, entry);

					entry.TI_OH_Supplier = owner3.PK;
					Factory.Save();
					AssertZones(header.GetType() + " with TP", new[] { zoneS1, zoneS2, zone31, zone32 }, entry);

					entry.TI_OH_Consignee = owner4.PK;
					entry.TI_OH_Consignor = owner5.PK;
					Factory.Save();
					AssertZones(header.GetType() + " with TP, CNE, CNR", new[] { zoneS1, zoneS2, zone31, zone32, zone41, zone42, zone51, zone52 }, entry);
				}
				else
				{
					header.TH_OH = owner1.PK;
					AssertZones(header.GetType().ToString(), new[] { zoneS1, zoneS2, zone11, zone12 }, entry);

					entry.TI_OH_Supplier = owner3.PK;
					entry.TI_OH_Consignee = owner4.PK;
					entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
					Factory.Save();
					AssertZones(header.GetType() + " with TP, CNE", new[] { zoneS1, zoneS2, zone11, zone12, zone31, zone32, zone41, zone42 }, entry);

					entry.TI_OH_Consignor = owner5.PK;
					Factory.Save();
					AssertZones(header.GetType() + " with TP, CNE, CNR", new[] { zoneS1, zoneS2, zone11, zone12, zone31, zone32, zone41, zone42, zone51, zone52 }, entry);
				}
			}
		}

		public void TestGetTransportZonesByMode_TransportZoneWithMoreGenericModeCanBeMatchedByMoreSpecificRateEntryMode()
		{
			var allZoneName = RatingConstants.RatingZoneTypes.All + " " + RateMode.ALL + " Zone";
			var allZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All, RateMode.ALL);
			allZoneSet.CreateRateTransportZoneForTest(allZoneName);

			var rateAllZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.ALL + " Zone";
			var rateAllZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			rateAllZoneSet.CreateRateTransportZoneForTest(rateAllZoneName);

			var rateROAZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.ROA + " Zone";
			var rateROAZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ROA);
			rateROAZoneSet.CreateRateTransportZoneForTest(rateROAZoneName);

			var rateFROZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.FRO + " Zone";
			var rateFROZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.FRO);
			rateFROZoneSet.CreateRateTransportZoneForTest(rateFROZoneName);

			var rateFWLZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.FWL + " Zone";
			var rateFWLZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.FWL);
			rateFWLZoneSet.CreateRateTransportZoneForTest(rateFWLZoneName);

			var quote = Factory.NewWithValidTestData<Quote>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Factory.NewWithValidTestData<Costing>();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			foreach (var header in new RatingHeader[] { clientRate, companyTariff, costing, quote })
			{
				var entry = header.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ALL, CountryCodes.Australia, "");
				var expected = new[] { allZoneName, rateAllZoneName };
				var actual = entry.Lookups.TransportZones.Select(x => x.TZ_ZoneName);
				AssertContainsExactElementsInAnyOrder(expected, actual);

				var entry1 = header.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.ROA, CountryCodes.Australia, "");
				var expected1 = new[] { allZoneName, rateAllZoneName, rateROAZoneName };
				var actual1 = entry1.Lookups.TransportZones.Select(x => x.TZ_ZoneName);
				AssertContainsExactElementsInAnyOrder(expected1, actual1);

				var entry2 = header.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.FRO, CountryCodes.Australia, "");
				var expected2 = new[] { allZoneName, rateAllZoneName, rateROAZoneName, rateFROZoneName };
				var actual2 = entry2.Lookups.TransportZones.Select(x => x.TZ_ZoneName);
				AssertContainsExactElementsInAnyOrder(expected2, actual2);

				var entry3 = header.AddRateEntry(RatingConstants.RateCategory.TBC, Core.Constants.RateMode.FWL, CountryCodes.Australia, "");
				var expected3 = new[] { allZoneName, rateAllZoneName, rateFWLZoneName };
				var actual3 = entry3.Lookups.TransportZones.Select(x => x.TZ_ZoneName);
				AssertContainsExactElementsInAnyOrder(expected3, actual3);
			}
		}

		public void TestGetTransportZonesByType()
		{
			var owner = Factory.NewWithValidTestData<OrgHeader>();

			var allZoneGroup = Helper.CreateRateTransportZoneSet(owner, CountryCodes.China, RatingConstants.RatingZoneTypes.All, RateMode.ALL);
			var allZone1 = allZoneGroup.CreateRateTransportZoneForTest("ALL 1");
			var allZone2 = allZoneGroup.CreateRateTransportZoneForTest("ALL 2");

			var opsZoneGroup = Helper.CreateRateTransportZoneSet(owner, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Operations, RateMode.ALL);
			opsZoneGroup.CreateRateTransportZoneForTest("OPS 1");
			opsZoneGroup.CreateRateTransportZoneForTest("OPS 2");

			var rateZoneGroup = Helper.CreateRateTransportZoneSet(owner, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LRA);
			var rateZone1 = rateZoneGroup.CreateRateTransportZoneForTest("RATE 1");
			var rateZone2 = rateZoneGroup.CreateRateTransportZoneForTest("RATE 2");

			var rptZoneGroup = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Reporting, RateMode.ALL);
			rptZoneGroup.CreateRateTransportZoneForTest("RPT 1");
			rptZoneGroup.CreateRateTransportZoneForTest("RPT 2");

			var rate = Factory.NewWithValidTestData<ClientRate>();
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Factory.NewWithValidTestData<Costing>();
			var quote = Factory.NewWithValidTestData<Quote>();

			Factory.Save();

			foreach (var header in new RatingHeader[] { rate, companyTariff, costing, quote })
			{
				rateZoneGroup.TP_ZoneType = RatingConstants.RatingZoneTypes.Rating;
				rateZoneGroup.TP_ZoneMode = Core.Constants.RateMode.LRA;
				if (!header.IsTariff())
				{
					header.TH_OH = owner.PK;
				}

				var entry = header.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LRA, "AUSYD", "AUBNE");
				entry.TI_OH_Consignee = owner.PK;
				entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
				Factory.Save();

				AssertZones(header.GetType() + " ALL and LRA zone mode are applicable", new[] { allZone1, allZone2, rateZone1, rateZone2 }, entry);

				rateZoneGroup.TP_ZoneType = RatingConstants.RatingZoneTypes.Operations;
				Factory.Save();

				AssertZones(header.GetType() + " Only ALL zone mode is applicable as there is no other zone with type RAT and zone mode LRA", new[] { allZone1, allZone2 }, entry);
			}
		}

		public void TestGetTransportZonesByType_RatingZonesWithMode()
		{
			var allZoneName = RatingConstants.RatingZoneTypes.All + " " + RateMode.ALL + " Zone";
			var allZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.All, RateMode.ALL);
			allZoneSet.CreateRateTransportZoneForTest(allZoneName);

			var rateAllZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.ALL + " Zone";
			var rateAllZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.ALL);
			rateAllZoneSet.CreateRateTransportZoneForTest(rateAllZoneName);

			var rateFCLZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.FCL);
			rateFCLZoneSet.CreateRateTransportZoneForTest(RatingConstants.RatingZoneTypes.Rating + " " + RateMode.FCL + " Zone");

			var rateLCLZoneName = RatingConstants.RatingZoneTypes.Rating + " " + RateMode.LCL + " Zone";
			var rateLCLZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LCL);
			rateLCLZoneSet.CreateRateTransportZoneForTest(rateLCLZoneName);

			var quote = Factory.NewWithValidTestData<Quote>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			var costing = Factory.NewWithValidTestData<Costing>();

			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			foreach (var header in new RatingHeader[] { clientRate, companyTariff, costing, quote })
			{
				var entry = header.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AU", "");
				var expected = new[] { allZoneName, rateAllZoneName, rateLCLZoneName };
				var actual = entry.Lookups.TransportZones.Select(x => x.TZ_ZoneName);

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		public void TestGetOnlyActiveTransportZones()
		{
			var allZoneGroup1 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LRA);
			var allZone1_active = allZoneGroup1.CreateRateTransportZoneForTest("Test A");
			var allZone2_active = allZoneGroup1.CreateRateTransportZoneForTest("Test B");
			var allZone3_inactive = allZoneGroup1.CreateRateTransportZoneForTest("Inactive Zone");
			allZone3_inactive.TZ_IsActive = false;

			var allZoneGroup2 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, RatingConstants.RatingZoneTypes.Rating, RateMode.LRA);
			allZoneGroup2.CreateRateTransportZoneForTest("Test A Inactive");
			allZoneGroup2.CreateRateTransportZoneForTest("Test B Inactive");
			allZoneGroup2.TP_IsActive = false;

			var rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_QuoteNumber = "";

			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			companyTariff.TH_QuoteNumber = "";

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_QuoteNumber = "";

			var quote = Factory.NewWithValidTestData<Quote>();

			Factory.Save();

			foreach (var header in new RatingHeader[] { rate, companyTariff, costing, quote })
			{
				var entry = header.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LRA, "AUSYD", "AUBNE");
				entry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
				Factory.Save();

				AssertZones(header.GetType() + " should find only active Zones", new[] { allZone1_active, allZone2_active }, entry);
			}
		}

		static void AssertZones(string message, IEnumerable<RateTransportZone> zones, RateEntry entry)
		{
			var expected = zones.Select(x => x.TZ_ZoneName);
			var actual = entry.Lookups.TransportZones.Select(x => x.TZ_ZoneName);

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		#endregion

		#region Tariff Type - Direction and Transport Mode

		public void TestGetApplicableDirections()
		{
			foreach (var tariffType in tariffTypes)
			{
				var directions = RateEntryLookups.GetApplicableDirections(tariffType);
				if (tariffType == RatingConstants.RateCategory.CST || tariffType == RatingConstants.RateCategory.WHS || tariffType == RatingConstants.RateCategory.TRW || tariffType == RatingConstants.RateCategory.TWU || tariffType == RatingConstants.RateCategory.TRN || tariffType == RatingConstants.RateCategory.CYD)
				{
					AssertEquals(1, directions.Count);
					AssertEquals("ALL", directions[0].Code);
				}
				else
				{
					AssertEquals(3, directions.Count);
					AssertEquals("ALL", directions[0].Code);
					AssertEquals("EXP", directions[1].Code);
					AssertEquals("IMP", directions[2].Code);
				}
			}
		}

		readonly string[] tariffTypes = { "DEF", "FRT", "ORG", "DST", "SFR", "SOR", "SDE", "SCD", "CFS", "WHS", "TRW", "TWU", "TRN", "CYD" };

		public void TestGetTransportModesByTariffType()
		{
			foreach (var tariffType in tariffTypes)
			{
				AssertModes(tariffType);
			}
		}

		void AssertModes(string code)
		{
			var modesList = RateEntryLookups.GetTransportModesByTariffType(code);

			switch (code)
			{
				case "DEF":
					AssertEquals(0, modesList.Count);
					break;

				case "FRT":
					AssertEquals(17, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BBK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BLK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("UNA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("OBC")));
					break;

				case "ORG":
				case "DST":
					AssertEquals(24, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("MAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BBK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BLK")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("BCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SCN")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("COU")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("UNA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("OBC")));
					break;

				case "SFR":
					AssertEquals(2, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					break;

				case "SOR":
				case "SDE":
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					break;

				case "SCD":
					AssertEquals(0, modesList.Count);
					break;

				case "CFS":
					AssertEquals(16, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ULD")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LSE")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FCL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("GRP")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					break;

				case "WHS":
					AssertEquals(0, modesList.Count);
					break;

				case "TRW":
					AssertEquals(0, modesList.Count);
					break;

				case "TWU":
					AssertEquals(4, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("SEA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					break;

				case "CYD":
					AssertEquals(2, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					break;

				case "TRN":
					AssertEquals(5, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("AIR")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					break;

				case "TBC":
					AssertEquals(9, modesList.Count);
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ALL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("ROA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRO")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FTL")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("RAI")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("LRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FRA")));
					Assert(!string.IsNullOrEmpty(modesList.GetDescriptionFromCode("FWL")));
					break;

				default:
					Fail(ZString.Format("{0} tariff type code needs to be checked here", code));
					break;
			}
		}

		#endregion

		#region Carrier Service Levels

		public void TestCarrierServiceLevels()
		{
			var airFrtEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			var lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "ZUB";
			lvl.PL_CarrierServiceLevelDescription = "ZUBIN";

			Factory.Save();

			AssertEquals("STD", 1, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = carrier.PK;
			AssertEquals("Two defined service levels + STD", 3, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = carrier2.PK;
			AssertEquals("One defined service levels + STD", 2, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = ZGuid.Empty;
			AssertEquals("STD", 1, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_Supplier = carrier2.PK;
			AssertEquals("One defined service levels + STD", 2, airFrtEntry.Lookups.CarrierServiceLevels.Count);
		}

		public void TestCarrierServiceLevels_Costing()
		{
			var airFrtEntry = Helper.NewCosting(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			var lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "XXX";
			lvl.PL_CarrierServiceLevelDescription = "XXX";
			lvl = carrier.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "DEF";
			lvl.PL_CarrierServiceLevelDescription = "DEF";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			lvl = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			lvl.PL_Code = "ZUB";
			lvl.PL_CarrierServiceLevelDescription = "ZUBIN";

			Factory.Save();

			AssertEquals("STD", 1, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = carrier.PK;
			AssertEquals("Two defined service levels + STD", 3, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = carrier2.PK;
			AssertEquals("Two defined service levels + STD", 2, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.TI_OH_TransportProvider = ZGuid.Empty;
			AssertEquals("STD", 1, airFrtEntry.Lookups.CarrierServiceLevels.Count);

			airFrtEntry.Parent.TH_OH = carrier2.PK;
			AssertEquals("Two defined service levels + STD", 2, airFrtEntry.Lookups.CarrierServiceLevels.Count);
		}

		#endregion

		#region Containers

		public void TestContainers()
		{
			var airContainer = RefContainer.New(Factory);
			airContainer.RC_ShippingMode = "AIR";
			airContainer.RC_Code = "AIR1";

			var fCLContainer = RefContainer.New(Factory);
			fCLContainer.RC_ShippingMode = "SEA";
			fCLContainer.RC_Code = "80GP";

			var roadContainer = RefContainer.New(Factory);
			roadContainer.RC_ShippingMode = "ROA";
			roadContainer.RC_Code = "TRK1";

			Factory.Save();

			var airFrtEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "");
			var fCLFrtEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "FCL", "AUSYD", "");
			var roadFCLFrtEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "FRO", "AUSYD", "");
			var airOriginEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG", "AIR", "AUSYD", "");
			var seaOriginEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG", "SEA", "AUSYD", "");
			var blankOriginEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG", "", "AUSYD", "");

			AssertEquals("AIR", airFrtEntry.Lookups.Containers.TransportMode);
			AssertEquals("SEA", fCLFrtEntry.Lookups.Containers.TransportMode);
			AssertEquals("ROA", roadFCLFrtEntry.Lookups.Containers.TransportMode);
			AssertEquals("AIR", airOriginEntry.Lookups.Containers.TransportMode);
			AssertEquals("SEA", seaOriginEntry.Lookups.Containers.TransportMode);
			AssertEquals("", blankOriginEntry.Lookups.Containers.TransportMode);
		}

		#endregion

		#region Transport Mode

		public void TestTransportModes()
		{
			var transportModes = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.ORG).Lookups.TransportModes;
			AssertEquals(24, transportModes.Count);
			Assert(transportModes.ContainsCode(RateMode.MAI));
			Assert(!transportModes.ContainsCode(RateMode.GRP));

			Assert(transportModes.ContainsCode(RateMode.UNA));
			Assert(transportModes.ContainsCode(RateMode.COU));
			Assert(transportModes.ContainsCode(RateMode.OBC));

			var testCFSRate = Helper.NewClientRate(Helper.NewOrgHeader());
			transportModes = testCFSRate.AddRateEntry(RatingConstants.RateCategory.PAC).Lookups.TransportModes;
			AssertEquals(16, transportModes.Count);
			Assert(!transportModes.ContainsCode(RateMode.MAI));
			Assert(transportModes.ContainsCode(RateMode.GRP));
			transportModes = testCFSRate.AddRateEntry(RatingConstants.RateCategory.CST).Lookups.TransportModes;
			AssertEquals(5, transportModes.Count);

			transportModes = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TRN).Lookups.TransportModes;
			AssertEquals(5, transportModes.Count);

			transportModes = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.CYD).Lookups.TransportModes;
			AssertEquals(2, transportModes.Count);

			AssertEquals("SCO", "SEA", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SCO).Lookups.TransportModes.CodesAsString);
			AssertEquals("SNC", "LCL", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SNC).Lookups.TransportModes.CodesAsString);
			AssertEquals("SOR", "ALL, LCL, FCL", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR).Lookups.TransportModes.CodesAsString);
			AssertEquals("SDE", "ALL, LCL, FCL", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SDE).Lookups.TransportModes.CodesAsString);
			AssertEquals("SED", "SEA", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SED).Lookups.TransportModes.CodesAsString);
			AssertEquals("SID", "SEA", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SID).Lookups.TransportModes.CodesAsString);
			AssertEquals("WHS", "ALL", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.WHS).Lookups.TransportModes.CodesAsString);
			AssertEquals("TRW", "ALL", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TRW).Lookups.TransportModes.CodesAsString);
			AssertEquals("TWU", "ALL, AIR, SEA, ROA", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TWU).Lookups.TransportModes.CodesAsString);
			AssertEquals("CYD", "ALL, ROA", Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.CYD).Lookups.TransportModes.CodesAsString);
		}

		public void TestTransportMode_ForwardingCategory_Caching()
		{
			AssertTransportMode_ForwardingCategory(Helper.NewClientRate(Helper.NewOrgHeader()));
			AssertTransportMode_ForwardingCategory(Helper.NewCosting(Helper.NewOrgHeader()));
			AssertTransportMode_ForwardingCategory(Helper.NewCompanyTariff());
			AssertTransportMode_ForwardingCategory(Helper.NewIntercompanyTariff());
			AssertTransportMode_ForwardingCategory(Helper.NewQuote(Helper.NewOrgHeader()));
			AssertTransportMode_ForwardingCategory(Helper.NewClientRate(Helper.NewOrgHeader()));
		}

		public void TestTransportMode_ForwardingCategory_ClientRate() => AssertTransportMode_ForwardingCategory(Helper.NewClientRate(Helper.NewOrgHeader()));

		public void TestTransportMode_ForwardingCategory_Costing() => AssertTransportMode_ForwardingCategory(Helper.NewCosting(Helper.NewOrgHeader()));

		public void TestTransportMode_ForwardingCategory_CompanyTariff() => AssertTransportMode_ForwardingCategory(Helper.NewCompanyTariff());

		public void TestTransportMode_ForwardingCategory_Quotation() => AssertTransportMode_ForwardingCategory(Helper.NewQuote(Helper.NewOrgHeader()));

		void AssertTransportMode_ForwardingCategory(RatingHeader ratingHeader)
		{
			CombineAssertions(() =>
			{
				AssertEquals("AIR", "LSE, ULD, BCN, SCN", GetTransportModesAsString(ratingHeader, RatingConstants.RateCategory.AIR));
				AssertEquals("FCL", "SEA, ROA, RAI, BCN, SCN", GetTransportModesAsString(ratingHeader, RatingConstants.RateCategory.FCL));
				AssertEquals("LCL", "LCL, LRO, FTL, LRA, FWL, BBK, BLK, ROR, BCN, SCN, UNA, OBC", GetTransportModesAsString(ratingHeader, RatingConstants.RateCategory.LCL));
				AssertEquals("ORG", "ALL, AIR, ULD, LSE, SEA, LCL, FCL, ROA, LRO, FRO, FTL, RAI, LRA, FRA, FWL, MAI, BBK, BLK, ROR, BCN, SCN, COU, UNA, OBC", GetTransportModesAsString(ratingHeader, RatingConstants.RateCategory.ORG));
				AssertEquals("DST", "ALL, AIR, ULD, LSE, SEA, LCL, FCL, ROA, LRO, FRO, FTL, RAI, LRA, FRA, FWL, MAI, BBK, BLK, ROR, BCN, SCN, COU, UNA, OBC", GetTransportModesAsString(ratingHeader, RatingConstants.RateCategory.DST));
			});
		}

		static string GetTransportModesAsString(RatingHeader ratingHeader, string category)
		{
			return ratingHeader.AddRateEntry(category).Lookups.TransportModes.CodesAsString;
		}

		#endregion

		#region Organizations

		public void TestConsignorsConsignees()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.ORG, "FCL", "", "");
			AssertEquals(typeof(ConsignorCollection), entry.Lookups.Consignors.GetType());
			AssertEquals(typeof(ConsigneeCollection), entry.Lookups.Consignees.GetType());

			entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TRN, "FCL", "", "");
			AssertEquals(typeof(OrganisationsFindBoxCollection), entry.Lookups.Consignors.GetType());
			AssertEquals(typeof(OrganisationsFindBoxCollection), entry.Lookups.Consignees.GetType());
		}

		public void TestShippingPrincipals()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertType(typeof(ShipsAgencyPrincipalCollection), entry.Lookups.ShippingPrincipals);
		}

		public void TestSuppliersCollection()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR");

			AssertType(typeof(CreditorCollection), rateEntry.Lookups.Suppliers);

			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("AIR");

			AssertType(typeof(ServiceProviderCollection), costEntry.Lookups.Suppliers);
		}

		public void TestShippingProvidersCollection_AIR()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			entry.TI_Mode = Core.Constants.RateMode.AIR;
			AssertType(typeof(AirShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertType(typeof(AirShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertType(typeof(AirShippingProviderCollection), entry.Lookups.ShippingProviders);
		}

		public void TestShippingProvidersCollection_SEA()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			entry.TI_Mode = Core.Constants.RateMode.SEA;
			AssertType(typeof(SeaShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.FCL;
			AssertType(typeof(SeaShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.LCL;
			AssertType(typeof(SeaShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.GRP;
			AssertType(typeof(SeaShippingProviderCollection), entry.Lookups.ShippingProviders);
		}

		public void TestShippingProvidersCollection_ROAD()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();

			entry.TI_Mode = Core.Constants.RateMode.ROA;
			AssertType(typeof(LocalTransportCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.LRO;
			AssertType(typeof(LocalTransportCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.FTL;
			AssertType(typeof(LocalTransportCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.FRO;
			AssertType(typeof(LocalTransportCollection), entry.Lookups.ShippingProviders);
		}

		public void TestShippingProvidersCollection_RAIL()
		{
			var entry = Factory.NewWithValidTestData<RateEntry>();
			entry.TI_Mode = Core.Constants.RateMode.RAI;

			AssertType(typeof(RailShippingProviderCollection), entry.Lookups.ShippingProviders);
			entry.TI_Mode = Core.Constants.RateMode.FRA;
			AssertType(typeof(RailShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.LRA;
			AssertType(typeof(RailShippingProviderCollection), entry.Lookups.ShippingProviders);

			entry.TI_Mode = Core.Constants.RateMode.FWL;
			AssertType(typeof(RailShippingProviderCollection), entry.Lookups.ShippingProviders);
		}

		public void TestDeliveryCapabilityWithAddressInactive()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testEntry = testRate.AddRateEntry(RatingConstants.RateCategory.DST);
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var delivery1 = supplier.Addresses.AddNew();
			delivery1.OA_Address1 = "Delivery1";
			delivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var delivery2 = supplier.Addresses.AddNew();
			delivery2.OA_Address1 = "Delivery2";
			delivery2.OA_IsActive = false;
			delivery2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			testEntry.TI_OH_Consignee = supplier.PK;
			Factory.Save();

			AssertEquals("Delivery capability should only appear if the address is active", true, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(delivery1));
			AssertEquals("Delivery capability should only appear if the address is active", false, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(delivery2));
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var transitWarehouse = Factory.New<IWhsWarehouse>();
			transitWarehouse.WW_WarehouseType = "TRW";

			var productWarehouse = Factory.New<IWhsWarehouse>();
			productWarehouse.WW_WarehouseType = "PRW";

			var containerYard = Factory.New<IWhsWarehouse>();
			containerYard.WW_WarehouseType = "CYD";

			var rateEntry = Factory.New<RateEntry>();
			rateEntry.TI_RateCategory = "TRW";
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)transitWarehouse },
				new RateEntryLookups(rateEntry).Warehouses.ToArray());

			rateEntry.TI_RateCategory = "WHS";
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)productWarehouse },
				new RateEntryLookups(rateEntry).Warehouses.ToArray());

			rateEntry.TI_RateCategory = "TRW";
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)transitWarehouse },
				new RateEntryLookups(rateEntry).Warehouses.ToArray());

			rateEntry.TI_RateCategory = "TWU";
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)transitWarehouse },
				new RateEntryLookups(rateEntry).Warehouses.ToArray());

			rateEntry.TI_RateCategory = "CYD";
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)containerYard },
				new RateEntryLookups(rateEntry).Warehouses.ToArray());
		}

		#endregion

		#region TestUnitSections

		public void TestContainerUnitSections()
		{
			var refUnitSection1 = Factory.New<RefUnitSection>();
			refUnitSection1.RUS_Code = "BL001";

			var refUnitSection2 = Factory.New<RefUnitSection>();
			refUnitSection2.RUS_Code = "BL001";

			var refUnitSection3 = Factory.New<RefUnitSection>();
			refUnitSection3.RUS_Code = "BY002";

			var rateEntry = Factory.New<RateEntry>();
			rateEntry.TI_RateCategory = "CYD";
			Assert(new RateEntryLookups(rateEntry).ContainerUnitSections.Count == 2);
			Assert(new RateEntryLookups(rateEntry).ContainerUnitSections.ToArray().Any(c => c.Code == "BY"));
			Assert(new RateEntryLookups(rateEntry).ContainerUnitSections.ToArray().Any(c => c.Code == "BL"));
		}

		#endregion

		#region TestGetTransportModesByTransportBookingType

		public void TestGetTransportModesByTransportBookingType()
		{
			var transportModes = RateEntryLookups.GetTransportModesByRateCategory(RatingConstants.RateCategory.TBC);
			AssertEquals(9, transportModes.Count);
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("ALL")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("ROA")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("LRO")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("FRO")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("RAI")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("LRA")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("FRA")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("FWL")));
			Assert(!string.IsNullOrEmpty(transportModes.GetDescriptionFromCode("FTL")));
		}

		#endregion

		#region HBL Delivery Mode

		public void TestAllHBLDeliveryModesInFilter()
		{
			// in rare case that we add new HBL Delivery Mode, we should update the list of HBL Delivery Modes
			var allHBLDeliveryModesInFilterStrip = RateEntryLookups.GetHBLDeliveryModeList(string.Empty, string.Empty).GetAllCodes().ToList();

			var fieldInfos = typeof(Core.Constants.HBLDeliveryModes.Codes).GetFields(System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
			var allHBLDeliveryModes = fieldInfos.Where(x => x.IsLiteral && !x.IsInitOnly).Select(x => (string)x.GetValue(null)).ToList();

			AssertContainsExactElementsInAnyOrder("The HBL Delivery Modes in filter should match the defined codes.", allHBLDeliveryModes, allHBLDeliveryModesInFilterStrip);
		}

		public void TestHBLDeliveryMode_AIR_FCL()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.AIR, RateMode.FCL);
		}

		public void TestHBLDeliveryMode_FCL_SEA()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.FCL, RateMode.SEA);
		}

		public void TestHBLDeliveryMode_FCL_ROA()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.FCL, RateMode.ROA);
		}

		public void TestHBLDeliveryMode_FCL_RAI()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.FCL, RateMode.RAI);
		}

		public void TestHBLDeliveryMode_DST_FRA()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.DST, RateMode.FRA);
		}

		public void TestHBLDeliveryMode_ORG_FRO()
		{
			CheckHBLDeliveryMode_FCL(RatingConstants.RateCategory.ORG, RateMode.FRO);
		}

		public void TestHBLDeliveryMode_ORG_AIR()
		{
			CheckHBLDeliveryMode_AIR(RatingConstants.RateCategory.ORG);
		}

		public void TestHBLDeliveryMode_DST_AIR()
		{
			CheckHBLDeliveryMode_AIR(RatingConstants.RateCategory.DST);
		}

		public void TestHBLDeliveryMode_ORG_LRA()
		{
			CheckHBLDeliveryMode_LCL(RatingConstants.RateCategory.ORG, RateMode.LRA);
		}

		public void TestHBLDeliveryMode_ORG_LRO()
		{
			CheckHBLDeliveryMode_LCL(RatingConstants.RateCategory.ORG, RateMode.LRO);
		}

		public void TestHBLDeliveryMode_DST_LRA()
		{
			CheckHBLDeliveryMode_LCL(RatingConstants.RateCategory.ORG, RateMode.LRA);
		}

		public void TestHBLDeliveryMode_DST_LRO()
		{
			CheckHBLDeliveryMode_LCL(RatingConstants.RateCategory.ORG, RateMode.LRO);
		}

		public void TestHBLDeliveryMode_LCL_LCL()
		{
			CheckHBLDeliveryMode_LCL(RatingConstants.RateCategory.LCL, RateMode.LCL);
		}

		void CheckHBLDeliveryMode_FCL(string rateCategory, string rateMode)
		{
			var org = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(org);

			var clientRateEntry = clientRate.AddRateEntry(rateCategory);
			clientRateEntry.TI_Mode = rateMode;

			var hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;

			AssertEquals(9, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/CY"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CY/CY"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CY/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CY/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/CY"));

			var collection = new HBLDeliveryModeCollection();
			collection.Add("AAA", (NoResString)"AAA - ShowInList-true");
			collection.Add("BBB", (NoResString)"BBB - ShowInList=true");
			collection.Add("CCC", (NoResString)"CCC - ShowInList=false", false);

			var rateKey = $"HBLDeliveryModeList{clientRateEntry.TI_RateCategory}{clientRateEntry.TI_Mode}";
			clientRate.Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			var hblDeliveryModes = new HBLDeliveryModes("FCL", collection);
			FreightDataRegistry.Instance.HBLDeliveryMode_FCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModes);

			hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;
			AssertEquals(2, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("AAA"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("BBB"));
		}

		void CheckHBLDeliveryMode_AIR(string rateCategory)
		{
			var org = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(org);

			var clientRateEntry = clientRate.AddRateEntry(rateCategory);
			clientRateEntry.TI_Mode = RateMode.AIR;

			var hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;

			AssertEquals(9, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/ARPT"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("ARPT/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("ARPT/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("ARPT/ARPT"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/ARPT"));

			var rateKey = $"HBLDeliveryModeList{clientRateEntry.TI_RateCategory}{clientRateEntry.TI_Mode}";
			clientRate.Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			var collectionLSE = new HBLDeliveryModeCollection();
			collectionLSE.Add("AAA", (NoResString)"AAA - ShowInList-true");
			collectionLSE.Add("BBB", (NoResString)"BBB - ShowInList=true");
			collectionLSE.Add("CCC", (NoResString)"CCC - ShowInList=false", false);

			var hblDeliveryModesLSE = new HBLDeliveryModes("LSE", collectionLSE);
			FreightDataRegistry.Instance.HBLDeliveryMode_LSE.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModesLSE);

			var collectionULD = new HBLDeliveryModeCollection();
			collectionULD.Add("DDD", (NoResString)"DDD - ShowInList-true");
			collectionULD.Add("EEE", (NoResString)"EEE - ShowInList=true");
			collectionULD.Add("FFF", (NoResString)"FFF - ShowInList=false", false);

			var hblDeliveryModesULD = new HBLDeliveryModes("ULD", collectionULD);
			FreightDataRegistry.Instance.HBLDeliveryMode_ULD.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModesULD);

			hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;
			AssertEquals(4, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("AAA"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("BBB"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DDD"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("EEE"));
		}

		void CheckHBLDeliveryMode_LCL(string rateCategory, string rateMode)
		{
			var org = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(org);

			var clientRateEntry = clientRate.AddRateEntry(rateCategory);
			clientRateEntry.TI_Mode = rateMode;

			var hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;

			AssertEquals(4, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/CFS"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("CFS/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/DOOR"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("DOOR/CFS"));

			var collection = new HBLDeliveryModeCollection();
			collection.Add("AAA", (NoResString)"AAA - ShowInList-true");
			collection.Add("BBB", (NoResString)"BBB - ShowInList=true");
			collection.Add("CCC", (NoResString)"CCC - ShowInList=false", false);

			var rateKey = $"HBLDeliveryModeList{clientRateEntry.TI_RateCategory}{clientRateEntry.TI_Mode}";
			clientRate.Factory.ClearCachedValue<CodeDescriptionPairList>(rateKey);

			var hblDeliveryModes = new HBLDeliveryModes("LCL", collection);
			FreightDataRegistry.Instance.HBLDeliveryMode_LCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModes);

			hblDeliveryModeList = clientRateEntry.Lookups.HBLDeliveryModeList;
			AssertEquals(2, hblDeliveryModeList.Count);

			AssertEquals(true, hblDeliveryModeList.ContainsCode("AAA"));
			AssertEquals(true, hblDeliveryModeList.ContainsCode("BBB"));
		}

		#endregion

		#region Locations

		public void TestLocations()
		{
			var org = Helper.NewOrgHeader();
			var stdCosting = Helper.NewCosting(null);
			var orgCosting = Helper.NewCosting(org);
			var clientRate = Helper.NewClientRate(org);

			AssertType("AIR standard costing has IATA locations", typeof(RatingIATASupportedLocationCollection), stdCosting.AddRateEntry(RatingConstants.RateCategory.AIR).Lookups.Locations);
			AssertType(typeof(RatingLocationCollection), stdCosting.AddRateEntry(RatingConstants.RateCategory.ORG).Lookups.Locations);
			AssertType(typeof(RatingLocationCollection), orgCosting.AddRateEntry(RatingConstants.RateCategory.AIR).Lookups.Locations);
			AssertType(typeof(RatingLocationCollection), clientRate.AddRateEntry(RatingConstants.RateCategory.AIR).Lookups.Locations);
		}

		public void TestLocationSourceOptions()
		{
			/**********************************************************************************
			 *  Before fixing this test think again if we need a data transformation or not!  *
			 **********************************************************************************/

			var actual = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.FCL)
				.Lookups
				.LocationSourceOptions
				.GetAllCodes();

			var expected = new[]
			{
				"1LD", // FirstLoad             refer to TI_FirstLoadLRC
				"LDC", // LastDischarge         refer to TI_LastDischargeLRC
				"MLD", // FirstRouteSetLoad     refer to TI_FirstRouteSetLoadPortLRC
				"MDC", // LastRouteSetDischarge refer to TI_LastRouteSetDischargePortLRC
			};

			AssertContainsExactElementsInAnyOrder("Location source options should match the expected codes.", expected, actual);
		}

		#endregion

		#region Helper

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}
