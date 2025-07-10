using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Warehouse.Environment.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Printing.Testing
{
	public class WhsPackingSlipDocumentPrinterTest : WhsDocumentPrinterTest
	{
		#region TestUseLegacyVersion

		public void TestUseLegacyVersion()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var documentPrinter = new WhsPackingSlipDocumentPrinterForTest((WhsPickableDocket)Parent, Factory);

			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition", true, DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value);
			AssertEquals("UseLegacyVersion should be the inverse UseNewDocBuilderWarehouseDocumentsOnly", false, documentPrinter.UseLegacyVersion_Exposed());

			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition", false, DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value);
			AssertEquals("UseLegacyVersion should be the inverse UseNewDocBuilderWarehouseDocumentsOnly", true, documentPrinter.UseLegacyVersion_Exposed());
		}

		class WhsPackingSlipDocumentPrinterForTest : WhsPackingSlipDocumentPrinter
		{
			public WhsPackingSlipDocumentPrinterForTest(WhsPickableDocket parent, BusinessObjectFactory factory)
				: base(parent, new TestNotificationBuffer())
			{
			}

			public ZBool UseLegacyVersion_Exposed()
			{
				return this.UseLegacyVersion;
			}
		}

		#endregion

		#region TestDocumentMenuPK

		protected override ZGuid GetExpectedDocBuilderMenuItemPK()
		{
			return new ZGuid("2a0c8863-f502-46dd-b606-7af730aa6317");
		}

		protected override ZGuid GetExpectedLegacyMenuItemPK()
		{
			return new ZGuid("29271EEF-8C08-4E4A-8309-CE9F8A0AD3A5");
		}

		#endregion

		#region Implementation

		protected override ZString GetExpectedMenuItemName()
		{
			return new ZString("Packing Slip");
		}

		protected override ZInt GetExpectedDocumentsToPrint()
		{
			return 1;
		}

		protected override IDocumentSupportable GetNewParentObject()
		{
			return Factory.New<WhsOrder>();
		}

		protected override WhsDocumentPrinter GetNewDocumentPrinter()
		{
			return new WhsPackingSlipDocumentPrinter((WhsOrder)Parent, new TestNotificationBuffer());
		}

		#endregion
	}
}
