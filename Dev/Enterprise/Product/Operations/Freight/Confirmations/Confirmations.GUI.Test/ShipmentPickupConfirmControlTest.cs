using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	sealed class ShipmentPickupConfirmControlTest : BaseFreightTest
	{
		public void TestCalculateDistanceMenu()
		{
			using (PickupConfirmControlTestForm form = new PickupConfirmControlTestForm(Factory.New<CommonShipment>()))
			{
				form.Show();
				AssertNotNull("Context menu contains Calculate distance", form.ConfirmsGrid.ContextMenu.MenuItems.FindByText("Calculate distance"));
			}
		}
	}
}
