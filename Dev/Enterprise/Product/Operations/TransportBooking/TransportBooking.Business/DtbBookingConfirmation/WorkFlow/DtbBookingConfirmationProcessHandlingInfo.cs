namespace Enterprise.TransportBookings.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.Integration;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Business.EventManagement;
	using Enterprise.ZArchitecture.Business.EventManagement;
	using Enterprise.ZArchitecture.Schema;

	public class DtbBookingConfirmationProcessHandlingInfo : ProcessHandlingInfo
	{
		public DtbBookingConfirmationProcessHandlingInfo(DtbBookingConfirmation bookingConfirmation) : base(bookingConfirmation)
		{
			this.bookingConfirmation = Argument.NotNull(bookingConfirmation, "bookingConfirmation");
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			if (bookingConfirmation.Booking == null)
			{
				return Array.Empty<ProcessTask>();
			}

			var filter = new ZQuery();
			filter.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, TriggerLineTypes.Codes.DtbBookingConfirmation);
			filter.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, logBeingAdded.SL_SE_NKEvent);

			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var bookings = new DtbBookingProcessTaskCollection(bookingConfirmation.Booking, filter);
				bookings.Load();

				return bookings.Cast<ProcessTask>();
			}
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Array.Empty<CascadingLink>();
		}

		readonly DtbBookingConfirmation bookingConfirmation;
	}
}
