using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyVessel : IVessel
	{
		public ZString Name { get; set; }

		public ZString LloydsIMO { get; set; }

		public ZString RadioCallSign { get; set; }

		public ICodeDescription Type { get; set; }

		public ICountry CountryOfRegistration { get; set; }
	}
}
