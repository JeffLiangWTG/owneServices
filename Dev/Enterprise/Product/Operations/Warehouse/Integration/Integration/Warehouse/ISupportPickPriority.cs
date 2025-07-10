using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.Warehouse
{
	public interface ISupportPickPriority
	{
		ZInt PickPriority { get; }
	}
}
