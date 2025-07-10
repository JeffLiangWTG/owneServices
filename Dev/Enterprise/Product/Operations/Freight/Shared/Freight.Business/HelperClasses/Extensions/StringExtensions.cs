using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Extensions
{
	public static class StringExtensions
	{
		/// <summary>
		///		Converts the string to a <see cref="KeyValuePair"/> with the value <paramref name="value"/>.
		/// </summary>
		/// <param name="key">
		///		The string to convert. It will be converted to a key in pair.
		/// </param>
		/// <param name="value">
		///		The value of the <see cref="KeyValuePair"/>.
		/// </param>
		public static KeyValuePair<string, string> AsKeyFor(this string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}

		public static ZString FallbackIfEmpty(this ZString value, ZString fallbackValue)
		{
			return !value.IsEmpty ? value : fallbackValue;
		}
	}
}
