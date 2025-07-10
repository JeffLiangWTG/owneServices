using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public partial class RefFacility
	{
		[NotMapped]
		public SerializedGeometry RFT_GeoLocation_WKT
		{
			get
			{
				return new SerializedGeometry { Geography = new GeographyWellKnownValue { CoordinateSystemId = RFT_GeoLocation.SRID, WellKnownText = RFT_GeoLocation.ToText() } };
			}
			set
			{
				RFT_GeoLocation = (Geometry)TypeExtension.ChangeType(value.Geography.WellKnownText, typeof(Geometry));
			}
		}
	}
}
