using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBooking))]
	sealed class TrackingBookingWorkflowProviderTest : WorkflowProviderTest<TrackingBooking, QuotedBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return ((IWorkflowProvider)BusinessObject.QuotedBooking).WorkflowType; }
		}

		protected override BusinessObject GetParent(TrackingBooking bizo) => bizo.QuotedBooking;

		protected override Type ParentProxyType => typeof(QuotedBooking);

		protected override TrackingBooking GetNewBusinessObject(BusinessObjectFactory factory)
		{
			ForwardingShipment booking = QuotedBooking.CreateNewBooking(factory);
			JobHeader job = new JobHeader.Loader(booking).TryCreate();
			job.JH_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			TrackingBooking trackingBooking = new TrackingBooking(booking.PK, factory, null);

			return trackingBooking;
		}

		protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, TrackingBooking workFlowProvider)
		{
			var booking = factory.Load<ForwardingShipment>(workFlowProvider.QuotedBooking.Booking.PK);
			return new TrackingBooking(booking.PK, factory, null);
		}

		protected override string RealTableNameForNonPersistentIWorkflowProvider => JobShipmentSchema.Constants.TableName;
	}
}
