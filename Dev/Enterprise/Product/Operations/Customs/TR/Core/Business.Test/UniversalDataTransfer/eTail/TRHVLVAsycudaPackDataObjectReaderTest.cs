using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TR.Business.UniversalDataTransfer.Testing
{
	class TRHVLVAsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportMultiplePackedItemsForPack()
		{
			var (hvlvShipment, packingLineDataObject) = PrepareDataObject();

			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var pack = new TRHVLVAsycudaPackDataObjectReader(packingLineDataObject, logger, Factory, bill, new TRAsycudaManifestDataObjectReaderHelper("TR", Factory.BOFactory), true, hvlvShipment).ReadIntoBusinessObject();

			AssertEquals("Should populate 2 packed items", 2, pack.PackedItems.Count);
		}

		(Shipment, PackingLine) PrepareDataObject()
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

			var item1 = new PackedItem();
			item1.Description = "PackedItemDesc";
			item1.PackedQuantity = 1;
			item1.GrossWeight = 1;
			item1.GrossWeightUnit = new UnitOfWeight() { Code = "KG" };
			item1.NetWeight = 1;
			item1.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item1.CIFValue = 1;
			item1.CommercialInvoiceLineLink = 1;

			var item2 = new PackedItem();
			item2.Description = "PackedItemDesc";
			item2.PackedQuantity = 1;
			item2.GrossWeight = 1;
			item2.GrossWeightUnit = new UnitOfWeight() { Code = "KG" };
			item2.NetWeight = 1;
			item2.NetWeightUnit = new UnitOfWeight { Code = "KG" };
			item2.CIFValue = 1;
			item2.CommercialInvoiceLineLink = 1;

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packingLine.SetPackedItemCollection(() => new List<PackedItem> { item1, item2 });
			return (hvlvShipment, packingLine);
		}
	}
}
