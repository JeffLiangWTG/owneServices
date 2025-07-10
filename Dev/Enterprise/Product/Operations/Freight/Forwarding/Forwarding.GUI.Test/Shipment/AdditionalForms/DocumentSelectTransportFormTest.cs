using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DocumentSelectTransportForm))]
	public class DocumentSelectTransportFormTest : ZFormBasherTest
	{
		public void TestFormText()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.TransportsIncludingRelated.AddNew();
			DocumentShipment docShipment = new DocumentShipment(shipment, Enterprise.Core.Constants.DataContext.GenericFreightJobRouting);
			using (DocumentSelectTransportForm form = new DocumentSelectTransportForm(docShipment))
			{
				form.Show();
				AssertEquals("Text", "Select Routing", form.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.TransportsIncludingRelated.AddNew();
			DocumentShipment docShipment = new DocumentShipment(shipment, Enterprise.Core.Constants.DataContext.GenericFreightJobRouting);
			return new DocumentSelectTransportForm(docShipment);
		}
	}
}
