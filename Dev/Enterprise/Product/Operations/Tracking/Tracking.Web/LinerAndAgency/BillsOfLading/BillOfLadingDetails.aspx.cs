using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class BillOfLadingDetails : BillOfLadingBasePage
	{
		protected override string GetPageName()
		{
			return WebTracker.Pages.BillOfLadingDetails;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void Page_Load(object sender, EventArgs e)
		{
			if (BillOfLading != null)
			{
				if (AllowEdit)
				{
					ShippingBillOfLadingLabel.Text = Res.GetString("1494395c-eeb7-489a-951e-c64f2f375b2f", "Forwarding Instruction");
					Header.Title = Res.GetString("816c978f-3995-4fda-aa23-49f930a514a4", "View Forwarding Instruction's Details");
					EditButton.OnClientClick = string.Format((NoResString)"javascript: window.location = '{0}?Ref={1}'; return false;", AppInstance.LinerAndAgencyEditForwardingInstructionPage, GetGuidFromParameter("Ref")); // javascript code
				}

				EditButton.Visible = AllowEdit;
				DocsMenu.DocumentMenuProvider = (LinerAndAgencyBillOfLadingWebInterfacesHelper)WebInterfacesHelper;

				if (SiteUser != null && SiteUser.IsShipmentQuickViewUser)
				{
					DocumentsGrid.Visible = false;
					NotesPanel.Visible = false;
					Addresses.Visible = false;
					ShipperCargoRow.Visible = false;
					PaymentReleaseRow.Visible = false;
					BillsRow.Visible = false;
					GoodsDescriptionRow.Visible = false;

					if (ContainersGrid is ZGrid)
					{
						((ZGrid)ContainersGrid).ShouldShowControl = false;
					}
					if (PacksGrid is ZGrid)
					{
						((ZGrid)PacksGrid).ShouldShowControl = false;
					}

					DuplicateBillOfLading.Visible = false;
					ReverseBillOfLading.Visible = false;
					EditButton.Visible = false;
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void DuplicateBillOfLading_Click(object sender, EventArgs e)
		{
			var oldBooking = Factory.Load<TrackingLinerAndAgencyBooking>(BillOfLading.PK);
			if (oldBooking != null)
			{
				var newBooking = oldBooking.TemplateCopy() as TrackingLinerAndAgencyBooking;
				if (newBooking != null)
				{
					newBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
					newBooking.BookingPartyDocumentaryAddress.OrganisationNameOrPK = SiteUser.LoggedInOrganisation.PK.ToString();
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.PK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.LinerAndAgencyEditBookingPage, newBooking.PK, DataSourceInSessionParameterName, DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		protected void ReverseBillOfLading_Click(object sender, EventArgs e)
		{
			var oldBooking = Factory.Load<TrackingLinerAndAgencyBooking>(BillOfLading.PK);
			if (oldBooking != null)
			{
				var newBooking = oldBooking.TemplateCopy() as TrackingLinerAndAgencyBooking;
				if (newBooking != null)
				{
					((ITemplateReversible)newBooking).Reverse();
					newBooking.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
					newBooking.BookingPartyDocumentaryAddress.OrganisationNameOrPK = SiteUser.LoggedInOrganisation.PK.ToString();
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.PK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.LinerAndAgencyEditBookingPage, newBooking.PK, DataSourceInSessionParameterName, DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected bool AllowEdit
		{
			get
			{
				return SiteUser != null && SiteUser.CanEditLinerAndAgencyFwdInstructions &&
					BillOfLading != null && BillOfLading.JS_ShipmentStatus == ShipmentStatusList.Codes.WebFwdInstruction;
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewLinerAndAgencyBillsOfLading; }
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();
			ConsignorLabel.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
		}

		public new TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyBillOfLadingDetailsPage;
		}
	}
}
