using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	public abstract class DtbBookingTestCaseWithFactory : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(IsFcl);
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		protected TransportBookingTestData Data
		{
			get { return data ?? (data = new TransportBookingTestData(Factory)); }
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		protected TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}

		protected BindToLists BindToLists
		{
			get { return bindToLists ?? (bindToLists = new BindToLists(Factory)); }
		}

		protected virtual bool IsFcl
		{
			get { return false; }
		}

		TransportBookingTestData data;
		TransportBookingTestHelper helper;
		PackingTestHelper packingHelper;
		TestNotificationBuffer notify;
		BindToLists bindToLists;
	}
}
