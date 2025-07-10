using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementCollection))]
	public class GteGateMovementCollectionTest : ActiveBusinessObjectCollectionTestCase<GteGateMovementCollection>
	{
		#region Implementation

		protected override GteGateMovementCollection GetCollectionToTest()
		{
			return new GteGateMovementCollection(Factory);
		}

		#endregion
	}
}
