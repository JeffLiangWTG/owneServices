using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Enterprise.MarketingManager.GUI
{
	class VoidSpanRemover
	{
		/// <summary>
		/// Class SpanTag
		/// </summary>
		class SpanTag
		{
			/// <summary>
			/// The is empty attribute tag
			/// </summary>
			public bool IsEmptyAttributeTag;
		}

		/// <summary>
		/// The collected span tags
		/// </summary>
		readonly Stack<SpanTag> collectedSpanTags = new Stack<SpanTag>();

		/// <summary>
		/// Parses the and fix.
		/// </summary>
		/// <param name="html">The HTML.</param>
		/// <returns>System.String.</returns>
		public string ParseAndFix(string html)
		{
			collectedSpanTags.Clear();
			var regexObj = new Regex("</{0,1}span[^>]*>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			return regexObj.Replace(html, SpanTagProcessor);
		}

		/// <summary>
		/// Spantags the processor.
		/// </summary>
		/// <param name="caughtHtmlElement">The caught HTML element.</param>
		/// <returns>System.String.</returns>
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		string SpanTagProcessor(Match caughtHtmlElement)
		{
			if (!caughtHtmlElement.Value.StartsWith("</"))
			{
				var isEmptyAttr = !caughtHtmlElement.Value.ToLower().Contains("<span ");
				collectedSpanTags.Push(new SpanTag { IsEmptyAttributeTag = isEmptyAttr });
				return isEmptyAttr ? string.Empty : caughtHtmlElement.Value;
			}

			if (collectedSpanTags.Count == 0)
			{
				return string.Empty;
			}

			var popedSpanBeginTag = collectedSpanTags.Pop();
			return (popedSpanBeginTag.IsEmptyAttributeTag ?
						string.Empty :
						caughtHtmlElement.Value);
		}
	}
}
