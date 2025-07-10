using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateModule))]
	sealed class AccOrgTaxConfigurationTemplateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccOrgTaxConfigurationTemplate;
		}

		public void TestCheckpoints()
		{
			using (var module = new AccOrgTaxConfigurationTemplateModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.TaxConfigurationTemplate, module.SecurityCheckpoint);
			}
		}

		[RequiresSTA]
		public void TestModuleRelatedBizo()
		{
			using (var module = new AccOrgTaxConfigurationTemplateModuleForTest())
			{
				var filterControl = module.NewFilterControl;

				AssertType<AccOrgTaxConfigurationTemplateController>(module.GetNewController());
				AssertType<AccOrgTaxConfigurationTemplateCollection>(module.NewGridCollection);
				AssertType<AccOrgTaxConfigurationTemplateFilterControl>(filterControl);
				AssertType<AccOrgTaxConfigurationTemplateFilterBusinessObject>(module.NewFilterBusinessObject);

				filterControl.Dispose();
			}
		}

		[RequiresSTA]
		public void TestMenuItems()
		{
			using (var moduleForm = new ZForm())
			using (var module = new AccOrgTaxConfigurationTemplateModule())
			{
				moduleForm.Controls.Add(module.EmbeddedControl);
				moduleForm.Show();

				AssertNotNull(module.ToolBarButtons.FindByText("New"));
				AssertNotNull(module.ToolBarButtons.FindByText("View"));
				AssertNotNull(module.ToolBarButtons.FindByText("Edit"));
				AssertNotNull(module.ToolBarButtons.FindByText("Copy"));

				AssertEquals("PreventDelete should be false to show Delete button", false, PreventDeleteAttribute.IsTrue(module.DisplayGrid.ElementTypeFromCollection));
				AssertNotNull(module.ToolBarButtons.FindByText("Delete"));

				AssertNotNull(module.NewMenuItem.MenuItems.FindByText("New A/R - Receivables Organizations Template"));
				AssertNotNull(module.NewMenuItem.MenuItems.FindByText("New A/P - Payables Organizations Template"));
			}
		}

		public void TestNewARAPOrganizationsTemplateMenuItems()
		{
			using (var module = new AccOrgTaxConfigurationTemplateModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("New"));

				var arMenuItem = module.NewMenuItem.MenuItems.FindByText("New A/R - Receivables Organizations Template");
				arMenuItem.PerformClick();
				var arForm = ZApplication.GetOpenForms().OfType<AccOrgTaxConfigurationTemplateForm>().FirstOrDefault();
				AssertNotNull(arForm);
				AssertEquals(true, (arForm.BusinessEntity as AccOrgTaxConfigurationTemplate).OCT_IsReceivable);
				arForm.Close();

				var apMenuItem = module.NewMenuItem.MenuItems.FindByText("New A/P - Payables Organizations Template");
				apMenuItem.PerformClick();
				var apForm = ZApplication.GetOpenForms().OfType<AccOrgTaxConfigurationTemplateForm>().FirstOrDefault();
				AssertNotNull(apForm);
				AssertEquals(false, (apForm.BusinessEntity as AccOrgTaxConfigurationTemplate).OCT_IsReceivable);
				apForm.Close();
			}
		}

		public void TestApplyTemplateMenuItems_EmptySelectErrorMessage()
		{
			using (var moduleForm = new ZForm())
			using (var module = new AccOrgTaxConfigurationTemplateModuleForTest())
			using (CreateMockHelper(out var mockHelper))
			{
				var menuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Apply Template");
				AssertNotNull(menuItem);

				module.BusinessObjectsToSelect = null;
				menuItem.PerformClick();
				AssertMultilineASCIIEquals(@"Please select a Tax Configuration Template", UnitTestUserNotification.Instance.LastMessage.Text);
				mockHelper.Verify(x => x.UpdateTaxConfigurationsForMultipleOrgnizations(It.IsAny<Guid>()), Times.Never);
			}
		}

		public void TestApplyTemplateMenuItems_MultiSelectErrorMessage()
		{
			using (var moduleForm = new ZForm())
			using (var module = new AccOrgTaxConfigurationTemplateModuleForTest())
			using (CreateMockHelper(out var mockHelper))
			{
				var menuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Apply Template");
				AssertNotNull(menuItem);

				var templates = new AccOrgTaxConfigurationTemplate[]
				{
					Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>(),
					Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>()
				};
				Factory.Save();

				module.BusinessObjectsToSelect = templates;
				menuItem.PerformClick();
				AssertMultilineASCIIEquals(@"Please only select one Tax Configuration Template", UnitTestUserNotification.Instance.LastMessage.Text);
				mockHelper.Verify(x => x.UpdateTaxConfigurationsForMultipleOrgnizations(It.IsAny<Guid>()), Times.Never);
			}
		}

		public void TestApplyTemplateMenuItems_Success()
		{
			using (var moduleForm = new ZForm())
			using (var module = new AccOrgTaxConfigurationTemplateModuleForTest())
			using (CreateMockHelper(out var mockHelper))
			{
				var menuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Apply Template");
				AssertNotNull(menuItem);

				var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
				template.OCT_IsReceivable = true;
				Factory.Save();

				module.BusinessObjectsToSelect = new AccOrgTaxConfigurationTemplate[] { template };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				AssertMultilineASCIIEquals("Successfully apply template tax configuration to linked organizations", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.Any(msg => msg.Text == @"Applying this Tax Configuration Template will update and re-default the Tax Configurations of all attached Organizations.
Do you want to proceed ? Select Yes to re-default Tax Configurations on all attached organizations.Select No to cancel this action."));
				mockHelper.Verify(x => x.UpdateTaxConfigurationsForMultipleOrgnizations(template.PK.ToGuid()), Times.Once);
			}
		}

		IDisposable CreateMockHelper(out Mock<IAccOrgTaxConfigurationTemplateDataHelper> mockHelper)
		{
			var mockDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockHelper = new Mock<IAccOrgTaxConfigurationTemplateDataHelper>();

			mockDependencyFactory.Setup(x => x.GetAccOrgTaxConfigurationTemplateDataHelper())
				.Returns(mockHelper.Object);

			return ObjectFactory.Substitute(mockDependencyFactory.Object);
		}
	}
}
