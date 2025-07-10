using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DocumentChargeSheet))]
	public class DocumentChargeSheetFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment docs = new DocumentShipment(shipment, Core.Constants.DataContext.ChargeSheet);
			docs.SetDefaultsFromDataContext();
			return new DocumentChargeSheet(docs);
		}

		[RequiresSTA]
		public void TestDialogResult()
		{
			using (DocumentChargeSheet form = (DocumentChargeSheet)GetFormToBash())
			{
				form.Show();

				ZButton printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (DocumentChargeSheet form = (DocumentChargeSheet)GetFormToBash())
			{
				form.Show();

				ZButton cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}
	}
}
