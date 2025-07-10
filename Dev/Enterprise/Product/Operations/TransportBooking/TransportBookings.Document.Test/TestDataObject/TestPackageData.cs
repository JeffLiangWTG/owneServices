using CargoWise.Types;

namespace Enterprise.TransportBookings.Document.Testing
{
	sealed class TestPackageData
	{
		public ZDecimal Weight { get; set; }
		public ZString WeightMetric { get; set; }
		public ZDecimal Volume { get; set; }
		public ZString VolumeMetric { get; set; }
		public ZInt Quantity { get; set; }
		public ZString Type { get; set; }
	}
}
