using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.PortHubs.Testing
{
	public class PortHubZonePivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCarrierAccounts()
		{
			var portHub = Factory.New<PortHubSelection>();
			portHub.TY_RatingFreightMode = Core.Constants.RateMode.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "The Zone";

			var pivot = portHub.PortHubZonePivots.AddNew();
			pivot.TX_TZ_Zone = zone.PK;

			AssertEquals("Should find 0 account number", 0, pivot.Lookups.CarrierAccounts.Count);

			var acc1 = carrier.CarrierAccounts.AddNew();
			acc1.OAN_AccountNumber = "123456";
			var acc2 = carrier.CarrierAccounts.AddNew();
			acc2.OAN_AccountNumber = "654321";

			AssertEquals("Should find 2 account numbers", 2, pivot.Lookups.CarrierAccounts.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "123456", "654321" }, pivot.Lookups.CarrierAccounts.Select(x => x.OAN_AccountNumber));
		}

		public void TestCarrierAccountsType()
		{
			var org = Factory.New<OrgHeader>();

			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = org.PK;

			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "The Zone";

			var pivot = Factory.New<PortHubZonePivot>();
			pivot.TX_TZ_Zone = zone.PK;

			pivot.Lookups.TransportZoneOwner.OH_IsShippingProvider = true;

			AssertType<OrgCarrierAccountCollection>(pivot.Lookups.CarrierAccounts);
		}

		public void TestCarrierAccounts_WhenOrgHeaderIsNull_IsEmpty()
		{
			var portHub = Factory.New<PortHubSelection>();
			portHub.TY_RatingFreightMode = Core.Constants.RateMode.FCL;

			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = ZGuid.Empty;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "The Zone";

			var pivot = portHub.PortHubZonePivots.AddNew();
			pivot.TX_TZ_Zone = zone.PK;

			AssertEquals("Should find 0 account number", 0, pivot.Lookups.CarrierAccounts.Count);
		}

		public void TestCarrierAccounts_WhenOrgHeaderIsNotShippingProvider_IsEmpty()
		{
			var portHub = Factory.New<PortHubSelection>();
			portHub.TY_RatingFreightMode = Core.Constants.RateMode.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = false;

			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "The Zone";

			var pivot = portHub.PortHubZonePivots.AddNew();
			pivot.TX_TZ_Zone = zone.PK;

			AssertEquals("Should find 0 account number", 0, pivot.Lookups.CarrierAccounts.Count);
		}

		public void TestMultipleZonesAndCarrierAccounts()
		{
			var zoneOwner1 = Factory.New<OrgHeader>();
			zoneOwner1.OH_IsShippingProvider = false;

			var zoneOwner2 = Factory.New<OrgHeader>();
			zoneOwner2.OH_IsShippingProvider = false;

			var carrier1 = Factory.New<OrgCarrierAccount>();
			carrier1.OAN_OH_Carrier = zoneOwner1.PK;
			carrier1.OAN_AccountNumber = "ACC_123";

			var carrier2 = Factory.New<OrgCarrierAccount>();
			carrier2.OAN_OH_Carrier = zoneOwner2.PK;
			carrier2.OAN_AccountNumber = "ACC_456";

			var zoneProvider1 = Factory.New<RateTransportProvider>();
			zoneProvider1.TP_OH_RelatedParty = carrier1.PK;

			var zoneProvider2 = Factory.New<RateTransportProvider>();
			zoneProvider2.TP_OH_RelatedParty = carrier2.PK;

			var zone1 = zoneProvider1.Zones.AddNew();
			zone1.TZ_ZoneName = "The Zone 1";

			var zone2 = zoneProvider2.Zones.AddNew();
			zone2.TZ_ZoneName = "The Zone 2";

			var pivot1 = Factory.New<PortHubZonePivot>();
			pivot1.TX_TZ_Zone = zone1.PK;
			pivot1.TransportZone.TransportProvider.TP_OH_RelatedParty = zoneOwner1.PK;

			var pivot2 = Factory.New<PortHubZonePivot>();
			pivot2.TX_TZ_Zone = zone2.PK;
			pivot2.TransportZone.TransportProvider.TP_OH_RelatedParty = zoneOwner2.PK;

			CombineAssertions(() =>
			{
				AssertEquals("There should be only one carrier account number in this lookup", 1, pivot1.Lookups.CarrierAccounts.Count);
				AssertEquals("There should be only one carrier account number in this lookup", 1, pivot2.Lookups.CarrierAccounts.Count);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Should retrieve carrier account number Acc123", "ACC_123", pivot1.Lookups.CarrierAccounts[0].OAN_AccountNumber);
				AssertEquals("Should retrieve carrier account number Acc456", "ACC_456", pivot2.Lookups.CarrierAccounts[0].OAN_AccountNumber);
			});
		}
	}
}
