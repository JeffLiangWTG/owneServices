using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[Keyless]
	public class SerializedGeometry
	{
		public GeographyWellKnownValue Geography { get; set; }
	}

	public class GeographyWellKnownValue
	{
		public int CoordinateSystemId { get; set; } = 4326;
		public string WellKnownText { get; set; }
	}
}
