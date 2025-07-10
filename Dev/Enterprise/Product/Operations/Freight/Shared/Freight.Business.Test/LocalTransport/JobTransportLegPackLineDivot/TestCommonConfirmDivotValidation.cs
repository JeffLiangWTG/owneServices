using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TestCommonConfirmDivotValidation : BusinessObjectValidationTestCase
	{
		public void TestCheckJ8_PackagesDelivered()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 30;
			packLine.JL_JS = shipment.PK;

			var confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival;
			var divot = confirm.GetDivot(packLine);
			shipment.RunPreSaveValidation();

			AssertNoErrors("Pre-condition", divot.J8_PackagesDeliveredInfo);

			packLine.JL_PackageCount = 20;
			shipment.RunPreSaveValidation();

			AssertHasWarningContaining(divot.J8_PackagesDeliveredInfo, "You cannot deliver more packs than there are in the shipment. Please check before proceeding.");
		}

		public void TestCheckJ8_PackagesDelivered_DoesCauseErrorsInForwarding()
		{
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 30;
			packLine.JL_JS = shipment.PK;

			var confirm = shipment.PickupConfirms.AddNew();
			confirm.EU_PickupDeliveryType = Core.Constants.PickupDeliveryConfirmTypes.OriginCFSArrival;
			var divot = confirm.GetDivot(packLine);
			shipment.RunPreSaveValidation();

			packLine.JL_PackageCount = 20;
			shipment.RunPreSaveValidation();

			AssertNoErrors("Expected no error as shipment is forwarding", divot.J8_PackagesDeliveredInfo);
		}

		public void TestCheckJ8_DeliveryWeight()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_ActualWeight = 100;
			shipment.RunPreSaveValidation();

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			shipment.RunPreSaveValidation();

			AssertNoErrors(divot.J8_DeliveryWeightInfo);

			packline.JL_ActualWeight = 50;
			shipment.RunPreSaveValidation();

			AssertHasWarningContaining(divot.J8_DeliveryWeightInfo, "You cannot deliver more weight than there is in the shipment.");
		}

		public void TestCheckJ8_DeliveryVolume()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_ActualVolume = 100;
			shipment.RunPreSaveValidation();

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];
			shipment.RunPreSaveValidation();

			AssertNoErrors(divot.J8_DeliveryVolumeInfo);

			packline.JL_ActualVolume = 50;
			shipment.RunPreSaveValidation();

			AssertHasWarningContaining(divot.J8_DeliveryVolumeInfo, "You cannot deliver more volume than there is in the shipment.");
		}
	}
}
