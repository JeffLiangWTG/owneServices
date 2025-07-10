using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.OceanCarrier.Business.Testing
{
	sealed class CarrierShipmentHeaderValidationTest : BusinessObjectValidationTestCase
	{
		#region ScreeningStatus

		public void TestInvalidScreeningStatusShouldThrowError()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_ScreeningStatus = "XXX";
			AssertHasError(carrierShipmentHeader.CSH_ScreeningStatusInfo, "Enter a valid Screening Status.");
		}

		public void TestValidScreeningStatusShouldNotThrowError()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			var validStates = new ScreeningStatusesList().GetAllCodes();
			foreach (var validState in validStates)
			{
				carrierShipmentHeader.CSH_ScreeningStatus = validState;
				AssertNoErrors(carrierShipmentHeader.CSH_ScreeningStatusInfo);
			}
		}

		#endregion
	}
}
