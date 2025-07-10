using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceiveLineCollection))]
	[SetGlobalsIsWeb]
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsReceiveLineCollection>
	{
		protected override TrackingWhsReceiveLineCollection GetCollectionToTest()
		{
			var trackingReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
			return new TrackingWhsReceiveLineCollection(trackingReceive);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TrackingHelper.Get(Factory.New<WhsReceiveLine>());
		}

		#region Implementation

		#region Test Setup

		public void TestLocationSortedProperlyCore(string locationPropertyToSort)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var trackingReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
			var trackingReceiveLine1 = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			trackingReceiveLine1.WhsReceiveLine.WE_WD = trackingReceive.PK;
			trackingReceiveLine1.WhsReceiveLine.WE_WL = data.Whs1.FindLocation("A-1").PK;
			var trackingReceiveLine2 = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			trackingReceiveLine2.WhsReceiveLine.WE_WD = trackingReceive.PK;
			trackingReceiveLine2.WhsReceiveLine.WE_WL = data.Whs1.FindLocation("A-2").PK;
			var trackingReceiveLine3 = TrackingHelper.Get(Factory.New<WhsReceiveLine>());
			trackingReceiveLine3.WhsReceiveLine.WE_WD = trackingReceive.PK;
			trackingReceiveLine3.WhsReceiveLine.WE_WL = data.Whs1.FindLocation("A-10").PK;

			// Check sort ascending
			trackingReceive.WhsReceive.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Ascending);
			AssertEquals("A-1", trackingReceive.WhsReceive.Lines[0].LocationString);
			AssertEquals("A-2", trackingReceive.WhsReceive.Lines[1].LocationString);
			AssertEquals("A-10", trackingReceive.WhsReceive.Lines[2].LocationString);

			// Check sort descending
			trackingReceive.WhsReceive.Lines.ApplySort(locationPropertyToSort, ListSortDirection.Descending);
			AssertEquals("A-10", trackingReceive.WhsReceive.Lines[0].LocationString);
			AssertEquals("A-2", trackingReceive.WhsReceive.Lines[1].LocationString);
			AssertEquals("A-1", trackingReceive.WhsReceive.Lines[2].LocationString);
		}

		#endregion
		#endregion
	}
}
