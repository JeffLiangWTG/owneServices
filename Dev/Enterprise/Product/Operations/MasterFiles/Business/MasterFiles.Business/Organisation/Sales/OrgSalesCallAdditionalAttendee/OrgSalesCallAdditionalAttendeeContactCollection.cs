using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallAdditionalAttendeeContactCollection : OrgSalesCallAdditionalAttendeeCollection
	{
		public OrgSalesCallAdditionalAttendeeContactCollection(OrgSalesCall parent) : base(parent)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery(OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeTableCode, OrgContactSchema.Constants.Prefix);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			OrgSalesCallAdditionalAttendee attendee = (OrgSalesCallAdditionalAttendee)child;
			attendee.O6_AttendeeTableCode = OrgContactSchema.Constants.Prefix;
			attendee.O6_ReceiverReminder = false;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			var attendee = (OrgSalesCallAdditionalAttendee)bizOAdded;

			attendee.O6_ReceiverReminderInfo.ValueChanged += ReceiveReminder_ValueChanged;
			if (ReceiveReminderHasValueChanged != null)
			{
				ReceiveReminderHasValueChanged(this, new AdditionalAttendeeEventArgs(attendee));
			}

			base.OnAdded(bizOAdded);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			var attendee = (OrgSalesCallAdditionalAttendee)bizO;

			attendee.O6_ReceiverReminderInfo.ValueChanged -= ReceiveReminder_ValueChanged;

			base.OnRemoved(bizO);
		}

		void ReceiveReminder_ValueChanged(object sender, EventArgs e)
		{
			if (ReceiveReminderHasValueChanged != null)
			{
				ReceiveReminderHasValueChanged(this, new AdditionalAttendeeEventArgs((OrgSalesCallAdditionalAttendee)sender));
			}
		}
		public event EventHandler<AdditionalAttendeeEventArgs> ReceiveReminderHasValueChanged;
	}

	public class AdditionalAttendeeEventArgs : EventArgs
	{
		public AdditionalAttendeeEventArgs(OrgSalesCallAdditionalAttendee additionalAttendee)
		{
			Argument.NotNull(additionalAttendee, "additionalAttendee");
			ShouldReceiveReminder = additionalAttendee.O6_ReceiverReminder;
		}

		public readonly bool ShouldReceiveReminder;
	}
}
