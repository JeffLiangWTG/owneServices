using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDDeliveryHeaderCollection))]
	class CYDDeliveryHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CYDDeliveryHeaderCollection>
	{
		#region Implementation

		protected override CYDDeliveryHeaderCollection GetCollectionToTest()
		{
			return new CYDDeliveryHeaderCollection(Factory);
		}

		#endregion
	}
}
