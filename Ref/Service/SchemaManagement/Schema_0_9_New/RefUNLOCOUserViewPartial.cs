using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New;

public partial class RefUNLOCOUserView
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
			RL_GeoLocation = TypeExtension.ConvertToGeometry(value.Geography.CoordinateSystemId, value.Geography.WellKnownText);
		}
	}
}
