using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class SupportingDocumentsUserControlTest : EU.GUI.PlugIn.Testing.SupportingDocumentsUserControlTest
{
	protected override IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
	{
		(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
		("CSI_CodeDescription", typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
		(SupportingDocument.Schema.CSI_Quantity, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_UnitOfQuantity, typeof(ZDropEditColumnStyle)),
		(SupportingDocument.Schema.CSI_Value, typeof(ZCalcEditColumnStyle)),
		(SupportingDocument.Schema.CSI_RX_NKCurrency, typeof(ZCodeFindBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
		(SupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyle)),
		(SupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyle)),
	};

	public new void TestGridColumnStyleProperties()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		var collection = new SupportingDocumentCollection(supportingDocument);
		using (var control = (SupportingDocumentsUserControl)Activator.CreateInstance(SupportingDocumentsUserControlType))
		{
			var grid = control.SupportingDocumentsGrid;
			grid.SetDataBinding(collection, "");
			control.Show();

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
				AssertEquals("CSI_DateOfExpiry: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry)).DateTimeFormat);
				AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_RX_NKCurrency).CharacterCasing);
				AssertEquals("CSI_Value: Decimals", 5, ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value)).Decimals);
				var csi_Quantity = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity);
				AssertEquals("CSI_Quantity: Decimals", 5, csi_Quantity.Decimals);
				AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", csi_Quantity.BindToDecimalPlaces);
				var csi_ReferenceNumber = (ZMultiControlColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber: FieldTypeColumnName", "ReferenceNumberFieldType", csi_ReferenceNumber.FieldTypeColumnName);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber.CharacterCasing);
				var csi_UnitOfQuantity = grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
				if (csi_UnitOfQuantity is ZMultiControlColumnStyleInfo multiControlColumnStyleInfo)
				{
					AssertEquals("CSI_UnitOfQuantity: FieldTypeColumnName", "UnitOfQuantityFieldType", multiControlColumnStyleInfo.FieldTypeColumnName);
				}
				AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, csi_UnitOfQuantity.CharacterCasing);
			});
		}
	}

	protected override Type SupportingDocumentsUserControlType => typeof(NLSupportingDocumentsUserControl);

	protected override Type SupportingDocumentsFieldsControlType => typeof(SupportingDocumentsFieldsControl);
}
