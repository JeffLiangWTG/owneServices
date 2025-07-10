using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module.Testing
{
	public class LearningCentreCampaignFilterControlTest : TestCaseWithFactory
	{
		public void TestExportTranslationsMenuItem()
		{
			var collection = new LearningCentreCampaignCollection(Factory);
			using (var control = new LearningCentreCampaignFilterControl(collection, new LearningCentreCampaignFilterBusinessObject()))
			{
				AssertNotEquals(ClientHookLoader.Instance.Client, Clients.EDI);
				var menuItem = control.Grid.ContextMenu.MenuItems.FindByText("Export Translations");
				AssertNull("Shouldn't show the menuItem for non EDI client", menuItem);
			}

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				using (var control = new LearningCentreCampaignFilterControl(collection, new LearningCentreCampaignFilterBusinessObject()))
				{
					AssertEquals(ClientHookLoader.Instance.Client, Clients.EDI);
					var menuItem = control.Grid.ContextMenu.MenuItems.FindByText("Export Translations");
					AssertNotNull("Should show the menuItem for EDI client", menuItem);
					menuItem.PerformClick();
					AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
				}
			}
		}
	}
}
