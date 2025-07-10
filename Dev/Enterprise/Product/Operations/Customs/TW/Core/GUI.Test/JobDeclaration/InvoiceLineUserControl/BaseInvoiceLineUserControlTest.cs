using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumnName()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(JobComInvoiceLine.Schema.JI_FormattedTariff, invoiceLineUserControl.TariffColumnName);
			}
		}

		public void TestCarInfoWhenOrderDescending()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var listManager = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager;
				var currentInvoiceLine = (BaseJobComInvoiceLine)listManager.GetCurrent();
				AssertEquals(line.PK, currentInvoiceLine.PK);
				var listView = jobDeclartion.FilteredInvoiceLines as IBindingList;
				listView.ApplySort(line.JI_LineNoInfo.PropertyDescriptor, ListSortDirection.Descending);
				currentInvoiceLine = (BaseJobComInvoiceLine)listManager.GetCurrent();
				AssertEquals(line1.PK, currentInvoiceLine.PK);
				Assert(!invoiceLineUserControl.CarInfoTabPage.TabVisible);
				currentInvoiceLine.JI_Tariff = "8766";
				Assert(invoiceLineUserControl.CarInfoTabPage.TabVisible);
			}
		}

		public void TestCurrentDataItem()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertNotNull(invoiceLineUserControl.CurrentDataItem);
			}
		}

		public void TestFilterBusinessObjectType()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertType(typeof(InvoiceLineFilterBusinessObject), invoiceLineUserControl.FilterBusinessObject);
			}
		}

		public void TestCharacterCasing()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				foreach (var name in new string[] { "JI_DescriptionLongTextControl", "TW_CompositionsLongTextControl", "JI_NDescriptionLongTextControl", "TWGroupLongTextControl" })
				{
					var longTextTextBox = invoiceLineUserControl.FindSingleOrDefault<LongTextControl>(c => c.Name == name);
					AssertNotNull(name, longTextTextBox);
					AssertEquals(name, CharacterCasing.Normal, longTextTextBox.CharacterCasing);
				}

				foreach (var name in new string[] { "DeclarationGoodsDecriptionTextBox", "JI_ModelTextBox", "JI_BrandNameTextBox" })
				{
					var textBox = invoiceLineUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == name);
					AssertNotNull(name, textBox);
					AssertEquals(name, CharacterCasing.Normal, textBox.CharacterCasing);
				}
			}
		}

		public void TestCarInfoVisibleControl()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					line.JI_Tariff = "86";
					var carInfoTab = invoiceLineUserControl.FindSingle<ZTabPage>("CarInfoTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = carInfoTab;
					AssertEquals(true, invoiceLineUserControl.FindSingle<ZPanel>("CarInfoPanel").Visible);
				}

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = "EXP";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					AssertEquals(false, invoiceLineUserControl.FindSingle<ZPanel>("CarInfoPanel").Visible);
				}
			}
		}

		public void TestChangeControlsVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
				AssertNull(invoiceLineUserControl.LineDetailTabControl.GetTabPage("LineDetailsTabPage"));
			}
		}

		public void TestCurrentInvoiceQuantityGroupPanelVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var currentInvoiceQuantityGroupPanel = invoiceLineUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "CurrentInvoiceQuantityGroupPanel");
					var currentInvoiceQuantityGroupGrid = currentInvoiceQuantityGroupPanel.FindSingleOrDefault<ZGrid>(c => c.Name == "CurrentInvoiceQuantityGroupGrid");
					AssertEquals(true, currentInvoiceQuantityGroupPanel.Visible);
					AssertEquals(true, currentInvoiceQuantityGroupGrid.Visible);
				}
			}
		}

		public void TestTWOtherDetailsTabPageVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var twOtherDetailsTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = twOtherDetailsTab;
					AssertEquals(true, twOtherDetailsTab.TabVisible);
				}

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = "EXP";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var twOtherDetailsTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = twOtherDetailsTab;
					AssertEquals(true, twOtherDetailsTab.TabVisible);
				}
			}
		}

		public void TestSupportingDocumentsTabPageVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var supportingDocumentsTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = supportingDocumentsTab;
					AssertEquals(true, supportingDocumentsTab.TabVisible);
				}

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = "EXP";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var supportingDocumentsTab = invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = supportingDocumentsTab;
					AssertEquals(true, supportingDocumentsTab.TabVisible);
				}
			}
		}

		public void TestJI_HazMatCodeVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Air;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var lineDetailTabPage = invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = lineDetailTabPage;
					var lineDetailsUserControl = invoiceLineUserControl.FindSingle<LineDetailsUserControl>("LineDetailsUserControl");
					AssertEquals(true, lineDetailsUserControl.FindSingle<ZCodeFindBox>("UNDGCodeFindBox").Visible);
				}

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Sea;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var lineDetailTabPage = invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = lineDetailTabPage;
					var lineDetailsUserControl = invoiceLineUserControl.FindSingle<LineDetailsUserControl>("LineDetailsUserControl");
					AssertEquals(true, lineDetailsUserControl.FindSingle<ZCodeFindBox>("UNDGCodeFindBox").Visible);
				}
			}
		}

		public void TestCarInfoTabPagesVisibilityBaseOnTariff_IfItsCarRelatedTariff()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					invoiceLine.JI_Tariff = "86";
					AssertEquals(invoiceLineUserControl.CarInfoTabPage.TabVisible, true);
					invoiceLine.JI_Tariff = "87";
					AssertEquals(invoiceLineUserControl.CarInfoTabPage.TabVisible, true);
					invoiceLine.JI_Tariff = "44";
					AssertEquals(invoiceLineUserControl.CarInfoTabPage.TabVisible, false);
				}
			}
		}

		public void TestControlOutsideBoundsOfParent()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			header.InvoiceLines.AddNew();
			TestControlOutsideBoundsOfParentForImport(jobDeclartion, true);
			TestControlOutsideBoundsOfParentForImport(jobDeclartion, false);
			TestControlOutsideBoundsOfParentForExport(jobDeclartion, true);
			TestControlOutsideBoundsOfParentForExport(jobDeclartion, false);
		}

		void TestControlOutsideBoundsOfParentForImport(JobDeclaration jobDeclartion, bool smallArea)
		{
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
				var bottomPanel = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "BottomPanel");
				if (smallArea)
				{
					bottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 480, true);
					bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1230, 480, true);
				}
				else
				{
					bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 500, true);
				}

				CombineAssertions(() =>
				{
					CheckControl(invoiceLinesUserControl);
					foreach (ZTabPage tabPage in invoiceLinesUserControl.LineDetailTabControl.TabPages)
					{
						invoiceLinesUserControl.LineDetailTabControl.SelectedTab = tabPage;
						CheckControl(tabPage);
					}
				}

				);
			}
		}

		void TestControlOutsideBoundsOfParentForExport(JobDeclaration jobDeclartion, bool smallArea)
		{
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLinesUserControl = ((ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
				var bottomPanel = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "BottomPanel");
				if (smallArea)
				{
					bottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 480, true);
					bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1230, 480, true);
				}
				else
				{
					bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 500, true);
				}

				CombineAssertions(() =>
				{
					CheckControl(invoiceLinesUserControl);
					foreach (ZTabPage tabPage in invoiceLinesUserControl.LineDetailTabControl.TabPages)
					{
						invoiceLinesUserControl.LineDetailTabControl.SelectedTab = tabPage;
						CheckControl(tabPage);
					}
				}

				);
			}
		}

		void CheckControl(Control control)
		{
			if (control.Visible && !(control.Parent is ZGrid))
			{
				if (control.Controls.Count > 0)
				{
					foreach (Control childControl in control.Controls)
					{
						CheckControl(childControl);
					}
				}

				CheckControlParentRelationship(control);
			}
		}

		void CheckControlParentRelationship(Control control)
		{
			bool outsideTheBounds = true;
			if (control.Parent != null && control.Parent.Width > 0 && control.Parent.Height > 0 && (control.Top < 0 || control.Left < 0 || control.Left + control.Width > control.Parent.Width || (!ControlHeightIsAllowedToGoOutsideBoundsOfParent(control) && control.Top + control.Height > control.Parent.Height)))
			{
				var parentAsScrollableControl = control.Parent as ScrollableControl;
				if (parentAsScrollableControl == null || !parentAsScrollableControl.AutoScroll)
				{
					outsideTheBounds = false;
				}
			}

			Assert($"{control.Name} positioned incorrectly (outside the bounds of the parent control)", outsideTheBounds);
		}

		bool ControlHeightIsAllowedToGoOutsideBoundsOfParent(Control control)
		{
			bool result = false;
			var toolBar = control as ToolBar;
			if (toolBar != null && toolBar.Parent != null)
			{
				int allowableSpace = toolBar.Height - toolBar.ButtonSize.Height;
				result = toolBar.Bottom >= toolBar.Parent.Height + allowableSpace;
			}

			return result;
		}

		public void TestContainersTabPageVisibility()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			AssertContainersTabPageVisibility(jobDeclaration);
			jobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			AssertContainersTabPageVisibility(jobDeclaration);
		}

		static void AssertContainersTabPageVisibility(JobDeclaration jobDeclaration)
		{
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					AssertEquals(false, invoiceLineUserControl.ContainersTabPage.TabVisible);
				}
			}
		}

		public void TestDefaultColumns()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var visibleColumnCount = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Where(x => x.IsVisible).Count();
					AssertEquals(21, visibleColumnCount);
				}

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var visibleColumnCount = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Where(x => x.IsVisible).Count();
					CombineAssertions(() =>
					{
						AssertEquals("visibleColumnCount should be 22", 22, visibleColumnCount);
						Assert("Should have JI_PrimaryPreference column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference));
						Assert("Should have PreviousBondedEntryNumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains("PreviousBondedEntryNumber"));
						Assert("Should have PreviousBondedEntryLineNumber column in CustomsInvoiceLinesBoundGrid", invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains("PreviousBondedEntryLineNumber"));
						AssertEquals("PreviousBondedEntryNumber should be hidden", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["PreviousBondedEntryNumber"].IsVisible);
						AssertEquals("PreviousBondedEntryLineNumber should be hidden", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns["PreviousBondedEntryLineNumber"].IsVisible);
						AssertEquals("Serial Number column should exist", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns.Contains(JobComInvoiceLineSchema.Constants.JI_SerialNumber));
						AssertEquals("Serial Number column should be hidden", false, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLineSchema.Constants.JI_SerialNumber].IsVisible);
						AssertEquals("TWL_DocumentaryQty column should be displayed", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.TWL_DocumentaryQty].IsVisible);
						AssertEquals("TWL_DocumentaryUQ column should be displayed", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.TWL_DocumentaryUQ].IsVisible);
						AssertEquals("TWL_DocumentaryUnitPrice column should be displayed", true, invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice].IsVisible);
					});
				}
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsTypes()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var grid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					AssertType(typeof(ZMultiLineTextBoxColumnStyle), grid.Columns.FirstOrDefault(t => t.ColumnName == JobComInvoiceLineSchema.Constants.JI_NDescription).ColumnStyle);
					AssertType(typeof(ZMultiLineTextBoxColumnStyle), grid.Columns.FirstOrDefault(t => t.ColumnName == JobComInvoiceLine.Schema.JI_Group).ColumnStyle);
					AssertType(typeof(ZMultiLineTextBoxColumnStyle), grid.Columns.FirstOrDefault(t => t.ColumnName == JobComInvoiceLine.Schema.JI_Compositions).ColumnStyle);
					AssertType(typeof(ZMultiLineTextBoxColumnStyle), grid.Columns.FirstOrDefault(t => t.ColumnName == JobComInvoiceLineSchema.Constants.JI_Description).ColumnStyle);
				}
			}
		}

		public void TestTabPagesOrder()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingAgency = "XX";
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX").IsLinkedCMHeader = true;
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					CombineAssertions(() =>
					{
						AssertEquals(false, invoiceLineUserControl.DutiesTaxesAndFeesTabPage.TabVisible);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[0]);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[1]);
					});
				}
			}

			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX").IsLinkedCMHeader = true;
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					CombineAssertions(() =>
					{
						AssertEquals(true, invoiceLineUserControl.DutiesTaxesAndFeesTabPage.TabVisible);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[0]);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[1]);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("DutiesTaxesAndFeesTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[2]);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[3]);
						AssertEquals(invoiceLineUserControl.FindSingle<ZTabPage>("AircraftPartsTabPage"), invoiceLineUserControl.LineDetailTabControl.TabPages[4]);
					});
				}
			}
		}

		public void TestTariffAttributesValueTextBoxReadOnly()
		{
			AssertTariffDescriptionTextBoxReadOnly(JobMessageTypeList.Codes.Import);
			AssertTariffDescriptionTextBoxReadOnly(JobMessageTypeList.Codes.Export);
		}

		void AssertTariffDescriptionTextBoxReadOnly(ZString messageType)
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = messageType;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var tariffDescriptionTextBox = invoiceLineUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "JI_TariffDescriptionTextBox");
				AssertEquals(true, tariffDescriptionTextBox.ReadOnly);
			}
		}

		public void TestDeclarationGoodsDecriptionTextBoxScrollBar()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage");
					var declarationGoodsDecriptionTextBox = invoiceLineUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "DeclarationGoodsDecriptionTextBox");
					AssertEquals("Should have vertical scrollbars", ScrollBars.Vertical, declarationGoodsDecriptionTextBox.ScrollBars);
				}
			}
		}

		public void TestPropertiesofCalcEdits()
		{
			AssertPropertiesofCalcEdit("JI_ModelYearCalcEdit", 9999m);
			AssertPropertiesofCalcEdit("JI_NumberOfDoorCalcEdit", 9m);
			AssertPropertiesofCalcEdit("JI_CylindersCalcEdit", 99m);
			AssertPropertiesofCalcEdit("JI_GearsCalcEdit", 99m);
			AssertPropertiesofCalcEdit("JI_SeatsCalcEdit", 99m);
		}

		void AssertPropertiesofCalcEdit(string controlName, decimal maxValue)
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				var calcEdit = control.FindSingleOrDefault<ZCalcEdit>(c => c.Name == controlName);
				Assert(calcEdit.ShowEmptyStringForEmptyValue);
				Assert(!calcEdit.ShowGroupSeparators);
				AssertEquals(maxValue, calcEdit.MaxValue);
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsStyles()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					var previousBondedEntryLineNumberStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "PreviousBondedEntryLineNumber") as ZCalcEditColumnStyleInfo;
					AssertEquals("previousBondedEntryLineNumber should not show group separators", false, previousBondedEntryLineNumberStyle.ShowGroupSeparators);

					var customsQuantityStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_CustomsQuantity");
					var customsUnitQtyStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_CustomsUnitQty");
					AssertEquals("JI_CustomsQuantity group Name should be", "Statistical Weight", customsQuantityStyle.GroupName.Caption);
					AssertEquals("JI_CustomsUnitQty group Name should be", "Statistical Weight", customsUnitQtyStyle.GroupName.Caption);

					var packagingQTYStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == AutoTWJobComInvoiceLine.Schema.JI_PackagingQTY);
					var packagingUQStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == AutoTWJobComInvoiceLine.Schema.JI_PackagingUQ);
					AssertEquals("JI_PackagingQTY group Name should be", "Number of Package", packagingQTYStyle.GroupName.Caption);
					AssertEquals("JI_PackagingUQ group Name should be", "Number of Package", packagingUQStyle.GroupName.Caption);
				}
			}
		}

		public void TestLineChargesTabPageVisibility()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					Assert("LineChargesTabPage should be hidden in TW.", !invoiceLineUserControl.LineChargesTabPage.TabVisible);
				}
			}
		}

		public void TestInvoiceLineNewOwnerPartColumns()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.Controls.Add(invoiceLineUserControl);
					form.Show();

					var invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CombineAssertions("InvoiceLineGrid Should Have All New Owner Part Columns", () =>
					{
						AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewSerialNumber));
						AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewOwnerPartNo));
						AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewPartAttribute1));
						AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewPartAttribute2));
						AssertNotNull(invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NewPartAttribute3));
					});
				}
			}
		}

		public void TestAircraftPartsTabPageVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = (JobComInvoiceLine)jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					AssertEquals(false, invoiceLineUserControl.AircraftPartsTabPage.TabVisible);
				}
			}

			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					AssertEquals(true, invoiceLineUserControl.AircraftPartsTabPage.TabVisible);
				}
			}
		}

		public void TestLineSummaryPanelVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals(false, invoiceLineUserControl.LineSummaryPanel.Visible);
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumns()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				var columns = boundGrid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertHasColumn(columns, JobComInvoiceLineSchema.Constants.JI_Description);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingNo1, "Permit 1");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingLineNo1, "Permit 1");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingNo2, "Permit 2");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingLineNo2, "Permit 2");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingNo3, "Permit 3");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingLineNo3, "Permit 3");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingNo4, "Permit 4");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingLineNo4, "Permit 4");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingNo5, "Permit 5");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PermitCusSupportingLineNo5, "Permit 5");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_DocumentaryQty, "Documentary Quantity/Unit");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_DocumentaryUQ, "Documentary Quantity/Unit");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.ReservedFieldCode1, "Reserved Field 1");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.ReservedFieldCode2, "Reserved Field 2");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.ReservedFieldValue1, "Reserved Field 1");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.ReservedFieldValue2, "Reserved Field 2");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.ManufacturerDocAddressOrgPK);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_TextileWidth, "Textile Width");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_TextileWidthUQ, "Textile Width");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_PermitUnitPrice);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_OriginCriteria);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_PTCriteria);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_PTCriteria2);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_ManufacturerRelationship);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_TariffPrintLength);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_IMPTariff);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.NX101ShippingMarks);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, "Licensing Quantity");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, "Licensing Quantity");
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_GoodsType);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.PreviousPermitNo);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_ProductThickness);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_ProductGrade);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_TariffExtensionCode);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_InnerPackType);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_InnerPackingMaterial);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_InnerPackDescription);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_QuarantineTreatment);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_QuarantineFeatures);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_VaccinationTypeDate);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_MicrochipID);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AnimalAgeYear);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AnimalAgeMonth);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AnimalMaleQty);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AnimalFemaleQty);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_BarCode);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlcoholCountryRegion);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_NoOriginalLotNoAmt);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_RemovedLotNoAmt);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlteredLotNoAmt);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_BottledDate);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_ExpirationDate);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlcoholEndOfShelfLife);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlcoholAge);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlcoholYear);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_AlcoholPercentage);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_PackagingQTY);
					AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_PackagingUQ);
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn, string groupName = "")
		{
			var anyColumnHasGivenName = string.IsNullOrEmpty(groupName) ? columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn) : columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn && column.GroupName.Caption == groupName);
			Assert($"Should have the column - {nameOfColumn} with group name - {groupName}", anyColumnHasGivenName);
		}
	}
}
