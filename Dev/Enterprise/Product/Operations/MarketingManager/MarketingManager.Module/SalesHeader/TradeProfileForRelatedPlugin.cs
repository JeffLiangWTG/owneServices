using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MarketingManager.Module
{
	public class TradeProfileForRelatedPlugin : ZPlugIn
	{
		public TradeProfileForRelatedPlugin(BusinessObject hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		#region Name

		public override string Name
		{
			get { return Res.GetString("07cef1bb-8777-47cf-b644-6a3da6fe4dc8", "Value Analysis"); }
		}

		#endregion

		#region User Control

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			var control = new TradeProfileForRelatedUserControl();
			control.Dock = DockStyle.Fill;
			return control;
		}

		#endregion

		#region RefreshData

		public override void RefreshData()
		{
			base.RefreshData();

			var tradeProfileForRelatedUserControl = (TradeProfileForRelatedUserControl)UserControl;
			tradeProfileForRelatedUserControl.RefreshSalesValueAnalysisCollection();
			tradeProfileForRelatedUserControl.RefreshCalculatedEstimatedValues();
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.RelationshipClientIntelligence; }
		}

		#endregion
	}
}
