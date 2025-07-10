using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IAllocationDistributionCollection : IBusinessObjectCollection
	{
		new IRatingContractAllocationLine this[int i] { get; }
	}
}
