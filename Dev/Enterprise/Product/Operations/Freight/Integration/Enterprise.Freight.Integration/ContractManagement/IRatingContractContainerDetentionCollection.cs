using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IRatingContractContainerDetentionCollection : IBusinessObjectCollection
	{
		new IRatingContractContainerDetention this[int i] { get; }
	}
}
