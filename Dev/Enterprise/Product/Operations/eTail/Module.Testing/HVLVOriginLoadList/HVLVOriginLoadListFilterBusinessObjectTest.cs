using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVOriginLoadListFilterBusinessObject))]
	class HVLVOriginLoadListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Numbers and References

		public void TestLoadListNo()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_UniqueReference = "LOADLIST1";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_UniqueReference = "LOADLIST2";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Load List #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "LOADLIST2";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestMasterBillNo()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_MasterBillNumber = "111-1111111";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_MasterBillNumber = "222-2222222";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Master Bill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "222-2222222";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestHouseBillNo()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_HouseBillNumber = "111-1111111";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_HouseBillNumber = "222-2222222";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["House Bill #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "222-2222222";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestVoyageVessel()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_VoyageFlight = "VOYAGE123";
			loadList1.HVL_VesselName = "BARRY WONG";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_VoyageFlight = "VOYAGE123";
			loadList2.HVL_VesselName = "HAY WHO FROO DAT";
			var loadList3 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList3.HVL_VoyageFlight = "VOYAGE456";
			loadList3.HVL_VesselName = "HAY WHO FROO DAT";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (VoyageVesselModuleFilter)filterBizO["Voyage / Vessel"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "VOYAGE123";
			filter.Vessel = "HAY WHO FROO DAT";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.VoyageFlightNo = "";
			filter.Vessel = "BARRY";
			filter.IsActive = true;

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { loadList1 }, results);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.VoyageFlightNo = "";
			filter.Vessel = "";
			filter.IsActive = true;

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { loadList1, loadList2, loadList3 }, results);
		}

		public void TestContainerNo()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_ContainerNumber = "CONTAINER1";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_ContainerNumber = "CONTAINER2";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Container #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CONTAINER2";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestItemId()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item1 = bookingHeader1.Consignments.AddNew().Items.AddNew();
			item1.HVI_HVL_LoadList = loadList1.PK;

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var item2 = bookingHeader2.Consignments.AddNew().Items.AddNew();
			item2.HVI_HVL_LoadList = loadList2.PK;

			Factory.Save();

			item1.HVI_ItemId = "ITEM1";
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Item ID"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ITEM2";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestConsignmentId()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
				var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

				var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment1 = bookingHeader1.Consignments.AddNew();
				consignment1.HVC_WaybillNumber = "CONSIGN1";
				var item1 = consignment1.Items.AddNew();
				item1.HVI_HVL_LoadList = loadList1.PK;
				item1.HVI_ItemId = "ITEM1";

				var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
				var consignment2 = bookingHeader2.Consignments.AddNew();
				consignment2.HVC_WaybillNumber = "CONSIGN2";
				var item2 = consignment2.Items.AddNew();
				item2.HVI_HVL_LoadList = loadList2.PK;
				item2.HVI_ItemId = "ITEM2";

				Factory.Save();

				consignment1.HVC_WaybillNumber = "WAYBILL1";
				consignment2.HVC_WaybillNumber = "WAYBILL2";

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1", consignment1.HVC_ConsignmentId);
				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2", consignment2.HVC_ConsignmentId);

				var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBizO["Consignment ID"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "CONSIGN2";
				filter.IsActive = true;

				var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

				AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
			}
		}

		public void TestShipperReference()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1 = bookingHeader1.Consignments.AddNew();
			consignment1.HVC_ShipperReference = "SHPREF1";
			var item1 = consignment1.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList1.PK;
			item1.HVI_ItemId = "ITEM1";

			var bookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2 = bookingHeader2.Consignments.AddNew();
			consignment2.HVC_ShipperReference = "SHPREF2";
			var item2 = consignment2.Items.AddNew();
			item2.HVI_HVL_LoadList = loadList2.PK;
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHPREF2";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Modes and Types

		public void TestServiceLevel()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_RS_NKServiceLevel = "STD";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_RS_NKServiceLevel = "D2D";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Service Level"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "D2D";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestTransportMode()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_TransportMode = "SEA";
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_TransportMode = "AIR";

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Transport Mode"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "AIR";
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestContainerType()
		{
			var ref20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var ref40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_RC_ContainerType = ref20GP.PK;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_RC_ContainerType = ref40GP.PK;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Container Type"];
			filter.Property = ref40GP.PK;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Dates

		public void TestETD()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_E_Dep = new ZDateTime(2016, 6, 15);
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_E_Dep = new ZDateTime(2016, 7, 15);

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleDateFilter)filterBizO["ETD"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 7, 1);
			filter.Property2 = new ZDateTime(2016, 7, 30);
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestETA()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_E_Arv = new ZDateTime(2016, 6, 15);
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_E_Arv = new ZDateTime(2016, 7, 15);

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleDateFilter)filterBizO["ETA"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2016, 7, 1);
			filter.Property2 = new ZDateTime(2016, 7, 30);
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Organisations and Staff

		public void TestOriginDepot()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_OA_OriginDepot = org1.MainAddress.PK;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_OA_OriginDepot = org2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Origin Depot"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestDestinationDepot()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_OA_DestinationDepot = org1.MainAddress.PK;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_OA_DestinationDepot = org2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Destination Depot"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		public void TestCargoTerminalOperator()
		{
			var cto1 = Factory.NewWithValidTestData<OrgHeader>();
			var cto2 = Factory.NewWithValidTestData<OrgHeader>();

			var ctoAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var ctoAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			ctoAddress1.OA_OH = cto1.PK;
			ctoAddress2.OA_OH = cto2.PK;
			ctoAddress1.OA_Code = "ABC";
			ctoAddress2.OA_Code = "XYZ";

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			var agent1 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var agent2 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			var agent3 = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			agent1.O5_OH = carrier1.PK;
			agent2.O5_OH = carrier2.PK;
			agent3.O5_OH = carrier3.PK;
			agent1.O5_OA_AgentOfficeAddress = ctoAddress1.PK;
			agent2.O5_OA_AgentOfficeAddress = ctoAddress2.PK;
			agent3.O5_OA_AgentOfficeAddress = ctoAddress2.PK;
			agent1.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Stevedore;
			agent2.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.AirCTO;
			agent3.O5_SeaAirCarrierOrForwarderType = CarrierOrForwarderType.Codes.Stevedore;

			var originDepot1 = Factory.NewWithValidTestData<OrgAddress>();
			var originDepot2 = Factory.NewWithValidTestData<OrgAddress>();
			var originDepot3 = Factory.NewWithValidTestData<OrgAddress>();

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_OH_Carrier = carrier1.PK;
			loadList1.HVL_OA_OriginDepot = originDepot1.PK;
			loadList1.HVL_TransportMode = Core.Constants.TransportModes.Sea;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_OH_Carrier = carrier2.PK;
			loadList2.HVL_OA_OriginDepot = originDepot2.PK;
			loadList2.HVL_TransportMode = Core.Constants.TransportModes.Air;
			var loadList3 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList3.HVL_OH_Carrier = carrier3.PK;
			loadList3.HVL_OA_OriginDepot = originDepot3.PK;
			loadList3.HVL_TransportMode = Core.Constants.TransportModes.Air;

			agent1.O5_PortOrCountry = originDepot1.OA_RL_NKRelatedPortCode;
			agent2.O5_PortOrCountry = originDepot2.OA_RL_NKRelatedPortCode;
			agent3.O5_PortOrCountry = originDepot3.OA_RL_NKRelatedPortCode;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Cargo Terminal Operator"];
			filter.Property = cto1.PK;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Expected filter to find load list with cto1", new[] { loadList1 }, results);

			filter.Property = cto2.PK;

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Expected filter to find load list with cto2", new[] { loadList2 }, results);

			loadList3.HVL_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Expected filter to find load lists with cto2", new[] { loadList2, loadList3 }, results);

			agent3.O5_PortOrCountry = "RANDM";
			Factory.Save();

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder("Expected filter to find load list with cto2", new[] { loadList2 }, results);

			filter.Property = carrier1.PK;

			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertEquals("Expected filter not to match with any load list as the carrier in the search parameter is not a CTO", 0, results.Count());
		}

		public void TestCarrier()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_OH_Carrier = org1.PK;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_OH_Carrier = org2.PK;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO["Carrier"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Status and Flags

		public void TestLoadListStatus()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = HVLVOriginLoadListStatus.Codes.Consolidated;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Load List Status"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = HVLVOriginLoadListStatus.Codes.Consolidated;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Is Master House

		public void TestIsMasterHouseFilter()
		{
			var isMasterHouseTrueLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			isMasterHouseTrueLoadList.HVL_IsMasterHouse = true;
			var isMasterHouseFalseloadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			isMasterHouseFalseloadList.HVL_IsMasterHouse = false;

			Factory.Save();

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterBizO["Is Master House"];
			filter.Property0 = true;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { isMasterHouseTrueLoadList }, results);

			filter.Property0 = false;
			results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { isMasterHouseFalseloadList }, results);
		}

		#endregion

		#region INCO

		public void TestINCODropdownFilter()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_INCO = Core.Constants.IncoTerms.UnpackedAtFactory;
			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_INCO = Core.Constants.IncoTerms.PackedAtFactory;

			var filterBizO = new HVLVOriginLoadListFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizO["Incoterm"];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Core.Constants.IncoTerms.PackedAtFactory;
			filter.IsActive = true;

			var results = new HVLVOriginLoadListCollection(Factory).Find(filterBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, results);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new HVLVOriginLoadListFilterBusinessObject();

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter("OrgAddress", "Origin Depot"));
			result.Add(TableFilter("OrgAddress", "Destination Depot"));

			return result;
		}

		#endregion
	}
}
