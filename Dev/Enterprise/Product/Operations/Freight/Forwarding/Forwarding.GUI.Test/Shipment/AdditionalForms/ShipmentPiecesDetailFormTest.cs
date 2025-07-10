using System.Windows.Forms;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(ShipmentPiecesDetailForm))]
	public class ShipmentPiecesDetailFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			return new ShipmentPiecesDetailForm(shipment);
		}
	}
}
