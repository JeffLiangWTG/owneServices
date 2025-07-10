using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPickableDocketLine : IWhsDocketLine
	{
		ZDecimal QuantityNotMet { get; }
		ZShort WE_PickGroup { get; set; }
		ZString WE_WHC_NKOrderedHeldCode { get; }
	}
}
