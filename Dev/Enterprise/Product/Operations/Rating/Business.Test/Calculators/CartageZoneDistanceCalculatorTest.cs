using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class CartageZoneDistanceCalculatorTest : CartageCalculatorsTestCase
	{
		public void TestCacheReset_UseACIZones()
		{
			var client = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);

			var calculator = line.GetCalculator<CartageZoneDistanceCalculator>();

			AssertEquals("By default ACI zones is off", false, calculator.UseACIZones);
			AssertEquals("This cached value should be false initially", false, line.UsesACIZones());
			calculator.Bool4 = true; // The UI checkbox for ACI zones is checked.
			AssertEquals("The checkbox on the UI for ACIZones was ticked.", true, calculator.UseACIZones);
			AssertEquals("This cached value for ACIZones should have updated also", true, line.UsesACIZones());
		}

		public void TestDeleteOrphanedRateLineItems_WhenChangingFromACIToTransportZone()
		{
			var aciZoneAU1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU1.F1_Zone = "ACIA1";
			aciZoneAU1.F1_RL_NKLoco = "AUSYD";

			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);

			var transportZoneSet1 = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "Z1", "Z2", "Z3" });
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "");
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;

			// The calculator makes many rateline items. In this test we're only concerned with
			// the ones with the below TM_Types
			var rateLineItemTypesForTest = new[] { "UNT", "MIN", "MAX" };

			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, "ACIA1");
			Factory.Save();

			var expectedRateLineItems = new[]
			{
				new { TM_Type = "UNT", TM_RelevantValue = 1m,  TM_F1Zone = "ACIA1" },
			};

			var preConditionActualItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_F1Zone}")
				.ToArray();

			var preConditionExpectedItems = expectedRateLineItems
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_F1Zone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Precondition: Check the created items are present",
				preConditionExpectedItems,
				preConditionActualItems
			);

			line.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();

			var postReloadActualItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_F1Zone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"After reloading the zones there should still be 1.",
				preConditionExpectedItems,
				postReloadActualItems
			);

			var cartageZoneActual = line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones
				.Select(x => (string)x.ZoneName)
				.ToArray();

			var cartageZoneExpected = new[] { ZString.Empty, aciZoneAU1.F1_Zone };

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default zone and ACIA1 and ACIA2",
				cartageZoneExpected,
				cartageZoneActual
			);

			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = false;
			Factory.Save(); // DeleteOrphanedZonesIfNeeded occurs on RateEntry.OnSaving

			var rateLineItemsPostSave = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.ToArray();

			AssertEquals(
				"There should be no RateLineItems as they got wiped on the reload",
				0,
				rateLineItemsPostSave.Length
			);

			var cartageZoneActualAfterDisable = line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones
				.Select(x => (string)x.ZoneName)
				.ToArray();

			var cartageZoneExpectedAfterDisable = new[] { string.Empty, "Z1", "Z2", "Z3" };

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default, and Z1, Z2, Z3",
				cartageZoneExpectedAfterDisable,
				cartageZoneActualAfterDisable
			);
		}

		public void TestDeleteOrphanedRateLineItems_WhenChangingFromTransportZoneToACI()
		{
			var aciZoneAU1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU1.F1_Zone = "ACIA1";
			aciZoneAU1.F1_RL_NKLoco = "AUSYD";

			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);

			var transportZoneSet1 = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "Z1", "Z2", "Z3" });
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "");
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = false;

			// The calculator makes many rateline items. In this test we're only concerned with
			// the ones with the below TM_Types
			var rateLineItemTypesForTest = new[] { "UNT", "MIN", "MAX" };

			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, transportZoneSet1.Zones[0].PK);
			Factory.Save();

			var expectedRateLineItems = new[]
			{
				new { TM_Type = "UNT", TM_RelevantValue = 1m, TM_TZ_DomesticZone = transportZoneSet1.Zones[0].PK },
			};

			var preconditionRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_TZ_DomesticZone}")
				.ToArray();

			var expectedRateLineItemsStrings = expectedRateLineItems
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Precondition: Check the created items are present",
				expectedRateLineItemsStrings,
				preconditionRateLineItems
			);

			line.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();

			var postReloadRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{i.TM_Type}|{i.TM_RelevantValue}|{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"After reloading the zones there should still be 1.",
				expectedRateLineItemsStrings,
				postReloadRateLineItems
			);

			var expectedCartageZones = new[]
			{
				ZGuid.Empty,
				transportZoneSet1.Zones[0].PK,
				transportZoneSet1.Zones[1].PK,
				transportZoneSet1.Zones[2].PK
			};

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default zone and Z1, Z2, Z3",
				expectedCartageZones,
				line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Select(x => x.ZonePK)
			);

			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;
			Factory.Save(); // DeleteOrphanedZonesIfNeeded occurs on RateEntry.OnSaving

			var postClearRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.ToArray();

			AssertEquals(
				"There should be no RateLineItems as they got wiped on the reload",
				0,
				postClearRateLineItems.Length
			);

			var expectedCartageZonesAfterACI = new[] { string.Empty, "ACIA1" };

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default, and ACIA1",
				expectedCartageZonesAfterACI,
				line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Select(x => (string)x.ZoneName)
			);
		}

		public void TestDeleteOrphanedRateLineItems_ForACIZones()
		{
			var aciZoneAU1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU1.F1_Zone = "ACIA1";
			aciZoneAU1.F1_RL_NKLoco = "AUSYD";
			var aciZoneAU2 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU2.F1_Zone = "ACIA2";
			aciZoneAU2.F1_RL_NKLoco = "AUSYD";

			var aciZoneUS1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneUS1.F1_Zone = "ACIU1";
			aciZoneUS1.F1_RL_NKLoco = "USNYC";

			Factory.Save();

			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "");
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;

			// The calculator makes many rateline items. In this test we're only concerned with
			// the ones with the below TM_Types
			var rateLineItemTypesForTest = new[] { "UNT", "MIN", "MAX" };

			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, "ACIA1");
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 2m, "ACIA1");
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 3m, "ACIA2");
			line.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, "ACIA2");
			line.Calculator.AddRateLineItemWithZone("MAX", 0m, 20m, "ACIA2");
			Factory.Save();

			var expectedRateLineItems = new[]
			{
				"UNT|1|ACIA1",
				"UNT|2|ACIA1",
				"UNT|3|ACIA2",
				"MIN|10|ACIA2",
				"MAX|20|ACIA2",
			};

			var actualRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{i.TM_RelevantValue}|{(string)i.TM_F1Zone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("Precondition: Check the created items are present.", expectedRateLineItems, actualRateLineItems);

			line.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();

			actualRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{i.TM_RelevantValue}|{(string)i.TM_F1Zone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("After reloading the zones there should still be 5.", expectedRateLineItems, actualRateLineItems);

			var expectedCartageZones = new[]
			{
				ZString.Empty,
				aciZoneAU1.F1_Zone,
				aciZoneAU2.F1_Zone
			};

			var actualCartageZones = line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones
				.Select(x => x.ZoneName)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("The CartageZone collection includes the blank default zone and ACIA1 and ACIA2.", expectedCartageZones, actualCartageZones);

			// now repeat in a different factory
			var factory2 = new BusinessObjectFactory();
			var loadedRateLine = factory2.Load<RateLine>(line.PK);

			actualRateLineItems = loadedRateLine.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{(int)i.TM_RelevantValue}|{(string)i.TM_F1Zone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("Since the zones have not loaded, there should still be five.", expectedRateLineItems, actualRateLineItems);

			// change the rateEntry so the ACIZone no longer applies.
			loadedRateLine.Parent.TI_OriginLRC = "USNYC"; // <- this causes a reload of the zones
			factory2.Save();

			expectedCartageZones = new[]
			{
				ZString.Empty,
				aciZoneUS1.F1_Zone
			};

			actualCartageZones = loadedRateLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones
				.Select(x => x.ZoneName)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("The CartageZone collection includes the blank default zone and ACIU1.", expectedCartageZones, actualCartageZones);

			loadedRateLine.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();

			actualRateLineItems = loadedRateLine.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{i.TM_RelevantValue}|{(string)i.TM_F1Zone}")
				.ToArray();

			AssertEquals("After re-loading the zones there should be none left.", 0, actualRateLineItems.Length);
		}

		public void TestDeleteOrphanedRateLineItems_ForTransportZone()
		{
			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();

			var transportZoneSet1 = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "Z1", "Z2", "Z3" });
			var transportZoneSet2 = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "G1", "G2" });

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "", removeLines: true);
			entry.TI_OH_Supplier = supplier.PK;
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);

			// The calculator makes many rateline items. In this test we're only concerned with
			// the ones with the below TM_Types
			var rateLineItemTypesForTest = new[] { "UNT", "MIN", "MAX" };

			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, transportZoneSet1.Zones[0].PK);
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 2m, transportZoneSet1.Zones[1].PK);
			line.Calculator.AddRateLineItemWithZone("UNT", 0m, 3m, transportZoneSet1.Zones[2].PK);
			line.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, transportZoneSet1.Zones[2].PK);
			line.Calculator.AddRateLineItemWithZone("MAX", 0m, 20m, transportZoneSet1.Zones[2].PK);
			Factory.Save();

			var expectedRateLineItems = new[]
			{
				"UNT|1|PK_" + transportZoneSet1.Zones[0].PK,
				"UNT|2|PK_" + transportZoneSet1.Zones[1].PK,
				"UNT|3|PK_" + transportZoneSet1.Zones[2].PK,
				"MIN|10|PK_" + transportZoneSet1.Zones[2].PK,
				"MAX|20|PK_" + transportZoneSet1.Zones[2].PK,
			};

			var actualRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{i.TM_RelevantValue}|PK_{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Precondition, check created items are there.",
				expectedRateLineItems,
				actualRateLineItems
			);

			line.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();

			actualRateLineItems = line.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{i.TM_RelevantValue}|PK_{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"After loading the zones there should still be 5.",
				expectedRateLineItems,
				actualRateLineItems
			);

			var expectedZonePKs = new[] { ZGuid.Empty, transportZoneSet1.Zones[0].PK, transportZoneSet1.Zones[1].PK, transportZoneSet1.Zones[2].PK };
			var actualZonePKs = line.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Select(x => x.ZonePK).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default zone and Z1, Z2 and Z3",
				expectedZonePKs,
				actualZonePKs
			);

			// change the transport zone to no longer apply.
			transportZoneSet1.TP_OH_RelatedParty = ZGuid.Empty;
			transportZoneSet1.TP_RN_NKCountry = CountryCodes.Iceland;
			Factory.Save();

			// now repeat in a different factory
			var factory2 = new BusinessObjectFactory();
			var loadedRatingHeader = factory2.Load<RatingHeader>(rate.PK);
			var loadedRateEntry = (RateEntry)loadedRatingHeader.ChildRateEntries.Single();
			var loadedRateLine = (RateLine)loadedRateEntry.RateLines.Single(rateLine => rateLine.PK == line.PK);

			actualRateLineItems = loadedRateLine.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{(int)i.TM_RelevantValue}|PK_{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"There should initially only be 5",
				expectedRateLineItems,
				actualRateLineItems
			);

			expectedZonePKs = [ZGuid.Empty, transportZoneSet2.Zones[0].PK, transportZoneSet2.Zones[1].PK];
			actualZonePKs = loadedRateLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Select(x => x.ZonePK).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The CartageZone collection includes the blank default zone and G1 and G2",
				expectedZonePKs,
				actualZonePKs
			);

			loadedRateLine.GetCalculator<CartageZoneDistanceCalculator>().ReloadCartageZones();
			actualRateLineItems = loadedRateLine.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{(int)i.TM_RelevantValue}|PK_{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"There should be 5",
				expectedRateLineItems,
				actualRateLineItems
			);

			loadedRateLine.Parent.TI_ContractNumber = "abc";
			factory2.Save(); // DeleteOrphanedZonesIfNeeded occurs on RateEntry.OnSaving

			actualRateLineItems = loadedRateLine.RateLineItems
				.Cast<RateLineItem>()
				.Where(i => rateLineItemTypesForTest.Contains((string)i.TM_Type))
				.Select(i => $"{(string)i.TM_Type}|{(int)i.TM_RelevantValue}|PK_{i.TM_TZ_DomesticZone}")
				.ToArray();

			AssertEquals(
				"After re-loading the zones there should be none left",
				0,
				actualRateLineItems.Length
			);
		}

		public void TestDeleteOrphanedZones_WhenRateEntrySupplierChanges()
		{
			var client = Helper.NewOrgHeader();
			var supplier = Helper.NewOrgHeader();
			var provider = Helper.CreateRateTransportZoneSet(supplier, CountryCodes.Australia, zoneNames: new ZString[] { "Z1", "Z2", "Z3" });
			var providerGeneric = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "G1", "G2" });

			var rate = Helper.NewClientRate(client);
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "");
			entry1.TI_OH_Supplier = supplier.PK;
			var line1 = entry1.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);

			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUMEL", "");
			entry2.TI_OH_Supplier = supplier.PK;
			var line2 = entry2.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			var line3 = entry2.AddRateLine("FRT", FlatCalculator.Code);

			var item1 = line1.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, provider.Zones[0].PK);
			var item2 = line1.Calculator.AddRateLineItemWithZone("UNT", 0m, 2m, provider.Zones[1].PK);
			var item3a = line1.Calculator.AddRateLineItemWithZone("UNT", 0m, 3m, provider.Zones[2].PK);
			var item3b = line1.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, provider.Zones[2].PK);
			var item3c = line1.Calculator.AddRateLineItemWithZone("MAX", 0m, 20m, provider.Zones[2].PK);

			line2.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, provider.Zones[0].PK);
			line2.Calculator.AddRateLineItemWithZone("UNT", 0m, 2m, provider.Zones[1].PK);
			line2.Calculator.AddRateLineItemWithZone("UNT", 0m, 3m, provider.Zones[2].PK);
			line2.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, provider.Zones[2].PK);
			line2.Calculator.AddRateLineItemWithZone("MAX", 0m, 20m, provider.Zones[2].PK);

			line3.GetCalculator<FlatCalculator>().BaseRate = 10;

			Factory.Save();
			var collection = new CartageZoneCollection(line1);
			collection.Load();

			AssertEquals(4, collection.Count);
			AssertEquals("The CartageZone collection includes the blank item", collection[0].ZonePK, ZGuid.Empty);
			AssertEquals("The CartageZone collection includes the Z1 item", collection[1].ZonePK, provider.Zones[0].PK);
			AssertEquals("The CartageZone collection includes the Z2 item", collection[2].ZonePK, provider.Zones[1].PK);
			AssertEquals("The CartageZone collection includes the Z3 item", collection[3].ZonePK, provider.Zones[2].PK);

			Assert("The Z1 cartage zone contains 1x UNT break", collection[1].ZoneRateLineItems.Contains(item1.PK));
			Assert("The Z2 cartage zone contains 1x UNT break", collection[2].ZoneRateLineItems.Contains(item2.PK));
			Assert("The Z3 cartage zone contains 1x UNT break", collection[3].ZoneRateLineItems.Contains(item3a.PK));
			Assert("The Z3 cartage zone contains 1x MIN break", collection[3].ZoneRateLineItems.Contains(item3b.PK));
			Assert("The Z3 cartage zone contains 1x MAX break", collection[3].ZoneRateLineItems.Contains(item3c.PK));

			var factory2 = new BusinessObjectFactory();
			var rateInNewFactory = factory2.Load<ClientRate>(rate.PK);
			var entry1InNewFactory = rateInNewFactory.AllEntries.Single(x => x.PK == entry1.PK);
			AssertEquals("PRE: all rate lines are loaded", 3, rateInNewFactory.AllEntries.Select(x => x.RateLines).Sum(x => x.Count));
			entry1InNewFactory.TI_OH_Supplier = ZGuid.Empty;
			factory2.Save();
			AssertEquals("Only one db hit on RateLineItems", 1, factory2.GetTableHitCount(AutoRateLineItems.Schema.TableName));
			var otherRateLines = rateInNewFactory.AllEntries.SelectMany(x => x.RateLines.Cast<RateLine>()).Where(x => x.PK != line1.PK);
			AssertEquals("Only load calculators for modified RateEntry", true, otherRateLines.All(x => !x.IsCalculatorInitialized));

			collection = new CartageZoneCollection(entry1InNewFactory.RateLines[0]);
			collection.Load();

			AssertEquals(3, collection.Count);
			AssertEquals("The CartageZone collection includes the <blank> item", collection[0].ZonePK, ZGuid.Empty);
			AssertEquals("The CartageZone collection includes the G1 item", collection[1].ZonePK, providerGeneric.Zones[0].PK);
			AssertEquals("The CartageZone collection includes the G2 item", collection[2].ZonePK, providerGeneric.Zones[1].PK);

			AssertNull("The previously-existing RateLineItems are now deleted", factory2.Load<RateLineItem>(item1.PK));
			AssertNull("The previously-existing RateLineItems are now deleted", factory2.Load<RateLineItem>(item2.PK));
			AssertNull("The previously-existing RateLineItems are now deleted", factory2.Load<RateLineItem>(item3a.PK));
			AssertNull("The previously-existing RateLineItems are now deleted", factory2.Load<RateLineItem>(item3b.PK));
			AssertNull("The previously-existing RateLineItems are now deleted", factory2.Load<RateLineItem>(item3c.PK));
		}

		public void TestAciZoneWithRestrictedReasons()
		{
			var zone1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			var zone2 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			var zone3 = Factory.NewWithValidTestData<RefDomesticCartageZone>();

			zone1.F1_Zone = "zone1";
			zone1.F1_RL_NKLoco = "AUSYD";
			zone2.F1_Zone = "zone2";
			zone2.F1_RL_NKLoco = "AUSYD";
			zone3.F1_Zone = "zone3";
			zone3.F1_RL_NKLoco = "AUSYD";

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			line.TL_RateCalculator = "CTZ";

			var calculator = line.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.UseACIZones = true;

			calculator.AddRateLineItemWithZone("-", 10, 100, "zone1");
			calculator.AddRateLineItemWithZone("+", 10, 110, "zone1");
			var restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone1");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Potato";

			calculator.AddRateLineItemWithZone("-", 10, 200, "zone2");
			calculator.AddRateLineItemWithZone("+", 10, 220, "zone2");
			restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone2");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Carrot";

			calculator.AddRateLineItemWithZone("-", 10, 300, "zone3");
			calculator.AddRateLineItemWithZone("+", 10, 330, "zone3");
			restricted = calculator.AddRateLineItemWithZone("+", 20, 0, "zone3");
			restricted.TM_CallForPricing = true;
			restricted.TM_Text = "Tomato";

			var current = calculator.RateLineItems
				.Cast<RateLineItem>()
				.Select(i => $"{i.TM_Type}|{(int)i.TM_Break}|{(int)i.TM_RelevantValue}|{i.TM_Text}|{i.TM_F1Zone}")
				.ToArray();

			var expected = new[]
			{
				"-|10|100||zone1",
				"+|10|110||zone1",
				"+|20|0|Potato|zone1",
				"-|10|200||zone2",
				"+|10|220||zone2",
				"+|20|0|Carrot|zone2",
				"-|10|300||zone3",
				"+|10|330||zone3",
				"+|20|0|Tomato|zone3"
			};

			AssertContainsExactElementsInAnyOrder(
				"The rate line items should match the expected collection",
				expected,
				current
			);
		}

		public void TestDeleteOrphanedZones_ForACIZones()
		{
			var aciZoneAU1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU1.F1_Zone = "ACIA1";
			aciZoneAU1.F1_RL_NKLoco = "AUSYD";
			var aciZoneAU2 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU2.F1_Zone = "ACIA2";
			aciZoneAU2.F1_RL_NKLoco = "AUSYD";
			var aciZoneAU3 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneAU3.F1_Zone = "ACIA3";
			aciZoneAU3.F1_RL_NKLoco = "AUSYD";

			var aciZoneUS1 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneUS1.F1_Zone = "ACIU1";
			aciZoneUS1.F1_RL_NKLoco = "USNYC";
			var aciZoneUS2 = Factory.NewWithValidTestData<RefDomesticCartageZone>();
			aciZoneUS2.F1_Zone = "ACIU2";
			aciZoneUS2.F1_RL_NKLoco = "USNYC";

			Factory.Save();

			var client = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			line.TL_RateCalculator = "CTZ";
			line.GetCalculator<CartageZoneDistanceCalculator>().UseACIZones = true;

			var item1 = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 1m, "ACIA1");
			var item2 = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 2m, "ACIA2");
			var item3a = line.Calculator.AddRateLineItemWithZone("UNT", 0m, 3m, "ACIA3");
			var item3b = line.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, "ACIA3");
			var item3c = line.Calculator.AddRateLineItemWithZone("MAX", 0m, 20m, "ACIA3");

			var collection = new CartageZoneCollection(line);
			collection.Load();

			AssertEquals(4, collection.Count);
			AssertEquals(collection[0].ZoneName, ZString.Empty);
			AssertEquals(1, collection.Cast<CartageZone>().Count(x => x.ZoneName == "ACIA1"));
			AssertEquals(1, collection.Cast<CartageZone>().Count(x => x.ZoneName == "ACIA2"));
			AssertEquals(1, collection.Cast<CartageZone>().Count(x => x.ZoneName == "ACIA3"));

			entry.TI_OriginLRC = "USNYC";
			collection.Load();

			AssertEquals(3, collection.Count);
			AssertEquals(collection[0].ZoneName, ZString.Empty);
			AssertEquals(1, collection.Cast<CartageZone>().Count(x => x.ZoneName == "ACIU1"));
			AssertEquals(1, collection.Cast<CartageZone>().Count(x => x.ZoneName == "ACIU2"));

			AssertEquals(false, item1.IsInDatabase);
			AssertEquals(false, item2.IsInDatabase);
			AssertEquals(false, item3a.IsInDatabase);
			AssertEquals(false, item3b.IsInDatabase);
			AssertEquals(false, item3c.IsInDatabase);
		}

		public void TestDeleteOrphanedZones_WhenEntryDeleted()
		{
			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");

			rateEntry.Delete();
			AssertNoExceptionThrown("Should not throw null exception when deleting orphaned zones from a deleted rate", () => CartageZoneDistanceCalculator.DeleteOrphanedZonesIfNeeded(rateEntry));
		}

		public void TestDeleteOrphanedZones_WhenEntryParentDeleted()
		{
			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");

			rate.Delete();
			AssertNoExceptionThrown("Should not throw null exception when deleting orphaned zones from a deleted rate", () => CartageZoneDistanceCalculator.DeleteOrphanedZonesIfNeeded(rateEntry));
		}

		protected override int NumberOfRateLineItemsAfterInitialization { get { return 6; } }

		public override void TestDocLineAmount()
		{
			var orgHeader = Helper.NewOrgHeader();
			var provider = Helper.CreateRateTransportZoneSet(orgHeader, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1", "Zone 2" });

			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX");
			rateEntry.TI_OH_Supplier = orgHeader.PK;

			var rateLine10 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 100, max: 109, flat: 101, perUnit: 102, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 103m, 104m, "KM", provider.Zones[0].PK), ("+", 5, 105m, 106m, "", provider.Zones[0].PK));
			var rateLine11 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 110, max: 119, flat: 111, perUnit: 112, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 113m, 114m, "KM", provider.Zones[0].PK), ("+", 5, 115m, 116m, "", provider.Zones[0].PK), ("+", 6, 117m, 118m, "", provider.Zones[0].PK));
			var rateLine12 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 120, max: 129, flat: 121, perUnit: 122, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 123m, 124m, "KM", provider.Zones[1].PK), ("+", 5, 125m, 126m, "", provider.Zones[1].PK));
			var rateLine13 = AddCartageRateLine(rateEntry, "FRT", "AUD", "KG", min: 130, max: 139, flat: 131, perUnit: 132, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 133m, 134m, "MI", provider.Zones[0].PK), ("+", 5, 135m, 136m, "", provider.Zones[0].PK));
			var rateLine14 = AddCartageRateLine(rateEntry, "FRT", "AUD", "M3", min: 140, max: 149, flat: 141, perUnit: 142, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 143m, 144m, "KM", provider.Zones[0].PK), ("+", 5, 145m, 146m, "", provider.Zones[0].PK));

			var rateLine20 = AddCartageRateLine(rateEntry, "FRT", "USD", "KG", min: 200, max: 209, flat: 201, perUnit: 202, equipmentType: FCLEquipmentNeeded.SideLoader, ("-", 5, 203m, 204m, "KM", provider.Zones[0].PK), ("+", 5, 205m, 206m, "", provider.Zones[0].PK));

			var docLineAmount = rateLine10.Calculator.GetDocLineAmount()
				+ rateLine11.Calculator.GetDocLineAmount()
				+ rateLine12.Calculator.GetDocLineAmount()
				+ rateLine13.Calculator.GetDocLineAmount()
				+ rateLine14.Calculator.GetDocLineAmount()
				+ rateLine20.Calculator.GetDocLineAmount();

			AssertQuotationLineList
			(
				docLineAmount,
				new[]
				{
					"Minimum|AUD|100.00|",
					"Standard Rate|AUD|605.00|",
					"Standard Rate|AUD|468.00|KG (1 M3 = 1000 KG)",
					"Maximum|AUD|149.00|",

					"Zone 1 - Less than 5 Kilometer(s)|AUD|216.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - Less than 5 Kilometer(s)|AUD|218.00|",
					"Zone 1 - 5 Kilometer(s) and above|AUD|105.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - 5 Kilometer(s) and above|AUD|106.00|",
					"Zone 1 - 5 Kilometer(s) to less than 6 Kilometer(s)|AUD|115.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - 5 Kilometer(s) to less than 6 Kilometer(s)|AUD|116.00|",
					"Zone 1 - 6 Kilometer(s) and above|AUD|117.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - 6 Kilometer(s) and above|AUD|118.00|",

					"Zone 2 - Less than 5 Kilometer(s)|AUD|123.00|per KG (1 M3 = 1000 KG)",
					"Zone 2 - Less than 5 Kilometer(s)|AUD|124.00|",
					"Zone 2 - 5 Kilometer(s) and above|AUD|125.00|per KG (1 M3 = 1000 KG)",
					"Zone 2 - 5 Kilometer(s) and above|AUD|126.00|",

					"Zone 1 - Less than 5 Mile(s)|AUD|133.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - Less than 5 Mile(s)|AUD|134.00|",
					"Zone 1 - 5 Mile(s) and above|AUD|135.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - 5 Mile(s) and above|AUD|136.00|",

					"Standard Rate|AUD|142.00|M3 / 1000 KG",
					"Zone 1 - Less than 5 Kilometer(s)|AUD|143.00|per M3 / 1000 KG",
					"Zone 1 - Less than 5 Kilometer(s)|AUD|144.00|",
					"Zone 1 - 5 Kilometer(s) and above|AUD|145.00|per M3 / 1000 KG",
					"Zone 1 - 5 Kilometer(s) and above|AUD|146.00|",

					"Minimum|USD|200.00|",
					"Standard Rate|USD|201.00|",
					"Standard Rate|USD|202.00|KG (1 M3 = 1000 KG)",
					"Maximum|USD|209.00|",
					"Zone 1 - Less than 5 Kilometer(s)|USD|203.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - Less than 5 Kilometer(s)|USD|204.00|",
					"Zone 1 - 5 Kilometer(s) and above|USD|205.00|per KG (1 M3 = 1000 KG)",
					"Zone 1 - 5 Kilometer(s) and above|USD|206.00|"
				}
			);
		}

		static RateLine AddCartageRateLine(RateEntry rateEntry, ZString chargeCode, string currency, string unit, decimal min, decimal max, decimal flat, decimal perUnit, string equipmentType, params (string breakType, int breakValue, decimal rate, decimal flat, string unit, ZGuid zone)[] breaks)
		{
			var rateLine = rateEntry.AddRateLine(chargeCode, CartageZoneDistanceCalculator.Code, unit, currency);
			var calculator = rateLine.GetCalculator<CartageZoneDistanceCalculator>();
			calculator.Minimum = min;
			calculator.Maximum = max;
			calculator.BaseRate = flat;
			calculator.PerUnit = perUnit;
			calculator.EquipmentType = equipmentType;

			foreach (var currentBreak in breaks)
			{
				calculator.AddRateLineItemWithZone(currentBreak.breakType, currentBreak.breakValue, currentBreak.rate, currentBreak.zone, currentBreak.flat, currentBreak.unit);
			}

			return rateLine;
		}

		public override void TestQuotationLines()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG);

			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1", "Zone 2", "Zone 3" });
			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.PerUnit = 45m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per M3 / 250 KG", quotationLines[0].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Charge|AUD|45.00|per M3 / 250 KG", quotationLines[0].ToString());

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 40m, provider.Zones[0].PK);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|AUD|45.00|per M3 / 250 KG", quotationLines[1].ToString());
			AssertEquals("Zone 1|AUD|40.00|per M3 / 250 KG", quotationLines[2].ToString());

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.UseChargeDescription, parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Charge|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|AUD|45.00|per M3 / 250 KG", quotationLines[1].ToString());
			AssertEquals("Zone 1|AUD|40.00|per M3 / 250 KG", quotationLines[2].ToString());

			TestCalculator.AddRateLineItemWithZone("-", 5m, 38m, provider.Zones[1].PK);
			TestCalculator.AddRateLineItemWithZone("+", 5m, 37m, provider.Zones[1].PK);
			TestCalculator.AddRateLineItemWithZone("+", 10m, 35.5m, provider.Zones[1].PK);
			Factory.Save();

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(7, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|AUD|45.00|per M3 / 250 KG", quotationLines[1].ToString());
			AssertEquals("Zone 1|AUD|40.00|per M3 / 250 KG", quotationLines[2].ToString());
			AssertEquals("Zone 2|||", quotationLines[3].ToString());
			AssertEquals("Less than 5 M3|AUD|38.00|per M3 / 250 KG", quotationLines[4].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|37.00|per M3 / 250 KG", quotationLines[5].ToString());
			AssertEquals("10 M3 and above|AUD|35.50|per M3 / 250 KG", quotationLines[6].ToString());

			TestCalculator.EquipmentType = Constants.LCLAIREquipmentNeeded.Premise;
			TestCalculator.BaseRate = 20m;
			TestCalculator.Minimum = 100m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 200m, provider.Zones[2].PK);
			Factory.Save();

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowEquipmentType, parentEntry);
			AssertEquals(11, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Premise Supplies Lift|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|||", quotationLines[1].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[2].ToString());
			AssertEquals("Base Rate|AUD|20.00|", quotationLines[3].ToString());
			AssertEquals("Per Unit|AUD|45.00|per M3 / 250 KG", quotationLines[4].ToString());
			AssertEquals("Zone 1|AUD|40.00|per M3 / 250 KG", quotationLines[5].ToString());
			AssertEquals("Zone 2|||", quotationLines[6].ToString());
			AssertEquals("Less than 5 M3|AUD|38.00|per M3 / 250 KG", quotationLines[7].ToString());
			AssertEquals("5 M3 to less than 10 M3|AUD|37.00|per M3 / 250 KG", quotationLines[8].ToString());
			AssertEquals("10 M3 and above|AUD|35.50|per M3 / 250 KG", quotationLines[9].ToString());
			AssertEquals("Zone 3|AUD|200.00|", quotationLines[10].ToString());
		}

		protected override void TestQuotationLinesWMCore()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var parentEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG);

			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1", "Zone 2", "Zone 3" });
			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";

			var quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate||Not Charged|", quotationLines[0].ToString());

			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			TestCalculator.PerUnit = 45m;
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(1, quotationLines.Count);
			AssertEquals("Test Rate|AUD|45.00|per W/M", quotationLines[0].ToString());

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 40m, provider.Zones[0].PK);
			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(3, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|AUD|45.00|per W/M", quotationLines[1].ToString());
			AssertEquals("Zone 1|AUD|40.00|per W/M", quotationLines[2].ToString());

			TestCalculator.AddRateLineItemWithZone("-", 5m, 38m, provider.Zones[1].PK);
			TestCalculator.AddRateLineItemWithZone("+", 5m, 37m, provider.Zones[1].PK);
			TestCalculator.AddRateLineItemWithZone("+", 10m, 35.5m, provider.Zones[1].PK);
			Factory.Save();

			quotationLines = TestCalculator.GetQuotationLines(parentEntry);
			AssertEquals(7, quotationLines.Count);
			AssertEquals("Test Rate|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|AUD|45.00|per W/M", quotationLines[1].ToString());
			AssertEquals("Zone 1|AUD|40.00|per W/M", quotationLines[2].ToString());
			AssertEquals("Zone 2|||", quotationLines[3].ToString());
			AssertEquals("Less than 5 W/M|AUD|38.00|per W/M", quotationLines[4].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|37.00|per W/M", quotationLines[5].ToString());
			AssertEquals("10 W/M and above|AUD|35.50|per W/M", quotationLines[6].ToString());

			TestCalculator.EquipmentType = Constants.LCLAIREquipmentNeeded.Premise;
			TestCalculator.BaseRate = 20m;
			TestCalculator.Minimum = 100m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 200m, provider.Zones[2].PK);
			Factory.Save();

			quotationLines = TestCalculator.GetQuotationLines(Calculator.GetQuotationLinesParam.ShowEquipmentType, parentEntry);
			AssertEquals(11, quotationLines.Count);
			AssertEquals(@"Test Rate 
-  Premise Supplies Lift|||", quotationLines[0].ToString());
			AssertEquals("Standard Rate|||", quotationLines[1].ToString());
			AssertEquals("Minimum|AUD|100.00|", quotationLines[2].ToString());
			AssertEquals("Base Rate|AUD|20.00|", quotationLines[3].ToString());
			AssertEquals("Per Unit|AUD|45.00|per W/M", quotationLines[4].ToString());
			AssertEquals("Zone 1|AUD|40.00|per W/M", quotationLines[5].ToString());
			AssertEquals("Zone 2|||", quotationLines[6].ToString());
			AssertEquals("Less than 5 W/M|AUD|38.00|per W/M", quotationLines[7].ToString());
			AssertEquals("5 W/M to less than 10 W/M|AUD|37.00|per W/M", quotationLines[8].ToString());
			AssertEquals("10 W/M and above|AUD|35.50|per W/M", quotationLines[9].ToString());
			AssertEquals("Zone 3|AUD|200.00|", quotationLines[10].ToString());
		}

		public void TestCalculation()
		{
			var defaultTransportProvider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, distances: new[] { 0, 10, 25, 50 });

			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			Criteria.PickupAddress = TestAddress;
			Criteria.WharfCTOAddress = BotanyAddress;

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 40m, ZGuid.Empty);
			var result = AssertCalculation(GetParameters(625m, 2m), 100m, "2.5 Cubic Meter(s) @ AUD 40.00/M3");
			AssertEquals("Standard", result.CartageZoneDescription);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.MIN, 0m, 120m, ZGuid.Empty);
			result = AssertCalculation(GetParameters(625m, 2m), 120m, "Minimum AUD 120.00");
			AssertEquals("Standard", result.CartageZoneDescription);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 2m, 25m, defaultTransportProvider.Zones[0].PK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 20m, defaultTransportProvider.Zones[0].PK);
			result = AssertCalculation(GetParameters(625m, 2m), 120m, "Minimum AUD 120.00");
			AssertEquals("Standard", result.CartageZoneDescription);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 2m, 35m, defaultTransportProvider.Zones[1].PK);
			var zone2Plus = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 2m, 30m, defaultTransportProvider.Zones[1].PK);
			Factory.Save();

			result = AssertCalculation(GetParameters(625m, 2m), 75m, "2.5 Cubic Meter(s) @ AUD 30.00/M3");
			AssertEquals("10 to 24", result.CartageZoneDescription);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 50m, defaultTransportProvider.Zones[1].PK);
			result = AssertCalculation(GetParameters(625m, 2m), 125m, "Base Rate AUD 50.00 + 2.5 Cubic Meter(s) @ AUD 30.00/M3");
			AssertEquals("10 to 24", result.CartageZoneDescription);

			zone2Plus.TM_BreakMinimum = 130m;
			AssertCalculation(GetParameters(625m, 2m), 130m, "Minimum AUD 130.00");
			AssertEquals("10 to 24", result.CartageZoneDescription);
			result = AssertCalculation(GetParameters(625m, 3m), 140m, "Base Rate AUD 50.00 + 3 Cubic Meter(s) @ AUD 30.00/M3");
			AssertEquals("10 to 24", result.CartageZoneDescription);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.MAX, 0m, 500m, defaultTransportProvider.Zones[1].PK);
			AssertCalculation(GetParameters(5000m, 30m), 500m, "Maximum AUD 500.00");
			AssertEquals("10 to 24", result.CartageZoneDescription);
		}

		public void TestCalculation_WithPostcode()
		{
			var cartageCo = Helper.NewOrgHeader();

			var provider = Helper.CreateRateTransportZoneSet(cartageCo, CountryCodes.Australia, zoneNames: new ZString[] { "CARTZ 1", "CARTZ 2", "CARTZ 3" });
			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.Parent.TI_OH_Supplier = cartageCo.PK;
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			Criteria.PickupAddress = TestAddress;
			Criteria.FreightMode = FreightMode.LCL;

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, provider.Zones[1].PK);
			Factory.Save();

			AssertCalculation(GetParameters(625m, 2m), "no transport zone being found for the given details.");

			Env.Registry.Rating.DefaultCTOAddressSea = "2220";
			AssertCalculation(GetParameters(625m, 2m), "no transport zone being found for the given details.");

			Env.Registry.Rating.DefaultCTOAddressSea = "99999";
			AssertCalculation(GetParameters(625m, 2m), "no transport zone being found for the given details.");

			var postCode1 = Helper.CreateRefPostCode("2100");
			var postCode2 = Helper.CreateRefPostCode("2200");
			provider.Zones[0].CreateRateTransportZoneItemForTest(postCode1, postCode2);

			Factory.Save();

			AssertCalculation(GetParameters(625m, 2m), 0m, "2.5 Cubic Meter(s) @ AUD 0.00/M3");

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 7m, provider.Zones[0].PK);
			Factory.Save();

			var result = AssertCalculation(GetParameters(625m, 2m), 17.5m, "2.5 Cubic Meter(s) @ AUD 7.00/M3");
			AssertEquals("TESTORG1 CARTZ 1", result.CartageZoneDescription);
		}

		public void TestCalculation_StandardZone()
		{
			var cartageCo = Helper.NewOrgHeader();
			var cartageCo2 = Helper.NewOrgHeader();

			Helper.CreateRateTransportZoneSet(cartageCo, CountryCodes.Australia, distances: new[] { 0, 30, 50 });
			Helper.CreateRateTransportZoneSet(cartageCo2, CountryCodes.Australia, distances: new[] { 0, 30, 50 });

			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.Parent.Parent.TH_OH = cartageCo2.PK;
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";
			Line.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);

			Criteria.PickupAddress = TestAddress;
			Criteria.FreightMode = FreightMode.LCL;

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, TestCalculator.CartageZones[0].ZonePK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, TestCalculator.CartageZones[1].ZonePK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, TestCalculator.CartageZones[2].ZonePK);
			Factory.Save();

			Env.Registry.Rating.DefaultCTOAddressSea = "2220";
			var result = AssertCalculation(GetParameters(625m, 2m), 50m, "2.5 Cubic Meter(s) @ AUD 20.00/M3"); //test fall backs
			AssertEquals("TESTORG2 0 to 29", result.CartageZoneDescription);

			Line.Parent.TI_OH_Supplier = cartageCo.PK;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, TestCalculator.CartageZones[0].ZonePK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 20m, TestCalculator.CartageZones[1].ZonePK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 30m, TestCalculator.CartageZones[2].ZonePK);
			Factory.Save();

			Env.Registry.Rating.DefaultCTOAddressSea = "2220";
			result = AssertCalculation(GetParameters(625m, 2m), 50m, "2.5 Cubic Meter(s) @ AUD 20.00/M3");

			AssertEquals("TESTORG1 0 to 29", result.CartageZoneDescription);

			Env.Registry.Rating.DefaultCTOAddressSea = "4000";
			result = AssertCalculation(GetParameters(625m, 2m), 25m, "2.5 Cubic Meter(s) @ AUD 10.00/M3");

			AssertEquals("Standard", result.CartageZoneDescription);
		}

		public void TestCalculation_Costing()
		{
			var cartageCo = Helper.NewOrgHeader();
			var provider = Helper.CreateRateTransportZoneSet(cartageCo, CountryCodes.Australia, zoneNames: new ZString[] { "CARTZ 1", "CARTZ 2", "CARTZ 3" });

			Criteria.PickupAddress = TestAddress;
			Criteria.FreightMode = FreightMode.LCL;

			var cost = Helper.NewCosting(cartageCo);
			var entry = cost.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AUSYD", "AUBNE");
			var costLine = entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			costLine.ConversionFactor = new ConversionFactor(250m, Constants.Weight.Kilograms, Constants.Volume.CubicMetres);
			costLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, provider.Zones[1].PK);
			Factory.Save();

			var (results, error) = costLine.Calculator.Calculate(GetParameters(625m, 2m));
			AssertEquals("Expected no calculation results when no transport zone is found.", 0, results.Count());
			AssertEquals("no transport zone being found for the given details.", error);

			Env.Registry.Rating.DefaultCTOAddressSea = "2220";
			(results, error) = costLine.Calculator.Calculate(GetParameters(625m, 2m));
			AssertEquals("Expected no calculation results when no transport zone is found.", 0, results.Count());
			AssertEquals("no transport zone being found for the given details.", error);

			Env.Registry.Rating.DefaultCTOAddressSea = "99999";
			(results, error) = costLine.Calculator.Calculate(GetParameters(625m, 2m));
			AssertEquals("Expected no calculation results when no transport zone is found.", 0, results.Count());
			AssertEquals("no transport zone being found for the given details.", error);

			var postCode1 = Helper.CreateRefPostCode("2100");
			var postCode2 = Helper.CreateRefPostCode("2200");
			provider.Zones[0].CreateRateTransportZoneItemForTest(postCode1, postCode2);

			Factory.Save();

			var result = costLine.Calculator.Calculate(GetParameters(625m, 2m)).results.Single();
			AssertEquals("Expected calculated amount to be 0m.", 0m, result.PaymentBases.Calculate().amount);
			AssertEquals("Expected description to match the format.", "2.5 Cubic Meter(s) @ AUD 0.00/M3", result.Description);

			costLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 7m, provider.Zones[0].PK);
			Factory.Save();

			result = costLine.Calculator.Calculate(GetParameters(625m, 2m)).results.Single();
			AssertEquals("Expected calculated amount to be 17.5m.", 17.5m, result.PaymentBases.Calculate().amount);
			AssertEquals("Expected description to match the format.", "2.5 Cubic Meter(s) @ AUD 7.00/M3", result.Description);
			AssertEquals("Expected cartage zone description to match.", "TESTORG1 CARTZ 1", result.CartageZoneDescription);
		}

		public void TestCalculation_ACIZones()
		{
			CreateZone("2125", "ZoneA", "AUSYD", "");
			CreateZone("2000", "ZoneB", "AUSYD", "");
			CreateZone("2000", "ZoneC", "AUSYD", "city1");
			Factory.Save();

			Entry.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = RatingConstants.Units.KG;

			var initialZones = TestCalculator.CartageZones
				.Select(z => z.ZoneName)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Initial zones should only contain the standard zone.",
				new[] { string.Empty },
				initialZones
			);

			TestCalculator.UseACIZones = true;

			var updatedZones = TestCalculator.CartageZones
				.Select(z => z.ZoneName)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Updated zones should include all defined zones and the standard zone.",
				new[] { string.Empty, "ZoneA", "ZoneB", "ZoneC" },
				updatedZones
			);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 2.5m, "ZoneA");
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 3.5m, "ZoneB");
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 4.5m, "ZoneC");
			Factory.Save();

			var ratingParams = GetParameters(625m, 2m);
			ratingParams.Criteria.PickupAddress = BotanyAddress;

			var result = AssertCalculation(ratingParams, 2187.5m, "625 Kilogram(s) @ AUD 3.50/KG");
			AssertEquals("The cartage zone description should match 'ZoneB'.", "ZoneB", result.CartageZoneDescription);

			BotanyAddress.OA_City = "city1";
			result = AssertCalculation(ratingParams, 2812.5m, "625 Kilogram(s) @ AUD 4.50/KG");
			AssertEquals("The cartage zone description should match 'ZoneC'.", "ZoneC", result.CartageZoneDescription);

			ratingParams.Criteria.PickupAddress = TestAddress;
			result = AssertCalculation(ratingParams, 1562.5m, "625 Kilogram(s) @ AUD 2.50/KG");
			AssertEquals("The cartage zone description should match 'ZoneA'.", "ZoneA", result.CartageZoneDescription);

			var unrelatedOrg = Factory.NewWithValidTestData<OrgHeader>();

			ratingParams.Criteria.PickupAddress = unrelatedOrg.MainAddress;
			AssertCalculation(ratingParams, "no zone being found for the given details.");
		}

		public void TestCalculation_ACIZones_OverriddenAddress()
		{
			CreateZone("2125", "ZoneA", "AUSYD", "");
			CreateZone("2000", "ZoneB", "AUSYD", "");
			CreateZone("2000", "ZoneC", "AUSYD", "city1");
			Factory.Save();

			Entry.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = RatingConstants.Units.KG;
			AssertEquals(1, TestCalculator.CartageZones.Count);

			TestCalculator.UseACIZones = true;
			AssertEquals(4, TestCalculator.CartageZones.Count);

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 2.5m, "ZoneA");
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 3.5m, "ZoneB");
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 4.5m, "ZoneC");
			Factory.Save();

			var address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_Postcode = "2000";

			var ratingParams = GetParameters(625m, 2m);
			ratingParams.Criteria.PickupAddress = address;

			ratingParams.Criteria.Origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");
			AssertCalculation(ratingParams, "no zone being found for the given details.");

			ratingParams.Criteria.Origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var result = AssertCalculation(ratingParams, 2187.5m, "625 Kilogram(s) @ AUD 3.50/KG");
			AssertEquals("ZoneB", result.CartageZoneDescription);

			address.E2_City = "city1";
			result = AssertCalculation(ratingParams, 2812.5m, "625 Kilogram(s) @ AUD 4.50/KG");
			AssertEquals("ZoneC", result.CartageZoneDescription);
		}

		public void TestValidateRateOperator()
		{
			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1", "Zone 2", "Zone 3", "Zone 4", "Zone 5" });
			Line.Parent.TI_Mode = RatingConstants.RateCategory.ORG;
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";

			var item1a = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 0m, 5m, ZGuid.Empty);
			AssertHasError(item1a.TM_TypeInfo, ErrorMessages.MoreLinesRequired);

			var item1b = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 0m, 4.5m, ZGuid.Empty);
			AssertNoErrors(item1b.TM_TypeInfo);
			item1a.Validation.ValidateTM_Type();
			AssertNoErrors(item1a.TM_TypeInfo);

			var item2a = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 6m, provider.Zones[0].PK);
			AssertNoErrors(item2a.TM_TypeInfo);
			item2a.TM_Type = Calculator.Items.Operator.Minus;
			AssertHasError(item2a.TM_TypeInfo, ErrorMessages.MoreLinesRequired);

			var item2b = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 5.5m, provider.Zones[0].PK);
			AssertHasError(item2b.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);
			item2b.TM_Type = Calculator.Items.Operator.Plus;
			AssertNoErrors(item2b.TM_TypeInfo);
			item2a.Validation.ValidateTM_Type();
			AssertNoErrors(item2a.TM_TypeInfo);

			var item3a = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 100m, provider.Zones[2].PK);
			AssertNoErrors(item3a.TM_TypeInfo);

			var item2c = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 0m, 4.75m, provider.Zones[0].PK);
			AssertNoErrors(item2c.TM_TypeInfo);

			var item4a = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 6m, provider.Zones[3].PK);
			AssertNoErrors(item4a.TM_TypeInfo);

			var item4b = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 0m, 6m, provider.Zones[3].PK);
			AssertHasError(item4b.TM_TypeInfo, ErrorMessages.MoreLinesRequired);
			AssertHasError(item4b.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			item4b.TM_Type = Calculator.Items.Operator.BAS;
			AssertNoErrors(item4b.TM_TypeInfo);

			var item5a = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 6m, provider.Zones[4].PK);
			AssertNoErrors(item5a.TM_TypeInfo);

			var item5b = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 0m, 6m, provider.Zones[4].PK);
			AssertHasError(item5b.TM_TypeInfo, ErrorMessages.UNTIncompatibleWithSlidingItems);

			item5b.TM_Type = Calculator.Items.Operator.BAS;
			AssertNoErrors(item5b.TM_TypeInfo);
		}

		public void TestWeightBreak()
		{
			var provider = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "Zone 1", "Zone 2", "Zone 3" });
			Line.Parent.TI_Mode = "LCL";
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.TL_WeightVolume = "M3";
			Line.TL_RX_NKCurrency = "AUD";

			var minusRateLineItem = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 10m, 0m, provider.Zones[0].PK);
			AssertNoErrors(minusRateLineItem.TM_BreakInfo);

			var firstPlusRateLineItem = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10m, 0m, provider.Zones[0].PK);
			AssertNoErrors(minusRateLineItem.TM_BreakInfo);
			AssertNoErrors(firstPlusRateLineItem.TM_BreakInfo);

			var secondPlusRateLineItem = TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10m, 0m, provider.Zones[0].PK);
			AssertHasError(secondPlusRateLineItem.TM_BreakInfo, ErrorMessages.WeightBreakIsTheSameAsAnotherPlusRate);

			secondPlusRateLineItem.TM_Break = 20m;
			firstPlusRateLineItem.Validation.ValidateTM_Break();
			AssertNoErrors(minusRateLineItem.TM_BreakInfo);
			AssertNoErrors(firstPlusRateLineItem.TM_BreakInfo);
			AssertNoErrors(secondPlusRateLineItem.TM_BreakInfo);
		}

		public override void TestGetCloneLineItems()
		{
			var cartageCo = Helper.NewOrgHeader();
			var provider = Helper.CreateRateTransportZoneSet(cartageCo, CountryCodes.Australia, zoneNames: new ZString[] { "CARTZ 1", "CARTZ 2", "CARTZ 3" });
			Line.TL_AC = Helper.ChargeCodes["OCART"].PK;
			Line.Parent.TI_Mode = Core.Constants.RateMode.LCL;
			Line.Parent.TI_OriginLRC = "AUSYD";
			Line.Parent.TI_OH_Supplier = cartageCo.PK;
			AssertEquals(4, TestCalculator.CartageZones.Count);

			TestCalculator.EquipmentType = Constants.LCLAIREquipmentNeeded.HandHaulier;

			TestCalculator.Line.ViewAgentRates = false;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0m, 10m, provider.Zones[0].PK).TM_AgentDeclaredRate = 15m;

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0m, 60m, provider.Zones[1].PK).TM_AgentDeclaredRate = 80m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 10m, 5m, provider.Zones[1].PK).TM_AgentDeclaredRate = 10m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 10m, 4m, provider.Zones[1].PK).TM_AgentDeclaredRate = 8m;

			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.MIN, 0m, 80m, provider.Zones[2].PK).TM_AgentDeclaredRate = 100m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 20m, 7m, provider.Zones[2].PK).TM_AgentDeclaredRate = 10m;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 20m, 6m, provider.Zones[2].PK).TM_AgentDeclaredRate = 8m;
			Factory.Save();

			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "AUSYD", "");
			entry.TI_OH_Supplier = cartageCo.PK;

			var companyTariffLine = entry.AddRateLine("OCART", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			var companyTariffCalc = (CompanyTariffOrCostBasedCalculator)companyTariffLine.Calculator;
			companyTariffCalc.PerUnit = 6m;
			companyTariffCalc.Minimum = 100m;
			Factory.Save();

			var cloneHelper = new CompanyTariffOrCostLineCloneHelper(companyTariffLine);
			var clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);

			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().EquipmentType);
			AssertEquals(4, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Count);
			AssertEquals(TestCalculator.CartageZones[0].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[0].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[1].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[1].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[2].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[2].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[3].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[3].ZoneRateLineItems.Count);

			clonedLine.GetCalculator<CartageZoneDistanceCalculator>().Line.ViewAgentRates = false;
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.UNT, provider.Zones[0].PK, 0m));

			AssertEquals(60m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[1].PK, 0m));
			AssertEquals(11m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[1].PK, 10m));
			AssertEquals(10m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[1].PK, 10m));

			AssertEquals(180m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.MIN, provider.Zones[2].PK, 0m));
			AssertEquals(13m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[2].PK, 20m));
			AssertEquals(12m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[2].PK, 20m));

			clonedLine.GetCalculator<CartageZoneDistanceCalculator>().Line.ViewAgentRates = true;
			AssertEquals(21m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.UNT, provider.Zones[0].PK, 0m));

			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[1].PK, 0m));
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[1].PK, 10m));
			AssertEquals(14m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[1].PK, 10m));

			AssertEquals(200m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.MIN, provider.Zones[2].PK, 0m));
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[2].PK, 20m));
			AssertEquals(14m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[2].PK, 20m));

			companyTariffCalc.Minimum = 0m;
			companyTariffCalc.BaseRate = 80m;
			clonedLine = cloneHelper.CreateClone(Line, Factory, RateLineItem.RateTypeToUpdate.StandardAndAgent);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().EquipmentType);
			AssertEquals(4, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones.Count);
			AssertEquals(TestCalculator.CartageZones[0].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[0].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[1].ZoneRateLineItems.Count + 1, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[1].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[2].ZoneRateLineItems.Count, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[2].ZoneRateLineItems.Count);
			AssertEquals(TestCalculator.CartageZones[3].ZoneRateLineItems.Count + 1, clonedLine.GetCalculator<CartageZoneDistanceCalculator>().CartageZones[3].ZoneRateLineItems.Count);

			clonedLine.GetCalculator<CartageZoneDistanceCalculator>().Line.ViewAgentRates = false;
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.UNT, provider.Zones[0].PK, 0m));
			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[0].PK, 0m));

			AssertEquals(140m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[1].PK, 0m));
			AssertEquals(11m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[1].PK, 10m));
			AssertEquals(10m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[1].PK, 10m));

			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.MIN, provider.Zones[2].PK, 0m));
			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[2].PK, 0m));
			AssertEquals(13m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[2].PK, 20m));
			AssertEquals(12m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[2].PK, 20m));

			clonedLine.GetCalculator<CartageZoneDistanceCalculator>().Line.ViewAgentRates = true;
			AssertEquals(21m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.UNT, provider.Zones[0].PK, 0m));
			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[0].PK, 0m));

			AssertEquals(160m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[1].PK, 0m));
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[1].PK, 10m));
			AssertEquals(14m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[1].PK, 10m));

			AssertEquals(100m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.MIN, provider.Zones[2].PK, 0m));
			AssertEquals(80m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.BAS, provider.Zones[2].PK, 0m));
			AssertEquals(16m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Minus, provider.Zones[2].PK, 20m));
			AssertEquals(14m, GetCartageZoneValue(clonedLine.GetCalculator<CartageZoneDistanceCalculator>(), Calculator.Items.Operator.Plus, provider.Zones[2].PK, 20m));
		}

		public void TestNoExceptionThrownWhenUseACIZonesAndInternationalZones()
		{
			var refZoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			refZoneHeader.FZ_Code = "NOOO";
			refZoneHeader.FZ_Description = "A Descriptive Description";

			Line.Parent.TI_OriginLRC = "NOOO";
			AssertNoExceptionThrown(() => TestCalculator.UseACIZones = true);

			TestCalculator.UseACIZones = false;
			Line.Parent.TI_OriginLRC = "";
			Line.Parent.TI_RateCategory = "DST";
			Line.Parent.TI_DestinationLRC = "NOOO";
			AssertNoExceptionThrown(() => TestCalculator.UseACIZones = true);
		}

		#region Cartage Address

		public void TestGetCartageAddress()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, "LCL", "AU", "").AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			var rateLine2 = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, "LCL", "", "AU").AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null);

			var calc1 = rateLine1.GetCalculator<CartageZoneDistanceCalculator>();
			var calc2 = rateLine2.GetCalculator<CartageZoneDistanceCalculator>();

			var (pickupOrDeliveryAddress1, _) = calc1.GetCartageAddressPair(criteria);
			AssertEquals(null, pickupOrDeliveryAddress1);
			var (pickupOrDeliveryAddress2, _) = calc2.GetCartageAddressPair(criteria);
			AssertEquals(null, pickupOrDeliveryAddress2);

			criteria.PickupAddress = Helper.NewOrgHeader().MainAddress;
			criteria.DeliveryAddress = Helper.NewOrgHeader().MainAddress;

			(pickupOrDeliveryAddress1, _) = calc1.GetCartageAddressPair(criteria);
			AssertEquals(criteria.PickupAddress, pickupOrDeliveryAddress1);
			(pickupOrDeliveryAddress2, _) = calc2.GetCartageAddressPair(criteria);
			AssertEquals(criteria.DeliveryAddress, pickupOrDeliveryAddress2);

			rateLine1.TL_AC = Helper.ChargeCodes.New("TORIGBR", "Test Origin Brokerage", UnitCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage).PK;
			rateLine2.TL_AC = Helper.ChargeCodes["CCLR"].PK;

			(pickupOrDeliveryAddress1, _) = calc1.GetCartageAddressPair(criteria);
			AssertEquals(criteria.PickupAddress, pickupOrDeliveryAddress1);
			(pickupOrDeliveryAddress2, _) = calc2.GetCartageAddressPair(criteria);
			AssertEquals(criteria.DeliveryAddress, pickupOrDeliveryAddress2);
		}

		public void TestGetCartageAddress_LocalTransport()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry(RatingConstants.RateCategory.TBC, "LCL", "AUSYD", "").AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, Constants.Volume.CubicMetres);
			var calc = line.GetCalculator<CartageZoneDistanceCalculator>();

			var criteria = new TestRatingCriteria("", "", FreightMode.LCL, 50M, 0.5M, null)
			{
				RateTypeToUse = RateType.TransportBookings,
				FreightMode = FreightMode.FRO
			};

			var (pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals(null, pickupOrDeliveryAddress);

			var ctoDocAddress = Factory.New<JobDocAddress>();
			ctoDocAddress.DocAddressType = DocAddressType.LocalCartageCTO;
			ctoDocAddress.E2_OA_Address = Helper.NewOrgHeader().MainAddress.PK;

			var cto2DocAddress = Factory.New<JobDocAddress>();
			cto2DocAddress.DocAddressType = DocAddressType.LocalCartageCTO;
			cto2DocAddress.E2_OA_Address = Helper.NewOrgHeader().MainAddress.PK;

			var cnrDocAddress = Factory.New<JobDocAddress>();
			cnrDocAddress.DocAddressType = DocAddressType.LocalCartageExporter;
			cnrDocAddress.E2_OA_Address = Helper.NewOrgHeader().MainAddress.PK;

			var cneDocAddress = Factory.New<JobDocAddress>();
			cneDocAddress.DocAddressType = DocAddressType.LocalCartageImporter;
			cneDocAddress.E2_OA_Address = Helper.NewOrgHeader().MainAddress.PK;

			var cfsDocAddress = Factory.New<JobDocAddress>();
			cfsDocAddress.DocAddressType = DocAddressType.LocalCartageCFS;
			cfsDocAddress.E2_OA_Address = Helper.NewOrgHeader().MainAddress.PK;

			// CTO -> CNE = Delivery Address
			criteria.PickupAddress = ctoDocAddress;
			criteria.DeliveryAddress = cneDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CTO->CNE = Delivery Address(CNE).", criteria.DeliveryAddress, pickupOrDeliveryAddress);

			// CNR -> CTO = Pickup Address
			criteria.PickupAddress = cnrDocAddress;
			criteria.DeliveryAddress = ctoDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CNR->CTO = Pickup Address(CNR).", criteria.PickupAddress, pickupOrDeliveryAddress);

			// CTO -> CTO2 = Delivery Address
			criteria.PickupAddress = ctoDocAddress;
			criteria.DeliveryAddress = cto2DocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CTO->CTO = Delivery Address(CNR).", criteria.DeliveryAddress, pickupOrDeliveryAddress);

			// CTO -> CFS = Delivery Address ( Implies an Import )
			criteria.PickupAddress = ctoDocAddress;
			criteria.DeliveryAddress = cfsDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CNR->CTO = Delivery Address(CNR).", criteria.DeliveryAddress, pickupOrDeliveryAddress);

			// CFS -> CTO = Pickup Address ( Implies Export )
			criteria.PickupAddress = cfsDocAddress;
			criteria.DeliveryAddress = ctoDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CFS->CTO = Pickup Address(CFS).", criteria.PickupAddress, pickupOrDeliveryAddress);

			// CFS -> CNE = Delivery Address
			criteria.PickupAddress = cfsDocAddress;
			criteria.DeliveryAddress = cneDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CFS->CNE = Delivery Address(CNE).", criteria.DeliveryAddress, pickupOrDeliveryAddress);

			// CNR -> CFS = Pickup Address
			criteria.PickupAddress = cnrDocAddress;
			criteria.DeliveryAddress = cfsDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CNR->CFS = Pickup Address(CNR).", criteria.PickupAddress, pickupOrDeliveryAddress);

			// CNR -> CNE = Delivery Address
			criteria.PickupAddress = cnrDocAddress;
			criteria.DeliveryAddress = cneDocAddress;
			(pickupOrDeliveryAddress, _) = calc.GetCartageAddressPair(criteria);
			AssertEquals("Expected CNR->CNE = Delivery Address(CNE).", criteria.DeliveryAddress, pickupOrDeliveryAddress);
		}

		#endregion

		public void TestGetTransportZone_SupportsCrossBorderTransportZone()
		{
			var brussels = Helper.GetCityTown("Brussels", "BRU", CountryCodes.Belgium);
			var hamburg = Helper.GetCityTown("Hamburg", "HH", CountryCodes.Germany);
			var amsterdam = Helper.GetCityTown("Amsterdam", "", CountryCodes.Netherlands);

			var zoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Germany);

			var belgiumZone = zoneSet.CreateRateTransportZoneForTest("Belgium Zone");
			belgiumZone.CreateRateTransportZoneItemForTest(brussels);

			var germanZone = zoneSet.CreateRateTransportZoneForTest("German Zone");
			germanZone.CreateRateTransportZoneItemForTest(hamburg);

			var dutchZone = zoneSet.CreateRateTransportZoneForTest("Dutch Zone");
			dutchZone.CreateRateTransportZoneItemForTest(amsterdam);

			Entry.TI_OriginLRC = "DE";
			Line.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 11m, belgiumZone.PK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 22m, germanZone.PK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 33m, dutchZone.PK);

			Factory.Save();

			var expected = new[] { "", "Belgium Zone", "German Zone", "Dutch Zone" };
			var zones = TestCalculator.CartageZones.Cast<CartageZone>().Select(x => x.ZoneName);
			AssertContainsExactElementsInAnyOrder("Pre-condition", expected, zones);

			var criteria = Helper.CreateRatingCriteria("DEHAM", "AUSYD");
			criteria.PickupAddress = CreateAddress(brussels, "BEBRU");
			AssertCalculation(criteria, 11m, "Base Rate EUR 11.00");

			criteria.PickupAddress = CreateAddress(hamburg, "DEHAM");
			AssertCalculation(criteria, 22m, "Base Rate EUR 22.00");

			criteria.PickupAddress = CreateAddress(amsterdam, "NLAMS");
			AssertCalculation(criteria, 33m, "Base Rate EUR 33.00");
		}

		public void TestGetTransportZone_SupportsCrossBorderTransportZoneWithInternationalZones_Destination()
		{
			var euroInternationalZone = Helper.NewInternationalZone("EURO", null, CountryCodes.Germany, CountryCodes.Switzerland);
			var belgiumCityTown = Helper.GetCityTown("Brussels", "BRU", CountryCodes.Belgium);

			var germanZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Germany);
			var germanZone = germanZoneSet.CreateRateTransportZoneForTest("Not ze Zone");
			germanZone.CreateRateTransportZoneItemForTest(belgiumCityTown);

			var swissZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Switzerland);
			var swissZone = swissZoneSet.CreateRateTransportZoneForTest("Neutral Zone");
			swissZone.CreateRateTransportZoneItemForTest(belgiumCityTown);

			Entry.TI_RateCategory = RatingConstants.RateCategory.DST;
			Entry.TI_DestinationLRC = "EURO";
			ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 111m, swissZone.PK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 222m, germanZone.PK);

			Factory.Save();

			var expected = new[] { "", "Not ze Zone", "Neutral Zone" };
			var zones = TestCalculator.CartageZones.Cast<CartageZone>().Select(x => x.ZoneName);
			AssertContainsExactElementsInAnyOrder("Pre-condition", expected, zones);

			var criteria = Helper.CreateRatingCriteria("AUSYD", "CHBSL");
			criteria.DeliveryAddress = CreateAddress(belgiumCityTown, "BEAAB");
			AssertCalculation(criteria, 111m, "Base Rate AUD 111.00");

			criteria.Destination = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM");
			AssertCalculation(criteria, 222m, "Base Rate AUD 222.00");
		}

		public void TestGetTransportZone_SupportsCrossBorderTransportZoneWithInternationalZones_Origin()
		{
			var euroInternationalZone = Helper.NewInternationalZone("EURO", null, CountryCodes.Germany, CountryCodes.Switzerland);
			var belgiumCityTown = Helper.GetCityTown("Brussels", "BRU", CountryCodes.Belgium);

			var germanZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Germany);
			var germanZone = germanZoneSet.CreateRateTransportZoneForTest("Not ze Zone");
			germanZone.CreateRateTransportZoneItemForTest(belgiumCityTown);

			var swissZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Switzerland);
			var swissZone = swissZoneSet.CreateRateTransportZoneForTest("Neutral Zone");
			swissZone.CreateRateTransportZoneItemForTest(belgiumCityTown);

			Entry.TI_RateCategory = RatingConstants.RateCategory.ORG;
			Entry.TI_OriginLRC = "EURO";
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 111m, swissZone.PK);
			TestCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.BAS, 0, 222m, germanZone.PK);

			Factory.Save();

			var expected = new[] { "", "Not ze Zone", "Neutral Zone" };
			var zones = TestCalculator.CartageZones.Cast<CartageZone>().Select(x => x.ZoneName);
			AssertContainsExactElementsInAnyOrder("Pre-condition", expected, zones);

			var criteria = Helper.CreateRatingCriteria("CHBSL", "AUSYD");
			criteria.PickupAddress = CreateAddress(belgiumCityTown, "BEAAB");
			AssertCalculation(criteria, 111m, "Base Rate AUD 111.00");

			criteria.Origin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "DEHAM");
			AssertCalculation(criteria, 222m, "Base Rate AUD 222.00");
		}

		#region Break Unit

		public void TestBreakUnitIsReadOnly()
		{
			var zoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Australia);
			var zone1 = zoneSet.CreateRateTransportZoneForTest("0-24");
			zone1.CreateRateTransportZoneItemForTest(0, 24);
			var zone2 = zoneSet.CreateRateTransportZoneForTest("25-99");
			zone2.CreateRateTransportZoneItemForTest(25, 99);

			Factory.Save();

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AU", "");
			var rateLine = rateEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, Constants.PkgUnit.Pallet);

			var standardMinusItem = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 45, 100, ZGuid.Empty);
			var standardPlusItem1 = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 45, 90, ZGuid.Empty);

			var zone1MinusItem = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Minus, 20, 99, zone1.PK);
			var zone1PlusItem1 = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 20, 89, zone1.PK);
			var zone1PlusItem2 = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 50, 79, zone1.PK);

			var zone2PlusItem = rateLine.Calculator.AddRateLineItemWithZone(Calculator.Items.Operator.Plus, 50, 90, zone2.PK);

			CombineAssertions("Top level pack (e.g. PLT) chargeable units allow any valid break units on the first plus/minus item", () =>
			{
				AssertEquals(false, standardMinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, standardPlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(false, zone1MinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem2.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(false, zone2PlusItem.TM_BreakWeightVolumeInfo.ReadOnly);
			});

			rateLine.TL_WeightVolume = QuantityUnit.KM;

			CombineAssertions("Distance chargeable units should not allow any break units on the first plus/minus item", () =>
			{
				AssertEquals(true, standardMinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, standardPlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(true, zone1MinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem2.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(true, zone2PlusItem.TM_BreakWeightVolumeInfo.ReadOnly);
			});

			rateLine.TL_WeightVolume = QuantityUnit.KG;

			CombineAssertions("Other chargeable units allows any valid break units on the first plus/minus item", () =>
			{
				AssertEquals(false, standardMinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, standardPlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(false, zone1MinusItem.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem1.TM_BreakWeightVolumeInfo.ReadOnly);
				AssertEquals(true, zone1PlusItem2.TM_BreakWeightVolumeInfo.ReadOnly);

				AssertEquals(false, zone2PlusItem.TM_BreakWeightVolumeInfo.ReadOnly);
			});
		}

		#endregion

		#region Implementation

		public override void TestCheckOrCreateItems()
		{
			AssertNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));
			AssertNull(Line.RateLineItems.FindByTM_Type(CartageZoneDistanceCalculator.Items.ACIZoneData));

			base.TestCheckOrCreateItems();

			AssertNotNull(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType));
			AssertNotNull(Line.RateLineItems.FindByTM_Type(CartageZoneDistanceCalculator.Items.ACIZoneData));

			AssertEquals(Line.RateLineItems.FindByTM_Type(CartageCalculator.Items.EquipmentType).TM_TextInfo, TestCalculator.String1Info);
			AssertEquals(Line.TL_ConversionFactorStringInfo, TestCalculator.String2Info);
			AssertEquals(Line.RateLineItems.FindByTM_Type(CartageZoneDistanceCalculator.Items.ACIZoneData).TM_TextInfo, TestCalculator.Bool4Info);
		}

		public override void TestMapping()
		{
			base.TestMapping();
			TestMapping(CartageZoneDistanceCalculator.Items.ACIZoneData, "Bool4");
		}

		RefDomesticCartageZone CreateZone(ZString postCode, ZString zoneName, ZString loco, ZString city)
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, loco);
			var iata = unloco != null ? unloco.RL_IATA : ZString.Empty;

			var zone = Factory.New<RefDomesticCartageZone>();
			zone.F1_CityTownPostCode = postCode;
			zone.F1_Zone = zoneName;
			zone.F1_RL_NKLoco = loco;
			zone.F1_CityTown = city;
			zone.F1_PortCode = iata;

			return zone;
		}

		OrgAddress CreateAddress(RefCityTown cityTown, string unloco)
		{
			var address = Helper.NewOrgHeader().MainAddress;
			address.OA_Address1 = $"Somewhere in {cityTown.R9_InternationalName}";
			address.OA_RN_NKCountryCode = cityTown.R9_RN_NKCountry;
			address.OA_City = cityTown.R9_InternationalName;
			address.OA_State = cityTown.R9_RW_NKState;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.PickupAndDelivery.Code);
			address.ClosestPort = unloco;

			return address;
		}

		protected override Type CalculatorType
		{
			get { return typeof(CartageZoneDistanceCalculator); }
		}

		protected override string CalculatorCode
		{
			get { return CartageZoneDistanceCalculator.Code; }
		}

		new CartageZoneDistanceCalculator TestCalculator
		{
			get { return (CartageZoneDistanceCalculator)base.TestCalculator; }
		}

		OrgAddress TestAddress
		{
			get
			{
				if (fAddress == null)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_RL_NKClosestPort = "AUSYD";
					org.OH_Code = "MCLAREN";
					fAddress = org.MainAddress;
					fAddress.OA_PostCode = "2125";
				}

				return fAddress;
			}
		}

		OrgAddress fAddress;

		OrgAddress BotanyAddress
		{
			get
			{
				if (fBotanyAddress == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_RL_NKClosestPort = "AUSYD";
					org.OH_Code = "ManUtd";
					fBotanyAddress = org.MainAddress;
					fBotanyAddress.OA_PostCode = "2000";
				}

				return fBotanyAddress;
			}
		}

		OrgAddress fBotanyAddress;

		static ZDecimal GetCartageZoneValue(CartageZoneDistanceCalculator calculator, ZString type, ZGuid zonePK, ZDecimal breakAmount)
		{
			var zone = calculator.CartageZones.FindZone(zonePK);
			var item = zone != null
				? zone.ZoneRateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.TM_Type == type && x.TM_Break == breakAmount)
				: null;

			return item != null ? item.TM_RelevantValue : ZDecimal.Zero;
		}

		#endregion
	}
}
