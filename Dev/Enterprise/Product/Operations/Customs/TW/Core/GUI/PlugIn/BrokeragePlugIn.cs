using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.TW.GUI
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

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			if (JobDeclaration != null)
			{
				JobDeclaration.RefreshExRateToLatestRateAvailableIfNeeded();
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				result = new CalculateDutyStrategy((JobDeclaration)JobDeclaration).ShowPreSaveDialogs(result);
			}
			return result;
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore() => new CreateDeclarationHelper();
	}
}
