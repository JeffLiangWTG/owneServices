using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDPickupHeaderCollection))]
	public class CYDPickupHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDPickupHeaderCollection>
	{
		#region Implementation

		protected override CYDPickupHeaderCollection GetCollectionToTest()
		{
			return new CYDPickupHeaderCollection(Factory);
		}

		#endregion
	}
}
