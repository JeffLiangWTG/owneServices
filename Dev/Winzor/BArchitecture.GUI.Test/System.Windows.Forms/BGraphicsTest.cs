using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms;

class BGraphicsTest
{
	[TestCase("a", 13, 16)]
	[TestCase("a\r\na", 13, 33)]
	[TestCase("Example Text", 78, 16)]
	[TestCase("Lorem ipsum dolor sit amet", 152, 16)]
	public void MeasureStringSmallerThanProposedSize(string text, int correctWidth, int correctHeight)
	{
		using (var g = new BGraphics())
		{
			using var font = new Font("Tahoma", 9f);
			var size = g.MeasureString(text, font, new Size(int.MaxValue, int.MaxValue));
			Assert.That(size.Width, Is.EqualTo(correctWidth));
			Assert.That(size.Height, Is.EqualTo(correctHeight));
		}
	}

	[TestCase("a", 1, 1)]
	[TestCase("a\r\na", 1, 1)]
	[TestCase("Example Text", 10, 10)]
	[TestCase("Lorem ipsum dolor sit amet", 10, 10)]
	public void MeasureStringLimitedToProposedSize(string text, int proposedWidth, int proposedHeight)
	{
		using (var g = new BGraphics())
		{
			using var font = new Font("Tahoma", 9f);
			var size = g.MeasureString(text, font, new Size(proposedWidth, proposedHeight));
			Assert.That(size.Width, Is.Not.GreaterThan(proposedWidth));
			Assert.That(size.Height, Is.Not.GreaterThan(proposedHeight));
		}
	}

	[Test]
	public void MeasureStringReturnsZeroSizeIfBlankString()
	{
		using (var g = new BGraphics())
		{
			using var font = new Font("Tahoma", 9f);
			var size = g.MeasureString("", font);
			Assert.That(size.Height, Is.EqualTo(0));
			Assert.That(size.Width, Is.EqualTo(0));
		}
	}
}
