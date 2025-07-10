using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderLineCollection))]
	public class MNRWorkOrderLineCollectionTest : ActiveBusinessObjectCollectionTestCase<MNRWorkOrderLineCollection>
	{
		#region Implementation

		protected override MNRWorkOrderLineCollection GetCollectionToTest()
		{
			return new MNRWorkOrderLineCollection(Factory);
		}

		#endregion
	}
}
