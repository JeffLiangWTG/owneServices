using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public static class MeasureTypeRetriever
	{
		public static Dictionary<string, MeasureType> GetMeasureTypes()
		{
			var result = new Dictionary<string, MeasureType>(staticMeasureTypes);

			foreach (var packType in new RefPackTypeCollection(new BusinessObjectFactory()))
			{
				if (!result.ContainsKey(packType.F3_Code))
				{
					result.Add(packType.F3_Code, MeasureType.Unit);
				}
			}

			return result;
		}

		static readonly ConcurrentDictionary<string, MeasureType> staticMeasureTypes = GetStaticMeasureTypes().ToConcurrent();

		static Dictionary<string, MeasureType> GetStaticMeasureTypes()
		{
			var result = new Dictionary<string, MeasureType>();

			Core.Constants.Weight.Codes.ForEach(code => result.Add(code, MeasureType.Weight));
			Core.Constants.Volume.Codes.ForEach(code => result.Add(code, MeasureType.Volume));

			var infos = typeof(QuantityUnit).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var info in infos)
			{
				var measures = info.GetCustomAttributes(typeof(QuantityUnit.MeasureTypeAttribute), false);
				if (measures.Length == 1)
				{
					result.Add((string)info.GetValue(null), ((QuantityUnit.MeasureTypeAttribute)measures[0]).MeasureType);
				}
			}

			return result;
		}
	}
}

