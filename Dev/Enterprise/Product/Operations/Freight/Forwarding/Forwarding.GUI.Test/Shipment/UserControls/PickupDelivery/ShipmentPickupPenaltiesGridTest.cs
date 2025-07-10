using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class ShipmentPickupPenaltiesGridTest : BaseFreightTest
	{
		[ExpectNoExceptions]
		public void TestShipmentPickupPenaltiesGrid()
		{
			using (var form = new ZForm())
			using (var control = new ShipmentPickupPenaltiesGrid())
			{
				control.Dock = DockStyle.Fill;
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Shipment, "");
				ErrorReporter.Clear();
			}
		}

		#region Implementation

		ForwardingShipment Shipment
		{
			get { return shipment ?? (shipment = Factory.New<ForwardingShipment>()); }
		}
		ForwardingShipment shipment;

		#endregion
	}
}
