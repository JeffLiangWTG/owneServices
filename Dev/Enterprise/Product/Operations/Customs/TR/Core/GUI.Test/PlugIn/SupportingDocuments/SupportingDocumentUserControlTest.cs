using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class SupportingDocumentUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(SupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(SupportingDocumentsUserControl), typeof(SupportingDocumentsFieldsControl));
		}

		public void TestGridColumns()
		{
			SupportingDocumentsControlTestHelper.AssertGridColumns(Factory, typeof(SupportingDocumentsUserControl), OrderedColumnNamesAndColumnStyleTypes);
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(SupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(135, panel.MinimumSize.Height);
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			var collection = new SupportingDocumentCollection(supportingDocument);
			using (var control = new SupportingDocumentsUserControl())
			{
				var grid = control.SupportingDocumentsGrid;
				grid.SetDataBinding(collection, "");
				control.Show();
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_CodeDescription: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle("CSI_CodeDescription").CharacterCasing);
					AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Status).CharacterCasing);
					AssertEquals("CSI_DateOfIssue: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue)).DateTimeFormat);
					AssertEquals("CSI_DateOfExpiry: Format", ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry)).DateTimeFormat);
				});
			}
		}

		IEnumerable<(string ColumnName, Type ColumnType)> OrderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
		{
			(SupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyle)),
			("CSI_CodeDescription", typeof(ZTextBoxColumnStyle)),
			(SupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZMultiControlColumnStyle)),
			(SupportingDocument.Schema.CSI_Status, typeof(ZDropEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyle)),
			(SupportingDocument.Schema.CSI_DateOfExpiry, typeof(ZDateEditColumnStyle)),
		};
	}
}
