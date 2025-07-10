using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReleaseLine))]
	sealed class TrackingWhsReleaseLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorIsPopulatingProperties()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var releaseLine = orderLine.ReleaseLines.AddNew("P1", "P2", "P3", "SN3", today, today.AddDays(1));
			ReleaseLinesOnPickManager.GetUnreleasedQuantityAndBuildCacheIfNeeded(orderLine, WhsReleaseLineCollection.GetKeyForLineWithoutReleaseCapturedAttribs(releaseLine, orderLine));
			releaseLine.Quantity = 1m;

			var trackingReleaseLine = new TrackingWhsReleaseLine(releaseLine, null, 2);
			AssertEquals("Should be P1", "P1", trackingReleaseLine.W1_PartAttrib1);
			AssertEquals("Should be P2", "P2", trackingReleaseLine.W1_PartAttrib2);
			AssertEquals("Should be P3", "P3", trackingReleaseLine.W1_PartAttrib3);
			AssertEquals("Should be SN3", "SN3", trackingReleaseLine.W1_SerialNumber);
			AssertEquals("Should be 1", releaseLine.Quantity.ToString(2), trackingReleaseLine.W1_Units);
		}

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingWhsReleaseLine(new WhsReleaseLine(Factory.New<WhsOrderLine>()), null, 2);
		}

		#endregion
	}
}
