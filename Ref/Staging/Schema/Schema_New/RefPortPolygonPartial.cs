using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public partial class RefPortPolygon
	{
		[NotMapped]
		public SerializedGeometry RPP_SerializedPolygon_WKT
		{
			get
			{
				return new SerializedGeometry { Geography = new GeographyWellKnownValue { CoordinateSystemId = RPP_SerializedPolygon.SRID, WellKnownText = RPP_SerializedPolygon.ToText() } };
			}
			set
			{
				RPP_SerializedPolygon = (Geometry)TypeExtension.ChangeType(value.Geography.WellKnownText, typeof(Geometry));
			}
		}
	}
}
