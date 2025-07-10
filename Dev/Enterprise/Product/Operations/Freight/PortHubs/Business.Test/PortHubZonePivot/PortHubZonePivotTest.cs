using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Business.Testing
{
	[TestedType(typeof(PortHubZonePivot))]
	public class PortHubZonePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPivotedBusinessObjects()
		{
			var portHub = Factory.New<PortHubSelection>();
			portHub.TY_RatingFreightMode = Core.Constants.RateMode.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;

			var zoneProvider = Factory.New<RateTransportProvider>();
			zoneProvider.TP_OH_RelatedParty = carrier.PK;
			var zone = zoneProvider.Zones.AddNew();
			zone.TZ_ZoneName = "The Zone";

			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "CSM";
			serviceLevel.PL_CarrierServiceLevelDescription = "Custom";

			var pivot = portHub.PortHubZonePivots.AddNew();
			pivot.TX_TZ_Zone = zone.PK;
			pivot.TX_PL_NKCarrierServiceLevel = serviceLevel.PL_Code;

			AssertEquals(Core.Constants.RateMode.FCL, pivot.PortHub.TY_RatingFreightMode);
			AssertEquals("The Zone", pivot.TransportZone.TZ_ZoneName);
			AssertEquals("CSM", pivot.CarrierServiceLevel.PL_Code);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var zone = factory.NewWithValidTestData<RateTransportZone>();
			var selction = factory.NewWithValidTestData<PortHubSelection>();
			var depotAddress = factory.NewWithValidTestData<OrgAddress>();
			selction.TY_OA_DepotAddress = depotAddress.PK;

			var result = factory.New<PortHubZonePivot>();
			result.TX_TY_Hub = selction.PK;
			result.TX_TZ_Zone = zone.PK;

			return result;
		}

		#region CarrierServiceLevel

		public void TestCarrierServiceLevelNoExceptionFromNonUniqueCode()
		{
			var pivot = Factory.New<PortHubZonePivot>();
			pivot.TX_PL_NKCarrierServiceLevel = "ABC";

			var serviceLevel = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel.PL_Code = "ABC";

			var serviceLevel2 = Factory.New<OrgCarrierServiceLevel>();
			serviceLevel2.PL_Code = "ABC";

			AssertNoExceptionThrown("Accessing CarrierServiceLevel should not throw exception", () => { _ = pivot.CarrierServiceLevel; });
		}

		#endregion
	}
}
