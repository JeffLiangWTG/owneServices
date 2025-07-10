using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PickingInfo))]
	class PickingInfoTest : DataObjectInfoTestCase<PickingInfo>
	{
		#region TestPickedQty

		public void TestPickedQty()
		{
			var info = new PickingInfo();
			AssertEquals(0m, info.PickedQty);

			info.PickedQty = 1m;
			AssertEquals(1m, info.PickedQty);
		}

		#endregion

		#region TestIsVerifiedNonEmpty

		public void TestIsVerifiedNonEmpty()
		{
			var info = new PickingInfo();
			AssertEquals(false, info.IsVerifiedNonEmpty);

			info.IsVerifiedNonEmpty = true;
			AssertEquals(true, info.IsVerifiedNonEmpty);
		}

		#endregion

		#region TestShouldSplit

		public void TestShouldSplit()
		{
			var info = new PickingInfo();
			AssertEquals(false, info.ShouldSplit);

			info.ShouldSplit = true;
			AssertEquals(true, info.ShouldSplit);
		}

		#endregion

		#region TestSuspendPickingLines

		public void TestSuspendPickingLines()
		{
			var info = new PickingInfo();
			AssertEquals(false, info.IsPickingSuspended);

			info.IsPickingSuspended = true;
			AssertEquals(true, info.IsPickingSuspended);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PickingInfo();
		}

		#endregion
	}
}
