using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccGLHeaderForm))]
	sealed class TestAccGLHeaderForm : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsGLAccountUsedInCompanyLevelRegistry(It.IsAny<ZGuid>(), It.IsAny<ZGuid[]>())).Returns(false);
			mock.Setup(m => m.IsGLAccountUsedInSystemLevelRegistry(It.IsAny<ZGuid>())).Returns(false);
			mock.Setup(m => m.GLAccountFormat).Returns("XXXX.XX.XX");
			ObjectFactory.Substitute(mock.Object);

			GLHeader = Factory.New<AccGLHeader>();
			return new AccGLHeaderForm(GLHeader);
		}

		public void TestWarningMessageWhenTickingIsGlobal()
		{
			using (AccGLHeaderForm form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var checkBox = (ZCheckBox)form.Controls.Find("AG_IsGlobalBoundCheckBox", true)[0];
				checkBox.Checked = false;
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				checkBox.Checked = true;
				AssertEquals("Setting this GL Account as a Global GL Account will remove all companies from the Companies Tab. Are you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCompanyFilterLabelVisibilityWhenTickingIsGlboal()
		{
			using (AccGLHeaderForm form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var tabControl = (ZTabControl)form.Controls.Find("AccGLHeaderTabControl", true)[0];
				var detailsTab = (ZTabPage)form.Controls.Find("DetailsTabPage", true)[0];
				var companiesTab = (ZTabPage)form.Controls.Find("companiesTabPage", true)[0];

				tabControl.SelectedTab = detailsTab;
				var checkBox = (ZCheckBox)form.Controls.Find("AG_IsGlobalBoundCheckBox", true)[0];
				Assert(checkBox.Checked);

				tabControl.SelectedTab = companiesTab;
				var label = (ZLabel)form.Controls.Find("CompanyFilterLabel", true)[0];
				Assert(label.Visible);

				tabControl.SelectedTab = detailsTab;
				checkBox.Checked = false;
				Assert(!checkBox.Checked);

				tabControl.SelectedTab = companiesTab;
				Assert(!label.Visible);
			}
		}

		public void TestAlternateChartsDissectionConfigurationTabPageEnableReportingBooksFeatureFalse()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.Controls.Find("AlternateChartsDissectionConfigurationTabPage", true).Any());
			}
		}

		[RequiresSTA]
		public void TestAlternateChartsDissectionConfigurationTabPageEnableReportingBooksFeatureTrue()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var alternateChartsDissectionConfigurationTab = (ZTabPage)form.Controls.Find("AlternateChartsDissectionConfigurationTabPage", true)[0];

				AssertNotNull(alternateChartsDissectionConfigurationTab);
				AssertNotNull(alternateChartsDissectionConfigurationTab.Controls.Find("ADC_AAC_AlternateChart", true));
				AssertNotNull(alternateChartsDissectionConfigurationTab.Controls.Find("ADC_Attribute", true));
				AssertNotNull(alternateChartsDissectionConfigurationTab.Controls.Find("AttributeDescription", true));
				AssertNotNull(alternateChartsDissectionConfigurationTab.Controls.Find("ADC_SeparateNumbering", true));
			}
		}

		public void TestNoDissectionConfigurationSecurityLabelVisibility()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var tabControl = (ZTabControl)form.Controls.Find("AccGLHeaderTabControl", true)[0];
				var detailsTab = (ZTabPage)form.Controls.Find("DetailsTabPage", true)[0];
				var alternateChartsDissectionConfigurationTab = (ZTabPage)form.Controls.Find("AlternateChartsDissectionConfigurationTabPage", true)[0];

				tabControl.SelectedTab = detailsTab;
				Env.Security.GLAccountsDissectionConfigurationEdit.IsAllowed = false;
				Assert(!Env.Security.GLAccountsDissectionConfigurationEdit.IsAllowed);

				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				var label = (ZLabel)form.Controls.Find("NoDissectionConfigurationSecurityLabel", true)[0];
				Assert(label.Visible);

				tabControl.SelectedTab = detailsTab;
				Env.Security.GLAccountsDissectionConfigurationEdit.IsAllowed = true;
				Assert(Env.Security.GLAccountsDissectionConfigurationEdit.IsAllowed);

				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				Assert(!label.Visible);
			}
		}

		public void TestNotBSHPLDissectionConfigurationLabelVisibility()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var tabControl = (ZTabControl)form.Controls.Find("AccGLHeaderTabControl", true)[0];
				var detailsTab = (ZTabPage)form.Controls.Find("DetailsTabPage", true)[0];
				var alternateChartsDissectionConfigurationTab = (ZTabPage)form.Controls.Find("AlternateChartsDissectionConfigurationTabPage", true)[0];
				var supportAccountType = new List<string>() { AccountType.BalanceSheetAccount, AccountType.ProfitAndLossAccount };

				tabControl.SelectedTab = detailsTab;
				var codeBox = (ZDropEdit)form.Controls.Find("AG_AccountTypeBoundDropEdit", true)[0];
				codeBox.Text = AccountType.BalanceSheetAccount;
				Assert(supportAccountType.Contains(codeBox.Text));

				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				var label = (ZLabel)form.Controls.Find("NotBSHPLDissectionConfigurationLabel", true)[0];
				Assert(!label.Visible);

				tabControl.SelectedTab = detailsTab;
				codeBox.Text = AccountType.Alternate;
				Assert(!supportAccountType.Contains(codeBox.Text));

				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				Assert(label.Visible);
			}
		}

		public void TestNoDissectionConfigurationLabelVisibility()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var form = (AccGLHeaderForm)GetFormToBash())
			{
				form.Show();
				Application.DoEvents();
				var tabControl = (ZTabControl)form.Controls.Find("AccGLHeaderTabControl", true)[0];
				var detailsTab = (ZTabPage)form.Controls.Find("DetailsTabPage", true)[0];
				var alternateChartsDissectionConfigurationTab = (ZTabPage)form.Controls.Find("AlternateChartsDissectionConfigurationTabPage", true)[0];
				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				var label = (ZLabel)form.Controls.Find("NoDissectionConfigurationLabel", true)[0];
				Assert(!label.Visible);

				tabControl.SelectedTab = detailsTab;
				AccountingMasterFilesRegistry.Instance.PendingTaxTransactionPrepaidAssetControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid());

				tabControl.SelectedTab = alternateChartsDissectionConfigurationTab;
				Assert(!GLHeader.IsAllowedToHaveAttributes());
				Assert(label.Visible);
			}
		}

		AccGLHeader GLHeader;
	}
}
