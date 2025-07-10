using System.Diagnostics;
using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms;

class TextRendererTest
{
	readonly Font font = new Font("tahoma", 8f);
	const int LineHeight = 13;

	[TestCase("Arial", 11f, FontStyle.Regular, 32, 17)]
	[TestCase("Arial", 11f, FontStyle.Italic, 32, 17)]
	[TestCase("Arial", 12f, FontStyle.Regular, 35, 19)]
	[TestCase("Arial", 12f, FontStyle.Bold, 37, 19)]
	[TestCase("Arial", 16f, FontStyle.Regular, 47, 25)]
	[TestCase("Segoe UI", 9f, FontStyle.Regular, 25, 16)]
	[TestCase("Segoe UI Symbol", 11f, FontStyle.Regular, 31, 20)]
	[TestCase("Tahoma", 7f, FontStyle.Bold, 22, 12)]
	[TestCase("Tahoma", 8f, FontStyle.Italic, 23, 13)]
	[TestCase("Tahoma", 9f, FontStyle.Regular, 25, 15)]
	[TestCase("Tahoma", 15f, FontStyle.Bold, 47, 25)]
	[TestCase("Times New Roman", 13f, FontStyle.Regular, 34, 20)]
	public void MeasureFont(string fontFace, float fontSize, FontStyle fontType, int width, int height)
	{
		using Font selectedFontCombo = new Font(fontFace, fontSize, fontType);
		var res = TextRenderer.MeasureText("abc", selectedFontCombo);
		Assert.That(res.Width, Is.EqualTo(width));
		Assert.That(res.Height, Is.EqualTo(height));
	}

	[TestCase("Tahoma", 40f, FontStyle.Bold, 124, 65)]                      //	No larger size
	[TestCase("Tahoma", 5f, FontStyle.Regular, 14, 9)]                      //	No smaller size
	[TestCase("Tahoma", 26f, FontStyle.Regular, 73, 42)]                    //	In the middle
	[TestCase("Century", 14f, FontStyle.Italic, 27, 13)]                    //	Not existing font
	[TestCase("Comic Sans MS", 30f, FontStyle.Bold, 41, 13)]                //	Not existing style
	public void MeasureFont_MissingFont_Handled(string fontFace, float fontSize, FontStyle fontType, int width, int height)
	{
		using Font selectedFontCombo = new Font(fontFace, fontSize, fontType);
		var res = TextRenderer.MeasureText("abc", selectedFontCombo);
		Assert.That(res.Width, Is.EqualTo(width));
		Assert.That(res.Height, Is.EqualTo(height));
	}

	[Test]
	public void MeasureTahoma_Normal_9pt()
	{
		using Font tahomaNormal_9pt = new Font("tahoma", 9f);
		var res = TextRenderer.MeasureText("Hello World!", tahomaNormal_9pt);
		Assert.That(res.Width, Is.EqualTo(72));
		Assert.That(res.Height, Is.EqualTo(15));
	}

	[Test]
	public void FontNotInCacheShouldHaveCorrectLeftSpacing()
	{
		using var font = new Font("Tahoma", 56f);
		var leftSpacing = TextRenderer.GetEmptyCharWidth(font);
		Assert.That(leftSpacing, Is.EqualTo(23.3).Within(0.1));
	}

	[TestCase("Tahoma", 8f, FontStyle.Bold, 6)]
	[TestCase("Tahoma", 16f, FontStyle.Regular, 12)]
	[TestCase("Tahoma", 32f, FontStyle.Italic, 24)]
	[TestCase("Tahoma", 40f, FontStyle.Italic | FontStyle.Bold, 30)]
	public void WidthAdjustmentShouldChangeWithFontSize(string fontFace, float fontSize, FontStyle fontType, int expectedWidth)
	{
		using var font = new Font(fontFace, fontSize, fontType);
		var width = TextRenderer.GetWidthAdjustment(font);
		Assert.That(width, Is.EqualTo(expectedWidth));
	}

	[Test]
	public void MeasureLineWithTahoma_Italic_8pt()
	{
		// Arrange
		using var italicFont = new Font("tahoma", 8f, FontStyle.Italic);

		// Act
		var res = TextRenderer.MeasureText("!@#$%^&*()()(**&^$!@@#$%^&*()*&^ Hello World! @#$%^&*()()(**&^$!@@#$%^&*()*&^", italicFont);

		// Assert
		Assert.That(res.Width, Is.EqualTo(425));
		Assert.That(res.Height, Is.EqualTo(13));
	}

	[Test]
	public void MeasureBlankText()
	{
		var res = TextRenderer.MeasureText(" ", font);
		Assert.That(res.Width, Is.EqualTo(10));
		Assert.That(res.Height, Is.EqualTo(LineHeight));

		res = TextRenderer.MeasureText("   \r\n", font);
		Assert.That(res.Width, Is.EqualTo(16));
		Assert.That(res.Height, Is.EqualTo(LineHeight * 2));

		res = TextRenderer.MeasureText("   \r\n\r\n", font);
		Assert.That(res.Width, Is.EqualTo(16));
		Assert.That(res.Height, Is.EqualTo(LineHeight * 3));
	}

	[Test]
	public void MeasureOneLineText()
	{
		var res = TextRenderer.MeasureText("aaaa bbbb cccc", font);
		Assert.That(res.Width, Is.EqualTo(79));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[TestCase("关闭(C)", "Segoe UI", 9, 44)]
	[TestCase("关闭(C)", "Segoe UI", 10, 50)]
	[TestCase("关闭(C)", "Segoe UI", 13, 64)]
	[TestCase("Close", "Segoe UI", 9, 35)]
	[TestCase("Close", "Segoe UI", 10, 39)]
	[TestCase("Close", "Segoe UI", 13, 51)]
	[TestCase("保存并关闭(A)", "Segoe UI", 9, 80)]
	[TestCase("保存并关闭(A)", "Segoe UI", 10, 89)]
	[TestCase("保存并关闭(A)", "Segoe UI", 13, 115)]
	public void MeasureOneLineTextWidth(string testText, string fontName, int fontPt, int expectedStringWidth)
	{
		using Font font = new Font(fontName, fontPt);

		var res = TextRenderer.MeasureText(testText, font);
		Assert.That(res.Width, Is.EqualTo(expectedStringWidth));
	}

	[TestCase('\u1111', 17, TestName = "{m}_RandomUnicodeChar")]
	[TestCase('\u25B2', 25, TestName = "{m}_UpPointingTriangle")]
	[TestCase('\u25BC', 25, TestName = "{m}_DownPointingTriangle")]
	public void MeasureUnicode(char unicodeChar, int expectedWidth)
	{
		var res = TextRenderer.MeasureText(unicodeChar.ToString(), font);
		Assert.That(res.Width, Is.EqualTo(expectedWidth));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[Test]
	public void MeasureOneMultiLineText()
	{
		var res = TextRenderer.MeasureText("aaaa\r\naaaa bbbb cccc\r\ncccc", font);
		Assert.That(res.Width, Is.EqualTo(79));
		Assert.That(res.Height, Is.EqualTo(LineHeight * 3));
	}

	[TestCase("Customer Service Ticket - CST00000003", 500, 40, false, 0, Description = "Not Overflowed")]
	[TestCase("Customer Service Ticket - CST00000003 - Please, sir, may I have abcdefghi", 120, 39, false, 0, Description = "Edge case for Overflowed or Not Overflowed (Not Overflowed Side)")]
	[TestCase("Customer Service Ticket - CST00000003 - Please, sir, may I have abcdefghij", 120, 39, true, 3, Description = "Edge case for Overflowed or Not Overflowed (Overflowed Side)")]
	[TestCase("Customer Service Ticket - CST00000003 - Please, sir, may I have a message? - Job Customer Service Ticket - CST00000003 - Please, sir, may I have a message? is complete.", 100, 40, true, 3, Description = "Overflowed")]
	[TestCase("Customer Service Ticket - CST00000003 - Please, sir, may I have a message? - Job Customer Service Ticket - CST00000003 - Please, sir, may I have a message? is complete.", 100, 39, true, 3, Description = "Overflowed: Edge case for MaximumLineCount")]
	[TestCase("Customer Service Ticket - CST00000003 - Please, sir, may I have a message? - Job Customer Service Ticket - CST00000003 - Please, sir, may I have a message? is complete.", 100, 38, true, 2, Description = "Overflowed: Edge case for MaximumLineCount")]
	public void MeasureMaximumLineCount(string text, int width, int height, bool expectedOverflowed, int expectedMaximumLineCount)
	{
		using var fontFamily = new FontFamily("Tahoma");
		using Font font = new Font(fontFamily, 8);
		(var overflowed, int maximumLineCount) = TextRenderer.MeasureMaximumLineCount(text, font, new Size(width, height), TextFormatFlags.WordBreak);
		Assert.That(overflowed, Is.EqualTo(expectedOverflowed));
		if (overflowed)
		{
			Assert.That(maximumLineCount, Is.EqualTo(expectedMaximumLineCount));
		}
	}

	const string StdFontName = "Tahoma";
	const int StdFontPt = 8;
	const int StdLineHeight = 13;
	const string SansSerifFontName = "Microsoft Sans Serif";
	const int SansSerifFontPt = 8;
	const int SansSerifLineHeight = 13;

	[TestCase("aa", 18, 1, 20, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-StdAa")]
	[TestCase("\r\n", 0, 2, 20, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-StdBreakOnly")]
	[TestCase("aa bbbb cccccccc", 46, 4, 20, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-StdThreeWordOverLong")]
	[TestCase("aa bbbb                   cccccccc", 46, 8, 16, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std3WordOverlongWithWhiteSpaceBlockInMiddle")]
	[TestCase("aa bbbb cccccccc dddd", 46, 4, 40, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std4WordOverLong")]
	[TestCase("aa bbbb cccccccc\r\ndddddd\r\neeeeeeeeeeee fff", 74, 5, 50, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std5WordOverLongWithBreaks")]
	[TestCase("aa bbbb cccccccc\r\ndddddd\r\n\r\neeeeeeeeeeee fff", 74, 6, 50, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std6WordOverLongWithBreaksAndSpaces")]
	[TestCase("     cccccccc", 46, 2, 50, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std1WordOverlongWithLeadingSpaces")]
	[TestCase("                   cccccccc", 50, 3, 46, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std1WordOverlongWithManyLeadingSpaces")]
	[TestCase("cccccccc", 46, 1, 20, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std1WordOverlong")]
	[TestCase("ccc      ", 21, 3, 16, 20,
		StdLineHeight, StdFontName, StdFontPt,
		TestName = "Measure-Std1WordTrailingSpaces")]
	[TestCase("aa", 18, 1, 20, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerifAa")]
	[TestCase("\r\n", 0, 2, 20, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerifBreakOnly")]
	[TestCase("aa bbbb cccccccc", 49, 4, 20, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerifThreeWordOverLong")]
	[TestCase("aa bbbb                   cccccccc", 49, 7, 16, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif3WordOverlongWithWhiteSpaceBlockInMiddle")]
	[TestCase("aa bbbb cccccccc dddd", 49, 4, 40, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif4WordOverLong")]
	[TestCase("aa bbbb cccccccc\r\ndddddd\r\neeeeeeeeeeee fff", 78, 5, 50, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif5WordOverLongWithBreaks")]
	[TestCase("aa bbbb cccccccc\r\ndddddd\r\n\r\neeeeeeeeeeee fff", 78, 6, 50, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif6WordOverLongWithBreaksAndSpaces")]
	[TestCase("     cccccccc", 49, 2, 50, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif1WordOverlongWithLeadingSpaces")]
	[TestCase("                   cccccccc", 52, 3, 46, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif1WordOverlongWithManyLeadingSpaces")]
	[TestCase("cccccccc", 49, 1, 20, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif1WordOverlong")]
	[TestCase("ccc      ", 22, 3, 16, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif1WordTrailingSpaces")]
	[TestCase("Assign to BOR", 50, 2, 44, 20,
		SansSerifLineHeight, SansSerifFontName, SansSerifFontPt,
		TestName = "Measure-SansSerif1WordEndSpaceNewline")]
	public void MeasureTextWithWordBreakNew(string text, int expectedWidth,
		int expectedNoLines, int proposedWidth, int proposedHeight, int expectedLineHeight,
		string fontName, int fontPt)
	{
		using Font font = new Font(fontName, fontPt);
		var res = TextRenderer.MeasureText(text, font, new Size(proposedWidth, proposedHeight), TextFormatFlags.WordBreak);
		Assert.That(res.Width, Is.EqualTo(expectedWidth));
		Assert.That(res.Height, Is.EqualTo(expectedLineHeight * expectedNoLines));
	}

	[Test]
	public void MeasureTextWithMnemonic()
	{
		var res = TextRenderer.MeasureText("&Test", font);
		Assert.That(res.Width, Is.EqualTo(27));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[Test]
	public void MeasureTextWithMnemonicNoPrefix()
	{
		var res = TextRenderer.MeasureText("&Test", font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
		Assert.That(res.Width, Is.EqualTo(34));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[Test]
	public void MeasureTextWithEscapedMnemonic()
	{
		var res = TextRenderer.MeasureText("&&Test", font);
		Assert.That(res.Width, Is.EqualTo(34));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[Test]
	public void MeasureTextWithEscapedMnemonicNoPrefix()
	{
		var res = TextRenderer.MeasureText("&&Test", font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPrefix);
		Assert.That(res.Width, Is.EqualTo(41));
		Assert.That(res.Height, Is.EqualTo(LineHeight));
	}

	[TestCase("Found record(s): 0 of 0", 26)]
	[TestCase("Found record(s): 0 of 0\r\n", 26)]
	[TestCase("Found record(s): 0 of 0\r\n\r\n", 39)]
	[TestCase("Found\r\nrecord(s)\r\n", 26)]
	[TestCase("Found\r\n\r\nrecord(s)\r\n", 39)]
	public void MeasureTextHeightWithTextboxControlAndWordBreak(string text, int expectedHeight)
	{
		var res = TextRenderer.MeasureText(text, font, new Size(100, 100), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
		Assert.That(res.Height, Is.EqualTo(expectedHeight));
	}

	[Test]
	public void MeasureTextWithEmptyString()
	{
		var res = TextRenderer.MeasureText("", font);
		Assert.That(res.Width, Is.EqualTo(0));
		Assert.That(res.Height, Is.EqualTo(0));
	}

	[TestCase(FontStyle.Regular, 127)]
	[TestCase(FontStyle.Bold, 140)]
	public void MeasureLineWithFontBold(FontStyle fontStyle, int expectedWidth)
	{
		using var font = new Font("Tahoma", 11f, fontStyle);
		var size = TextRenderer.MeasureLine("!@#Aaa Bb Cc Test", font, new Size(int.MaxValue, int.MaxValue),
			TextFormatFlags.NoPrefix);
		Assert.That(size.Width, Is.EqualTo(expectedWidth));
	}

	[Test]
	public void MeasureTextWithSpaceAsFirstChar()
	{
		var res = TextRenderer.MeasureText(" Test", font);
		Assert.That(res.Width, Is.GreaterThan(0));
		Assert.That(res.Height, Is.GreaterThan(0));
	}

	[Test]
	public void MeasureTextWithExactWidthNeeded()
	{
		var size1 = TextRenderer.MeasureText("any text", font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
		var size2 = TextRenderer.MeasureText("any text", font, new Size(size1.Width - TextRenderer.GetWidthAdjustment(font), int.MaxValue), TextFormatFlags.WordBreak);
		Assert.That(size2, Is.EqualTo(size1));
	}

	[Test]
	public void MeasureTextIsFast()
	{
		// Warm-up
		for (var i = 0; i < 100; i++)
		{
			TextRenderer.MeasureText(i.ToString(), font);
		}

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		TextRenderer.MeasureText("...", font);
		var stopwatch = Stopwatch.StartNew();
		for (var i = 0; i < 1000; i++)
		{
			TextRenderer.MeasureText(i.ToString(), font);
		}
		Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromMilliseconds(50)));
	}

	[Test]
	public void MeasureTextShouldConsiderWrappedTextHeight()
	{
		var text = "This is the text that need to be measured and is intentionally to make it long enough so it will be wrapped to multi-lines";
		var singleLineTextSize = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.Default);
		var numberOfLines = 2;
		var simulatedTextBoxWidth = singleLineTextSize.Width / numberOfLines;

		var wrappedTextHeight = TextRenderer.MeasureText(text, font, new Size(simulatedTextBoxWidth, int.MaxValue), TextFormatFlags.WordBreak).Height;
		Assert.That(wrappedTextHeight, Is.GreaterThanOrEqualTo(singleLineTextSize.Height * numberOfLines));
	}

	[TestCase("Arial", true)]
	[TestCase("Segoe UI", true)]
	[TestCase("Segoe UI Symbol", true)]
	[TestCase("Times New Roman", true)]
	[TestCase("Times New", false)]
	[TestCase("RandomName", false)]
	[TestCase("RandomFont", false)]
	public void TestIsFontCached(string font, bool expectedResult)
	{
		Assert.That(TextRenderer.IsFontSupportedInWinzor(font), Is.EqualTo(expectedResult));
	}

	[Test]
	public void MeasureTextFontUnit()
	{
		using Font fontPixel = new Font("Arial", 52f, FontStyle.Bold, GraphicsUnit.Pixel);
		using Font fontPoint = new Font("Arial", 39f, FontStyle.Bold, GraphicsUnit.Point);
		var pixelSize = TextRenderer.MeasureText("Hello World!", fontPixel);
		var pointSize = TextRenderer.MeasureText("Hello World!", fontPoint);
		Assert.That(pixelSize.Width, Is.EqualTo(pointSize.Width));
		Assert.That(pixelSize.Height, Is.EqualTo(pointSize.Height));
	}

	[Test]
	public void LabelWidthSameWhenFontSizeChange()
	{
		for (int i = 0; i < 10; i++)
		{
			using var fontPixelInitial = new Font("Arial", (float)8.1, FontStyle.Bold, GraphicsUnit.Pixel);
			using var fontPixelfinal = new Font("Arial", (float)8.1, FontStyle.Bold, GraphicsUnit.Pixel);
			var res1 = TextRenderer.MeasureText("TestForSizeShouldBeSameForDifferentSizes", fontPixelInitial);
			var res2 = TextRenderer.MeasureText("TestForSizeShouldBeSameForDifferentSizes", fontPixelfinal);
			Assert.That(res1.Width, Is.EqualTo(res2.Width));
		}
	}

	[TestCase("\n")]
	[TestCase("\n\n\n\n\n\n")]
	[TestCase("a\na")]
	[TestCase("\na\na\n")]
	public void MeasureTextReturnEqualSizesForWindowsAndUnixNewlines(string input)
	{
		using var font = new Font("Tahoma", 8f);

		var unix = TextRenderer.MeasureText(input, font);
		var windows = TextRenderer.MeasureText(input.Replace("\n", "\r\n"), font);
		Assert.That(unix, Is.EqualTo(windows));
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		font.Dispose();
	}
}
