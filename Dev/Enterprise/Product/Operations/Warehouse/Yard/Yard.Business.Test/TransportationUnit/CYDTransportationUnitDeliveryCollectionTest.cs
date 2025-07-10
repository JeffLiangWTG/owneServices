using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitDeliveryCollection))]
	public class CYDTransportationUnitDeliveryCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDTransportationUnitDeliveryCollection>
	{
		#region Implementation

		protected override CYDTransportationUnitDeliveryCollection GetCollectionToTest()
		{
			return new CYDTransportationUnitDeliveryCollection(Factory.New<CYDTransportationUnit>());
		}

		#endregion
	}
}
