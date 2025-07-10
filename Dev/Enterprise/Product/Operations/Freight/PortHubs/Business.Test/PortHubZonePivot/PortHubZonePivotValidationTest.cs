using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.PortHubs.Testing
{
	internal class PortHubZonePivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTX_TZ_Zone()
		{
			var portHubZonePivot = Factory.New<PortHubZonePivot>();

			portHubZonePivot.TX_TZ_Zone = ZGuid.NewZGuid();
			AssertHasErrors(portHubZonePivot.TX_TZ_ZoneInfo);

			var portDepotSelection = Factory.New<PortHubSelection>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "zone";

			portHubZonePivot.TX_TZ_Zone = zone.PK;
			AssertNoErrors(portHubZonePivot.TX_TZ_ZoneInfo);

			portHubZonePivot.TX_TZ_Zone = ZGuid.Empty;
			AssertHasErrors(portHubZonePivot.TX_TZ_ZoneInfo);
		}

		public void TestCheckTX_PL_NKCarrierServiceLevel()
		{
			var portDepotSelection = Factory.New<PortHubSelection>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "zone";
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "CSM";
			serviceLevel.PL_CarrierServiceLevelDescription = "Custom";
			var portHubZonePivot = portDepotSelection.PortHubZonePivots.AddNew();
			portHubZonePivot.TX_TZ_Zone = zone.PK;

			portHubZonePivot.TX_PL_NKCarrierServiceLevel = "ABC";
			AssertHasErrors(portHubZonePivot.TX_PL_NKCarrierServiceLevelInfo);

			portHubZonePivot.TX_PL_NKCarrierServiceLevel = "CSM";
			AssertNoErrors(portHubZonePivot.TX_PL_NKCarrierServiceLevelInfo);

			portHubZonePivot.TX_PL_NKCarrierServiceLevel = ZString.Empty;
			AssertNoErrors(portHubZonePivot.TX_PL_NKCarrierServiceLevelInfo);

			carrier.OH_IsShippingProvider = false;
			portHubZonePivot.TX_PL_NKCarrierServiceLevel = "CSM";
			AssertHasErrors(portHubZonePivot.TX_PL_NKCarrierServiceLevelInfo);

			portHubZonePivot.TX_PL_NKCarrierServiceLevel = ZString.Empty;
			AssertNoErrors(portHubZonePivot.TX_PL_NKCarrierServiceLevelInfo);
		}

		public void TestCheckForDuplicates()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone1 = zoneProvider.Zones.AddNew();
			zone1.TZ_ZoneName = "zone1";

			var serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "CS1";
			serviceLevel1.PL_CarrierServiceLevelDescription = "Custom1";
			var serviceLevel2 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel2.PL_Code = "CS2";
			serviceLevel2.PL_CarrierServiceLevelDescription = "Custom2";

			var portDepotSelection = Factory.New<PortHubSelection>();
			var portHubZonePivot1 = portDepotSelection.PortHubZonePivots.AddNew();
			portHubZonePivot1.TX_TZ_Zone = zone1.PK;
			portHubZonePivot1.TX_PL_NKCarrierServiceLevel = serviceLevel1.PL_Code;
			var portHubZonePivot2 = portDepotSelection.PortHubZonePivots.AddNew();
			portHubZonePivot2.TX_TZ_Zone = zone1.PK;
			portHubZonePivot2.TX_PL_NKCarrierServiceLevel = serviceLevel2.PL_Code;

			portHubZonePivot1.RunPreSaveValidation();
			portHubZonePivot2.RunPreSaveValidation();

			AssertHasError(portHubZonePivot1.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot1.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");

			portHubZonePivot2.TX_PL_NKCarrierServiceLevel = serviceLevel1.PL_Code;
			portHubZonePivot1.RunPreSaveValidation();
			portHubZonePivot2.RunPreSaveValidation();

			AssertHasError(portHubZonePivot1.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot1.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");

			portHubZonePivot1.TX_PL_NKCarrierServiceLevel = ZString.Empty;
			portHubZonePivot2.TX_PL_NKCarrierServiceLevel = ZString.Empty;
			portHubZonePivot1.RunPreSaveValidation();
			portHubZonePivot2.RunPreSaveValidation();

			AssertHasError(portHubZonePivot1.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_PL_NKCarrierServiceLevelInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot1.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");
			AssertHasError(portHubZonePivot2.TX_TZ_ZoneInfo, "Duplicate combination of Transport Zone and Zone Owner for the same Port and Depot selection is not allowed.");

			var zone2 = zoneProvider.Zones.AddNew();
			zone2.TZ_ZoneName = "zone2";

			portHubZonePivot2.TX_TZ_Zone = zone2.PK;
			portHubZonePivot1.RunPreSaveValidation();
			portHubZonePivot2.RunPreSaveValidation();

			AssertNoErrors(portHubZonePivot1.TX_PL_NKCarrierServiceLevelInfo);
			AssertNoErrors(portHubZonePivot2.TX_PL_NKCarrierServiceLevelInfo);
			AssertNoErrors(portHubZonePivot1.TX_TZ_ZoneInfo);
			AssertNoErrors(portHubZonePivot2.TX_TZ_ZoneInfo);
		}
	}
}
