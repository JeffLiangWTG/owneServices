using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(ProjectModule))]
	class ProjectModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Project;
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = GetModule())
			{
				AssertEquals(Env.Licence.ProductivityTools, module.LicenceCheckPoint);
			}
		}

		public void TestActionsMenu_ShouldIncludeImportFromJiraMenuItem()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();

			using (var module = GetModuleWithBuiltContextMenu())
			{
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Import from Jira", false);

				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType<JiraProjectSelectorForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestImportFromJiraMenuItem_WhenNoUrlIsDefinedInRegistry_ShouldShowError()
		{
			using (var module = GetModuleWithBuiltContextMenu())
			{
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Import from Jira", false);

				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("Please enter the Jira site URL in the registry item Jira Site URLs within Productivity Tools/Project/Jira Integration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestImportFromJiraMenuItem_WhenValidationErrorPresent_ShouldInformUser()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();

			using (var module = GetDummyModuleWithBuiltContextMenu())
			{
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Import from Jira", false);
				var projectKey = default(ProjectKeyViewModel);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					if (form is JiraProjectSelectorForm selectionForm) // to ignore all the other forms that open during the importing process TODO shortcut for now
					{
						var control = selectionForm.FindSingle<JiraProjectSelectorUserControl>();
						var viewModel = (DummyProjectsToImportViewModel)selectionForm.BusinessEntity;
						viewModel.ShouldImportAllProjects = false;
						projectKey = viewModel.SpecificProjectsToImport.AddNew();

						control.BeginImport_ForTest(selectionForm);
						AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(false, selectionForm.IsDisposed);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						projectKey.ProjectKey = "R.I.C.H.A.R.D.: Richard-Impersonating Cybernetic Helper Automaton Richard Device";

						control.BeginImport_ForTest(selectionForm);
						AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(false, selectionForm.IsDisposed);

						viewModel.JiraUserName = "Homer.Simpson";
						viewModel.JiraAuthToken = "FlancrestEnterprises";

						control.BeginImport_ForTest(selectionForm);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertEquals("A successful closing of the form results in no previous messages", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				});

				menuItem.PerformClick();
				Application.DoEvents();
			}
		}

		ZFilterGridModule GetModuleWithBuiltContextMenu()
		{
			var module = (ZFilterGridModule)GetModule();
			_ = module.EmbeddedControl;

			return module;
		}

		#region Implementation - Empty Import Method Form

		ZFilterGridModule GetDummyModuleWithBuiltContextMenu()
		{
			var module = new DummyProjectModule();
			_ = module.EmbeddedControl;

			return module;
		}

		class DummyProjectModule : ProjectModule
		{
			protected override void ShowJiraImportForm()
			{
				ZFormModaliser.ShowDialogAndDispose(new DummyJiraImportForm(new BusinessObjectFactory()));
			}
		}

		class DummyJiraImportForm : JiraProjectSelectorForm
		{
			public DummyJiraImportForm(BusinessObjectFactory factory)
				: base(factory)
			{
				SetDataBinding(new DummyProjectsToImportViewModel(factory), "");
			}
		}

		class DummyProjectsToImportViewModel : ProjectsToImportViewModel
		{
			public DummyProjectsToImportViewModel(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ProjectImportReport ImportJiraContentAndReportResultCore(IJiraImporterProgressTracker progressTracker)
			{
				return new ProjectImportReport(true, "This response is for when everything is OK");
			}
		}

		#endregion
	}
}
