using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsLocationViewLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestAreas

		public void TestAreas()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			Helper.CreateArea(warehouse, "P1", "", true, false);
			Helper.CreateArea(warehouse, "P2", "", false, true);
			Helper.CreateArea(warehouse, "P3", "", true, true);
			Factory.Save();

			var location = Factory.New<WhsLocation>();
			AssertEquals(typeof(WhsAreaCollection), location.Lookups.PickingAreas.GetType());
			AssertEquals("If there is no Warehouse, Areas collection should be empty.", 0, location.Lookups.PickingAreas.Count);

			location.WLV_WR = warehouse.DefaultLocation.WLV_WR;
			var pickingAreas = warehouse.Areas.Where(a => a.WA_Name != "P2").ToArray();
			AssertContainsExactElementsInAnyOrder(pickingAreas, location.Lookups.PickingAreas);
			AssertEquals(pickingAreas.Length, location.Lookups.PickingAreaList.Count);
			AssertContainsExactElementsInAnyOrder(pickingAreas.Select(a => a.WA_Name), location.Lookups.PickingAreaList.GetAllCodes());

			var putawayAreas = warehouse.Areas.Where(a => a.WA_Name != "P1").ToArray();
			AssertContainsExactElementsInAnyOrder(putawayAreas, location.Lookups.PutawayAreas);
			AssertEquals(putawayAreas.Length, location.Lookups.PutawayAreaList.Count);
			AssertContainsExactElementsInAnyOrder(putawayAreas.Select(a => a.WA_Name), location.Lookups.PutawayAreaList.GetAllCodes());

			var locationWithoutWarehouse = Factory.New<WhsLocation>();
			AssertEquals("If there is no Warehouse, Picking Areas collection should be empty.", 0, locationWithoutWarehouse.Lookups.PickingAreaList.Count);
			AssertEquals("If there is no Warehouse, Putaway Areas collection should be empty.", 0, locationWithoutWarehouse.Lookups.PutawayAreaList.Count);
		}

		public void TestPickingAreasCached()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			Helper.CreateArea(warehouse, "P1", "", true, false);
			Helper.CreateArea(warehouse, "P2", "", false, true);
			Helper.CreateArea(warehouse, "P3", "", true, true);
			Factory.Save();

			var location = Factory.New<WhsLocation>();
			location.WLV_WR = warehouse.DefaultLocation.WLV_WR;
			var pickingAreas = warehouse.Areas.Where(a => a.WA_Name != "P2").ToArray();
			AssertContainsExactElementsInAnyOrder(pickingAreas, location.Lookups.PickingAreas);
			AssertEquals(pickingAreas.Length, location.Lookups.PickingAreaList.Count);
			AssertContainsExactElementsInAnyOrder(pickingAreas.Select(a => a.WA_Name), location.Lookups.PickingAreaList.GetAllCodes());

			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<WhsAreaCollection>($"WhsAreaCollection|GetPickingAreas|{warehouse.PK}", () => null));
		}

		public void TestPutawayAreasCached()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			Helper.CreateArea(warehouse, "P1", "", true, false);
			Helper.CreateArea(warehouse, "P2", "", false, true);
			Helper.CreateArea(warehouse, "P3", "", true, true);
			Factory.Save();

			var location = Factory.New<WhsLocation>();
			location.WLV_WR = warehouse.DefaultLocation.WLV_WR;
			var putawayAreas = warehouse.Areas.Where(a => a.WA_Name != "P1").ToArray();
			AssertContainsExactElementsInAnyOrder(putawayAreas, location.Lookups.PutawayAreas);
			AssertEquals(putawayAreas.Length, location.Lookups.PutawayAreaList.Count);
			AssertContainsExactElementsInAnyOrder(putawayAreas.Select(a => a.WA_Name), location.Lookups.PutawayAreaList.GetAllCodes());

			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<WhsAreaCollection>($"WhsAreaCollection|GetPutawayAreas|{warehouse.PK}", () => null));
		}

		#endregion

		#region TestAreaList

		public void TestAreaList_Translatable()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A");
			var area = Helper.CreateArea(warehouse, "Area");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "ROW");
			Factory.Save();

			var location = row.Locations.Single();
			AssertCollectionContains("Precondition", "Area", location.Lookups.PickingAreaList.GetAllCodes());
			AssertCollectionContains("Precondition", "Area", location.Lookups.PutawayAreaList.GetAllCodes());
			Factory.Save();

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Area").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				var newFactory = new BusinessObjectFactory();
				var locationInNewFactory = newFactory.Load<WhsLocation>(location.PK);
				mockRes.Put(resKey, new ResourceStringData(resKey, "库区"));

				AssertCollectionContains("库区", locationInNewFactory.Lookups.PickingAreaList.GetAllCodes());
				AssertCollectionContains("库区", locationInNewFactory.Lookups.PutawayAreaList.GetAllCodes());
			}
		}

		public void TestAreaList_PickingAreaListCached()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A");
			var area = Helper.CreateArea(warehouse, "Area");
			Factory.Save();

			var location = Factory.New<WhsLocation>();
			location.WLV_WR = warehouse.DefaultLocation.WLV_WR;
			Assert("Precondition", location.Lookups.PickingAreaList.Count > 0);
			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<CodeDescriptionPairList>($"PickingAreaList|{warehouse.PK}", () => null));
		}

		public void TestAreaList_PutawayAreaListCached()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A");
			var area = Helper.CreateArea(warehouse, "Area");
			Factory.Save();

			var location = Factory.New<WhsLocation>();
			location.WLV_WR = warehouse.DefaultLocation.WLV_WR;
			Assert("Precondition", location.Lookups.PutawayAreaList.Count > 0);
			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<CodeDescriptionPairList>($"PutawayAreaList|{warehouse.PK}", () => null));
		}

		#endregion

		#region TestApprovedKnownStatuses

		public virtual void TestApprovedKnownStatuses()
		{
			AssertEquals(true, Lookups.ApprovedKnownStatuses.ContainsCode(CodeLists.ApprovedKnownStatus.Codes.NO));
		}

		public void TestApprovedKnownStatuses_Cached() => TestApprovedKnownStatuses_CachedCore();

		protected virtual void TestApprovedKnownStatuses_CachedCore()
		{
			AssertNotNull(Lookups.ApprovedKnownStatuses);
			AssertNotNull("Cache should exist on factory", Factory.GetCachedValue<ApprovedKnownStatus>($"WhsLocationViewLookups|ApprovedKnownStatuses", () => null));
		}

		#endregion

		#region TestWeightUnits

		public void TestWeightUnits()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.WeightUnits);
			AssertEquals(true, lookups.WeightUnits.ContainsCode("KG"));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.WeightUnits);
		}

		#endregion

		#region TestCubicUnits

		public void TestCubicUnits()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.CubicUnits);
			AssertEquals(true, lookups.CubicUnits.ContainsCode("M3"));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Volume), lookups.CubicUnits);
		}

		#endregion

		#region TestDimensionUnits

		public void TestDimensionUnits()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.DimensionUnits);
			AssertEquals(true, lookups.DimensionUnits.ContainsCode("M"));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Length), lookups.DimensionUnits);
		}

		#endregion

		#region TestLocations

		public void TestLocations()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "ZZ";

			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Code = "AAAA";
			zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.TransitWarehouse.Code;

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_Code = "BBBBB";

			Factory.Save();

			var whsLocation = Factory.New<WhsLocation>();
			var query = new ZQuery(RefCountrySchema.RN_Code, "ZZ");
			whsLocation.Lookups.Locations.LoadCountry(query);
			AssertCollectionContains(country, whsLocation.Lookups.Locations);

			query = new ZQuery(RefZoneHeaderSchema.FZ_Code, "AAAA");
			whsLocation.Lookups.Locations.LoadZone(query);
			AssertCollectionContains(zone, whsLocation.Lookups.Locations);

			query = new ZQuery(RefUNLOCOSchema.RL_Code, "BBBBB");
			whsLocation.Lookups.Locations.LoadUNLoco(query);
			AssertCollectionContains(unloco, whsLocation.Lookups.Locations);
		}

		public void TestLocations_OnlyContainTransitWarehouseTypeAndAllTypeZones()
		{
			var zoneTransit = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneTransit.FZ_Code = "TTTT";
			zoneTransit.FZ_ZoneType = ZoneTypeCodeDescriptionPair.TransitWarehouse.Code;

			var zoneAllType = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneAllType.FZ_Code = "AAAA";
			zoneAllType.FZ_ZoneType = ZoneTypeCodeDescriptionPair.All.Code;

			var zoneReporting = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneReporting.FZ_Code = "RRRR";
			zoneReporting.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Reporting.Code;

			var location = Factory.New<WhsLocation>();
			var zoneQuery = new ZQuery();
			location.Lookups.Locations.LoadZone(zoneQuery);
			AssertCollectionContains("Should contain a zone with zone type equals to TransitWarehouse.", zoneTransit, location.Lookups.Locations);
			AssertCollectionContains("Should contain a zone with zone type equals to All.", zoneAllType, location.Lookups.Locations);
			AssertCollectionNotContains("Should not contain a zone with zone type not equal to TransitWarehouse", zoneReporting, location.Lookups.Locations);
		}

		#endregion

		#region TestLocationTypes

		public void TestLocationTypes()
		{
			Helper.CreateLocationType("123");

			AssertEquals(7, Lookups.LocationTypes.Count);
			// user created location types
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "123");
			// system location types
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "DOC");
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "OPN");
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "RDO");
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "RNO");
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "PFC");
			Lookups.LocationTypes.Single(lt => lt.WLT_Code == "DPF");
		}

		#endregion

		#region TestTransitServiceLevels

		public void TestTransitServiceLevels()
		{
			var activeServiceLevel = Helper.CreateServiceLevel("ACT", "Active");
			var inactiveServiceLevel = Helper.CreateServiceLevel("INA", "Inactive", false);

			var lookupServiceLevelCodes = Lookups.TransitServiceLevels.Select(s => s.RS_Code);
			AssertEquals("Should contain Active Service Levels", true, lookupServiceLevelCodes.Contains("ACT"));
			AssertEquals("Should *not* contain Inactive Service Levels", false, lookupServiceLevelCodes.Contains("INA"));
		}

		#endregion

		#region Implementation

		protected virtual WhsLocationViewLookups GetNewLookups()
		{
			return new WhsLocationViewLookups(Factory.New<WhsLocation>());
		}

		protected WhsLocationViewLookups Lookups => lookups ?? (lookups = GetNewLookups());
		WhsLocationViewLookups lookups;

		#endregion
	}
}
