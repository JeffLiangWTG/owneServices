using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class LayoutSupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestGetSupportingDocumentsFieldsControl()
	{
		using (var control = new LayoutSupportingDocumentsUserControl())
		{
			var declaration = Factory.New<JobDeclaration>();
			control.SetDataBinding(declaration, null);
			AssertType<LayoutSupportingDocumentsFieldsControl>(control.SupportingDocumentsFieldsControl);
		}
	}

	public void TestSupportingDocumentsFieldsControlBindingString() => CombineAssertions(() =>
	{
		using (var control = new LayoutSupportingDocumentsUserControlForTest())
		{
			control.SupportingDocumentsGrid.DataMember = "";
			AssertEquals("DataMember is empty", string.Empty, control.GetSupportingDocumentsFieldsControlBindingStringExposed);

			var testBinding = nameof(JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.SupportingDocuments);
			control.SupportingDocumentsGrid.DataMember = testBinding;
			AssertEquals("DataMember is not empty", expected: testBinding, control.GetSupportingDocumentsFieldsControlBindingStringExposed);
		}
	});

	public void TestSupportingDocumentsGridColumns() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.SupportingDocuments.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.SupportingDocuments.AddNew();
		invoice.InvoiceLines.AddNew().SupportingDocuments.AddNew();

		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

		AssertColumns(declaration, nameof(JobDeclaration.CustomsEntryInstructions), OrderedColumnsImport);
		AssertColumns(declaration, nameof(JobDeclaration.Invoices), OrderedColumnsImport);
		AssertColumns(declaration, nameof(JobDeclaration.FilteredInvoiceLines), OrderedColumnsImportInvoiceLine);

		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;

		AssertColumns(declaration, nameof(JobDeclaration.CustomsEntryInstructions), OrderedColumnsExport);
		AssertColumns(declaration, nameof(JobDeclaration.Invoices), OrderedColumnsExport);
		AssertColumns(declaration, nameof(JobDeclaration.FilteredInvoiceLines), OrderedColumnsExport);

		void AssertColumns(JobDeclaration declaration, string bindingMember, IEnumerable<(string ColumnName, Type ColumnType)> columns)
		{
			using (var form = new ZForm())
			using (var userControl = new LayoutSupportingDocumentsUserControl())
			{
				new ControlRebinder().Rebind(userControl, nameof(JobDeclaration.FilteredInvoiceLines), bindingMember);
				form.Controls.Add(userControl);
				form.Show();
				userControl.SetDataBinding(declaration, null);

				var grid = userControl.SupportingDocumentsGrid;
				var description = $"{declaration.JE_MessageType} - {bindingMember}.";

				AssertEquals($"{description}. Columns count", columns.Count(), grid.Columns.Count);
				var num = 0;
				foreach (var orderedColumnNamesAndColumnStyleType in columns)
				{
					var columnName = orderedColumnNamesAndColumnStyleType.ColumnName;
					var item = orderedColumnNamesAndColumnStyleType.ColumnType;
					var zGridColumn = grid.Columns.SingleOrDefault((ZGridColumn x) => x.ColumnName == columnName);
					if (zGridColumn == null)
					{
						Assert($"{description}. Column: {columnName} does not exists", condition: false);
					}
					else
					{
						Type type = zGridColumn.ColumnStyle.GetType();
						AssertEquals($"{description}. Column: {columnName} exists at expected position '{num}'", columnName, grid.Columns[num].ColumnName);
						AssertEquals($"{description}. Column: {columnName} columnStyleType", item, type);
						AssertEquals($"{description}. Column: {columnName} visible", true, zGridColumn.IsVisible);
					}

					num = checked(num + 1);
				}
			}
		}
	});

	IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnsImportInvoiceLine => new[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_CodeDescription, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_Description, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle))
	};

	IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnsImport => new[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_CodeDescription, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_Description, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle))
	};

	IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnsExport => new[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_AdditionalDescription, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
		(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
		(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle))
	};

	class LayoutSupportingDocumentsUserControlForTest : LayoutSupportingDocumentsUserControl
	{
		public string GetSupportingDocumentsFieldsControlBindingStringExposed => base.GetSupportingDocumentsFieldsControlBindingString();
	}
}
