using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractAllocationLineCollection : IBusinessObjectCollection
	{
		new IRatingContractAllocationLine this[int i] { get; }
	}
}
