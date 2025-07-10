using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVItemLookupTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerNumberList()
		{
			var type = typeof(HVLVItem);
			AssertHasCustomAttribute<ListAttribute>(type, AutoHVLVItem.Schema.HVI_ContainerNumber, false, a => a.ListDataSourceMember == "Lookups.ContainerNumber_List");
			var item = Factory.New<HVLVItem>();
			AssertNull("pre-condition", item.Shipment);
			AssertNull("ContainerNumber_List should be null", item.Lookups.ContainerNumber_List);

			var shipment = Factory.New<ForwardingShipment>();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "CONSOL001";
			consol2.JK_UniqueConsignRef = "CONSOL002";
			consol1.Containers.AddNew().JC_ContainerNum = "CONTAINER001";
			consol2.Containers.AddNew().JC_ContainerNum = "CONTAINER002";

			AssertNotNull("pre-condition", item.Shipment);
			AssertNotNull("ContainerNumber_List should not be null", item.Lookups.ContainerNumber_List);
			Assert("ContainerNumber_List should only have 2 items", item.Lookups.ContainerNumber_List.ContainsOnly("CONTAINER001", "CONTAINER002"));

			AssertEquals("CONSOL001", item.Lookups.ContainerNumber_List.GetDescriptionFromCode("CONTAINER001"));
			AssertEquals("CONSOL002", item.Lookups.ContainerNumber_List.GetDescriptionFromCode("CONTAINER002"));
		}

		public void TestHVI_JS_LoadedOnShipment_List()
		{
			var shipment1 = HVLVTestHelper.GetNewShipment(Factory, "S0100000", "HVL");
			shipment1.JS_IsBooking = false;

			var shipment2 = HVLVTestHelper.GetNewShipment(Factory, "S0100001", "HVL");
			shipment2.JS_IsBooking = true;

			var shipment3 = HVLVTestHelper.GetNewShipment(Factory, "S0100002", "STD");
			shipment3.JS_IsBooking = false;

			var shipment4 = HVLVTestHelper.GetNewShipment(Factory, "S0100003", "STD");
			shipment4.JS_IsBooking = true;

			var item = Factory.New<HVLVItem>();
			var lookups = item.Lookups.HVI_JS_LoadedOnShipment_List;
			lookups.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "S0100000" }, lookups.Cast<CommonShipment>().Select(x => x.JS_UniqueConsignRef));
		}

		public void TestHVI_HVO_OuterPackage_List()
		{
			var outerPackage1 = Factory.New<HVLVOuterPackage>();
			var outerPackage2 = Factory.New<HVLVOuterPackage>();
			var outerPackage3 = Factory.New<HVLVOuterPackage>();

			var item = Factory.New<HVLVItem>();
			var lookups = item.Lookups.HVI_HVO_OuterPackage_List;

			AssertContainsExactElementsInAnyOrder(new[] { outerPackage1, outerPackage2, outerPackage3 }, lookups);
		}

		public void TestHVI_HVL_LoadList_List()
		{
			var loadList1 = Factory.New<HVLVOriginLoadList>();
			loadList1.HVL_Status = "BKD";

			var loadList2 = Factory.New<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";

			var loadList3 = Factory.New<HVLVOriginLoadList>();
			loadList3.HVL_Status = "XYZ";

			var item = Factory.New<HVLVItem>();
			var lookups = item.Lookups.HVI_HVL_LoadList_List;

			AssertContainsExactElementsInAnyOrder(new[] { loadList2 }, lookups);
		}

		public void TestHVI_CarrierBookingStatus_List()
		{
			var item = Factory.New<HVLVItem>();
			var lookups = item.Lookups.HVI_CarrierBookingStatus_List;

			var hviCarrierBookingStatusList = new[] { "NON", "BKQ", "BKJ", "BKL", "BKC" };

			AssertEquals("HVI Status List Count", 5, lookups.Count);
			AssertContainsExactElementsInAnyOrder("HVI Carrier Booking Status List", hviCarrierBookingStatusList, lookups.GetAllCodes());
		}

		public void TestHVI_Status_List()
		{
			var item = Factory.New<HVLVItem>();
			var lookups = item.Lookups.HVI_Status_List;

			var hviStatusList = new[] { "MAN", "ROE", "LLA", "LDG", "SHP", "DEP", "ARV", "SUD", "PCD", "DDD", "RDX", "RLM", "SZD", "DHS", "SSD", "DLC", "PUP", "PRF", "OND", "DLV", "RTS", "RDP", "SUS", "LOS", "TRC", "UNK", "POD", "PUS", "RAO", "DFA" };

			AssertEquals("HVI Status List Count", 30, lookups.Count);
			AssertContainsExactElementsInAnyOrder("HVI Status List", hviStatusList, lookups.GetAllCodes());
		}

		public void TestGivenHVLVOriginLoadListStatusIsOpen_WhenHVLVItemDoesNotHaveALinkedShipment_ThenHVLVItemCanBeAddedToHVLVOriginLoadListWithOPNStatus()
		{
			var factory = new BusinessObjectFactory();

			var openLoadList = factory.New<HVLVOriginLoadList>();
			var closedLoadList = factory.New<HVLVOriginLoadList>();

			openLoadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			closedLoadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Closed;

			var linkedHVLVItem = factory.NewWithValidTestData<HVLVItem>();
			var unlinkedHVLVItem = factory.NewWithValidTestData<HVLVItem>();

			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			linkedHVLVItem.HVI_JS_LoadedOnShipment = shipment.PK;
			unlinkedHVLVItem.HVI_JS_LoadedOnShipment = Guid.Empty;

			var linkedLookup = linkedHVLVItem.Lookups.HVI_HVL_LoadList_List;
			var unlinkedLookup = unlinkedHVLVItem.Lookups.HVI_HVL_LoadList_List;

			AssertContainsExactElementsInAnyOrder("Expected openLoadList to not be in the linked HVLVItem's originLoadList lookup", new HVLVOriginLoadListCollection(new BusinessObjectFactory()), linkedLookup);
			AssertContainsExactElementsInAnyOrder("Expected closedLoadList to  be in the linked HVLVItem's originLoadList lookup", new[] { openLoadList }, unlinkedLookup);
		}
	}
}
