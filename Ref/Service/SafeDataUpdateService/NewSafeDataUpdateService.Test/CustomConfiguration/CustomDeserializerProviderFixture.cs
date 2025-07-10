using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class CustomDeserializerProviderFixture
	{
		[Test]
		public void CheckGeometryPropertyNames()
		{
			var errorBuilder = new StringBuilder();
			var entitiesWithGeometry = typeof(RefCusTariff).Assembly.GetTypes()
				.Where(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => p.PropertyType == typeof(Geometry)));
			var geoProperties = entitiesWithGeometry.SelectMany(
				x => x.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(Geometry)));

			foreach (var geoProperty in geoProperties)
			{
				if (!GeometryIncludedTypeDeserializer.GeometryPropertyNames.Contains(geoProperty.Name))
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{geoProperty.Name} should be included in GeometryIncludedTypeDeserializer.GeometryPropertyNames.");
				}
			}
			foreach (var propertyName in GeometryIncludedTypeDeserializer.GeometryPropertyNames)
			{
				if (!geoProperties.Any(x => x.Name == propertyName))
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{propertyName} should not be included in GeometryIncludedTypeDeserializer.GeometryPropertyNames.");
				}
			}
			Assert.That(string.IsNullOrEmpty(errorBuilder.ToString()), errorBuilder.ToString());
		}
	}
}
