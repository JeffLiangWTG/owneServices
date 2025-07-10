using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibility_OnlyRequiredInImports()
		{
			testDec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			using (var form = new ZForm(testDec))
			{
				using (var testUserControl = new ZAInvoiceHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = testDec;
					testUserControl.SetDataBinding(testDec, "");
					form.Show();
					var valuationCodeDropEdit = (ZDropEdit)testUserControl.Controls.Find("ValuationCodeDropEdit", true)[0];
					var relatedIndicatorDropEdit = (ZDropEdit)testUserControl.Controls.Find("RelatedIndicatorDropEdit", true)[0];
					var vDNTextBox = (ZTextBox)testUserControl.Controls.Find("VDNTextBox", true)[0];
					var vBMPercentCalcEdit = (ZCalcEdit)testUserControl.Controls.Find("VBMPercentCalcEdit", true)[0];
					AssertEquals("Require ValuationCodeDropEdit in imports", true, valuationCodeDropEdit.Visible);
					AssertEquals("Require RelatedIndicatorDropEdit in imports", true, relatedIndicatorDropEdit.Visible);
					AssertEquals("Require VDNTextBox in imports", true, vDNTextBox.Visible);
					AssertEquals("Require VBMPercentCalcEdit in imports", true, vBMPercentCalcEdit.Visible);
				}
			}

			testDec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			using (var form = new ZForm(testDec))
			{
				using (var testUserControl = new ZAInvoiceHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = testDec;
					testUserControl.SetDataBinding(testDec, "");
					form.Show();
					var valuationCodeDropEdit = (ZDropEdit)testUserControl.Controls.Find("ValuationCodeDropEdit", true)[0];
					var relatedIndicatorDropEdit = (ZDropEdit)testUserControl.Controls.Find("RelatedIndicatorDropEdit", true)[0];
					var vDNTextBox = (ZTextBox)testUserControl.Controls.Find("VDNTextBox", true)[0];
					var vBMPercentCalcEdit = (ZCalcEdit)testUserControl.Controls.Find("VBMPercentCalcEdit", true)[0];
					AssertEquals("remove ValuationCodeDropEdit from form when Shipment type is not imports", false, valuationCodeDropEdit.Visible);
					AssertEquals("remove RelatedIndicatorDropEdit from form when Shipment type is not imports", false, relatedIndicatorDropEdit.Visible);
					AssertEquals("remove VDNTextBox from form when Shipment type is not imports", false, vDNTextBox.Visible);
					AssertEquals("remove VBMPercentCalcEdit from form when Shipment type is not imports", false, vBMPercentCalcEdit.Visible);
				}
			}
		}

		public void TestRemoveGSTApplicableFromInvoiceChargeAndApportionedCharge()
		{
			using (var form = new ZForm(testDec))
			{
				using (ZAInvoiceHeaderUserControl testUserControl = new ZAInvoiceHeaderUserControl())
				{
					form.Controls.Add(testUserControl);
					testUserControl.JobDeclaration = testDec;
					testUserControl.SetDataBinding(testDec, "");
					var column = testUserControl.InvoiceChargesGrid.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable];
					AssertNull("GST applicable is not relevant for ZA", column);
					column = testUserControl.ApportionedChargesGrid.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable];
					AssertNull("GST applicable is not relevant for ZA", column);
					column = testUserControl.BaseGroupChargesGrid.Columns[InvoiceCharge.Schema.J7_IsGSTApplicable];
					AssertNull("GST applicable is not relevant for ZA", column);
					AssertNotNull(testUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_VDN));
					AssertNotNull(testUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_ValuationMarkup));
				}
			}
		}

		public void TestColumnsAreAddedToInvoiceGrid()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: false))
			using (var form = new ZForm(testDec))
			using (var userControl = new ZAInvoiceHeaderUserControl())
			{
				form.Controls.Add(userControl);
				userControl.JobDeclaration = testDec;
				userControl.SetDataBinding(testDec, "");
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin, isVisible: false, 106, typeof(ZCodeFindBoxColumnStyle));
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_ROOCert, isVisible: false, 121, typeof(ZTextBoxColumnStyle));
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_PaymentNo, isVisible: true, 80, typeof(ZTextBoxColumnStyle));

				var columnStyle = userControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_PaymentTerms);
				AssertNull($"Column: {JobComInvoiceHeader.Schema.JZ_PaymentTerms}", columnStyle);
			}
		}

		public void TestColumnsAreAddedToInvoiceGrid_AddInvoiceDetailsToCUSDECMessageEnabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, value: true))
			using (var form = new ZForm(testDec))
			using (var userControl = new ZAInvoiceHeaderUserControl())
			{
				form.Controls.Add(userControl);
				userControl.JobDeclaration = testDec;
				userControl.SetDataBinding(testDec, "");
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_RN_NKDefaultOrigin, isVisible: false, 106, typeof(ZCodeFindBoxColumnStyle));
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_ROOCert, isVisible: false, 121, typeof(ZTextBoxColumnStyle));
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_PaymentNo, isVisible: true, 80, typeof(ZTextBoxColumnStyle));
				AssertColumnExistsWithRightDetails(userControl.InvoiceHeadersBoundGrid, JobComInvoiceHeader.Schema.JZ_PaymentTerms, isVisible: true, 90, typeof(ZDropEditColumnStyle));
			}
		}

		public void TestRemoveImporterColumnFromInvoiceHeaderGrid()
		{
			using (var form = new ZForm(testDec))
			using (var userControl = new ZAInvoiceHeaderUserControl())
			{
				form.Controls.Add(userControl);
				userControl.JobDeclaration = testDec;
				userControl.SetDataBinding(testDec, "");
				var column = userControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_OH_Buyer];
				AssertNull("Invoice Header Importer is not relevant for ZA", column);
			}
		}

		public void TestGridId()
		{
			using (var control = new ZAInvoiceHeaderUserControl())
			{
				AssertEquals("GridLayout7zaGfUuf5rM2V1f8hiQdQg==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		void AssertColumnExistsWithRightDetails(BaseInvoiceArrayBoundGrid grid, ZString columnName, bool isVisible, int width, Type columnStyleType)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertNotNull($"Column: {columnName}", columnStyle);
			CombineAssertions(columnName, () =>
			{
				AssertEquals("IsVisible", isVisible, columnStyle.IsVisible);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnStyle.Width);
				AssertEquals("Column Style", columnStyleType, columnStyle.ColumnStyleType);
			});
		}

		JobDeclaration testDec;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
		}
	}
}
