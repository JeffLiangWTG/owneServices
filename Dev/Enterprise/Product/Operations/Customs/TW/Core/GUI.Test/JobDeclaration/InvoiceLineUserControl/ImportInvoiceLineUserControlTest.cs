using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineUserControl))]
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestRepairAssemblyProcessingWhenOrderDescending()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var line1 = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			Application.DoEvents();
			form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("DutiesTaxesAndFeesTabPage");
			var listManager = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager;
			var currentInvoiceLine = (BaseJobComInvoiceLine)listManager.GetCurrent();
			AssertEquals(line.PK, currentInvoiceLine.PK);
			var listView = jobDeclartion.FilteredInvoiceLines as IBindingList;
			listView.ApplySort(line.JI_LineNoInfo.PropertyDescriptor, ListSortDirection.Descending);
			currentInvoiceLine = (BaseJobComInvoiceLine)listManager.GetCurrent();
			AssertEquals(line1.PK, currentInvoiceLine.PK);
			Assert(!invoiceLineUserControl.ImportDetailsUserControl.RAPRORGroupBox.Visible);
			currentInvoiceLine.JI_Procedure = "37";
			Assert(invoiceLineUserControl.ImportDetailsUserControl.RAPRORGroupBox.Visible);
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using var control = new ImportInvoiceLineUserControl();
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}

		public void TestChangeControlsVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLine.JI_Tariff = "86";
				var carInfoTab = invoiceLineUserControl.FindSingle<ZTabPage>("CarInfoTabPage");
				form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = carInfoTab;
				var carInfoPanel = invoiceLineUserControl.FindSingle<ZPanel>("CarInfoPanel");
				AssertEquals(true, carInfoPanel.Visible);
			}
		}

		public void TestShowLineCalculationsControls()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			Factory.Save();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
			var jobDeclarationUserControl = (JobDeclarationUserControl)brokerageControl?.DeclarationUserControlForTesting;
			if (jobDeclarationUserControl != null)
			{
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				var controls = brokerageControl.InvoiceLinesUserControl.Controls;
				AssertType<ImportInvoiceLineUserControl>(brokerageControl.InvoiceLinesUserControl);
				AssertEquals("Show Freight Amount(JI_Calc_FreightInInvoiceCurr) is true", false, controls.Find("JI_Calc_FreightConvertToLocalCurrencyControl", true)[0].Visible);
				AssertEquals("Show Insurance Amount(JI_Calc_InsuranceInInvoiceCur) is true", false, controls.Find("JI_Calc_InsuranceConvertToLocalCurrencyControl", true)[0].Visible);
				AssertEquals("Show Duty(JI_Calc_DutyAmountIncludingWHEstimate) is false", false, controls.Find("JI_Calc_DutyConvertToLocalCurrencyControl", true)[0].Visible);
				AssertEquals("Show VAT(JI_Calc_GSTVATAmountIncludingWHEstimate) is false", false, controls.Find("JI_Calc_GSTConvertToLocalCurrencyControl", true)[0].Visible);
				AssertEquals("Show CIF(JI_Calc_CIF) is false", false, controls.Find("JI_Calc_CIFConvertToLocalCurrencyControl", true)[0].Visible);
				AssertEquals("Show FOB Value(JI_Calc_FOB) is false", false, controls.Find("JI_Calc_FOBConvertToLocalCurrencyControl", true)[0].Visible);
			}
		}

		public void TestChineseDescVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = "IMP";
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var twOtherDetailsTabPage = invoiceLineUserControl.FindSingle<ZTabPage>("TWOtherDetailsTabPage");
			form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = twOtherDetailsTabPage;
			AssertEquals(true, twOtherDetailsTabPage.TabVisible);
		}

		public void TestControlOutsideBoundsOfParent()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			TestControlOutsideBoundsOfParentForImport(jobDeclartion, true);
			TestControlOutsideBoundsOfParentForImport(jobDeclartion, false);
		}

		void TestControlOutsideBoundsOfParentForImport(JobDeclaration jobDeclartion, bool smallArea)
		{
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLinesUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
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
			});
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
			var outsideTheBounds = true;
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
			var result = false;
			var toolBar = control as ToolBar;
			if (toolBar != null && toolBar.Parent != null)
			{
				var allowableSpace = toolBar.Height - toolBar.ButtonSize.Height;
				result = toolBar.Bottom >= toolBar.Parent.Height + allowableSpace;
			}

			return result;
		}

		public void TestRAPVisibilityAndCaption()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("DutiesTaxesAndFeesTabPage");
			var controlTW_RAPRORPriceConvertToLocalCurrencyControl = invoiceLineUserControl.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TW_RAPRORPriceConvertToLocalCurrencyControl");
			var controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl = invoiceLineUserControl.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl");
			var controlRAPRORGroupBox = invoiceLineUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RAPRORGroupBox");
			AssertEquals(false, controlTW_RAPRORPriceConvertToLocalCurrencyControl.Visible);
			AssertEquals(false, controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Visible);
			AssertEquals(false, controlRAPRORGroupBox.Visible);
			var rapList = new string[] { "39", "3F", "37" };
			foreach (var item in rapList)
			{
				line.JI_Procedure = item;
				AssertEquals(true, controlTW_RAPRORPriceConvertToLocalCurrencyControl.Visible);
				AssertEquals(true, controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Visible);
				AssertEquals(true, controlRAPRORGroupBox.Visible);

				AssertEquals("RAP Price", controlTW_RAPRORPriceConvertToLocalCurrencyControl.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("RAP Unit Price", controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Repair/Assembly/Processing", controlRAPRORGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestRORVisibilityAndCaption()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("DutiesTaxesAndFeesTabPage");
			var controlTW_RAPRORPriceConvertToLocalCurrencyControl = invoiceLineUserControl.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "TW_RAPRORPriceConvertToLocalCurrencyControl");
			var controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl = invoiceLineUserControl.FindSingleOrDefault<ZCalcDropEdit>(c => c.Name == "JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl");
			var controlRAPRORGroupBox = invoiceLineUserControl.FindSingleOrDefault<ZGroupBox>(c => c.Name == "RAPRORGroupBox");
			AssertEquals(false, controlTW_RAPRORPriceConvertToLocalCurrencyControl.Visible);
			AssertEquals(false, controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Visible);
			AssertEquals(false, controlRAPRORGroupBox.Visible);

			var rorList = new string[] { "38", "3E" };
			foreach (var item in rorList)
			{
				line.JI_Procedure = item;

				AssertEquals("ROR Price", controlTW_RAPRORPriceConvertToLocalCurrencyControl.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("ROR Unit Price", controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Rental/Royalty", controlRAPRORGroupBox.CaptionResourceString.Caption);

				AssertEquals(true, controlTW_RAPRORPriceConvertToLocalCurrencyControl.Visible);
				AssertEquals(true, controlJI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Visible);
				AssertEquals(true, controlRAPRORGroupBox.Visible);
			}
		}

		public void TestCVCheckBoxVisibility()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			form.CustomsBrokerageUserControl.InvoiceLinesUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingle<ZTabPage>("DutiesTaxesAndFeesTabPage");
			var controlJI_UseOneTenthCVCheckBox = invoiceLineUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "JI_UseOneTenthCVCheckBox");
			AssertEquals(false, controlJI_UseOneTenthCVCheckBox.Visible);

			var cvList = new string[] { "39", "37" };
			foreach (var item in cvList)
			{
				line.JI_Procedure = item;
				AssertEquals(true, controlJI_UseOneTenthCVCheckBox.Visible);
			}

			line.JI_Procedure = "XX";
			AssertEquals(false, controlJI_UseOneTenthCVCheckBox.Visible);
		}

		public void TestRAPPriceColumnsGroupName()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.JI_Procedure = Constants.ProcedureCodes._37;
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			customsInvoiceLinesBoundGrid.ResetColumns();

			var calc_RAPRORUnitPriceColumnStyle = customsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_RAPRORUnitPrice);
			var rapCurrColumnStyle = customsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RAPCurr);
			CombineAssertions(() =>
			{
				AssertEquals("RAP/ROR Unit Price", calc_RAPRORUnitPriceColumnStyle.GroupName.Caption);
				AssertEquals("RAP/ROR Unit Price", rapCurrColumnStyle.GroupName.Caption);
			});
		}

		public void TestDefaultColumnsInSortOrder()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var customsInvoiceLinesBoundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			customsInvoiceLinesBoundGrid.ResetColumns();
			var totalColumns = customsInvoiceLinesBoundGrid.Columns;
			AssertEquals(165, totalColumns.Count);
			var visibleColumnCount = customsInvoiceLinesBoundGrid.Columns.Where(x => x.IsVisible).Count();
			CombineAssertions(() =>
			{
				AssertEquals(22, visibleColumnCount);
				for (var i = 0; i < ExpectedDefaultColumnsForGrid.Count; i++)
				{
					var column = customsInvoiceLinesBoundGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedDefaultColumnsForGrid[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			});
		}

		List<string> ExpectedDefaultColumnsForGrid
		{
			get
			{
				var columns = new List<string>
				{
					Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice,
					JobComInvoiceLine.Schema.InvoiceHeaderDisplaySequence,
					JobComInvoiceLineSchema.Constants.JI_LineNo,
					JobComInvoiceLineSchema.Constants.JI_PartNo,
					JobComInvoiceLine.Schema.JI_FormattedTariff,
					JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin,
					JobComInvoiceLineSchema.Constants.JI_PrimaryPreference,
					JobComInvoiceLineSchema.Constants.JI_Procedure,
					JobComInvoiceLine.Schema.JI_Group,
					JobComInvoiceLineSchema.Constants.JI_Description,
					JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity,
					JobComInvoiceLineSchema.Constants.JI_InvoiceUQ,
					JobComInvoiceLine.Schema.JI_EnteredUnitPrice,
					JobComInvoiceLineSchema.Constants.JI_LinePrice,
					JobComInvoiceLineSchema.Constants.JI_NetWeight,
					JobComInvoiceLineSchema.Constants.JI_NetWeightUQ,
					JobComInvoiceLineSchema.Constants.JI_CustomsSecondQuantity,
					JobComInvoiceLineSchema.Constants.JI_CustomsSecondUnitQty,
					JobComInvoiceLineSchema.Constants.JI_BrandName,
					JobComInvoiceLine.Schema.TWL_DocumentaryQty,
					JobComInvoiceLine.Schema.TWL_DocumentaryUQ,
					JobComInvoiceLine.Schema.TWL_DocumentaryUnitPrice
				};
				return columns;
			}
		}

		public void TestCustomsInvoiceLinesBoundGridColumns()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			var columns = boundGrid.ColumnStyles;
			CombineAssertions(() =>
			{
				AssertHasColumn(columns, JobComInvoiceLine.Schema.JI_ConcessionOrder);
				AssertHasColumn(columns, JobComInvoiceLine.Schema.QuotaPermitNumber, "Tariff Rate Quota Certificate");
				AssertHasColumn(columns, JobComInvoiceLine.Schema.QuotaPermitNumberItemNumber, "Tariff Rate Quota Certificate");
				AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_AircraftPartsCategory, "CAA Code");
				AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_AircraftPartsCode, "CAA Code");
				AssertHasColumn(columns, JobComInvoiceLine.Schema.TWL_AircraftIPC);
			});
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn, string groupName = "")
		{
			var anyColumnHasGivenName = string.IsNullOrEmpty(groupName) ? columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn) : columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn && column.GroupName.Caption == groupName);
			Assert($"Should have the column - {nameOfColumn} with group name - {groupName}", anyColumnHasGivenName);
		}

		public void TestLineDetailsUserControl()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var lineDetails = invoiceLineUserControl.FindSingle<LineDetailsUserControl>("LineDetailsUserControl");
			AssertType(typeof(ImportLineDetailsUserControl), lineDetails);
		}

		public void TestImportDetailsTariffFindBox()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var importDetailsTabPage = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "ImportDetailsTabPage");
			var importDetails = invoiceLineUserControl.FindSingleOrDefault<ImportDetailsUserControl>(c => c.Name == "ImportDetailsUserControl");
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = importDetailsTabPage;
			AssertEquals("HSN", importDetails.JI_TariffFindBox.GetTariffType());
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsStyles()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			using var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var boundGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			boundGrid.ResetColumns();

			CombineAssertions(() =>
			{
				var formattedDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "FormattedAdValoremDutyRate") as ZTextBoxColumnStyleInfo;
				AssertEquals(true, formattedDutyRateStyle.IsReadOnly);
				AssertEquals(false, formattedDutyRateStyle.IsVisible);

				var formattedSpecificDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "FormattedSpecificDutyRate") as ZTextBoxColumnStyleInfo;
				AssertEquals(true, formattedSpecificDutyRateStyle.IsReadOnly);
				AssertEquals(false, formattedSpecificDutyRateStyle.IsVisible);

				var antiDumpingDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_AntiDumpingDutyRate") as ZCalcEditColumnStyleInfo;
				AssertEquals(false, antiDumpingDutyRateStyle.IsVisible);

				var countervailingDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_CountervailingDutyRate") as ZCalcEditColumnStyleInfo;
				AssertEquals(false, countervailingDutyRateStyle.IsVisible);

				var additionalDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_AdditionalDutyRate") as ZCalcEditColumnStyleInfo;
				AssertEquals(false, additionalDutyRateStyle.IsVisible);

				var retaliatoryDutyRateStyle = boundGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "JI_RetaliatoryDutyRate") as ZCalcEditColumnStyleInfo;
				AssertEquals(false, retaliatoryDutyRateStyle.IsVisible);
			});
		}

		public void TestSetJI_DescriptionWhenTWL_AircraftPartsCodeChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACodeCategory, "TaiwanAircraftPartCAACodeCategory");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACodeCategory, "3", "第六類：航空器化學及油漆材料", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACode, "TaiwanAircraftPartCAACode");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACode, "3.2", "Aircraft Antenna Equipment & Parts", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACode, "3.3a", "a Amplifier", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanAircraftPartCAACode, "3.3b", "b Battery-Dry cell or Storage(with Battery Terminal)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			line.JI_DescriptionInfo.ClearValue();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var twLine = line.AddInfoChild;
			twLine.TWL_AircraftPartsCategory = "3";
			twLine.TWL_AircraftPartsCode = "2";
			var expectedMessage = "Aircraft Parts English description must be declared. Do you want to override the existing English description with the Aircraft Parts English description?";
			CombineAssertions(() =>
			{
				AssertNotContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Aircraft Antenna Equipment & Parts", line.JI_Description);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			twLine.TWL_AircraftPartsCode = "3b";
			CombineAssertions(() =>
			{
				AssertEquals("b Battery-Dry cell or Storage(with Battery Terminal)", line.JI_Description);
				AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			twLine.TWL_AircraftPartsCode = "3a";
			AssertEquals("b Battery-Dry cell or Storage(with Battery Terminal)", line.JI_Description);
		}
	}
}
