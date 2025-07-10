using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyUnloco : DocDataObject, IUnloco
	{
		public ZString Code { get; set; }

		public ZString Name { get; set; }

		public ICountry Country { get; set; }

		public ZString IATACode { get; set; }

		public IRefUNLOCOCollection Unlocos { get; set; }
	}
}
