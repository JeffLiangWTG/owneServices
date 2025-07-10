using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCall
	{
		public string TotalItems { get; set; }
		public int TotalPages { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public PortCallItem[] Items { get; set; }
	}
}
