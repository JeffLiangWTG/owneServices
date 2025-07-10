using NUnit.Framework;

namespace WinzorFramework.Extensions;

class MnemonicTextExtensionTest
{
	[TestCase("Test", true, "Test", TestName = "{m}_FormatString")]
	[TestCase("Test", false, "Test", TestName = "{m}_FormatStringDisplayFalse")]
	[TestCase("&Test", true, "<span class=\"mnemonickey\">T</span>est", TestName = "{m}_FormatFirstCharMnemonicInSpan")]
	[TestCase("&Test", false, "Test", TestName = "{m}_FormatFirstCharMnemonicInSpanDisplayFalse")]
	[TestCase("T&est", true, "T<span class=\"mnemonickey\">e</span>st", TestName = "{m}_FormatSecondCharMnemonicInSpan")]
	[TestCase("T&est", false, "Test", TestName = "{m}_FormatSecondCharMnemonicInSpanDisplayFalse")]
	[TestCase("Tes&t", true, "Tes<span class=\"mnemonickey\">t</span>", TestName = "{m}_FormatLastCharMnemonicInSpan")]
	[TestCase("Tes&t", false, "Test", TestName = "{m}_FormatLastCharMnemonicInSpanDisplayFalse")]
	[TestCase("Test&", true, "Test", TestName = "{m}_LastCharAmpersand")]
	[TestCase("Test&", false, "Test", TestName = "{m}_LastCharAmpersandDisplayFalse")]
	[TestCase("Te&&st", true, "Te&amp;st", TestName = "{m}_EscapedAmpersand")]
	[TestCase("Te&&st", false, "Te&amp;st", TestName = "{m}_EscapedAmpersandDisplayFalse")]
	[TestCase("<b>Test</b>", true, "&lt;b&gt;Test&lt;/b&gt;", TestName = "{m}_EncodesHtml")]
	[TestCase("<b>Test</b>", false, "&lt;b&gt;Test&lt;/b&gt;", TestName = "{m}_EncodesHtmlDisplayFalse")]
	public void ProcessMnemonicToHtml(string text, bool displayMnemonic, string expectedHtml)
	{
		Assert.That(text.ProcessMnemonicToHtml(displayMnemonic).ToString(), Is.EqualTo(expectedHtml));
	}
}
