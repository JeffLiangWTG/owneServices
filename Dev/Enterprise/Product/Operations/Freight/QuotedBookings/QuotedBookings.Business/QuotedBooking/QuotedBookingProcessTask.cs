using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public sealed class QuotedBookingProcessTask : ProcessTask, IQuotedBookingProcessTask
	{
		public QuotedBookingProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(QuotedBooking); }
		}

		public new QuotedBooking Parent
		{
			get { return (QuotedBooking)base.Parent; }
		}

		protected override bool ShouldDefaultEstimate => !Parent?.IsConsolidated ?? base.ShouldDefaultEstimate;

		public override ControllerID ParentControllerID => Parent?.ObjectState == QuotedBookingState.QuoteOnly
			? ControllerIDs.OneOffQuotes
			: ControllerIDs.QuotedBookings;

		public override void AddParentFetchHint()
		{
			if (P9_ParentID.IsValid)
			{
				Factory.AddFetchHint(typeof(ForwardingShipment), P9_ParentID);
				Factory.AddFetchHint(typeof(Quote), P9_ParentID);
			}
		}
	}
}
