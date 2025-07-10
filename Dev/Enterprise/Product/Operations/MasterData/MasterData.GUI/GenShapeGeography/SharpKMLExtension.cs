using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Enterprise.ZArchitecture.Core;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace Enterprise.MasterData.GUI
{
	static class SharpKMLExtension
	{
		public static string AsWKT(this Placemark placemark, out long pointsCount)
		{
			pointsCount = 0;

			if (placemark == null)
			{
				throw new ArgumentNullException(nameof(placemark));
			}

			if (!(placemark.Geometry is MultipleGeometry) && !(placemark.Geometry is Polygon))
			{
				throw new NotImplementedException(Res.GetString("EA0B35F4-543C-4F47-934F-EEB2AEF1FABA", "The KML file do not have spatial type Polygon or Multi-Polygon."));
			}

			var coordinates = placemark.ConvertToCoordinates();

			if (placemark.Geometry is MultipleGeometry)
			{
				return GenerateMultiplePolygonWKT(coordinates, ref pointsCount);
			}

			return GeneratePolygonWKT(coordinates.FirstOrDefault(), ref pointsCount);
		}

		static string AsWKT(this Vector[][] polygon, ref long pointsCount)
		{
			var sb = new StringBuilder("((");
			sb.Append(polygon[0].AsCoordinateString(ref pointsCount));
			foreach (var innerRing in polygon.Skip(1))
			{
				sb.Append("),(");
				sb.Append(innerRing.AsCoordinateString(ref pointsCount));
			}
			sb.Append("))");
			return sb.ToString();
		}

		static string AsCoordinateString(this Vector[] vectors, ref long pointsCount)
		{
			var cordStrings = vectors.Select(v => v.AsCoordinatePair()).ToList();
			pointsCount += cordStrings.Count;
			return string.Join(", ", cordStrings);
		}

		static string AsCoordinatePair(this Vector coordinate)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", coordinate.Longitude, coordinate.Latitude);
		}

		static List<Vector[][]> ConvertToCoordinates(this Placemark placemark)
		{
			var polygons = new List<Vector[][]>();

			foreach (var polygon in placemark.Flatten().OfType<Polygon>())
			{
				polygons.Add(polygon.AsVectorCoordinates());
			}

			return polygons;
		}

		static string GenerateMultiplePolygonWKT(List<Vector[][]> polygons, ref long pointsCount)
		{
			var sb = new StringBuilder();
			sb.Append((NoResString)"MULTIPOLYGON (");
			sb.Append(polygons[0].AsWKT(ref pointsCount));
			foreach (var polygon in polygons.Skip(1))
			{
				sb.Append(",");
				sb.Append(polygon.AsWKT(ref pointsCount));
			}
			sb.Append(")");
			return sb.ToString();
		}

		static string GeneratePolygonWKT(Vector[][] polygon, ref long pointsCount)
		{
			var sb = new StringBuilder();
			sb.Append("POLYGON ");
			sb.Append(polygon.AsWKT(ref pointsCount));
			return sb.ToString();
		}

		static Vector[][] AsVectorCoordinates(this Polygon polygon)
		{
			var coordinates = new List<List<Vector>>();
			coordinates.Add(new List<Vector>());
			coordinates[0].AddRange(polygon.OuterBoundary.LinearRing.Coordinates);
			coordinates.AddRange(polygon.InnerBoundary.Select(inner => inner.LinearRing.Coordinates.ToList()));
			return coordinates.Select(c => c.ToArray()).ToArray();
		}
	}
}
