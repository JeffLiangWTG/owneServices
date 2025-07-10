using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyCodeDescription : ICodeDescription
	{
		public ZString Code { get; set; }
		public ZString Description { get; set; }
		public object Codes { get; set; }
	}
}
