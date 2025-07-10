using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public static class CompareHelper
	{
		public static bool IsEquals(string value1, string value2, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase) => (string.IsNullOrEmpty(value1) && string.IsNullOrEmpty(value2)) || value1.Equals(value2, stringComparison);

		public static bool IsEquals(Guid? value1, Guid? value2) => (value1 == null && value2 == null) || value1 == value2;

		public static bool IsEquals(DateTimeOffset value1, DateTimeOffset value2) => value1 == value2;

		public static bool IsEquals(byte value1, byte value2) => value1 == value2;

		public static bool IsEquals(bool value1, bool value2) => value1 == value2;

		public static bool IsEquals(int value1, int value2) => value1 == value2;

		public static bool MatchPropertyValues(Type type, object propertyValue1, object propertyValue2, Operations operation = Operations.Equals, StringComparison comparisonType = StringComparison.Ordinal)
		{
			if (operation == Operations.StartsWith && type != typeof(string))
			{
				throw new InvalidOperationException("StartsWith operation must only be used with string types");
			}

			var result = propertyValue1 == null && propertyValue2 == null;
			if (!result)
			{
				if ((propertyValue1 == null && propertyValue2 != null) || (propertyValue2 == null))
				{
					return false;
				}

				if (type == typeof(byte[]))
				{
					return ((byte[])propertyValue1).SequenceEqual((byte[])propertyValue2);
				}

				switch (operation)
				{
					case Operations.Equals:
						if (type == typeof(string))
						{
							result = string.Compare((string)propertyValue1, (string)propertyValue2, comparisonType) == 0;
						}
						else if (type == typeof(SerializedGeometry))
						{
							result = (((SerializedGeometry)propertyValue1).Geography?.WellKnownText)?.Replace("SRID=4326;", "") == (((SerializedGeometry)propertyValue2).Geography?.WellKnownText)?.Replace("SRID=4326;", "");
						}
						else
						{
							result = propertyValue1.Equals(propertyValue2);
						}
						break;
					case Operations.StartsWith:
						result = ((string)propertyValue1).StartsWith((string)propertyValue2, comparisonType);
						break;
				}
			}
			return result;
		}
	}
}
