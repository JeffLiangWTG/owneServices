using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStorageLinesCollection))]
	public class CYDYardUnitStorageLinesCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDYardUnitStorageLinesCollection>
	{
		#region Implementation

		protected override CYDYardUnitStorageLinesCollection GetCollectionToTest()
		{
			return new CYDYardUnitStorageLinesCollection(Factory.New<CYDYardUnitState>());
		}

		#endregion
	}
}
