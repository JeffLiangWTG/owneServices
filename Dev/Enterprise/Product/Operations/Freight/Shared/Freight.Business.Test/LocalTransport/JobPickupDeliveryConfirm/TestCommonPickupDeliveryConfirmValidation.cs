using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TestCommonPickupDeliveryConfirmValidation : BusinessObjectValidationTestCase
	{
		public void TestEU_DistanceUnit()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			AssertNoErrors("Precondition", confirm.EU_DistanceUnitInfo);

			confirm.EU_DistanceUnit = "";
			AssertNoErrors("Precondition", confirm.EU_DistanceUnitInfo);

			confirm.EU_Distance = 10m;
			confirm.Validation.ValidateEU_DistanceUnit();
			AssertHasErrors("Distance units mandatory if the distance entered", confirm.EU_DistanceUnitInfo);

			confirm.EU_DistanceUnit = Core.Constants.Length.Kilometres;
			confirm.Validation.ValidateEU_DistanceUnit();
			AssertNoErrors("No errors", confirm.EU_DistanceUnitInfo);

			confirm.EU_DistanceUnit = "XXX";
			confirm.Validation.ValidateEU_DistanceUnit();
			AssertHasErrors("Wrong distance unit", confirm.EU_DistanceUnitInfo);
		}

		public void TestValidateEU_VehicleRegistration()
		{
			CommonConsol consol = (CommonConsol)Factory.New<CFS.ICFSLoadListConsol>();
			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			shipment.Consols.Add(consol);
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_TranshipToOtherCFS = true;
			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 10;
			packLine.SetContainer(consol, container);

			CommonPickupDeliveryConfirm confirm = shipment.DestinationCFSDepartures.AddNew();
			confirm.Validation.ValidateEU_VehicleRegistration();
			AssertHasErrors("Vehicle Rego should now have errors", confirm.EU_VehicleRegistrationInfo);

			Factory.Save();
			confirm.Validation.ValidateEU_VehicleRegistration();
			AssertNoErrors("Confirm is now in DB. Don't validate", confirm.EU_VehicleRegistrationInfo);
		}
	}
}
