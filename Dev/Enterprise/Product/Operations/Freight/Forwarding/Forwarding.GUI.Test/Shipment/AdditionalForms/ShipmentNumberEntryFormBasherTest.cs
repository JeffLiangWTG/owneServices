using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ShipmentNumberEntryForm))]
	public class ShipmentNumberEntryFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ShipmentNumberEntry entry = new ShipmentNumberEntry(shipment);

			return new ShipmentNumberEntryForm(entry);
		}

		#endregion
	}
}
