using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobShipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInspectionTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var types = new ShipmentInspectionTypes();
				var collection = types.Types;
				collection.Add("ABC", (NoResString)"ABC Desc", true, true);
				collection.Add("DEF", (NoResString)"DEF Desc", true, false);
				collection.Add("GHI", (NoResString)"GHI Desc", false, true);

				using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, types))
				{
					var shipment = Factory.New<CommonShipment>();
					var lookups = new BaseJobShipmentLookups(shipment);

					AssertEquals("APP is added to the start of the list", "APP", lookups.InspectionTypes[0].Code);
					AssertEquals("SCR is in the list", "SCR", lookups.InspectionTypes[1].Code);
					AssertEquals("ABC is in the list", "ABC", lookups.InspectionTypes[2].Code);
					AssertEquals("DEF is in the list", "DEF", lookups.InspectionTypes[3].Code);
					AssertEquals("UNK is added to the end of the list", "UNK", lookups.InspectionTypes[4].Code);
					AssertEquals("GHI is not in the list", 5, lookups.InspectionTypes.Count);
				}
			}
		}

		public void TestInspectionTypes_IATA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_TransportMode = "AIR";
				var lookups = new BaseJobShipmentLookups(shipment);

				AssertContains("PHS", lookups.InspectionTypes.CodesAsString);
				AssertContains("XRY", lookups.InspectionTypes.CodesAsString);
				AssertContains("EDS", lookups.InspectionTypes.CodesAsString);
				AssertContains("EDD", lookups.InspectionTypes.CodesAsString);
				AssertContains("ETD", lookups.InspectionTypes.CodesAsString);
				AssertContains("CMD", lookups.InspectionTypes.CodesAsString);
				AssertContains("VCK", lookups.InspectionTypes.CodesAsString);
				AssertContains("AOM", lookups.InspectionTypes.CodesAsString);
				AssertContains("SMU", lookups.InspectionTypes.CodesAsString);
				AssertContains("MAI", lookups.InspectionTypes.CodesAsString);
				AssertContains("BIO", lookups.InspectionTypes.CodesAsString);
				AssertContains("DIP", lookups.InspectionTypes.CodesAsString);
				AssertContains("LFS", lookups.InspectionTypes.CodesAsString);
				AssertContains("NUC", lookups.InspectionTypes.CodesAsString);
				AssertContains("TRN", lookups.InspectionTypes.CodesAsString);
				AssertContains("EVD", lookups.InspectionTypes.CodesAsString);

				AssertNotContains("RES", lookups.InspectionTypes.CodesAsString);
				AssertNotContains("FRD", lookups.InspectionTypes.CodesAsString);
				AssertNotContains("VPT", lookups.InspectionTypes.CodesAsString);
				AssertNotContains("PRT", lookups.InspectionTypes.CodesAsString);
				AssertNotContains("MDE", lookups.InspectionTypes.CodesAsString);
				AssertNotContains("SIM", lookups.InspectionTypes.CodesAsString);
			}
		}

		public void TestInspectionTypes_EU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			{
				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "ITROM";
				shipment.JS_RL_NKDestination = "INBOM";
				var lookups = new BaseJobShipmentLookups(shipment);

				AssertEquals("APP is added to the start of the list", "APP", lookups.InspectionTypes[0].Code);
				AssertEquals("SCR is next", "SCR", lookups.InspectionTypes[1].Code);
				AssertContains("PHS", lookups.InspectionTypes.CodesAsString);
				AssertContains("XRY", lookups.InspectionTypes.CodesAsString);
				AssertContains("EDS", lookups.InspectionTypes.CodesAsString);
				AssertContains("EDD", lookups.InspectionTypes.CodesAsString);
				AssertContains("ETD", lookups.InspectionTypes.CodesAsString);
				AssertContains("CMD", lookups.InspectionTypes.CodesAsString);
				AssertContains("VCK", lookups.InspectionTypes.CodesAsString);
				AssertContains("AOM", lookups.InspectionTypes.CodesAsString);
				AssertContains("SMU", lookups.InspectionTypes.CodesAsString);
				AssertContains("MAI", lookups.InspectionTypes.CodesAsString);
				AssertContains("BIO", lookups.InspectionTypes.CodesAsString);
				AssertContains("DIP", lookups.InspectionTypes.CodesAsString);
				AssertContains("LFS", lookups.InspectionTypes.CodesAsString);
				AssertContains("NUC", lookups.InspectionTypes.CodesAsString);
				AssertContains("TRN", lookups.InspectionTypes.CodesAsString);
				AssertContains("UNK", lookups.InspectionTypes.CodesAsString);
			}
		}

		public void TestInspectionTypes_SCR()
		{
			var countryCodeList = new List<string> { "GB", "AU", "US", "CA" };
			foreach (var countryCode in countryCodeList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var shipment = Factory.NewWithValidTestData<CommonShipment>();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_RL_NKOrigin = "ITROM";
					shipment.JS_RL_NKDestination = "INBOM";
					var lookups = new BaseJobShipmentLookups(shipment);
					AssertContains("SCR is in the list", "SCR", lookups.InspectionTypes.CodesAsString);
				}
			}
		}

		public void TestAdditionalInspectionTypes()
		{
			var shipment = Factory.New<CommonShipment>();
			var lookups = new BaseJobShipmentLookups(shipment);
			var codes = lookups.AdditionalInspectionTypes.GetAllCodes();

			AssertCollectionContains("PHS", codes);
			AssertCollectionContains("VCK", codes);
			AssertCollectionContains("XRY", codes);
			AssertCollectionContains("EDS", codes);
			AssertCollectionContains("AOM", codes);
			AssertCollectionContains("EDD", codes);
			AssertCollectionContains("ETD", codes);
			AssertCollectionContains("CMD", codes);
			AssertCollectionContains("ZZZ", codes);
			AssertCollectionContains("UNK", codes);
			AssertCollectionContains("SCR", codes);
		}

		public void TestCommunityTransitStatusCodes()
		{
			var shipment = Factory.New<CommonShipment>();
			AssertContains("T2LSM", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			AssertContains("X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			AssertContains("T2LSM", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			AssertContains("X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			AssertContains("T2LSM", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			AssertContains("X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			shipment.JS_RL_NKDestination = "DEFRA";
			AssertContains("T2LSM", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			AssertContains("X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			shipment.JS_RL_NKOrigin = "GBLHR";
			AssertContains("T2LSM", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			AssertContains("X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
		}

		public void TestCommunityTransitStatusCode_ShouldContainsC_WhenLoginCountryIsSwitzerland()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Switzerland))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertContains(ExportCommunityTransitStatusList.Codes.C, shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}
		}

		public void TestCommunityTransitStatusCode_ShouldContainsC_WhenLoginCountryIsUnitedKingdom()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertContains(ExportCommunityTransitStatusList.Codes.C, shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}
		}

		[TestDate(2020, 12, 31)]
		public void TestCommunityTransitStatusCodes_EUCTP()
		{
			// add EU and transit groups
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroup = helper.CreateTradeGroup(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusTradeGroup.Codes.EuropeanUnionForCustoms, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.Latvia, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			tradeGroup = helper.CreateTradeGroup(RefDataGrouping.Codes.EuropeanUnionEUN, RefCusTradeGroup.Codes.EUCommonTransitProcedure, new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.Turkey, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.UnitedKingdom, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.Switzerland, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.Vanuatu, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			helper.AddCountry(tradeGroup, Constants.CountryCodes.SierraLeone, new ZDate(2019, 1, 1), new ZDate(2020, 06, 30));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("lookups.CommunityTransitStatusIDList.ExportTransit", "F, T, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM, TD, TF, X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("lookups.CommunityTransitStatusIDList.ExportTransit", "C, F, T, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM, TD, TF, X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("lookups.CommunityTransitStatusIDList.ExportTransit", "C, F, T, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM, TD, TF, X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Switzerland))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("Switzerland has C in ExportTransit list, to be able to have transit between CHGVA and EU (Regardless of its TradeGroup)", "C, F, T, T1, T2, T2F, T2L, T2LF, T2LSM, T2SM, TD, TF, X", shipment.Lookups.CommunityTransitStatusCodes.CodesAsString);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Vanuatu))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("Vanuatu should not have C in ExportTransit list since it is not expired", false, shipment.Lookups.CommunityTransitStatusCodes.ContainsCode(ExportCommunityTransitStatusList.Codes.C));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SierraLeone))
			{
				var shipment = Factory.New<CommonShipment>();
				AssertEquals("Sierra Leone should have C in ExportTransit list since it is expired", true, shipment.Lookups.CommunityTransitStatusCodes.ContainsCode(ExportCommunityTransitStatusList.Codes.C));
			}
		}

		public void TestCoLoadShipment_List()
		{
			var asm = FreightTestHelper.GetShipment<CommonShipment>("ASM", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var cld = FreightTestHelper.GetShipment<CommonShipment>("CLD", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var clb = FreightTestHelper.GetShipment<CommonShipment>("CLB", Constants.ShipmentTypes.BlindCoLoadMaster, Factory);
			var bcn = FreightTestHelper.GetShipment<CommonShipment>("BCN", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var std = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			var shipment = FreightTestHelper.GetShipment<CommonShipment>("shipment", Constants.ShipmentTypes.AssemblyMaster, Factory);

			Factory.Save();

			var collection = shipment.Lookups.CoLoadShipment_List.Cast<BusinessObject>();
			FreightTestHelper.AssertShipmentCollection(collection, asm, cld, clb, std);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			collection = shipment.Lookups.CoLoadShipment_List.Cast<BusinessObject>();
			FreightTestHelper.AssertShipmentCollection(collection, std);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			collection = shipment.Lookups.CoLoadShipment_List.Cast<BusinessObject>();
			FreightTestHelper.AssertShipmentCollection(collection, asm, cld, clb, std);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			collection = shipment.Lookups.CoLoadShipment_List.Cast<BusinessObject>();
			FreightTestHelper.AssertShipmentCollection(collection);
		}

		public void TestWarehouses()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = helper.CreateWarehouse("WH1");
			Factory.Save();

			var loc = Factory.New<PackLocation>();
			Assert("Shipment.Lookups.Warehouses.Count > 0", shipment.Lookups.Warehouses.Count > 0);
		}

		[TestDate(2010, 12, 10)]
		public void TestJS_INCO_ListBefore2011EffectiveDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertJS_INCO_List(shipment, new string[] { "PPD", "CLT", "C3P", "FCD" });

			shipment.JS_RL_NKDestination = "NZAKL";

			Assert("Prerequisite", shipment.JS_E_DEP.IsEmpty);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2000);

			shipment.JS_E_DEP = new ZDateTime(2011, 1, 2);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2000);

			shipment.JS_E_DEP = new ZDateTime(2010, 12, 22);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 7, 25)]
		public void TestJS_INCO_ListAfter2011EffectiveDate()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";

			AssertJS_INCO_List(shipment, new string[] { "PPD", "CLT", "C3P", "FCD" });

			shipment.JS_RL_NKDestination = "NZAKL";

			Assert("Prerequisite", shipment.JS_E_DEP.IsEmpty);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2010);

			shipment.JS_E_DEP = new ZDateTime(2011, 7, 30);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2010);

			shipment.JS_E_DEP = new ZDateTime(2010, 12, 22);

			AssertJS_INCO_List(shipment, Constants.IncoTerms.Incoterms2010);
		}

		void AssertJS_INCO_List(CommonShipment shipment, IEnumerable<string> expectedCodes)
		{
			string[] actualCodes = (from elem in shipment.Lookups.JS_INCO_List.Cast<CodeDescriptionPair>() select elem.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		public void TestJS_TransportMode_List()
		{
			var shipment = GetShipment();
			Assert(shipment.Lookups.JS_TransportMode_List.Count > 0);
		}

		public void TestJS_ShipmentType_List()
		{
			var hasHVLVClearance = HVLVDataRegistry.HasHVLVClearance;

			try
			{
				HVLVDataRegistry.HasHVLVClearance = true;

				var shipment = GetShipment();
				AssertEquals("Incorrect number of shipment types", 8, shipment.Lookups.JS_ShipmentType_List.Count);
				Assert("Expected to find HVL shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVL"));
				Assert("Expected to find 3PT shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("3PT"));
			}
			finally
			{
				HVLVDataRegistry.HasHVLVClearance = hasHVLVClearance;
			}
		}

		public void TestJS_ShipmentType_List_HVM()
		{
			var hasHVLVClearance = HVLVDataRegistry.HasHVLVClearance;

			try
			{
				HVLVDataRegistry.HasHVLVClearance = true;

				var shipment = GetShipment();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;
				AssertEquals("Incorrect number of shipment types", 9, shipment.Lookups.JS_ShipmentType_List.Count);
				Assert("Expected to find HVL shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVL"));
				Assert("Expected to find HVM shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVM"));
				Assert("Expected to find 3PT shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("3PT"));

				HVLVDataRegistry.HasHVLVClearance = false;

				shipment = GetShipment();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;
				Assert("Expected to find HVL shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVL"));
				Assert("Expected to find HVM shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVM"));
				Assert("Expected to find 3PT shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("3PT"));
			}
			finally
			{
				HVLVDataRegistry.HasHVLVClearance = hasHVLVClearance;
			}
		}

		public void TestJS_ShipmentType_List_IsNotCached()
		{
			HVLVDataRegistry.HasHVLVClearance = true;

			var shipment = GetShipment();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			Assert("Expected not to find HVM shipment type", !shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVM"));

			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;
			Assert("Expected to find HVM shipment type", shipment.Lookups.JS_ShipmentType_List.ContainsCode("HVM"));
		}

		public void TestJS_PackingMode_List()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = "Jnk";
			Assert(shipment.Lookups.JS_PackingMode_List.Count == 0);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert(shipment.Lookups.JS_PackingMode_List.Count > 0);
		}

		public void TestJS_HBLContainerPackModeOverride_List()
		{
			var collection = new HBLDeliveryModeCollection();
			collection.Add("AAA", (NoResString)"AAA - ShowInList-true");
			collection.Add("BBB", (NoResString)"BBB - ShowInList=true");
			collection.Add("CCC", (NoResString)"CCC - ShowInList=false", false);

			var hblDeliveryModes = new HBLDeliveryModes("FCL", collection);
			FreightDataRegistry.Instance.HBLDeliveryMode_FCL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, hblDeliveryModes);

			var shipment = GetShipment();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";

			var lookup = shipment.Lookups.JS_HBLContainerPackModeOverride_List;
			AssertEquals(2, lookup.Count);
			AssertEquals("AAA is in the list", "AAA", lookup[0].Code);
			AssertEquals("BBB is in the list", "BBB", lookup[1].Code);
		}

		public void TestJS_DiscrepancyReason_ListNotEmpty()
		{
			var shipment = GetShipment();
			Assert(shipment.Lookups.JS_DiscrepancyReason_List.Count > 0);
		}

		public void TestRefCurrency_ListContainsValidItems()
		{
			var shipment = GetShipment();
			var list = shipment.Lookups.RefCurrency_List;
			Assert(list.Count > 0);

			foreach (Object o in list)
			{
				Assert(o is RefCurrency);
			}
		}

		public void TestRefServiceLevel_ListContainsValidItems()
		{
			var shipment = GetShipment();
			var list = shipment.Lookups.RefServiceLevel_List;
			Assert(list.Count > 0);

			foreach (Object o in list)
			{
				Assert(o is RefServiceLevel);
			}
		}

		public void TestRefCommodity_ListContainsValidItems()
		{
			var shipment = GetShipment();
			var list = shipment.Lookups.RefCommodity_List;
			Assert(list.Count > 0);

			foreach (Object o in list)
			{
				Assert(o is RefCommodityCode);
			}
		}

		public void TestAWBDimsCodeDescriptionPairList()
		{
			var shipment = GetShipment();
			AssertNotNull(shipment.Lookups.AWBDimsCodeDescriptionPairList);
			AssertEquals(OLookUpEditType.AWBDimensions, shipment.Lookups.AWBDimsCodeDescriptionPairList.LookupEditType);
		}

		public void TestHouseBillOfLadingTypeList()
		{
			var shipment = GetShipment();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert("List should have items for SEA.", shipment.Lookups.JS_HouseBillOfLadingType_List.Count > 0);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			Assert("List should have items for ROAD.", shipment.Lookups.JS_HouseBillOfLadingType_List.Count > 0);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			Assert("List should have items for RAIL.", shipment.Lookups.JS_HouseBillOfLadingType_List.Count > 0);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			Assert("List should be empty AIR.", shipment.Lookups.JS_HouseBillOfLadingType_List.Count == 0);
		}

		public void TestHouseBillOfLadingTypeListForSea_RegistryValueNotModified()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");
			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(15, list, true);

			var addtionalTypesCollection = new AdditionalHouseBillOfLadingTypeCollection();
			var newType = addtionalTypesCollection.AddNew();
			newType.Enable = true;
			newType.Code = "abc";

			var registry = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea;
			var additionalTypesRegistry = FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes;
			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			using (additionalTypesRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addtionalTypesCollection))
			{
				var registryCollection = FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea.Value;

				var shipment = GetShipment();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				var lookupList = shipment.Lookups.JS_HouseBillOfLadingType_List;
				AssertGreaterThan("An addtional type should be added to the lookup result list without modifying the registry item.", lookupList.Count, registryCollection.Count);
			}
		}

		public void TestJS_HouseBillOfLadingTypeDefault_NoListProvided_Empty()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("", shipment.Lookups.JS_HouseBillOfLadingTypeDefault);
		}

		public void TestJS_HouseBillOfLadingTypeDefault_NoDefaultSet_FirstInTheList()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("FIA", shipment.Lookups.JS_HouseBillOfLadingTypeDefault);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("IAU", shipment.Lookups.JS_HouseBillOfLadingTypeDefault);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals("FIA", shipment.Lookups.JS_HouseBillOfLadingTypeDefault);
		}

		public void TestJS_HouseBillOfLadingTypeDefault_DefaultSet_UseRegistry()
		{
			var shipment = GetShipment();

			AssertDefaultValue(shipment, FreightDataRegistry.Instance.HouseBillOfLadingTypesForRoad, Constants.TransportModes.Road, "TY2");
			AssertDefaultValue(shipment, FreightDataRegistry.Instance.HouseBillOfLadingTypesForSea, Constants.TransportModes.Sea, "TY1");
			AssertDefaultValue(shipment, FreightDataRegistry.Instance.HouseBillOfLadingTypesForRail, Constants.TransportModes.Rail, "TY2");
		}

		static void AssertDefaultValue(CommonShipment shipment, CodeDescriptionPairListWithDefaultCodeRegistryItem registry, string transportMode, string expectedValue)
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("TY1", "Type 1");
			list.AddPair("TY2", "Type 2");

			var typesWithDefault = new SystemDefinableCodeDescriptionBoolCollection(3, list, true);
			typesWithDefault.SetDefaultCode(expectedValue, false);

			using (registry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, typesWithDefault))
			{
				shipment.JS_TransportMode = transportMode;
				AssertEquals(expectedValue, shipment.Lookups.JS_HouseBillOfLadingTypeDefault);
			}
		}

		public void TestHouseBillOfLadingWhenTransportChanges()
		{
			var testShipment = Factory.New<TestShipment>();

			testShipment.JS_TransportMode = Constants.TransportModes.Sea;
			Assert(testShipment.Lookups.JS_HouseBillOfLadingType_List.ContainsCode("FIA"));

			testShipment.JS_HouseBillOfLadingType = "FIA";
			testShipment.JS_TransportMode = Constants.TransportModes.Road;
			Assert(testShipment.Lookups.JS_HouseBillOfLadingType_List.ContainsCode("FIA"));
			AssertEquals("Code still valid, should not be cleared.", "FIA", testShipment.JS_HouseBillOfLadingType);

			testShipment.JS_HouseBillOfLadingType = "IPT";
			testShipment.JS_TransportMode = Constants.TransportModes.Rail;
			Assert(testShipment.Lookups.JS_HouseBillOfLadingType_List.ContainsCode("FIA"));
			Assert(!testShipment.Lookups.JS_HouseBillOfLadingType_List.ContainsCode("IPT"));
			AssertEquals("Code no longer valid, default to first code.", "FIA", testShipment.JS_HouseBillOfLadingType);

			testShipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("List for Air is empty, code should be cleared.", "", testShipment.JS_HouseBillOfLadingType);
		}

		public void TestJS_HouseBillOfLadingTypeInfo()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("HBL type should be read-only when transport mode is Air", true, shipment.JS_HouseBillOfLadingTypeInfo.ReadOnly);

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals("HBL type should be read-only when transport mode is AirSea", true, shipment.JS_HouseBillOfLadingTypeInfo.ReadOnly);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("HBL type should not be read-only when transport mode is Sea", false, shipment.JS_HouseBillOfLadingTypeInfo.ReadOnly);
		}

		public void TestJS_HBLAWBChargesDisplay_List()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Air charge types should be displayed", DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList, shipment.Lookups.JS_HBLAWBChargesDisplay_List);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Non-air charge types should be displayed", DocumentsDataRegistry.Instance.HBLChargesDefaultDisplayTypesPairList, shipment.Lookups.JS_HBLAWBChargesDisplay_List);
		}

		public void TestJS_HBLAWBChargesDisplayRefreshesCorrectly()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Default charge type is used in Air", "NON", shipment.JS_HBLAWBChargesDisplay);

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals("Default charge type is not used in AirSea", "", shipment.JS_HBLAWBChargesDisplay);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Default charge type should match registry default for non-air shipments", DocumentsDataRegistry.Instance.HBLChargesDefaultDisplay.Value, shipment.JS_HBLAWBChargesDisplay);

			var dept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES"));

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, dept.PK.ToGuid()))
			{
				shipment = GetShipment();
				AssertEquals("Default charge type should match registry default for non-air shipments", DocumentsDataRegistry.Instance.HBLChargesDefaultDisplay.Value, shipment.JS_HBLAWBChargesDisplay);
			}
		}

		public void TestJS_HBLAWBChargesDisplayInfo()
		{
			var shipment = GetShipment();

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Charges Display should not be read-only when transport mode is Air", false, shipment.JS_HBLAWBChargesDisplayInfo.ReadOnly);

			shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertEquals("Charges Display should not be read-only when transport mode is AirSea", false, shipment.JS_HBLAWBChargesDisplayInfo.ReadOnly);

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Charges Display should not be read-only when transport mode is Sea", false, shipment.JS_HBLAWBChargesDisplayInfo.ReadOnly);
		}

		public void TestConsignorConsigneeForwarderLists()
		{
			var shipment = GetShipment();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			Assert("Type of ConsignorForwarder_List when Coload is true.", shipment.Lookups.ConsignorForwarder_List is ConsignorCollection);
			Assert("Type of ConsignorForwarder_List when Coload is true.", shipment.Lookups.ConsignorForwarderPickup_List is ConsignorCollection);
			Assert("Type of ConsigneeForwarder_List when Coload is true.", shipment.Lookups.ConsigneeForwarder_List is ConsigneeCollection);
			Assert("Type of ConsigneeForwarder_List when Coload is true.", shipment.Lookups.ConsigneeForwarderDelivery_List is ConsigneeCollection);

			Assert("Collection should allow New Temporary Organisations.", shipment.Lookups.ConsignorForwarder_List.AllowNewTemporaryOrganisations);
			Assert("Collection should allow New Temporary Organisations.", shipment.Lookups.ConsignorForwarderPickup_List.AllowNewTemporaryOrganisations);
			Assert("Collection should allow New Temporary Organisations.", shipment.Lookups.ConsigneeForwarder_List.AllowNewTemporaryOrganisations);
			Assert("Collection should allow New Temporary Organisations.", shipment.Lookups.ConsigneeForwarderDelivery_List.AllowNewTemporaryOrganisations);

			AssertEquals("Collection should NOT allow other Organisations.", false, shipment.Lookups.ConsignorForwarder_List.AllowOtherOrgTypes);
			Assert("Collection Pickup List should allow other Organisations.", shipment.Lookups.ConsignorForwarderPickup_List.AllowOtherOrgTypes);
			AssertEquals("Collection should Not allow other Organisations.", false, shipment.Lookups.ConsigneeForwarder_List.AllowOtherOrgTypes);
			Assert("Collection Delivery List should allow other Organisations.", shipment.Lookups.ConsigneeForwarderDelivery_List.AllowOtherOrgTypes);

			AssertEquals("IsForwarder should not be set (defaults for FilterBusinessObject", false, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
			AssertEquals("IsForwarder should not be set (defaults for FilterBusinessObject", false, shipment.Lookups.ConsignorForwarderPickup_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));

			AssertEquals("IsConsignor should be set (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"].Value);
			AssertEquals("IsConsignor should be set (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarderPickup_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"].Value);

			AssertEquals("IsForwarder should not be set (defaults for FilterBusinessObject", false, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));
			AssertEquals("IsForwarder should not be set (defaults for FilterBusinessObject", false, shipment.Lookups.ConsigneeForwarderDelivery_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"));

			AssertEquals("IsConsignee should be set (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);

			AssertEquals("IsConsignee should be set (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarderDelivery_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = Factory.New<OrgHeader>().PK;

			AssertEquals("Related Consign on Consignee Collection should exist (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			AssertEquals("Related Consign on Consignee Collection should exist (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarderDelivery_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			AssertEquals("Related Consign should be Empty on Consignee Collection (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value.IsEmpty);
			AssertEquals("Related Consign should be Empty on Consignee Collection (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsigneeForwarderDelivery_List.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value.IsEmpty);

			AssertEquals("Related Consign on Consignor Collection should exist (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals("Related Consign on Consignor Collection should exist (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarderPickup_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			AssertEquals("Related Consign should be Empty on Consignor Collection (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value.IsEmpty);
			AssertEquals("Related Consign should be Empty on Consignor Collection (defaults for FilterBusinessObject)", true, shipment.Lookups.ConsignorForwarderPickup_List.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value.IsEmpty);

			var buyLink = shipment.Consignor.BuyerLinks.AddNew();
			buyLink.OL_OH_Buyer = shipment.Consignee.PK;
			AssertEquals("Value of Related Consign on Consignee Collection (defaults for FilterBusinessObject)", shipment.Consignor.PK, (ZGuid)shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults["Consignee - Related Consignor" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			var supLink = shipment.Consignee.SupplierLinks.AddNew();
			supLink.OL_OH_Supplier = shipment.Consignor.PK;
			AssertEquals("Value of Related Consign on Consignor Collection (defaults for FilterBusinessObject)", shipment.Consignee.PK, (ZGuid)shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults["Consignor - Related Consignee" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Assert("Type of ConsignorForwarder_List when Coload is true.", shipment.Lookups.ConsignorForwarder_List is ForwarderCollection);
			Assert("Type of ConsigneeForwarder_List when Coload is true.", shipment.Lookups.ConsigneeForwarder_List is ForwarderCollection);
			Assert("Collection should allow New Temporary Organisations.", shipment.Lookups.ConsigneeForwarder_List.AllowNewTemporaryOrganisations);
			AssertEquals("Collection should Not allow other Organisations.", false, shipment.Lookups.ConsigneeForwarder_List.AllowOtherOrgTypes);
			Assert("Collection should allow other Organisations.", shipment.Lookups.ConsigneeForwarderDelivery_List.AllowOtherOrgTypes);

			AssertEquals("IsForwarder should be set (defaults for FilterBusinessObject", true, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"].Value);

			AssertEquals("IsConsignor should not be set (defaults for FilterBusinessObject)", false, shipment.Lookups.ConsignorForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property3"));

			AssertEquals("IsForwarder should be set (defaults for FilterBusinessObject", true, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property5"].Value);
			AssertEquals("IsConsignee should not be set (defaults for FilterBusinessObject)", false, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
		}

		public void TestSetServiceLevel()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.RegistryServiceLevel.RS_Code = "STD";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			AssertEquals("STD", shipment.JS_RS_NKServiceLevel);

			Factory.Save();

			var shipment1 = Factory.Load<CommonShipment>(shipment.PK);
			shipment1.JS_RS_NKServiceLevel = "CCC";

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";
			shipment1.ConsigneePK = consignee1.PK;

			var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			shipment1.ConsignorPK = consignor1.PK;

			AssertEquals("AAA", shipment1.JS_RS_NKServiceLevel);

			var shipment2 = Factory.Load<CommonShipment>(shipment.PK);
			shipment2.JS_RS_NKServiceLevel = "CCC";

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			shipment2.ConsigneePK = consignee2.PK;

			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			consignor2.MiscServ.OM_RS_NKEXDefaultServiceLevel = "BBB";
			shipment2.ConsignorPK = consignor2.PK;

			AssertEquals("BBB", shipment2.JS_RS_NKServiceLevel);

			var shipment3 = Factory.Load<CommonShipment>(shipment.PK);
			shipment3.JS_RS_NKServiceLevel = "CCC";

			var consignor3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment3.ConsignorPK = consignor3.PK;

			var consignee3 = Factory.NewWithValidTestData<OrgHeader>();
			shipment3.ConsigneePK = consignee3.PK;

			AssertEquals("CCC", shipment3.JS_RS_NKServiceLevel);

			var consignee4 = Factory.NewWithValidTestData<OrgHeader>();
			consignee4.MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";
			shipment3.ConsigneePK = consignee4.PK;

			var consignor4 = Factory.NewWithValidTestData<OrgHeader>();
			consignor4.MiscServ.OM_RS_NKEXDefaultServiceLevel = "BBB";
			shipment3.ConsignorPK = consignor4.PK;

			AssertEquals("AAA", shipment3.JS_RS_NKServiceLevel);

			var shipment4 = Factory.NewWithValidTestData<CommonShipment>();
			shipment4.RegistryServiceLevel.RS_Code = "STD";

			var consignee5 = Factory.NewWithValidTestData<OrgHeader>();
			consignee5.MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";
			shipment4.ConsigneePK = consignee5.PK;

			var consignor5 = Factory.NewWithValidTestData<OrgHeader>();
			consignor5.MiscServ.OM_RS_NKEXDefaultServiceLevel = "BBB";
			shipment4.ConsignorPK = consignor5.PK;

			AssertEquals("AAA", shipment4.JS_RS_NKServiceLevel);

			var shipment5 = Factory.NewWithValidTestData<CommonShipment>();
			shipment5.RegistryServiceLevel.RS_Code = "STD";

			var consignee6 = Factory.NewWithValidTestData<OrgHeader>();
			shipment5.ConsigneePK = consignee6.PK;

			var consignor6 = Factory.NewWithValidTestData<OrgHeader>();
			consignor6.MiscServ.OM_RS_NKEXDefaultServiceLevel = "BBB";
			shipment5.ConsignorPK = consignor6.PK;

			AssertEquals("BBB", shipment5.JS_RS_NKServiceLevel);
		}

		public void TestOrganisationsWithConsigneesAsDefault()
		{
			var shipment = GetShipment();
			AssertEquals(ZBool.True, shipment.Lookups.ConsigneeForwarder_List.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"].Value);

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var transporter = Factory.NewWithValidTestData<OrgHeader>();
			transporter.OH_IsTransportClient = true;

			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			forwarder.OH_IsForwarder = true;

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_IsWarehouseClient = true;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingConsortium = true;

			var broker = Factory.NewWithValidTestData<OrgHeader>();
			broker.OH_IsBroker = true;

			var service = Factory.NewWithValidTestData<OrgHeader>();
			service.OH_IsMiscFreightServices = true;

			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			competitor.OH_IsCompetitor = true;

			var sales = Factory.NewWithValidTestData<OrgHeader>();
			sales.OH_IsSalesLead = true;

			var alternativeFilter = new ZQuery();
			alternativeFilter.FetchOnlyFromLocalCache = true;
			shipment.Lookups.ConsigneeForwarderDelivery_List.Load(alternativeFilter);
			AssertEquals("Consignee should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(consignee));
			AssertEquals("Consignor should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(consignor));
			AssertEquals("Transporter should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(transporter));
			AssertEquals("Forwarder should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(forwarder));
			AssertEquals("Warehouse should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(warehouse));
			AssertEquals("Carrier should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(carrier));
			AssertEquals("Broker should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(broker));
			AssertEquals("Service should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(service));
			AssertEquals("Competitor should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(competitor));
			AssertEquals("Sales should be included in the collection", true, shipment.Lookups.ConsigneeForwarderDelivery_List.Contains(sales));
		}

		CommonShipment GetShipment()
		{
			return Factory.New<CommonShipment>();
		}

		internal class TestShipment : CommonShipment
		{
			public TestShipment(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ConsolCollection GetNewConsolCollection()
			{
				return new ConsolCollection(this);
			}
		}
	}
}
