using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReleaseLineCollection))]
	sealed class TrackingWhsReleaseLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsReleaseLineCollection>
	{
		#region TestCollection

		public void TestCollection()
		{
			var trackingWhsReleaseLine = new TrackingWhsReleaseLine();
			var trackingWhsReleaseLineCollection = new TrackingWhsReleaseLineCollection(trackingWhsReleaseLine.Factory);
			AssertEquals("No TrackingWhsReleaseLines", 0, trackingWhsReleaseLineCollection.Count);

			var inv1 = trackingWhsReleaseLineCollection.AddNew();
			var inv2 = trackingWhsReleaseLineCollection.AddNew();
			AssertEquals("2 TrackingWhsReleaseLines", 2, trackingWhsReleaseLineCollection.Count);
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var collection = GetCollectionToTest();
			AssertEquals("Empty collection", 0, collection.Count);
			AssertEquals("Cannot add lines", false, collection.AllowNew);
			AssertEquals("Cannot delete lines", false, collection.AllowRemove);
		}

		#endregion

		#region Overrides

		protected override TrackingWhsReleaseLineCollection GetCollectionToTest()
		{
			var trackingWhsReleaseLine = new TrackingWhsReleaseLine();
			return new TrackingWhsReleaseLineCollection(trackingWhsReleaseLine.Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TrackingWhsReleaseLine();
		}

		#endregion
	}
}
