using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Printing.Testing
{
	public abstract class WhsDocumentPrinterTest : TestCaseWithFactory
	{
		#region TestPrintDocument

		public void TestPrintDocument()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			WhsDocumentPrinter.LastPrintedDocumentName = "";
			AssertEquals(ZString.Empty, WhsDocumentPrinter.LastPrintedDocumentName);

			AssertEquals(true, DocumentPrinter.PrintDocument());
			AssertEquals(GetExpectedDocumentsToPrint(), DocumentPrinter.LastDocumentRunner.LastRunReportInfosForTesting.Count);
			AssertEquals(GetExpectedMenuItemName(), WhsDocumentPrinter.LastPrintedDocumentName);
		}

		protected abstract ZString GetExpectedMenuItemName();
		protected abstract ZInt GetExpectedDocumentsToPrint();

		#endregion

		#region TestPrintDocument_TryingSaveFactoryWithDocumentCommandShouldNotSaveParent

		public void TestPrintDocument_TryingSaveFactoryWithDocumentCommandShouldNotSaveParent()
		{
			var printResult = DocumentPrinter.PrintDocument();
			AssertEquals("Precondition: Document should be able to print.", true, printResult);
			AssertEquals("Precondition: Parent should not be in DB.", false, ((BusinessObject)Parent).IsInDatabase);

			DocumentPrinter.DocumentCommandForTest.Factory.Save();
			AssertEquals("After doing a factory save in Document command's factory, parent should not have any changes.", false, ((BusinessObject)Parent).IsInDatabase);
			AssertNotEquals("BizO and document commands should not be in same factory.", DocumentPrinter.DocumentCommandForTest.Factory, ((BusinessObject)Parent).Factory);
		}

		#endregion

		#region TestDocumentMenuPK

		public void TestDocumentMenuPK()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var docBuilderMenuItem = DocumentPrinter.GetDocumentCommand();
			AssertNotNull(docBuilderMenuItem);
			AssertEquals(GetExpectedDocBuilderMenuItemPK(), docBuilderMenuItem.PK);

			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var legacyMenuItem = DocumentPrinter.GetDocumentCommand();
			AssertNotNull(legacyMenuItem);
			AssertEquals(GetExpectedLegacyMenuItemPK(), legacyMenuItem.PK);
		}

		protected abstract ZGuid GetExpectedDocBuilderMenuItemPK();

		protected abstract ZGuid GetExpectedLegacyMenuItemPK();

		#endregion

		#region Implementation

		#region Parent

		protected IDocumentSupportable Parent
		{
			get { return parent ?? (parent = GetNewParentObject()); }
		}

		protected IDocumentSupportable parent;

		protected abstract IDocumentSupportable GetNewParentObject();

		#endregion

		#region DocumentPrinter

		protected WhsDocumentPrinter DocumentPrinter
		{
			get { return documentPrinter ?? (documentPrinter = GetNewDocumentPrinter()); }
		}

		WhsDocumentPrinter documentPrinter;

		protected abstract WhsDocumentPrinter GetNewDocumentPrinter();

		#endregion

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
