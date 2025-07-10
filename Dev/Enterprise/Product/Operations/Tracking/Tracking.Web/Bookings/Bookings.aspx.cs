using System;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Module;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Bookings
{
	/// <summary>
	/// Bookings summary and search page.
	/// </summary>
	public partial class Bookings : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return searchControl.Module.CreateNewFilterBusinessObject();
		}

		public QuotedBookingFilterStripBusinessObject FilterBusinessObject
		{
			get { return DataSource as QuotedBookingFilterStripBusinessObject; }
		}

		#endregion

		#region Page Setup

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		#endregion

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.TrackingBookings;
			searchControl.NewButtonUrl = !string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value) ?
				string.Format("{0}?RedirectUrl={1}", AppInstance.TermsAndConditionsPage, WebUtility.UrlEncode(AppInstance.EditBookingPage)) :  // partial URL
				AppInstance.EditBookingPage;
			searchControl.NewButtonText = Res.GetString("7398bfc7-aaa9-45b4-86fa-e9cb59c92c8d", "Make a booking");
			searchControl.IsNewButtonVisible = SiteUser.CanEditBookings;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetControlKeyIdentifier(System.Web.UI.Control control)
		{
			return searchControl.ModuleID.Name;
		}

		#endregion

		#region Overrides

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewBookings; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Forwarding Bookings"; } // Event logging
		}

		#endregion

		#region Automatically generated

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
			SetupSearchControl();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			OrdersLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
		}

		#endregion

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.Bookings;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.BookingsPage;
		}
	}
}
