using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyCountry : ICountry
	{
		public ZString Code { get; set; }

		public ZString Name { get; set; }

		public IRefCountryCollection Countries { get; set; }
	}
}
