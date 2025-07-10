using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(BulkRateUpdater))]
	public class BulkRateUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerTypes_WhenModifyingTypeFromContainerisedToNonContainerised()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.FCL;
			bulkRateUpdater.Mode = Mode.SEA;

			var containerType20GP = bulkRateUpdater.ContainerTypes.AddNew();
			containerType20GP.RC_Code = "20GP";
			AssertContainerTypes("Containerised FCL-SEA", bulkRateUpdater.ContainerTypes, expectedReadOnly: false, expectedContainerTypes: new[] { "20GP" });

			bulkRateUpdater.Mode = Mode.ROA;
			AssertContainerTypes("Containerised FCL-ROA", bulkRateUpdater.ContainerTypes, expectedReadOnly: false, expectedContainerTypes: new[] { "20GP" });

			bulkRateUpdater.Type = Category.AIR;
			AssertContainerTypes("NonContainerised AIR-ROA", bulkRateUpdater.ContainerTypes, expectedReadOnly: true, expectedContainerTypes: Array.Empty<string>());

			bulkRateUpdater.Mode = Mode.LSE;
			AssertContainerTypes("NonContainerised AIR-LSE", bulkRateUpdater.ContainerTypes, expectedReadOnly: true, expectedContainerTypes: Array.Empty<string>());

			void AssertContainerTypes(string message, BulkRateUpdaterContainerTypeCollection containerTypes, bool expectedReadOnly, string[] expectedContainerTypes)
				=> CombineAssertions
				(
					message,
					() =>
					{
						AssertEquals("ReadOnly", expectedReadOnly, bulkRateUpdater.ContainerTypes.ReadOnly);
						AssertContainsExactElementsInAnyOrder
						(
							"ContainerTypes",
							expectedContainerTypes,
							bulkRateUpdater.ContainerTypes.Select(containerType => containerType.RC_Code)
						);
					}
				);
		}

		public void TestModules()
		{
			var bulkRateUpdater = new BulkRateUpdater();
			AssertContainsExactElementsInExactOrder("modules", new[] { "FWD", "SHP", "CFS", "TRT", "WRH", "CUS" }, bulkRateUpdater.Modules.GetAllCodes());
		}

		#region Type Filter

		public void TestTypeModeFilter()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			CreateRateEntriesWithAllCategoriesAndModes(clientRate, "AUSYD", "USLAX");
			Factory.Save();

			AssertTypeModeFilter("FWD", Category.AIR, Mode.LSE, expectedRateEntries: new[] { (Category.AIR, Mode.LSE) });
			AssertTypeModeFilter("FWD", Category.AIR, Mode.ULD, expectedRateEntries: new[] { (Category.AIR, Mode.ULD) });
			AssertTypeModeFilter("FWD", Category.FCL, Mode.SEA, expectedRateEntries: new[] { (Category.FCL, Mode.SEA) });
			AssertTypeModeFilter("FWD", Category.ORG, Mode.ALL, expectedRateEntries: new[] { (Category.ORG, "") });
			AssertTypeModeFilter("SHP", Category.SCO, Mode.SEA, expectedRateEntries: new[] { (Category.SCO, Mode.SEA) });

			// Customs
			AssertTypeModeFilter("CUS", Category.CAI, Mode.LSE, expectedRateEntries: new[] { (Category.CAI, Mode.LSE) });
			AssertTypeModeFilter("CUS", Category.CAI, Mode.ULD, expectedRateEntries: new[] { (Category.CAI, Mode.ULD) });
			AssertTypeModeFilter("CUS", Category.CAI, Mode.BCN, expectedRateEntries: new[] { (Category.CAI, Mode.BCN) });
			AssertTypeModeFilter("CUS", Category.CFC, Mode.SEA, expectedRateEntries: new[] { (Category.CFC, Mode.SEA) });
			AssertTypeModeFilter("CUS", Category.CFC, Mode.ROA, expectedRateEntries: new[] { (Category.CFC, Mode.ROA) });
			AssertTypeModeFilter("CUS", Category.CFC, Mode.RAI, expectedRateEntries: new[] { (Category.CFC, Mode.RAI) });
			AssertTypeModeFilter("CUS", Category.CFC, Mode.BCN, expectedRateEntries: new[] { (Category.CFC, Mode.BCN) });

			AssertTypeModeFilter("CUS", Category.CLC, Mode.LCL, expectedRateEntries: new[] { (Category.CLC, Mode.LCL) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.LRO, expectedRateEntries: new[] { (Category.CLC, Mode.LRO) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.FTL, expectedRateEntries: new[] { (Category.CLC, Mode.FTL) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.LRA, expectedRateEntries: new[] { (Category.CLC, Mode.LRA) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.FWL, expectedRateEntries: new[] { (Category.CLC, Mode.FWL) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.BBK, expectedRateEntries: new[] { (Category.CLC, Mode.BBK) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.BLK, expectedRateEntries: new[] { (Category.CLC, Mode.BLK) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.ROR, expectedRateEntries: new[] { (Category.CLC, Mode.ROR) });
			AssertTypeModeFilter("CUS", Category.CLC, Mode.BCN, expectedRateEntries: new[] { (Category.CLC, Mode.BCN) });

			AssertTypeModeFilter("CUS", Category.COR, Mode.ALL, expectedRateEntries: new[] { (Category.COR, "") });
			AssertTypeModeFilter("CUS", Category.COR, Mode.AIR, expectedRateEntries: new[] { (Category.COR, Mode.AIR) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.ULD, expectedRateEntries: new[] { (Category.COR, Mode.ULD) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.LSE, expectedRateEntries: new[] { (Category.COR, Mode.LSE) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.SEA, expectedRateEntries: new[] { (Category.COR, Mode.SEA) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.LCL, expectedRateEntries: new[] { (Category.COR, Mode.LCL) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.FCL, expectedRateEntries: new[] { (Category.COR, Mode.FCL) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.ROA, expectedRateEntries: new[] { (Category.COR, Mode.ROA) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.LRO, expectedRateEntries: new[] { (Category.COR, Mode.LRO) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.FRO, expectedRateEntries: new[] { (Category.COR, Mode.FRO) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.FTL, expectedRateEntries: new[] { (Category.COR, Mode.FTL) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.RAI, expectedRateEntries: new[] { (Category.COR, Mode.RAI) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.LRA, expectedRateEntries: new[] { (Category.COR, Mode.LRA) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.FRA, expectedRateEntries: new[] { (Category.COR, Mode.FRA) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.FWL, expectedRateEntries: new[] { (Category.COR, Mode.FWL) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.MAI, expectedRateEntries: new[] { (Category.COR, Mode.MAI) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.BBK, expectedRateEntries: new[] { (Category.COR, Mode.BBK) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.BLK, expectedRateEntries: new[] { (Category.COR, Mode.BLK) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.ROR, expectedRateEntries: new[] { (Category.COR, Mode.ROR) });
			AssertTypeModeFilter("CUS", Category.COR, Mode.BCN, expectedRateEntries: new[] { (Category.COR, Mode.BCN) });

			AssertTypeModeFilter("CUS", Category.CDS, Mode.ALL, expectedRateEntries: new[] { (Category.CDS, "") });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.AIR, expectedRateEntries: new[] { (Category.CDS, Mode.AIR) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.ULD, expectedRateEntries: new[] { (Category.CDS, Mode.ULD) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.LSE, expectedRateEntries: new[] { (Category.CDS, Mode.LSE) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.SEA, expectedRateEntries: new[] { (Category.CDS, Mode.SEA) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.LCL, expectedRateEntries: new[] { (Category.CDS, Mode.LCL) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.FCL, expectedRateEntries: new[] { (Category.CDS, Mode.FCL) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.ROA, expectedRateEntries: new[] { (Category.CDS, Mode.ROA) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.LRO, expectedRateEntries: new[] { (Category.CDS, Mode.LRO) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.FRO, expectedRateEntries: new[] { (Category.CDS, Mode.FRO) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.FTL, expectedRateEntries: new[] { (Category.CDS, Mode.FTL) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.RAI, expectedRateEntries: new[] { (Category.CDS, Mode.RAI) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.LRA, expectedRateEntries: new[] { (Category.CDS, Mode.LRA) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.FRA, expectedRateEntries: new[] { (Category.CDS, Mode.FRA) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.FWL, expectedRateEntries: new[] { (Category.CDS, Mode.FWL) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.MAI, expectedRateEntries: new[] { (Category.CDS, Mode.MAI) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.BBK, expectedRateEntries: new[] { (Category.CDS, Mode.BBK) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.BLK, expectedRateEntries: new[] { (Category.CDS, Mode.BLK) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.ROR, expectedRateEntries: new[] { (Category.CDS, Mode.ROR) });
			AssertTypeModeFilter("CUS", Category.CDS, Mode.BCN, expectedRateEntries: new[] { (Category.CDS, Mode.BCN) });

			AssertTypeModeFilter("WRH", Category.TWU, Mode.ALL, expectedRateEntries: new[] { (Category.TWU, "") });
			AssertTypeModeFilter("WRH", Category.TWU, Mode.AIR, expectedRateEntries: new[] { (Category.TWU, Mode.AIR) });
			AssertTypeModeFilter("WRH", Category.TWU, Mode.SEA, expectedRateEntries: new[] { (Category.TWU, Mode.SEA) });
			AssertTypeModeFilter("WRH", Category.TWU, Mode.ROA, expectedRateEntries: new[] { (Category.TWU, Mode.ROA) });
		}

		void CreateRateEntriesWithAllCategoriesAndModes(RatingHeader ratingHeader, string origin, string destination)
		{
			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var rateEntry = ratingHeader.AddRateEntry(category);
				foreach (CodeDescriptionPair transportMode in rateEntry.Lookups.TransportModes)
				{
					//Modes list is retrieved from lookup in rateEntry instance.
					//Hence on first loop, we just reuse the created rateEntry and create new rateEntries on subsequent loops
					if (string.IsNullOrEmpty(rateEntry.TI_Mode) || string.IsNullOrEmpty(rateEntry.TI_OriginLRC) || string.IsNullOrEmpty(rateEntry.TI_DestinationLRC))
					{
						rateEntry.TI_Mode = transportMode.Code;
						rateEntry.TI_OriginLRC = origin;
						rateEntry.TI_DestinationLRC = destination;
					}
					else
					{
						ratingHeader.AddRateEntry(category, transportMode.Code, origin, destination);
					}
				}
			}
		}

		void AssertTypeModeFilter(string module, string type, string mode, (string category, string mode)[] expectedRateEntries)
		{
			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = module;
			bulkRateUpdater.Type = type;
			bulkRateUpdater.Mode = mode;
			bulkRateUpdater.LoadEntries();

			var expectedRateEntryBizObjs = new HashSet<RateEntry>();
			foreach (var expectedRateEntry in expectedRateEntries)
			{
				var query = new ZQuery(RateEntrySchema.TI_RateCategory, expectedRateEntry.category);
				if (!string.IsNullOrEmpty(expectedRateEntry.mode))
				{
					query.AddToFilter(new ZQuery(RateEntrySchema.TI_Mode, expectedRateEntry.mode));
				}
				var rateEntries = Factory.Load<RateEntry>(query);
				expectedRateEntryBizObjs.UnionWith(rateEntries);
			}

			TestHelper.AssertRateEntries
			(
				expectedRateEntryBizObjs,
				bulkRateUpdater.Entries.Select(entry => entry),
				$"BulkRateUpdater.Entries: module '{module}', type '{type}', mode '{mode}'"
			);
		}

		public void TestTypeDefaultsOnlyMode()
		{
			var updater = new BulkRateUpdater();
			updater.Module = "FWD";
			updater.Type = RatingConstants.RateCategory.AIR;
			AssertContainsExactElementsInAnyOrder(new[] { "LSE", "ULD", "BCN", "SCN" }, updater.Modes.GetAllCodes());
			AssertEquals("Has four options so should not default mode", "", updater.Mode);

			updater.Module = "CUS";
			updater.Type = Category.CAI;
			AssertContainsExactElementsInAnyOrder(new[] { Mode.LSE, Mode.ULD, Mode.BCN, Mode.SCN }, updater.Modes.GetAllCodes());
			AssertNullOrEmpty("Has four options so should not default mode", updater.Mode);

			updater.Module = "SHP";
			updater.Type = RatingConstants.RateCategory.SNC;
			AssertEquals(1, updater.Modes.Count);
			AssertEquals("Has only one option so should default it", Core.Constants.RateMode.LCL, updater.Mode);

			updater.Type = RatingConstants.RateCategory.SCO;
			AssertEquals(1, updater.Modes.Count);
			AssertEquals("Has only one option so should default it", Core.Constants.RateMode.SEA, updater.Mode);

			updater.Module = "TRT";
			updater.Type = RatingConstants.RateCategory.TRN;
			AssertEquals(5, updater.Modes.Count);
			AssertHasErrors("SEA is not a valid mode for land transport", updater.ModeInfo);
			AssertEquals("Has multiple options so should not change transport mode even if it's invalid", Core.Constants.RateMode.SEA, updater.Mode);

			updater.Module = "WRH";
			updater.Type = RatingConstants.RateCategory.WHS;
			AssertContainsExactElementsInAnyOrder(new[] { Mode.ALL }, updater.Modes.GetAllCodes());
			AssertEquals("Has only one option so should default it", Mode.ALL, updater.Mode);

			updater.Module = "WRH";
			updater.Type = RatingConstants.RateCategory.TRW;
			AssertContainsExactElementsInAnyOrder(new[] { Mode.ALL }, updater.Modes.GetAllCodes());
			AssertEquals("Has only one option so should default it", Mode.ALL, updater.Mode);

			updater.Module = "WRH";
			updater.Type = RatingConstants.RateCategory.TWU;
			AssertContainsExactElementsInAnyOrder(new[] { Mode.ALL, Mode.AIR, Mode.SEA, Mode.ROA }, updater.Modes.GetAllCodes());
			AssertEquals("Has multiple options so should not change transport mode", Mode.ALL, updater.Mode);
		}

		[TestDate(2023, 06, 01)]
		public void TestShowExpiredFilter()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry MakeEntry(string origin, string destination, ZDate startDate, ZDate? endDate)
			{
				var rateEntry = clientRate.AddRateEntry(Category.AIR, Mode.LSE, origin, destination);
				rateEntry.TI_RateStartDate = startDate;
				rateEntry.TI_RateEndDate = endDate ?? new ZDate(null);

				return rateEntry;
			}

			var rateEntryA = MakeEntry("AUMEL", "USLAX", new ZDate(2023, 01, 01), new ZDate(2023, 02, 01));
			var rateEntryB = MakeEntry("AUSYD", "USLAX", new ZDate(2023, 01, 01), new ZDate(2023, 09, 01));
			var rateEntryC = MakeEntry("AUADL", "USLAX", new ZDate(2023, 01, 01), null);
			var rateEntryD = MakeEntry("NZAKL", "USLAX", new ZDate(2023, 09, 01), new ZDate(2023, 10, 01));

			Factory.Save();

			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.AIR;
			bulkRateUpdater.Mode = Mode.LSE;

			Assert("ShowExpired should default to enabled", bulkRateUpdater.ShowExpired);

			bulkRateUpdater.LoadEntries();
			var entryPks = bulkRateUpdater.Entries.Select(e => e.PK).ToList();

			AssertEquals(4, entryPks.Count);
			AssertCollectionContains(rateEntryA.PK, entryPks);
			AssertCollectionContains(rateEntryB.PK, entryPks);
			AssertCollectionContains(rateEntryC.PK, entryPks);
			AssertCollectionContains(rateEntryD.PK, entryPks);

			bulkRateUpdater.ShowExpired = false;
			bulkRateUpdater.LoadEntries();
			entryPks = bulkRateUpdater.Entries.Select(e => e.PK).ToList();

			AssertEquals(3, entryPks.Count);
			AssertCollectionNotContains(rateEntryA.PK, entryPks);
			AssertCollectionContains(rateEntryB.PK, entryPks);
			AssertCollectionContains(rateEntryC.PK, entryPks);
			AssertCollectionContains(rateEntryD.PK, entryPks);

			bulkRateUpdater.ShowExpired = true;
			bulkRateUpdater.NoExpireDate = true;
			bulkRateUpdater.LoadEntries();
			entryPks = bulkRateUpdater.Entries.Select(e => e.PK).ToList();

			AssertEquals(1, entryPks.Count);
			AssertCollectionNotContains(rateEntryA.PK, entryPks);
			AssertCollectionNotContains(rateEntryB.PK, entryPks);
			AssertCollectionContains(rateEntryC.PK, entryPks);
			AssertCollectionNotContains(rateEntryD.PK, entryPks);
		}

		public void TestContainerModeFilter()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry20GP = clientRate.AddRateEntry(Category.AIR, Mode.LSE, "AUMEL", "USLAX", container: "20GP");
			var rateEntry40GP = clientRate.AddRateEntry(Category.AIR, Mode.LSE, "AUMEL", "USLAX", container: "40GP");
			var rateEntry20OT = clientRate.AddRateEntry(Category.AIR, Mode.LSE, "AUMEL", "USLAX", container: "20OT");

			Factory.Save();

			var bulkRateUpdater = new BulkRateUpdater();
			bulkRateUpdater.Module = "FWD";
			bulkRateUpdater.Type = Category.AIR;
			bulkRateUpdater.Mode = Mode.LSE;

			var cont20GP = bulkRateUpdater.ContainerTypes.AddNew();
			cont20GP.RC_Code = "20GP";

			var cont40GP = bulkRateUpdater.ContainerTypes.AddNew();
			cont40GP.RC_Code = "40GP";

			bulkRateUpdater.LoadEntries();
			var entryPks = bulkRateUpdater.Entries.Select(e => e.PK).ToList();

			AssertEquals(2, entryPks.Count);
			AssertCollectionContains(rateEntry20GP.PK, entryPks);
			AssertCollectionContains(rateEntry40GP.PK, entryPks);
			AssertCollectionNotContains(rateEntry20OT.PK, entryPks);
		}

		#endregion

		#region Read-Only

		public void TestContainerTypeReadOnly()
		{
			TestUpdater.Mode = "AIR";
			Assert(TestUpdater.ContainerTypes.ReadOnly);

			TestUpdater.Mode = "FCL";
			Assert(!TestUpdater.ContainerTypes.ReadOnly);

			TestUpdater.Mode = "SEA";
			Assert(TestUpdater.ContainerTypes.ReadOnly);

			TestUpdater.Mode = "ULD";
			Assert(!TestUpdater.ContainerTypes.ReadOnly);
		}

		#endregion

		#region Load Entries

		public void TestLoadEntries()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1AirLse = rate1.AddRateEntry(Category.AIR, Mode.LSE, "AUSYD", "USLAX");
			var entry1FclSea = rate1.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "STD", "20GP");

			var entry1AirUld = rate1.AddRateEntry(Category.AIR, Mode.ULD, "AUSYD", "USLAX");
			var entry1AirUldStd20gp = rate1.AddRateEntry(Category.AIR, Mode.ULD, "AUSYD", "USLAX", "STD", "20GP");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2AirLse = rate2.AddRateEntry(Category.AIR, Mode.LSE, "AUMEL", "USLAX");
			var entry2FclSea = rate2.AddRateEntry(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "D2D", "40GP", "HAZ");

			var rate3 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry3AirLse = rate3.AddRateEntry(Category.AIR, Mode.LSE, "AUMEL", "USSFO");
			var entry3LclLcl = rate3.AddRateEntry(Category.LCL, Mode.LCL, "AUSYD", "USLAX");
			var entry3OrgFcl = rate3.AddRateEntry(Category.ORG, Mode.FCL, "AUSYD", "USLAX", "STD", "40GP");
			entry3OrgFcl.AddRateLine("ODOC", FlatCalculator.Code);

			// Customs rates
			var rate4 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry4CaiLse = rate4.AddRateEntry(Category.CAI, Mode.LSE, "AUSYD", "USLAX");
			var entry4CfcSea = rate4.AddRateEntry(Category.CFC, Mode.SEA, "AUSYD", "USLAX", "STD", "20GP");

			var entry4CaiUld = rate4.AddRateEntry(Category.CAI, Mode.ULD, "AUSYD", "USLAX");
			var entry4CaiUldStd20gp = rate4.AddRateEntry(Category.CAI, Mode.ULD, "AUSYD", "USLAX", "STD", "20GP");

			var rate5 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry5CaiLse = rate5.AddRateEntry(Category.CAI, Mode.LSE, "AUMEL", "USLAX");
			var entry5CfcSea = rate5.AddRateEntry(Category.CFC, Mode.SEA, "AUSYD", "USLAX", "D2D", "40GP", "HAZ");

			var rate6 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry6CaiLse = rate6.AddRateEntry(Category.CAI, Mode.LSE, "AUMEL", "USSFO");
			var entry6ClcLcl = rate6.AddRateEntry(Category.CLC, Mode.LCL, "AUSYD", "USLAX");
			var entry6OrgFcl = rate6.AddRateEntry(Category.COR, Mode.FCL, "AUSYD", "USLAX", "STD", "40GP");
			entry6OrgFcl.AddRateLine("ODOC", FlatCalculator.Code);

			// Warehouse rates
			var rate7 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry7WhsAll = rate7.AddRateEntry(Category.WHS, Mode.ALL, "AUSYD", "USLAX");
			var entry7WhsAllStd20gp = rate7.AddRateEntry(Category.WHS, Mode.ALL, "AUSYD", "USLAX", "STD", "20GP");

			var rate8 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry8TrwAll = rate8.AddRateEntry(Category.TRW, Mode.ALL, "AUMEL", "USLAX");
			var entry8TrwAllD2d40gp = rate8.AddRateEntry(Category.TRW, Mode.ALL, "AUSYD", "USLAX", "D2D", "40GP", "HAZ");

			var rate9 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry9TwuAll = rate9.AddRateEntry(Category.TWU, Mode.ALL, "AUMEL", "USSFO");
			var entry9TwuAir = rate9.AddRateEntry(Category.TWU, Mode.AIR, "AUSYD", "USLAX");
			var entry9TwuSea = rate9.AddRateEntry(Category.TWU, Mode.SEA, "AUSYD", "USLAX", "STD", "40GP");
			var entry9TwuRoa = rate9.AddRateEntry(Category.TWU, Mode.ROA, "AUSYD", "USLAX", "STD", "40GP");

			Factory.Save();

			AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry1AirUld, entry1AirUldStd20gp, entry2AirLse, entry3AirLse });
			AssertLoadEntries("FWD", Category.AIR, Mode.ULD, new[] { entry1AirUld, entry1AirUldStd20gp });
			AssertLoadEntries("FWD", Category.AIR, Mode.LSE, new[] { entry1AirLse, entry2AirLse, entry3AirLse });
			AssertLoadEntries("FWD", Category.LCL, Mode.LCL, new[] { entry3LclLcl });
			AssertLoadEntries("FWD", Category.FCL, Mode.SEA, new[] { entry1FclSea, entry2FclSea });
			AssertLoadEntries("FWD", Category.FCL, Mode.SEA, new[] { entry1FclSea, entry2FclSea });
			AssertRateEntriesByOthers("D2D", "HAZ", ZString.Empty, new[] { entry2FclSea });
			AssertRateEntriesByOthers("STD", "", ZString.Empty, new[] { entry1FclSea });
			AssertRateEntriesByOthers("", "", "40GP", new[] { entry2FclSea });
			AssertLoadEntries("FWD", Category.ORG, Category.FCL, new[] { entry3OrgFcl });

			// Customs
			TestUpdater.ServiceLevel = "";
			TestUpdater.CommodityCode = "";

			AssertLoadEntries("CUS", Category.CAI, Mode.ALL, new[] { entry4CaiLse, entry4CaiUld, entry4CaiUldStd20gp, entry5CaiLse, entry6CaiLse });
			AssertLoadEntries("CUS", Category.CAI, Mode.ULD, new[] { entry4CaiUld, entry4CaiUldStd20gp });
			AssertLoadEntries("CUS", Category.CAI, Mode.LSE, new[] { entry4CaiLse, entry5CaiLse, entry6CaiLse });
			AssertLoadEntries("CUS", Category.CLC, Mode.LCL, new[] { entry6ClcLcl });
			AssertLoadEntries("CUS", Category.CFC, Mode.SEA, new[] { entry4CfcSea, entry5CfcSea });
			AssertLoadEntries("CUS", Category.CFC, Mode.SEA, new[] { entry4CfcSea, entry5CfcSea });
			AssertRateEntriesByOthers("D2D", "HAZ", ZString.Empty, new[] { entry5CfcSea });
			AssertRateEntriesByOthers("STD", "", ZString.Empty, new[] { entry4CfcSea });
			AssertRateEntriesByOthers("", "", "40GP", new[] { entry5CfcSea });
			AssertLoadEntries("CUS", Category.COR, Category.FCL, new[] { entry6OrgFcl });

			// Warehouse rates
			TestUpdater.ServiceLevel = "";
			TestUpdater.CommodityCode = "";

			AssertLoadEntries("WRH", Category.WHS, Mode.ALL, new[] { entry7WhsAll, entry7WhsAllStd20gp });
			AssertRateEntriesByOthers("STD", "", ZString.Empty, new[] { entry7WhsAllStd20gp });
			AssertLoadEntries("WRH", Category.TRW, Mode.ALL, new[] { entry8TrwAll, entry8TrwAllD2d40gp });
			AssertRateEntriesByOthers("D2D", "HAZ", ZString.Empty, new[] { entry8TrwAllD2d40gp });
			AssertRateEntriesByOthers("", "", "40GP", new[] { entry8TrwAllD2d40gp });
			AssertLoadEntries("WRH", Category.TWU, Mode.ALL, new[] { entry9TwuAll, entry9TwuAir, entry9TwuSea, entry9TwuRoa });
			AssertLoadEntries("WRH", Category.TWU, Mode.AIR, new[] { entry9TwuAir });
			AssertLoadEntries("WRH", Category.TWU, Mode.SEA, new[] { entry9TwuSea });
			AssertLoadEntries("WRH", Category.TWU, Mode.ROA, new[] { entry9TwuRoa });
		}

		public void TestLoadEntries_GatewayServiceLevel()
		{
			Helper.ChargeCodes.CreateGlobalCharge("TEST");

			var rate1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1AirLse = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUSYD", "USLAX", "TEST", 10m);
			entry1AirLse.TI_RS_NKGatewayServiceLevel = "DIR";

			var entry1AirUld = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.ULD, "AUSYD", "USLAX", "TEST", 20m);
			entry1AirUld.TI_RS_NKGatewayServiceLevel = "STD";
			var entry1AirUldStd20gp = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.ULD, "AUSYD", "USLAX", "TEST", 30m, container: "20GP");

			var rate2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2AirLse = rate2.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUMEL", "USLAX", "TEST", 40m);
			entry2AirLse.TI_RS_NKGatewayServiceLevel = "STD";

			var rate3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3AirLse = rate3.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUMEL", "USSFO", "TEST", 50m);
			entry3AirLse.TI_RS_NKGatewayServiceLevel = "DIR";

			Factory.Save();

			var originalShowClientRatesValue = TestUpdater.ShowClientRates;
			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;

			using (new DisposableAction(() =>
			{
				TestUpdater.ShowClientRates = originalShowClientRatesValue;
				TestUpdater.ShowIntercompanyTariffs = false;
			}))
			{
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry1AirUld, entry1AirUldStd20gp, entry2AirLse, entry3AirLse });

				TestUpdater.GatewayServiceLevel = "DIR";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry3AirLse });

				TestUpdater.GatewayServiceLevel = "STD";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirUld, entry2AirLse });

				TestUpdater.GatewayServiceLevel = "";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry1AirUld, entry1AirUldStd20gp, entry2AirLse, entry3AirLse });
			}
		}

		public void TestLoadEntries_ShipmentGatewayServiceLevel()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;

			Helper.ChargeCodes.CreateGlobalCharge("TEST");

			var rate1 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry1AirLse = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUSYD", "USLAX", "TEST", 10m);
			entry1AirLse.TI_RS_NKShipmentGatewayServiceLevel = "DIR";

			var entry1AirUld = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.ULD, "AUSYD", "USLAX", "TEST", 20m);
			entry1AirUld.TI_RS_NKShipmentGatewayServiceLevel = "STD";
			var entry1AirUldStd20gp = rate1.AddRateEntryWithFlatRateLine(Category.AIR, Mode.ULD, "AUSYD", "USLAX", "TEST", 30m, container: "20GP");

			var rate2 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry2AirLse = rate2.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUMEL", "USLAX", "TEST", 40m);
			entry2AirLse.TI_RS_NKShipmentGatewayServiceLevel = "STD";

			var rate3 = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry3AirLse = rate3.AddRateEntryWithFlatRateLine(Category.AIR, Mode.LSE, "AUMEL", "USSFO", "TEST", 50m);
			entry3AirLse.TI_RS_NKShipmentGatewayServiceLevel = "DIR";

			Factory.Save();

			var originalShowClientRatesValue = TestUpdater.ShowClientRates;
			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;

			using (new DisposableAction(() =>
			{
				TestUpdater.ShowClientRates = originalShowClientRatesValue;
				TestUpdater.ShowIntercompanyTariffs = false;
			}))
			{
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry1AirUld, entry1AirUldStd20gp, entry2AirLse, entry3AirLse });

				TestUpdater.ShipmentGatewayServiceLevel = "DIR";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry3AirLse });

				TestUpdater.ShipmentGatewayServiceLevel = "STD";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirUld, entry2AirLse });

				TestUpdater.ShipmentGatewayServiceLevel = "";
				AssertLoadEntries("FWD", Category.AIR, Mode.ALL, new[] { entry1AirLse, entry1AirUld, entry1AirUldStd20gp, entry2AirLse, entry3AirLse });
			}
		}

		void AssertLoadEntries(string module, string type, string mode, RateEntry[] expectedRateEntries)
		{
			TestUpdater.Module = module;
			TestUpdater.Type = type;
			TestUpdater.Mode = mode;
			TestUpdater.ServiceLevel = "";
			TestUpdater.CommodityCode = "";
			TestUpdater.ContainerTypes.RemoveAll();
			TestUpdater.LoadEntries();
			TestHelper.AssertRateEntries(expectedRateEntries, TestUpdater.Entries.Cast<RateEntry>());
		}

		void AssertRateEntriesByOthers(string serviceLevel, string commodityCode, ZString containerType, RateEntry[] expectedRateEntries)
		{
			TestUpdater.ServiceLevel = serviceLevel;
			TestUpdater.CommodityCode = commodityCode;
			TestUpdater.ContainerTypes.RemoveAll();
			if (!containerType.IsEmpty)
			{
				TestUpdater.ContainerTypes.AddNew().RC_Code = containerType;
			}
			TestUpdater.LoadEntries();
			TestHelper.AssertRateEntries(expectedRateEntries, TestUpdater.Entries.Cast<RateEntry>());
		}

		public void TestLoadEntries_ShouldApplySupplierFilterForIntercompanyTariff()
		{
			Helper.ChargeCodes.CreateGlobalCharge("TEST");

			var serviceProvider1 = Helper.NewOrgHeader();
			var serviceProvider2 = Helper.NewOrgHeader();

			var rate1 = Helper.NewIntercompanyTariff(serviceProvider1);
			var entry1 = rate1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "TEST", 10m);

			var rate2 = Helper.NewIntercompanyTariff(serviceProvider2);
			var entry2 = rate2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "TEST", 20m);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			TestUpdater.ShowIntercompanyTariffs = true;
			TestUpdater.ShowClientRates = false;

			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));
			Assert(TestUpdater.Entries.Contains(entry2));

			TestUpdater.Supplier = serviceProvider1.PK;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));

			TestUpdater.Supplier = serviceProvider2.PK;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry2));
		}

		public void TestLoadWithShowRatingHeaderTypeFlags()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.Header.CompanyData.OB_ARAutoUpdateRates = false;
			var clientRateEntry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var today = ZDate.Today;
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_QuoteEndDate = today;
			var quoteEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var acceptedQuote = Helper.NewQuote(Helper.NewOrgHeader());
			acceptedQuote.TH_Accepted = ZDateTime.Today;
			var acceptedQuoteEntry = acceptedQuote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var cancelledQuote = Helper.NewQuote(Helper.NewOrgHeader());
			cancelledQuote.TH_IsCancelled = true;
			var cancelledQuoteEntry = cancelledQuote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var expiredQuote = Helper.NewQuote(Helper.NewOrgHeader());
			expiredQuote.TH_QuoteDate = today.AddMonths(-2);
			expiredQuote.TH_QuoteEndDate = today.AddMonths(-1);
			var expiredQuoteEntry = expiredQuote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var companyTariff1 = Helper.NewCompanyTariff();
			var tariff1Entry = companyTariff1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			companyTariff1.TH_GlobalRateLevel = 1;
			companyTariff1.Factory.Save();

			var companyTariff2 = Helper.NewCompanyTariff();
			companyTariff2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			companyTariff2.TH_GlobalRateLevel = 2;
			companyTariff2.Factory.Save();

			var interCompanyTariff = Helper.NewIntercompanyTariff();
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var interCompanyTariffEntry = interCompanyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			interCompanyTariff.Factory.Save();

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(clientRateEntry));
			AssertEquals(false, TestUpdater.Entries[0].IncludeInUpdate);

			TestUpdater.ShowClientRates = false;
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);

			TestUpdater.ShowCostings = true;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(costingEntry));
			AssertEquals(true, TestUpdater.Entries[0].IncludeInUpdate);

			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(costingEntry));
			Assert(TestUpdater.Entries.Contains(quoteEntry));

			TestUpdater.ShowClientRates = true;
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(clientRateEntry));
			Assert(TestUpdater.Entries.Contains(costingEntry));
			Assert(TestUpdater.Entries.Contains(quoteEntry));

			TestUpdater.ShowCompanyTariffs = true;
			TestUpdater.LoadEntries();
			AssertEquals(4, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(clientRateEntry));
			Assert(TestUpdater.Entries.Contains(costingEntry));
			Assert(TestUpdater.Entries.Contains(quoteEntry));
			Assert(TestUpdater.Entries.Contains(tariff1Entry));

			TestUpdater.ShowIntercompanyTariffs = true;
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);
			Assert(!TestUpdater.Entries.Contains(clientRateEntry));
			Assert(!TestUpdater.Entries.Contains(costingEntry));
			Assert(!TestUpdater.Entries.Contains(quoteEntry));
			Assert(!TestUpdater.Entries.Contains(tariff1Entry));
			Assert(!TestUpdater.Entries.Contains(interCompanyTariffEntry));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowCostings = false;
			TestUpdater.ShowActiveQuotes = false;
			TestUpdater.ShowCompanyTariffs = false;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(!TestUpdater.Entries.Contains(clientRateEntry));
			Assert(!TestUpdater.Entries.Contains(costingEntry));
			Assert(!TestUpdater.Entries.Contains(quoteEntry));
			Assert(!TestUpdater.Entries.Contains(tariff1Entry));
			Assert(TestUpdater.Entries.Contains(interCompanyTariffEntry));
		}

		public void TestLoadWithDifferentLocations()
		{
			Helper.NewInternationalZone("EURR", null, "GB");

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var entry1b = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "SGSIN");
			var entry1c = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "GBLON", "USSFO");
			var entry1d = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "US");
			var entry1e = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUEC", "USCA");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(5, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry1e));

			TestUpdater.Origin = "AUSYD";
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));

			TestUpdater.Origin = "AU";
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.Origin = "";
			TestUpdater.Destination = "USCA";
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1e));

			TestUpdater.Destination = "USLAX";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));

			TestUpdater.Origin = "EURR";
			TestUpdater.Destination = "";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
		}

		public void TestLoadWithDatesFilter()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1a.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			entry1a.TI_RateEndDate = ZDate.Today.AddMonths(-4);
			var entry1b = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1b.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			entry1b.TI_RateEndDate = ZDate.Today.AddMonths(-2);
			var entry1c = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1c.TI_RateStartDate = ZDate.Today.AddMonths(-1);
			entry1c.TI_RateEndDate = ZDate.Today;
			var entry1d = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USSFO");
			entry1d.TI_RateStartDate = ZDate.Today;
			entry1d.TI_RateEndDate = ZDate.Today.AddMonths(1);

			var quote1 = Helper.NewQuote(Helper.NewOrgHeader());
			quote1.TH_QuoteDate = ZDate.Today;
			quote1.TH_QuoteEndDate = ZDate.Today.AddDays(15);
			var entry2a = quote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var quote2 = Helper.NewQuote(Helper.NewOrgHeader());
			quote2.TH_QuoteDate = ZDate.Today.AddDays(-15);
			quote2.TH_QuoteEndDate = ZDate.Today;
			var entry3a = quote2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var quote3 = Helper.NewQuote(Helper.NewOrgHeader());
			quote3.TH_QuoteDate = ZDate.Today.AddMonths(-2);
			quote3.TH_QuoteEndDate = ZDate.Today.AddDays(-15);
			var entry4a = quote3.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(4, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.StartDate = ZDate.Today;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.EndDate = ZDate.Today.AddDays(15);
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.StartDate = ZDate.Empty;
			TestUpdater.EndDate = ZDate.Today;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.LoadEntries();
			AssertEquals(4, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry2a));
			Assert(TestUpdater.Entries.Contains(entry3a));

			TestUpdater.StartDate = ZDate.Today;
			TestUpdater.EndDate = ZDate.Today.AddDays(15);
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry2a));

			TestUpdater.EndDate = ZDate.Empty;
			TestUpdater.LoadEntries();
			AssertEquals(4, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry2a));
			Assert(TestUpdater.Entries.Contains(entry3a));

			TestUpdater.StartDate = ZDate.Today.AddMonths(-1);
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
		}

		public void TestLoadWithNoExpireDateFilter()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1a.TI_RateStartDate = ZDate.Today.AddMonths(-5);
			entry1a.TI_RateEndDate = ZDate.Today.AddMonths(-4);
			var entry1b = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1b.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			entry1b.TI_RateEndDate = ZDate.Today.AddMonths(-2);
			var entry1c = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1c.TI_RateStartDate = ZDate.Today.AddMonths(-1);
			entry1c.TI_RateEndDate = ZDate.Empty;

			var quote1 = Helper.NewQuote(Helper.NewOrgHeader());
			quote1.TH_QuoteDate = ZDate.Today;
			quote1.TH_QuoteEndDate = ZDate.Today.AddDays(15);
			quote1.TH_OneTimeQuote = true;
			var entry2a = quote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			var quote2 = Helper.NewQuote(Helper.NewOrgHeader());
			quote2.TH_QuoteDate = ZDate.Today.AddDays(-15);
			quote2.TH_QuoteEndDate = ZDate.Empty;
			quote2.TH_OneTimeQuote = true;
			var entry3a = quote2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			Factory.Save();

			TestUpdater.StartDate = ZDate.Empty;
			TestUpdater.EndDate = ZDate.Empty;

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));
			Assert(TestUpdater.Entries.Contains(entry1c));

			TestUpdater.NoExpireDate = true;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));

			TestUpdater.ShowActiveQuotes = true;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry3a));
		}

		public void TestLoadQuotesEntriesWithEndDateFilter()
		{
			var today = ZDate.Today;
			var oneOffQuote1 = Helper.NewQuote(Helper.NewOrgHeader());
			oneOffQuote1.TH_QuoteDate = today;
			oneOffQuote1.TH_QuoteEndDate = today;
			oneOffQuote1.TH_OneTimeQuote = true;
			var oneOffQuoteEntry1 = oneOffQuote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			var oneOffQuote2 = Helper.NewQuote(Helper.NewOrgHeader());
			oneOffQuote2.TH_QuoteDate = today;
			oneOffQuote2.TH_QuoteEndDate = today.AddDays(15);
			oneOffQuote2.TH_OneTimeQuote = true;
			var oneOffQuoteEntry2 = oneOffQuote2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			var quotation1 = Helper.NewQuote(Helper.NewOrgHeader());
			quotation1.TH_QuoteDate = today;
			quotation1.TH_QuoteEndDate = today;
			var quotationEntry1 = quotation1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			var quotation2 = Helper.NewQuote(Helper.NewOrgHeader());
			quotation2.TH_QuoteDate = today;
			quotation2.TH_QuoteEndDate = today.AddDays(15);
			var quotationEntry2 = quotation2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			var quotation3 = Helper.NewQuote(Helper.NewOrgHeader());
			quotation3.TH_QuoteDate = today.AddDays(10);
			quotation3.TH_QuoteEndDate = today.AddDays(30);
			var quotationEntry3 = quotation3.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			var expiredQuote = Helper.NewQuote(Helper.NewOrgHeader());
			expiredQuote.TH_QuoteDate = today.AddMonths(-5);
			expiredQuote.TH_QuoteEndDate = today.AddMonths(-4);
			expiredQuote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.ShowActiveQuotes = true;

			TestUpdater.StartDate = today;
			TestUpdater.EndDate = ZDate.Empty;
			TestUpdater.LoadEntries();

			var expected = new[] { oneOffQuoteEntry1.PK, oneOffQuoteEntry2.PK, quotationEntry1.PK, quotationEntry2.PK };
			var actual = TestUpdater.Entries.Select(x => x.PK);
			AssertContainsExactElementsInAnyOrder(expected, actual);

			TestUpdater.StartDate = today.AddDays(10);
			TestUpdater.EndDate = today.AddDays(30);
			TestUpdater.LoadEntries();

			Assert("Should only find rate starting in 10 days", TestUpdater.Entries.All(x => x.PK == quotationEntry3.PK));

			TestUpdater.StartDate = today.AddMonths(-5);
			TestUpdater.EndDate = today.AddMonths(-4);
			TestUpdater.LoadEntries();

			Assert("Even though the date range matches the expired rate, there's no valid business reason to update an expired quote", !TestUpdater.Entries.Any());
		}

		public void TestLoadWithSupplierCarrier()
		{
			var supplier1 = Helper.NewOrgHeader();
			var supplier2 = Helper.NewOrgHeader();
			var carrier1 = Helper.NewOrgHeader();
			var carrier2 = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var entry1b = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "SGSIN");
			entry1b.TI_OH_Supplier = supplier1.PK;
			var entry1c = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "GBLON", "USSFO");
			entry1c.TI_OH_Supplier = supplier2.PK;
			entry1c.TI_OH_TransportProvider = carrier1.PK;
			var entry1d = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "US");
			entry1d.TI_OH_Supplier = supplier2.PK;
			entry1d.TI_OH_TransportProvider = carrier2.PK;
			var entry1e = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUEC", "USCA");
			entry1e.TI_OH_TransportProvider = carrier2.PK;

			var cost1 = Helper.NewCosting(supplier1);
			var costEntry1a = cost1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			costEntry1a.TI_OH_TransportProvider = carrier2.PK;

			var cost2 = Helper.NewCosting(supplier2);
			var costEntry2a = cost2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(5, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1a));
			Assert(TestUpdater.Entries.Contains(entry1b));
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry1e));

			TestUpdater.Supplier = supplier2.PK;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.ShowCostings = true;
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1c));
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(costEntry2a));

			TestUpdater.Carrier = carrier2.PK;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1d));

			TestUpdater.Supplier = ZGuid.Empty;
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1d));
			Assert(TestUpdater.Entries.Contains(entry1e));
			Assert(TestUpdater.Entries.Contains(costEntry1a));
		}

		public void TestLoadWithSelectedClientFilter()
		{
			var org1 = Helper.NewOrgHeader();
			var rate1 = Helper.NewClientRate(org1);
			var rateEntry1a = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var rateEntry1b = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");
			var rateEntry1c = rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "CNSHA");
			var quote1 = Helper.NewQuote(org1);
			quote1.TH_QuoteEndDate = ZDate.Today;
			var quoteEntry1a = quote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var quoteEntry1b = quote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");
			var quoteEntry1c = quote1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "CNSHA");

			var org2 = Helper.NewOrgHeader();
			var rate2 = Helper.NewClientRate(org2);
			var rateEntry2a = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var rateEntry2b = rate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");
			var quote2 = Helper.NewQuote(org2);
			quote2.TH_QuoteEndDate = ZDate.Today;
			var quoteEntry2a = quote2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var quoteEntry2b = quote2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");

			var org3 = Helper.NewOrgHeader();
			var rate3 = Helper.NewClientRate(org3);
			var rateEntry3a = rate3.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var quote3 = Helper.NewQuote(org3);
			quote3.TH_QuoteEndDate = ZDate.Today;
			var quoteEntry3a = quote3.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");

			Factory.Save();

			var rateEntryComparer = new RateEntryComparer();

			TestUpdater.ShowClientRates = true;
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry1a, rateEntry1b, rateEntry1c, rateEntry2a, rateEntry2b, rateEntry3a }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org1.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry1a, rateEntry1b, rateEntry1c }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org2.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry2a, rateEntry2b }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org3.PK;
			TestUpdater.LoadEntries();
			Assert(TestUpdater.Entries.Contains(rateEntry3a));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowActiveQuotes = true;

			TestUpdater.SelectedClient = ZGuid.Empty;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { quoteEntry1a, quoteEntry1b, quoteEntry1c, quoteEntry2a, quoteEntry2b, quoteEntry3a }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org1.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { quoteEntry1a, quoteEntry1b, quoteEntry1c }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org2.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { quoteEntry2a, quoteEntry2b }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org3.PK;
			TestUpdater.LoadEntries();
			Assert(TestUpdater.Entries.Contains(quoteEntry3a));

			TestUpdater.ShowClientRates = true;
			TestUpdater.ShowActiveQuotes = true;

			TestUpdater.SelectedClient = ZGuid.Empty;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer
				, new[] { rateEntry1a, rateEntry1b, rateEntry1c, rateEntry2a, rateEntry2b, rateEntry3a, quoteEntry1a, quoteEntry1b, quoteEntry1c, quoteEntry2a, quoteEntry2b, quoteEntry3a }
				, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org1.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry1a, rateEntry1b, rateEntry1c, quoteEntry1a, quoteEntry1b, quoteEntry1c }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org2.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry2a, rateEntry2b, quoteEntry2a, quoteEntry2b }, TestUpdater.Entries.Cast<RateEntry>());

			TestUpdater.SelectedClient = org3.PK;
			TestUpdater.LoadEntries();
			AssertContainsExactElementsInAnyOrder(rateEntryComparer, new[] { rateEntry3a, quoteEntry3a }, TestUpdater.Entries.Cast<RateEntry>());
		}

		public void TestLoadWithContractNumberFilter()
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(org);
			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1.TI_ContractNumber = "A00001001";
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NLAMS");
			entry2.TI_ContractNumber = "A00001002";
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");

			Factory.Save();

			TestUpdater.ShowClientRates = true;
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));
			Assert(TestUpdater.Entries.Contains(entry2));
			Assert(TestUpdater.Entries.Contains(entry3));

			TestUpdater.ContractNumber = "A00001001";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));

			TestUpdater.ContractNumber = "A00001002";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry2));

			TestUpdater.ContractNumber = "A00001003";
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);
		}

		public void TestLoadWithContractNumberLinkedFilter()
		{
			var org = Helper.NewOrgHeader();
			var rate = Helper.NewCosting(org);
			Helper.NewRatingContract(org, "C1234", RatingContractTypes.Provider, transportMode: Core.Constants.TransportModes.Air);

			var entry1 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry1.TI_ContractNumber = "C1234";
			entry1.TI_ContractNumberLinked = true;
			var entry2 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NLAMS");
			entry2.TI_ContractNumber = "A00001002";
			entry1.TI_ContractNumberLinked = true;
			entry2.TI_ContractNumberLinked = false;
			var entry3 = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");

			Factory.Save();

			TestUpdater.ShowCostings = true;
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			AssertEquals("Initial state", YesNoEmptyList.Codes.Empty, TestUpdater.ContractNumberLinked);
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));
			Assert(TestUpdater.Entries.Contains(entry2));
			Assert(TestUpdater.Entries.Contains(entry3));

			TestUpdater.ContractNumberLinked = YesNoEmptyList.Codes.No;
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry2));
			Assert(TestUpdater.Entries.Contains(entry3));

			TestUpdater.ContractNumberLinked = YesNoEmptyList.Codes.Yes;
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));
		}

		public void TestLoadWithGatewayAgentTypeFilter()
		{
			var org = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(org);
			var clientRateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var clientRateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NLAMS");
			var clientRateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");

			clientRate.Factory.Save();

			var rate = Helper.NewIntercompanyTariff();
			Helper.ChargeCodes.CreateGlobalCharge("TEST");
			var entry1 = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "TEST", 10m);
			entry1.TI_GatewayAgentType = "RAG";
			var entry2 = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NLAMS", "TEST", 10m);
			entry2.TI_GatewayAgentType = "SAG";

			rate.Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			TestUpdater.ShowClientRates = true;
			TestUpdater.ShowIntercompanyTariffs = false;
			TestUpdater.GatewayAgentType = "RAG";
			AssertHasWarning(TestUpdater.GatewayAgentTypeInfo, "Gateway Agent Type filter is applicable only for Intercompany Tariffs.");
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(clientRateEntry1));
			Assert(TestUpdater.Entries.Contains(clientRateEntry2));
			Assert(TestUpdater.Entries.Contains(clientRateEntry3));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = false;
			TestUpdater.GatewayAgentType = "RAG";
			AssertHasWarning(TestUpdater.GatewayAgentTypeInfo, "Gateway Agent Type filter is applicable only for Intercompany Tariffs.");
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);

			TestUpdater.GatewayAgentType = "";
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);

			TestUpdater.ShowIntercompanyTariffs = true;
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));
			Assert(TestUpdater.Entries.Contains(entry2));

			TestUpdater.GatewayAgentType = "RAG";
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry1));

			TestUpdater.GatewayAgentType = "SAG";
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertNoErrors(TestUpdater.GatewayAgentTypeInfo);
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(entry2));

			TestUpdater.GatewayAgentType = "ABC";
			AssertNoWarnings(TestUpdater.GatewayAgentTypeInfo);
			AssertHasError(TestUpdater.GatewayAgentTypeInfo, "Enter a valid selection.");
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);
		}

		public void TestLoadWithShipmentConsolidationStatusFilter()
		{
			var org = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(org);
			var clientRateEntry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			var clientRateEntry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "NLAMS");
			var clientRateEntry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USCHI");

			clientRate.Factory.Save();

			var standardCosting = Helper.NewCosting(null);
			var rate1 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUMEL", "USLAX", "FRT", 50m);
			rate1.TI_ShipmentConsolidationStatus = "STS";
			var rate2 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "WAR", 40m);
			rate2.TI_ShipmentConsolidationStatus = "";

			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rate3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "US", "FRT", 30m);
			rate3.TI_ShipmentConsolidationStatus = "STS";
			var rate4 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "US", "CAF", 10m);
			rate4.TI_ShipmentConsolidationStatus = "CNS";

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";

			TestUpdater.ShowClientRates = true;
			TestUpdater.ShowIntercompanyTariffs = false;
			TestUpdater.ShowCostings = false;
			TestUpdater.ShipmentConsolidationStatus = "STS";
			AssertHasWarning(TestUpdater.ShipmentConsolidationStatusInfo, "Shipment Consolidation Status filter is only applicable to Costing Tariffs.");
			AssertNoErrors(TestUpdater.ShipmentConsolidationStatusInfo);
			TestUpdater.LoadEntries();
			AssertEquals(3, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(clientRateEntry1));
			Assert(TestUpdater.Entries.Contains(clientRateEntry2));
			Assert(TestUpdater.Entries.Contains(clientRateEntry3));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowCostings = true;
			TestUpdater.ShipmentConsolidationStatus = "STS";
			AssertNoWarnings(TestUpdater.ShipmentConsolidationStatusInfo);
			AssertNoErrors(TestUpdater.ShipmentConsolidationStatusInfo);
			TestUpdater.LoadEntries();
			AssertEquals(2, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(rate1));
			Assert(TestUpdater.Entries.Contains(rate3));

			TestUpdater.ShowCostings = true;
			TestUpdater.ShipmentConsolidationStatus = "CNS";
			TestUpdater.LoadEntries();
			AssertEquals(1, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(rate4));

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowCostings = true;
			TestUpdater.ShipmentConsolidationStatus = "";
			AssertNoWarnings(TestUpdater.ShipmentConsolidationStatusInfo);
			AssertNoErrors(TestUpdater.ShipmentConsolidationStatusInfo);
			TestUpdater.LoadEntries();
			AssertEquals(4, TestUpdater.Entries.Count);
			Assert(TestUpdater.Entries.Contains(rate1));
			Assert(TestUpdater.Entries.Contains(rate2));
			Assert(TestUpdater.Entries.Contains(rate3));
			Assert(TestUpdater.Entries.Contains(rate4));

			TestUpdater.ShipmentConsolidationStatus = "ABC";
			AssertNoWarnings(TestUpdater.ShipmentConsolidationStatusInfo);
			AssertHasError(TestUpdater.ShipmentConsolidationStatusInfo, "Enter a valid selection.");
			TestUpdater.LoadEntries();
			AssertEquals(0, TestUpdater.Entries.Count);
		}

		#endregion

		#region Action

		public void TestActionProperty()
		{
			AssertActionProperty(true);
		}

		public void TestActionProperty_IntercompanyTariff()
		{
			TestUpdater.ShowClientRates = false;
			Assert("Precondition: Non Intercompany Tariff module should not be selected", !IsNonIntercompanyTariffsModuleSelected());

			TestUpdater.ShowIntercompanyTariffs = true;
			AssertActionProperty(false);
		}

		public void AssertActionProperty(bool expectedUnitFactorReadOnly)
		{
			Assert(TestUpdater.AddOrReplaceCharge);
			Assert(!TestUpdater.ReplaceCharge);
			Assert(!TestUpdater.IncreaseDecreaseCharge);
			Assert(!TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.AddOrReplaceCharge, TestUpdater.Action);

			TestUpdater.ActionsLine.OverrideChargeDescription = true;

			TestUpdater.ReplaceCharge = true;
			Assert(!TestUpdater.AddOrReplaceCharge);
			Assert(TestUpdater.ReplaceCharge);
			Assert(!TestUpdater.IncreaseDecreaseCharge);
			Assert(!TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.ReplaceCharge, TestUpdater.Action);
			Assert(!TestUpdater.ActionsLine.LockCalculator);
			Assert(!TestUpdater.ActionsLine.TL_RateDescInfo.ReadOnly);
			AssertEquals(expectedUnitFactorReadOnly, TestUpdater.ActionsLine.TL_UnitFactorInfo.ReadOnly);

			TestUpdater.IncreaseDecreaseCharge = true;
			Assert(!TestUpdater.AddOrReplaceCharge);
			Assert(!TestUpdater.ReplaceCharge);
			Assert(TestUpdater.IncreaseDecreaseCharge);
			Assert(!TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.IncreaseDecreaseCharge, TestUpdater.Action);
			AssertEquals(CompanyTariffOrCostBasedCalculator.CostBasedCode, TestUpdater.ActionsLine.TL_RateCalculator);
			Assert(TestUpdater.ActionsLine.LockCalculator);
			Assert(!TestUpdater.ActionsLine.RateLineItems.ReadOnly);
			Assert(TestUpdater.ActionsLine.TL_RateDescInfo.ReadOnly);
			Assert(TestUpdater.ActionsLine.TL_UnitFactorInfo.ReadOnly);

			TestUpdater.DeleteCharge = true;
			Assert(!TestUpdater.AddOrReplaceCharge);
			Assert(!TestUpdater.ReplaceCharge);
			Assert(!TestUpdater.IncreaseDecreaseCharge);
			Assert(TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.DeleteCharge, TestUpdater.Action);
			AssertEquals("", TestUpdater.ActionsLine.TL_RateCalculator);
			Assert(TestUpdater.ActionsLine.LockCalculator);
			Assert(TestUpdater.ActionsLine.TL_UnitFactorInfo.ReadOnly);

			TestUpdater.AddOrReplaceCharge = true;
			Assert(TestUpdater.AddOrReplaceCharge);
			Assert(!TestUpdater.ReplaceCharge);
			Assert(!TestUpdater.IncreaseDecreaseCharge);
			Assert(!TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.AddOrReplaceCharge, TestUpdater.Action);
			Assert(!TestUpdater.ActionsLine.LockCalculator);
			AssertEquals(expectedUnitFactorReadOnly, TestUpdater.ActionsLine.TL_UnitFactorInfo.ReadOnly);

			TestUpdater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			Assert(!TestUpdater.AddOrReplaceCharge);
			Assert(TestUpdater.ReplaceCharge);
			Assert(!TestUpdater.IncreaseDecreaseCharge);
			Assert(!TestUpdater.DeleteCharge);
			AssertEquals(BulkRateUpdater.Actions.ReplaceCharge, TestUpdater.Action);
		}

		[ExpectNoExceptions]
		public void TestLockedCalculator_CheckOrCreateItems_NoExceptionThrown()
		{
			TestUpdater.DeleteCharge = true;
			TestUpdater.IncreaseDecreaseCharge = true;
			TestUpdater.ActionsLine.Calculator.PerformPostConstructionActions();
		}

		public void TestDoNotSetRateLineItemReadOnlyWhenSwitchingActionType()
		{
			TestUpdater.AddOrReplaceCharge = true;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;

			TestUpdater.ActionsLine.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			AssertEquals("Should not be readonly", false, TestUpdater.ActionsLine.Calculator.FindRateLineItem(Calculator.Items.Operator.BAS).ReadOnly);

			TestUpdater.DeleteCharge = true;
			AssertNull("Item should be deleted", TestUpdater.ActionsLine.Calculator.FindRateLineItem(Calculator.Items.Operator.BAS));

			TestUpdater.AddOrReplaceCharge = true;
			AssertEquals("Should not be readonly", false, TestUpdater.ActionsLine.Calculator.FindRateLineItem(Calculator.Items.Operator.BAS).ReadOnly);

			TestUpdater.IncreaseDecreaseCharge = true;
			AssertEquals("Should not be readonly", false, TestUpdater.ActionsLine.Calculator.FindRateLineItem(Calculator.Items.Operator.BAS).ReadOnly);
		}

		public void TestAddOrReplaceChargeActionDoesNotClearRateLineItemsForAccumulatedCalculator()
		{
			TestUpdater.DeleteCharge = true;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OLAB"].PK;
			AssertNoExceptionThrown("RateLineItems should not be set to read only as they are cleared when a Calculator is set for accumulated values.",
				() => TestUpdater.AddOrReplaceCharge = true);
		}

		#endregion

		#region ChargeCodeValidation

		public void TestChargeCodeValidation()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GRT");
			Factory.Save();

			TestUpdater.ShowClientRates = true;
			TestUpdater.ActionsLine.TL_AC = globalChargeCode.PK;
			AssertHasError(TestUpdater.ActionsLine.TL_ACInfo, "Enter a valid Charge Code.");
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertNoErrors(TestUpdater.ActionsLine.TL_ACInfo);

			TestUpdater.ShowClientRates = false;
			TestUpdater.ShowIntercompanyTariffs = true;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
			AssertHasError(TestUpdater.ActionsLine.TL_ACInfo, "Enter a valid Global Charge Code.");
			TestUpdater.ActionsLine.TL_AC = globalChargeCode.PK;
			AssertNoErrors(TestUpdater.ActionsLine.TL_ACInfo);
		}

		#endregion

		#region Actions Line RatingHeaderType

		public void TestActionsLine_RatingHeaderType()
		{
			TestUpdater.ShowClientRates = false;
			Assert("Precondition: Non Intercompany Tariff module should not be selected", !IsNonIntercompanyTariffsModuleSelected());

			TestUpdater.ShowIntercompanyTariffs = true;
			Assert("Rating Header should be Intercompany Tariff.", TestUpdater.ActionsLine.IsIntercompanyTariff());

			TestUpdater.ShowIntercompanyTariffs = false;
			Assert("Rating Header should not be Intercompany Tariff.", !TestUpdater.ActionsLine.IsIntercompanyTariff());
		}

		#endregion

		#region SelectAllEntries

		public void TestSelectAllEntries()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USSFO");
			rate1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "GBLON");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			rate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			rate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USSFO");
			rate2.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "GBLON");

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "AIR";
			TestUpdater.Mode = "LSE";
			TestUpdater.LoadEntries();
			AssertEquals(6, TestUpdater.Entries.Count);

			TestUpdater.SelectAllEntries(true);
			foreach (RateEntry entry in TestUpdater.Entries)
			{
				Assert(entry.IncludeInUpdate);
			}

			TestUpdater.SelectAllEntries(false);
			foreach (RateEntry entry in TestUpdater.Entries)
			{
				Assert(!entry.IncludeInUpdate);
			}

			TestUpdater.SelectAllEntries(true);
			foreach (RateEntry entry in TestUpdater.Entries)
			{
				Assert(entry.IncludeInUpdate);
			}
		}

		#endregion

		#region Reset Actions RateLine Charge Description

		public void TestResetActionsLineChargeDescription()
		{
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = rate.AddRateEntry("ORG");
			var rateLine1 = entry1.RateLines.AddNew();

			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var entry2 = cost.AddRateEntry("ORG");
			var rateLine2 = entry2.RateLines.AddNew();

			var tariff = Helper.NewCompanyTariff();
			var entry3 = tariff.AddRateEntry("ORG");
			var rateLine3 = entry3.RateLines.AddNew();

			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry4 = quote.AddRateEntry("ORG");
			var rateLine4 = entry4.RateLines.AddNew();

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var entry5 = intercompanyTariff.AddRateEntry("ORG");
			var rateLine5 = entry5.RateLines.AddNew();

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "Test 1";
			Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed = false;
			entry1.IncludeInUpdate = true;
			TestUpdater.ResetActionsLineChargeDescription(entry1);
			AssertEquals(false, TestUpdater.ActionsLine.OverrideChargeDescription);
			AssertEquals(Helper.ChargeCodes["ODOC"].AC_Desc, TestUpdater.ActionsLine.TL_RateDesc);
			Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed = true;

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "Test 2";
			Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed = false;
			entry2.IncludeInUpdate = true;
			TestUpdater.ResetActionsLineChargeDescription(entry2);
			AssertEquals(false, TestUpdater.ActionsLine.OverrideChargeDescription);
			AssertEquals(Helper.ChargeCodes["ODOC"].AC_Desc, TestUpdater.ActionsLine.TL_RateDesc);
			Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed = true;

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "Test 3";
			Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed = false;
			entry3.IncludeInUpdate = true;
			TestUpdater.ResetActionsLineChargeDescription(entry3);
			AssertEquals(false, TestUpdater.ActionsLine.OverrideChargeDescription);
			AssertEquals(Helper.ChargeCodes["ODOC"].AC_Desc, TestUpdater.ActionsLine.TL_RateDesc);
			Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed = true;

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "Test 4";
			Env.Security.QuotationChargeDescriptionOverride.IsAllowed = false;
			entry4.IncludeInUpdate = true;
			TestUpdater.ResetActionsLineChargeDescription(entry4);
			AssertEquals(false, TestUpdater.ActionsLine.OverrideChargeDescription);
			AssertEquals(Helper.ChargeCodes["ODOC"].AC_Desc, TestUpdater.ActionsLine.TL_RateDesc);
			Env.Security.QuotationChargeDescriptionOverride.IsAllowed = true;

			TestUpdater.ActionsLine.OverrideChargeDescription = true;
			TestUpdater.ActionsLine.TL_RateDesc = "Test 5";
			Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed = false;
			entry5.IncludeInUpdate = true;
			TestUpdater.ResetActionsLineChargeDescription(entry5);
			AssertEquals(false, TestUpdater.ActionsLine.OverrideChargeDescription);
			AssertEquals(Helper.ChargeCodes["ODOC"].AC_Desc, TestUpdater.ActionsLine.TL_RateDesc);
			Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed = true;
		}

		#endregion

		#region Saving

		public void TestSaving()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry1a.AddRateLine("ODOC", FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USSFO");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry2a.AddRateLine("ODOC", FlatCalculator.Code);
			var entry2b = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "GBLON");
			entry2b.AddRateLine("ODOC", FlatCalculator.Code);
			var entry2c = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "HKHKG");
			entry2c.AddRateLine("ODOC", FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.LoadEntries();
			AssertEquals(5, TestUpdater.Entries.Count);
			TestUpdater.UpdateFirstRateEntriesBatch();
			AssertEquals(5, TestUpdater.PreviewEntries.Count);
			AssertEquals(5, TestUpdater.AllEntriesToUpdateCount);

			TestUpdater.ProcessingProgressed += new EventHandler<BulkRateUpdater.ProcessingProgressedEventArgs>(TestUpdater_ProcessingProgressed_Once);
			processingIteration = 0;
			var failureMessage = TestUpdater.TrySaveRateEntryInBatches();
			AssertEquals(1, processingIteration);
			AssertNull("should succeed", failureMessage);
		}

		void TestUpdater_ProcessingProgressed_Once(object sender, BulkRateUpdater.ProcessingProgressedEventArgs e)
		{
			processingIteration++;
			AssertEquals(100, e.PercentComplete);
		}

		public void TestSavingInBatches()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1a = rate1.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry1a.AddRateLine("ODOC", FlatCalculator.Code);
			var entry1b = rate1.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USSFO");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry2a = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			entry2a.AddRateLine("ODOC", FlatCalculator.Code);
			var entry2b = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "GBLON");
			entry2b.AddRateLine("ODOC", FlatCalculator.Code);
			var entry2c = rate2.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "HKHKG");
			entry2c.AddRateLine("ODOC", FlatCalculator.Code);

			Factory.Save();

			fTestUpdater = new BulkRateUpdaterForTest();
			TestUpdater.Module = "FWD";
			TestUpdater.Type = "ORG";
			TestUpdater.Mode = "AIR";
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["ODOC"].PK;
			TestUpdater.LoadEntries();
			AssertEquals(5, TestUpdater.Entries.Count);
			TestUpdater.UpdateFirstRateEntriesBatch();
			AssertEquals(2, TestUpdater.PreviewEntries.Count);
			AssertEquals(5, TestUpdater.AllEntriesToUpdateCount);

			TestUpdater.ProcessingProgressed += new EventHandler<BulkRateUpdater.ProcessingProgressedEventArgs>(TestUpdater_ProcessingProgressed);
			processingIteration = 0;
			var failureMessage = TestUpdater.TrySaveRateEntryInBatches();
			AssertEquals(3, processingIteration);
			AssertNull("should succeed", failureMessage);
		}

		int processingIteration;

		void TestUpdater_ProcessingProgressed(object sender, BulkRateUpdater.ProcessingProgressedEventArgs e)
		{
			processingIteration++;
			switch (processingIteration)
			{
				case 1:
					AssertEquals(2 * 100 / 5, e.PercentComplete);
					break;
				case 2:
					AssertEquals(4 * 100 / 5, e.PercentComplete);
					break;
				case 3:
					AssertEquals(100, e.PercentComplete);
					break;
			}
		}

		[TestDate(2016, 02, 14)]
		public void TestActionReplace_CreatesMoreEntriesThanDefaultPreviewCount()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AUSYD", "OCART", 40m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AUMEL", "OCART", 40m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AUBNE", "OCART", 40m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AUPER", "OCART", 40m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "CNSHA", "OCART", 40m);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "USLAX", "ODOC", 140m);

			Factory.Save();

			var updater = new BulkRateUpdaterForTest
			{
				Module = "FWD",
				Type = RatingConstants.RateCategory.ORG,
				Mode = Core.Constants.RateMode.LCL,
				Destination = Core.Constants.CountryCodes.Australia
			};

			updater.LoadEntries();
			updater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			updater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			updater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			updater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			updater.CreateNewEntry = true;

			var newStartDate = ZDate.Today.AddMonths(1);
			var newEndDate = ZDate.Today.AddMonths(6);
			updater.NewEntryStartDate = newStartDate;
			updater.NewEntryEndDate = newEndDate;

			updater.UpdateFirstRateEntriesBatch();

			AssertEquals(6, clientRate.AllEntries.Count());
			AssertEquals("Preview results are limited by batch size", 2, updater.PreviewEntries.Count);

			updater.TrySaveRateEntryInBatches();

			AssertEquals("Expected to have created another rate entries for each applicable rate entry", 10, clientRate.AllEntries.Count());

			foreach (var rateEntry in clientRate.AllEntries.Where(entry => entry.TI_RateStartDate == newStartDate))
			{
				var rateLine = rateEntry.RateLines[0];
				AssertEquals(FlatCalculator.Code, rateLine.TL_RateCalculator);
				AssertEquals("Expected newly created rate entry to have the replacement amount", 50m, rateLine.GetCalculator<FlatCalculator>().BaseRate);
			}
		}

		[TestDate(2016, 02, 14)]
		public void TestActionReplace_CreatingNonOverlappingDates()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var clientRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "", "AUSYD", "OCART", 40m);
			var today = ZDate.Today;
			clientRateEntry.TI_RateStartDate = today;
			clientRateEntry.TI_RateEndDate = today.AddMonths(2);

			Factory.Save();

			var updater = new BulkRateUpdaterForTest
			{
				Module = "FWD",
				Type = RatingConstants.RateCategory.ORG,
				Mode = Core.Constants.RateMode.LCL,
				Destination = Core.Constants.CountryCodes.Australia
			};

			updater.LoadEntries();
			updater.Action = BulkRateUpdater.Actions.ReplaceCharge;
			updater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			updater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			updater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 50m;

			updater.CreateNewEntry = true;

			updater.NewEntryStartDate = today.AddMonths(4);
			updater.NewEntryEndDate = today.AddMonths(6);

			updater.UpdateFirstRateEntriesBatch();

			AssertEquals("Dates do not overlap at all so there's nothing to replace", 0, updater.PreviewEntries.Count);

			updater.NewEntryStartDate = today.AddMonths(-4);
			updater.NewEntryEndDate = today.AddMonths(-2);

			updater.UpdateFirstRateEntriesBatch();

			AssertEquals(0, updater.PreviewEntries.Count);
		}

		[TestDate(2010, 10, 10)]
		public void TestActionAddOrReplace_FindsPreviewEntriesFromApplicableEntriesNotJustFirstLoaded()
		{
			var chargeCode = Helper.ChargeCodes["DDOC"];
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			RateEntry addRateEntry(string destination)
			{
				var result = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LCL, "AU", destination);
				result.TI_RateStartDate = new ZDate(2010, 10, 10);
				result.TI_RateEndDate = new ZDate(2011, 11, 11);

				var rateLine = result.AddRateLine(chargeCode, FlatCalculator.Code);
				rateLine.GetCalculator<FlatCalculator>().BaseRate = 10m;

				return result;
			}

			var expiredEntry1 = addRateEntry("AO");
			var expiredEntry2 = addRateEntry("BO");
			var expiredEntry3 = addRateEntry("CO");

			var updateableEntry = addRateEntry("CN");
			updateableEntry.TI_RateStartDate = new ZDate(2012, 12, 12);
			updateableEntry.TI_RateEndDate = ZDate.Empty;

			var expiredEntry4 = addRateEntry("VA");
			var expiredEntry5 = addRateEntry("XZ");
			var expiredEntry6 = addRateEntry("AQ");

			Factory.Save();

			var updater = new BulkRateUpdaterForTest();
			updater.Module = "FWD";
			updater.Type = RatingConstants.RateCategory.DST;
			updater.Mode = Core.Constants.RateMode.LCL;
			updater.LoadEntries();
			updater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			updater.ActionsLine.TL_AC = chargeCode.PK;
			updater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			updater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			updater.UpdateFirstRateEntriesBatch();

			var count = updater.PreviewEntries.Count;
			AssertEquals("Batch size should be 2 so only 2 rate entries should be shown", 2, count);
			AssertEquals(true, updater.HasAdditionalBatchesToProcess);

			var message = "Pre-condition: the second part of this test assumes that the updateableEntry is not the first rate selected.";
			AssertEquals(message, false, updater.PreviewEntries.Any(e => e.PK == updateableEntry.PK));

			updater.CreateNewEntry = true;
			updater.NewEntryStartDate = new ZDate(2012, 12, 12);
			updater.NewEntryEndDate = new ZDate(2012, 12, 24);
			updater.UpdateFirstRateEntriesBatch();

			message = "Should find updateableEntry and create a new rate even though it is not in the first batch of rate entries.";
			AssertEquals(message, 2, updater.PreviewEntries.Count);
			AssertEquals(message, true, updater.PreviewEntries.Any(e => e.PK == updateableEntry.PK));
		}

		public void TestActionAddOrReplace_UpdatesRatesWithoutOverlappingThem()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry1.AddRateLine("OCART", FlatCalculator.Code);
			entry1.TI_RateStartDate = new ZDate(2017, 05, 10);
			entry1.TI_RateEndDate = new ZDate(2017, 05, 11);

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry2.AddRateLine("OCART", FlatCalculator.Code);
			entry2.TI_RateStartDate = new ZDate(2017, 05, 12);
			entry2.TI_RateEndDate = new ZDate(2017, 05, 13);

			var entry3 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry3.AddRateLine("OCART", FlatCalculator.Code);
			entry3.TI_RateStartDate = new ZDate(2017, 05, 14);
			entry3.TI_RateEndDate = new ZDate(2017, 05, 15);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.LCL;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var count = TestUpdater.PreviewEntries.Count;
			AssertEquals(3, count);

			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2017, 05, 06);
			TestUpdater.NewEntryEndDate = new ZDate(2017, 06, 16);

			TestUpdater.UpdateFirstRateEntriesBatch();

			var message = TestUpdater.TrySaveRateEntryInBatches();

			AssertNull("Should be able to successfully update the rates", message);

			var newFactory = new BusinessObjectFactory();
			var reloadedEntry1 = newFactory.Load<RateEntry>(entry1.PK);
			var reloadedEntry2 = newFactory.Load<RateEntry>(entry2.PK);
			var reloadedEntry3 = newFactory.Load<RateEntry>(entry3.PK);

			AssertEquals(new ZDate(2017, 05, 10), reloadedEntry1.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 11), reloadedEntry1.TI_RateEndDate);
			AssertEquals(FlatCalculator.Code, reloadedEntry1.RateLines[0].TL_RateCalculator);
			AssertEquals(20m, reloadedEntry1.RateLines[0].GetCalculator<FlatCalculator>().BaseRate);

			AssertEquals(new ZDate(2017, 05, 12), reloadedEntry2.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 13), reloadedEntry2.TI_RateEndDate);
			AssertEquals(FlatCalculator.Code, reloadedEntry2.RateLines[0].TL_RateCalculator);
			AssertEquals(20m, reloadedEntry2.RateLines[0].GetCalculator<FlatCalculator>().BaseRate);

			AssertEquals(new ZDate(2017, 05, 14), reloadedEntry3.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 15), reloadedEntry3.TI_RateEndDate);
			AssertEquals(FlatCalculator.Code, reloadedEntry3.RateLines[0].TL_RateCalculator);
			AssertEquals(20m, reloadedEntry3.RateLines[0].GetCalculator<FlatCalculator>().BaseRate);
		}

		public void TestGivenNewRatesHasOverlappingDateRangeWithExistingRate_WhenUpdateByActionAddOrReplace_ThenRateLinesShouldBeUpdatedToo()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry1.TI_RateStartDate = new ZDate(2017, 05, 1);
			entry1.TI_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine1 = entry1.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RateStartDate = new ZDate(2017, 05, 1);
			rateLine1.TL_RateEndDate = new ZDate(2017, 05, 10);

			var rateLine2 = entry1.AddRateLine("OCART", FlatCalculator.Code);
			rateLine2.TL_RateStartDate = new ZDate(2017, 05, 10);
			rateLine2.TL_RateEndDate = new ZDate(2017, 05, 20);

			var rateLine3 = entry1.AddRateLine("OCART", FlatCalculator.Code);
			rateLine3.TL_RateStartDate = new ZDate(2017, 05, 20);
			rateLine3.TL_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine4 = entry1.AddRateLine("OCART", FlatCalculator.Code);
			rateLine4.TL_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine5 = entry1.AddRateLine("OCART", FlatCalculator.Code);

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry2.TI_RateStartDate = new ZDate(2017, 06, 1);
			entry2.TI_RateEndDate = new ZDate(2017, 06, 30);

			var rateLine6 = entry2.AddRateLine("OCART", FlatCalculator.Code);
			rateLine6.TL_RateStartDate = new ZDate(2017, 06, 1);
			rateLine6.TL_RateEndDate = new ZDate(2017, 06, 10);

			var rateLine7 = entry2.AddRateLine("OCART", FlatCalculator.Code);
			rateLine7.TL_RateStartDate = new ZDate(2017, 06, 10);
			rateLine7.TL_RateEndDate = new ZDate(2017, 06, 20);

			var rateLine8 = entry2.AddRateLine("OCART", FlatCalculator.Code);
			rateLine8.TL_RateStartDate = new ZDate(2017, 06, 20);
			rateLine8.TL_RateEndDate = new ZDate(2017, 06, 30);

			var rateLine9 = entry2.AddRateLine("OCART", FlatCalculator.Code);
			rateLine9.TL_RateStartDate = new ZDate(2017, 06, 1);

			var rateLine10 = entry2.AddRateLine("OCART", FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.LCL;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var count = TestUpdater.PreviewEntries.Count;
			AssertEquals(2, count);

			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2017, 05, 15);
			TestUpdater.NewEntryEndDate = new ZDate(2017, 06, 15);

			TestUpdater.UpdateFirstRateEntriesBatch();

			var message = TestUpdater.TrySaveRateEntryInBatches();

			AssertNull("Should be able to successfully update the rates", message);

			var newFactory = new BusinessObjectFactory();
			var reloadedEntry1 = newFactory.Load<RateEntry>(entry1.PK);
			var reloadedEntry2 = newFactory.Load<RateEntry>(entry2.PK);

			AssertEquals(new ZDate(2017, 05, 1), reloadedEntry1.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedEntry1.TI_RateEndDate);

			AssertEquals(new ZDate(2017, 06, 16), reloadedEntry2.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 06, 30), reloadedEntry2.TI_RateEndDate);

			var reloadedLine1 = newFactory.Load<RateLine>(rateLine1.PK);
			AssertEquals(new ZDate(2017, 05, 1), reloadedLine1.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 10), reloadedLine1.TL_RateEndDate);

			var reloadedLine2 = newFactory.Load<RateLine>(rateLine2.PK);
			AssertEquals(new ZDate(2017, 05, 10), reloadedLine2.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedLine2.TL_RateEndDate);

			var reloadedLine3 = newFactory.Load<RateLine>(rateLine3.PK);
			AssertEquals(new ZDate(2017, 05, 20), reloadedLine3.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 30), reloadedLine3.TL_RateEndDate);

			var reloadedLine4 = newFactory.Load<RateLine>(rateLine4.PK);
			AssertEquals(ZDate.Empty, reloadedLine4.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedLine4.TL_RateEndDate);

			var reloadedLine5 = newFactory.Load<RateLine>(rateLine5.PK);
			AssertEquals(ZDate.Empty, reloadedLine5.TL_RateStartDate);
			AssertEquals(ZDate.Empty, reloadedLine5.TL_RateEndDate);

			var reloadedLine6 = newFactory.Load<RateLine>(rateLine6.PK);
			AssertEquals(new ZDate(2017, 06, 1), reloadedLine6.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 06, 10), reloadedLine6.TL_RateEndDate);

			var reloadedLine7 = newFactory.Load<RateLine>(rateLine7.PK);
			AssertEquals(new ZDate(2017, 06, 16), reloadedLine7.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 06, 20), reloadedLine7.TL_RateEndDate);

			var reloadedLine8 = newFactory.Load<RateLine>(rateLine8.PK);
			AssertEquals(new ZDate(2017, 06, 20), reloadedLine8.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 06, 30), reloadedLine8.TL_RateEndDate);

			var reloadedLine9 = newFactory.Load<RateLine>(rateLine9.PK);
			AssertEquals(new ZDate(2017, 06, 16), reloadedLine9.TL_RateStartDate);
			AssertEquals(ZDate.Empty, reloadedLine9.TL_RateEndDate);

			var reloadedLine10 = newFactory.Load<RateLine>(rateLine10.PK);
			AssertEquals(ZDate.Empty, reloadedLine10.TL_RateStartDate);
			AssertEquals(ZDate.Empty, reloadedLine10.TL_RateEndDate);
		}

		public void TestGivenNewRatesWithinExistingRateDateRange_WhenUpdateByActionAddOrReplace_ThenRateLinesShouldBeUpdatedToo()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			entry.TI_RateStartDate = new ZDate(2017, 05, 1);
			entry.TI_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine1 = entry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine1.TL_RateStartDate = new ZDate(2017, 05, 1);
			rateLine1.TL_RateEndDate = new ZDate(2017, 05, 10);

			var rateLine2 = entry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine2.TL_RateStartDate = new ZDate(2017, 05, 10);
			rateLine2.TL_RateEndDate = new ZDate(2017, 05, 20);

			var rateLine3 = entry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine3.TL_RateStartDate = new ZDate(2017, 05, 20);
			rateLine3.TL_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine4 = entry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine4.TL_RateEndDate = new ZDate(2017, 05, 30);

			var rateLine5 = entry.AddRateLine("OCART", FlatCalculator.Code);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.LCL;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var count = TestUpdater.PreviewEntries.Count;
			AssertEquals(1, count);

			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2017, 05, 15);
			TestUpdater.NewEntryEndDate = new ZDate(2017, 05, 18);

			TestUpdater.UpdateFirstRateEntriesBatch();

			var message = TestUpdater.TrySaveRateEntryInBatches();

			AssertNull("Should be able to successfully update the rates", message);

			var newFactory = new BusinessObjectFactory();
			var reloadedEntry = newFactory.Load<RateEntry>(entry.PK);

			AssertEquals(new ZDate(2017, 05, 1), reloadedEntry.TI_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedEntry.TI_RateEndDate);

			var reloadedLine1 = newFactory.Load<RateLine>(rateLine1.PK);
			AssertEquals(new ZDate(2017, 05, 1), reloadedLine1.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 10), reloadedLine1.TL_RateEndDate);

			var reloadedLine2 = newFactory.Load<RateLine>(rateLine2.PK);
			AssertEquals(new ZDate(2017, 05, 10), reloadedLine2.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedLine2.TL_RateEndDate);

			var reloadedLine3 = newFactory.Load<RateLine>(rateLine3.PK);
			AssertEquals(new ZDate(2017, 05, 20), reloadedLine3.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 30), reloadedLine3.TL_RateEndDate);

			var reloadedLine4 = newFactory.Load<RateLine>(rateLine4.PK);
			AssertEquals(ZDate.Empty, reloadedLine4.TL_RateStartDate);
			AssertEquals(new ZDate(2017, 05, 14), reloadedLine4.TL_RateEndDate);

			var reloadedLine5 = newFactory.Load<RateLine>(rateLine5.PK);
			AssertEquals(ZDate.Empty, reloadedLine5.TL_RateStartDate);
			AssertEquals(ZDate.Empty, reloadedLine5.TL_RateEndDate);
		}

		public void TestActionAddOrReplace_IgnoresDatesOutsideDateRange()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var earlierEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			earlierEntry.AddRateLine("OCART", FlatCalculator.Code);
			earlierEntry.TI_RateStartDate = new ZDate(2017, 05, 14);
			earlierEntry.TI_RateEndDate = new ZDate(2017, 05, 15);

			var laterEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD");
			laterEntry.AddRateLine("OCART", FlatCalculator.Code, QuantityUnit.CN);
			laterEntry.TI_RateStartDate = new ZDate(2017, 06, 17);
			laterEntry.TI_RateEndDate = new ZDate(2017, 06, 18);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.LCL;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			AssertEquals("Should find the two rates to update", 2, TestUpdater.PreviewEntries.Count);

			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2017, 05, 16);
			TestUpdater.NewEntryEndDate = new ZDate(2017, 06, 16);

			TestUpdater.UpdateFirstRateEntriesBatch();
			AssertEquals("As both existing rate entries fall out of side of the date rate for the new entry, no rates are added", 0, TestUpdater.PreviewEntries.Count);
		}

		public void TestActionAddOrReplace_UpdatingExistingRates_ConcurrencyExceptionIsHandled()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "EETLL", "AUSYD", "OCART", 10);
			entry.AddRateLine("OCART", FlatCalculator.Code);
			entry.TI_RateStartDate = new ZDate(2017, 08, 01);
			entry.TI_RateEndDate = new ZDate(2017, 08, 31);

			Factory.Save();

			TestUpdater.Module = "FWD";
			TestUpdater.Type = RatingConstants.RateCategory.ORG;
			TestUpdater.Mode = Core.Constants.RateMode.LCL;
			TestUpdater.LoadEntries();
			TestUpdater.Action = BulkRateUpdater.Actions.AddOrReplaceCharge;
			TestUpdater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
			TestUpdater.ActionsLine.TL_RateCalculator = FlatCalculator.Code;
			TestUpdater.ActionsLine.GetCalculator<FlatCalculator>().BaseRate = 20m;

			TestUpdater.UpdateFirstRateEntriesBatch();
			var count = TestUpdater.PreviewEntries.Count;
			AssertEquals("Pre-condition: filters should find this rate entry", 1, count);

			//Create a new rate entry with bulk rate updater
			TestUpdater.CreateNewEntry = true;
			TestUpdater.NewEntryStartDate = new ZDate(2017, 07, 08);
			TestUpdater.NewEntryEndDate = new ZDate(2017, 08, 08);

			TestUpdater.UpdateFirstRateEntriesBatch();
			var results = TestUpdater.PreviewEntries.Cast<RateEntry>().OrderBy(r => r.TI_RateStartDate).ToArray();
			AssertEquals("As the existing date range exceeds the new date range, an other rate entry is created to preserve the old rate", 2, results.Length);
			AssertEquals(new ZDate(2017, 08, 01), results[0].TI_RateStartDate);
			AssertEquals(new ZDate(2017, 08, 08), results[0].TI_RateEndDate);
			AssertEquals(new ZDate(2017, 08, 09), results[1].TI_RateStartDate);
			AssertEquals(new ZDate(2017, 08, 31), results[1].TI_RateEndDate);

			TestUpdater.UpdateFirstRateEntriesBatch();

			//Create an overlapping rate entry in an independant factory and save
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var reloadedEntry1 = anotherFactory.Load<RateEntry>(entry.PK);
			reloadedEntry1.TI_RateEndDate = new ZDate(2017, 08, 14);
			var message = "Should be able to save these rates in another factory, as the overlapping rates in bulk rate updater haven't been saved yet";
			AssertNoExceptionThrown(message, () => anotherFactory.Save());

			var savingMessage = TestUpdater.TrySaveRateEntryInBatches();
			AssertEquals("An error occurred saving updated rates.", savingMessage);
		}

		#endregion

		#region Implementation

		BulkRateUpdater TestUpdater
		{
			get { return fTestUpdater ?? (fTestUpdater = new BulkRateUpdater()); }
		}

		BulkRateUpdater fTestUpdater;

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		bool IsNonIntercompanyTariffsModuleSelected()
		{
			return TestUpdater.ShowClientRates || TestUpdater.ShowActiveQuotes || TestUpdater.ShowCostings || TestUpdater.ShowCompanyTariffs;
		}

		class RateEntryComparer : IEqualityComparer<RateEntry>
		{
			public bool Equals(RateEntry b1, RateEntry b2)
			{
				return b1.PK == b2.PK;
			}

			public int GetHashCode(RateEntry entry)
			{
				return entry.PK.GetHashCode();
			}
		}

		#endregion
	}

	class BulkRateUpdaterForTest : BulkRateUpdater
	{
		protected override int DefaultBatchSize
		{
			get { return 2; }
		}
	}
}
