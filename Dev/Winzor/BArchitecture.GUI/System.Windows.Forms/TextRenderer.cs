using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public static class TextRenderer
{
	/// <summary>
	/// This adjustment is not exact, but will bring the length value closer to WinForms.
	/// </summary>
	static readonly double WidthAdjustmentFactor = 0.75;

	public static Size MeasureText(string? text, Font? font) => MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.Default);

	public static Size MeasureText(string? text, Font? font, int width) => MeasureText(text, font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak);

	public static Size MeasureText(string? text, Font? font, Size proposedSize, TextFormatFlags flags)
	{
		var maxLineLength = 0;
		var height = 0;

		if (!string.IsNullOrEmpty(text))
		{
			font ??= Control.DefaultFont;
			if ((flags & TextFormatFlags.NoPrefix) != TextFormatFlags.NoPrefix)
			{
				var stringBuilder = new StringBuilder();
				for (var i = 0; i < text.Length; i++)
				{
					if (text[i] != '&')
					{
						stringBuilder.Append(text[i]);
					}
					else if (i + 1 < text.Length && text[i + 1] == '&')
					{
						stringBuilder.Append('&');
						i++;
					}
				}
				text = stringBuilder.ToString();
			}

			var lines = Regex.Split(text, "\\r?\\n");
			var lineHeight = font.GetFontHeight();
			for (var i = 0; i < lines.Length; i++)
			{
				if (string.IsNullOrEmpty(lines[i]))
				{
					if (flags.HasFlag(TextFormatFlags.TextBoxControl) && i == lines.Length - 1)
					{
						break;
					}
					height += lineHeight;
				}
				else
				{
					var blockSize = MeasureLine(lines[i], font, proposedSize, flags);
					maxLineLength = Math.Max(maxLineLength, blockSize.Width);
					height += blockSize.Height;
				}
			}
		}

		if (maxLineLength > 0)
		{
			maxLineLength += GetWidthAdjustment(font);
		}

		return new Size(maxLineLength, height);
	}

	static double GetCharWidth(char character, double[] charWidthArr)
	{
		if (character < charWidthArr.Length)
		{
			return charWidthArr[character];
		}

		// Introduced for WI00596875 - Error Window - Button Text Wrapping.
		// A special case for measuring Unicode char, the width is an approximate value.
		// If the number of special caese increases we need to design a general solution for measuring non-ASCII.
		if (character == '\u25B2' || character == '\u25BC')
		{
			return charWidthArr[0] * 1.7;
		}
		// A general solution for measuring non-ASCII is a huge project. We need to consider so many conditions: like Arabic language spell from right to left.
		// If we consider all the conditions, we also need to create different fontWidthDict which size will be too big to pass test MeasureTextIsFast().
		// We can fix the bug in Chinese condition because Chinese characters share the same width.
		if (character >= '\u4e00' && character <= '\u9fa5')
		{
			return charWidthArr[0] * 1.5;
		}
		return charWidthArr[0];
	}

	public static int GetWidthAdjustment(Font? font = null)
	{
		font ??= Control.DefaultFont;
		var fontSize = font.Size * (font?.Unit == GraphicsUnit.Pixel ? 0.75f : 1f);
		return (int)(WidthAdjustmentFactor * fontSize);
	}

	public static Size MeasureLine(string line, Font font, Size proposedSize, TextFormatFlags flags)
	{
		var maxLineWidth = 0d;
		var lineHeight = font.GetFontHeight();
		var height = lineHeight;
		var lineWidth = 0d;

		if (flags.HasFlag(TextFormatFlags.WordBreak))
		{
			var startIndex = 0;
			var lineNo = 0;
			var lineStr = "";
			while (startIndex < line.Length)
			{
				CalcLineBreak(line.Substring(startIndex), proposedSize.Width, font,
					flags.HasFlag(TextFormatFlags.TextBoxControl),
					out var breakPoint, out lineWidth);
				lineStr = line.Substring(startIndex, breakPoint + 1);
				startIndex += breakPoint + 1;
				maxLineWidth = Math.Max(maxLineWidth, lineWidth);
				if (startIndex < line.Length) { height += lineHeight; }
				lineNo++;
			}
		}
		else
		{
			var charWidthArr = GetCharWidthsByFont(font, ref height);
			var lineSize = line.Sum(c => GetCharWidth(c, charWidthArr));
			maxLineWidth = lineSize;
		}
		return new Size((int)Math.Ceiling(maxLineWidth), height);
	}

	public static (bool, int) MeasureMaximumLineCount(string line, Font font, Size proposedSize, TextFormatFlags flags)
	{
		var lineHeight = font.GetFontHeight();
		var maximumLineCount = Math.Max(proposedSize.Height / lineHeight, 1);

		var startIndex = 0;
		var lineNo = 0;
		while (startIndex < line.Length && lineNo < maximumLineCount)
		{
			lineNo++;

			CalcLineBreak(line.Substring(startIndex), proposedSize.Width, font,
				flags.HasFlag(TextFormatFlags.TextBoxControl),
				out var breakPoint, out _);

			startIndex += breakPoint + 1;
		}

		return (startIndex < line.Length && lineNo >= maximumLineCount, maximumLineCount);
	}

	public static Size MeasureText(BGraphics graphics, string text, Font font, Size proposedSize, TextFormatFlags flags) => MeasureText(text, font, proposedSize, flags);

	static double[] GetCharWidthsByFont(Font font, ref int height)
	{
		var fontStyle = FontStyle.Regular;
		if (font.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (font.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}

		var fontSize = font.Size;
		if (font.Unit == GraphicsUnit.Pixel)
		{
			fontSize *= 0.75f;
		}

		return TextSizeCaching.GetFontWidthData(font.Name, fontStyle, fontSize, ref height);
	}

	public static double GetEmptyCharWidth(Font font)
	{
		var height = 0;
		double[] fontList = GetCharWidthsByFont(font, ref height);
		return GetCharWidth(' ', fontList);
	}

	static void CalcLineBreak(string line, int fitWidth, Font font, bool splitMidWord,
		out int breakPoint, out double lineWidth)
	{
		var lineWidthArr = new double[line.Length];
		var lastWhiteSpaceIndex = -1;
		lineWidth = 0d;
		breakPoint = 0;
		var height = 0;
		var charWidthArr = GetCharWidthsByFont(font, ref height);
		for (var i = 0; i < line.Length; i++)
		{
			var c = line[i];
			var charWidth = GetCharWidth(c, charWidthArr);
			lineWidth = (i == 0 ? 0 : lineWidthArr[i - 1]) + charWidth;
			breakPoint = i;
			lineWidthArr[i] = lineWidth;
			if (lineWidth > fitWidth)
			{
				// spaces not found up to now but ends with a space, go back one char (unless rogue space at the start of the line)
				// When the last character is a space, do not find the previous space
				if (char.IsWhiteSpace(c) && i > 0)
				{
					lineWidth = lineWidthArr[i - 1];
					breakPoint = i - 1;
					break;
				}
				// spaces previously found, go back to last one to break
				if (lastWhiteSpaceIndex > -1)
				{
					lineWidth = lineWidthArr[lastWhiteSpaceIndex];
					breakPoint = lastWhiteSpaceIndex;
					break;
				}
				// spaces not found up to now and line ends on a word, or splitting in the middle of a word
				if (i == (line.Length - 1) || splitMidWord)
				{
					lineWidth = lineWidthArr[i];
					breakPoint = i;
					break;
				}
			}
			else
			{
				// record last white space found within length
				if (char.IsWhiteSpace(c))
				{
					lastWhiteSpaceIndex = i;
				}
			}
		}
	}

	public static bool IsFontSupportedInWinzor(string font)
	{
		return TextSizeCaching.IsFontSupported(font);
	}
}
