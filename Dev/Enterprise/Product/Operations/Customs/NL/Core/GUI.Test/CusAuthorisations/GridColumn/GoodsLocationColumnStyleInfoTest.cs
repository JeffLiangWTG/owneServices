using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(GoodsLocationColumnStyleInfo))]
sealed class GoodsLocationColumnStyleInfoTest : TestCase
{
	public void TestColumnStyleType()
	{
		var columnStyleInfo = new GoodsLocationColumnStyleInfo();
		AssertEquals(typeof(GoodsLocationColumnStyle), columnStyleInfo.ColumnStyleType);
	}
}
