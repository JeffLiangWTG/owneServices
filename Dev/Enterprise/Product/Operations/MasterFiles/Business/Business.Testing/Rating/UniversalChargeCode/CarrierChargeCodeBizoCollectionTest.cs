using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(CarrierChargeCodeBizoCollection))]
	public class CarrierChargeCodeBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CarrierChargeCodeBizoCollection>
	{
		protected override CarrierChargeCodeBizoCollection GetCollectionToTest()
		{
			return new CarrierChargeCodeBizoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CarrierChargeCodeBizo(Factory);
		}
	}
}
