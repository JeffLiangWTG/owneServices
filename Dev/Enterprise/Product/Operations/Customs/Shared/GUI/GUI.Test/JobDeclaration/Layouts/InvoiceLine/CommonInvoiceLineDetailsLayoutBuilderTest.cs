using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonInvoiceLineDetailsLayoutBuilder<BaseJobComInvoiceLine>))]
	sealed class CommonInvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CommonInvoiceLineDetailsLayoutBuilder<BaseJobComInvoiceLine>, BaseJobComInvoiceLine, CommonInvoiceLineDetailsControlBag>
	{
		public void TestEntryInstructionGuidDropEdit_Visibility()
		{
			var (nonPersistantinvoiceLine, invoiceLine) = GetInvoiceLines();
			CombineAssertions(() =>
			{
				AssertEquals("Persistant", expected: true, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, invoiceLine));
				AssertEquals("Non Persistant", expected: false, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, nonPersistantinvoiceLine));
			});
		}

		public void TestBondedWHSOrder_Visibility()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoiceLineOK = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLineKO = dec.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLineOK.JI_BondedWHSOrderNumber = "No";
			Assert("Prerequisite: invoiceLineKO", !invoiceLineKO.IsBondedWHSOrderNumberVisible);
			Assert("Prerequisite: invoiceLineOK", invoiceLineOK.IsBondedWHSOrderNumberVisible);

			CombineAssertions(() =>
			{
				AssertEquals("BondedWHSOrderLineNumberCalcEdit non visible", expected: false, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLineKO));
				AssertEquals("BondedWHSOrderNumberTextBox non visible", expected: false, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLineKO));

				AssertEquals("BondedWHSOrderLineNumberCalcEdit visible", expected: true, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderLineNumberCalcEdit, invoiceLineOK));
				AssertEquals("BondedWHSOrderNumberTextBox visible", expected: true, layout.IsVisible(CommonInvoiceLineDetailsControlBag.Instance.BondedWHSOrderNumberTextBox, invoiceLineOK));
			});
		}

		public void TestPreviousEntryNumberTextBox_Visibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineForVisibleControl = invoice.JobComInvoiceLines.AddNew();
			invoiceLineForVisibleControl.JI_BondedWhsQuantity = 1;
			var invoiceLineForInvisibleControl = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Prerequisite: invoiceLineForVisibleControl.IsPreviousEntryNumberVisible", expected: true, invoiceLineForVisibleControl.IsPreviousEntryNumberVisible);
			AssertEquals("Prerequisite: invoiceLineForInvisibleControl.IsPreviousEntryNumberVisible", expected: false, invoiceLineForInvisibleControl.IsPreviousEntryNumberVisible);
			CombineAssertions(() =>
			{
				var previousEntryNumberTextBox = CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox;
				AssertEquals("PreviousEntryNumberTextBox visible", expected: true, layout.IsVisible(previousEntryNumberTextBox, invoiceLineForVisibleControl));
				AssertEquals("PreviousEntryNumberTextBox not visible", expected: false, layout.IsVisible(previousEntryNumberTextBox, invoiceLineForInvisibleControl));
				var expectedVisibilityDependencies = GetBondedWhsQtyAndPreviousEntryNumberVisibilityDependencies(invoiceLineForVisibleControl);
				AssertContainsExactElementsInAnyOrder("PreviousEntryNumberTextBox visibility dependency", expectedVisibilityDependencies, layout.GetVisibilityDependencies(previousEntryNumberTextBox, invoiceLineForVisibleControl));
			});
		}

		public void TestPreviousEntryLineNumberCalcEdit_Visibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineForVisibleControl = invoice.JobComInvoiceLines.AddNew();
			invoiceLineForVisibleControl.JI_BondedWhsQuantity = 1;
			var invoiceLineForInvisibleControl = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Prerequisite: invoiceLineForVisibleControl.IsPreviousEntryNumberVisible", expected: true, invoiceLineForVisibleControl.IsPreviousEntryNumberVisible);
			AssertEquals("Prerequisite: invoiceLineForInvisibleControl.IsPreviousEntryNumberVisible", expected: false, invoiceLineForInvisibleControl.IsPreviousEntryNumberVisible);
			CombineAssertions(() =>
			{
				var previousEntryLineNumberCalcEdit = CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit;
				AssertEquals("PreviousEntryLineNumberCalcEdit visible", expected: true, layout.IsVisible(previousEntryLineNumberCalcEdit, invoiceLineForVisibleControl));
				AssertEquals("PreviousEntryLineNumberCalcEdit not visible", expected: false, layout.IsVisible(previousEntryLineNumberCalcEdit, invoiceLineForInvisibleControl));
				var expectedVisibilityDependencies = GetBondedWhsQtyAndPreviousEntryNumberVisibilityDependencies(invoiceLineForVisibleControl);
				AssertContainsExactElementsInAnyOrder("PreviousEntryLineNumberCalcEdit visibility dependency", expectedVisibilityDependencies, layout.GetVisibilityDependencies(previousEntryLineNumberCalcEdit, invoiceLineForVisibleControl));
			});
		}

		public void TestBondedWhsQuantityCalcDropEdit_Visibility()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLineForVisibleControl = invoice.JobComInvoiceLines.AddNew();
			invoiceLineForVisibleControl.JI_BondedWhsQuantity = 1;
			var invoiceLineForInvisibleControl = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("Prerequisite: invoiceLineForVisibleControl.IsBondedWhsQuantityVisible", expected: true, invoiceLineForVisibleControl.IsBondedWhsQuantityVisible);
			AssertEquals("Prerequisite: invoiceLineForInvisibleControl.IsBondedWhsQuantityVisible", expected: false, invoiceLineForInvisibleControl.IsBondedWhsQuantityVisible);
			CombineAssertions(() =>
			{
				var bondedWhsQuantityCalcDropEdit = CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit;
				AssertEquals("BondedWhsQuantityCalcDropEdit visible", expected: true, layout.IsVisible(bondedWhsQuantityCalcDropEdit, invoiceLineForVisibleControl));
				AssertEquals("BondedWhsQuantityCalcDropEdit not visible", expected: false, layout.IsVisible(bondedWhsQuantityCalcDropEdit, invoiceLineForInvisibleControl));
				var expectedVisibilityDependencies = GetBondedWhsQtyAndPreviousEntryNumberVisibilityDependencies(invoiceLineForVisibleControl);
				AssertContainsExactElementsInAnyOrder("BondedWhsQuantityCalcDropEdit visibility dependency", expectedVisibilityDependencies, layout.GetVisibilityDependencies(bondedWhsQuantityCalcDropEdit, invoiceLineForVisibleControl));
			});
		}

		protected override int ExpectedMaxColumns => 3;

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override CommonInvoiceLineDetailsLayoutBuilder<BaseJobComInvoiceLine> GetColumnLayoutBuilderForTesting() => new CommonInvoiceLineDetailsLayoutBuilder<BaseJobComInvoiceLine>();

		(BaseJobComInvoiceLine NonPersistantinvoiceLine, BaseJobComInvoiceLine InvoiceLine) GetInvoiceLines()
		{
			var nonPersistantDeclaration = Factory.New<BaseJobDeclaration>();
			nonPersistantDeclaration.MakeNonPersistent();
			var nonPersistantinvoiceLine = nonPersistantDeclaration.Invoices.AddNew().InvoiceLines.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return (nonPersistantinvoiceLine, invoiceLine);
		}

		PanelLayout layout => ((IPanelLayoutProvider)new CommonInvoiceLineDetailsLayouts()).Layout;

		IEnumerable<ZPropertyInfo> GetBondedWhsQtyAndPreviousEntryNumberVisibilityDependencies(BaseJobComInvoiceLine invoiceLine) => new[] { invoiceLine.JI_ProcedureInfo, invoiceLine.JI_PreviousEntryNumberInfo, invoiceLine.JI_PreviousEntryLineNumberInfo, invoiceLine.JI_BondedWhsQuantityInfo };
	}
}
