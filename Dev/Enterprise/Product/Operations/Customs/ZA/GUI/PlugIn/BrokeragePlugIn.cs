using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ZA.GUI
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

		protected MenuItem fTopLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new EDIMenu();
			}
			return fTopLevelMenu;
		}

		protected override IEnumerable<PreSaveDialogStrategy> GetPreSaveDialogStrategies()
		{
			foreach (var strategy in base.GetPreSaveDialogStrategies())
			{
				yield return strategy;
			}
			var declaration = this.JobDeclaration as Business.JobDeclaration;
			if (declaration != null)
			{
				yield return new DA63PreSaveDialogStrategy(declaration);
			}
		}
	}
}
