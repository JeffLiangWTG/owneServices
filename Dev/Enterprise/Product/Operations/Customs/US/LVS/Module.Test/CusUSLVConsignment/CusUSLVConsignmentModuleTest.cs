using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVConsignmentModule))]
	public class CusUSLVConsignmentModuleTest : ZModuleBasherTest
	{
		public void TestOverrideProperties()
		{
			using (var module = new CusUSLVConsignmentModule())
			using (var filterControl = module.GetNewFilterControlForGrid())
			{
				CombineAssertions(() =>
				{
					AssertEquals(ControllerIDs.Customs.US.USLowValueEntriesBill, module.GetNewController().ID);
					AssertEquals(ModuleIDs.Customs.US.USLowValueEntriesBill, module.ID);
					AssertEquals(Env.Licence.CoreCustomsModule, module.LicenceCheckPoint);
					AssertType<USConsignmentCombinedFilterBusinessObject>(module.FilterBusinessObject);
					AssertEquals(Env.Security.USLVConsignment, module.SecurityCheckpoint);
					AssertType<CusUSLVConsignmentFilterControl>(filterControl);
				});
			}
		}

		public void TestModuleMenuOptions()
		{
			using (var module = new CusUSLVConsignmentModule())
			{
				Assert(module.AllowView);
				Assert(!module.AllowNew);
				Assert(module.AllowEdit);
				Assert(!module.AllowDelete);
			}
		}

		public void TestViewBill()
		{
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				grid.Select(2);

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);
				viewBillMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 2 view forms", 2, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVConsignmentForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestViewBill_With11ConsignmentsSelected_AnswerYes()
		{
			for (int i = 0; i < 11; i++)
			{
				Factory.NewWithValidTestData<CusUSLVConsignment>();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				viewBillMenuItem.PerformClick();
				AssertEquals("Right message", "You have selected a large number of Consignments, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 11 view forms", 11, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVConsignmentForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestViewBill_With11ConsignmentsSelected_AnswerNo()
		{
			for (int i = 0; i < 11; i++)
			{
				Factory.NewWithValidTestData<CusUSLVConsignment>();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				viewBillMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open no view form", 0, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.ReadOnly));
			}
		}

		public void TestViewLowValueEntries()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance1.CusUSLVConsignments.AddNew();
			clearance1.CusUSLVConsignments.AddNew();
			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance2.CusUSLVConsignments.AddNew();
			clearance2.CusUSLVConsignments.AddNew();
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				grid.Select(2);

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewLowValueEntriesMenuItem = viewMenuItem.MenuItems.FindByText("View Low Value Entries");
				AssertNotNull(viewLowValueEntriesMenuItem);
				viewLowValueEntriesMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 2 view forms", 2, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVClearanceForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestViewLowValueEntries_With11LowValueEntriesSelected_AnswerYes()
		{
			for (int i = 0; i < 11; i++)
			{
				var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				clearance.CusUSLVConsignments.AddNew();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewLowValueEntriesMenuItem = viewMenuItem.MenuItems.FindByText("View Low Value Entries");
				AssertNotNull(viewLowValueEntriesMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				viewLowValueEntriesMenuItem.PerformClick();
				AssertEquals("Right message", "You have selected a large number of Low Value Entries jobs, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 11 view forms", 11, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVClearanceForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestViewLowValueEntries_With11LowValueEntriesSelected_AnswerNo()
		{
			for (int i = 0; i < 11; i++)
			{
				var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				clearance.CusUSLVConsignments.AddNew();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewLowValueEntriesMenuItem = viewMenuItem.MenuItems.FindByText("View Low Value Entries");
				AssertNotNull(viewLowValueEntriesMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				viewLowValueEntriesMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open no view form", 0, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.ReadOnly));
			}
		}

		public void TestEditBill()
		{
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				grid.Select(2);

				var editMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var editBillMenuItem = editMenuItem.MenuItems.FindByText("Edit Bill");
				AssertNotNull(editBillMenuItem);
				editBillMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 2 edit forms", 2, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.Browse));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVConsignmentForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestEditBill_With11ConsignmentsSelected_AnswerYes()
		{
			for (int i = 0; i < 11; i++)
			{
				Factory.NewWithValidTestData<CusUSLVConsignment>();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("Edit Bill");
				AssertNotNull(viewBillMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				viewBillMenuItem.PerformClick();
				AssertEquals("Right message", "You have selected a large number of Consignments, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 11 edit forms", 11, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.Browse));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVConsignmentForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestEditBill_With11ConsignmentsSelected_AnswerNo()
		{
			for (int i = 0; i < 11; i++)
			{
				Factory.NewWithValidTestData<CusUSLVConsignment>();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("Edit Bill");
				AssertNotNull(viewBillMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				viewBillMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open no edit form", 0, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.Browse));
			}
		}

		public void TestEditLowValueEntries()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance1.CusUSLVConsignments.AddNew();
			clearance1.CusUSLVConsignments.AddNew();
			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance2.CusUSLVConsignments.AddNew();
			clearance2.CusUSLVConsignments.AddNew();
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);
				grid.Select(2);

				var editMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var editLowValueEntriesMenuItem = editMenuItem.MenuItems.FindByText("Edit Low Value Entries");
				AssertNotNull(editLowValueEntriesMenuItem);
				editLowValueEntriesMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 2 edit forms", 2, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.Browse));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVClearanceForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestEditLowValueEntries_With11LowValueEntriesSelected_AnswerYes()
		{
			for (int i = 0; i < 11; i++)
			{
				var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				clearance.CusUSLVConsignments.AddNew();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var editMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var editLowValueEntriesMenuItem = editMenuItem.MenuItems.FindByText("Edit Low Value Entries");
				AssertNotNull(editLowValueEntriesMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				editLowValueEntriesMenuItem.PerformClick();
				AssertEquals("Right message", "You have selected a large number of Low Value Entries jobs, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 11 edit forms", 11, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.Browse));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVClearanceForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestEditLowValueEntries_With11LowValueEntriesSelected_AnswerNo()
		{
			for (int i = 0; i < 11; i++)
			{
				var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
				clearance.CusUSLVConsignments.AddNew();
			}
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var editMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Edit");
				var editLowValueEntriesMenuItem = editMenuItem.MenuItems.FindByText("Edit Low Value Entries");
				AssertNotNull(editLowValueEntriesMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				editLowValueEntriesMenuItem.PerformClick();

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open no edit form", 0, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVClearanceForm) && x.DisplayMode == ODisplayMode.Browse));
			}
		}

		public void TestModuleDescription()
		{
			using (var module = new CusUSLVConsignmentModule())
			{
				AssertEquals("Low Value Entries by Bill", module.ID.Description);
			}
		}

		public void TestSendOriginalMessageForLowValueEntries()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance1.CusUSLVConsignments.AddNew();

			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance2.CusUSLVConsignments.AddNew();
			clearance2.CusUSLVConsignments.AddNew();

			clearance1.ULH_EntryFilerCode = "XJ5";
			clearance2.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("Right message", "1 original message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessagesForConsignments()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
			clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
			clearance.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("Right message", "3 original message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessageMenuItem_WhenMultipleConsignmentBillsSelected_ThenDisplayMenuItemConsignments()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance1.CusUSLVConsignments.AddNew();

			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance2.CusUSLVConsignments.AddNew();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendOriginalMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				sendOriginalMessageMenuItem.ShowPopupMenu();
				var sendOriginalMessageConsignmentsMenuItem = sendOriginalMessageMenuItem.MenuItems.FindByText("Consignments");
				var sendOriginalMessageLowValueEntriesMenuItem = sendOriginalMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				AssertEquals("Expected menu item Send Original Message > Consignments to be visible when multiple consignment bills are selected", true, sendOriginalMessageConsignmentsMenuItem.Visible);
				AssertEquals("Expected menu item Send Original Message > Entire Low Value Entries not to be visible when multiple consignment bills are selected", false, sendOriginalMessageLowValueEntriesMenuItem.Visible);
			}
		}

		public void TestSendOriginalMessage_WhenSingleConsignmentBillsSelected_ThenDisplayTwoMenuItems_ConsignmentsAndLowValueEntries()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.CusUSLVConsignments.AddNew();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendOriginalMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendOriginalMessageMenuItem.MenuItems.FindByText("Consignments");
				var sendOriginalMessageLowValueEntriesMenuItem = sendOriginalMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				AssertEquals("Expected menu item Send Original Message > Consignments to be visible when multiple consignment bills are selected", true, sendOriginalMessageConsignmentsMenuItem.Visible);
				AssertEquals("Expected menu item Send Original Message > Entire Low Value Entries to be visible when multiple consignment bills are selected", true, sendOriginalMessageLowValueEntriesMenuItem.Visible);
			}
		}

		public void TestSendOriginalMessages_FailToGenerateEntryNumber_ShouldBeIgnored()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance1.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance2.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;
			clearance2.CusUSLVConsignments.AddNew().ULB_NumberOfPacks = 1;

			clearance2.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageConsignmentsMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageConsignmentsMenuItem.PerformClick();
				AssertEquals("Right message", "2 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessagesForConsignments_WithValidationMessage_AnswerNo()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			consignment1.FirstCusUSLVItemTariff = "2517.10.0015";
			consignment1.FirstCusUSLVItemCountryOfOrigin = "AU";
			consignment1.FirstCusUSLVItemLineValue = 5.0;
			SetUpConsignmentWithValidTestData(consignment1);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 0;
			SetUpConsignmentWithValidTestData(consignment2);

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;
			var item1 = consignment3.CusUSLVItems.AddNew();
			item1.ULI_RN_NKCountryOfOrigin = "SS";
			SetUpConsignmentWithValidTestData(consignment3);

			var consignment4 = clearance.CusUSLVConsignments.AddNew();
			consignment4.ULB_NumberOfPacks = 1;
			var itemPGA = consignment4.CusUSLVItems.AddNew().CusUSLVItemPGAs.AddNew();
			itemPGA.ULP_Agency = "TST";
			itemPGA.ULP_AgencyProgram = "TST";
			itemPGA.ULP_DisclaimReason = "C";
			itemPGA.ULP_Indicator = "C";
			SetUpConsignmentWithValidTestData(consignment4);

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", @"It is likely that your message(s) will be rejected by Customs, as one or more Consignments contain message errors.
Select Yes to send the message(s) despite these errors, No to send all messages without errors or Cancel to return to the Bill menu.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Right message", "1 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessagesForLowValueEntries_WithValidationMessage_AnswerNo()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;
			consignment1.FirstCusUSLVItemTariff = "2517.10.0015";
			consignment1.FirstCusUSLVItemCountryOfOrigin = "AU";
			consignment1.FirstCusUSLVItemLineValue = 5.0;
			SetUpConsignmentWithValidTestData(consignment1);

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 0;
			SetUpConsignmentWithValidTestData(consignment2);

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;
			var item1 = consignment3.CusUSLVItems.AddNew();
			item1.ULI_RN_NKCountryOfOrigin = "SS";
			SetUpConsignmentWithValidTestData(consignment3);

			var consignment4 = clearance.CusUSLVConsignments.AddNew();
			consignment4.ULB_NumberOfPacks = 1;
			var itemPGA = consignment4.CusUSLVItems.AddNew().CusUSLVItemPGAs.AddNew();
			itemPGA.ULP_Agency = "TST";
			itemPGA.ULP_AgencyProgram = "TST";
			itemPGA.ULP_DisclaimReason = "C";
			itemPGA.ULP_Indicator = "C";
			SetUpConsignmentWithValidTestData(consignment4);

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(1);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", @"It is likely that your message(s) will be rejected by Customs, as one or more Consignments contain message errors.
Select Yes to send the message(s) despite these errors, No to send all messages without errors or Cancel to return to the Bill menu.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Right message", "1 original message(s) generated; 3 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendOriginalMessagesForConsignments_WithValidationMessage_AnswerCancel()
		{
			var consignment1 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment1.ULB_NumberOfPacks = 1;
			var consignment2 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment2.ULB_NumberOfPacks = 0;

			var consignment3 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment3.ULB_NumberOfPacks = 1;
			var item1 = consignment3.CusUSLVItems.AddNew();
			item1.ULI_RN_NKCountryOfOrigin = "SS";

			var consignment4 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment4.ULB_NumberOfPacks = 1;
			var itemPGA = consignment4.CusUSLVItems.AddNew().CusUSLVItemPGAs.AddNew();
			itemPGA.ULP_Agency = "TST";
			itemPGA.ULP_AgencyProgram = "TST";
			itemPGA.ULP_DisclaimReason = "C";
			itemPGA.ULP_Indicator = "C";

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText("Consignments");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", @"It is likely that your message(s) will be rejected by Customs, as one or more Consignments contain message errors.
Select Yes to send the message(s) despite these errors, No to send all messages without errors or Cancel to return to the Bill menu.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("0 original message(s) generated; 4 message(s) ignored.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestSendOriginalMessagesForLowValueEntries_WithValidationMessage_AnswerCancel()
		{
			var consignment1 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment1.ULB_NumberOfPacks = 1;

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText("Entire Low Value Entries");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", @"It is likely that your message(s) will be rejected by Customs, as one or more Consignments contain message errors.
Select Yes to send the message(s) despite these errors, No to send all messages without errors or Cancel to return to the Bill menu.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestSendOriginalMessages()
		{
			AssertSendOriginalMessagesForConsignments_ConsignmentWaitingForResponse_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessagesForConsignments_ConsignmentWaitingForResponse_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentIsInactive_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentIsInactive_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentMessageStatusIsCSA_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentMessageStatusIsCSA_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentHasReleaseStatus_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_ConsignmentHasReleaseStatus_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_MessageStatusIsPendingForSendOriginalMessage_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_MessageStatusIsPendingForSendOriginalMessage_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_ClearanceWithMutex_ShouldBeIgnored("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_ClearanceWithMutex_ShouldBeIgnored("Entire Low Value Entries");
			RestartTransaction();
			AssertSendOriginalMessages_WithValidationMessage_AnswerYes("Consignments");
			RestartTransaction();
			AssertSendOriginalMessages_WithValidationMessage_AnswerYes("Entire Low Value Entries");
		}

		public void TestConvertPartyAddressesToNewOrgMenuItem_IsAddedToModuleMenu()
		{
			using (var module = new CusUSLVConsignmentModule())
			{
				var menuItems = module.FormActionMenu;
				var convertPartyAddressesMenuItem = menuItems.FindByText("Convert Party Addresses to Organizations");

				AssertNotNull("Convert Party Addresses to Organizations menu item should exist", convertPartyAddressesMenuItem);
				AssertEquals("Convert Party Addresses to Organizations", convertPartyAddressesMenuItem.Text);
			}
		}

		public void TestConvertPartyAddressesToNewOrgMenuItem_WithFreeTextAddresses()
		{
			var consignment1 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment1.ULB_ConsigneeName = "Test Consignee";
			consignment1.ULB_ConsigneeAddress1 = "123 Test Street";
			consignment1.ULB_ConsigneeCity = "Test City";
			consignment1.ULB_RN_NKConsigneeCountry = "US";
			consignment1.ULB_ConsigneeState = "CA";

			var consignment2 = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment2.ULB_SellerName = "Test Seller";
			consignment2.ULB_SellerAddress1 = "456 Seller Ave";
			consignment2.ULB_SellerCity = "Seller City";
			consignment2.ULB_RN_NKSellerCountry = "US";

			Assert("Precondition: consignee should not be linked to an organization", !consignment1.ConsigneeIsOrganisation);
			Assert("Precondition: seller should not be linked to an organization", !consignment2.ShipperIsOrganisation);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var convertPartyAddressesMenuItem = module.FormActionMenu.FindByText("Convert Party Addresses to Organizations");
				AssertNotNull(convertPartyAddressesMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				using (ZFormModaliser.SuspendDispose())
				{
					convertPartyAddressesMenuItem.PerformClick();

					var freeTextAddressForm = ZFormModaliser.LastFormShownDialogForTest;
					CombineAssertions("Should show FreeTextAddressConversionForm", () =>
					{
						AssertNotNull(freeTextAddressForm);
						AssertType<FreeTextAddressConversionForm<CusUSLVConsignment>>(freeTextAddressForm);
					});
				}
			}
		}

		#region TestSendOriginalMessages

		void AssertSendOriginalMessagesForConsignments_ConsignmentWaitingForResponse_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_NumberOfPacks = 1;
			consignment.ULB_MessageStatus = "AAV";

			Assert("Precondition: ULB_MessageStatus is in list WaitingForResponse", consignment.Lookups.ULB_MessageStatusList.IsWaitingForResponse(consignment.ULB_MessageStatus));

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertSendOriginalMessages_ConsignmentIsInactive_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_NumberOfPacks = 1;
			consignment.ULB_IsActive = false;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				((ZArchitecture.Business.ModuleTextBaseFilter)filterControl.FilterBusinessObject["Active Status"]).Property = "All";
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				AssertEquals("Pre-condition: Inactive consignment is shown and selected", 1, grid.SelectedRowCount);

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Expected inactive consignments to be ignored when sending messages.", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertSendOriginalMessages_ConsignmentMessageStatusIsCSA_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_NumberOfPacks = 1;
			consignment.ULB_MessageStatus = "CSA";

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertSendOriginalMessages_ConsignmentHasReleaseStatus_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ULB_NumberOfPacks = 1;
			consignment.CE_EntryStatus = "HHH";

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertSendOriginalMessages_MessageStatusIsPendingForSendOriginalMessage_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.CusUSLVConsignments.AddNew();

			var originalRequestPendingConsignment = clearance.CusUSLVConsignments.AddNew();
			originalRequestPendingConsignment.ULB_MessageStatus = ImportMessageStatusList.Codes.OriginalRequestPending;

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("No message was sent", "0 original message(s) generated; 2 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", "1 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertSendOriginalMessages_ClearanceWithMutex_ShouldBeIgnored(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.CusUSLVConsignments.AddNew();

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var shipment = ((USConsignmentCombined)grid.SelectedElements[0]).Consignment.Shipment;
				try
				{
					if (shipment.LockSendCustomsMessageMutex())
					{
						var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
						var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

						sendOriginalMessageOptionMenuItem.PerformClick();
						AssertEquals("Right message", "0 original message(s) generated; 1 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					shipment.UnlockSendCustomsMessageMutex();
				}
			}
		}

		void AssertSendOriginalMessages_WithValidationMessage_AnswerYes(string sendOriginalMessageOption)
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();

			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.ULB_NumberOfPacks = 1;

			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.ULB_NumberOfPacks = 0;

			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.ULB_NumberOfPacks = 1;
			var item1 = consignment3.CusUSLVItems.AddNew();
			item1.ULI_RN_NKCountryOfOrigin = "SS";

			var consignment4 = clearance.CusUSLVConsignments.AddNew();
			consignment4.ULB_NumberOfPacks = 1;
			var itemPGA = consignment4.CusUSLVItems.AddNew().CusUSLVItemPGAs.AddNew();
			itemPGA.ULP_Agency = "TST";
			itemPGA.ULP_AgencyProgram = "TST";
			itemPGA.ULP_DisclaimReason = "C";
			itemPGA.ULP_Indicator = "C";

			clearance.ULH_EntryFilerCode = "XJ5";
			var stmNumsSetting = ACEEntryStmNumsSetting.New(GlbBranch.CurrentBranch, "XJ5");
			stmNumsSetting.AddForTesting(GlbBranch.CurrentBranch.PK, 10000, 10010);

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("No message was sent.", "0 original message(s) generated; 4 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();

				var sendMessageMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("Send Original Messages");
				var sendOriginalMessageOptionMenuItem = sendMessageMenuItem.MenuItems.FindByText(sendOriginalMessageOption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				sendOriginalMessageOptionMenuItem.PerformClick();
				AssertEquals("Right message", "4 original message(s) generated; 0 message(s) ignored.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetsCorrectControllerForCusUSLVConsignment()
		{
			Factory.NewWithValidTestData<CusUSLVConsignment>();
			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);
				viewBillMenuItem.PerformClick();

				var lastController = ((IFilterModuleInternalsForTesting)module).LastController;
				AssertType<CusUSLVConsignmentController>("The last controller is made from Low Value Entries Bill", lastController);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 1 Low Value Entries Bill view form", 1, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(CusUSLVConsignmentForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(CusUSLVConsignmentForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestGetsCorrectControllerForJobDeclaration()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			Factory.Save();

			using (var module = new CusUSLVConsignmentModule())
			using (var form = new ZForm())
			{
				var filterControl = (CusUSLVConsignmentFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var viewMenuItem = (ZMenuItem)module.FormActionMenu.FindByText("View");
				var viewBillMenuItem = viewMenuItem.MenuItems.FindByText("View Bill");
				AssertNotNull(viewBillMenuItem);
				viewBillMenuItem.PerformClick();

				var lastController = ((IFilterModuleInternalsForTesting)module).LastController;
				AssertType<CusUSLVDeclarationController>("The last controller is made from Job Declaration", lastController);

				var openForms = ZApplication.GetOpenForms();
				AssertEquals("Should open 1 Job Declaration view form", 1, openForms.OfType<ZForm>().Count(x => x.GetType() == typeof(JobDeclarationForm) && x.DisplayMode == ODisplayMode.ReadOnly));

				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(JobDeclarationForm)))
				{
					openForm.Close();
				}
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USLowValueEntriesBill;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			GetBusinessObjectsToGetControllersFor();
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			Factory.Save();

			return new BusinessObject[] { Factory.Load<USConsignmentCombined>(consignment.PK), Factory.Load<USConsignmentCombined>(jobDeclaration.PK) };
		}

		RefCusTaxOrFee deminimus;

		protected override void SetUp()
		{
			base.SetUp();
			if (deminimus == null)
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				deminimus = helper.CreateTaxOrFee("DEM", 1000m, Core.Constants.CountryCodes.UnitedStates);
				Factory.Save();
			}
		}

		void SetUpConsignmentWithValidTestData(CusUSLVConsignment consignment)
		{
			consignment.ULB_SellerName = "someone";
			consignment.ULB_SellerCity = "somewhere";
			consignment.ULB_SellerAddress1 = "somewhere";
			consignment.ULB_RN_NKSellerCountry = "AU";
			consignment.ULB_ConsigneeName = "Ian";
			consignment.ULB_ConsigneeCity = "syd";
			consignment.ULB_ConsigneeAddress1 = "xx";
			consignment.ULB_RN_NKConsigneeCountry = "AU";
		}

		#endregion
	}
}
