using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V19Transform : ITransformStrategy
	{
		static V19Transform v19Transform;

		V19Transform() { }

		public static V19Transform Instance()
		{
			if (v19Transform == null)
			{
				v19Transform = new V19Transform();
			}
			return v19Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 19 && dataSetType == typeof(RefUNLOCO);
		}

		public T Transform<T>(T data)
		{
			var result = data;
			if (result != null)
			{
				if (result is RefUNLOCO)
				{
					var item = result as RefUNLOCO;
					item.RL_CoOrdinates = GetCoOrdinateStringFromDbGeography(item.RL_GeoLocation);
				}
			}
			return result;
		}

		static string GetCoOrdinateStringFromDbGeography(string dbGeography)
		{
			if (!string.IsNullOrEmpty(dbGeography))
			{
				var reader = new WKTReader();
				var point = reader.Read(dbGeography) as Point;
				if (point != null && !point.IsEmpty)
				{
					var latitude = Math.Abs(Math.Truncate((decimal)point.Y)).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
									+ Math.Abs(decimal.Round(((decimal)point.Y - Math.Truncate((decimal)point.Y)) * 60)).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
									+ (point.Y < 0 ? "S" : "N");
					var longitude = Math.Abs(Math.Truncate((decimal)point.X)).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')
									+ Math.Abs(decimal.Round(((decimal)point.X - Math.Truncate((decimal)point.X)) * 60)).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
									+ (point.X < 0 ? "W" : "E");
					return $"{latitude} {longitude}";
				}
			}
			return string.Empty;
		}

		readonly Type[] typesToBeTransformed = {
				typeof(RefUNLOCO)
			};

		public Type[] ToBeTransformedTypes
		{
			get { return typesToBeTransformed; }
		}
	}
}
