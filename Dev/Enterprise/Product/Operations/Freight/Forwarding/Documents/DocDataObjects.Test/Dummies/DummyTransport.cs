using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyTransport : ITransport
	{
		public ZInt LegOrder { get; set; }

		public ZString VoyageFlightNumber { get; set; }

		public ZDateTime ETD { get; set; }

		public ZDateTime ETA { get; set; }

		public ZDateTime ATD { get; set; }

		public ZDateTime ATA { get; set; }

		public ZDateTime LCLCutOff { get; set; }

		public ZDateTime LCLReceivalCommences { get; set; }

		public ICodeDescription Mode { get; set; }

		public ICodeDescription AdditionalTransportMode { get; set; }

		public ICodeDescription Type { get; set; }

		public ICodeDescription Status { get; set; }

		public IVessel Vessel { get; set; }

		public IUnloco PortOfLoading { get; set; }

		public IUnloco PortOfDischarge { get; set; }

		public IAddress Carrier { get; set; }
	}
}
