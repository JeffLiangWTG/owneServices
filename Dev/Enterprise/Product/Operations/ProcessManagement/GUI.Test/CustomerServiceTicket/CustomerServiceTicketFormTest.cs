using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using CargoWise.Windows.UI.Testing;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(CustomerServiceTicketForm))]
	class CustomerServiceTicketFormTest : ZFormBasherTest
	{
		public void TestStatusBarTextForClient()
		{
			var cst = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Photocopy the copy of the photo", "XRX");

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(cst))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<CustomerServiceTicketDetailsControl>("DetailsTabControl");
				var clientControl = tabControl.FindSingle<ZGuidFindBox>("ClientFindBox");
				clientControl.Focus();

				Application.DoEvents();

				AssertEquals("The client who raised this Ticket.", form.StatusBarTextForTesting);
			}
		}

		#region Screen Layout

		public void TestCustomFields()
		{
			var template = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template", FallbackTypeList.Codes.AlwaysFallback, "AAA");
			var templateDefinitionString = template.GenCustomColumnDefinitions.AddNew();
			templateDefinitionString.XC_Name = "CustomString";
			templateDefinitionString.XC_Type = AddOnColumnDataType.Codes.String;
			templateDefinitionString.XC_DisplaySequence = 0;
			var templateDefinitionInteger = template.GenCustomColumnDefinitions.AddNew();
			templateDefinitionInteger.XC_Name = "CustomInteger";
			templateDefinitionInteger.XC_Type = AddOnColumnDataType.Codes.Integer;
			templateDefinitionInteger.XC_DisplaySequence = 1;

			Factory.Save();

			var requestWithoutTemplate = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Translate the oeuvre of Proust into Klingon", "BBB");
			using (var form = new CustomerServiceTicketForm(requestWithoutTemplate))
			{
				form.Show();
				Application.DoEvents();

				var customFieldsControl = form.FindAll<CustomPropertiesControl>().Single();
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];

				AssertEquals("No custom controls visible as this CST is not linked to the template", 0, rowLayoutPanel.Controls.Count);
			}

			var requestWithTemplate = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Translate the oeuvre of E.L. James into Dothraki", "AAA");
			using (var form = new CustomerServiceTicketForm(requestWithTemplate))
			{
				form.Show();
				Application.DoEvents();

				var customFieldsControl = form.FindAll<CustomPropertiesControl>().Single();
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];

				AssertEquals("Custom controls should be visible as this CST is linked to the template", 2, rowLayoutPanel.Controls.Count);
				AssertEquals("First control in sequence should be the CustomString field", rowLayoutPanel.Controls[0].GetType(), typeof(ZTextBox));
				AssertEquals("Second control in sequence should be the CustomInteger field", rowLayoutPanel.Controls[1].GetType(), typeof(ZCalcEdit));
			}
		}

		public void TestControlVisibility()
		{
			var template = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template", FallbackTypeList.Codes.AlwaysFallback, "AAA");
			var criterionField = template.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(f => f.ElementName == "SelectionCriterion2DropEdit");
			criterionField.Visible = false;

			Factory.Save();

			var cst = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Cook me eggs", "AAA");

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(cst))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<CustomerServiceTicketDetailsControl>("DetailsTabControl");
				var leftPanel = tabControl.FindSingle<RowLayoutPanel>("LeftTopPanel");
				var midPanel = tabControl.FindSingle<RowLayoutPanel>("MiddleTopPanel");
				var visibilityConfigurationProvider = tabControl.GetVisibilityConfigurationProviderForTest();

				AssertEquals(leftPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(leftPanel));
				AssertEquals(midPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(midPanel));

				var criterion1Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");

				AssertEquals(true, criterion1Control.Visible);
				AssertEquals("Template applied so should NOT be visible", false, criterion2Control.Visible);
			}

			var cst2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Cook me quinoa", "BBB");

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(cst2))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<CustomerServiceTicketDetailsControl>("DetailsTabControl");
				var leftPanel = tabControl.FindSingle<RowLayoutPanel>("LeftTopPanel");
				var midPanel = tabControl.FindSingle<RowLayoutPanel>("MiddleTopPanel");
				var visibilityConfigurationProvider = tabControl.GetVisibilityConfigurationProviderForTest();

				AssertEquals(leftPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(leftPanel));
				AssertEquals(midPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(midPanel));

				var criterion1Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");

				AssertEquals(true, criterion1Control.Visible);
				AssertEquals("Template not applied so should be visible", true, criterion2Control.Visible);
			}
		}

		public void TestControlDisplayOrder()
		{
			var template = ProcessMgmtTestHelper.CreateWorkRequestWorkflowTemplate(Factory, "template", FallbackTypeList.Codes.AlwaysFallback, "AAA");
			var criterion1Field = template.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(f => f.ElementName == "SelectionCriterion1DropEdit");
			var criterion2Field = template.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().Single(f => f.ElementName == "SelectionCriterion2DropEdit");

			AssertEquals(0, criterion1Field.RowNumber);
			AssertEquals(1, criterion2Field.RowNumber);

			criterion1Field.RowNumber = 1;
			criterion2Field.RowNumber = 0;

			Factory.Save();

			var cst = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Cook me kale", "AAA");

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(cst))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<CustomerServiceTicketDetailsControl>("DetailsTabControl");
				var leftPanel = tabControl.FindSingle<RowLayoutPanel>("LeftTopPanel");
				var midPanel = tabControl.FindSingle<RowLayoutPanel>("MiddleTopPanel");
				var visibilityConfigurationProvider = tabControl.GetVisibilityConfigurationProviderForTest();

				AssertEquals(leftPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(leftPanel));
				AssertEquals(midPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(midPanel));

				var criterion1Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");

				AssertGreaterThan("Template has been applied, therefore the ordering of selection criteria controls should have switched", criterion1Control.Top, criterion2Control.Top);
			}

			var cst2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Cook me croquembouche", "BBB");

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(cst2))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<CustomerServiceTicketDetailsControl>("DetailsTabControl");
				var leftPanel = tabControl.FindSingle<RowLayoutPanel>("LeftTopPanel");
				var midPanel = tabControl.FindSingle<RowLayoutPanel>("MiddleTopPanel");
				var visibilityConfigurationProvider = tabControl.GetVisibilityConfigurationProviderForTest();

				AssertEquals(leftPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(leftPanel));
				AssertEquals(midPanel.Name, true, visibilityConfigurationProvider.GetIsVisibilityConfigured(midPanel));

				var criterion1Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2Control = leftPanel.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");

				AssertGreaterThan("Template has NOT been applied, therefore the ordering of selection criteria controls should NOT be switched", criterion2Control.Top, criterion1Control.Top);
			}
		}

		#endregion

		#region ZForm Concerns

		public void TestFormCaption()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			Factory.Save();

			using (var form = new CustomerServiceTicketForm(request))
			{
				AssertEquals("Customer Service Ticket - CST00000001 - Pliz halp", form.FormCaption);
			}

			using (var form = new CustomerServiceTicketForm(null))
			{
				AssertEquals("Customer Service Ticket", form.FormCaption);
			}
		}

		#endregion

		#region Related Items

		public void TestHasRelatedItemsTab()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			using (var form = new CustomerServiceTicketForm(workRequest))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var relatedItemsTabPage = tabControl.GetTabPage("RelatedItemsTabPage");

				AssertNotNull("Related Items should be enabled on the Customer Service Ticket form. SAD!", relatedItemsTabPage);

				var tabNames = tabControl.AllTabPages.Where(x => x.Text == "Workflow && Tracking" || x.Text == "Related Items").Select(x => x.Text);
				AssertSequencesEqual("The Related Items tab should come just after the Workflow tab. SAD!", new[] { "Workflow && Tracking", "Related Items" }, tabNames);

				var workflowTabPage = tabControl.GetTabPage("WorkflowTabPage");
				AssertEquals("The Related Items tab should come after the Workflow tab. SAD!", tabControl.TabPages.IndexOf(workflowTabPage) + 1, tabControl.TabPages.IndexOf(relatedItemsTabPage));
			}
		}

		public void TestRelatedItemsTab_AddNew_ShouldCreateAndAttachNewWorkItem()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory);
			workRequest.WKR_OC_Client = contact.PK;

			Factory.Save();

			var allLinks = Factory.Load<WorkItemRequestLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("There shouldn't be any work items attached by default. SAD!", Array.Empty<WorkItemRequestLink>(), allLinks);

			var workItemPk = ZGuid.Empty;

			using (var form = new CustomerServiceTicketForm(workRequest))
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

				var newButton = control.FindSingle<ZButton>("NewButton");
				newButton.PerformClick();
				Application.DoEvents();

				var contextMenu = control.MenuStripNew_ForTest;
				AssertEquals("There should be only one option to select. SAD!", 1, contextMenu.Items.Count);

				var menuItem = contextMenu.Items[0];
				AssertEquals("The single menu item should be for Work Item. SAD!", "Work Item", menuItem.Text);

				WorkItemForm workItemForm = null;

				try
				{
					menuItem.PerformClick();
					Application.DoEvents();

					workItemForm = Application.OpenForms.OfType<WorkItemForm>().Single();
					var workItem = (WorkItem)workItemForm.BusinessEntity;
					workItem.WKI_Summary = "Mr Sneezy 3000";

					workItemPk = workItem.PK;

					workItemForm.FireSaveButton();
					Application.DoEvents();
				}
				finally
				{
					workItemForm?.Dispose();
				}

				var items = GetItemsFromGrid();
				AssertContainsExactElementsInAnyOrder("The work item should now be attached and shown in the grid. SAD!", new[] { "Mr Sneezy 3000" }, items.Select(x => x.ItemDescription));

				form.FireSaveButton();
				Application.DoEvents();
			}

			var link = Factory.Load<WorkItemRequestLink>(new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, workItemPk)).SingleOrDefault();
			AssertNotNull("The work item and work request should now be linked. SAD!", link);
			AssertEquals("The work item and work request should now be linked. SAD!", workRequest.PK, link.WKL_WKR_Request);

			using (var form = new CustomerServiceTicketForm(workRequest))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var control = tabControl.SelectedTab.FindSingle<WorkTaskRelatedItemUserControl>();
				var grid = control.FindSingle<ZGrid>("RelatedItemGrid");
				AssertContainsExactElementsInAnyOrder("The work item should still be attached and shown in the grid. SAD!", new[] { "Mr Sneezy 3000" }, grid.ListManager.List.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));
			}
		}

		public void TestRelatedItemsTab_AttachWorkItem()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory);
			workRequest.WKR_OC_Client = contact.PK;

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "ABC";
			workItem.WKI_Summary = "Mr Sneezy 3000";

			Factory.Save();

			var allLinks = Factory.Load<WorkItemRequestLink>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("The work item and work request should not be linked by default. SAD!", Array.Empty<WorkItemRequestLink>(), allLinks);

			using (var form = new CustomerServiceTicketForm(workRequest))
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
				AssertEquals("There should be only one option to select. SAD!", 1, contextMenu.Items.Count);

				var menuItem = contextMenu.Items[0];
				AssertEquals("The single menu item should be for Work Item. SAD!", "Work Item", menuItem.Text);

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
				AssertContainsExactElementsInAnyOrder("The work item should now be attached and shown in the grid. SAD!", new[] { "Mr Sneezy 3000" }, items.Select(x => x.ItemDescription));

				form.FireSaveButton();
				Application.DoEvents();
			}

			var link = Factory.Load<WorkItemRequestLink>(new ZQuery(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, workItem.PK)).SingleOrDefault();
			AssertNotNull("The work item and work request should now be linked. SAD!", link);
			AssertEquals("The work item and work request should now be linked. SAD!", workRequest.PK, link.WKL_WKR_Request);

			using (var form = new CustomerServiceTicketForm(workRequest))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				tabControl.SelectTab("RelatedItemsTabPage");
				Application.DoEvents();

				var control = tabControl.SelectedTab.FindSingle<WorkTaskRelatedItemUserControl>();
				var grid = control.FindSingle<ZGrid>("RelatedItemGrid");
				AssertContainsExactElementsInAnyOrder("The work item should still be attached and shown in the grid. SAD!", new[] { "Mr Sneezy 3000" }, grid.ListManager.List.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));

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

				AssertEquals("1AA - 1AA depth 1", grid[0, selectionCriteriaColumns[0]]);
				AssertEquals("2AA - 2AA depth 2", grid[0, selectionCriteriaColumns[1]]);
				AssertEquals("3AA - 3AA depth 3", grid[0, selectionCriteriaColumns[2]]);
				AssertEquals("4AA - 4AA depth 4", grid[0, selectionCriteriaColumns[3]]);
				AssertEquals("ABC", grid[0, selectionCriteriaColumns[4]]);
			}
		}

		#endregion

		#region Workflow

		public void TestHasWorkflowTab()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Cater to my every whim");
			var task = request.WorkflowItems.Tasks.AddNew();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.CustomerServiceTicket).ShowFormForNewEntity(request))
			{
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task);
				Application.DoEvents();

				AssertEquals("Workflow && Tracking", form.FindSingle<ZTemplateTabControl>("MainTabControl").SelectedTab.Text);
			}
		}

		#endregion

		#region Selection Criteria

		public void TestSelectionCriteriaFields_DefaultValues()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Iron my shirts");

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.CustomerServiceTicket).ShowFormForNewEntity(request))
			{
				var criterion1 = form.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2 = form.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");
				var criterion3 = form.FindSingle<ZDropEdit>("SelectionCriterion3DropEdit");
				var criterion4 = form.FindSingle<ZDropEdit>("SelectionCriterion4DropEdit");
				var criterion5 = form.FindSingle<ZDropEdit>("SelectionCriterion5DropEdit");

				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("Selection Criterion 1", criterion1.CaptionResourceString.Caption);
					AssertEquals("The first criterion for deciding which Workflow Template to use for this Customer Service Ticket.", criterion1.CaptionResourceString.FullDescription);

					AssertEquals("Selection Criterion 2", criterion2.CaptionResourceString.Caption);
					AssertEquals("The second criterion for deciding which Workflow Template to use for this Customer Service Ticket.", criterion2.CaptionResourceString.FullDescription);

					AssertEquals("Selection Criterion 3", criterion3.CaptionResourceString.Caption);
					AssertEquals("The third criterion for deciding which Workflow Template to use for this Customer Service Ticket.", criterion3.CaptionResourceString.FullDescription);

					AssertEquals("Selection Criterion 4", criterion4.CaptionResourceString.Caption);
					AssertEquals("The fourth criterion for deciding which Workflow Template to use for this Customer Service Ticket.", criterion4.CaptionResourceString.FullDescription);

					AssertEquals("Selection Criterion 5", criterion5.CaptionResourceString.Caption);
					AssertEquals("The fifth criterion for deciding which Workflow Template to use for this Customer Service Ticket.", criterion5.CaptionResourceString.FullDescription);
				});
			}
		}

		public void TestSelectionCriteriaFields_OverriddenValues()
		{
			ProcessManagementRegistry.Instance.SelectionCriterion1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 1");
			ProcessManagementRegistry.Instance.SelectionCriterion2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 2");
			ProcessManagementRegistry.Instance.SelectionCriterion3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 3");
			ProcessManagementRegistry.Instance.SelectionCriterion4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 4");
			ProcessManagementRegistry.Instance.SelectionCriterion5Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 5");

			ProcessManagementRegistry.Instance.SelectionCriterion1Description.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Number 1 is what we are, yes indeedy.");
			ProcessManagementRegistry.Instance.SelectionCriterion2Description.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Number 2 is what we are, fo shizzle.");
			ProcessManagementRegistry.Instance.SelectionCriterion3Description.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Number 3 is what we are, indubitably.");
			ProcessManagementRegistry.Instance.SelectionCriterion4Description.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Number 4 is what we are, no doubt about it.");
			ProcessManagementRegistry.Instance.SelectionCriterion5Description.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Number 5 is what we are, trufax.");

			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Plough my fields");

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.CustomerServiceTicket).ShowFormForNewEntity(request))
			{
				var criterion1 = form.FindSingle<ZDropEdit>("SelectionCriterion1DropEdit");
				var criterion2 = form.FindSingle<ZDropEdit>("SelectionCriterion2DropEdit");
				var criterion3 = form.FindSingle<ZDropEdit>("SelectionCriterion3DropEdit");
				var criterion4 = form.FindSingle<ZDropEdit>("SelectionCriterion4DropEdit");
				var criterion5 = form.FindSingle<ZDropEdit>("SelectionCriterion5DropEdit");

				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("We are number 1", criterion1.CaptionResourceString.Caption);
					AssertEquals("Number 1 is what we are, yes indeedy.", criterion1.CaptionResourceString.FullDescription);

					AssertEquals("We are number 2", criterion2.CaptionResourceString.Caption);
					AssertEquals("Number 2 is what we are, fo shizzle.", criterion2.CaptionResourceString.FullDescription);

					AssertEquals("We are number 3", criterion3.CaptionResourceString.Caption);
					AssertEquals("Number 3 is what we are, indubitably.", criterion3.CaptionResourceString.FullDescription);

					AssertEquals("We are number 4", criterion4.CaptionResourceString.Caption);
					AssertEquals("Number 4 is what we are, no doubt about it.", criterion4.CaptionResourceString.FullDescription);

					AssertEquals("We are number 5", criterion5.CaptionResourceString.Caption);
					AssertEquals("Number 5 is what we are, trufax.", criterion5.CaptionResourceString.FullDescription);
				});
			}
		}

		#endregion

		#region Organisation

		public void TestOrganisation()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "WorkRequest 1");

			using (var form = new CustomerServiceTicketForm(workRequest))
			{
				form.Show();
				Application.DoEvents();

				var organisationFindBox = form.FindSingleOrDefault<ZGuidFindBox>("OrganisationFindBox");

				CombineAssertions("Organisation FindBox should exist", () =>
				{
					AssertNotNull("FindBox", organisationFindBox);
					AssertEquals("Binding Member", "OrganisationPK", organisationFindBox.GetBindingMember());
				});
			}
		}

		public void TestClientField_WhenEmpty_AndF3Pressed_ShouldNotShowSecurityError()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory);
			Factory.Save();

			var ticket = Factory.New<WorkRequest>();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				var organizationFindBox = form.FindSingleOrDefault<ZGuidFindBox>("OrganisationFindBox");
				var clientFindBox = form.FindSingle<ZGuidFindBox>("ClientFindBox");

				AssertEquals(string.Empty, organizationFindBox.CodeBox.Text);
				AssertEquals(string.Empty, clientFindBox.CodeBox.Text);

				((IFindBoxUserControl)clientFindBox).ShowEditOrViewForm();
				Application.DoEvents();

				AssertEquals("Please select an Organization first.", UnitTestUserNotification.Instance.LastMessage.Text.Replace("Organisation", "Organization"));
				AssertNull("No form should have been shown because we don't know which organization to add a new contact to.", ZFormModaliser.LastFormShownForTest);
				UnitTestUserNotification.Instance.ClearMessages();

				ticket.OrganisationPK = contact.Header.PK;
				Application.DoEvents();

				AssertEquals(contact.Header.OH_Code, organizationFindBox.CodeBox.Text);
				AssertEquals(string.Empty, clientFindBox.CodeBox.Text);

				AssertFormOpenedWhenF3Pressed();

				ticket.WKR_OC_Client = contact.PK;
				AssertEquals(contact.Name.ToUpper(), clientFindBox.CodeBox.Text.ToUpper());

				AssertFormOpenedWhenF3Pressed();

				void AssertFormOpenedWhenF3Pressed()
				{
					ZOrganisationsForm orgForm = null;

					try
					{
						((IFindBoxUserControl)clientFindBox).ShowEditOrViewForm();
						Application.DoEvents();

						AssertNull("Pressing F3 should not show an error message, even when the Organization is selected.", UnitTestUserNotification.Instance.LastMessage.Text);

						orgForm = Application.OpenForms.ToList<ZOrganisationsForm>().SingleOrDefault();
						AssertNotNull("The form for the specified organization should have been opened.", orgForm);
						AssertEquals("The system should navigate to the Contacts tab automatically.", "Contact", orgForm.OrganisationsTabControl.SelectedTab.Text);
					}
					finally
					{
						orgForm?.Dispose();
					}
				}
			}
		}

		public void TestClientField_WhenInvalid_AndF3Pressed_ShouldShowAppropriateErrorMessage()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory);
			Factory.Save();

			var ticket = Factory.New<WorkRequest>();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				var organizationFindBox = form.FindSingleOrDefault<ZGuidFindBox>("OrganisationFindBox");
				var clientFindBox = form.FindSingle<ZGuidFindBox>("ClientFindBox");

				ticket.OrganisationPK = contact.Header.PK;
				Application.DoEvents();

				ticket.WKR_OC_Client = contact.PK;
				clientFindBox.CodeBox.Text += "S";
				AssertEquals("JAN MICHAEL VINCENTS", clientFindBox.CodeBox.Text.ToUpper());

				ZOrganisationsForm orgForm = null;

				try
				{
					((IFindBoxUserControl)clientFindBox).ShowEditOrViewForm();
					Application.DoEvents();

					AssertEquals("No contact [JAN MICHAEL VINCENTS] exists for the specified Organization.", UnitTestUserNotification.Instance.LastMessage.Text.Replace("Organisation", "Organization"));

					orgForm = Application.OpenForms.ToList<ZOrganisationsForm>().SingleOrDefault();
					AssertNull("The form should not have been opened because the value was invalid.", orgForm);
				}
				finally
				{
					orgForm?.Dispose();
				}
			}
		}

		#endregion

		#region Plugins

		#region eConversation

		public void TestEConversationControl_ShouldHaveInternalNoteButton()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "I wanna run in a stream!");

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<EConversationFullControl>();
				var addInternalNoteButton = control.FindSingle<ZButton>("AddInternalCommentButton");

				AssertEquals("The button should be visible on this form, even though it's not visible by default. SAD!", true, addInternalNoteButton.Visible);
			}
		}

		public void TestEConversationControl_ShouldNotShowParticipantPanel()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Here's another quick mystery.");

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<EConversationFullControl>();
				AssertEquals("The participation panel should not be shown on this form. SAD!", EConversationViewMode.ShowOnlyEConversation, control.ViewMode);

				var splitContainer = control.FindSingle<SplitContainer>("MainSplitContainer");
				AssertEquals("No, seriously, we don't want the participation panel. SAD!", true, splitContainer.Panel2Collapsed);
			}
		}

		public void TestNewRequest_ShouldDisableEConversationControls()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "There are some very fine controls on both sides.");

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindSingle<EConversationFullControl>();
				var textbox = control.FindSingle<ZAutoCompleteTextBox>("econversationMessageTextBox");
				AssertEquals(true, textbox.ReadOnly);
				AssertEquals("Please save the form before using eConversation.", textbox.Text);

				foreach (var button in control.FindAll<ZButton>())
				{
					AssertEquals(true, button.ReadOnly);
				}

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(false, textbox.ReadOnly);
				AssertEquals(string.Empty, textbox.Text);

				textbox.Text = "Text to enable button controls";

				foreach (var button in control.FindAll<ZButton>())
				{
					if (button.Visible)
					{
						AssertEquals(false, button.ReadOnly);
					}
				}
			}
		}

		public void TestFormSetToMinimumSize_ShouldNotHideEConversationButtons()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "There are some very fine controls on both sides.");

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Size = form.MinimumSize;
				form.Show();
				Application.DoEvents();

				Control button = form.FindSingle<KRadioButton>("allMessagesRadioButton");
				var bottom = ControlTestHelper.GetControlAbsoluteBottom(button, form);
				var right = ControlTestHelper.GetControlAbsoluteRight(button, form);

				AssertLessThan("The buttons should not be off the visible range of the form, even when the form is at its smallest. SAD!", bottom, form.Height);
				AssertLessThan("The buttons should not be off the visible range of the form, even when the form is at its smallest. SAD!", right, form.Width);

				button = form.FindSingle<ZButton>("sendButton");
				right = ControlTestHelper.GetControlAbsoluteRight(button, form);
				AssertLessThan("The buttons should not be off the visible range of the form, even when the form is at its smallest. SAD!", right, form.Width);
			}
		}

		public void TestEConversationTab_ShouldContainParticipantManagementOnly()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			Factory.Save();

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>();
				AssertCollectionContains("eConversation Participants", tabControl.TabPages.Cast<ZTabPage>().Select(tab => tab.Text));

				var eConvoTab = tabControl.TabPages.Cast<ZTabPage>().Single(tab => tab.Text == "eConversation Participants");

				tabControl.SelectedTab = eConvoTab;
				Application.DoEvents();

				var eConvoControl = eConvoTab.FindSingle<EConversationFullControl>();
				var splitContainer = eConvoControl.FindSingle<SplitContainer>("MainSplitContainer");

				AssertEquals(EConversationViewMode.ShowOnlyParticipants, eConvoControl.ViewMode);
				AssertEquals("We just want the participation panel.", true, splitContainer.Panel1Collapsed);
				AssertEquals("We just want the participation panel.", false, splitContainer.Panel2Collapsed);
			}
		}

		public void TestAddConversationMessageAndSave_ShouldSendNotificationEmailsToParticipants()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "cows@tv.com");
			var resource1 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "horses@tv.com");
			var resource2 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "cats@tv.com");

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>();
				var eConvoTab = tabControl.TabPages.Cast<ZTabPage>().Single(tab => tab.Text == "eConversation Participants");

				tabControl.SelectedTab = eConvoTab; // Force the EConversationPlugin to be instantiated and be tracked in the factory.
				Application.DoEvents();

				ticket.Conversation.Participants.AddNewParticipant(resource1);
				ticket.Conversation.Participants.AddNewParticipant(resource2);

				form.FireSaveButton();
				Application.DoEvents();

				ticket.Conversation.AddMessageFromCurrentUser("Cows don't look like cows on TV. You gotta use horses.", isInternal: false);
				Application.DoEvents();

				AssertEquals(0, Env.AllEmailsCreated.Count());

				form.FireSaveButton();
				Application.DoEvents();

				var emails = Env.AllEmailsCreated.ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { "cows@tv.com", "horses@tv.com", "cats@tv.com" }, emails.Select(email => email.Recipients.Cast<RecipientDef>().Single()).Select(r => r.Email));

				const string bodyWithHtmlEscapedChars = "Cows don&#39;t look like cows on TV. You gotta use horses.";

				AssertContains(bodyWithHtmlEscapedChars, emails[0].Body);
				AssertContains(bodyWithHtmlEscapedChars, emails[1].Body);
				AssertContains(bodyWithHtmlEscapedChars, emails[2].Body);
			}
		}

		public void TestAddConversationMessage_ShouldRegisterClientAsParticipant()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "CW@hunrath.com");
			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				ticket.Conversation.AddMessageFromCurrentUser("Them Kaptar Critters", isInternal: false);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(1, Env.AllEmailsCreated.Count());
				AssertContains("Them Kaptar Critters", Env.AllEmailsCreated.Single().Body);
				AssertEquals("CW@hunrath.com", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

				Env.ClearAllEmailsCreated();

				var otherContact = ticket.Client.ParentOrg.Contacts.AddNew();
				otherContact.FillWithValidTestData();
				otherContact.OC_Email = "farley@hunrath.com";

				ticket.WKR_OC_Client = otherContact.PK;
				Application.DoEvents();

				ticket.Conversation.AddMessageFromCurrentUser("We all lost everything", isInternal: false);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals(1, Env.AllEmailsCreated.Count());
				AssertContains("We all lost everything", Env.AllEmailsCreated.Single().Body);
				AssertEquals("farley@hunrath.com", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

				Env.ClearAllEmailsCreated();

				ticket.WKR_OC_Client = ZGuid.Empty;
				Application.DoEvents();

				ticket.Conversation.AddMessageFromCurrentUser("Shorah", isInternal: false);
				AssertExceptionThrown<ZSaveException>(Factory.Save);
				AssertEquals(0, Env.AllEmailsCreated.Count());
			}
		}

		public void TestAddExternalConversationMessage_WhenNoParticipants_ShouldUseRegistryFallback()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var notificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "AntsInMyEyes@johnson.com", groups: new[] { notificationGroup });

			Factory.Save();
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Gazorpazorpfield@Gazorpazorp.com");

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				ticket.Conversation.AddMessageFromCurrentUser("I hope our prices aren't too low!", isInternal: false);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				Env.ClearAllEmailsCreated();

				ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Mmmm enchiladas!");
				Application.DoEvents();

				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();

				AssertContainsExactElementsInAnyOrder("For now we are also sending a notification to the client when they write their own messages. This will likely change. The important thing here is we DO fall back to the notification group because we have no participants.",
					new[] { "AntsInMyEyes@johnson.com", "Gazorpazorpfield@Gazorpazorp.com" }, Env.AllEmailsCreated.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals(2, Env.AllEmailsCreated.Count());
			}
		}

		public void TestAddExternalConversationMessage_WhenParticipantExists_ShouldNotUseRegistryFallback()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var notificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var notificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "AntsInMyEyes@johnson.com", groups: new[] { notificationGroup });
			var participantStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "Jon@Gazorpazorp.com");

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Gazorpazorpfield@Gazorpian.com");

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				ticket.Conversation.Participants.AddNewParticipant(participantStaff);
				ticket.Conversation.AddMessageFromCurrentUser("I hope our prices aren't too low!", isInternal: false);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				Env.ClearAllEmailsCreated();

				ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Mmmm enchiladas!");
				Application.DoEvents();

				AssertEquals(0, Env.AllEmailsCreated.Count());

				MasterFilesTestHelper.RunLogWalker();

				AssertContainsExactElementsInAnyOrder("For now we are also sending a notification to the client when they write their own messages. This will likely change. The important thing here is we DON'T fall back to the notification group because we have a participant.",
					new[] { "Jon@Gazorpazorp.com", "Gazorpazorpfield@Gazorpian.com" }, Env.AllEmailsCreated.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals(2, Env.AllEmailsCreated.Count());
			}
		}

		public void TestConversationMessageNotification_ShouldUseExternalLinksForContacts_AndInternalLinksForStaff()
		{
			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "sleep@data.com");
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				ticket.Conversation.Participants.AddNewParticipant(staff);
				ticket.Conversation.AddMessageFromCurrentUser("Sleep... Data.", isInternal: false);
				Application.DoEvents();

				form.FireSaveButton();
				Application.DoEvents();

				var emails = Env.AllEmailsCreated.ToArray();

				AssertEquals(2, emails.Length);

				var emailToStaff = emails.Single(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == "sleep@data.com"));
				var emailToContact = emails.Single(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == ticket.Client.OC_Email));

				var expectedInternalUrl = FormattableString.Invariant($"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=CustomerServiceTicket&BusinessEntityPK={ticket.PK}");
				var expectedExternalUrl = FormattableString.Invariant($"{GlowRegistry.Instance.GlowPortalsUri.Value}TKT/Desktop#/formFlow/c02c315d-c12a-4abb-aec7-e8d3b57a2dba/{ticket.PK}");

				AssertContains(expectedInternalUrl, emailToStaff.Body);
				AssertContains(expectedExternalUrl, emailToContact.Body);

				AssertNotContains(expectedExternalUrl, emailToStaff.Body);
				AssertNotContains(expectedInternalUrl, emailToContact.Body);
			}
		}

		#endregion

		#region JobInvoicing

		public void TestJobInvoicingTabExists()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			Factory.Save();

			using (var form = new CustomerServiceTicketForm(request))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTemplateTabControl>();
				AssertCollectionContains("Billing", tabControl.TabPages.Cast<ZTabPage>().Select(tab => tab.Text));
			}
		}

		#endregion

		#endregion

		#region Menu Items

		public void TestActionsMenu_ForCancelledTicket_ShouldContainUnCancelMenuItem()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ticket.Cancel(shouldCancelAttachedWorkItems: false);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				AssertNotNull(form.FindMenuItem_ForTest("Un-Cancel Ticket"));
				AssertNull(form.FindMenuItem_ForTest("Cancel Ticket"));
			}
		}

		public void TestActionsMenu_ForOpenTicket_ShouldContainCancelMenuItem()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				AssertNotNull(form.FindMenuItem_ForTest("Cancel Ticket"));
				AssertNull(form.FindMenuItem_ForTest("Un-Cancel Ticket"));
			}
		}

		public void TestActionsMenu_CancelMenuItem()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Last();

				AssertEquals("Cancel Ticket", cancelMenuItem.Text);
				AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

				cancelMenuItem.PerformClick();

				AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
				AssertEquals(TicketStatusList.Codes.Cancelled, ticketTask.P9_Status);

				AssertNull("No work items were attached, so no need to display any notifications.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}
		}

		public void TestActionsMenu_CancelMenuItem_WhenIncompleteWorkItemAttached_ShouldPromptUser_AnsweringYes()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Last();

				AssertEquals("Cancel Ticket", cancelMenuItem.Text);
				AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelMenuItem.PerformClick();

				AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItemTask.P9_Status);

				AssertEquals("There are incomplete Work Items attached to this ticket. Should these also be canceled?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancel incomplete Work Items?", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}
		}

		public void TestActionsMenu_CancelMenuItem_WhenIncompleteWorkItemAttached_ShouldPromptUser_AnsweringNo()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Last();

				AssertEquals("Cancel Ticket", cancelMenuItem.Text);
				AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				cancelMenuItem.PerformClick();

				AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItemTask.P9_Status);

				AssertEquals("There are incomplete Work Items attached to this ticket. Should these also be canceled?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancel incomplete Work Items?", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}
		}

		public void TestActionsMenu_CancelMenuItem_WhenIncompleteWorkItemAttached_ShouldPromptUser_AnsweringCancel()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Last();

				AssertEquals("Cancel Ticket", cancelMenuItem.Text);
				AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				cancelMenuItem.PerformClick();

				AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, ticketTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItemTask.P9_Status);

				AssertEquals("There are incomplete Work Items attached to this ticket. Should these also be canceled?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cancel incomplete Work Items?", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
			}
		}

		public void TestActionsMenu_CancelMenuItem_WhenAlreadyCompleteWorkItemAttached_ShouldNotPromptUser()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem, status: ProcessTaskStatusCodeList.Codes.Closed);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Last();

				AssertEquals("Cancel Ticket", cancelMenuItem.Text);
				AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

				cancelMenuItem.PerformClick();

				AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItemTask.P9_Status);

				AssertNull("No incomplete work items were attached, so no need to display any notifications.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionsMenu_UnCancelMenuItem_ShouldUpdateStatus()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ticket.WKR_Status = TicketStatusList.Codes.Cancelled;

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var unCancelMenuItem = form.FindMenuItem_ForTest("Un-Cancel Ticket");

				AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

				unCancelMenuItem.PerformClick();
				AssertEquals("Selecting the Un-Cancel menu item should force re-calculation of the status to CLS.", TicketStatusList.Codes.Closed, ticket.WKR_Status);
				AssertNull("Nothing to prompt the user about.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionsMenu_WhenOpenedMultipleTimes_ForNonCancelledTicket_ShouldNotDuplicateMenuItem()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenu = form.FindMenuItem_ForTest("Actions");

				actionsMenu.OnPopup(EventArgs.Empty);

				var initialCount = actionsMenu.MenuItems.Count;

				actionsMenu.OnPopup(EventArgs.Empty);
				actionsMenu.OnPopup(EventArgs.Empty);
				actionsMenu.OnPopup(EventArgs.Empty);

				AssertEquals(initialCount, actionsMenu.MenuItems.Count);
			}
		}

		public void TestActionsMenu_WhenOpenedMultipleTimes_ForCancelledTicket_ShouldNotDuplicateMenuItem()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ticket.Cancel(shouldCancelAttachedWorkItems: false);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenu = form.FindMenuItem_ForTest("Actions");

				actionsMenu.OnPopup(EventArgs.Empty);

				var initialCount = actionsMenu.MenuItems.Count;

				actionsMenu.OnPopup(EventArgs.Empty);
				actionsMenu.OnPopup(EventArgs.Empty);
				actionsMenu.OnPopup(EventArgs.Empty);

				AssertEquals(initialCount, actionsMenu.MenuItems.Count);
			}
		}

		void TestCancelTicketWithCopySchedulesUsingScheduleDeactivatorForm(string buttonToClick, string ticketFinalStatus, bool copyScheduleActiveStatus)
		{
			var ticket = Factory.NewWithValidTestData<WorkRequest>();
			ticket.WorkflowItems.AddNew();
			ticket.WKR_Status = ProcessTaskStatusCodeList.Codes.Working;

			var ticket_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var ticket_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			ticket_copySchedule.SUC_CopyObjectId = ticket.PK;
			ticket_copyScheduleTask.S5_ParentID = ticket_copySchedule.PK;

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				form.FindMenuItem_ForTest("Actions").OnPopup(EventArgs.Empty);

				var actionsMenuItem = form.FindMenuItem_ForTest("Actions");
				var cancelMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().FindByText("Cancel Ticket");

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

				AssertEquals(ticketFinalStatus, ticket.WKR_Status);
				AssertEquals(copyScheduleActiveStatus, ticket_copyScheduleTask.S5_IsActive);
			}
		}

		public void TestCancelTicketWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelTicketWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, false);
		}

		public void TestCancelTicketWithCopySchedules_WhenUserSelectsCancelAndDoNotDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelTicketWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and do not deactivate", ProcessTaskStatusCodeList.Codes.Cancelled, true);
		}

		public void TestDoNotCancelTicketWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelTicketWithCopySchedulesUsingScheduleDeactivatorForm("Do not cancel or deactivate", ProcessTaskStatusCodeList.Codes.Working, true);
		}

		public void TestJobInvoicingMenu_ShouldExist()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull("We should have a Job Invoicing menu item, but instead...", form.FindMenuItem_ForTest("Job Invoicing"));
			}
		}

		public void TestDocumentsMenu_ShouldExist()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			using (var form = new CustomerServiceTicketForm(ticket))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull("We should have a Documents menu item. SAD!", form.FindMenuItem_ForTest("Documents"));
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			Factory.Save();

			return new CustomerServiceTicketForm(request) { ControllerID = ControllerIDs.CustomerServiceTicket, Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000) };
		}

		#endregion
	}
}
