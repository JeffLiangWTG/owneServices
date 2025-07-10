using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStateProcessTaskCollection))]
	public class CYDYardUnitStateProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDYardUnitStateProcessTaskCollection>
	{
		protected override CYDYardUnitStateProcessTaskCollection GetCollectionToTestCore()
		{
			var yardUnitState = Factory.NewWithValidTestData<CYDYardUnitState>();
			return new CYDYardUnitStateProcessTaskCollection(yardUnitState);
		}
	}
}
