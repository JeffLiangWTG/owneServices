using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New;

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
			RFT_GeoLocation = TypeExtension.ConvertToGeometry(value.Geography.CoordinateSystemId, value.Geography.WellKnownText);
		}
	}
}
