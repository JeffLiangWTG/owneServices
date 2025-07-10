using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.GUI;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoiceLineGridUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnTypes()
		{
			var grid = GetInvoiceLineGridUserControl().InvoiceLineCollectionGrid;

			CombineAssertions("Grid column type:", () =>
			{
				AssertType<ZGuidDropEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GIH_Header));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_LineNo));
				AssertType<ZCodeFindBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Product));
				AssertType<TariffColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Tariff1));
				AssertType<TariffColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Tariff2));
				AssertType<ZTextBoxColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Description));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_InvoiceQuantity));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_InvoiceUQ));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Volume));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_VolumeUQ));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GrossWeight));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GrossWeightUQ));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_NetWeight));
				AssertType<ZDropEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_NetWeightUQ));
				AssertType<ZCalcEditColumnStyleInfo>(grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_LinePrice));
			});
		}

		public void TestGridColumnCaptions()
		{
			var grid = GetInvoiceLineGridUserControl().InvoiceLineCollectionGrid;

			CombineAssertions("Grid column caption:", () =>
			{
				AssertEquals("Invoice No.", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GIH_Header).CaptionResourceString.Caption);
				AssertEquals("Inv. Line #", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_LineNo).CaptionResourceString.Caption);
				AssertEquals("Product Code", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Product).CaptionResourceString.Caption);
				AssertEquals("Tariff 1", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Tariff1).CaptionResourceString.Caption);
				AssertEquals("Tariff 2", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Tariff2).CaptionResourceString.Caption);
				AssertEquals("Goods Description", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Description).CaptionResourceString.Caption);
				AssertEquals("Invoice Qty", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_InvoiceQuantity).CaptionResourceString.Caption);
				AssertEquals("UQ", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_InvoiceUQ).CaptionResourceString.Caption);
				AssertEquals("Volume", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_Volume).CaptionResourceString.Caption);
				AssertEquals("UQ", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_VolumeUQ).CaptionResourceString.Caption);
				AssertEquals("Gross Weight", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GrossWeight).CaptionResourceString.Caption);
				AssertEquals("UQ", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_GrossWeightUQ).CaptionResourceString.Caption);
				AssertEquals("Net Weight", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_NetWeight).CaptionResourceString.Caption);
				AssertEquals("UQ", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_NetWeightUQ).CaptionResourceString.Caption);
				AssertEquals("Price", grid.GetColumnStyle(AutoGlobalCommercialInvoiceLine.Schema.GIL_LinePrice).CaptionResourceString.Caption);
			});
		}

		public void TestGridBaseControlAndType()
		{
			var gridUserControl = GetInvoiceLineGridUserControl();

			AssertEquals($"{nameof(GlobalCommercialInvoiceLineGridUserControl)} should be inherited from {nameof(ZUserControl)}",
				typeof(ZUserControl), gridUserControl.GetType().BaseType);

			AssertEquals($"{nameof(GlobalCommercialInvoiceLineGridUserControl)} should have dock style {nameof(DockStyle.Fill)}",
				DockStyle.Fill,	gridUserControl.Dock);

			AssertEquals($"The grid should be of type {nameof(ZGrid)}",
				typeof(ZGrid), gridUserControl.InvoiceLineCollectionGrid.GetType());

			AssertEquals($"The grid should have dock style {nameof(DockStyle.Fill)}",
				DockStyle.Fill,	gridUserControl.InvoiceLineCollectionGrid.Dock);
		}

		GlobalCommercialInvoiceLineGridUserControl GetInvoiceLineGridUserControl()
		{
			var shipment = Factory.CreateNewShipment();
			Factory.CreateInvoiceHeader((BusinessObject)shipment); // The header is required to create the line grid.
			using var plugin = new GlobalCommercialInvoicePlugin((IBusiness)shipment);
			var splitContainerPanel = ((GlobalCommercialInvoicePluginUserControl)plugin.UserControl).InvoiceLineSplitContainer.Panel1;
			return (GlobalCommercialInvoiceLineGridUserControl)splitContainerPanel.Controls[nameof(GlobalCommercialInvoiceLineGridUserControl)];
		}
	}
}
