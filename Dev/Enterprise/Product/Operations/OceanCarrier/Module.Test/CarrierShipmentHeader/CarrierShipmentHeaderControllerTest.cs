using Enterprise.OceanCarrier.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Module.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderController))]
	sealed class CarrierShipmentHeaderControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.CarrierShipmentHeader;

		public void TestShouldNotShowFormForNewCarrierShipment()
		{
			var carrierShipmentHeader = Factory.New<Business.CarrierShipmentHeader>();
			var controller = new CarrierShipmentHeaderController();

			using (var form = controller.ShowFormForNewEntity(carrierShipmentHeader))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Carrier Shipment on the CargoWise Desktop, please go to the Ocean Carrier Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShouldShowFormForExistingCarrierShipment()
		{
			var carrierShipmentHeader = Factory.New<Business.CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "CS001";

			Factory.Save();

			var controller = new CarrierShipmentHeaderController();

			using (var form = controller.ShowEditForm(carrierShipmentHeader))
			{
				AssertType<CarrierShipmentHeaderForm>("shown form for CarrierShipment", form);
			}
		}

		#region Implementation

		public override void TestNewForm() => Assert("This form is mainly for Document Customisation. All other operations should be performed in the OCS Glow portal", true);

		#endregion
	}
}
