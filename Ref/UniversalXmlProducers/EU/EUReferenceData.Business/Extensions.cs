using System.Linq;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public static class Extensions
	{
		public static string SubstringSafe(this string value, int startIndex, int length)
		{
			if (value == null)
			{
				return null;
			}

			return new string(value.Skip(startIndex)
				.Take(length)
				.ToArray());
		}

		public static string Left(this string value, int lenght) => value.SubstringSafe(0, lenght);

		public static string TrimEnd(this string source, string value)
		{
			if (source == null)
			{
				return null;
			}

			if (value == null || !source.EndsWith(value, System.StringComparison.InvariantCulture))
			{
				return source;
			}

			var trimmedString = source.Remove(source.LastIndexOf(value, System.StringComparison.InvariantCulture));

			if (source != trimmedString)
			{
				return trimmedString.TrimEnd(value);
			}
			return trimmedString;
		}
	}
}
