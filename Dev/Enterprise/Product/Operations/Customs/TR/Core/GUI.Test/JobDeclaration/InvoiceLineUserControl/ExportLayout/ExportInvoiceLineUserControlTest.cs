using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
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

		public void TestStatisticalValueLocalCurrencyControlCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			{
				using (var control = new ExportInvoiceLineUserControl())
				{
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					CombineAssertions("Below assertions are for EXP & MISC!", () =>
					{
						AssertEquals("Statistical Value", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "StatisticalValueLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);

						declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;

						AssertEquals("Statistical Value", control.FindSingle<ConvertToLocalCurrencyControl>(x => x.Name == "StatisticalValueLocalCurrencyControl").GetExtension<ILabelCaptionRenderer>().Caption);
					});
				}
			}
		}

		public void TestGetInvoiceLineVehicleUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(InvoiceLineVehicleUserControl), control.GetInvoiceLineVehicleUserControlType_Exposed());
			}
		}

		public void TestInitializeContainerGridLayout()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
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

		#region Visibility & Ordering
		public void TestAviationFuelTypeTabPageVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				var permitsTabPage = control.FindSingle<ZTabPage>("AviationFuelTypeTabPage");
				AssertEquals("Aviation Fuel Type Tab Page is visible", true, permitsTabPage.TabVisible);
			}
		}

		public void TestInitializeGridLayout_ColumnVisiblityAndOrder()
		{
			using (var control = new ExportInvoiceLineUserControl())
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
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_LinePrice,
					EU.Business.Declaration.JobComInvoiceLine.Schema.JI_SupplementaryCode1,
					EU.Business.Declaration.JobComInvoiceLine.Schema.JI_SupplementaryCode2,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					Customs.Business.AutoJobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					EU.Business.Declaration.JobComInvoiceLine.Schema.EntryReferenceNumber,
					Customs.Business.BaseJobComInvoiceLine.Schema.MergedLineNumber,
					EU.Business.Declaration.AutoJobComInvoiceLine.Schema.ZG_CusNumber
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
					"SupportingDocumentsTabPage", "AdditionalInfosTabPage", "PreviousDocumentsTabPage", "PackagesPivotTabPage", "InvoiceLinePaymentTabPage", "VehicleTabPage", "DangerousGoodsTabPage", "CustomFieldsTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
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
		#endregion

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestDynamicLayoutApplied()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(true, control.DynamicLayoutApplied_Exposed);
			}
		}

		public void TestInvoiceLineDetailsPanelLayout()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				var layout = control.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
				var exportLayout = ((IPanelLayoutProvider)new ExportInvoiceLineDetailsLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, exportLayout.IncludedControls);
			}
		}

		public void TestGetAdditionalInfosUserControlType()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertEquals(typeof(AdditionalInfosUserControl), control.GetAdditionalInfosUserControlType_Exposed());
			}
		}
	}

	sealed class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
	{
		public ZBool DynamicLayoutApplied_Exposed => DynamicLayoutApplied;

		public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => GetNewInvoiceLineDetailsPanelLayout();

		public Type GetInvoiceLineVehicleUserControlType_Exposed() => GetInvoiceLineVehicleUserControlType();

		public Type GetAdditionalInfosUserControlType_Exposed() => GetAdditionalInfosUserControlType();
	}
}
