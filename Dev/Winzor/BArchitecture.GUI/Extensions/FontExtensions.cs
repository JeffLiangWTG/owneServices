using System.Collections.Concurrent;
using System.Drawing;

namespace WinzorFramework.Extensions;

public static class FontExtensions
{
	public static int GetFontHeight(this Font font)
	{
		return fontHeightCache.GetOrAdd(new FontKey(font), _ => font.Height);
	}

	static readonly ConcurrentDictionary<FontKey, int> fontHeightCache = new ConcurrentDictionary<FontKey, int>();

	class FontKey
	{
		public FontKey(Font font)
		{
			this.Name = font.Name;
			this.Size = font.Size;
			this.Style = font.Style;
		}

		public string Name { get; }
		public float Size { get; }
		public FontStyle Style { get; }

		public override bool Equals(object? obj)
		{
			return obj is FontKey key &&
				   Name == key.Name &&
				   Size == key.Size &&
				   Style == key.Style;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Size, Style);
		}
	}
}
