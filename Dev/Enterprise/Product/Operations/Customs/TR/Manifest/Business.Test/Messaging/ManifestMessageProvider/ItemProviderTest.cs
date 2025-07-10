using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class ItemProviderTest : TestCaseWithFactory
	{
		public void TestGoodsInformationMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);
				IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();
				ILadingLines ladingLines = billofLading.LadingLines.FirstOrDefault();
				IGoodsInformation goodsInformation = ladingLines.GoodsInformation.FirstOrDefault();
				CombineAssertions("package Item Level", () =>
				{
					AssertEquals("0004a", goodsInformation.UNGoodCode);
					AssertEquals(Convert.ToDecimal(0), goodsInformation.GrossWeight);
					AssertEquals("123412121212", goodsInformation.TariffCode);
					AssertEquals("xGoods Description", goodsInformation.GoodsDescription);
					AssertEquals(Convert.ToDecimal(0), goodsInformation.GoodsValue);
					AssertEquals("EUR", goodsInformation.GoodsValueCurrency);
					AssertEquals(1, goodsInformation.OrderNo);
					AssertEquals(Convert.ToDecimal(0), goodsInformation.NetWeight);
					AssertEquals(TurkishConstants.WeightUnitType, goodsInformation.CustomsUQ);
				});
			}
		}
	}
}
