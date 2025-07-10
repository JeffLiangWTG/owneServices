using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class BookingDetails : BookingBasePage
	{
		protected override string GetPageName()
		{
			return WebTracker.Pages.LinerAndAgencyBookingDetails;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Booking != null)
			{
				if (Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.WebFwdInstruction)
				{
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", AppInstance.LinerAndAgencyBillOfLadingDetailsPage, Booking.PK)); // partial URL
				}

				if (AllowEditOrCancel)
				{
					EditBooking.OnClientClick = string.Format((NoResString)"javascript: window.location = '{0}?Ref={1}'; return false;", AppInstance.LinerAndAgencyEditBookingPage, GetGuidFromParameter("Ref")); // partial URL
					ConvertBooking.OnClientClick = string.Format((NoResString)@"javascript: if (confirm('Are you sure you want to convert this Booking to a Forwarding Instruction?\n\nThe Forwarding Instruction will be accessible via the Bills of Lading menu item.')) window.location = '{0}?Ref={1}'; return false;", AppInstance.LinerAndAgencyEditForwardingInstructionPage, GetGuidFromParameter("Ref")); // javascript code
				}

				EditButtonDiv.Visible = AllowEditOrCancel;
				CancelledBookingDiv.Visible = IsCancelled;
				ConvertBooking.Visible = CanConvertToInstruction;

				if (SiteUser != null && SiteUser.IsShipmentQuickViewUser)
				{
					CancelBooking.OnClientClick = string.Empty;
					ConvertBooking.OnClientClick = string.Empty;
					ShipperRefLabel.Visible = false;
					ShipperRef.Visible = false;
					CargoTypeRow.Visible = false;
					PaymentTermRow.Visible = false;
					GoodsDescriptionRow.Visible = false;
					DocumentsGrid.Visible = false;
					NotesPanel.Visible = false;
					EditBooking.Visible = false;
					CancelBooking.Visible = false;
					DuplicateBooking.Visible = false;
					ReverseBooking.Visible = false;
					if (BookedContainersGrid is ZGrid)
					{
						((ZGrid)BookedContainersGrid).ShouldShowControl = false;
					}
					if (ContainersGrid is ZGrid)
					{
						((ZGrid)ContainersGrid).ShouldShowControl = false;
					}
					if (PacksGrid is ZGrid)
					{
						((ZGrid)PacksGrid).ShouldShowControl = false;
					}
				}
			}
		}

		public new TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void DuplicateBooking_Click(object sender, EventArgs e)
		{
			if (Booking != null)
			{
				var newAgencyBooking = (TrackingLinerAndAgencyBooking)Booking.TemplateCopy();
				if (newAgencyBooking != null)
				{
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newAgencyBooking.PK, newAgencyBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.LinerAndAgencyEditBookingPage, newAgencyBooking.PK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void ReverseBooking_Click(object sender, EventArgs e)
		{
			if (Booking != null)
			{
				var newAgencyBooking = (TrackingLinerAndAgencyBooking)Booking.TemplateCopy();
				if (newAgencyBooking != null)
				{
					((ITemplateReversible)newAgencyBooking).Reverse();
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newAgencyBooking.PK, newAgencyBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.LinerAndAgencyEditBookingPage, newAgencyBooking.PK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewLinerAndAgencyBookings; }
		}

		protected bool AllowEditOrCancel
		{
			get
			{
				return Booking != null && !IsCancelled &&
					Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.WebFwdInstruction &&
					Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.Confirmed;
			}
		}

		protected bool IsCancelled
		{
			get { return Booking != null && Booking.JS_IsCancelled; }
		}

		protected bool CanConvertToInstruction
		{
			get { return Booking != null && !IsCancelled && Booking.JS_ShipmentStatus == ShipmentStatusList.Codes.Booked; }
		}

		protected void CancelReActivateBooking_Click(object sender, EventArgs e)
		{
			Booking.JS_IsCancelled = !Booking.JS_IsCancelled;
			SaveDataSourceFactory();
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyBookingDetailsPage;
		}
	}
}
