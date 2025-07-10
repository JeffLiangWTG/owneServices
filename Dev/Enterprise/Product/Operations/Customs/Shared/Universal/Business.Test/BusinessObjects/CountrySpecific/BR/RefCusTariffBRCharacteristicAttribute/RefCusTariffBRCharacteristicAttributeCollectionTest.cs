using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffBRCharacteristicAttributeCollection))]
	public class RefCusTariffBRCharacteristicAttributeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusTariffBRCharacteristicAttributeCollection>
	{
		protected override RefCusTariffBRCharacteristicAttributeCollection GetCollectionToTest()
		{
			return new RefCusTariffBRCharacteristicAttributeCollection(Factory.New<RefCusTariffBRCharacteristic>());
		}
	}
}
