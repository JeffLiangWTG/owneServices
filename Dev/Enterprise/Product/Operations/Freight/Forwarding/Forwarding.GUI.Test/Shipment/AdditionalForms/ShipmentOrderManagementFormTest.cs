using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ShipmentOrderManagementForm))]
	public class ShipmentOrderManagementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new ShipmentOrderManagementForm(shipment);
		}
	}
}
