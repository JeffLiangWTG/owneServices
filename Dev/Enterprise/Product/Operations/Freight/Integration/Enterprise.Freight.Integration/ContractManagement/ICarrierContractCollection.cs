using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface ICarrierContractCollection : IBusinessObjectCollection
	{
		new IRatingContract this[int i] { get; }
	}
}
