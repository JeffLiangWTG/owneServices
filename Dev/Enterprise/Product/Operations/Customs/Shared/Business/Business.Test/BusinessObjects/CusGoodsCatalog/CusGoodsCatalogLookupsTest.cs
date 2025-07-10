using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class CusGoodsCatalogLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusGoodsCatalogCodeList()
		{
			var list = GoodsCatalog.Lookups.TypeList;
			CombineAssertions(() =>
			{
				AssertType<GoodsCatalogTypeList>("Type", list);
				AssertSame("Type", list, GoodsCatalog.Lookups.TypeList);
			});
		}

		public void TestOwners()
		{
			AssertType<ConsigneeCollection>("Consignees type", GoodsCatalog.Lookups.Owners);
		}

		public void TestMessageStatusList()
		{
			AssertType<MessageStatusList>("Message Status", GoodsCatalog.Lookups.MessageStatusList);
			AssertContainsExactElementsInAnyOrder("Message Status",new string[] { "ACC", "ACD", "ACO", "ACR", "AWC", "AWD", "AWO", "AWR", "CLC", "CLD", "CLO", "CLR", "ERC", "ERD", "ERO", "ERR", "", "SNT", "UNK" }, GoodsCatalog.Lookups.MessageStatusList.GetAllCodes());
		}

		BaseCusGoodsCatalog GoodsCatalog => goodsCatalog ??= Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
		BaseCusGoodsCatalog goodsCatalog;
	}
}

