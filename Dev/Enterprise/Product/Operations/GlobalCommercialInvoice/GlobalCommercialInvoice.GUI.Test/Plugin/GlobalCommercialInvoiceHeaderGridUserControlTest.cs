using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoiceHeaderGridUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnTypes()
		{
			var grid = GetInvoiceHeaderGridUserControl().InvoiceHeaderCollectionGrid;

			CombineAssertions("Grid column type:", () =>
			{
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceNumber));
				AssertType<ZDateEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceDate));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceAmount));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RX_NKInvoiceCurrency));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_Description));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryImport));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryExport));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryOrigin));
				AssertType<ZOrganisationFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_OH_Importer));
				AssertType<ZOrganisationFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_OH_Supplier));
			});
		}

		public void TestGridColumnCaptions()
		{
			var grid = GetInvoiceHeaderGridUserControl().InvoiceHeaderCollectionGrid;

			CombineAssertions("Grid column caption:", () =>
			{
				AssertEquals("Invoice No.", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceNumber).CaptionResourceString.Caption);
				AssertEquals("Inv. Date", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceDate).CaptionResourceString.Caption);
				AssertEquals("Invoice Total", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_InvoiceAmount).CaptionResourceString.Caption);
				AssertEquals("Curr.", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RX_NKInvoiceCurrency).CaptionResourceString.Caption);
				AssertEquals("Goods Description", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_Description).CaptionResourceString.Caption);
				AssertEquals("Ctry/Rgn. Of Export", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryExport).CaptionResourceString.Caption);
				AssertEquals("Ctry/Rgn. Of Import", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryImport).CaptionResourceString.Caption);
				AssertEquals("Goods Origin", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_RN_NKCountryOrigin).CaptionResourceString.Caption);
				AssertEquals("Importer", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_OH_Importer).CaptionResourceString.Caption);
				AssertEquals("Supplier", grid.GetColumnStyle(AutoGlobalCommercialInvoiceHeader.Schema.GIH_OH_Supplier).CaptionResourceString.Caption);
			});
		}

		public void TestGridBaseControlAndType()
		{
			var gridUserControl = GetInvoiceHeaderGridUserControl();

			AssertEquals($"{nameof(GlobalCommercialInvoiceHeaderGridUserControl)} should be inherited from {nameof(ZUserControl)}",
				typeof(ZUserControl), gridUserControl.GetType().BaseType);

			AssertEquals($"{nameof(GlobalCommercialInvoiceHeaderGridUserControl)} should have dock style {nameof(DockStyle.Fill)}",
				DockStyle.Fill,	gridUserControl.Dock);

			AssertEquals($"The grid should be of type {nameof(ZGrid)}",
				typeof(ZGrid), gridUserControl.InvoiceHeaderCollectionGrid.GetType());

			AssertEquals($"The grid should have dock style {nameof(DockStyle.Fill)}",
				DockStyle.Fill,	gridUserControl.InvoiceHeaderCollectionGrid.Dock);
		}

		GlobalCommercialInvoiceHeaderGridUserControl GetInvoiceHeaderGridUserControl()
		{
			using var plugin = new GlobalCommercialInvoicePlugin((IBusiness)Factory.CreateNewShipment());
			var splitContainerPanel = ((GlobalCommercialInvoicePluginUserControl)plugin.UserControl).InvoiceHeaderSplitContainer.Panel1;
			return (GlobalCommercialInvoiceHeaderGridUserControl)splitContainerPanel.Controls[nameof(GlobalCommercialInvoiceHeaderGridUserControl)];
		}
	}
}
