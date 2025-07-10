using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusClassPartPivotCollection))]
	sealed class BaseCusClassPartPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseCusClassPartPivotCollection(Factory);
		}
	}
}
