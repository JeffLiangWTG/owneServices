using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGHVLVAsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportHVLVPack()
		{
			var bill = Factory.BOFactory.New<AsycudaBill>();
			var line = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			var item = new PackedItem();
			line.PackType = new PackageType()
			{ Code = "BAG" };
			line.GoodsDescription = "123456789";
			line.Weight = 1.3m;
			line.WeightUnit = new UnitOfWeight()
			{ Code = "KG" };
			line.Volume = 3.1m;
			line.VolumeUnit = new UnitOfVolume()
			{ Code = "M3" };
			item.PackedQuantity = 3m;
			var packBO = new SGHVLVAsycudaPackDataObjectReader(line, item, null, Logger, Factory, bill).ReadIntoBusinessObject();
			AssertNotNull(packBO);
			AssertEquals("packBO.APA_PackUQ", "BAG", packBO.APA_PackUQ);
			AssertEquals("packBO.APA_GoodsDescription", "123456789", packBO.APA_GoodsDescription);
			AssertEquals("packBO.APA_Weight", 1.3m, packBO.APA_Weight);
			AssertEquals("packBO.APA_WeightUQ", "KG", packBO.APA_WeightUQ);
			AssertEquals("packBO.APA_Volume", 3.1m, packBO.APA_Volume);
			AssertEquals("packBO.APA_VolumeUQ", "M3", packBO.APA_VolumeUQ);
			AssertEquals("packBO.APA_PackQty", 3, packBO.APA_PackQty);
		}
	}
}
