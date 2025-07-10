using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyContainerType : IContainerType
	{
		public ZString ISOCode { get; set; }
		public ZString Code { get; set; }
		public ZString Description { get; set; }

		public object Codes => null;

		public ICodeDescription Type { get; set; }
	}
}
