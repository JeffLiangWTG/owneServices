using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(DocumentDeliveryAgentsForm))]
	public class DocumentDeliveryAgentsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DocumentDeliveryAgents doc = new DocumentDeliveryAgents(new DeliveryAgentToSelectFromForPrintingCollection(Factory));
			return new DocumentDeliveryAgentsForm(doc);
		}

		public void TestDialogResult()
		{
			using (DocumentDeliveryAgentsForm form = (DocumentDeliveryAgentsForm)GetFormToBash())
			{
				form.Show();

				ZButton printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (DocumentDeliveryAgentsForm form = (DocumentDeliveryAgentsForm)GetFormToBash())
			{
				form.Show();

				ZButton cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}
	}
}
