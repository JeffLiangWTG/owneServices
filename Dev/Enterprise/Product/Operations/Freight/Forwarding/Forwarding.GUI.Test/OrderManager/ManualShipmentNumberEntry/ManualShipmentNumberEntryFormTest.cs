using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(ManualShipmentNumberEntryForm))]
	public class ManualShipmentNumberEntryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();

			ShipmentNumberEntries shipmentNumberEntries = new ShipmentNumberEntries(consol, Factory);
			shipmentNumberEntries.Load();
			return new ManualShipmentNumberEntryForm(shipmentNumberEntries);
		}
	}
}
