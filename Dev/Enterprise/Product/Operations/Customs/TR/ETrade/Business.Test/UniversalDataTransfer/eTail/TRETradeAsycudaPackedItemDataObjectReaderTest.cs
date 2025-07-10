using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(TRETradeHVLVAsycudaPackedItemDataObjectReader))]
	sealed class TRETradeAsycudaPackedItemDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestReadIntoBusinessObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine.Link = 1;
			commercialInvoiceLine.CountryOfOrigin = new Country { Code = "CN" };
			commercialInvoiceLine.HarmonisedCode = "123456";

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;

			var packedItemDataObject = new PackedItem();
			packedItemDataObject.GrossWeight = 1000;
			packedItemDataObject.GrossWeightUnit = new UnitOfWeight() { Code = "LB" };
			packedItemDataObject.CommercialInvoiceLineLink = 1;
			packedItemDataObject.CIFValue = 1.23m;
			packedItemDataObject.Description = "DESC";
			packedItemDataObject.PackedQuantity = 3;
			packedItemDataObject.NetWeight = 9000;
			packedItemDataObject.NetWeightUnit = new UnitOfWeight() { Code = "G" };
			packedItemDataObject.GoodsValue = 4.56M;

			var packingLineDataObject = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLineDataObject.SetPackedItemCollection(() => new List<PackedItem> { packedItemDataObject, });

			var pack = Factory.NewWithValidTestData<AsycudaBill>().Packs.AddNew();
			var packedItem = new TRETradeHVLVAsycudaPackedItemDataObjectReader(packingLineDataObject, packedItemDataObject, logger, Factory, pack, new TRETradeHVLVAsycudaManifestDataObjectReaderHelper("TR", Factory.BOFactory), hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions("Packed Item read correctly", () =>
			{
				AssertNotNull(packedItem);
				AssertEquals("Customs Qty2", 453.59237m, packedItem.API_CustomsQty2);
				AssertEquals("Customs UQ2", "KGM", packedItem.API_CustomsUQ2);
				AssertEquals("Goods Value", 1.23m, packedItem.API_GoodsValue);
				AssertEquals("Goods Description", "DESC", packedItem.API_GoodsDescription);
				AssertEquals("Customs Qty", 3m, packedItem.API_CustomsQty);
				AssertEquals("Gross Weight", 1000m, packedItem.API_GrossWeight);
				AssertEquals("Gross Weight UQ", "LB", packedItem.API_GrossWeightUQ);
				AssertEquals("Net Weight", 9000m, packedItem.API_NetWeight);
				AssertEquals("Net Weight UQ", "G", packedItem.API_NetWeightUQ);
				AssertEquals("Customs Value", 4.56m, packedItem.API_CustomsValue);
				AssertEquals("Goods Origin", "CN", packedItem.API_RN_NKGoodsOrigin);
				AssertEquals("Tariff", "123456", packedItem.API_Tariff);
			});
		}
	}
}
