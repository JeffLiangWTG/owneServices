using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.ContractManagement.Business
{
	public class CarrierContractQuantityUnitPair : NonPersistentBusinessObject, ICarrierContractQuantityUnitPair
	{
		public CarrierContractQuantityUnitPair(
			ZDecimal teuValue,
			ZDecimal containerValue)
		{
			TEUValue = teuValue;
			ContainerValue = containerValue;
		}

		[DecimalPlaces(2)]
		public ZDecimal TEUValue { get; }

		[DecimalPlaces(0)]
		public ZDecimal ContainerValue { get; }
	}
}
