using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;
using NetTopologySuite.Geometries;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public partial class RefUNLOCO
	{
		[NotMapped]
		public SerializedGeometry RL_GeoLocation_WKT
		{
			get
			{
				return new SerializedGeometry { Geography = new GeographyWellKnownValue { CoordinateSystemId = RL_GeoLocation.SRID, WellKnownText = RL_GeoLocation.ToText() } };
			}
			set
			{
				RL_GeoLocation = (Geometry)TypeExtension.ChangeType(value.Geography.WellKnownText, typeof(Geometry));
			}
		}
	}
}
