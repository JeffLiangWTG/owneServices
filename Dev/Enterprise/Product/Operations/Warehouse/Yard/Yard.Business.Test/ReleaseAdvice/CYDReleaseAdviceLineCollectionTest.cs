using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Yard.Busines;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReleaseAdviceLineCollection))]
	public class CYDReleaseAdviceLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CYDReleaseAdviceLineCollection(Factory.New<CYDReleaseAdvice>());
		}
	}
}
