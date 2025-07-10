using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestsSubclassesOf(typeof(BaseInvoiceLineUserControl))]
abstract class BaseInvoiceLineUserControlTest<T> : TestCaseWithFactory where T : BaseInvoiceLineUserControl
{
	protected record ColumnProperties(string columnName)
	{
		public static implicit operator ColumnProperties(string columnName) => new(columnName);
	}

	public void TestTabPages_Visible()
	{
		CombineAssertions(() =>
		{
			InvLineUserControl.AssertContainsControl<ZTabPage>("NewLineDetailsTabPage", x => x.WithTabVisible(isActiveTab: true));
			InvLineUserControl.AssertContainsControl<ZTabPage>("LineChargesTabPage", x => x.WithTabVisible(isActiveTab: false));
			InvLineUserControl.AssertContainsControl<ZTabPage>("SupportingDocumentsTabPage", x => x.WithTabVisible(isActiveTab: false));
			InvLineUserControl.AssertContainsControl<ZTabPage>("CustomFieldsTabPage", x => x.WithTabVisible(isActiveTab: false));
			InvLineUserControl.AssertContainsControl<ZTabPage>("PackagesPivotTabPage", x => x.WithTabVisible(isActiveTab: false));
		});
	}

	public void TestInvoiceLineGrid_JI_Preference_AvailableNotVisible()
	{
		var column = InvLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode);
		CombineAssertions(() =>
		{
			AssertNotNull(column);
			AssertEquals(expected: false, column.IsVisible);
		});
	}

	public void TestInvoiceLineGrid_MergedLineNumber()
	{
		var column = InvLineUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.MergedLineNumber);
		CombineAssertions(() =>
		{
			AssertNotNull(column);
			AssertEquals("Merged Ln. #", column.CaptionResourceString.Caption);
			AssertEquals("Width", 80, column.Width);
		});
	}

	public void TestInvoiceLinesSummaryGroupBoxCaption()
	{
		CombineAssertions(() =>
		{
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_LinePriceInLocalCurrencyControl", x => x
				.WithCaption("Inv. Line Value")
				.WithFullDescription("Invoice line value for current line item.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_LinePriceInLocalCurrency"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_Calc_FreightConvertToLocalCurrencyControl", x => x
				.WithCaption("Freight")
				.WithFullDescription("Freight Charges for current line item. (On invoices is in foreign currencies minor rounding issues may be seen.)")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_Calc_FreightInLocalCurrency"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_Calc_InsuranceConvertToLocalCurrencyControl", x => x
				.WithCaption("Insurance")
				.WithFullDescription("Insurance charges for current line item. (On invoices is in foreign currencies minor rounding issues may be seen.)")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_Calc_InsuranceInLocalCurrency"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("TotalOtherChargesInNOKControl", x => x
				.WithCaption("Other Charges")
				.WithFullDescription("Other charges for current line item.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.TotalOtherChargesInNOK"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("TotalDeductionsInNOKControl", x => x
				.WithCaption("Deductions")
				.WithFullDescription("Total deductions for current line item.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.TotalDeductionsInNOK"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_Calc_CIFConvertToLocalCurrencyControl", x => x
				.WithCaption("CIF Value")
				.WithFullDescription("CIF value for current item line. CIF value is invoice line value plus(/minus) charges. It is value at time of border crossing, and that is the basis for calculating customs and excise duties.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_Calc_CIF_InLocalCurrency"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_Calc_DutyAmountIncludingWHEstimateControl", x => x
				.WithCaption("Customs Duty")
				.WithFullDescription("Customs duties for current line item.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_Calc_DutyAmountIncludingWHEstimate"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("TotalExciseDutiesControl", x => x
				.WithCaption("Excise Duties")
				.WithFullDescription("Excise duties for current line item.")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.TotalExciseDuties"));
			InvLineUserControl.AssertContainsControl<ConvertToLocalCurrencyControl>("JI_Calc_GSTConvertToLocalCurrencyControl", x => x
				.WithCaption("VAT Amount")
				.WithFullDescription("VAT value for current line item")
				.WithBindToUnit("FilteredInvoiceLines.JI_RX_LocalCurrency")
				.WithBindToAmount("FilteredInvoiceLines.JI_Calc_GSTVATAmountIncludingWHEstimate"));

			AssertNull("JI_Calc_FOBConvertToLocalCurrencyControl", InvLineUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>("JI_Calc_FOBConvertToLocalCurrencyControl"));
		});
	}

	public void TestBottomPanel_HasExpectedMinimumSize()
	{
		CombineAssertions(() =>
		{
			var expectedSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1151, 381, true);
			var bottomPanel = InvLineUserControl
				.FindAll<ZPanel>(panel => panel.Name == "BottomPanel")
				.First();
			var actualSize = bottomPanel.MinimumSize;

			AssertEquals("BottomPanel.MinimumSize.Width", expectedSize.Width, actualSize.Width);
			AssertEquals("BottomPanel.MinimumSize.Height", expectedSize.Height, actualSize.Height);
		});
	}

	public void TestInvoiceLineGrid_DefaultColumns()
	{
		var columns = InvLineUserControl.CustomsInvoiceLinesBoundGrid.Columns;
		CombineAssertions(() =>
		{
			var i = 0;
			foreach (var x in ExpectedInvoiceLineGrid_DefaultColumns)
			{
				AssertDefaultColumn(x.columnName, columns, i++);
			}
		});
	}

	protected abstract IEnumerable<ColumnProperties> ExpectedInvoiceLineGrid_DefaultColumns { get; }

	public void TestInvoiceLineGrid_AllColumns()
	{
		var invLinesGrid = InvLineUserControl.CustomsInvoiceLinesBoundGrid;
		CombineAssertions(() =>
		{
			foreach (var x in ExpectedInvoiceLineGrid_DefaultColumns)
			{
				AssertNotNull($"Column {x.columnName}", invLinesGrid.GetColumnStyle(x.columnName));
			}
		});
	}

	protected abstract IEnumerable<ColumnProperties> ExpectedInvoiceLineGrid_AllColumns { get; }

	protected void AssertDefaultColumn(string expectedName, ZGridColumns columns, int index)
	{
		var column = columns[index];
		AssertEquals(index.ToString(), expectedName, column.ColumnStyle.MappingName);
		AssertEquals(expectedName, expected: true, column.IsVisible);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = GetJE_MessageType();
		declaration.Invoices.AddNew();
		decForm = new JobDeclarationForm(declaration);
		decForm.Show();
		decForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = decForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
		InvLineUserControl = (T)decForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
	}

	protected override void TearDown()
	{
		decForm?.Dispose();
		base.TearDown();
	}

	JobDeclarationForm decForm;
	protected T InvLineUserControl { get; private set; }

	protected abstract ZString GetJE_MessageType();
}
