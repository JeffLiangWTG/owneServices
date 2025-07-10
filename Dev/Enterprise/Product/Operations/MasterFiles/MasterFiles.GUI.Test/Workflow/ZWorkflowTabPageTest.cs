using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZWorkflowTabPageTest : ZTabPageControlTest
	{
		#region Columns Test

		public void TestMilestoneNotificationsHasSameColumnsAsTriggersNotifications()
		{
			using (var milestoneControl = new ZMilestonesUserControl())
			using (var completionActionsControl = new CompletionTriggerActionsUserControl())
			{
				var milestoneNotificationsGrid = milestoneControl.FindAll<ZGrid>().Single(n => n.Name == "TriggersGrid");
				var milestoneTemplateGrid = completionActionsControl.FindAll<ZGrid>().Single();

				var excludedMilestonesColumns = new HashSet<string>(new[] { "OverrideEmail" });
				var excludedTriggersColumns = new HashSet<string>(new[] { "PQ_Offset", "PQ_ActionReference" });

				AssertContainsExactElementsInAnyOrder(milestoneNotificationsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(s => s.ColumnName).Except(excludedMilestonesColumns).Union(excludedTriggersColumns),
					milestoneTemplateGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(s => s.ColumnName));
			}
		}

		public void TestMacroColumnUseSameOpenCloseSymbols()
		{
			using (var milestoneControl = new ZMilestonesUserControl())
			using (var completionActionsControl = new CompletionTriggerActionsUserControl())
			{
				var milestoneNotificationsGrid = milestoneControl.FindAll<ZGrid>().Single(n => n.Name == "TriggersGrid");
				var milestoneTemplateGrid = completionActionsControl.FindAll<ZGrid>().Single();

				AssertContainsExactElementsInAnyOrder(milestoneNotificationsGrid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Where(s => s.SupportsMacroTemplates).Select(s => s.ColumnName + s.MacroClosingBracket + s.MacroOpeningBracket),
					milestoneTemplateGrid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().Where(s => s.SupportsMacroTemplates).Select(s => s.ColumnName + s.MacroClosingBracket + s.MacroOpeningBracket));
			}
		}

		public void TestExpressionInFieldNameAndFieldValue()
		{
			using (var control = new CompletionTriggerActionsUserControl())
			{
				var fieldNameColumnStyle = control.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldName");
				var fieldValueColumnStyle = control.CompletionTriggerActionGrid.GetColumnStyle("PQ_FieldValue");

				AssertEquals("PQ_FieldName should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)fieldNameColumnStyle).IsUsedForExpressions);
				AssertEquals("PQ_FieldValue should accept equal-sign.", true, ((ZMacrosFindBoxColumnStyleInfo)fieldValueColumnStyle).IsUsedForExpressions);
			}
		}

		public void TestPQ_RelatedEntity()
		{
			using (var control = new CompletionTriggerActionsUserControl())
			{
				AssertEquals("PQ_RelatedEntityId is visible", true, control.CompletionTriggerActionGrid.GetColumnStyle("PQ_RelatedEntityId").IsVisible);
				AssertNull("PQ_RelatedEntityTableCode is not present", control.CompletionTriggerActionGrid.GetColumnStyle("PQ_RelatedEntityTableCode"));
			}
		}

		#endregion

		#region Universal Data Manual Export Action Menu Items

		public void TestSendUniversalDataCannotBeRunWhenBusinessObjectHasChanges()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage(shouldSave: false))
			{
				var dummyBO = form.DataSource as DummyWithWorkflow;
				dummyBO.Z0_Code = "!?!";
				var sendUniversalXmlMenuItem = GetUniversalXmlMenuItem(form);
				var universalEventMenuItem = sendUniversalXmlMenuItem.MenuItems.FindByText("Universal Event");
				AssertNotNull("'Universal Event' menu item", universalEventMenuItem);

				AssertEquals("Precondition", true, dummyBO.HasChanges);
				TrySendUXmlActionAndAssertThatFormIsNotShownDueToUnsavedChanges(dummyBO, universalEventMenuItem);

				dummyBO.HasChanges = false;
				TrySendUXmlActionAndAssertThatFormIsNotShownDueToUnsavedChanges(dummyBO, universalEventMenuItem);

				Factory.Save();
				universalEventMenuItem.PerformClick();
				AssertEquals("lastFormShown.FormHeading", "Manually Send XML Universal Event", ((ZForm)ZFormModaliser.LastFormShownDialogForTest).FormHeading);
			}
		}

		static void TrySendUXmlActionAndAssertThatFormIsNotShownDueToUnsavedChanges(DummyWithWorkflow dummyBO, MenuItem universalEventMenuItem)
		{
			universalEventMenuItem.PerformClick();
			if (ZFormModaliser.LastFormShownDialogForTest is ZForm lastFormShown)
			{
				AssertNotEquals("lastFormShown.FormHeading", "Manually Send XML Universal Event", lastFormShown.FormHeading);
			}
			AssertEquals("Last Message", "Warning Please save your changes before sending XML Universal Data.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestUniversalShipmentMenuItemShowsSendUniversalXmlDialog()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage())
			{
				var sendUniversalXmlMenuItem = GetUniversalXmlMenuItem(form);
				var universalShipmentMenuItem = sendUniversalXmlMenuItem.MenuItems.FindByText("Universal Shipment");
				AssertNotNull("'Universal Shipment' menu item", universalShipmentMenuItem);
				universalShipmentMenuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest as ZForm;
				AssertNotNull("Form Shown in Test", lastFormShown);
				AssertEquals("lastFormShown.FormHeading", "Manually Send XML Universal Shipment", lastFormShown.FormHeading);
			}
		}

		public void TestUniversalEventMenuItemShowsSendUniversalXmlDialog()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage())
			{
				var sendUniversalXmlMenuItem = GetUniversalXmlMenuItem(form);
				var universalEventMenuItem = sendUniversalXmlMenuItem.MenuItems.FindByText("Universal Event");
				AssertNotNull("'Universal Event' menu item", universalEventMenuItem);
				universalEventMenuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest as ZForm;
				AssertNotNull("Form Shown in Test", lastFormShown);
				AssertEquals("lastFormShown.FormHeading", "Manually Send XML Universal Event", lastFormShown.FormHeading);
			}
		}

		public void TestUniversalTransactionMenuItemShowsSendUniversalXmlDialog()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage())
			{
				var sendUniversalXmlMenuItem = GetUniversalXmlMenuItem(form);
				var universalTransactionMenuItem = sendUniversalXmlMenuItem.MenuItems.FindByText("Universal Transaction");
				AssertNotNull("'Universal Transaction' menu item", universalTransactionMenuItem);
				universalTransactionMenuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest as ZForm;
				AssertNotNull("Form Shown in Test", lastFormShown);
				AssertEquals("lastFormShown.FormHeading", "Manually Send XML Universal Transaction", lastFormShown.FormHeading);
			}
		}

		public void TestUniversalActionMenuItemsAreAdded()
		{
			AssertUniversalDataMenuItems("Universal Shipment, Universal Event, Universal Transaction", true, true, true);
			AssertUniversalDataMenuItems("Universal Shipment, Universal Event", true, true, false);
			AssertUniversalDataMenuItems("Universal Event", false, true, false);
			AssertUniversalDataMenuItems("Universal Event, Universal Transaction", false, true, true);
			AssertUniversalDataMenuItems("Universal Shipment", true, false, false);
			AssertUniversalDataMenuItems("Universal Shipment, Universal Transaction", true, false, true);
			AssertUniversalDataMenuItems("Universal Transaction", false, false, true);
			AssertUniversalDataMenuItems(null, false, false, false);
			AssertUniversalDataMenuItems("Universal Activity", false, false, false, managesActivities: true);
		}

		void AssertUniversalDataMenuItems(string expectedMenuItems, bool managesShipments, bool managesEvents, bool manageInvoices, bool managesSchedules = false, bool managesActivities = false)
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage(managesShipments, managesEvents, manageInvoices, managesSchedules, managesActivities))
			{
				var sendUniversalXmlMenuItem = GetUniversalXmlMenuItem(form);

				if (expectedMenuItems == null)
				{
					AssertNull("'Send Universal XML menu item under 'Actions' menu (Should not exist...)", sendUniversalXmlMenuItem);
				}
				else
				{
					AssertNotNull("'Send Universal XML menu item under 'Actions' menu", sendUniversalXmlMenuItem);

					var menuItems = new List<string>();
					foreach (MenuItem menuItem in sendUniversalXmlMenuItem.MenuItems)
					{
						menuItems.Add(menuItem.Text);
					}

					AssertEquals("sendUniversalXmlMenuItem.MenuItems", expectedMenuItems, string.Join(", ", menuItems.ToArray()));
				}
			}
		}

		public void TestReapplyTemplateMenuItem_OnlyWhenSupportsReapplyTemplatesMenuItem()
		{
			DummyWorkflowDescriptor.Instance.SupportsReapplyTemplatesMenuItemExposed = true;
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage())
			{
				var actionsMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem;
				var menuItem = actionsMenuItem.MenuItems.FindByText("Reapply Workflow Templates");
				AssertNotNull(menuItem);
			}

			DummyWorkflowDescriptor.Instance.SupportsReapplyTemplatesMenuItemExposed = false;
			using (var form = GetNewFormWithInitializedZWorkFlowTabPage())
			{
				var actionsMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem;
				var menuItem = actionsMenuItem.MenuItems.FindByText("Reapply Workflow Templates");
				AssertNull(menuItem);
			}
		}

		ZForm GetNewFormWithInitializedZWorkFlowTabPage(bool shouldSave = true)
		{
			return GetNewFormWithInitializedZWorkFlowTabPage(true, true, true, true, true, shouldSave);
		}

		ZForm GetNewFormWithInitializedZWorkFlowTabPage(bool managesShipments, bool managesEvents, bool manageInvoices, bool managesSchedules = false, bool managesActivities = false, bool shouldSave = true)
		{
			var dummyBusinessObject = Factory.NewWithValidTestData<DummyWithWorkflowAndUniversalXmlManagement>();
			dummyBusinessObject.ManagesShipments = managesShipments;
			dummyBusinessObject.ManagesEvents = managesEvents;
			dummyBusinessObject.ManagesSchedules = managesSchedules;
			dummyBusinessObject.ManagesTransactions = manageInvoices;
			dummyBusinessObject.ManagesActivities = managesActivities;

			var form = new ZForm(dummyBusinessObject);
			form.ControllerID = DummyControllerIDs.Dummy;
			var tabControl = new ZTemplateTabControl();
			form.Controls.Add(tabControl);
			var tabPage = new TestWorkflowTabPage();
			tabControl.TabPages.Add(tabPage);
			tabPage.Initialize(dummyBusinessObject);
			if (shouldSave)
			{
				Factory.Save();
			}
			return form;
		}

		static MenuItem GetUniversalXmlMenuItem(ZForm form)
		{
			var actionsMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem;
			AssertNotNull("Precondition: Actions menu item", actionsMenuItem);
			return actionsMenuItem.MenuItems.FindByText("Send Universal XML");
		}

		#endregion

		public void TestExcludeFromBindingOnSave()
		{
			AssertEquals("For performance don't bind on save", true, TestTabPage.ExcludeFromBindingOnSave);
		}

		[RequiresSTA]
		public void TestHostedWorkflowControl()
		{
			WorkflowTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif

			Form.Controls.Add(TestTabControl);
			Form.Show();
			TestTabControl.SelectedTab = UnrelatedTabPage;

			AssertEquals(1, WorkflowTabPage.Controls.Count);
			ZTabPage workflowTabPage = new ZWorkflowTabPage();
			AssertEquals(typeof(ZWorkflowUserControl), workflowTabPage.Controls[0].GetType());
			workflowTabPage.Dispose();
		}

		public void TestInitialize_TrackingNotSupported()
		{
			TestWorkflowDescriptor provider = new TestWorkflowDescriptor();
			provider.SupportsEventTrackingExposed = false;
			TestTabPage.WorkflowDescriptor = provider;
			TestTabPage.Initialize(Dummy);
			AssertEquals("Workflow", TestTabPage.Text);
		}

		public void TestInitialize_WithTrackingSupported()
		{
			TestWorkflowDescriptor provider = new TestWorkflowDescriptor();
			provider.SupportsEventTrackingExposed = true;
			TestTabPage.WorkflowDescriptor = provider;
			TestTabPage.Initialize(Dummy);
			AssertEquals("Workflow && Tracking", TestTabPage.Text);
		}

		public void TestInitialize_Initialized()
		{
			TestWorkflowDescriptor provider = new TestWorkflowDescriptor();
			provider.SupportsEventTrackingExposed = false;
			TestTabPage.WorkflowDescriptor = provider;
			Assert("Initialize is not called yet", !(TestTabPage as IWorkflowTabPage).Initialized);
			TestTabPage.Initialize(Dummy);
			Assert("Initialize is called", (TestTabPage as IWorkflowTabPage).Initialized);
		}

		[RequiresSTA]
		public void TestLoginToWorkflowLicenceOnTabChange()
		{
			TestTabControl.TabPages.Clear();

			WorkflowTabPage.Initialize(Dummy);
			ZTabPage dummyTabPage = new ZTabPage();
			TestTabControl.TabPages.Add(dummyTabPage);
			TestTabControl.TabPages.Add(WorkflowTabPage);
			Form.Controls.Add(TestTabControl);
			Form.Show();

			AssertEquals("Tab not active initially", dummyTabPage, TestTabControl.SelectedTab);
			AssertEquals("Licence not acquired initially", false, Env.Licence.Workflow.IsLoggedIn);
			TestTabControl.SelectedTab = WorkflowTabPage;
			AssertEquals("Tab page changed successfully", TestTabPage, TestTabControl.SelectedTab);
			AssertEquals("Licence acquired", true, Env.Licence.Workflow.IsLoggedIn);

			WorkflowTabPage.Dispose();
			AssertEquals("Licence relinquished after tab page disposed", false, Env.Licence.Workflow.IsLoggedIn);
		}

		[RequiresSTA]
		public void TestAlarmIconShownWhenHasUnresolvedExceptions()
		{
			ProcessTask exception = Dummy.WorkflowItems.Exceptions.AddNew();
			TestTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif
			Form.Controls.Add(TestTabControl);

			Form.Show();
			AssertEquals("UserIdleWorker should have a work item to update the image", true, UserIdleWorker.IsActive);
			UserIdleWorker.Flush();
			AssertEquals("UpdateInitialTabImageCore not called", true, WorkflowTabPage.UpdateInitialTabImageCoreCalled);
			FireApplicationIdle();
			AssertEquals("Icon shown when there are outstanding exceptions", Icons.GetImageIndex(IconTypes.RedAlarmBell), WorkflowTabPage.ImageIndex);

			TestTabControl.SelectedTab = WorkflowTabPage;
			exception.IsExceptionActioned = true;
			FireApplicationIdle();
			AssertEquals("Icon removed when all exceptions resolved", -1, WorkflowTabPage.ImageIndex);
		}

		[RequiresSTA]
		public void TestAlarmIconNotOverridesOtherIcons()
		{
			ProcessTask exception = Dummy.WorkflowItems.Exceptions.AddNew();
			TestTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif
			Form.Controls.Add(TestTabControl);

			Form.Show();
			AssertEquals("UserIdleWorker should have a work item to update the image", true, UserIdleWorker.IsActive);
			UserIdleWorker.Flush();
			AssertEquals("UpdateInitialTabImageCore not called", true, WorkflowTabPage.UpdateInitialTabImageCoreCalled);
			WorkflowTabPage.ImageIndex = Icons.GetImageIndex(IconTypes.Error);
			FireApplicationIdle();
			AssertEquals("Error icon not overriden by alarm", Icons.GetImageIndex(IconTypes.Error), WorkflowTabPage.ImageIndex);
		}

		[RequiresSTA]
		public void TestAlarmIconShownWhenRelatedItemHasUnresolvedExceptions()
		{
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask relatedException = Dummy.RelatedDummyWithTasks.WorkflowItems.Exceptions.AddNew();
			TestTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif
			Form.Controls.Add(TestTabControl);

			Form.Show();
			AssertEquals("UserIdleWorker should have a work item to update the image", true, UserIdleWorker.IsActive);
			UserIdleWorker.Flush();
			AssertEquals("UpdateInitialTabImageCore not called", true, WorkflowTabPage.UpdateInitialTabImageCoreCalled);
			FireApplicationIdle();
			AssertEquals("Icon shown when there are outstanding exceptions", Icons.GetImageIndex(IconTypes.RedAlarmBell), WorkflowTabPage.ImageIndex);

			TestTabControl.SelectedTab = WorkflowTabPage;
			relatedException.IsExceptionActioned = true;
			FireApplicationIdle();
			AssertEquals("Icon removed when all exceptions resolved", -1, WorkflowTabPage.ImageIndex);
		}

		[RequiresSTA]
		public void TestNavigateToWorkflowItem()
		{
			WorkflowTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif
			Form.Controls.Add(TestTabControl);
			TestTabControl.SelectedTab = UnrelatedTabPage;
			Form.Show();

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			WorkflowTabPage.NavigateToWorkflowItem(milestone);
			Assert("MainTabControl should be switched to Workflow&Tracking tab", TestTabControl.SelectedTab == WorkflowTabPage);
			AssertEquals("(ZWorkflowUserControl)TrackingUserControl.NavigateToWorkflowItem(Task) was called", WorkflowTabPage.GetTrackingUserControl.NavigateToWorkflowItemCall);
		}

		[RequiresSTA]
		public void TestMilestoneExceptionNotAddedForFutureDateFromFormSave()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = new ZForm(dummy) { ControllerID = DummyControllerIDs.Dummy })
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.AddRange(new[] { tabPage1, workflowTabPage });
				form.Show();

				workflowTabPage.Initialize(dummy);

				var milestone = dummy.WorkflowItems.Milestones.AddNew();
				milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(1));

				var saved = form.FireSaveButton();
				AssertEquals(ContinueWithSave.No, saved);
				AssertEquals("Future date exception should not be added when save fails due to validation error.", 0, dummy.WorkflowItems.Exceptions.Count);

				Factory.Save();
				AssertEquals("Post condition: Expecting future date exception when save succeeds.", 1, dummy.WorkflowItems.Exceptions.Count);
			}
		}

		[RequiresSTA]
		public void TestDoNotRefreshBindingBetweenSettingLocalAndUtcDateProperties()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			using (var form = new ZForm(dummy) { ControllerID = DummyControllerIDs.Dummy })
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				workflowTabPage.Initialize(dummy);
				form.Controls.Add(tabControl);
				tabControl.TabPages.AddRange(new[] { tabPage1, workflowTabPage });
				form.Show();

				var milestone = dummy.WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				Factory.Save();

				Application.DoEvents();
				workflowTabPage.NavigateToWorkflowItem(milestone);

				AssertEquals("Precondition:", true, milestone.IsBound);

				milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
				AssertNoExceptionThrown("If bindings refresh when the UTC date and local date are out of sync there should be no offset exceptions",
					() => milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-2));

				milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
				AssertNoExceptionThrown("If bindings refresh when the UTC date and local date are out of sync there should be no offset exceptions",
					() => milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-2));

				milestone.P9_SuspendedAtForBinding = ZDateTimeOffset.Now;
				AssertNoExceptionThrown("If bindings refresh when the UTC date and local date are out of sync there should be no offset exceptions",
					() => milestone.P9_SuspendedAtForBinding = ZDateTimeOffset.Now.AddDays(-2));

				milestone.P9_ExceptionAddedForBinding = ZDateTimeOffset.Now;
				AssertNoExceptionThrown("If bindings refresh when the UTC date and local date are out of sync there should be no offset exceptions",
					() => milestone.P9_ExceptionAddedForBinding = ZDateTimeOffset.Now.AddDays(-2));
			}
		}

		[RequiresSTA]
		public void TestMCRHasDotDotDot()
		{
			var trigger = (ProcessTask)Dummy.AddNewTrigger();
			trigger.TriggerConditions.TriggerCondition = "MCR";
			trigger.TriggerConditions.TriggerConditionValue = "\"1\"==\"1\"";
			Factory.Save();

			WorkflowTabPage.Initialize(Dummy);
#if !WINZOR
			TestTabControl.TabPages.Add(WorkflowTabPage);
#endif

			Form.Controls.Add(TestTabControl);
			TestTabControl.SelectedTab = WorkflowTabPage;
			Form.Width = 1000;
			Form.Height = 1000;

			Form.Show();

			Application.DoEvents();
			WorkflowTabPage.NavigateToWorkflowItem(trigger);
			Application.DoEvents();
			Assert("MainTabControl should be switched to Workflow&Tracking tab", TestTabControl.SelectedTab == WorkflowTabPage);
			AssertEquals("(ZWorkflowUserControl)TrackingUserControl.NavigateToWorkflowItem(Task) was called", WorkflowTabPage.GetTrackingUserControl.NavigateToWorkflowItemCall);
		}

		[RequiresSTA]
		public void TestNavigateToWorkflowItem_NoExceptionWhenChangeToWorkflowTabCancelled()
		{
			var workflowTabPage = new ZWorkflowTabPage();
			workflowTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
			TestTabControl.TabPages.Add(workflowTabPage);
			Form.Controls.Add(TestTabControl);
			TestTabControl.SelectedTab = UnrelatedTabPage;
			Form.Show();

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			TestTabControl.Selecting += (sender, e) => e.Cancel = true;

			AssertNoExceptionThrown(() =>
				{
					workflowTabPage.NavigateToWorkflowItem(milestone);
				});
		}

		[RequiresSTA]
		public void TestNavigateToWorkflow_HiddenTab()
		{
			WorkflowTabPage.Initialize(Dummy);
			TestTabControl.TabPages.Add(WorkflowTabPage);
			Form.Controls.Add(TestTabControl);
			WorkflowTabPage.TabVisible = false;
			Form.Show();

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			WorkflowTabPage.NavigateToWorkflowItem(milestone);
			string expectedError = "Error Cannot navigate to workflow & tracking tab because it is hidden.";
			AssertEquals("Error message of hidden-tab must be shown.", expectedError, UnitTestUserNotification.Instance.LastMessage.ToString());

			WorkflowTabPage.Dispose();
		}

		public void TestMinimumAutoSize()
		{
			AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(935), WorkflowTabPage.MinimumAutoSizedWidth);
			AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(520), WorkflowTabPage.MinimumAutoSizedHeight);
		}

		public void TestConsumeWorkflowLicence()
		{
			Assert(!Env.Licence.Workflow.IsLoggedIn);

			WorkflowTabPage.ConsumeWorkflowLicence();
			Assert(Env.Licence.Workflow.IsLoggedIn);

			using (TestWorkflowTabPage anotherWorkflowTabPage = new TestWorkflowTabPage())
			{
				anotherWorkflowTabPage.ConsumeWorkflowLicence();
				Assert(Env.Licence.Workflow.IsLoggedIn);
			}
			Assert(Env.Licence.Workflow.IsLoggedIn);

			WorkflowTabPage.Dispose();
			Assert(!Env.Licence.Workflow.IsLoggedIn);
		}

		[RequiresSTA]
		public void TestControlW_ShouldShowWorkflowTab()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			using (var form = new ZForm(dummy) { ControllerID = DummyControllerIDs.Dummy })
			using (var tabControl = new ZTabControl())
			using (var tabPage1 = new ZTabPage())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.AddRange(new[] { tabPage1, workflowTabPage });
				form.Show();

				workflowTabPage.Initialize(dummy);

				AssertEquals(tabPage1, tabControl.SelectedTab);

				KeySender.SendKeyDownToProcessCmdKey(form, (int)(Keys.Control | Keys.Q));
				AssertEquals(tabPage1, tabControl.SelectedTab);

				KeySender.SendKeyDownToProcessCmdKey(form, (int)(Keys.Control | Keys.W));
				AssertEquals(workflowTabPage, tabControl.SelectedTab);
			}
		}

		[RequiresSTA]
		public void TestEventViewSecurityWhenNotAllowed()
		{
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowEventsJustViewAutoGeneratedCode);
			eventCheckpoint.IsAllowed = false;

			var dummy = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var tabControl = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.AddRange(new[] { workflowTabPage });
				workflowTabPage.Initialize(dummy);
				form.Show();

				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();
				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var securityDeniedLabel = eventsTab.FindSingle<ZLabel>("SecurityDeniedLabel");
				AssertNotNull(securityDeniedLabel);
				AssertEquals(true, securityDeniedLabel.Visible);
				AssertEquals("You do not have the appropriate security rights to run this function." +
					"\r\n\r\nIf you require access to this function, ask your system administrator to change either your " +
					"Staff or Group Security Rights to allow access to:\r\n\r\nMaintain -> Master Data -> Organization -> Workflow -> View Events", securityDeniedLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestEventViewSecurityWhenAllowed()
		{
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowEventsJustViewAutoGeneratedCode);
			Assert(eventCheckpoint.IsAllowed);
			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var tabControl = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(tabControl);
				tabControl.TabPages.AddRange(new[] { workflowTabPage });
				workflowTabPage.Initialize(dummy);
				form.Show();

				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();
				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var securityDeniedLabel = eventsTab.FindSingleOrDefault<ZLabel>("SecurityDeniedLabel");
				AssertNull(securityDeniedLabel);
			}
		}
		void InvokeContextMenuPopupEvent(ContextMenu menu)
		{
			typeof(ContextMenu).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menu, new object[] { EventArgs.Empty });
		}

		MenuItem GetAddNewEventMenuItem(ContextMenu menu)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text == "&Add New Event")
				{
					return menuItem;
				}
			}
			return null;
		}

		[RequiresSTA]
		public void TestAddNewEventWhenNotAllowed()
		{
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowAddEventsAutoGeneratedCode);
			eventCheckpoint.IsAllowed = false;
			var dummy = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var control = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(control);
				control.TabPages.Add(workflowTabPage);
				workflowTabPage.Initialize(dummy);
				form.Show();
				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();

				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var eventsControl = eventsTab.FindSingleOrDefault<ZStmALogFilterControl>();
				InvokeContextMenuPopupEvent(eventsControl.FilteredGrid.ContextMenu);
				var addNewEventMenuItem = GetAddNewEventMenuItem(eventsControl.FilteredGrid.ContextMenu);
				Assert(addNewEventMenuItem.Visible);
				Assert(!addNewEventMenuItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestAddNewEventWhenAllowed()
		{
			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowAddEventsAutoGeneratedCode);
			Assert(eventCheckpoint.IsAllowed);
			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var control = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(control);
				control.TabPages.Add(workflowTabPage);
				workflowTabPage.Initialize(dummy);
				form.Show();
				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();

				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var eventsControl = eventsTab.FindSingleOrDefault<ZStmALogFilterControl>();
				InvokeContextMenuPopupEvent(eventsControl.FilteredGrid.ContextMenu);
				var addNewEventMenuItem = GetAddNewEventMenuItem(eventsControl.FilteredGrid.ContextMenu);
				Assert(addNewEventMenuItem.Visible);
				Assert(addNewEventMenuItem.Enabled);
				using (ZFormModaliser.SuspendDispose())
				{
					addNewEventMenuItem.PerformClick();
					var addForm = (ZStmALogAddForm)ZFormModaliser.LastFormShownDialogForTest;
					addForm.Show();
					var addFormBusinessEntity = (BaseStmALog)addForm.BusinessEntity;
					addFormBusinessEntity.SL_SE_NKEvent = Events.ArrivalCode;
					var addButton = (ZButton)addForm.Controls["FlowLayoutPanel"].Controls["AddButton"];

					var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
					var allLogs = dummy.Logs.GetAllLogs();
					var arvLogsCount_Before = allLogs.Find(query).Length;

					addButton.PerformClick();

					var arvLogsCount_After = allLogs.Find(query).Length;
					AssertEquals("A new event should have been added", arvLogsCount_Before + 1, arvLogsCount_After);
				}
			}
		}

		MenuItem GetCancelMenuItem(ContextMenu menu)
		{
			foreach (MenuItem menuItem in menu.MenuItems)
			{
				if (menuItem.Text.ToLower().IndexOf("cancel") != -1)
				{
					return menuItem;
				}
			}
			return null;
		}

		[RequiresSTA]
		public void TestCancelEventWhenAllowed()
		{
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowCancelEventsAutoGeneratedCode);
			Assert("Precondition", eventCheckpoint.IsAllowed);
			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var control = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(control);
				control.TabPages.Add(workflowTabPage);
				workflowTabPage.Initialize(dummy);
				form.Show();
				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();

				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var userControl = eventsTab.FindSingleOrDefault<ZStmALogUserControl>();

				var module = userControl.LogsModule;
				var eventsControl = module.EmbeddedControl as ZStmALogFilterControl;

				((ModuleFlagsFilter)(eventsControl.FilterBusinessObject as ZStmALogFilterBusinessObject)[ZStmALogFilterBusinessObject.Schema.ShowCancelled]).Property0 = ZBool.True;
				var log = dummy.Logs.AddNew();
				module.PerformSearch_ForTest();

				var grid = eventsControl.FindSingleOrDefault<ZDisplayGrid>();
				grid.SelectAllElements();
				InvokeContextMenuPopupEvent(eventsControl.FilteredGrid.ContextMenu);
				var cancelItem = GetCancelMenuItem(eventsControl.FilteredGrid.ContextMenu);

				Assert(cancelItem.Visible);
				Assert(cancelItem.Enabled);
				cancelItem.PerformClick();
				InvokeContextMenuPopupEvent(eventsControl.FilteredGrid.ContextMenu);
				cancelItem = GetCancelMenuItem(eventsControl.FilteredGrid.ContextMenu);
				AssertEquals("Event already canceled", cancelItem.Text);
				AssertEquals(false, cancelItem.Enabled);
			}
		}

		[RequiresSTA]
		public void TestCancelEventWhenNotAllowed()
		{
			var eventCheckpoint = Env.Security.FindOrCreateWorkflowItemCheckpoint(Env.Security.Organisation, SecurityCore.WorkflowCancelEventsAutoGeneratedCode);
			eventCheckpoint.IsAllowed = false;
			var dummy = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(dummy) { ControllerID = ControllerIDs.Organisation })
			using (var control = new ZTabControl())
			using (var workflowTabPage = new ZWorkflowTabPage())
			{
				form.Controls.Add(control);
				control.TabPages.Add(workflowTabPage);
				workflowTabPage.Initialize(dummy);
				form.Show();
				Application.DoEvents();
				workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab = workflowTabPage.TrackingUserControl.MainTabControl.GetTabPage("EventsTab");
				Application.DoEvents();

				var eventsTab = workflowTabPage.TrackingUserControl.MainTabControl.SelectedTab;
				var eventsControl = eventsTab.FindSingleOrDefault<ZStmALogFilterControl>();
				InvokeContextMenuPopupEvent(eventsControl.FilteredGrid.ContextMenu);
				var cancelItem = GetCancelMenuItem(eventsControl.FilteredGrid.ContextMenu);
				Assert(cancelItem.Visible);
				Assert(!cancelItem.Enabled);
			}
		}
		#region Test Classes

		class TestWorkflowTabPage : ZWorkflowTabPage
		{
			public WorkflowDescriptor WorkflowDescriptor;
			protected override WorkflowDescriptor GetWorkflowDescriptor(ZString workflowType)
			{
				return WorkflowDescriptor ?? base.GetWorkflowDescriptor(workflowType);
			}

			public bool UpdateInitialTabImageCoreCalled;
			protected override void UpdateInitialTabImageCore()
			{
				UpdateInitialTabImageCoreCalled = true;
				base.UpdateInitialTabImageCore();
			}

			#region Implementation

			public TestZWorkflowUserControl GetTrackingUserControl
			{
				get { return (TestZWorkflowUserControl)TrackingUserControl; }
			}

			protected internal override ZWorkflowUserControl TrackingUserControl
			{
				get
				{
					if (trackingUserControl == null)
					{
						trackingUserControl = new TestZWorkflowUserControl();
					}
					return trackingUserControl;
				}
			}
			ZWorkflowUserControl trackingUserControl;

			#endregion
		}

		class TestZWorkflowUserControl : ZWorkflowUserControl
		{
			public override void NavigateToWorkflowItem(ProcessTask workflowItem)
			{
				//base.NavigateToWorkflowItem(workflowItem);
				NavigateToWorkflowItemCall = "(ZWorkflowUserControl)TrackingUserControl.NavigateToWorkflowItem(Task) was called";
			}

			public ZString NavigateToWorkflowItemCall;
		}

		class TestWorkflowDescriptor : WorkflowDescriptor
		{
			public bool SupportsEventTrackingExposed;

			public override bool SupportsEventTracking
			{
				get { return SupportsEventTrackingExposed; }
			}

			public override string Code
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override IMultilingualString Description
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override ControllerID ControllerID
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override Type WorkflowProviderType
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion

		#region Implementation

		void FireApplicationIdle()
		{
			Form form = new Form();

			Timer timer = new Timer();
			timer.Tick += delegate
			{ form.Dispose(); };
			timer.Interval = 200;
			timer.Start();

			form.ShowDialog();
			timer.Dispose();
		}

		new DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Dummy);
					form.ControllerID = DummyControllerIDs.Dummy;
				}
				return form;
			}
		}
		ZChildForm form;

		ZTabPage UnrelatedTabPage
		{
			get { return unrelatedTabPage ?? (unrelatedTabPage = new ZTabPage()); }
		}
		ZTabPage unrelatedTabPage;

		TestWorkflowTabPage WorkflowTabPage
		{
			get { return TestTabPage; }
		}

		new TestWorkflowTabPage TestTabPage
		{
			get { return (TestWorkflowTabPage)base.TestTabPage; }
		}

		protected override ZTabPage NewTabPage()
		{
			return new TestWorkflowTabPage();
		}

		protected override ZTabControl NewTabControl()
		{
			return new ZTemplateTabControl();
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(DummyWorkflowDescriptor.Instance);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
