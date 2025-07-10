using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI
{
	[TestClass]
	internal class GlbCompanyCampaignFormForTest : GlbCompanyCampaignForm
	{
		public GlbCompanyCampaignFormForTest(GlbCompanyCampaign campaign, bool shouldShowSendingTab) : base(campaign, shouldShowSendingTab)
		{
		}

		public ZTabControl TopLevelTabControl_Exposed => base.TopLevelTabControl;
		public ZTemplateTabControl MainTabControl_Exposed => base.MainTabControl;
		public ZTabPage MainTabPage_Exposed => base.MainTabPage;
		public ContinueWithSave ValidateAndSave_Exposed() => base.ValidateAndSave();
		public void OnDragDrop_Exposed(DragEventArgs args) => base.OnDragDrop(args);
		public ContinueWithSave ShowPreSaveDialogs_Exposed() => base.ShowPreSaveDialogs();
		public void Delete_Exposed() => base.Delete();
	}
}
