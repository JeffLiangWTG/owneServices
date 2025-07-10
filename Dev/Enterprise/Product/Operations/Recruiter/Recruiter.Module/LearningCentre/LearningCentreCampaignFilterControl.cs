using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.Module
{
	public partial class LearningCentreCampaignFilterControl : ZFilterStripControl
	{
		public LearningCentreCampaignFilterControl()
		{
			InitializeComponent();
		}

		public LearningCentreCampaignFilterControl(IBusinessObjectCollection gridCollection, LearningCentreCampaignFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			SetupContextMenu();
		}

		void SetupContextMenu()
		{
			this.Grid.ContextMenu.MenuItems.Add("-");
			this.Grid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("7620ae3e-b1d9-4cab-8f2f-a6d789738a48", "Bulk Create Related Skill Rating"), BulkActionHandler));
			if (ZArchitecture.Modules.ClientHookLoader.Instance.Client == Clients.EDI)
			{
				this.Grid.ContextMenu.MenuItems.Add("-");
				this.Grid.ContextMenu.MenuItems.Add(new ZMenuItem((NoResString)"Bulk Fix Incorrect Scores and Completed Dates (until 02-Sep-16)", BulkActionHandlerForTestHistoryAndResults)); // Temporary code for EDI users
				this.Grid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("9dfa769f-5344-4dd1-8bd6-2a5f4c11fbed", "Export Translations"), ExportTranslationsHandler));
			}
		}

		void BulkActionHandler(object sender, EventArgs e)
		{
			foreach (var bizO in Grid.SelectedElements)
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var campaign = factory.Load<LearningCentreCampaign>(bizO.PK);
			}
		}

		void BulkActionHandlerForTestHistoryAndResults(object sender, EventArgs e) // Temporary code for EDI users
		{
			foreach (var bizO in Grid.SelectedElements)
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var campaign = factory.Load<LearningCentreCampaign>(bizO.PK);
				if (campaign != null)
				{
					campaign.FixIncorrectScoresAndCompletedOnMostRecentTestResults();
					factory.Save();
				}
			}
		}

		void ExportTranslationsHandler(object sender, EventArgs e)
		{
			var dir = GetDirectory();
			if (!string.IsNullOrEmpty(dir))
			{
				var message = new ExportTranslationsHelper().Run(dir, Grid.SelectedElements.Cast<LearningCentreCampaign>());
				Globals.Message.Show(message);
			}
		}

		string GetDirectory()
		{
			var directory = string.Empty;

			using (var browser = new ZFolderBrowserDialog())
			{
				browser.ShowNewFolderButton = true;
				browser.Description = Res.GetString("a73a2adf-3df4-45dd-ba08-51266f6ebd48", "Please select the folder into which the exported files should be saved. Existing files will be overridden.");
				browser.RequireMappablePath = true;

				if (ShowCommonDialogWithoutDispose(browser) == DialogResult.OK)
				{
					if (browser.IsNeedingToUseEnterpriseChannel)
					{
						directory = browser.UnmappedSelectedPath;
					}
					else
					{
						directory = browser.MappedSelectedPath;
					}
				}
			}

			return directory;
		}

		DialogResult ShowCommonDialogWithoutDispose(ZFolderBrowserDialog dialog)
		{
			using (PerformanceStatisticsCollector.Exclude())
			{
				var result = DialogResult.OK;
				if (!Globals.IsTest)
				{
					result = dialog.ShowDialog();
				}
#if DEBUG
				else
				{
					result = ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
				}
#endif
				return result;
			}
		}
	}
}
