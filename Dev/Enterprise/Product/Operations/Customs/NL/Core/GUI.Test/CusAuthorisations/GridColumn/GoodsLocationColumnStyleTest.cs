using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(GoodsLocationColumnStyle))]
sealed class GoodsLocationColumnStyleTest : TestCase
{
	public void TestEditControl() => CombineAssertions(() =>
	{
		var columnStyleInfo = new GoodsLocationColumnStyleInfo();

		using (var columnStyle = new GoodsLocationColumnStyle(columnStyleInfo))
		{
			AssertType<GoodsLocationGridFindBox>(columnStyle.EditControl);
		}
	});
}
