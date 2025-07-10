using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingParentWrapper))]
	class DtbBookingParentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DtbBookingParentWrapper(null, DtbBookingDirection.None));
		}

		public void TestBookings()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var wrapper1 = new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC);
			AssertEquals(0, wrapper1.Bookings.Count);

			var consolidation = Helper.CreateConsolidation(dummy);
			var booking = Helper.CreateBooking(consolidation);
			AssertEquals("Wrapper was initialised with no Consolidation, Bookings remains empty.", 0, wrapper1.Bookings.Count);

			var wrapper2 = new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC);
			AssertContainsExactElementsInAnyOrder(new[] { booking }, wrapper2.Bookings);
		}

		public void TestConsolidation()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			AssertNull(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC).Consolidation);

			var consolidation = Helper.CreateConsolidation(dummy);
			AssertEquals(consolidation, new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC).Consolidation);
			AssertNull("Only existing Booking does not match direction.", new DtbBookingParentWrapper(dummy, DtbBookingDirection.DLV).Consolidation);
		}

		public void TestDirection()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			AssertEquals(DtbBookingDirection.PIC, new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC).Direction);
		}

		public void TestParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			AssertEquals(dummy, new DtbBookingParentWrapper(dummy, DtbBookingDirection.None).Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			return new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(false);
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();

			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
