using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	public interface IChildWhsOrderCollection : IActiveBusinessObjectCollection
	{
		new IWhsDocket this[int index] { get; }
	}
}
