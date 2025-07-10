using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderUserControl))]
	sealed class InvoiceHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<InvoiceHeaderUserControl, JobDeclaration>
	{
		JobDeclarationForm decForm;
		BaseCustomsSupplierHeaderUserControl invHeaderForm;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });

		public void TestBottomPanel()
		{
			CombineAssertions(() =>
			{
				invHeaderForm.LeftBottomPanel.AssertContainsControl<ZTextBox>("IncoTermTextBox", x => x
					.WithBindTo($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.IncoTerm)}")
				);
				invHeaderForm.LeftBottomPanel.AssertContainsControl<ConvertToLocalCurrencyControl>("LineTotalBoundConvertToLocalCurrencyControl", x => x
					.WithBindToAmount($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.InvoiceLineTotal)}")
					.WithBindToUnit($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.InvoiceLineTotalCurrency)}")
				);
				invHeaderForm.RightBottomPanel.AssertContainsControl<ConvertToLocalCurrencyControl>("JZ_FOBAmountBoundCurrencyControl", x => x
					.WithBindToAmount($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)}")
					.WithBindToUnit($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)}")
				);
				invHeaderForm.RightBottomPanel.AssertDoesNotContainsControl("JZ_CIFAmountBoundCurrencyControl");
				invHeaderForm.RightBottomPanel.AssertDoesNotContainsControl("JZ_Calc_TNIBoundInvoiceCurrencyControl");
				invHeaderForm.RightBottomPanel.AssertContainsControl<ConvertToLocalCurrencyControl>("JZ_Calc_ChargesAmountBoundInvoiceCurrencyControl", x => x
					.WithCaption("Charges")
					.WithBindToAmount($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.JZ_Calc_ChargesAmount)}")
					.WithBindToUnit($"{nameof(JobDeclaration.Invoices)}.{nameof(JobComInvoiceHeader.JZ_Calc_ChargesCurrency)}")
				);
			});
		}

		public void TestInvoiceHeaderGrid_DefaultColumns()
		{
			var invHeaderGrid = invHeaderForm.InvoiceHeadersBoundGrid;
			CombineAssertions(() =>
			{
				var i = 0;
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_InvoiceNumber, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_InvoiceDate, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_InvoiceAmount, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_IncoTerm, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_IncoTermPlace, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_ValuationCode, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_ValuationMethod, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_OH_Supplier, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.InvoiceLineTotal, invHeaderGrid, i++);
				AssertDefaultColumn(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString, invHeaderGrid, i++);
			});
			void AssertDefaultColumn(string expectedName, BaseInvoiceArrayBoundGrid grid, int index)
			{
				var column = grid.Columns[index];
				AssertEquals(index.ToString(), expectedName, column.ColumnStyle.MappingName);
				AssertEquals(expectedName, true, column.IsVisible);
			}
		}

		public void TestInvoiceHeaderGrid_AllColumns()
		{
			var invHeaderGrid = invHeaderForm.InvoiceHeadersBoundGrid;
			CombineAssertions(() =>
			{
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_InvoiceNumber, 80);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_InvoiceDate, 90);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_InvoiceAmount, 90);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency, 50);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_IncoTerm, 50);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_IncoTermPlace, 125);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_ValuationCode, 95);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_ValuationMethod, 95);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_OH_Supplier, 80);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.InvoiceLineTotal, 100);
				AssertColumnWidth(invHeaderGrid, JobComInvoiceHeader.Schema.JZ_Calc_BalanceString, 120);
			});
			void AssertColumnWidth(BaseInvoiceArrayBoundGrid grid, string columnName, int width)
			{
				AssertEquals(columnName, grid.GetColumnStyle(columnName).Width, width);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			decForm = new JobDeclarationForm(declaration);
			decForm.Show();
			decForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = decForm.CustomsBrokerageUserControl.InvoicesTabPage;
			invHeaderForm = decForm.CustomsBrokerageUserControl.SupplierHeaderUserControl;
		}

		protected override void TearDown()
		{
			decForm?.Dispose();
			base.TearDown();
		}
	}
}
