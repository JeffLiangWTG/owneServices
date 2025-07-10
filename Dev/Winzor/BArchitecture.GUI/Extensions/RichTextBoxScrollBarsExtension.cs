using System.Windows.Forms;

namespace WinzorFramework.Extensions;

public static class RichTextBoxScrollBarsExtension
{
	public static string GetVerticalScrollStyleString(this RichTextBoxScrollBars richTextBoxScrollBars)
	{
		switch (richTextBoxScrollBars)
		{
			case RichTextBoxScrollBars.Both:
			case RichTextBoxScrollBars.Vertical:
				return "auto";
			case RichTextBoxScrollBars.ForcedBoth:
			case RichTextBoxScrollBars.ForcedVertical:
				return "scroll";
			default:
				return "hidden";
		}
	}

	public static string GetHorizontalScrollStyleString(this RichTextBoxScrollBars richTextBoxScrollBars, bool wordWrap = false)
	{
		if (wordWrap)
		{
			return "hidden";
		}

		switch (richTextBoxScrollBars)
		{
			case RichTextBoxScrollBars.Both:
			case RichTextBoxScrollBars.Horizontal:
				return "auto";
			case RichTextBoxScrollBars.ForcedBoth:
			case RichTextBoxScrollBars.ForcedHorizontal:
				return "scroll";
			default:
				return "hidden";
		}
	}
}
