using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingParentInvoicingSupporter))]
	class DtbBookingParentInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestShowChargeCostReferenceFilter()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			AssertEquals(true, ((IJobInvoicingSupporter)new DtbBookingParentInvoicingSupporter(booking)).ShowOperationalJobRefFilter);
		}

		public void TestShowChargeCostReferenceFilter_WhenParentIsHidden()
		{
			var booking = Helper.CreateBooking();
			DtbAgentBooking.NewFromBooking(booking);
			AssertEquals(false, ((IJobInvoicingSupporter)new DtbBookingParentInvoicingSupporter(booking)).ShowOperationalJobRefFilter);
		}

		public void TestDefaultChargeCostReference()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			IJobInvoicingSupporter supporter = new DtbBookingParentInvoicingSupporter(booking);
			AssertEquals("", supporter.OperationalJobRef);

			booking.KM_JobID = "JobID";
			AssertEquals("JobID", supporter.OperationalJobRef);

			booking.KM_TransportReference = "SupRef";
			AssertEquals("SupRef", supporter.OperationalJobRef);
		}

		protected override bool SetDefaultChargeCostReference(IJobInvoicingPlugIn parent, ZString costReference)
		{
			((DtbBooking)parent).KM_TransportReference = costReference;
			return true;
		}

		protected override JobHeader CreateJobHeader(IJobInvoicingPlugIn parent)
		{
			var booking = parent as DtbBooking;
			var bookingParent = (IJobInvoicingPlugIn)booking.ParentJob.ParentWithWorkflow;
			var job = new JobHeader.Loader(bookingParent).TryLoadOrCreate();
			var supporter = (DummyWithDtbBookingJobInvoicingSupporter)bookingParent.InvoicingSupporter;
			supporter.SetJob(job);

			return job;
		}

		public void TestServiceDirection()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBookingWithIServiceDirection);
			try
			{
				var booking = GetNewDtbBookingWithIServiceDirection();
				IServiceDirection parentInvoicingSupporter = (IServiceDirection)booking.InvoicingPlugIn.InvoicingSupporter;
				IServiceDirection invoicingSupporter = new DtbBookingParentInvoicingSupporter(booking);
				AssertEquals(parentInvoicingSupporter.ServiceDirection, invoicingSupporter.ServiceDirection);
			}
			finally
			{
				DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			}
		}

		public void TestServiceDirectionWhenParentInvoicingSupporterDoesNotImplement()
		{
			var booking = (DtbBooking)GetNewBusinessObject();
			var invoicingSupporter = new DtbBookingParentInvoicingSupporter(booking);
			AssertEquals(ZString.Empty, invoicingSupporter.ServiceDirection);
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			return Helper.CreateBooking(consolidation);
		}

		protected DtbBooking GetNewDtbBookingWithIServiceDirection()
		{
			var parent = Factory.New<DummyWithDtbBookingWithIServiceDirection>();
			var consolidation = Helper.CreateConsolidation(parent);
			return Helper.CreateBooking(consolidation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
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

		TransportBookingTestData data;
		TransportBookingTestHelper helper;
	}
}
