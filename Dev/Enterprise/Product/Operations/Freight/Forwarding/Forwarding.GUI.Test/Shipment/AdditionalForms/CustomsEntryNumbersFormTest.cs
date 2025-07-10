using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CustomsEntryNumbersForm))]
	public class CustomsEntryNumbersFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.CusEntryNumbers.AddNew();
			shipment.CusEntryNumbers.AddNew();
			return new CustomsEntryNumbersForm(shipment);
		}

		[RequiresSTA]
		public void TestCloseButton()
		{
			using (CustomsEntryNumbersForm form = (CustomsEntryNumbersForm)GetFormToBashCore())
			{
				bool formClosed = false;
				form.Closed += (s, e) => formClosed = true;

				form.Show();
				ZButton closeButton = form.Controls.Find("closeButton", true)[0] as ZButton;
				closeButton.PerformClick();

				Assert(formClosed);
			}
		}
	}
}
