using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(QuickPODShipmentSelectionForm))]
	sealed class QuickPODShipmentSelectionFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ShipmentCollection shipments = new ShipmentCollection(Factory);
			shipments.AddNew();
			shipments.AddNew();
			shipments.AddNew();
			QuickPODMultipleShipmentsEventArgs e = new QuickPODMultipleShipmentsEventArgs("S101", shipments);
			return new QuickPODShipmentSelectionForm(e);
		}
	}
}
