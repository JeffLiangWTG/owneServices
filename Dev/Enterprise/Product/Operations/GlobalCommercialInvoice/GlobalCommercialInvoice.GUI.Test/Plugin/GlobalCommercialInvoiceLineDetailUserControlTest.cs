using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoiceLineDetailUserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			using var control = new GlobalCommercialInvoiceLineDetailUserControl();
			CombineAssertions("User control type:", () =>
			{
				AssertType<ZGuidDropEdit>("Inv.No", control.Controls.Find("InvoiceNumberGuidDropEdit", true).Single());
				AssertType<ZCodeFindBox>("Inv.Product Code", control.Controls.Find("InvoiceProductCodeFindBox", true).Single());
				AssertType<TariffFindBox>("Inv.Tariff1", control.Controls.Find("InvoiceTariff1FindBox", true).Single());
				AssertType<TariffFindBox>("Inv.Tariff2", control.Controls.Find("InvoiceTariff2FindBox", true).Single());
				AssertType<ZTextBox>("Inv.Goods Description", control.Controls.Find("InvoiceGoodsDescriptionTextBox", true).Single());
				AssertType<ZCalcEdit>("Inv.Price", control.Controls.Find("InvoiceLinePriceCalcEdit", true).Single());
				AssertType<ZCalcDropEdit>("Inv.Net Weight", control.Controls.Find("InvoiceNetWeightCalcDropEdit", true).Single());
				AssertType<ZCalcDropEdit>("Inv.Gross Weight", control.Controls.Find("InvoiceGrossWeightCalcDropEdit", true).Single());
				AssertType<ZCalcDropEdit>("Inv.Volume", control.Controls.Find("InvoiceVolumeCalcDropEdit", true).Single());
				AssertType<ZCalcDropEdit>("Inv.Qty", control.Controls.Find("InvoiceQuantityCalcDropEdit", true).Single());
				AssertType<ZTextBox>("Inv.Price Currency", control.Controls.Find("InvoiceLinePriceCurrencyTextBox", true).Single());
			});
		}

		public void TestUserControlCaptions()
		{
			using var control = new GlobalCommercialInvoiceLineDetailUserControl();
			CombineAssertions("User control caption:", () =>
			{
				Assert("Inv.No", control.Controls.Find("InvoiceNumberGuidDropEdit", true).OfType<ZGuidDropEdit>().Any(c => c.CaptionResourceString.Caption == "Invoice No."));
				Assert("Inv.Product Code", control.Controls.Find("InvoiceProductCodeFindBox", true).OfType<ZCodeFindBox>().Any(c => c.CaptionResourceString.Caption == "Product Code"));
				Assert("Inv.Tariff1", control.Controls.Find("InvoiceTariff1FindBox", true).OfType<TariffFindBox>().Any(c => c.CaptionResourceString.Caption == "Tariff 1"));
				Assert("Inv.Tariff2", control.Controls.Find("InvoiceTariff2FindBox", true).OfType<TariffFindBox>().Any(c => c.CaptionResourceString.Caption == "Tariff 2"));
				Assert("Inv.Goods Description", control.Controls.Find("InvoiceGoodsDescriptionTextBox", true).OfType<ZTextBox>().Any(c => c.CaptionResourceString.Caption == "Goods Description"));
				Assert("Inv.Price", control.Controls.Find("InvoiceLinePriceCalcEdit", true).OfType<ZCalcEdit>().Any(c => c.CaptionResourceString.Caption == "Price"));
				Assert("Inv.Net Weight", control.Controls.Find("InvoiceNetWeightCalcDropEdit", true).OfType<ZCalcDropEdit>().Any(c => c.CaptionResourceString.Caption == "Net Weight"));
				Assert("Inv.Gross Weight", control.Controls.Find("InvoiceGrossWeightCalcDropEdit", true).OfType<ZCalcDropEdit>().Any(c => c.CaptionResourceString.Caption == "Gross Weight"));
				Assert("Inv.Volume", control.Controls.Find("InvoiceVolumeCalcDropEdit", true).OfType<ZCalcDropEdit>().Any(c => c.CaptionResourceString.Caption == "Volume"));
				Assert("Inv.Qty", control.Controls.Find("InvoiceQuantityCalcDropEdit", true).OfType<ZCalcDropEdit>().Any(c => c.CaptionResourceString.Caption == "Invoice Qty"));
				Assert("Inv.Price Currency", control.Controls.Find("InvoiceLinePriceCurrencyTextBox", true).OfType<ZTextBox>().Any(c => c.CaptionResourceString.Caption == null));
			});
		}
	}
}
