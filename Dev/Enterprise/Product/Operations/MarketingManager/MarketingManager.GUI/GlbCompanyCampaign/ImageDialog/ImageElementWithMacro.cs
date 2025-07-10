using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Enterprise.MarketingManager.GUI
{
	public class ImageElementWithMacro
	{
		public string SrcUrl { get; set; }

		public string Align { get; set; }

		public string Title { get; set; }

		public string AlternativeText { get; set; }

		public string BorderStyle { get; set; }

		public Color? BorderColor { get; set; }

		public string Height { get; set; }

		public string Width { get; set; }

		public string BorderWidth { get; set; }

		public string Macro { get; set; }

		string cssStyle;

		public string ToHtmlStringWithMacro()
		{
			if (!string.IsNullOrEmpty(BorderWidth))
			{
				cssStyle = HtmlEditorUtils.UpdateStyleValueInStyleString(cssStyle, "border-width", BorderWidth);
			}

			var htmlString = ToHtmlString();
			if (string.IsNullOrEmpty(Macro))
			{
				return htmlString;
			}

			var macroAttribute = FormattableString.Invariant($"macro=\"{Macro}\" />");
			return htmlString.Replace("/>", macroAttribute);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html style element")]
		string ToHtmlString()
		{
			var attributeCollection = new List<string>();
			if (!string.IsNullOrEmpty(SrcUrl))
			{
				attributeCollection.Add($"src=\"{SrcUrl}\"");
			}

			if (!string.IsNullOrEmpty(Title))
			{
				attributeCollection.Add($"title=\"{Title}\"");
			}

			if (!string.IsNullOrEmpty(Align))
			{
				attributeCollection.Add($"align=\"{Align}\"");
			}

			if (!string.IsNullOrEmpty(AlternativeText))
			{
				attributeCollection.Add($"alt=\"{AlternativeText}\"");
			}

			if (!string.IsNullOrEmpty(Height))
			{
				cssStyle = HtmlEditorUtils.UpdateStyleValueInStyleString(cssStyle, "height", Height);
			}

			if (!string.IsNullOrEmpty(Width))
			{
				cssStyle = HtmlEditorUtils.UpdateStyleValueInStyleString(cssStyle, "width", Width);
			}

			if (BorderColor.HasValue)
			{
				cssStyle = HtmlEditorUtils.UpdateStyleValueInStyleString(cssStyle, "border-color", ColorTranslator.ToHtml(BorderColor.Value));
			}

			if (!string.IsNullOrEmpty(BorderStyle))
			{
				cssStyle = HtmlEditorUtils.UpdateStyleValueInStyleString(cssStyle, "border-style", BorderStyle);
			}

			if (!string.IsNullOrEmpty(cssStyle))
			{
				attributeCollection.Add($"style=\"{cssStyle}\"");
			}

			var value = string.Join(" ", attributeCollection);
			return string.IsNullOrEmpty(value) ? "<img />" : $"<img {value} />";
		}
	}
}
