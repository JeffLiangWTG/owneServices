using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CO2eLegBasedDataTransferHelperTest : TestCaseWithFactory
	{
		public void TestIsCO2eResponseApplicable()
		{
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			Assert(((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			shipment.JS_ActualWeight = 2m;
			AssertEquals("Shipment Weight has been changed.", true, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.JS_TransportMode = "AIR";
			AssertEquals("Shipment Transport Mode has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.JS_RL_NKOrigin = "AUMEL";
			AssertEquals("Shipment Origin has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			shipment.JS_RL_NKDestination = "VNSGN";
			AssertEquals("Shipment Destination has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			shipment.Transports[0].JW_VoyageFlight = "VY2";
			AssertEquals("Transport voyage number has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			shipment.Transports[0].JW_OA_CarrierAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals("Transport carrier has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));

			shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Factory.Save();
			shipment.Transports[1].JW_AircraftType = "N98";
			AssertEquals("Transport aircraft type has been changed.", false, ((ICO2eLegBasedSupporter)shipment).IsCO2eResponseApplicable(dataObject));
		}

		public void TestIsCO2eResponseNotApplicable_CO2eStatusNCU_RequireTEUFalse()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			var supporter = (ICO2eLegBasedSupporter)shipment;

			// Act & Assert
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Assert(supporter.IsCO2eResponseApplicable(dataObject));

			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			AssertEquals("Co2e Status is NCU and it should not be applicable.", false, supporter.IsCO2eResponseApplicable(dataObject));
		}

		public void TestIsCO2eResponseNotApplicable_CO2eStatusNCU_RequireTEUTrue()
		{
			// Arrange
			var shipment = CO2eTestHelper.CreateForwardingShipmentRequiringTEU(Factory);
			var supporter = (ICO2eLegBasedSupporter)shipment;
			var dataObject = CO2eTestHelper.GetSampleCO2eResponseDataObject();

			// Act & Assert
			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.Current);
			Assert(supporter.IsCO2eResponseApplicable(dataObject));

			(shipment as ICO2eProvider).SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			Assert(supporter.RequireTEU);
			AssertEquals("Co2e Status is NCU and it should not be applicable.", false, supporter.IsCO2eResponseApplicable(dataObject));
		}
	}
}
