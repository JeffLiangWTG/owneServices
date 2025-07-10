using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class ManualShipmentNumberEntry
	{
		public static void ShipmentNumber_OnAfterShipmentsCreated(object sender, EnterShipmentNumbersEventArgs e)
		{
			if (Env.Registry.AllowManualShipmentEntry)
			{
				var shipmentNumberEntries = new ShipmentNumberEntries(e.Consol, e.Consol.Factory);
				shipmentNumberEntries.Load();

				DialogResult result;

				result = shipmentNumberEntries.Count == 1
					? ZFormModaliser.ShowDialogAndDispose(new OrderManualShipmentNumberEntryForm(shipmentNumberEntries[0]))
					: ZFormModaliser.ShowDialogAndDispose(new ManualShipmentNumberEntryForm(shipmentNumberEntries));

#if DEBUG
				if (Globals.IsTest)
				{
					int i = 0;
					foreach (ShipmentNumberEntry entry in shipmentNumberEntries)
					{
						entry.ShipmentNumber = "Heya" + i;
						i++;
					}
				}
#endif
				if (result == DialogResult.OK)
				{
					foreach (ShipmentNumberEntry entry in shipmentNumberEntries)
					{
						entry.Accept();
					}
				}
			}
		}
	}
}
