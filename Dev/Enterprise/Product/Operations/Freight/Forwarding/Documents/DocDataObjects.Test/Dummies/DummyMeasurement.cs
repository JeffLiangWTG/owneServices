using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyMeasurement : IMeasurement
	{
		public ZDecimal Value { get; set; }

		public ICodeDescription Unit { get; set; }
	}
}
