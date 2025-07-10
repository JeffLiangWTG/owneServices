using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class CompareColumnsEquality<T> : IEqualityComparer<T>
	{
		public bool Equals(T x, T y)
		{
			var xProperty = x as IDictionary<string, object>;
			var yProperty = y as IDictionary<string, object>;

			if (xProperty.Count != yProperty.Count)
			{
				return false;
			}
			if (xProperty.Keys.Except(yProperty.Keys).Any())
			{
				return false;
			}
			if (yProperty.Keys.Except(xProperty.Keys).Any())
			{
				return false;
			}
			foreach (var pair in xProperty)
			{
				if (yProperty.ContainsKey(pair.Key))
				{
					var yValue = yProperty[pair.Key];
					return CompareHelper.MatchPropertyValues(pair.GetType(), pair.Value, yValue, Operations.Equals, StringComparison.OrdinalIgnoreCase);
				}
			}
			return true;
		}

		public int GetHashCode(T obj)
		{
			return obj.ToString().GetHashCode();
		}
	}
}
