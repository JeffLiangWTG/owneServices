using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusRateUOMCollection))]
	internal class RefCusRateUOMCollection_RefCusRate_Test : ActiveBusinessObjectCollectionTestCase<RefCusRateUOMCollection>
	{
		protected override RefCusRateUOMCollection GetCollectionToTest()
		{
			return Factory.New<RefCusRate>().UnitsOfMeasure;
		}
	}
}
