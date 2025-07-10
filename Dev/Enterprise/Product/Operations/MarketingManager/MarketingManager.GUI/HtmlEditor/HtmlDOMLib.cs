using HtmlAgilityPack;

namespace Enterprise.MarketingManager.GUI
{
	public static class HtmlDOMLib
	{
		/// <summary>
		/// Parses the HTML.
		/// </summary>
		/// <param name="html">The HTML.</param>
		/// <returns>HtmlDocument.</returns>
		public static HtmlDocument ParseHtml(string html)
		{
			var doc = new HtmlDocument
			{
				OptionWriteEmptyNodes = true,
				OptionAutoCloseOnEnd = true,
				OptionFixNestedTags = true
			};
			doc.LoadHtml(html);
			return doc;
		}
	}
}
