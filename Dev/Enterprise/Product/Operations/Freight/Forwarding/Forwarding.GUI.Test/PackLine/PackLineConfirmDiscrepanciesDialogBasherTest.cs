using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PackLineConfirmDiscrepanciesDialog))]
	public class PackLineConfirmDiscrepanciesDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new PackLineConfirmDiscrepanciesDialog(Factory.New<CommonShipment>().OuterPackLines.AddNew());
	}
}
