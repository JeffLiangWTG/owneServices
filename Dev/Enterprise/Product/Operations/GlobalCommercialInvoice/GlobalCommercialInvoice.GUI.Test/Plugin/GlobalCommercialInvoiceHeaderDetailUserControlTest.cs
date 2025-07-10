using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoiceHeaderDetailUserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			using var control = new GlobalCommercialInvoiceHeaderDetailUserControl();
			CombineAssertions("User control type:", () =>
			{
				AssertType<ZTextBox>("Inv.No", control.Controls.Find("InvoiceNumberTextBox", true).Single());
				AssertType<ZDateEdit>("Inv.Date", control.Controls.Find("InvoiceDateDateEdit", true).Single());
				AssertType<ZCalcFindBox>("Inv.Amount", control.Controls.Find("InvoiceAmountCurrencyControl", true).Single());
				AssertType<ZTextBox>("Inv.Description", control.Controls.Find("InvoiceDescriptionTextBox", true).Single());
				AssertType<ZCodeFindBox>("Inv.Export", control.Controls.Find("InvoiceExportCountryBoundFindBox", true).Single());
				AssertType<ZCodeFindBox>("Inv.Import", control.Controls.Find("InvoiceImportCountryBoundFindBox", true).Single());
				AssertType<ZCodeFindBox>("Inv.Origin", control.Controls.Find("InvoiceGoodsOriginBoundFindBox", true).Single());
				AssertType<ZOrgAddressControl>("Inv.Importer", control.Controls.Find("InvoiceImporterOrgAddressControl", true).Single());
				AssertType<ZOrgAddressControl>("Inv.Supplier", control.Controls.Find("InvoiceSupplierOrgAddressControl", true).Single());
			});
		}

		public void TestUserControlCaptions()
		{
			using var control = new GlobalCommercialInvoiceHeaderDetailUserControl();
			CombineAssertions("User control caption:", () =>
			{
				Assert("Inv.No", control.Controls.Find("InvoiceNumberTextBox", true).OfType<ZTextBox>().Any(c => c.CaptionResourceString.Caption == "Invoice No."));
				Assert("Inv.Date", control.Controls.Find("InvoiceDateDateEdit", true).OfType<ZDateEdit>().Any(c => c.CaptionResourceString.Caption == "Invoice Date"));
				Assert("Inv.Amount", control.Controls.Find("InvoiceAmountCurrencyControl", true).OfType<ZCalcFindBox>().Any(c => c.CaptionResourceString.Caption == "Invoice Total"));
				Assert("Inv.Description", control.Controls.Find("InvoiceDescriptionTextBox", true).OfType<ZTextBox>().Any(c => c.CaptionResourceString.Caption == "Goods Description"));
				Assert("Inv.Export", control.Controls.Find("InvoiceExportCountryBoundFindBox", true).OfType<ZCodeFindBox>().Any(c => c.CaptionResourceString.Caption == "Ctry/Rgn. Of Export"));
				Assert("Inv.Import", control.Controls.Find("InvoiceImportCountryBoundFindBox", true).OfType<ZCodeFindBox>().Any(c => c.CaptionResourceString.Caption == "Ctry/Rgn. Of Import"));
				Assert("Inv.Origin", control.Controls.Find("InvoiceGoodsOriginBoundFindBox", true).OfType<ZCodeFindBox>().Any(c => c.CaptionResourceString.Caption == "Goods Origin"));
				Assert("Inv.Importer", control.Controls.Find("InvoiceImporterOrgAddressControl", true).OfType<ZOrgAddressControl>().Any(c => c.CaptionResourceString.Caption == "Importer"));
				Assert("Inv.Supplier", control.Controls.Find("InvoiceSupplierOrgAddressControl", true).OfType<ZOrgAddressControl>().Any(c => c.CaptionResourceString.Caption == "Supplier"));
			});
		}
	}
}
