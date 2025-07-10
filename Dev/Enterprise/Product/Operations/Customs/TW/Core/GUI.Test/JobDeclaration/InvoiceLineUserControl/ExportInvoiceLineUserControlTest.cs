using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (ExportInvoiceLineUserControl control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestShowLineCalculationsControls()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				using (var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting)
				{
					if (jobDeclarationUserControl != null)
					{
						brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
						var controls = brokerageControl.InvoiceLinesUserControl.Controls;
						AssertType<ExportInvoiceLineUserControl>(brokerageControl.InvoiceLinesUserControl);
						AssertEquals("Show Freight Amount(JI_Calc_FreightInInvoiceCurr) is true", false, controls.Find("JI_Calc_FreightConvertToLocalCurrencyControl", true)[0].Visible);
						AssertEquals("Show Insurance Amount(JI_Calc_InsuranceInInvoiceCur) is true", false, controls.Find("JI_Calc_InsuranceConvertToLocalCurrencyControl", true)[0].Visible);
					}
				}
			}
		}

		public void TestCaptions()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLine.JI_Tariff = "1";
					var lineDetailTabPage = invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage");
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = lineDetailTabPage;
					var procedureColumnInfo = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Procedure);
					AssertEquals("Mode of Statistics", procedureColumnInfo.CaptionResourceString.Caption);
					AssertEquals("The customs statistics code that indicates the trading type of exported goods.", procedureColumnInfo.CaptionResourceString.FullDescription);
				}
			}
		}

		public void TestControlOutsideBoundsOfParent()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			TestControlOutsideBoundsOfParentForExport(jobDeclartion, true);
			TestControlOutsideBoundsOfParentForExport(jobDeclartion, false);
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

		public void TestJI_ProcedureDropEditCaptionResourceString()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = ((BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl))
				{
					var fJI_ProcedureDropEditCaptionResourceString = invoiceLineUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "JI_ProcedureDropEdit").CaptionResourceString;
					AssertEquals("Mode of Statistics", fJI_ProcedureDropEditCaptionResourceString.Caption);
					AssertEquals("The customs statistics code that indicates the trading type of exported goods.", fJI_ProcedureDropEditCaptionResourceString.FullDescription);
				}
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("TWLineDetailsTabPage");
					var invoiceLinesGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					Assert(!invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_BondedGoodsCode).IsVisible);
				}
			}
		}

		public void TestDefaultColumnsInSortOrder()
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
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
				customsInvoiceLinesBoundGrid.ResetColumns();
				AssertEquals(129, customsInvoiceLinesBoundGrid.Columns.Count);
				var visibleColumns = customsInvoiceLinesBoundGrid.Columns.Where(x => x.IsVisible);
				var visibleColumnCount = visibleColumns.Count();
				AssertEquals(21, visibleColumnCount);
				for (var i = 0; i < ExpectedDefaultColumnsForGrid.Count; i++)
				{
					var column = customsInvoiceLinesBoundGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedDefaultColumnsForGrid[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		List<string> ExpectedDefaultColumnsForGrid
		{
			get
			{
				var columns = new List<string>();
				columns.Add(Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice);
				columns.Add(JobComInvoiceLine.Schema.InvoiceHeaderDisplaySequence);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_LineNo);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_PartNo);
				columns.Add(JobComInvoiceLine.Schema.JI_FormattedTariff);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_Procedure);
				columns.Add(JobComInvoiceLine.Schema.JI_Group);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_Description);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ);
				columns.Add(JobComInvoiceLine.Schema.JI_EnteredUnitPrice);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_LinePrice);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeight);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty);
				columns.Add(JobComInvoiceLineSchema.Constants.JI_BrandName);
				columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryQty);
				columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUQ);
				columns.Add(JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice);
				return columns;
			}
		}

		public void TestLineDetailsUserControl()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var lineDetails = invoiceLineUserControl.FindSingle<LineDetailsUserControl>("LineDetailsUserControl");
				AssertType(typeof(ExportLineDetailsUserControl), lineDetails);
			}
		}
	}
}
