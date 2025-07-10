using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core.Forms;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCountryForm : ZForm, ISupportViewDpsLogsTab
	{
		public RefCountryForm(RefCountry country)
			: base(country)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);

			AddDeniedPartyScreeningLogsTabPage();
			InitialiseComplianceRulesTabPage();

			new DeniedPartyScreeningActionsProvider(this).AddEntitiesMenuItem();
			UpdateShowInnersVisibility();
		}

		public new RefCountry BusinessEntity => (RefCountry)base.BusinessEntity;

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ControlDpiScalingHelper.SetTop(ref PostingButtonsUserControl, MainStatusBar.Top - PostingButtonsUserControl.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && BusinessEntity.RN_IsActiveInfo.HasChanges)
			{
				result = Globals.Message.ShowConfirmation(
					Res.GetString("AD4E604C-6862-4669-98B1-0549840DC1B2", "If a country/region is incorrectly modified there will be a large negative impact on the system and its behaviors. Many functions such as customs messaging and invoicing could cease to function and any features that require geographic definitions will not be available for user defined countries/regions. Please ensure you only complete this action if you understand all of the impacts on your system."),
					Res.GetString("4504A82C-E252-4141-822F-32270AA9E758", "Important Impact to Know"),
					Res.GetString("054B6A2E-116F-4EE9-8201-25181FD860D4", "Please type the following to continue:"),
					Res.GetString("175C6B19-850E-4BD2-B24C-DFFCE1162031", "I understand the impact of this change and wish to proceed"),
					MessageBoxIcon.Warning,
					ConfirmationMessageLayout.LineBreakAfterEachPart) == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		string DpsLogsTabName => Res.GetString("D523562F-FAFC-4E28-ADC5-043B21EDEF3B", "Denied Party Screening Logs");

		void AddDeniedPartyScreeningLogsTabPage()
		{
			var screeningLogControl = new StmEntityScreeningLogControl();
			screeningLogControl.SetBindingMember("ScreeningLogCollection");
			zLogsTabPage1.AddAdditionalTab(DpsLogsTabName, screeningLogControl);
		}

		void InitialiseComplianceRulesTabPage()
		{
			if (ComplianceRiskHelper.IsComplianceCommodityScreeningEnable)
			{
				var complianceRuleUserControl = ObjectFactory.Get<IComplianceRuleUserControl>() as ZUserControl;
				complianceRuleUserControl.SetBindingMember("ComplianceRules");
				complianceRuleUserControl.Dock = DockStyle.Fill;
				zComplianceRulesTabPage.Controls.Add(complianceRuleUserControl);
			}
			else
			{
				zComplianceRulesTabPage.TabVisible = false;
			}
		}

		public void SwitchToDpsLogsTab()
		{
			if (zLogsTabPage1 != null)
			{
				initialTabPage = zLogsTabPage1;
				MainTabControl.SelectedTab = zLogsTabPage1;
				zLogsTabPage1.SetSelectedTab(DpsLogsTabName);
			}
		}

		void UpdateShowInnersVisibility()
		{
			if (!FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				var showInnersColumn = RulesGrid.ColumnStyles.Cast<ZGridColumnInfo>()
					.FirstOrDefault(x => x.ColumnName == RefCountryRules.Schema.R7_ShowInner);

				if (showInnersColumn != null)
				{
					RulesGrid.ColumnStyles.Remove(showInnersColumn);
				}
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (initialTabPage != null)
			{
				MainTabControl.SelectedTab = initialTabPage;
			}
		}

		ZTabPage initialTabPage;

		#endregion
	}
}
