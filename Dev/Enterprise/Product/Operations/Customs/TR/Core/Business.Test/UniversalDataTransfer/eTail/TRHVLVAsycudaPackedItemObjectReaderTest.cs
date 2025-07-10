using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer.Testing
{
	class TRHVLVAsycudaPackedItemObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportPackedItem()
		{
			var (hvlvShipment, packingLineDataObject, packedItemDataObject) = PrepareDataObject();

			var pack = Factory.NewWithValidTestData<AsycudaBill>().Packs.AddNew();
			var packedItem = new TRHVLVAsycudaPackedItemObjectReader(packingLineDataObject, packedItemDataObject, null, logger, Factory, pack, new TRAsycudaManifestDataObjectReaderHelper("TR", Factory.BOFactory), hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions("Packed Item read correctly", () =>
			{
				AssertNotNull(packedItem);
				AssertEquals("Goods Description", "PackedItemDesc", packedItem.API_GoodsDescription);
				AssertEquals("Tariff", "8723.98", packedItem.API_FormattedTariff);
				AssertEquals("Customs Qty", new ZDecimal(1), packedItem.API_CustomsQty);
				AssertEquals("Customs UQ", "PP", packedItem.API_CustomsUQ);
				AssertEquals("Gross Weight", new ZDecimal(1), packedItem.API_GrossWeight);
				AssertEquals("Gross Weight UQ", "KG", packedItem.API_GrossWeightUQ);
				AssertEquals("Net Weight", new ZDecimal(1), packedItem.API_NetWeight);
				AssertEquals("Net Weight UQ", "KG", packedItem.API_NetWeightUQ);
				AssertEquals("Goods Value", new ZDecimal(1), packedItem.API_GoodsValue);
				AssertEquals("Currency", "AUD", packedItem.API_RX_NKGoodsValueCurrency);
			});
		}

		(Shipment, PackingLine, PackedItem) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine.Link = 1;
			commercialInvoiceLine.HarmonisedCode = "872398";

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;
			hvlvShipment.GoodsValueCurrency = new Currency();
			hvlvShipment.GoodsValueCurrency.Code = "AUD";

			var item = new PackedItem();
			item.Description = "PackedItemDesc";
			item.PackedQuantity = 1;
			item.GrossWeight = 1;
			item.GrossWeightUnit = new UnitOfWeight() { Code = "KG" };
			item.NetWeight = 1;
			item.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item.CIFValue = 1;
			item.CommercialInvoiceLineLink = 1;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item, });
			return (hvlvShipment, packingLine, item);
		}
	}
}
