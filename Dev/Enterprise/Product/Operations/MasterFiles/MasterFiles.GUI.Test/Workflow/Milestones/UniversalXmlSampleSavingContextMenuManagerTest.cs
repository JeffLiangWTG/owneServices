using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UniversalXmlSampleSavingContextMenuManagerTest : TestCaseWithFactory
	{
		public void TestCanShowOnBusinessObjectWithUniversalDataSourceAttributeSetup()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_Description = "J00001000";

			var trigger = dummyBO.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			using (var grid = new ZGrid())
			{
				var contextMenuItemsCount = grid.ContextMenu.MenuItems.Count;
				UniversalXmlSampleSavingContextMenuManager.AttachToGrid(grid);
				AssertEquals("manager.CanShow(action)", true, UniversalXmlSampleSavingContextMenuManager.CanShow(action));
				using (var tempDirectory = new TempDirectory())
				{
					DummyWithWorkflowDataContextManager.DefaultOutputDirectoryForTesting = tempDirectory.DirectoryName;

					UniversalXmlSampleSavingContextMenuManager.SaveSample(dummyBO, action);
					AssertEquals("LastMessage.Text", "Please save changes before generating sample XML.", UnitTestUserNotification.Instance.LastMessage.Text);

					Factory.Save();
					UniversalXmlSampleSavingContextMenuManager.SaveSample(dummyBO, action);
					var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertContains("LastMessage.Text", "UniversalEvent from [J00001000] saved", lastMessage);
					var messageLines = lastMessage.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					Assert("messageLines.Length >= 2", messageLines.Length >= 2);
					var filename = messageLines[1];
					AssertContains("filename", tempDirectory.DirectoryName, filename);
					var fileContents = File.ReadAllText(filename);
					AssertContains("fileContents", "<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/", fileContents);
				}
			}
		}

		public void TestMenuItemsGetAdded()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			using (var actionsGrid = new ZGrid())
			{
				var contextMenuItemsCount = actionsGrid.ContextMenu.MenuItems.Count;
				UniversalXmlSampleSavingContextMenuManager.AttachToGrid(actionsGrid);
				AssertEquals("Should be 3 new menu items added.", 3, actionsGrid.ContextMenu.MenuItems.Count - contextMenuItemsCount);
				var saveSampleMenuItem = actionsGrid.ContextMenu.MenuItems.FindByText("Save sample XML (Debug Build Only)");
				AssertNotNull(saveSampleMenuItem);
				var indexOfSaveSampleMenuItem = actionsGrid.ContextMenu.MenuItems.IndexOf(saveSampleMenuItem);
				AssertEquals("MenuItem.Text above [Save sample XML (Debug Build Only)]", "-", actionsGrid.ContextMenu.MenuItems[indexOfSaveSampleMenuItem - 1].Text);
				AssertEquals("MenuItem.Text below [Save sample XML (Debug Build Only)]", "-", actionsGrid.ContextMenu.MenuItems[indexOfSaveSampleMenuItem + 1].Text);
			}
		}

		public void TestWorkFlowTriggersCompletionTriggerActionsForException()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Collection";
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("Z0_Text", 10));
				form.Controls.Add(grid);
				form.Show();

				UniversalXmlSampleSavingContextMenuManager.AttachToGrid(grid);
				grid.ListManager.RemoveAt(0);
				AssertNoExceptionThrown(grid.OnPopup_CallForTesting);
			}
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}
	}
}
