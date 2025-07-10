using CargoWise.Types;
using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public sealed class EntryNumber : IAWBEntryNumberMessageDetailsProvider
	{
		public ZString Type { get; set; }
		public ZString Number { get; set; }
	}
}
