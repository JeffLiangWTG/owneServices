using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IPart
	{
		ZString PartNum { get; set; }
		ZString Description { get; set; }
		IOrgHeader Supplier { get; set; }
		IOrgHeader Buyer { get; set; }
		ZString StockKeepingUnit { get; set; }
	}
}
