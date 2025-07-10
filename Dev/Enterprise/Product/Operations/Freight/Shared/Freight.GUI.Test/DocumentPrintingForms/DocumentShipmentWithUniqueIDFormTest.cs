using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DocumentShipmentWithUniqueIDForm))]
	sealed class DocumentShipmentWithUniqueIDFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment doc = new DocumentShipment(shipment, Core.Constants.DataContext.FreightLabels);
			return new DocumentShipmentWithUniqueIDForm(doc);
		}
	}
}
