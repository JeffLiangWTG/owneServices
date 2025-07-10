using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSGoodsItemsProviderTest : TestCaseWithFactory
	{
		public void TestGoodsItemsMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var goodsItems = nctsHeaderProvider.GoodsItems.ToArray();

				CombineAssertions("Goods items members", () =>
				{
					AssertEquals("LineNumber", 1, goodsItems[0].LineNumber);
					AssertEquals("TariffCode", "690320", goodsItems[0].TariffCode);
					AssertEquals("Description", "AKSAM PARCA", goodsItems[0].Description);
					AssertEquals("DescriptionLNG", TRMessageConstants.LanguageCode, goodsItems[0].DescriptionLNG);
					AssertEquals("GrossWeight", 120.5500m, goodsItems[0].GrossWeight);
					AssertEquals("NetWeight", 110.2400m, goodsItems[0].NetWeight);
					AssertEquals("CountyOfDispatchCode", Core.Constants.CountryCodes.Turkey, goodsItems[0].CountyOfDispatchCode);
					AssertEquals("CountyOfDestinationCode", Core.Constants.CountryCodes.UnitedKingdom, goodsItems[0].CountyOfDestinationCode);
					AssertEquals("MonetaryValue", 0.00m, goodsItems[0].MonetaryValue);
					AssertEquals("MonetaryValueCurrencyCode", Core.Constants.CurrencyCodes.UnitedStates, goodsItems[0].MonetaryValueCurrencyCode);
					AssertNull("Consignor", goodsItems[0].Consignor);
					AssertNull("Consignor", goodsItems[0].Consignee);
				});
			}
		}
	}
}
