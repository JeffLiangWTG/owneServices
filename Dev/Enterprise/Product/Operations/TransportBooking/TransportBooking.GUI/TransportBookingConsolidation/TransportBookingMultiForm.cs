using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI
{
	public sealed partial class TransportBookingMultiForm : ZTemplateForm, INotifications, INotificationSubscriberQueryUser
	{
		public TransportBookingMultiForm(DtbBookingConsolidation consolidation)
			: base(consolidation)
		{
			SetLastJobNumber();

			InitializeComponent();
			AddPlugIns();
			WorkflowTabPage.Initialize(consolidation);
			HookEvents();
			transportBookingsControl.DtbBookingInstructionsViewsPanel.IsAdditionalReferencesVisible = true;

			foreach (var booking in consolidation.Bookings)
			{
				booking.UpdateReadOnly_MultiForm();
			}
		}

		protected override void SaveToRecentItems()
		{
			if (Consolidation.IsMultiBooking)
			{
				base.SaveToRecentItems();
			}
		}

		void AddPlugIns()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			if (!Consolidation.IsMultiBooking)
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 1);
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.PackingPlugIn, 1);
			}
			else
			{
				PlugIns.Add(ControllerIDs.Apportionment);
			}
		}

		void HookEvents()
		{
			Consolidation.KB_StatusInfo.ValueChanged += new EventHandler(KB_StatusInfo_ValueChanged);
			Consolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(TransportBooking_OnSelectDtbBookingsToPrint);
			Consolidation.Bookings.CountChanged += new EventHandler(Bookings_CountChanged);
			Consolidation.OnConsolidationIsOverrideChanged();
			Consolidation.NotificationManager.Push(this);

			HookBookings();
		}

		void Bookings_CountChanged(object sender, EventArgs e)
		{
			HookBookings();
			CheckJobNumberChanged();
		}

		void CheckJobNumberChanged()
		{
			if (!Consolidation.IsMultiBooking)
			{
				var newJobNumber = Consolidation.JobNumber;
				if (LastJobNumber != newJobNumber)
				{
					LastJobNumber = newJobNumber;
					var packageJob = PkgPackageJob.LoadPackageJob(Consolidation); // only set if previously created
					if (packageJob != null)
					{
						packageJob.OnParentJobNumberChanged();
					}
				}
			}
		}

		void SetLastJobNumber()
		{
			if (!Consolidation.IsMultiBooking)
			{
				LastJobNumber = Consolidation.JobNumber;
			}
		}

		ZString LastJobNumber;

		void UnhookEvents()
		{
			if (Consolidation != null)
			{
				Consolidation.KB_StatusInfo.ValueChanged -= new EventHandler(KB_StatusInfo_ValueChanged);
				Consolidation.OnSelectDtbBookingsToPrint -= new EventHandler<DtbBookingsToPrintEventArgs>(TransportBooking_OnSelectDtbBookingsToPrint);
				Consolidation.Bookings.CountChanged -= new EventHandler(Bookings_CountChanged);
				Consolidation.NotificationManager.Pop();
				UnHookAllBookings();
			}
		}

		void HookBookings()
		{
			var consolidatedBookings = Consolidation != null ? Consolidation.Bookings.ToArray() : Array.Empty<DtbBooking>();

			foreach (var bookingToUnhook in HookedBookings.Except(consolidatedBookings).ToArray())
			{
				bookingToUnhook.CancelBookingDelete -= new EventHandler<CancelEventArgs>(bookingToHook_CancelBookingDelete);
				HookedBookings.Remove(bookingToUnhook);
			}

			foreach (var bookingToHook in consolidatedBookings.Except(HookedBookings).ToArray())
			{
				bookingToHook.CancelBookingDelete += new EventHandler<CancelEventArgs>(bookingToHook_CancelBookingDelete);
				HookedBookings.Add(bookingToHook);
			}
		}

		void UnHookAllBookings()
		{
			foreach (var bookingToUnhook in HookedBookings.ToArray())
			{
				bookingToUnhook.CancelBookingDelete -= new EventHandler<CancelEventArgs>(bookingToHook_CancelBookingDelete);
				HookedBookings.Remove(bookingToUnhook);
			}
		}

		void bookingToHook_CancelBookingDelete(object sender, CancelEventArgs e)
		{
			var booking = (DtbBooking)sender;
			var result = Globals.Message.Show(Res.GetString("98c6e385-3980-4080-83f8-b4534b7c459a", "This Booking '{0}' has Packages assigned and/or Dates and References. Are you sure you want to delete it?", booking.KM_JobID), Res.GetString("d2bfe341-58b5-4ae0-92aa-e101115bbfa7", "Delete Booking?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			e.Cancel = result == DialogResult.No;
		}

		List<DtbBooking> HookedBookings
		{
			get { return hookedBookings ?? (hookedBookings = new List<DtbBooking>()); }
		}

		List<DtbBooking> hookedBookings;

		public override string FormCaption
		{
			get { return (Consolidation != null) ? (string)Consolidation.Description : ""; }
		}

		void KB_StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshCaption();
		}

		public override bool IsResizableByTabPageAllowed => true;

		DtbBookingConsolidation Consolidation
		{
			get { return (DtbBookingConsolidation)DataSource; }
		}

		void TransportBooking_OnSelectDtbBookingsToPrint(object sender, DtbBookingsToPrintEventArgs e)
		{
			using (var form = new TransportBookingSelectBookingsToPrintForm(e.BookingsToSelectFrom))
			{
				form.ShowDialog();
				e.ContinueToPrint = (form.DialogResult == DialogResult.OK);
			}
		}

		public void SelectBooking(DtbBooking transportBooking)
		{
			if (transportBooking != null && transportBookingsControl != null)
			{
				transportBookingsControl.SelectBooking(transportBooking);
			}
		}

		public void SetView(TransportBookingInstructionView view)
		{
			if (transportBookingsControl != null) // WI00054972 - Issue 00864570 - Object reference not set to an instance of an object.
			{
				transportBookingsControl.SetInstructionView(view);
			}
		}

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			var args = (QueryUserMsgBoxEventArgs)e;
			args.Response = Globals.Message.Show(args.Message, args.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookEvents();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
