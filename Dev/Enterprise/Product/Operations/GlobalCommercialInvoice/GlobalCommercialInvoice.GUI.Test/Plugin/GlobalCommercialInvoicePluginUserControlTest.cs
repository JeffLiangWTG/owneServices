using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.GlobalCommercialInvoice.Business;
using Enterprise.GlobalCommercialInvoice.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.GlobalCommercialInvoice.GUI.Test
{
	public class GlobalCommercialInvoicePluginUserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			var shipment = Factory.CreateNewShipment();
			var pluginBizO = new GlobalCommercialInvoicePluginBusinessObject((BusinessObject)shipment);
			using var control = new GlobalCommercialInvoicePluginUserControl(pluginBizO);
			CombineAssertions("User control type:", () =>
			{
				AssertType<GlobalCommercialInvoiceHeaderGridUserControl>("Inv.Header Grid", control.Controls.Find("GlobalCommercialInvoiceHeaderGridUserControl", true).Single());
				AssertType<GlobalCommercialInvoiceHeaderDetailUserControl>("Inv.Header Detail", control.Controls.Find("GlobalCommercialInvoiceHeaderDetailUserControl", true).Single());
				AssertType<GlobalCommercialInvoiceLineGridUserControl>("Inv.Line Grid", control.Controls.Find("GlobalCommercialInvoiceLineGridUserControl", true).Single());
				AssertType<ZLabel>("Inv.Line Visibility Label", control.Controls.Find("InvoiceLineUserControlsVisibilityLabel", true).Single());
				AssertType<GlobalCommercialInvoiceLineDetailUserControl>("Inv.Line Detail", control.Controls.Find("GlobalCommercialInvoiceLineDetailUserControl", true).Single());
			});
		}

		public void TestUserControlTabCaptions()
		{
			var shipment = Factory.CreateNewShipment();
			var pluginBizO = new GlobalCommercialInvoicePluginBusinessObject((BusinessObject)shipment);
			using var control = new GlobalCommercialInvoicePluginUserControl(pluginBizO);
			CombineAssertions("User control caption:", () =>
			{
				var tabControl = (ZTemplateTabControl)control.Controls.Find("InvoiceMainTabControl", true).Single();
				Assert("Inv.No", tabControl.Controls.Find("InvoiceHeaderTabPage", true).OfType<ZTabPage>().Any(c => c.CaptionResourceString.Caption == "Inv. Headers"));
				Assert("Inv.Date", tabControl.Controls.Find("InvoiceLineTabPage", true).OfType<ZTabPage>().Any(c => c.CaptionResourceString.Caption == "Inv. Lines"));
			});
		}

		public void TestUserControlInvoiceLineTabVisibility()
		{
			var shipment = (IBusiness)Factory.CreateNewShipment();
			AssertInvoiceLineTabVisibility("No invoice header", expectedLabelVisibility: true, expectedSplitterVisibility: false);

			var invoiceHeader = Factory.CreateInvoiceHeader((BusinessObject)shipment);
			AssertInvoiceLineTabVisibility("With invoice header", expectedLabelVisibility: false, expectedSplitterVisibility: true);

			void AssertInvoiceLineTabVisibility(string message, bool expectedLabelVisibility, bool expectedSplitterVisibility)
			{
				var pluginBizO = new GlobalCommercialInvoicePluginBusinessObject((BusinessObject)shipment);

				using var form = new ZForm();
				using var userControl = new GlobalCommercialInvoicePluginUserControl(pluginBizO);
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions($"Invoice line tab visibility : {message}", () =>
				{
					((ZTemplateTabControl)userControl.Controls.Find("InvoiceMainTabControl", true).Single()).SelectedTab = (ZTabPage)userControl.Controls.Find("InvoiceLineTabPage", true).Single();
					var visibilityLabel = (ZLabel)userControl.Controls.Find("InvoiceLineUserControlsVisibilityLabel", true).Single();

					AssertEquals("Invoice line visibility caption", "You must enter at least one Invoice Header before you can create Invoice Lines.", visibilityLabel.CaptionResourceString.Caption);
					AssertEquals("Invoice line visibility label", expectedLabelVisibility, visibilityLabel.Visible);
					AssertEquals("Invoice line splitter", expectedSplitterVisibility, userControl.Controls.Find("InvoiceLineSplitContainer", true).Single().Visible);
				});

				form.Dispose();
			}
		}
	}
}
