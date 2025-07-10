using System.Windows.Forms;

using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			return new CustomsBrokerageUserControl();
		}

		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new EDIMenu();
			}
			return fTopLevelMenu;
		}
		EDIMenu fTopLevelMenu;
	}
}
