using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	sealed class ShipmentDeliveryConfirmControlTest : BaseFreightTest
	{
		public void TestCalculateDistanceMenu()
		{
			using (DeliveryConfirmControlTestForm form = new DeliveryConfirmControlTestForm(Factory.New<CommonShipment>()))
			{
				form.Show();
				AssertNotNull("Context menu contains Calculate distance", form.ConfirmsGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
			}
		}
	}
}
