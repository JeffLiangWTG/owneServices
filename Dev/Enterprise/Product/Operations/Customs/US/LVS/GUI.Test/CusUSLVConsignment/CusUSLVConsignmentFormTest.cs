using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(CusUSLVConsignmentForm))]
	public class CusUSLVConsignmentFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_HouseBill = "0001";

			using (var form = new CusUSLVConsignmentForm(consignment))
			{
				form.Show();

				AssertEquals("Form Caption", "House Bill 0001", form.FormCaption);
			}
		}

		public void TestDataBinding_ConsigneeIdentifier()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				var consigneeIdTextBox = ConsignmentForm.Controls.Find("textBoxConsigneeId", true).Single() as ZTextBox;

				CombineAssertions("Consignee Identifier text box", () =>
				{
					AssertNotNull(consigneeIdTextBox);
					AssertEquals("ULB_ConsigneeIdentifier", consigneeIdTextBox.GetBindingMember());
				});
			}
		}

		public void TestDataBinding_SellerIdentifier()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				var sellerIdTextBox = ConsignmentForm.Controls.Find("textBoxSellerId", true).Single() as ZTextBox;

				CombineAssertions("Seller Identifier text box", () =>
				{
					AssertNotNull(sellerIdTextBox);
					AssertEquals("ULB_SellerIdentifier", sellerIdTextBox.GetBindingMember());
				});
			}
		}

		public void TestOnlyFirstFormEditableAmongMultipleOpenedForms()
		{
			var consignment = Factory.New<CusUSLVConsignment>();

			using (var form1 = new CusUSLVConsignmentForm(consignment))
			{
				form1.Show();
				Application.DoEvents();

				using (var form2 = new CusUSLVConsignmentForm(consignment))
				{
					form2.Show();
					Application.DoEvents();

					CombineAssertions(() =>
					{
						AssertEquals("First form should be editable", ODisplayMode.Browse, form1.DisplayMode);
						AssertEquals("Second form should be readonly", ODisplayMode.ReadOnly, form2.DisplayMode);
					});
				}
			}
		}

		public void TestDISFeatures()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				var plugIn = ConsignmentForm.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				ConsignmentForm.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
				var disDataButton = ConsignmentForm.FindMatchingControl(plugIn.UserControl, x => x is ZButton && x.Name == "DISDataButton");

				Assert("DIS Button Should be visible", disDataButton.Visible);
			}
		}

		public void TestCheckBoxMultipleLines()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();

				var checkBoxMultipleLines = ConsignmentForm.Controls.Find("checkBoxMultipleLines", true)[0] as ZCheckBox;
				AssertNotNull("pre condition", checkBoxMultipleLines);
				AssertEquals("pre condition", 0, Consignment.CusUSLVItems.Count);
				AssertEquals("pre condition", false, checkBoxMultipleLines.Checked);

				Consignment.CusUSLVItems.AddNew();

				AssertEquals(1, Consignment.CusUSLVItems.Count);
				AssertEquals(false, checkBoxMultipleLines.Checked);

				_ = Consignment.FirstCusUSLVItem;
				Consignment.CusUSLVItems.AddNew();

				AssertEquals(2, Consignment.CusUSLVItems.Count);
				AssertEquals(true, checkBoxMultipleLines.Checked);
			}
		}

		public void TestCheckBoxPGARequirements()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();

				var checkBoxPGARequirements = ConsignmentForm.Controls.Find("checkBoxPGARequirements", true)[0] as ZCheckBox;
				AssertNotNull("pre condition", checkBoxPGARequirements);
				AssertEquals("pre condition", false, checkBoxPGARequirements.Checked);

				const string tariffNum = "8923894890";
				var tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffNum;
				tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
				tariff.UE_DateTo = ZDateTime.Today;
				tariff.UE_PGACodes = "OM1";

				var item = Consignment.CusUSLVItems.AddNew();
				item.ULI_TariffFormatted = tariffNum;
				AssertEquals(true, checkBoxPGARequirements.Checked);

				item.ULI_TariffFormatted = "123";
				AssertEquals(false, checkBoxPGARequirements.Checked);
			}
		}

		public void TestWhenParentFormIsConsignmentForm_ThenTransportFieldsAreReadOnly()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			using (InitializeTestingForm(consignment))
			{
				ConsignmentForm.Show();
				Application.DoEvents();

				var groupBoxTransportDetails = ConsignmentForm.Controls.Find("groupBoxTransportDetails", true)[0] as ZGroupBox;
				var textBoxEntryFilerCode = ConsignmentForm.Controls.Find("textBoxEntryFilerCode", true)[0] as ZTextBox;
				var textBoxContactName = ConsignmentForm.Controls.Find("textBoxContactName", true)[0] as ZTextBox;
				var textBoxContactPhone = ConsignmentForm.Controls.Find("textBoxContactPhone", true)[0] as ZTextBox;
				var orgFindBoxMainTabClient = ConsignmentForm.Controls.Find("orgFindBoxMainTabClient", true)[0];
				var orgFindBoxMainTabImporter = ConsignmentForm.Controls.Find("orgFindBoxMainTabImporter", true)[0];

				var expectedNonReadOnlyControls = new List<string>() { "panelLocationDate", "panelTransportDetailGroup", "DropButton", "DropButton", "SeparatorLabel" };
				var otherActualNonReadOnlyControls = new[] { textBoxEntryFilerCode, textBoxContactName, textBoxContactPhone, orgFindBoxMainTabClient, orgFindBoxMainTabImporter }.Where(c => !c.GetReadOnly());
				var actualTransportDetailsNonReadOnlyControls = groupBoxTransportDetails.FindAll<Control>(c => !c.GetReadOnly());
				var actualAllNonReadOnlyControls = actualTransportDetailsNonReadOnlyControls.Union(otherActualNonReadOnlyControls).Select(c => c.Name);

				AssertContainsExactElementsInAnyOrder("There are fields/controls that are editable but should be read only", expectedNonReadOnlyControls, actualAllNonReadOnlyControls);
			}
		}

		public void TestCommodityDetailsGrid()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				ConsignmentForm.CommoditiesTabPage.Show();
				CombineAssertions("Commodity Details Grid Columns", () =>
				{
					AssertEquals(true, ConsignmentForm.GridCommodityDetails.Visible);
					AssertEquals(9, ConsignmentForm.GridCommodityDetails.Columns.Count);
					AssertEquals("Product Code", ConsignmentForm.GridCommodityDetails.Columns[0].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[0].ColumnStyle.ReadOnly);
					AssertEquals("Tariff", ConsignmentForm.GridCommodityDetails.Columns[1].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[1].ColumnStyle.ReadOnly);
					AssertEquals("Goods Description", ConsignmentForm.GridCommodityDetails.Columns[2].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[2].ColumnStyle.ReadOnly);
					AssertEquals("Line Value", ConsignmentForm.GridCommodityDetails.Columns[3].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[3].ColumnStyle.ReadOnly);
					AssertEquals("Line Currency", ConsignmentForm.GridCommodityDetails.Columns[4].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[4].ColumnStyle.ReadOnly);
					AssertEquals("Exchange Rate", ConsignmentForm.GridCommodityDetails.Columns[5].ColumnStyle.HeaderText);
					AssertEquals(true, ConsignmentForm.GridCommodityDetails.Columns[5].ColumnStyle.ReadOnly);
					AssertEquals("Ctry/Rgn. of Origin", ConsignmentForm.GridCommodityDetails.Columns[6].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[6].ColumnStyle.ReadOnly);
					AssertEquals("ADD N/A", ConsignmentForm.GridCommodityDetails.Columns[7].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[7].ColumnStyle.ReadOnly);
					AssertEquals("CVD N/A", ConsignmentForm.GridCommodityDetails.Columns[8].ColumnStyle.HeaderText);
					AssertEquals(false, ConsignmentForm.GridCommodityDetails.Columns[8].ColumnStyle.ReadOnly);
				});
			}
		}

		public void TestCommodityDetailsBindings()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				ConsignmentForm.CommoditiesTabPage.Show();

				AssertNotNull(ConsignmentForm.GridCommodityDetails);
				AssertEquals("CusUSLVItems", ConsignmentForm.GridCommodityDetails.GetBindingMember());
			}
		}

		public void TestStatusTabBindings()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				ConsignmentForm.StatusTabPage.Show();

				AssertNotNull(ConsignmentForm.GridDisposition);
				AssertEquals("DispositionCodesView", ConsignmentForm.GridDisposition.GetBindingMember());
			}
		}

		public void TestPGARequirementsControlBindings()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();
				ConsignmentForm.CommoditiesTabPage.Show();

				AssertNotNull(ConsignmentForm.PGARequirementsControl);
				AssertEquals("CusUSLVItems", ConsignmentForm.PGARequirementsControl.GetBindingMember());
			}
		}

		public void TestMessagingMenuItemContents()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			using (InitializeTestingForm(consignment))
			{
				ConsignmentForm.Show();
				var messagingMenuItems = ConsignmentForm.MainMenuForTest.MenuItems.FindByText("Messaging").MenuItems.Cast<MenuItem>();

				var expectedMenuItems = new[] { "Send Original Message", "Send Replacement Message", "Send Update Message", "Send Deletion Message" };
				AssertContainsExactElementsInAnyOrder("Messaging Menu for single bill should exclude Update Messages action", expectedMenuItems, messagingMenuItems.Select(menuItem => menuItem.Text));
			}
		}

		public void TestNoMessagingMenuWhenConsignmetIsInactive()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_IsActive = false;

			using (InitializeTestingForm(consignment))
			{
				ConsignmentForm.Show();
				var messagingMenu = ConsignmentForm.MainMenuForTest.MenuItems.FindByText("Messaging");

				AssertNull("Messaging Menu is not added because consignment is inactive", messagingMenu);
			}
		}

		public void TestOpenParentMenuExists()
		{
			using (InitializeTestingForm())
			{
				ConsignmentForm.Show();

				var menuItem = ((IFileMenuItemsProvider)ConsignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Low Value Entries parent job");
				AssertNotNull(menuItem);
			}
		}

		public void TestOpenParentMenuDisable()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			using (var clearanceForm = new CusUSLVClearanceForm(clearance))
			{
				clearanceForm.Show();

				using (InitializeTestingForm(consignment))
				{
					ConsignmentForm.Show();

					var menuItem = ((IFileMenuItemsProvider)ConsignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Low Value Entries parent job");
					Assert(!menuItem.Enabled);
				}
			}
		}

		public void TestOpenParentMenuWorks()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			Factory.Save();

			using (InitializeTestingForm(consignment))
			{
				ConsignmentForm.Show();
				var menuItem = ((IFileMenuItemsProvider)ConsignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Low Value Entries parent job");
				menuItem.PerformClick();

				var clearanceForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name.Contains("USLVClearance"));
				AssertNotNull(clearanceForm);
				AssertEquals((clearanceForm as CusUSLVClearanceForm).BusinessEntity.PK, clearance.PK);
				AssertEquals(ControllerIDs.Customs.US.USLowValueEntries, (clearanceForm as CusUSLVClearanceForm).ControllerID);

				clearanceForm.Close();
			}
		}

		public void TestOpenParentMenuWorksWithChange()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearanceForTest>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item = consignment.CusUSLVItems.AddNew();
			item.ULI_RN_NKCountryOfOrigin = "AU";
			item.ULI_RX_NKCurrency = "AUD";
			item.ULI_Tariff = "12345678";
			consignment.ULB_NumberOfPacks = 1;

			using (InitializeTestingForm(consignment))
			{
				try
				{
					ConsignmentForm.Show();
					consignment.ULB_HouseBill = "123456789";

					var menuItem = ((IFileMenuItemsProvider)ConsignmentForm).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Low Value Entries parent job");
					menuItem.PerformClick();
					Application.DoEvents();
					var message = UnitTestUserNotification.Instance.LastMessage;
					AssertContains("This consignment has not yet been saved. Do you want to save and proceed?", message.Text);
					var clearanceForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name.Contains("USLVClearance"));
					AssertNotNull(clearanceForm);
					clearanceForm.Close();
				}
				catch (Exception ex)
				{
					var messageBuilder = new StringBuilder();
					messageBuilder.AppendLine(string.Format("Consignment's Clearance is null: {0}", consignment.Shipment == null));
					messageBuilder.AppendLine(string.Format("Consignment's clearance PK: {0}", consignment.Shipment?.PK ?? ZGuid.Empty));
					messageBuilder.AppendLine(string.Format("Clearance is null: {0}", clearance == null));
					messageBuilder.AppendLine(string.Format("Clearance PK: {0}", clearance == null ? ZGuid.Empty : clearance.PK));
					messageBuilder.AppendLine(string.Format("Clearance Deletion Stack Trace: {0}", clearance.StackTraceOfDeletion));
					throw new DeveloperNotificationException(messageBuilder.ToString(), ex);
				}
			}
		}

		public void TestShowNewForm_Consignment()
		{
			var controller = new CusUSLVConsignmentController();

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			Factory.Save();
			var consignmentView = Factory.Load<USConsignmentCombined>(consignment.PK);

			using (var form = controller.ShowEditForm(consignmentView) as CusUSLVConsignmentForm)
			{
				var saveButtonControl = form.Controls.Find("SaveButtonUserControl", true).OfType<ZPostingButtonsUserControl>().Single();
				saveButtonControl.SaveButton.PerformClick();
				Application.DoEvents();

				var newForm = Application.OpenForms.OfType<CusUSLVConsignmentForm>().Single();
				var newConsignment = (CusUSLVConsignment)newForm.BusinessEntity;
				AssertNotEquals("A new consignment should have been created", consignment.PK, newConsignment.PK);
				AssertEquals("New consignment should have same parent clearance", clearance.PK, newConsignment.Shipment.PK);
				Assert("New consignment should not be saved yet", !newConsignment.IsInDatabase);

				newForm.Close();
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CusUSLVConsignmentForm(Factory.New<CusUSLVConsignment>());
			return form;
		}

		CusUSLVConsignmentFormForTest ConsignmentForm { get; set; }

		CusUSLVConsignment Consignment { get; set; }

		IDisposable InitializeTestingForm(CusUSLVConsignment consignment = null)
		{
			Consignment = consignment ?? Factory.New<CusUSLVConsignment>();
			return ConsignmentForm = new CusUSLVConsignmentFormForTest(Consignment);
		}

		#endregion
	}

	public class CusUSLVConsignmentFormForTest : CusUSLVConsignmentForm
	{
		public CusUSLVConsignmentFormForTest(CusUSLVConsignment consignment) : base(consignment) { }

		public ZGrid GridCommodityDetails => Controls.Find("gridCommodityDetails", true).OfType<ZGrid>().Single();

		public ZGrid GridDisposition => Controls.Find("gridDisposition", true).OfType<ZGrid>().Single();

		public ZTabPage StatusTabPage => Controls.Find("tabPageStatus", true).OfType<ZTabPage>().Single();

		public Control FindMatchingControl(Control parentControl, Predicate<Control> match)
		{
			foreach (Control control in parentControl.Controls)
			{
				if (match(control))
				{
					return control;
				}
			}

			foreach (Control control in parentControl.Controls)
			{
				var result = FindMatchingControl(control, match);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public OGAPGARequirementsControl PGARequirementsControl => Controls.Find("pgaRequirementsControl", true).OfType<OGAPGARequirementsControl>().Single();

		public ZTabPage CommoditiesTabPage => Controls.Find("tabPageCommodities", true).OfType<ZTabPage>().Single();

		public MainMenu MainMenuForTest => MainMenu;
	}

	public class CusUSLVClearanceForTest : CusUSLVClearance
	{
		public CusUSLVClearanceForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			StackTraceOfDeletion = "";
		}

		public override void Delete()
		{
			base.Delete();
			StackTraceOfDeletion = new System.Diagnostics.StackTrace().ToString();
		}

		public string StackTraceOfDeletion;
	}
}
