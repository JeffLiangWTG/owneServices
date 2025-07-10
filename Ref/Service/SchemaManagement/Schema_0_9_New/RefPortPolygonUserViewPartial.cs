using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Service.Schema_0_9_New;

public partial class RefPortPolygonUserView
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
			RPP_SerializedPolygon = TypeExtension.ConvertToGeometry(value.Geography.CoordinateSystemId, value.Geography.WellKnownText);
		}
	}
}
