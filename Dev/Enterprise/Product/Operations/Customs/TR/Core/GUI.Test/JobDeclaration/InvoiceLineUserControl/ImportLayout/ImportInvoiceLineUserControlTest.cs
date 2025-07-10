using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineUserControl))]
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestControlBindingMember()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				var target = control.FindSingle<ConvertToLocalCurrencyControl>("StatisticalValueLocalCurrencyControl");
				AssertEquals("FilteredInvoiceLines.JI_StatisticalValueUSD", target.BindToAmount);
				AssertEquals("FilteredInvoiceLines.JI_CurrencyUSD", target.BindToUnit);
			}
		}

		public void TestGetInvoiceLineVehicleUserControlType()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(InvoiceLineVehicleUserControl), control.GetInvoiceLineVehicleUserControlType_Exposed());
			}
		}

		public void TestInitializeContainerGridLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var userControl = form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					var grid = userControl.ContainersTabPage.FindSingle<ZGrid>("CusContainerInvoiceLineGrid");
					InvoiceLineUserControlHelperTest.AssertInitializeContainerGridLayout(grid);
				}
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		#region #region Visibility & Ordering
		public void TestFieldInvisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.InvoiceLines.AddNew();

			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				CombineAssertions("Below assertions are for the visibilities with false!", () =>
				{
					AssertEquals("Valuation Method invisible", false, control.ValuationMethodDropEdit.Visible);
					AssertEquals("Valuation Adjustment Code invisible", false, control.ValuationAdjustmentCodeDropEdit.Visible);
					AssertEquals("Valuation Adjustment Percentage invisible", false, control.ValuationAdjustmentPercentageCalcEdit.Visible);
				});
			}
		}

		public void TestFieldVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				var mediumTaxTypeDropEdit = control.Controls.Find("TaxTypeDropEdit", true)[0] as ZDropEdit;
				Assert(mediumTaxTypeDropEdit.Visible);
			}
		}

		public void TestInvoiceLineDetailsPanelLayout()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				var layout = control.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
				var importLayout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, importLayout.IncludedControls);
			}
		}

		public void TestInitializeGridLayout_ColumnVisiblityAndOrder()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var columnNamesInSortOrder = new string[]
				{
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_LineNo,
					Customs.Business.BaseJobComInvoiceLine.Schema.JI_Calc_Invoice,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CEI,
					EU.Business.Declaration.JobComInvoiceLine.Schema.EntryInstructionDescription,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_PartNo,
					Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_Description,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_NDescription,
					EU.Business.Declaration.JobComInvoiceLine.Schema.JI_FormattedProcedure,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_InvoiceQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CountryOfOrigin,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_PrimaryPreference,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice,
					EU.Business.Declaration.JobComInvoiceLine.Schema.JI_SupplementaryCode1,
					EU.Business.Declaration.JobComInvoiceLine.Schema.JI_SupplementaryCode2,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
					EU.Business.Declaration.JobComInvoiceLine.Schema.EntryReferenceNumber,
					Customs.Business.BaseJobComInvoiceLine.Schema.MergedLineNumber,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_BondedWhsUnitQty
				};

				control.InitializeGridLayout();
				var visibleGridColumns = control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => x.IsVisible).Select(x => x.ColumnName).ToArray();
				CombineAssertions(() =>
				{
					for (var i = 0; i < visibleGridColumns.Length; i++)
					{
						var columnToCheck = visibleGridColumns[i];
						AssertEquals($"The column name '{columnToCheck}' is out of order or not visible", columnNamesInSortOrder[i], columnToCheck);
					}
				});
			}
		}

		public void TestEntryExitPurposeCodeAndDescriptionVisibilty()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "4000";

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;

				var entryExitPurposeCodeDropEdit = control.FindSingle<ZDropEdit>(nameof(EU.GUI.InvoiceLineDetailsControlBag.EntryExitPurposeCodeDropEdit));
				var entryExitPurposeDetailTextBox = control.FindSingle<ZTextBox>(nameof(EU.GUI.InvoiceLineDetailsControlBag.EntryExitPurposeDetailTextBox));

				CombineAssertions("Entry/Exit Purpose Fields", () =>
				{
					AssertEquals("Prcodure 4000 | entryExitPurposeCodeDropEdit NOT Visible", entryExitPurposeCodeDropEdit.Visible, false);
					AssertEquals("Prcodure 4000 | Purpose Empty | entryExitPurposeDetailTextBox NOT Visible", entryExitPurposeDetailTextBox.Visible, false);

					invoiceLine.JI_FormattedProcedure = "2100";
					AssertEquals("Prcodure 2100 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 2100 | Purpose Empty | entryExitPurposeDetailTextBox NOT Visible", entryExitPurposeDetailTextBox.Visible, false);

					invoiceLine.ZG_EntryExitPurposeCode = "01";
					AssertEquals("Prcodure 2100 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 2100 | Purpose 01 | entryExitPurposeDetailTextBox NOT Visible", entryExitPurposeDetailTextBox.Visible, false);

					invoiceLine.ZG_EntryExitPurposeCode = "05";
					AssertEquals("Prcodure 2100 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 2100 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "3151";
					AssertEquals("Prcodure 3151 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 3151 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "5100";
					AssertEquals("Prcodure 5100 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 5100 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "5171";
					AssertEquals("Prcodure 5171 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 5171 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "6121";
					AssertEquals("Prcodure 6121 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 6121 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "6321";
					AssertEquals("Prcodure 6321 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 6321 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);

					invoiceLine.JI_FormattedProcedure = "6771";
					AssertEquals("Prcodure 6771 | entryExitPurposeCodeDropEdit Visible", entryExitPurposeCodeDropEdit.Visible, true);
					AssertEquals("Prcodure 6771 | Purpose 05 | entryExitPurposeDetailTextBox Visible", entryExitPurposeDetailTextBox.Visible, true);
				});
			}
		}

		public void TestTabPagesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(declaration, ZString.Empty);
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				var tabPages = control.LineDetailTabControl.TabPages;
				AssertArrayEqualsByElements("The order of Invoice Line Tab page is equal to hte predefined order", new[] { "NewLineDetailsTabPage", "LineChargesTabPage", "InvoiceLineTaxTabPage", "AviationFuelTypeTabPage",
					"SupportingDocumentsTabPage", "AdditionalInfosTabPage", "PreviousDocumentsTabPage", "PackagesPivotTabPage", "InvoiceLinePaymentTabPage", "VehicleTabPage", "DangerousGoodsTabPage", "CustomFieldsTabPage" },
					tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
			}
		}
		#endregion

		public void TestDynamicLayoutApplied()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.DynamicLayoutApplied_Exposed);
			}
		}

		public void TestAdditionalInfosTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControl));
		}

		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			{
				using (var control = new ImportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					CombineAssertions("Below assertions are for the captions for necessary controls!", () =>
					{
						AssertEquals("Deferred VAT", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "JI_Calc_GSTVATDeferredConvertToLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
						AssertEquals("Statistical Value", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "StatisticalValueLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					});
				}
			}
		}
	}

	sealed class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public new ZDropEdit ValuationMethodDropEdit => base.ValuationMethodDropEdit;

		public new ZDropEdit ValuationAdjustmentCodeDropEdit => base.ValuationAdjustmentCodeDropEdit;

		public new ZCalcEdit ValuationAdjustmentPercentageCalcEdit => base.ValuationAdjustmentPercentageCalcEdit;

		public ZBool DynamicLayoutApplied_Exposed => DynamicLayoutApplied;

		public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => GetNewInvoiceLineDetailsPanelLayout();

		public Type GetInvoiceLineVehicleUserControlType_Exposed() => GetInvoiceLineVehicleUserControlType();

		public Type GetAdditionalInfosUserControlType_Exposed() => GetAdditionalInfosUserControlType();
	}
}
