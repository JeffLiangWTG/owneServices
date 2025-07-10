using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Freight.OnlineSailingSchedules.ServiceModel
{
	public class Route
	{
		public Carrier Carrier { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public Leg[] Legs { get; set; }

		public int TransitTime { get; set; }
	}
}
