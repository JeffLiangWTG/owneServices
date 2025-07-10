using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class HtmlBlockWriterParamTest : TestCase
{
	public void TestName() => CombineAssertions(() =>
	{
		var paramsValue = new HtmlBlockWriterParam<string>("TEST NAME", "VALUE", (_, _) => { });
		AssertEquals("String", "TEST NAME", paramsValue.Name);

		paramsValue = new HtmlBlockWriterParam<string>(null, "VALUE", (_, _) => { });
		AssertEquals("Null", string.Empty, paramsValue.Name);
	});

	public void TestWrite() => CombineAssertions(() =>
	{
		using var writer = new HtmlBlockWriter();
		var paramsValue = new HtmlBlockWriterParam<string>("NAME", "TEST VALUE", (_, val) => writer.WriteText(val));
		writer.WriteMultipleTextWithCaption([paramsValue]);
		AssertEquals("String", "TEST VALUE", writer.ToString());
	});
}
