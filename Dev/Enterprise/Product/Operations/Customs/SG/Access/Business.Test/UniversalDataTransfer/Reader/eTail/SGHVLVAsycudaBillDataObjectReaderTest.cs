using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGHVLVAsycudaBillDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportHVLVBill()
		{
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry, Factory.BOFactory);

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.WayBillNumber = "12345678";
			subShipment.TotalNoOfPieces = 3;
			subShipment.TotalNoOfPacksPackageType = new PackageType() { Code = "BAG" };
			subShipment.TotalWeight = 20m;
			subShipment.TotalWeightUnit = new UnitOfWeight() { Code = "T" };
			subShipment.TotalVolume = 10m;
			subShipment.TotalVolumeUnit = new UnitOfVolume() { Code = "M3" };
			subShipment.GoodsDescription = "87654321";
			subShipment.ShipmentIncoTerm = new UniversalIncoTerm() { Code = "FOB" };
			subShipment.GoodsValue = 12.34m;
			subShipment.GoodsValueCurrency = new Currency() { Code = "SGD" };

			subShipment.SetCustomsReferenceCollection(() => new List<CustomsReference>());
			var reference = new CustomsReference();
			reference.Type = new CodeDescriptionPair();
			reference.Type.Code = nameof(Core.Constants.DataContext.Declaration);
			reference.Reference = "B00000001";
			subShipment.CustomsReferenceCollection.AddSafe(reference);

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			var line1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var line2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.PackingLineCollection.Add(line1);
			subShipment.PackingLineCollection.Add(line2);
			line1.SetPackedItemCollection(() => new List<PackedItem>());
			var item1 = new PackedItem();
			var item2 = new PackedItem();
			line1.PackedItemCollection.AddSafe(item1);
			line1.PackedItemCollection.AddSafe(item2);

			var billBO = new SGHVLVAsycudaBillDataObjectReader(subShipment, Logger, Factory, header, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(billBO);
			AssertEquals("billBO.ABL_BillNumber", "12345678", billBO.ABL_BillNumber);
			AssertEquals("billBO.ABL_ManifestQty", 3, billBO.ABL_ManifestQty);
			AssertEquals("billBO.ABL_ManifestUQ", "BAG", billBO.ABL_ManifestUQ);
			AssertEquals("billBO.ABL_GrossWeight", 20m, billBO.ABL_GrossWeight);
			AssertEquals("billBO.ABL_GrossWeightUQ", "T", billBO.ABL_GrossWeightUQ);
			AssertEquals("billBO.ABL_Volume", 10m, billBO.ABL_Volume);
			AssertEquals("billBO.ABL_VolumeUQ", "M3", billBO.ABL_VolumeUQ);
			AssertEquals("billBO.ABL_PrepaidCollect", "CLT", billBO.ABL_PrepaidCollect);
			AssertEquals("billBO.ABL_FreightValue", 12.34m, billBO.ABL_FreightValue);
			AssertEquals("billBO.ABL_GoodsDescription", "87654321", billBO.ABL_GoodsDescription);
			AssertEquals("billBO.ABL_RX_NKFreightValueCurrency", "SGD", billBO.ABL_RX_NKFreightValueCurrency);
			AssertEquals("billBO.CustomsJobNumber", "B00000001", billBO.CustomsJobNumber);
			AssertEquals("should have 3 packs", 3, billBO.Packs.Count);
		}
	}
}
