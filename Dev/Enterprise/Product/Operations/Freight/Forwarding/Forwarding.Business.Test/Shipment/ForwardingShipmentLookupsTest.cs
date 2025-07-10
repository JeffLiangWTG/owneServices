using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestScreeningStatusesList()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertEquals(typeof(ScreeningStatusesList), shipment.Lookups.ScreeningStatusesList.GetType());
		}

		public void TestJS_Phase_Lookups()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var commonList = PhaseConstants.GetCommonPhaseList();
			AssertContainsExactElementsInAnyOrder(commonList, shipment.Lookups.Phases);

			var security = new PhaseSecurity(PhaseConstants.GetShipmentLocationsList());
			var phase1 = security.Phases.AddNew();
			phase1.Code = "AAA";
			phase1.Description = (NoResString)"Hello";

			var phase2 = security.Phases.AddNew();
			phase2.Code = "BBB";
			phase2.Description = (NoResString)"World";

			var rule = phase1.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			phase2.Rules.Add(rule);
			ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			var expectedList = PhaseConstants.GetCommonPhaseList();
			expectedList.AddPair("AAA", "Hello");
			expectedList.AddPair("BBB", "World");

			shipment = new BusinessObjectFactory().New<ForwardingShipment>();
			AssertContainsExactElementsInAnyOrder(expectedList, shipment.Lookups.Phases);
		}

		public void TestJS_ShipmentStatus_List()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertContainsExactElementsInAnyOrder(new[] { "ESI", "CNF", "SIJ" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsShipping = false;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsBooking = true;
			AssertContainsExactElementsInAnyOrder(new[] { "EBK", "BKD", "BKX", "EBC", "BKJ" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());

			shipment.JS_IsForwardRegistered = true;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Amendment;
			AssertContainsExactElementsInAnyOrder(new[] { "AMD", "BKD" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			AssertContainsExactElementsInAnyOrder(new[] { "AMD", "BKD" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());

			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Amendment;
			AssertContainsExactElementsInAnyOrder(new[] { "AMD", "BKD" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());
			Factory.Save();

			shipment.JS_ShipmentStatus = ZString.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { "AMD", "BKD" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());
		}

		public void TestJS_ShipmentStatus_List_Booking_ShipmentStatus_is_EBK()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsShipping = false;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsBooking = true;
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			AssertContainsExactElementsInAnyOrder(new[] { "EBK", "BKD", "BKJ" }, shipment.Lookups.JS_ShipmentStatus_List.GetAllCodes());
		}

		public void TestServiceLevelOrTransitTimeCollection()
		{
			var transportModes = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Sea };
			foreach (var transportMode in transportModes)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = transportMode;

				using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
				{
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_CY);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_CY);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.CY_DOOR);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_CY);
					AssertCollectionIsTransitTimeCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR);
					AssertCollectionIsServiceLevelCollection(shipment, Core.Constants.HBLDeliveryModes.Codes.PORT_PORT);
				}

				using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
				{
					AssertEquals(
						"Type of ServiceLevelOrTransitTimeCollection should be ActiveServiceLevelCollection when DDD registry is disabled.",
						typeof(ActiveServiceLevelCollection),
						shipment.Lookups.ServiceLevelOrTransitTimeCollection.GetType());
				}
			}
		}

		void AssertCollectionIsTransitTimeCollection(ForwardingShipment shipment, ZString hblDeliveryMode)
		{
			shipment.JS_HBLContainerPackModeOverride = hblDeliveryMode;
			AssertEquals(
				$"Type of ServiceLevelOrTransitTimeCollection [{shipment.JS_TransportMode}][{hblDeliveryMode}] should be TransitTimeServiceLevelCombinationCollection.",
				typeof(TransitTimeServiceLevelCombinationCollection), shipment.Lookups.ServiceLevelOrTransitTimeCollection.GetType());
		}

		void AssertCollectionIsServiceLevelCollection(ForwardingShipment shipment, ZString hblDeliveryMode)
		{
			shipment.JS_HBLContainerPackModeOverride = hblDeliveryMode;
			AssertEquals(
				$"Type of ServiceLevelOrTransitTimeCollection [{shipment.JS_TransportMode}][{hblDeliveryMode}] should be ActiveServiceLevelCollection.",
				typeof(ActiveServiceLevelCollection), shipment.Lookups.ServiceLevelOrTransitTimeCollection.GetType());
		}

		public void TestServiceLevelOrTransitTimeCollection_FallbackToServiceLevel()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var deliveryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			deliveryAddress.Address1 = "357/4 Bindon Place";
			deliveryAddress.Postcode = "2217";
			deliveryAddress.StateCode = "NSW";
			deliveryAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = deliveryAddress.OA_OH;

			var deliverAgentHeader = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCFSHeader = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OA_ImportReleaseDepot = deliverAgentHeader.MainAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = pickupCFSHeader.MainAddress.PK;

			var domesticDeliverZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			domesticDeliverZoneSet.TP_OH_RelatedParty = deliverAgentHeader.PK;
			var domesticDeliverZone = Factory.NewWithValidTestData<RateTransportZone>();
			domesticDeliverZone.TZ_TP = domesticDeliverZoneSet.PK;

			var domesticPickupZoneSet = Factory.NewWithValidTestData<RateTransportProvider>();
			domesticPickupZoneSet.TP_OH_RelatedParty = pickupCFSHeader.PK;
			var domesticPickupZone = Factory.NewWithValidTestData<RateTransportZone>();
			domesticPickupZone.TZ_TP = domesticPickupZoneSet.PK;

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "AAA";
			serviceLevel.RS_DefaultTransitHours = 10;

			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			transitTime.RTT_FZ_OriginInternationalZone = ZGuid.Empty;
			transitTime.RTT_FZ_DestinationInternationalZone = ZGuid.Empty;
			transitTime.RTT_TZ_OriginDomesticZone = domesticPickupZone.PK;
			transitTime.RTT_TZ_DestinationDomesticZone = domesticDeliverZone.PK;
			transitTime.RTT_RS_NKServiceLevel = serviceLevel.RS_Code;

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var results = shipment.Lookups.ServiceLevelOrTransitTimeCollection.ToList<BusinessObject>();
				AssertNotNull(results.FirstOrDefault(x => x.PK == transitTime.PK));
				AssertNotNull(results.FirstOrDefault(x => x.PK == serviceLevel.PK));
			}
		}

		public void TestServiceLevelLookupModeFilterDefaultContainerMode()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment1.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			shipment2.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment3.JS_PackingMode = Core.Constants.ContainerModes.Loose;
			shipment3.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			var shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment4.JS_PackingMode = Core.Constants.ContainerModes.BuyersConsol;
			shipment4.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var combinationViews1 = (TransitTimeServiceLevelCombinationCollection)shipment1.Lookups.ServiceLevelOrTransitTimeCollection;
				var defaultMode1 = combinationViews1.GetType().GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(combinationViews1);
				AssertEquals("FCL", defaultMode1);

				var combinationViews2 = (TransitTimeServiceLevelCombinationCollection)shipment2.Lookups.ServiceLevelOrTransitTimeCollection;
				var defaultMode2 = combinationViews2.GetType().GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(combinationViews2);
				AssertEquals("SEA", defaultMode2);

				var combinationViews3 = (TransitTimeServiceLevelCombinationCollection)shipment3.Lookups.ServiceLevelOrTransitTimeCollection;
				var defaultMode3 = combinationViews3.GetType().GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(combinationViews3);
				AssertEquals("LSE", defaultMode3);

				var combinationViews4 = (TransitTimeServiceLevelCombinationCollection)shipment4.Lookups.ServiceLevelOrTransitTimeCollection;
				var defaultMode4 = combinationViews4.GetType().GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(combinationViews4);
				AssertEquals("AIR", defaultMode4);
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
