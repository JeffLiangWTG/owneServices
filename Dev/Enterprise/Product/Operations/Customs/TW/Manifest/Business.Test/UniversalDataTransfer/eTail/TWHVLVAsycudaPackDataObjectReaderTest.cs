using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.TW.Manifest.Business.UniversalDataTransfer.Testing
{
	sealed class TWHVLVAsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportPackingLine_ShouldReadPackedItemInfo()
		{
			var (hvlvShipment, packingLine) = PrepareDataObject();

			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper("TW", Factory.BOFactory);
			var pack = new TWHVLVAsycudaPackDataObjectReader(packingLine, logger, Factory, bill, readerHelper, isUpdateEnabled: true, hvlvShipment).ReadIntoBusinessObject();

			CombineAssertions("Pack read correctly", () =>
			{
				AssertNotNull(pack);
				AssertEquals("HS Code should be populated", "1234.56.78", pack.PackedItem.API_Tariff);
			});
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
			packingLine.PackQty = 1;

			return (hvlvShipment, packingLine);
		}
	}
}
