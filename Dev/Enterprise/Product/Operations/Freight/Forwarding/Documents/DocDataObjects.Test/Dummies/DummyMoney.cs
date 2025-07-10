using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyMoney : IMoney
	{
		public ZDecimal Amount { get; set; }
		public ICodeDescription Currency { get; set; }
	}
}
