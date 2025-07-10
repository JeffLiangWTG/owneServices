using System.Drawing;
using System.Windows.Forms;

namespace WinzorFramework.Extensions;

public static class RichTextBoxHtmlExtension
{
	public static string CreateHtmlNewLine(this RichTextBox richTextBox, string newText, Font newFont, Color newColor)
	{
		newFont ??= richTextBox.Font;
		if (Color.Empty == newColor)
		{
			newColor = richTextBox.ForeColor;
		}

		if (newFont.Bold)
		{
			newText = $"<strong>{newText}</strong>";
		}

		var style = $"color: rgb({newColor.R}, {newColor.G}, {newColor.B}); font-family: {newFont.Name}, sans-serif; font-size: {newFont.Size}pt;";
		var span = $"<span style=\"{style}\">{newText}</span>";
		return $"<p>{span}</p>";
	}
}
