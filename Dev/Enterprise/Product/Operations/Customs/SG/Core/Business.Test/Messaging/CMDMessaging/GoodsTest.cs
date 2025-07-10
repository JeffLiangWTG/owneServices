using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	public class GoodsTest : TestCase
	{
		public void TestInitialise()
		{
			var freightGoods = new Goods("Description 101", "HM002");
			AssertEquals("Description 101", freightGoods.ManifestDescription);
			AssertEquals("HM002", freightGoods.HarmonisedCode);
		}
	}
}
