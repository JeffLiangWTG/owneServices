using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	class ProjectFormForTest : ProjectForm
	{
		public ProjectFormForTest(Project project)
			: base(project)
		{
		}

		public MenuItem[] ProjectAction_Exposed
		{
			get { return (MenuItem[])base.ProjectActionsMenuItems; }
		}
	}

	[TestedType(typeof(ProjectForm))]
	class ProjectFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Project project = Factory.New<Project>();
			ProjectForm result = new ProjectForm(project);
			result.ControllerID = ControllerIDs.Project;
			return result;
		}

		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequencingPanelIsVisibleWhenReleaseSequencingIsEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var project = Factory.NewWithValidTestData<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23, 0, 0, 0);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<ProjectReleaseSequenceControl>();
				AssertNotNull(releaseSequencePanel);

				AssertEquals("Name of sequence", releaseSequencePanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ReleaseSequenceNameBox").Text);
				AssertEquals("2", releaseSequencePanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ReleaseSequenceValueBox").Text);
				AssertEquals("1", releaseSequencePanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ReleaseSequencePositionBox").Text);
				AssertEquals("3", releaseSequencePanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ReleaseSequenceInvestmentBox").Text);
				AssertEquals("23-Nov-21 00:00", releaseSequencePanel.FindSingleOrDefault<ZTextBox>(c => c.Name == "ReleaseSequenceDateBox").Text);
			}
		}

		public void TestReleaseSequencingPanelIsNotVisibleWhenReleaseSequencingIsDisabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;

			var project = Factory.NewWithValidTestData<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<ProjectReleaseSequenceControl>();
				AssertNull(releaseSequencePanel);
			}
		}

		public void TestOpenSequenceButtonIsDisabledWhenNotLinkedToReleaseSequence()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var project = Factory.NewWithValidTestData<Project>();

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<ProjectReleaseSequenceControl>();
				var openSequenceButton = releaseSequencePanel.FindSingleOrDefault<ZButton>(c => c.Name == "OpenSequenceButton");

				AssertEquals(false, openSequenceButton.Enabled);
			}
		}

		public void TestOpenSequenceButton_OnClick()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://EagleDatamationInternational/Portals");

			helper.CreateSystem(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var project = Factory.NewWithValidTestData<Project>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(project, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<ProjectReleaseSequenceControl>();
				var openSequenceButton = releaseSequencePanel.FindSingleOrDefault<ZButton>(c => c.Name == "OpenSequenceButton");

				AssertEquals(true, openSequenceButton.Enabled);

				openSequenceButton.PerformClick();

				var recentLink = RecentItemManager.Instance.GetRecentItems(string.Empty).First();
				AssertEquals(recentLink.STL_ModuleID, ModuleIDs.BMReleaseSequence.Name);
				AssertEquals(recentLink.STL_ItemPK, sequence.BMR_PK);
				AssertEquals(recentLink.STL_ItemUrl, ShowEditFormUrlHandler.Instance.Create(ControllerIDs.BMReleaseSequence, sequence.BMR_PK));
			}
		}

		public void TestFormCaption()
		{
			var project = Factory.New<Project>();
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Another company";
			using (var form = new ProjectForm(project))
			{
				AssertEquals("Project", form.FormCaption.Trim());
				project.WKP_ProjectNumber = "PRJ00000001";
				project.WKP_Summary = "Important";
				project.WKP_OA_ClientAddress = org.MainAddress.PK;
				AssertEquals("Project PRJ00000001 - Important - Another company", form.FormCaption);
			}
		}

		[ExpectNoExceptions]
		public void TestCustomisablePanel_ChangeControlParent_NoExceptionThrown()
		{
			var template1 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode, "AAA");

			var template2 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ProjectWorkflowDescriptorCode, "AAA", "BBB");
			var criterionField2 = template2.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(f => f.ElementName == ProjectFormCustomisationSettingsProvider.ControlNames.Status);
			criterionField2.DisplayTabCode = ProjectFormCustomisationSettingsProvider.ControlNames.AdditionalDetailsTabName;

			Factory.Save();

			var project = ProcessMgmtTestHelper.CreateProject(Factory);
			project.WKP_Type = "AAA";

			Factory.Save();

			using (var form = new ProjectForm(project))
			{
				form.Show();
				Application.DoEvents();

				var detailsTabControl = form.FindSingle<ProjectDetailsControl>();
				AssertNotNull("The form customisation settings from template 1 should be applied, so the Status control should be visible on the Details Tab", detailsTabControl.FindSingleOrDefault<ZDropEdit>(ProjectFormCustomisationSettingsProvider.ControlNames.Status));

				project.WKP_SubType = "BBB";
				Application.DoEvents();
				var additionalDetailsTabControl = form.FindSingle<ProjectAdditionalDetailsControl>();
				AssertNull("The form customisation settings from template 2 should now be applied, so the Status control should NOT be visible on the Details Tab", detailsTabControl.FindSingleOrDefault<ZDropEdit>(ProjectFormCustomisationSettingsProvider.ControlNames.Status));
				AssertNotNull("The form customisation settings from template 2 should now be applied, so the Status control should be visible on the Additional Details Tab", additionalDetailsTabControl.FindSingleOrDefault<ZDropEdit>(ProjectFormCustomisationSettingsProvider.ControlNames.Status));
			}
		}

		#region Menu Items

		public void TestDisableActionMenuWhenViewOrDelete()
		{
			var project = Factory.NewWithValidTestData<Project>();
			Factory.Save();

			using (var viewForm = new ProjectFormForTest(project))
			{
				viewForm.DisplayMode = ODisplayMode.ReadOnly;
				viewForm.Show();
				AssertCustomisedActionsMenuAbility(false, viewForm);
				project.RelatedItems.Load(); // force 'changes' on children to trigger Project.HasChangesChanged event
				AssertCustomisedActionsMenuAbility(false, viewForm);
			}

			project.Reload();

			using (var editForm = new ProjectFormForTest(project))
			{
				editForm.DisplayMode = ODisplayMode.Edit;
				editForm.Show();
				AssertCustomisedActionsMenuAbility(true, editForm);
			}

			using (var deleteForm = new ProjectFormForTest(project))
			{
				deleteForm.DisplayMode = ODisplayMode.Delete;
				deleteForm.Show();
				AssertCustomisedActionsMenuAbility(false, deleteForm);
				project.RelatedItems.Load(); // force 'changes' on children to trigger Project.HasChangesChanged event
				AssertCustomisedActionsMenuAbility(false, deleteForm);
			}
		}

		void AssertCustomisedActionsMenuAbility(bool expected, ProjectFormForTest form)
		{
			CombineAssertions(() =>
			{
				foreach (var menuItem in form.ProjectAction_Exposed)
				{
					AssertEquals(string.Format("{0}'s {1} is enabled", form.Text, menuItem.Text), expected, menuItem.Enabled);
				}
			});
		}

		#endregion

		public void TestConversation_TabPageShouldBeVisible()
		{
			var project = Factory.NewWithValidTestData<Project>();
			using (var form = new ProjectForm(project))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var tabPage = tabControl.FindAll<TabPage>().SingleOrDefault(l => l.Name == "eConversationTabPage");
				AssertNotNull("The eConversation tab page does not exist on the project form.", tabPage);
			}
		}

		#region Copy Schedules

		void TestCancelProjectWithCopySchedulesUsingScheduleDeactivatorForm(string buttonToClick, string projectFinalStatus, bool copyScheduleActiveStatus)
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WorkflowItems.AddNew();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Open;

			var project_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var project_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			project_copySchedule.SUC_CopyObjectId = project.PK;
			project_copyScheduleTask.S5_ParentID = project_copySchedule.PK;

			Factory.Save();

			using (var form = new ProjectFormForTest(project))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().FindByText("Close/Cancel");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var closeCancelProjectForm = f as CloseProjectPopupForm;

					closeCancelProjectForm.Shown += (o, x_) =>
					{
						ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(s =>
						{
							var scheduleDeactivatorForm = s as ZChildForm;
							AssertEquals("ScheduleDeactivatorInvocationForm implements IScheduleDeactivatorView, but we want to avoid explictly referencing the form here",
								true, s is IScheduleDeactivatorView);

							scheduleDeactivatorForm.Shown += (_, y_) =>
							{
								var button = scheduleDeactivatorForm.FindAll<ZButton>(b => b.Text == buttonToClick).Single();
								button.PerformClick();
								Application.DoEvents();
							};
						});

						var closeTypeDropEdit = closeCancelProjectForm.FindAll<ZDropEdit>().Single();
						closeTypeDropEdit.Text = ProcessTaskStatusCodeList.Codes.Cancelled;

						var okButton = closeCancelProjectForm.FindAll<ZButton>(b => b.Text == "OK").Single();
						okButton.PerformClick();
						Application.DoEvents();
					};
				});

				cancelMenuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(projectFinalStatus, project.WKP_Status);
				AssertEquals(copyScheduleActiveStatus, project_copyScheduleTask.S5_IsActive);
			}
		}

		public void TestCancelProjectWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelProjectWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, false);
		}

		public void TestCancelProjectWithCopySchedules_WhenUserSelectsCancelAndDoNotDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelProjectWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and do not deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, true);
		}

		public void TestDoNotCancelProjectWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelProjectWithCopySchedulesUsingScheduleDeactivatorForm("Do not cancel or deactivate", ProcessTaskStatusCodeList.Codes.Open, true);
		}

		#endregion
	}
}
