using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormBaseOnlyTest : CommercialInvoiceFormAbstractTest
	{
		public void TestInvoiceLineUserControlChangeWhenMessageTypeChanges()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				var exportInvoiceLineUserControl = form.InvoiceLineUserControl;

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

				form.MainTabControl.SelectedTab = form.LinesTabPage;
				var importInvoiceLineUserControl = form.InvoiceLineUserControl;
				AssertNotEquals("A new InvoiceLineUserControl should have been created", exportInvoiceLineUserControl, importInvoiceLineUserControl);
				AssertEquals("exportInvoiceLineUserControl should have been disposed", true, exportInvoiceLineUserControl.IsDisposed);
			}
		}

		public void TestHeaderTabPageHasInvoiceHeaderUserControl()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				var headerTabPage = form.HeaderTabPage;
				form.MainTabControl.SelectedTab = headerTabPage;
				AssertEquals("headerTabPage.Controls.Count", 1, headerTabPage.Controls.Count);
				AssertType<InvoiceHeaderUserControl>(headerTabPage.Controls[0]);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestExportToXmlMenu()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_InvoiceNumber = "uyiwer798243";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			invoice.JZ_OH_Supplier = supplier.PK;

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			invoice.JZ_OH_Buyer = importer.PK;

			Factory.Save();

			using (var form = new CommercialInvoiceForm(invoice))
			{
				var exportMenu = form.Menu.MenuItems.FindByText("Actio&ns").MenuItems.FindByText("Export to XML (Verbose)");

				AssertNotNull(exportMenu);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestInvoiceLogsTab()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			invoice.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedIndex = 8;

				var logsTabPage = (ZLogsTabPage)form.MainTabControl.TabPages["CommercialInvoiceEventTabPage"];
				AssertNotNull(logsTabPage);
				var logsUserControl = (ZUserControl)logsTabPage.Controls["ZLogsUserControl"];
				AssertNotNull(logsUserControl);
				var tabControl = (TabControl)logsUserControl.Controls["MainTabControl"];
				AssertNotNull(tabControl);
				var tabPage = (ZStmALogTabPage)tabControl.Controls["ChangeLogsTabPage"];
				AssertNotNull(tabPage);
				var kSplitContainer = (KSplitContainer)tabPage.Controls[0];
				AssertNotNull(kSplitContainer);
				var stmALogUserControl = (ZUserControl)kSplitContainer.Panel1.Controls[0];
				AssertNotNull(stmALogUserControl);
			}
		}

		public void TestInvoiceWorkflowTab()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			invoice.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();

				var workflowTabPage = (ZWorkflowTabPage)form.MainTabControl.TabPages["CommercialInvoiceWorkflowTabPage"];
				AssertNotNull(workflowTabPage);
			}
		}

		public void TestSaveFormDoesNotSaveDeclaration()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				invoice.Factory.Save();

				Assert(invoice.IsInDatabase);
				Assert("Should not be saved:JobDeclaration", !invoice.JobDeclaration.IsInDatabase);
			}
		}

		public void TestIDataGridLayoutIdentifierRoot()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;

			using (var form = new CommercialInvoiceForm(invoice))
			{
				AssertEquals("ID should return current country to have a country-specific setting for invoice line grid", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ((IDataGridLayoutIdentifierRoot)form).ID);
			}
		}

		public void TestPlugIns()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia); //this is to ensure the LandedCosting Document is loaded
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				AssertNotNull(ControllerIDs.DocDataPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestBrokerageMenuItems()
		{
			using (var form = GetNewCommercialInvoiceForm())
			{
				form.Show();
				var brokerageMenu = form.FindMenuItem_ForTest("Brokerage");
				brokerageMenu.ShowPopupMenu();

				AssertEquals(5, brokerageMenu.MenuItems.Count);
				AssertEquals("Auto Apportion &Weight", brokerageMenu.MenuItems[0].Text);
				AssertEquals("Allocate Remaining Weight", brokerageMenu.MenuItems[1].Text);
				AssertEquals("&Copy Previous Invoice Line", brokerageMenu.MenuItems[2].Text);
				AssertEquals("&Lock Commercial Invoice", brokerageMenu.MenuItems[3].Text);
				AssertEquals("&Unlock Commercial Invoice", brokerageMenu.MenuItems[4].Text);
			}
		}

		public void TestCustomLabelsLoadedProperly()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "org1";
			var label = org.CustomLabels.AddNew();
			label.OT_FieldName = Core.Constants.CustomLabels.ComInvoiceLine.CustomAttribute1;
			label.OT_Caption = "MUST SHOW ME";

			CombineAssertions(() =>
			{
				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				using (var form = new TestCommercialInvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					invoice.JZ_OH_Supplier = org.PK;
					invoice.JZ_MessageType = "EXP";
					form.JobDeclaration.JE_MessageType = "IMP";
					form.MainTabControl.SelectedTab = form.LinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 1", "MUST SHOW ME", form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}

				invoice = Factory.New<BaseJobComInvoiceHeader>();
				using (var form = new TestCommercialInvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();
					form.MainTabControl.SelectedTab = form.LinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 2", "Attribute 1", form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);

					form.MainTabControl.SelectedTab = form.HeaderTabPage;
					invoice.JZ_OH_Buyer = org.PK;
					invoice.JZ_MessageType = "IMP";
					form.MainTabControl.SelectedTab = form.LinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 2", "MUST SHOW ME", form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}

				invoice = Factory.New<BaseJobComInvoiceHeader>();
				invoice.JZ_OH_Buyer = org.PK;
				invoice.JZ_MessageType = "IMP";
				using (var form = new TestCommercialInvoiceForm(invoice))
				{
					form.Show();
					form.MainTabControl.SelectedTab = form.LinesTabPage;
					Application.DoEvents();
					AssertEquals("Case 3", "MUST SHOW ME", form.InvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["JI_CustomAttrib1"].ColumnStyle.HeaderText);
				}
			});
		}

		public void TestInvoiceLinesGridHasInvoiceColumnRemoved()
		{
			using (var form = GetNewCommercialInvoiceForm())
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				CheckColumnHasBeenRemoved(form.InvoiceLineUserControl, BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
			}
		}

		protected override CommercialInvoiceForm GetNewCommercialInvoiceForm()
		{
			var jobComInvoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			Factory.Save();
			return new CommercialInvoiceForm(jobComInvoiceHeader);
		}

		sealed class TestCommercialInvoiceForm : CommercialInvoiceForm
		{
			public TestCommercialInvoiceForm(BaseJobComInvoiceHeader header)
				: base(header)
			{
			}

			public BaseInvoiceLineUserControl InvoiceUserControl => InvoiceLineUserControl;
		}
	}
}
