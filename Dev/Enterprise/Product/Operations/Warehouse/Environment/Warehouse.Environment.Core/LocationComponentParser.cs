using CargoWise.Common;

namespace Enterprise.Warehouse.Environment.Core
{
	public static class LocationComponentParser
	{
		public static short Parse(string value, bool useAlpha)
		{
			Argument.NotNull(value, nameof(value));

			short result = -1;
			if (useAlpha)
			{
				if (value.Length == 1)
				{
					char a = value[0];
					if (a >= 'A' && a <= 'Z')
					{
						result = ((short)(a - 'A' + 1));
					}
					else if (a >= 'a' && a <= 'z')
					{
						result = ((short)(a - 'a' + 1));
					}
				}
			}

			if (result == -1)
			{
				short parsedResult;
				if (short.TryParse(value, out parsedResult))
				{
					result = parsedResult;
				}
			}

			return result;
		}
	}
}
