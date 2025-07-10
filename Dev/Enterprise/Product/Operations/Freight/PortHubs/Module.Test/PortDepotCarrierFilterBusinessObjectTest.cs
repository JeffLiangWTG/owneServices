using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(PortDepotCarrierFilterStripBusinessObject))]
	public class PortDepotCarrierFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDirection()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_Direction = PortHubSelectionDirectionList.Codes.Delivery;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_Direction = PortHubSelectionDirectionList.Codes.Pickup;

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.Direction];
			filter.Property = PortHubSelectionDirectionList.Codes.Pickup;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = PortHubSelectionDirectionList.Codes.Delivery;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestServiceLevels()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_RS_NKServiceLevel = "D2D";

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_RS_NKServiceLevel = "STD";

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.ServiceLevel];
			filter.Property = "D2D";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = "STD";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestPackTypes()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_F3_NKPackType = Core.Constants.PkgUnit.Drum;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.PackType];
			filter.Property = Core.Constants.PkgUnit.Pallet;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = Core.Constants.PkgUnit.Drum;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestTransportMode()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_RatingFreightMode = Core.Constants.TransportModes.Sea;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_RatingFreightMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.TransportMode];
			filter.Property = Core.Constants.TransportModes.Sea;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = Core.Constants.TransportModes.Air;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestUNDGClass()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_UndgClass = "1";

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_UndgClass = "All";

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.UndgClass];
			filter.Property = "All";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = "1";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestOriginDepot()
		{
			var originDepot1 = Factory.NewWithValidTestData<OrgHeader>();
			var originDepot2 = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_OA_DispatchDepotAddress = originDepot1.MainAddress.PK;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_OA_DispatchDepotAddress = originDepot2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.OriginDepot];
			filter.Property = originDepot1.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = originDepot2.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestDestinationDepot()
		{
			var destinationDepot1 = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot2 = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = destinationDepot1.MainAddress.PK;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = destinationDepot2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.DestinationDepot];
			filter.Property = destinationDepot1.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = destinationDepot2.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestProcessType()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithProcessType(string processType)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_ProcessType = processType;

				return portHubSelection;
			}

			var portHubSelection_HVH = CreatePortHubWithProcessType("HVH");
			var portHubSelection_SHP = CreatePortHubWithProcessType("SHP");
			var portHubSelection_HVL = CreatePortHubWithProcessType("HVL");

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.ProcessType];
			filter.Property = "HVH";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_HVH, collection);
			AssertCollectionNotContainsMany("portHubSelection_HVL and SHP should not be in filtered collection for processType HVH", new List<PortHubSelection> { portHubSelection_SHP, portHubSelection_HVL }, collection);

			filter.Property = "SHP";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_SHP, collection);
			AssertCollectionNotContainsMany("portHubSelection_HVH and HVL should not be in filtered collection for processType SHP", new List<PortHubSelection> { portHubSelection_HVH, portHubSelection_HVL }, collection);

			filter.Property = "HVL";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_HVL, collection);
			AssertCollectionNotContainsMany("portHubSelection_HVH and SHP should not be in filtered collection for processType HVL", new List<PortHubSelection> { portHubSelection_HVH, portHubSelection_SHP }, collection);
		}

		public void TestMasterHouse()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithMasterHouse(bool isMasterHouse)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_IsMasterHouse = isMasterHouse;

				return portHubSelection;
			}

			var portHubSelection_MasterHouse = CreatePortHubWithMasterHouse(true);
			var portHubSelection_NotMasterHouse = CreatePortHubWithMasterHouse(false);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.MasterHouse];

			filter.Property0 = true;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_MasterHouse, collection);
			AssertCollectionNotContains(portHubSelection_NotMasterHouse, collection);

			filter.Property0 = false;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_NotMasterHouse, collection);
			AssertCollectionNotContains(portHubSelection_MasterHouse, collection);
		}

		public void TestShipper()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithShipperAddress(ZGuid oaPK)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_OA_ShipperAddress = oaPK;

				return portHubSelection;
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var portHubSelection1 = CreatePortHubWithShipperAddress(orgHeader.MainAddress.PK);
			var portHubSelection2 = CreatePortHubWithShipperAddress(ZGuid.Empty);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.Shipper];

			filter.Property = orgHeader.PK;
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);

			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestCarrier()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithShipperAddress(ZGuid ohPK)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_OH_Carrier = ohPK;

				return portHubSelection;
			}

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var portHubSelection1 = CreatePortHubWithShipperAddress(orgHeader.PK);
			var portHubSelection2 = CreatePortHubWithShipperAddress(ZGuid.Empty);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.Carrier];

			filter.Property = orgHeader.PK;
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);

			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestWeightUnit()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithWeightUnit(string weightUnit)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_WeightUQ = weightUnit;

				return portHubSelection;
			}

			var portHubSelection_KG = CreatePortHubWithWeightUnit("KG");
			var portHubSelection_G = CreatePortHubWithWeightUnit("G");

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.WeightUnit];

			filter.Property = "KG";
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_KG, collection);
			AssertCollectionNotContains(portHubSelection_G, collection);

			filter.Property = "G";
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_G, collection);
			AssertCollectionNotContains(portHubSelection_KG, collection);
		}

		public void TestVolumeUnit()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			PortHubSelection CreatePortHubWithVolumeUnit(string volumeUnit)
			{
				var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
				portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
				portHubSelection.TY_VolumeUQ = volumeUnit;

				return portHubSelection;
			}

			var portHubSelection_L = CreatePortHubWithVolumeUnit("L");
			var portHubSelection_M3 = CreatePortHubWithVolumeUnit("M3");

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.VolumeUnit];

			filter.Property = "L";
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_L, collection);
			AssertCollectionNotContains(portHubSelection_M3, collection);

			filter.Property = "m3";
			filter.IsActive = true;
			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection_M3, collection);
			AssertCollectionNotContains(portHubSelection_L, collection);
		}

		PortHubSelection CreatePortHubWithWeights(ZDecimal minweight, ZDecimal maxWeight)
		{
			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection.TY_MinWeight = minweight;
			portHubSelection.TY_MaxWeight = maxWeight;

			return portHubSelection;
		}

		PortHubSelection CreatePortHubWithVolumes(ZDecimal minVolume, ZDecimal maxVolume)
		{
			var portHubSelection = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection.TY_MinVolume = minVolume;
			portHubSelection.TY_MaxVolume = maxVolume;

			return portHubSelection;
		}

		void AssertMeasurementRange(ModuleNumberRangeFilter filter, PortDepotCarrierFilterStripBusinessObject filterBizO, PortHubSelection portHubSelection1, PortHubSelection portHubSelection2, PortHubSelectionCollection collection, ZString filterDescription)
		{
			filter.Property1 = 5.0;
			filter.Property2 = 9.0;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains($"Range [5, 9] fits under PortHubSelection1 with {filterDescription} 5", portHubSelection1, collection);
			AssertCollectionNotContains($"Range[5, 9] does not fit under PortHubSelection2 {filterDescription} 10", portHubSelection2, collection);

			filter.Property1 = 5.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContainsMany($"Range [5,10] fits under PortHubSelection1 with {filterDescription} 5 and PortHubSelection2 with {filterDescription} 10", new List<PortHubSelection> { portHubSelection1, portHubSelection2 }, collection);

			filter.Property1 = 6.0;
			filter.Property2 = 10.0;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionNotContains($"Range [6, 10] does not fit under PortHubSelection1 with {filterDescription} of 5", portHubSelection1, collection);
			AssertCollectionContains($"Range [6, 10] fits under PortHubSelection2 with {filterDescription} of 10", portHubSelection2, collection);
		}

		public void TestWeightMax()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = CreatePortHubWithWeights(0.0, 5.0);
			var portHubSelection2 = CreatePortHubWithWeights(0.0, 10.0);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.WeightMax];
			AssertMeasurementRange(filter, filterBizO, portHubSelection1, portHubSelection2, collection, PortDepotCarrierFilterStripBusinessObject.Descriptions.WeightMax);
		}

		public void TestWeightMin()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = CreatePortHubWithWeights(5.0, 1000.0);
			var portHubSelection2 = CreatePortHubWithWeights(10.0, 1000.0);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.WeightMin];

			AssertMeasurementRange(filter, filterBizO, portHubSelection1, portHubSelection2, collection, PortDepotCarrierFilterStripBusinessObject.Descriptions.WeightMin);
		}

		public void TestVolumeMax()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = CreatePortHubWithVolumes(0, 5.0);
			var portHubSelection2 = CreatePortHubWithVolumes(0, 10.0);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.VolumeMax];

			AssertMeasurementRange(filter, filterBizO, portHubSelection1, portHubSelection2, collection, PortDepotCarrierFilterStripBusinessObject.Descriptions.VolumeMax);
		}

		public void TestVolumeMin()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;
			var portHubSelection1 = CreatePortHubWithVolumes(5.0, 1000.0);
			var portHubSelection2 = CreatePortHubWithVolumes(10.0, 1000.0);

			Factory.Save();

			var filterBizO = new PortDepotCarrierFilterStripBusinessObject();
			var filter = (ModuleNumberRangeFilter)filterBizO[PortDepotCarrierFilterStripBusinessObject.Descriptions.VolumeMin];

			AssertMeasurementRange(filter, filterBizO, portHubSelection1, portHubSelection2, collection, PortDepotCarrierFilterStripBusinessObject.Descriptions.VolumeMin);
		}

		void AssertCollectionContainsMany(string message, IEnumerable<PortHubSelection> items, PortHubSelectionCollection collection)
		{
			foreach (var item in items)
			{
				AssertCollectionContains(message, item, collection);
			}
		}

		void AssertCollectionNotContainsMany(string message, IEnumerable<PortHubSelection> items, PortHubSelectionCollection collection)
		{
			foreach (var item in items)
			{
				AssertCollectionNotContains(message, item, collection);
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PortDepotCarrierFilterStripBusinessObject();
		}
	}
}
