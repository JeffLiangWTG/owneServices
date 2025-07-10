namespace System.Windows.Forms;
public interface IParsable
{
	object Parse(string input, IFormatProvider formatProvider);
}

public class DateTimeParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
#pragma warning disable CW1122 // Do Not Use DateTime Parse Method
		return DateTime.Parse(input, formatProvider);
#pragma warning restore CW1122 // Do Not Use DateTime Parse Method
	}
}

public class IntParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return int.Parse(input, formatProvider);
	}
}
public class DoubleParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return double.Parse(input, formatProvider);
	}
}
public class DecimalParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return decimal.Parse(input, formatProvider);
	}
}
public class BooleanParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return bool.Parse(input);
	}
}
public class FloatParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return float.Parse(input, formatProvider);
	}
}
public class ByteParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return byte.Parse(input, formatProvider);
	}
}
public class ShortParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return short.Parse(input, formatProvider);
	}
}
public class LongParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return long.Parse(input, formatProvider);
	}
}
public class GuidParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		return Guid.Parse(input);
	}
}
public class CharParser : IParsable
{
	public object Parse(string input, IFormatProvider formatProvider)
	{
		if (input == null)
		{
			throw new ArgumentNullException(nameof(input));
		}
		if (input.Length != 1)
		{
			throw new FormatException("Input string must be exactly one character long.");
		}
		return input[0];
	}
}

public static class ParserFactory
{
	static readonly Dictionary<Type, IParsable> parsers = new Dictionary<Type, IParsable>
	{
		{ typeof(DateTime), new DateTimeParser() },
		{ typeof(int), new IntParser() },
		{ typeof(double), new DoubleParser() },
		{ typeof(decimal), new DecimalParser() },
		{ typeof(bool), new BooleanParser() },
		{ typeof(float), new FloatParser() },
		{ typeof(byte), new ByteParser() },
		{ typeof(short), new ShortParser() },
		{ typeof(long), new LongParser() },
		{ typeof(Guid), new GuidParser() },
		{ typeof(char), new CharParser() }
	};
	public static IParsable GetParser(Type type)
	{
		if (parsers.TryGetValue(type, out var parser))
		{
			return parser;
		}
		throw new NotSupportedException($"Parsing not supported for type {type.Name}");
	}
}

