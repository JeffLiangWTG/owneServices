using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test
{
	[TestFixture]
	class EntityClassFixture
	{
		[Test]
		public void EntityWithGeometryPropertyTest()
		{
			var errorBuilder = new StringBuilder();
			var entitiesWithGeometry = typeof(Schema_New.RefCusTariff).Assembly.GetTypes()
				.Where(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => p.PropertyType == typeof(Geometry)));
			foreach (var entity in entitiesWithGeometry)
			{
				var properties = entity.GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var geoProperties = properties.Where(x => x.PropertyType == typeof(Geometry));
				foreach (var geoProperty in geoProperties)
				{
					if (!properties.Any(p => p.Name == geoProperty.Name + "_WKT" && p.PropertyType == typeof(SerializedGeometry)))
					{
						errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"{entity.Name} should have {geoProperty.Name}_WKT property for {geoProperty.Name} property.");
					}
				}
			}
			Assert.That(string.IsNullOrEmpty(errorBuilder.ToString()), errorBuilder.ToString());
		}
	}
}
