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
	class CustomSerializerProviderFixture
	{
		[Test]
		public void CheckGeometryIncludedTypeNames()
		{
			var errorBuilder = new StringBuilder();
			var entitiesWithGeometry = typeof(RefCusTariff).Assembly.GetTypes()
				.Where(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => p.PropertyType == typeof(Geometry)));

			foreach (var entity in entitiesWithGeometry)
			{
				if (!CustomSerializerProvider.GeometryIncludedTypeNames.Contains(entity.Name))
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{entity.Name} should be included in CustomSerializerProvider.GeometryIncludedTypeNames.");
				}
			}
			foreach (var typeName in CustomSerializerProvider.GeometryIncludedTypeNames)
			{
				if (!entitiesWithGeometry.Any(x => x.Name == typeName))
				{
					errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{typeName} should not be included in CustomSerializerProvider.GeometryIncludedTypeNames.");
				}
			}
			Assert.That(string.IsNullOrEmpty(errorBuilder.ToString()), errorBuilder.ToString());
		}
	}
}
