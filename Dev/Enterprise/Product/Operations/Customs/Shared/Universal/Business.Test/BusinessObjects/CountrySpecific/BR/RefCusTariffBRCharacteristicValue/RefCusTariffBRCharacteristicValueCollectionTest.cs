using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffBRCharacteristicValueCollection))]
	public class RefCusTariffBRCharacteristicValueCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusTariffBRCharacteristicValueCollection>
	{
		protected override RefCusTariffBRCharacteristicValueCollection GetCollectionToTest()
		{
			return new RefCusTariffBRCharacteristicValueCollection(Factory.New<RefCusTariffBRCharacteristic>());
		}
	}
}

