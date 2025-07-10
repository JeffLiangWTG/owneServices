using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(CarrierContractQuantityUnitPair))]
	public class CarrierContractQuantityUnitPairTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CarrierContractQuantityUnitPair(10, 100);
		}

		public void TestPropertiesAreSet()
		{
			var unitPair = GetNewBusinessObject() as CarrierContractQuantityUnitPair;
			AssertEquals("TEU value is set", 10m, unitPair.TEUValue);
			AssertEquals("Container value is set", 100m, unitPair.ContainerValue);
		}
	}
}
