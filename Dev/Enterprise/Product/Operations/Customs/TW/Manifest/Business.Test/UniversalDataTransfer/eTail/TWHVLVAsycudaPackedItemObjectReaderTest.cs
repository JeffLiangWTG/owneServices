using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer.Testing
{
	sealed class TWHVLVAsycudaPackedItemObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportPackedItem()
		{
			var (hvlvShipment, packingLine) = PrepareDataObject();

			var pack = Factory.NewWithValidTestData<AsycudaBill>().Packs.AddNew();
			var packedItem = new TWHVLVAsycudaPackedItemObjectReader(packingLine, null, logger, Factory, pack, new TWAsycudaManifestDataObjectReaderHelper("TW", Factory.BOFactory), hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions("Packed Item read correctly", () =>
			{
				AssertNotNull(packedItem);
				AssertEquals("Goods Description", "PackedItemDesc", packedItem.API_GoodsDescription);
				AssertEquals("HS Code", "1234.56.78", packedItem.API_Tariff);
			});
		}

		public void TestGoodsDescriptionFallback()
		{
			var (hvlvShipment, packingLine) = PrepareDataObject();
			hvlvShipment.GoodsDescription = "ConsignmentGoodsDescription";
			packingLine.PackedItemCollection.Single().Description = string.Empty;

			var pack = Factory.NewWithValidTestData<AsycudaBill>().Packs.AddNew();
			var packedItem = new TWHVLVAsycudaPackedItemObjectReader(packingLine, null, logger, Factory, pack, new TWAsycudaManifestDataObjectReaderHelper("TW", Factory.BOFactory), hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("packed item description should fall back to consignment description", "ConsignmentGoodsDescription", packedItem.API_GoodsDescription);
		}

		(Shipment, PackingLine) PrepareDataObject()
		{
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var commercialInvoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceLine.Link = 1;
			commercialInvoiceLine.HarmonisedCode = "1234.56.78";

			var commercialInvoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInvoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine> { commercialInvoiceLine });

			var commercialInfo = new CommercialInfo();
			commercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			commercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader> { commercialInvoiceHeader });
			hvlvShipment.CommercialInfo = commercialInfo;

			var item = new PackedItem();
			item.Description = "PackedItemDesc";
			item.CommercialInvoiceLineLink = 1;
			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item });

			return (hvlvShipment, packingLine);
		}
	}
}
