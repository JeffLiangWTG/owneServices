using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class HtmlSanitizerFixture
	{
		[TestCaseSource(nameof(TestFileNameData))]
		public void TestSanitizeEmailContent(string originalContent, string expectedContent)
		{
			Assert.That(HtmlSanitizer.Sanitize(originalContent), Is.EqualTo(expectedContent));
		}

		static object[] TestFileNameData => new object[]
		{
			new object[] { "abc", "abc" },
			new object[] { "test<h1>test", "test&lt;h1&gt;test"},
			new object[] { "Click this link <a href=\"https://test.com\">Click Here</a>", "Click this link &lt;a href=&quot;https://test.com&quot;&gt;Click Here&lt;/a&gt;" }
		};
	}
}
