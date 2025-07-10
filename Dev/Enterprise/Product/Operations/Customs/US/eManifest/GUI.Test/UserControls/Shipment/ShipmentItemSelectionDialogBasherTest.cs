using System.Windows.Forms;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(ShipmentItemSelectionDialog))]
	sealed class ShipmentItemSelectionDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<Trip>();
			var shipemnt = header.Shipments.AddNew();
			shipemnt.B0_ReferenceID = "ship1";
			var shipemnt2 = header.Shipments.AddNew();
			shipemnt2.B0_ReferenceID = "ship2";
			Factory.Save();
			return new ShipmentItemSelectionDialog(header, new[] { shipemnt, shipemnt2 });
		}
	}
}
