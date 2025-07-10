
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVBookingHeaderTestConsignmentsCreatorTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCreateConsignment_ShouldCreateConsignments()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			factory.Save();

			AssertEquals(0, bookingHeader.Consignments.Count);

			AssertNoExceptionThrown(() => HVLVBookingHeaderTestConsignmentsCreator.CreateConsignments(bookingHeader.HVH_BookingReference, 5, "Test"));

			var consignments = factory.Load<HVLVConsignment>(new ZQuery());

			CombineAssertions("Created consignments", () =>
			{
				AssertEquals("Should created 5 consignments", 5, consignments.Length);
				Assert("Waybill number should start with Test", consignments.All(c => c.HVC_WaybillNumber.StartsWith("Test")));
				Assert("All created consignments should link to booking header", consignments.All(c => c.HVC_HVH_BookingHeader == bookingHeader.PK));
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_ShouldDeleteExistingConsignments()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();
			factory.Save();

			var factory1 = new BusinessObjectFactory();
			var consignments1 = factory1.Load<HVLVConsignment>(new ZQuery());
			AssertEquals("1 consignments has been inserted", 1, consignments1.Length);

			var factory2 = new BusinessObjectFactory();
			HVLVBookingHeaderTestConsignmentsCreator.DeleteExistingConsignments(bookingHeader.HVH_BookingReference);
			var consignments = factory2.Load<HVLVConsignment>(new ZQuery());
			AssertEquals("All data in table HVLVConsignment has been deleted", 0, consignments.Length);
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_LastMileCarrierServiceLevel()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.Consignments.AddNew();

			factory.Save();

			var factory1 = new BusinessObjectFactory();
			var consignments1 = factory1.LoadTop1<HVLVConsignment>(new ZQuery());
			AssertEquals(string.Empty, consignments1.HVC_PL_NKLastMileCarrierServiceLevel);
		}
	}
}
