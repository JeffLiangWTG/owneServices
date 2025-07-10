using System.Linq;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public static class OpportunityFormExtensions
	{
		public static ZToolStripDropDownButton GetNewProspectValueButton(this OpportunityForm form)
		{
			var toolStrip = form.GetProspectValuesToolStrip();

			return (ZToolStripDropDownButton)toolStrip.Items.Find("newProspectValueButton", true).First();
		}

		public static ZToolStripButton GetEditProspectValuesButton(this OpportunityForm form)
		{
			var toolStrip = form.GetProspectValuesToolStrip();

			return (ZToolStripButton)toolStrip.Items.Find("editProspectValuesButton", true).First();
		}

		public static ZToolStripButton GetUpdateProspectStatusButton(this OpportunityForm form)
		{
			var toolStrip = form.GetProspectValuesToolStrip();

			return (ZToolStripButton)toolStrip.Items.Find("updateProspectStatusButton", true).First();
		}

		public static OpportunitySalesValueAnalysisControl GetSalesValueAnalysisControl(this OpportunityForm form)
		{
			return form.FindSingle<OpportunitySalesValueAnalysisControl>("salesValueAnalysisControl");
		}

		static ZToolStrip GetProspectValuesToolStrip(this OpportunityForm form)
		{
			var userControl = form.FindSingle<TradeProfileForRelatedUserControl>("TradeProfileForRelatedUserControl");
			return userControl.FindSingle<ZToolStrip>("toolStrip");
		}
	}
}
