using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class EditBooking : BookingBasePage
	{
		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyEditBookingPage;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveButton.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Booking != null && !IsPostBack)
			{
				Sailing.ButtonText = Res.GetString("18601279-412e-4232-b175-df4ef7787096", "Select Sailing");
				BindWebUserEditableNote(WebUserNote);
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanEditLinerAndAgencyBookings; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (Booking != null && Booking.JS_ShipmentStatus != ShipmentStatusList.Codes.WebBooking)
			{
				EnableChildWebControls(BookingDetailsDiv, false);
				EnableChildWebControls(OriginDestinationRow, true);
				EnableChildWebControls(EtdEtaRow, true);
				EnableChildWebControls(GoodsDescriptionRow, true);
				ShipperRefLabel.Enabled = true;
				ShipperRef.Enabled = true;

				EnableChildWebControls(SailingPanel, false);

				EnableChildWebControls(ContainersPanel, false);
				EnableChildWebControls(PacksPanel, false);
			}
		}

		void EnableChildWebControls(Control parent, bool enable)
		{
			foreach (Control control in parent.Controls)
			{
				WebControl webControl = control as WebControl;
				if (webControl != null)
				{
					webControl.Enabled = enable;
				}
				EnableChildWebControls(control, enable);
			}
		}

		protected override void SetupGrids()
		{
			base.SetupGrids();
			SetupDocumentsGrid(DocumentsGrid);
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditLinerAndAgencyBooking;
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			TrackingLinerAndAgencyBooking result = null;
			ZGuid pk = GetGuidFromParameter("Ref");

			if (pk.IsValid)
			{
				result = (TrackingLinerAndAgencyBooking)base.GetNewDataSource();
				if (result?.JS_IsCancelled ?? false)
				{
					result = null;
				}
			}
			else if (SiteUser.IsLoggedIn)
			{
				result = Factory.New<TrackingLinerAndAgencyBooking>();
				result.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
				result.BookingPartyDocumentaryAddress.OrganisationNameOrPK = SiteUser.LoggedInOrganisation.PK.ToString();
			}
			return result;
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		#region Event Handling

		protected void OnPackingModeChanged(object source, EventArgs e)
		{
			ContainersPanel.Visible = IsFCL;
			SetupPackLinesGrid();
			PacksGrid.Bind(Booking);
		}

		protected void OnSailingChanged(object source, EventArgs e)
		{
			Booking.JS_JX = Booking.JS_JX;  // To re-set Principal that could be lost during initial binding

			Principal.Bind(Booking);
			LoadPort.Bind(Booking);
			DischargePort.Bind(Booking);
			Origin.Bind(Booking);
			Destination.Bind(Booking);
			ETD.Bind(Booking);
			ETA.Bind(Booking);
		}

		protected override string ViewPageUrl
		{
			get { return AppInstance.LinerAndAgencyBookingDetailsPage; }
		}

		#endregion
	}
}
