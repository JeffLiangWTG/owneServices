using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface ICarrierContractQuantityUnitPair : IBusiness
	{
		public ZDecimal TEUValue { get; }
		public ZDecimal ContainerValue { get; }
	}
}
