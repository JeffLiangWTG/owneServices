using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(OrderManualShipmentNumberEntryForm))]
	public class OrderManualShipmentNumberEntryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var entry = new ShipmentNumberEntry(shipment);

			return new OrderManualShipmentNumberEntryForm(entry);
		}

		#endregion
	}
}
