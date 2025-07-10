using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDYardUnitStateCollection))]
	public class CYDYardUnitStateCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDYardUnitStateCollection>
	{
		#region Implementation

		protected override CYDYardUnitStateCollection GetCollectionToTest()
		{
			return new CYDYardUnitStateCollection(Factory);
		}

		#endregion
	}
}
