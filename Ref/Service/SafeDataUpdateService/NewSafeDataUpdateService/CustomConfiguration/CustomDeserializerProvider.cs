using System;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class CustomDeserializerProvider : ODataDeserializerProvider
	{
		public CustomDeserializerProvider(IServiceProvider rootContainer) : base(rootContainer)
		{
		}

		public override IODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType, bool isDelta = false)
		{
			var fullTypeName = edmType.Definition.FullTypeName();
			if (!edmType.IsCollection() && CustomSerializerProvider.GeometryIncludedTypeNames.Any(x => fullTypeName.Contains(x, StringComparison.OrdinalIgnoreCase)))
			{
				return new GeometryIncludedTypeDeserializer(this);
			}
			return base.GetEdmTypeDeserializer(edmType, isDelta);
		}
	}

	public class GeometryIncludedTypeDeserializer : ODataResourceDeserializer
	{
		public static string[] GeometryPropertyNames { get; } = new string[] { nameof(RefPortPolygon.RPP_SerializedPolygon), nameof(RefUNLOCO.RL_GeoLocation), nameof(RefFacility.RFT_GeoLocation) };

		public GeometryIncludedTypeDeserializer(ODataDeserializerProvider deserializerProvider) : base(deserializerProvider)
		{
		}

		public override object ReadResource(ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
		{
			ODataUntypedValue resourceGeoValue = null;
			var properties = resourceWrapper.Resource.Properties;
			var upperGeometryPropertyNames = GeometryPropertyNames.Select(x => x.ToUpperInvariant());

			var resourceGeoProperty = properties.FirstOrDefault(x => upperGeometryPropertyNames.Contains(x.Name.ToUpperInvariant()));
			if (resourceGeoProperty != null)
			{
				resourceGeoValue = resourceGeoProperty.Value as ODataUntypedValue;
				resourceWrapper.Resource.Properties = properties.Where(x => !upperGeometryPropertyNames.Contains(x.Name.ToUpperInvariant()));
			}
			var result = base.ReadResource(resourceWrapper, structuredType, readContext);
			return result;
		}
	}
}
