using System.Linq;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	class CustomsDeclarationMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuAction()
		{
			var shipment = PrepareShipment();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = header.Consignments.AddNew();
			consignment1.HVC_WaybillNumber = "Waybill001";
			consignment1.HVC_GoodsValue = 10;
			consignment1.HVC_RX_NKGoodsValueCurrency = "AUD";

			var item1 = consignment1.Items.AddNew();
			var line1 = item1.Lines.AddNew();
			line1.HVS_GoodsDescription = "Goods1";
			line1.HVS_OriginGoodsDescription = "OriginGoods1";
			line1.HVS_WeightUnit = "KG";
			line1.HVS_GrossWeight = 0.5;
			line1.HVS_NetWeight = 0.4;
			line1.HVS_Quantity = 1;
			line1.HVS_RN_NKOriginCountryCode = "AU";

			var consignment2 = header.Consignments.AddNew();
			consignment2.HVC_WaybillNumber = "Waybill002";
			consignment2.HVC_RX_NKGoodsValueCurrency = "AUD";

			var item2 = consignment2.Items.AddNew();
			var line2 = item2.Lines.AddNew();
			line2.HVS_GoodsDescription = "Goods2";
			line2.HVS_OriginGoodsDescription = "OriginGoods2";
			line2.HVS_CustomsValue = 20;
			line2.HVS_WeightUnit = "G";
			line2.HVS_GrossWeight = 200;
			line2.HVS_NetWeight = 150;
			line2.HVS_Quantity = 2;
			line2.HVS_RN_NKOriginCountryCode = "NZ";

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(TestingCountry))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				UnitTestUserNotification.Instance.AddOKAnswer();

				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == RelatedJobName);
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == $"Create {RelatedJobName}").PerformClick();

				var job = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity as Enterprise.Integration.Customs.IBaseJobDeclaration;
				var invoiceHeaders = job.Invoices.OfType<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>().ToList();
				var invoiceLines = job.InvoiceLines.OfType<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>().ToList();

				CombineAssertions("Invoice headers and lines", () =>
				{
					AssertEquals("Should created 2 invoice headers", 2, invoiceHeaders.Count);

					AssertEquals("Header1 JZ_InvoiceAmount", 10m, invoiceHeaders[0].JZ_InvoiceAmount);
					AssertEquals("Header1 JZ_InvoiceNumber", "WAYBILL001", invoiceHeaders[0].JZ_InvoiceNumber);
					AssertEquals("Header1 JZ_NetWeight", 0.4m, invoiceHeaders[0].JZ_NetWeight);
					AssertEquals("Header1 JZ_NetWeightUQ", "KG", invoiceHeaders[0].JZ_NetWeightUQ);
					AssertEquals("Header1 JZ_Weight", 0.5m, invoiceHeaders[0].JZ_Weight);
					AssertEquals("Header1 JZ_WeightUQ", "KG", invoiceHeaders[0].JZ_WeightUQ);

					AssertEquals("Header2 JZ_InvoiceAmount", 20m, invoiceHeaders[1].JZ_InvoiceAmount);
					AssertEquals("Header2 JZ_InvoiceNumber", "WAYBILL002", invoiceHeaders[1].JZ_InvoiceNumber);
					AssertEquals("Header2 JZ_NetWeight", 150m, invoiceHeaders[1].JZ_NetWeight);
					AssertEquals("Header2 JZ_NetWeightUQ", "G", invoiceHeaders[1].JZ_NetWeightUQ);
					AssertEquals("Header2 JZ_Weight", 200m, invoiceHeaders[1].JZ_Weight);
					AssertEquals("Header2 JZ_WeightUQ", "G", invoiceHeaders[1].JZ_WeightUQ);

					AssertEquals("Should created 2 invoice lines", 2, invoiceLines.Count);

					AssertEquals("Line1 JI_CountryOfOrigin", "AU", invoiceLines[0].JI_CountryOfOrigin);
					AssertEquals("Line1 JI_CustomsQuantity", 1m, invoiceLines[0].JI_CustomsQuantity);
					AssertEquals("Line1 JI_Description", "Goods1", invoiceLines[0].JI_Description);
					AssertEquals("Line1 JI_InvoiceQuantity", 1m, invoiceLines[0].JI_InvoiceQuantity);
					AssertEquals("Line1 JI_InvoiceUQ", "PCE", invoiceLines[0].JI_InvoiceUQ);
					AssertEquals("Line1 JI_NDescription", "OriginGoods1", invoiceLines[0].JI_NDescription);
					AssertEquals("Line1 JI_NetWeight", 0.4m, invoiceLines[0].JI_NetWeight);
					AssertEquals("Line1 JI_NetWeightUQ", "KG", invoiceLines[0].JI_NetWeightUQ);
					AssertEquals("Line1 JI_Weight", 0.5m, invoiceLines[0].JI_Weight);
					AssertEquals("Line1 JI_WeightUQ", "KG", invoiceLines[0].JI_WeightUQ);

					AssertEquals("Line2 JI_CountryOfOrigin", "NZ", invoiceLines[1].JI_CountryOfOrigin);
					AssertEquals("Line2 JI_CustomsQuantity", 2m, invoiceLines[1].JI_CustomsQuantity);
					AssertEquals("Line2 JI_Description", "Goods2", invoiceLines[1].JI_Description);
					AssertEquals("Line2 JI_InvoiceQuantity", 2m, invoiceLines[1].JI_InvoiceQuantity);
					AssertEquals("Line2 JI_InvoiceUQ", "PCE", invoiceLines[1].JI_InvoiceUQ);
					AssertEquals("Line2 JI_NDescription", "OriginGoods2", invoiceLines[1].JI_NDescription);
					AssertEquals("Line2 JI_NetWeight", 150m, invoiceLines[1].JI_NetWeight);
					AssertEquals("Line2 JI_NetWeightUQ", "G", invoiceLines[1].JI_NetWeightUQ);
					AssertEquals("Line2 JI_Weight", 200m, invoiceLines[1].JI_Weight);
					AssertEquals("Line2 JI_WeightUQ", "G", invoiceLines[1].JI_WeightUQ);
				});
			}
		}

		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersCAeManifest;

		protected override string TestingCountry => CountryCodes.Canada;

		protected override string ExpectedAppLockKey => "CustomsDeclarationCommand";

		protected override string RelatedJobName => "Stand Alone Declaration";

		protected override bool HasConsigneeInfo => false;

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "CATOR";

			return shipment;
		}
	}
}
