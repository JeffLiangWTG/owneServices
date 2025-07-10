using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteBookingCollection))]
	public class GteBookingCollectionTest : ActiveBusinessObjectCollectionTestCase<GteBookingCollection>
	{
		public override void TestDelete()
		{
			base.TestDelete();
		}
		#region Implementation

		protected override GteBookingCollection GetCollectionToTest()
		{
			return new GteBookingCollection(Factory);
		}

		#endregion
	}
}
