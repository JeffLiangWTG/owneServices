using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitPickupCollection))]
	public class CYDTransportationUnitPickupCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDTransportationUnitPickupCollection>
	{
		#region Implementation

		protected override CYDTransportationUnitPickupCollection GetCollectionToTest()
		{
			return new CYDTransportationUnitPickupCollection(Factory.New<CYDTransportationUnit>());
		}

		#endregion
	}
}
