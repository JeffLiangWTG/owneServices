using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing
{
	sealed class CO2eLegBasedSupporterHelperTest : TestCaseWithFactory
	{
		public void TestOnRequestedShouldClearTotalCO2eAndSetPending()
		{
			// Arrange
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			var supporter = (ICO2eLegBasedSupporter)shipment;
			supporter.SetTotalCO2e(100m);
			supporter.SetCO2eStatus(CO2eStatusList.Codes.Current);

			// Act
			CO2eLegBasedSupporterHelper.OnRequested(supporter);

			// Assert
			AssertEquals(0m, supporter.GetTotalCO2e());
			AssertEquals(CO2eStatusList.Codes.Pending, supporter.GetCO2eStatus());
		}
	}
}
