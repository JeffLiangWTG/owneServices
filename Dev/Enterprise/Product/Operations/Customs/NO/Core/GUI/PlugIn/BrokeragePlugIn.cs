using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NO.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected override MenuItem GetNewTopLevelMenuCore() => topLevelMenu ?? (topLevelMenu = new EDIMenu());
		MenuItem topLevelMenu;
	}
}
