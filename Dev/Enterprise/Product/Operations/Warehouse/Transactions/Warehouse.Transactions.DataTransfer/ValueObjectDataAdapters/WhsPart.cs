using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.ValueObjectDataAdapters
{
	public class WhsPart : IPart
	{
		public ZString PartNum { get; set; }
		public ZString Description { get; set; }
		public IOrgHeader Supplier { get; set; }
		public IOrgHeader Buyer { get; set; }
		public ZString StockKeepingUnit { get; set; }
	}
}
