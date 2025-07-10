using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(WorkItemForm))]
	public class WorkItemFormTest : ZFormBasherTest
	{
		[TestUtcOffset(0, 0, 0)]
		public void TestReleaseSequencingPanelIsVisibleWhenReleaseSequencingIsEnabled()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var workitem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workitem, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23, 0, 0, 0);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (WorkItemForm form = new WorkItemForm(workitem))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<WorkItemReleaseSequenceControl>();
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

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = false;

			var workitem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workitem, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (WorkItemForm form = new WorkItemForm(workitem))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<WorkItemReleaseSequenceControl>();
				AssertNull(releaseSequencePanel);
			}
		}

		public void TestOpenSequenceButtonIsDisabledWhenNotLinkedToReleaseSequence()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var workitem = Factory.NewWithValidTestData<WorkItem>();

			Factory.Save();

			using (WorkItemForm form = new WorkItemForm(workitem))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<WorkItemReleaseSequenceControl>();
				var openSequenceButton = releaseSequencePanel.FindSingleOrDefault<ZButton>(c => c.Name == "OpenSequenceButton");

				AssertEquals(false, openSequenceButton.Enabled);
			}
		}

		public void TestOpenSequenceButton_OnClick()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://EagleDatamationInternational/Portals");

			helper.CreateSystem(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled = true;

			var workitem = Factory.NewWithValidTestData<WorkItem>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(workitem, Factory);
			jobHeader.FH_AgreedDeliveryDate = new ZDateTime(2021, 11, 23);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var sequence = helper.CreateReleaseSequence(Factory, group.PK.ToGuid(), "Name of sequence");
			var sequenceItem = helper.CreateReleaseSequenceItem(sequence, jobHeader, position: 1, value: 2, investment: 3);

			Factory.Save();

			using (WorkItemForm form = new WorkItemForm(workitem))
			{
				form.Show();

				var releaseSequencePanel = form.FindSingleOrDefault<WorkItemReleaseSequenceControl>();
				var openSequenceButton = releaseSequencePanel.FindSingleOrDefault<ZButton>(c => c.Name == "OpenSequenceButton");

				AssertEquals(true, openSequenceButton.Enabled);

				openSequenceButton.PerformClick();

				var recentLink = RecentItemManager.Instance.GetRecentItems(string.Empty).First();
				AssertEquals(recentLink.STL_ModuleID, ModuleIDs.BMReleaseSequence.Name);
				AssertEquals(recentLink.STL_ItemPK, sequence.BMR_PK);
				AssertEquals(recentLink.STL_ItemUrl, ShowEditFormUrlHandler.Instance.Create(ControllerIDs.BMReleaseSequence, sequence.BMR_PK));
			}
		}

		public void TestCustomisablePanelsHasIsVisibilityConfigured()
		{
			using (var form = (WorkItemForm)GetFormToBash())
			{
				var visibilityConfigurationProvider = form.GetVisibilityConfigurationProviderForTest();

				foreach (var panel in FindCustomisablePanels(form))
				{
					AssertEquals(panel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(panel));
				}
			}

			// SetIsVisibilityConfigured should be set after InitializeComponent (i.e. not in the designer)
			// since all controls need to be contructed first.
			// E.g., can't move a control from its default panel to another panel if the other panel doesn't exist yet.
			// So use the constructor for the designer and verify visibility not configured.
			using (var form = new WorkItemForm())
			{
				var visibilityConfigurationProvider = form.GetVisibilityConfigurationProviderForTest();

				foreach (var panel in FindCustomisablePanels(form))
				{
					AssertEquals(panel.Name, false, visibilityConfigurationProvider.GetIsVisibilityConfigured(panel));
				}
			}
		}

		public void TestWorkItemFormCaption()
		{
			WorkItem workItem = Factory.New<WorkItem>();
			workItem.WKI_WorkItemNumber = "WI12345";
			workItem.WKI_Summary = "Some Summary";

			using (WorkItemForm form = new WorkItemForm(workItem))
			{
				AssertEquals("WI12345 - Some Summary", form.FormCaption);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateTopPanelHeight()
		{
			WorkItem workItem = Factory.NewWithValidTestData<WorkItem>();
			using (MockWorkItemForm form = new MockWorkItemForm(workItem))
			{
				form.Show();
				SplitContainer splitter = form.GetSplitContainer();
				splitter.SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				workItem.WKI_WorkItemArea = "WHY";
				form.WindowState = FormWindowState.Minimized;
			}
		}

		#region Copy Schedules

		void TestCancelWorkItemWithCopySchedulesUsingScheduleDeactivatorForm(string buttonToClick, string workItemFinalStatus, bool copyScheduleActiveStatus)
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WorkflowItems.AddNew();
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Open;

			var workItem_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var workItem_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			workItem_copySchedule.SUC_CopyObjectId = workItem.PK;
			workItem_copyScheduleTask.S5_ParentID = workItem_copySchedule.PK;

			Factory.Save();

			using (var form = new MockWorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().FindByText("Cancel");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var scheduleDeactivatorForm = f as ZChildForm;
					AssertEquals("ScheduleDeactivatorInvocationForm implements IScheduleDeactivatorView, but we want to avoid explictly referencing the form here",
						true, f is IScheduleDeactivatorView);

					scheduleDeactivatorForm.Shown += (_, x_) =>
					{
						var button = scheduleDeactivatorForm.FindAll<ZButton>(b => b.Text == buttonToClick).Single();
						button.PerformClick();
						Application.DoEvents();
					};
				});

				cancelMenuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(workItemFinalStatus, workItem.WKI_Status);
				AssertEquals(copyScheduleActiveStatus, workItem_copyScheduleTask.S5_IsActive);
			}
		}

		public void TestCancelWorkItemWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkItemWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, false);
		}

		public void TestCancelWorkItemWithCopySchedules_WhenUserSelectsCancelAndDoNotDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkItemWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and do not deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, true);
		}

		public void TestDoNotCancelWorkItemWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkItemWithCopySchedulesUsingScheduleDeactivatorForm("Do not cancel or deactivate", ProcessTaskStatusCodeList.Codes.Open, true);
		}

		#endregion

		#region Related Items

		public void TestRelatedItemsTab_ShouldSupportWorkRequests()
		{
			ProcessMgmtTestHelper.SetMrsSullivanRelatedSelectionCriteriaValues();

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var workRequest = WorkRequestRelatedItemTest.GetPopulatedWorkRequestForTest(Factory);
			Factory.Save();

			var allLinks = Factory.Load<WorkItemRequestLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("The work item and work request should not be linked by default. SAD!", Array.Empty<WorkItemRequestLink>(), allLinks);

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var control = tabControl.SelectedTab.FindSingle<WorkTaskRelatedItemUserControl>();
				var grid = control.FindSingle<ZGrid>("RelatedItemGrid");

				IEnumerable<IWorkTaskRelatedItem> GetItemsFromGrid() => grid.ListManager.List.Cast<IWorkTaskRelatedItem>();

				AssertContainsExactElementsInAnyOrder("The grid should be empty by default. SAD!", Array.Empty<string>(), GetItemsFromGrid().Select(x => x.ItemDescription));

				var attachButton = control.FindSingle<ZButton>("AttachButton");
				attachButton.PerformClick();
				Application.DoEvents();

				var contextMenu = control.MenuStripAttach_ForTest;
				ToolStripItem menuItem = null;

				for (var i = 0; i < contextMenu.Items.Count; i++)
				{
					var currentMenuItem = contextMenu.Items[i];
					if (currentMenuItem.Text == "Customer Service Ticket")
					{
						menuItem = currentMenuItem;
						break;
					}
				}

				AssertNotNull("There should be an option to attach work requests to work items. SAD!", menuItem);

				EmbeddedModulePopup modulePopup = null;

				try
				{
					menuItem.PerformClick();
					Application.DoEvents();

					modulePopup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single();
					var moduleControl = (ZFilterStripCommonControl)modulePopup.Module_ForTest.EmbeddedControl;
					moduleControl.FirePerformSearch();

					modulePopup.Module_ForTest.DisplayGrid.Select(0);
					modulePopup.ExposedOKButtonForTesting.PerformClick();
					Application.DoEvents();
				}
				finally
				{
					modulePopup?.Dispose();
				}

				var items = GetItemsFromGrid();
				AssertContainsExactElementsInAnyOrder("The work request should now be attached and shown in the grid. SAD!", new[] { "Last Will and Testameow" }, items.Select(x => x.ItemDescription));

				form.FireSaveButton();
				Application.DoEvents();
			}

			var link = Factory.Load<WorkItemRequestLink>(new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, workItem.PK)).SingleOrDefault();
			AssertNotNull("The work item and work request should now be linked. SAD!", link);
			AssertEquals("The work item and work request should now be linked. SAD!", workRequest.PK, link.WKL_WKR_Request);

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var control = tabControl.SelectedTab.FindSingle<WorkTaskRelatedItemUserControl>();
				var grid = control.FindSingle<ZGrid>("RelatedItemGrid");
				AssertContainsExactElementsInAnyOrder("The work request should still be attached and shown in the grid. SAD!", new[] { "Last Will and Testameow" }, grid.ListManager.List.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));

				var selectionCriteriaColumns = new List<int>();

				int GetColumnNumber(int selectionCriterionNumber)
				{
					for (var i = 0; i < grid.Columns.Count; i++)
					{
						var column = grid.Columns[i];

						if (column.ColumnName == "SelectionCriterion" + selectionCriterionNumber)
						{
							AssertEquals("All the selection criteria columns should be visible on the form. SAD!", true, column.IsVisible);

							return i;
						}
					}

					return -1;
				}

				for (var i = 1; i <= 5; i++)
				{
					selectionCriteriaColumns.Add(GetColumnNumber(i));
				}

				AssertCollectionNotContains("All the selection criteria columns should be included in the grid. SAD!", -1, selectionCriteriaColumns);

				AssertEquals("AAA - Mrs Sullivan always planned to leave everything to her cats.", grid[0, selectionCriteriaColumns[0]]);
				AssertEquals("BBB - But sometimes, plans need a helping paw.", grid[0, selectionCriteriaColumns[1]]);
				AssertEquals("CCC - What are the kitties to do, but buckle together and work as a team.", grid[0, selectionCriteriaColumns[2]]);
				AssertEquals("DDD - This fall sparks will fly between one guy who can't get a break...", grid[0, selectionCriteriaColumns[3]]);
				AssertEquals("EEE - And nine cats who break all the rules.", grid[0, selectionCriteriaColumns[4]]);
			}
		}

		#endregion

		#region eConversation

		public void TestConversation_TabPageShouldBeVisible()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var tabPage = tabControl.FindAll<TabPage>().SingleOrDefault(l => l.Name == "eConversationTabPage");
				AssertNotNull("Oh no! The eConversation tab page does not exist on the work item form.", tabPage);
			}
		}

		public void TestConversation_WhenSendNormalMessage_ShouldShowUpInJobConversationAndWorkItemHasChanges()
		{
			AssertConversation_WhenSendMessage_ShouldShowUpInJobConversationAndWorkItemHasChanges("sendButton");
		}

		public void TestConversation_WhenSendInternalMessage_ShouldShowUpInJobConversationAndWorkItemHasChanges()
		{
			AssertConversation_WhenSendMessage_ShouldShowUpInJobConversationAndWorkItemHasChanges("AddInternalCommentButton");
		}

		void AssertConversation_WhenSendMessage_ShouldShowUpInJobConversationAndWorkItemHasChanges(string sendButtonName)
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("eConversationTabPage");
				Application.DoEvents();

				var control = form.FindSingle<EConversationFullControl>();
				var button = control.FindSingle<ZButton>(sendButtonName);

				AssertEquals("Precondition - The Work Item should not have changes.", false, workItem.HasChanges);

				const string msgText = "ok mate, so a brick weighs one kilogram plus half a brick, how much does the brick weigh??";
				control.SetMessageTextboxText(msgText);
				button.PerformClick();
				Application.DoEvents();

				AssertEquals("The work item should have changes since we added a new message.", true, workItem.HasChanges);
				AssertEquals("The new message should be in the workItem!", msgText, workItem.Conversation.Messages.Single().Body);
			}
		}

		public void TestConversation_WhenWorkItemUnsaved_ShouldDisableEConversationControls()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("eConversationTabPage");
				Application.DoEvents();

				var tabPage = tabControl.FindAll<TabPage>().SingleOrDefault(l => l.Name == "eConversationTabPage");
				var tabPageLabel = tabPage.FindSingle<ZLabel>();
				AssertEquals("The tab page must have this label on it", "The form must be saved and this tab reloaded before an eConversation can be started.", tabPageLabel.Text);
			}
		}

		public void TestConversation_WhenAddConversationMessageAndSave_ShouldSendNotificationEmailsToParticipants()
		{
			AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(isInternal: false);
		}

		public void TestConversation_WhenAddInternalMessageAndSave_ShouldSendNotificationEmailsToStaff()
		{
			AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(isInternal: true);
		}

		void AssertTestConversation_WhenAddMessageAndSave_ShouldSendNotificationEmails(bool isInternal)
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var resource1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, contactEmail: "superman@krypton.com");
			var resource2 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "theflash@speedforce.com");
			var resource3 = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, contactEmail: "livewire@electricity.com");
			var resource4 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "missmarvel@kree.com");

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("eConversationTabPage");
				Application.DoEvents();

				var participant1 = workItem.Conversation.Participants.AddNewParticipant(resource1);
				var participant2 = workItem.Conversation.Staff.AddNewParticipant(resource2);
				var participant3 = workItem.Conversation.RelatedParties.AddNewParticipant(resource3);
				var participant4 = workItem.Conversation.Staff.AddNewParticipant(resource4);
				participant4.JCP_IsSubscribed = false;

				form.FireSaveButton();
				Application.DoEvents();

				const string brickMsg = "A brick weighs one kilogram plus half a brick. How much does the brick weigh?";
				workItem.Conversation.AddMessageFromCurrentUser(brickMsg, isInternal);
				Application.DoEvents();

				AssertEquals(0, Env.AllEmailsCreated.Count());

				form.FireSaveButton();
				Application.DoEvents();

				var emails = Env.AllEmailsCreated.ToArray();

				var expectedEmailAddresses = new[]
				{
					new { Include = !isInternal, Email = "superman@krypton.com" },
					new { Include = true, Email = "theflash@speedforce.com" },
					new { Include = !isInternal, Email = "livewire@electricity.com" },
					new { Include = false, Email = "missmarvel@kree.com" }
				}.Where(l => l.Include).Select(m => m.Email);

				AssertContainsExactElementsInAnyOrder("Should have sent the email to the correct people.", expectedEmailAddresses, emails.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals("Should've sent the correct number of emails.", expectedEmailAddresses.Count(), Env.AllEmailsCreated.Count());

				emails.ForEach(email => AssertContains("The email should contain the stunning message about bricks.", brickMsg, email.Body));
			}
		}

		#endregion

		#region Custom Fields

		public void TestCustomFieldsPanel_WhenChangingAllWorkItemSelectionCriteriaDropLists_ShouldShowMatchingCustomFieldsWithoutReOpeningForm()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var customField = MasterFilesTestHelper.CreateCustomField(template, "Dinglebop");
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			AssertCustomFieldShown(template.P0_SubType1Info, workItem.WKI_WorkItemTypeInfo, "AAA");
			AssertCustomFieldShown(template.P0_SubType2Info, workItem.WKI_WorkItemAreaInfo, "BBB");
			AssertCustomFieldShown(template.P0_SubType3Info, workItem.WKI_ActivityTypeInfo, "CCC");
			AssertCustomFieldShown(template.P0_SubType4Info, workItem.WKI_ActivitySubtypeInfo, "DDD");
			AssertCustomFieldShown(template.P0_SubType5Info, workItem.WKI_PriorityInfo, "EEE");

			void AssertCustomFieldShown(ZPropertyInfo templatePropertyToSet, ZPropertyInfo workItemPropertyToSet, string selectionCriterionValue)
			{
				templatePropertyToSet.Value = (ZString)selectionCriterionValue;
				Factory.Save();

				using (var form = new WorkItemForm(workItem))
				{
					form.Show();
					Application.DoEvents();

					var customFieldsControl = form.FindSingle<ProcessTemplateCustomFieldsControl>();
					var nothingSetupLabel = customFieldsControl.FindSingle<ZLabel>("nothingSetupMessageLabel");

					AssertEquals("Pre-condition: work item does not currently match the template's selection criteria", true, nothingSetupLabel.Visible);
					AssertCustomFieldControlShown("Pre-condition: should not show the custom field because work item does not currently match the template's selection criteria", false);

					workItemPropertyToSet.Value = (ZString)selectionCriterionValue;
					Application.DoEvents();
					AssertCustomFieldControlShown("Now that the work item selection criteria matches the template, the custom field should be shown. It shouldn't be necessary to close and re-open the form.", true);

					void AssertCustomFieldControlShown(string assertionMessage, bool shouldBeShown)
					{
						var customFieldControl = customFieldsControl.FindSingleOrDefault<ZTextBox>();

						if (shouldBeShown)
						{
							AssertNotNull(assertionMessage, customFieldControl);
						}
						else
						{
							AssertNull(assertionMessage, customFieldControl);
						}
					}
				}
			}
		}

		public void TestCustomFieldsCanBindAllSupportedCharacterTypes()
		{
			var allCharsBuilder = new StringBuilder();

			for (char theChar = (char)0; theChar < char.MaxValue; ++theChar)
			{
				allCharsBuilder.Append(theChar);
			}

			var allCharsStrings = ((ZString)allCharsBuilder.ToString()).StripNonWesternEuropeanCharacters().Split(20);

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			foreach (var allCharString in allCharsStrings)
			{
				MasterFilesTestHelper.CreateCustomField(template, allCharString);
			}

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			using (var form = new WorkItemForm(workItem))
			{
				form.Show();
				Application.DoEvents();
				AssertNotNull("Custom Field Control Should be shown", form.FindSingle<ProcessTemplateCustomFieldsControl>());
			}
		}

		public void TestCustomFieldsDoNotAllowSubstitutionCharacters()
		{
			// 加 and 点 are substituted for + and . which cannot be used for binding. As a result, these character can never be allowed.
			var allCharsStrings = new[] { "加", "点" };
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			foreach (var allCharString in allCharsStrings)
			{
				Assert(MasterFilesTestHelper.CreateCustomField(template, allCharString).HasErrors);
			}
		}

		#endregion

		public static class CustomisablePanelNames
		{
			public const string LeftTopPanel = "LeftTopPanel";
			public const string LeftBottomPanel = "LeftBottomPanel";
			public const string MiddleTopPanel = "MiddleTopPanel";
			public const string MiddleBottomPanel = "MiddleBottomPanel";
			public const string RightTopPanel = "RightTopPanel";
			public const string RightBottomPanel = "RightBottomPanel";
		}

		protected string[] ExpectedPanelNames
		{
			get
			{
				return new string[]
				{
					CustomisablePanelNames.LeftTopPanel,
					CustomisablePanelNames.MiddleTopPanel,
					CustomisablePanelNames.RightTopPanel,
					CustomisablePanelNames.RightBottomPanel
				};
			}
		}

		protected List<ZPanel> FindCustomisablePanels(Control control)
		{
			List<ZPanel> listPanels = new List<ZPanel>();

			WalkControls(control, (ctrl) =>
			{
				if (ctrl is ZPanel && ExpectedPanelNames.Contains(ctrl.Name))
				{
					listPanels.Add(ctrl as ZPanel);
				}
				return true;
			});

			return listPanels;
		}

		protected void WalkControls(Control ctrl, Func<Control, bool> func)
		{
			if (!func(ctrl))
			{
				return;
			}

			foreach (Control childCtrl in ctrl.Controls)
			{
				WalkControls(childCtrl, func);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var workItem = Factory.New<WorkItem>();

			WorkItemForm result = new WorkItemForm(workItem);
			result.ControllerID = ControllerIDs.WorkItem;
			result.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(1524);
			return result;
		}
	}

	class MockWorkItemForm : WorkItemForm
	{
		public MockWorkItemForm(WorkItem workItem)
			: base(workItem)
		{
		}

		public SplitContainer GetSplitContainer()
		{
			return splitContainerMain;
		}
	}
}
