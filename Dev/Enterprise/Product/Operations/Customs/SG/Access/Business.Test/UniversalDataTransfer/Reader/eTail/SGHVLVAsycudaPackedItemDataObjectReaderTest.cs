using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGHVLVAsycudaPackedItemDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportHVLVPackedItem()
		{
			var pack = Factory.BOFactory.New<AsycudaPack>();
			var item = new PackedItem();
			var commercialInvoiceLine = new CommercialInvoiceLine();
			item.Description = "123456";
			item.PackedQuantity = 4.4m;
			commercialInvoiceLine.HarmonisedCode = "1234.56.78";
			commercialInvoiceLine.CountryOfOrigin = new Country()
			{ Code = "US" };
			commercialInvoiceLine.CustomsValue = 5.5m;
			var itemBO = new SGHVLVAsycudaPackedItemDataObjectReader(item, commercialInvoiceLine, Logger, Factory, pack).ReadIntoBusinessObject();
			AssertNotNull(itemBO);
			AssertItemBo(itemBO, "123456", 4.4m, "NMB", 5.5m, "1234.56.78", "US");
		}

		public void TestUpdateHVLVPackedItem()
		{
			var item = new PackedItem();
			var commercialInvoiceLine = new CommercialInvoiceLine();
			item.Description = "12345678";
			item.PackedQuantity = 9.9m;
			commercialInvoiceLine.HarmonisedCode = "4231.58.67";
			commercialInvoiceLine.CountryOfOrigin = new Country()
			{ Code = "NZ" };
			commercialInvoiceLine.CustomsValue = 5.57m;
			var pack = Factory.BOFactory.New<AsycudaPack>();
			var packedItem = pack.PackedItems.AddNewPackedItem();
			packedItem.API_CustomsQty = 8.8m;
			packedItem.API_CustomsUQ = "BAG";
			packedItem.API_CustomsValue = 1.1m;
			packedItem.API_Tariff = "8765.43.21";
			packedItem.API_RN_NKGoodsOrigin = "AU";
			var itemBO = new SGHVLVAsycudaPackedItemDataObjectReader(item, commercialInvoiceLine, Logger, Factory, pack).ReadIntoBusinessObject();
			AssertNotNull(itemBO);
			AssertItemBo(itemBO, "12345678", 9.9m, "NMB", 5.57m, "4231.58.67", "NZ");
		}

		void AssertItemBo(ASYCUDA.Business.AsycudaPackedItem itemBO, string description, decimal customQty, string customUQ, decimal customValue, string tariff, string goodsOrigin)
		{
			AssertEquals("itemBO.API_GoodsDescription", description, itemBO.API_GoodsDescription);
			AssertEquals("itemBO.API_CustomsQty", customQty, itemBO.API_CustomsQty);
			AssertEquals("itemBO.API_CustomsUQ", customUQ, itemBO.API_CustomsUQ);
			AssertEquals("itemBO.API_CustomsValue", customValue, itemBO.API_CustomsValue);
			AssertEquals("itemBO.API_Tariff", tariff, itemBO.API_Tariff);
			AssertEquals("itemBO.API_RN_NKGoodsOrigin", goodsOrigin, itemBO.API_RN_NKGoodsOrigin);
		}
	}
}
