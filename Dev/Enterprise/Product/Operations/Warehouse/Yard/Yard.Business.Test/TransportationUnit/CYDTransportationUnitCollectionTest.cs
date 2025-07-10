using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitCollection))]
	public class CYDTransportationUnitCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDTransportationUnitCollection>
	{
		#region Implementation

		protected override CYDTransportationUnitCollection GetCollectionToTest()
		{
			return new CYDTransportationUnitCollection(Factory);
		}

		#endregion
	}
}
