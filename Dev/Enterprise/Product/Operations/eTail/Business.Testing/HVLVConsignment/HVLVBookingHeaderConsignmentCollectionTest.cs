using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVBookingHeaderConsignmentCollection))]
	public class HVLVBookingHeaderConsignmentCollectionTest : HVLVConsignmentCollectionWithPrefetchTest
	{
		public void TestConsignmentCollectionLoadsInactiveConsignmentsByDefault()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignment.HVC_IsActive = false;

			var consignmentCollection = new HVLVBookingHeaderConsignmentCollection(bookingHeader);
			consignmentCollection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, consignmentCollection.Count);
				Assert("Consignment is inactive", !consignment.HVC_IsActive);
			});
		}

		public void TestConsignmentCollectionUsesClusterKey()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_ClusterKey = 666;
			var consignmentWithSameClusterKey = Factory.New<HVLVConsignment>();
			consignmentWithSameClusterKey.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignmentWithSameClusterKey.HVC_ClusterKey = 666;

			var consignmentWithDifferentClusterKey = Factory.New<HVLVConsignment>();
			consignmentWithDifferentClusterKey.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignmentWithDifferentClusterKey.HVC_ClusterKey = 888;

			var consignmentCollection = bookingHeader.Consignments;
			consignmentCollection.Load();

			AssertEquals(1, consignmentCollection.Count);
			AssertCollectionContains("Consignments should contain ones with same ClusterKey", consignmentWithSameClusterKey, bookingHeader.Consignments);
			AssertCollectionNotContains("Consignments shouldn't contain ones with different ClusterKeys", consignmentWithDifferentClusterKey, bookingHeader.Consignments);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVBookingHeaderConsignmentCollection(Factory.NewWithValidTestData<HVLVBookingHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVConsignment>();
		}
	}
}
