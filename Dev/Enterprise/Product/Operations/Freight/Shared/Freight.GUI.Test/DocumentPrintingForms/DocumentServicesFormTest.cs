using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DocumentServicesForm))]
	sealed class DocumentServicesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobService service = shipment.DocsAndCartage.Services.AddNew();

			DocumentServices documentServices = new DocumentServices(shipment.DocsAndCartage, new ServiceToSelectFromForPrintingCollection(shipment.DocsAndCartage.Services));
			return new DocumentServicesForm(documentServices);
		}

		public void TestDialogResult()
		{
			using (DocumentServicesForm form = (DocumentServicesForm)GetFormToBash())
			{
				form.Show();

				ZButton printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (DocumentServicesForm form = (DocumentServicesForm)GetFormToBash())
			{
				form.Show();

				ZButton cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}
	}
}
