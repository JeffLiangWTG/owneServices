using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData;
using Microsoft.OData.Edm;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class CustomSerializerProvider : ODataSerializerProvider
	{
		public static string[] GeometryIncludedTypeNames { get; } = new string[] { nameof(RefPortPolygon), nameof(RefPortPolygonUserView), nameof(RefUNLOCO), nameof(RefUNLOCOUserView), nameof(RefFacility) };

		public CustomSerializerProvider(IServiceProvider rootContainer) : base(rootContainer) { }

		public override IODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
		{
			if (ShouldUseCustomSerializer(edmType))
			{
				return new CustomSerializer(this);
			}
			return base.GetEdmTypeSerializer(edmType);
		}

		static bool ShouldUseCustomSerializer(IEdmTypeReference edmType)
		{
			var fullTypeName = edmType.Definition.FullTypeName();
			return !edmType.IsCollection() &&
				(GeometryIncludedTypeNames.Any(x => fullTypeName.Contains(x, StringComparison.OrdinalIgnoreCase)));
		}
	}

	class CustomSerializer : ODataResourceSerializer
	{
		public CustomSerializer(ODataSerializerProvider deserializerProvider) : base(deserializerProvider) { }

		public override void AppendDynamicProperties(ODataResource resource, SelectExpandNode selectExpandNode, ResourceContext resourceContext)
		{
			var instance = resourceContext.ResourceInstance;
			var type = instance.GetType();

			var geometryProperty = type.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(Geometry));
			if (geometryProperty != null && geometryProperty.GetValue(instance) != null)
			{
				var serializedGeometryProperty = type.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(SerializedGeometry));
				var serializedGeometryValue = (SerializedGeometry)serializedGeometryProperty?.GetValue(instance);
				resourceContext.DynamicComplexProperties ??= new Dictionary<string, object>();
				resourceContext.DynamicComplexProperties.Add(geometryProperty.Name, serializedGeometryValue);
			}

			base.AppendDynamicProperties(resource, selectExpandNode, resourceContext);
		}
	}
}
