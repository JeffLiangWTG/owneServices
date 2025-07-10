using System;
using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;

namespace WinzorFramework;
class ParserHelperTest
{
	[Test]
	public void DateTimeParser_ValidInput_ParsesCorrectly()
	{
		var parser = new DateTimeParser();
		var input = "2024-07-30T13:45:00";
#pragma warning disable CW1122 // Do Not Use DateTime Parse Method -- allow for testing
		var expected = DateTime.Parse(input, CultureInfo.InvariantCulture);
#pragma warning restore CW1122 // Do Not Use DateTime Parse Method
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void DateTimeParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new DateTimeParser();
		var input = "InvalidDateTime";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void IntParser_ValidInput_ParsesCorrectly()
	{
		var parser = new IntParser();
		var input = "123";
		var expected = int.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void IntParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new IntParser();
		var input = "InvalidInt";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void DoubleParser_ValidInput_ParsesCorrectly()
	{
		var parser = new DoubleParser();
		var input = "123.45";
		var expected = double.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void DoubleParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new DoubleParser();
		var input = "InvalidDouble";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void DecimalParser_ValidInput_ParsesCorrectly()
	{
		var parser = new DecimalParser();
		var input = "123.45";
		var expected = decimal.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void DecimalParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new DecimalParser();
		var input = "InvalidDecimal";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void BooleanParser_ValidInput_ParsesCorrectly()
	{
		var parser = new BooleanParser();
		var input = "true";
		var expected = bool.Parse(input);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void BooleanParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new BooleanParser();
		var input = "InvalidBool";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void FloatParser_ValidInput_ParsesCorrectly()
	{
		var parser = new FloatParser();
		var input = "123.45";
		var expected = float.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void FloatParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new FloatParser();
		var input = "InvalidFloat";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void ByteParser_ValidInput_ParsesCorrectly()
	{
		var parser = new ByteParser();
		var input = "123";
		var expected = byte.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void ByteParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new ByteParser();
		var input = "InvalidByte";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void ShortParser_ValidInput_ParsesCorrectly()
	{
		var parser = new ShortParser();
		var input = "123";
		var expected = short.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void ShortParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new ShortParser();
		var input = "InvalidShort";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void LongParser_ValidInput_ParsesCorrectly()
	{
		var parser = new LongParser();
		var input = "1234567890123";
		var expected = long.Parse(input, CultureInfo.InvariantCulture);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void LongParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new LongParser();
		var input = "InvalidLong";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void GuidParser_ValidInput_ParsesCorrectly()
	{
		var parser = new GuidParser();
		var input = "12345678-1234-1234-1234-1234567890ab";
		var expected = Guid.Parse(input);
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void GuidParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new GuidParser();
		var input = "InvalidGuid";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void CharParser_ValidInput_ParsesCorrectly()
	{
		var parser = new CharParser();
		var input = "A";
		var expected = 'A';
		var result = parser.Parse(input, CultureInfo.InvariantCulture);
		Assert.That(expected, Is.EqualTo(result));
	}

	[Test]
	public void CharParser_InvalidInput_ThrowsFormatException()
	{
		var parser = new CharParser();
		var input = "AB";
		Assert.Throws<FormatException>(() => parser.Parse(input, CultureInfo.InvariantCulture));
	}

	[Test]
	public void CharParser_NullInput_ThrowsArgumentNullException()
	{
		var parser = new CharParser();
		Assert.Throws<ArgumentNullException>(() => parser.Parse(null, CultureInfo.InvariantCulture));
	}

	[Test]
	public void ParserFactory_UnsupportedType_ThrowsNotSupportedException()
	{
		Assert.Throws<NotSupportedException>(() => ParserFactory.GetParser(typeof(DateTimeOffset)));
	}
}
