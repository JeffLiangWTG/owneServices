using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class Part : IPart
	{
		public ZString PartNum { get; set; }
		public ZString Description { get; set; }
		public IOrgHeader Supplier { get; set; }
		public IOrgHeader Buyer { get; set; }
		public ZString StockKeepingUnit { get; set; }
	}
}
