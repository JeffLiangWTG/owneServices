using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class Bookings : BasePageWithAuthorisation, IModulePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewLinerAndAgencyBookings; }
		}

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return searchControl.Module.CreateNewFilterBusinessObject();
		}

		#endregion

		#region Overrides

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return Res.GetString("5d0b07c8-3f7d-4a3f-8107-2279d442a5b4", "Shipping Bookings"); }
		}

		#endregion

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyBookingsPage;
		}

		#region Search Control setup

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);
			SetupSearchControl();
		}

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		[SuppressMessage("Microsoft.Globalization", "CA1305:Redirect URL.")]
		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.LinerAndAgencyBookings;
			searchControl.IsNewButtonVisible = SiteUser.CanEditLinerAndAgencyBookings;
			searchControl.NewButtonText = Res.GetString("0cdc438f-37c0-46e4-9614-7c692cffd9e3", "Make a Booking");
			searchControl.NewButtonClientClickScript = string.Format("javascript: window.location = '{0}'; return false;", AppInstance.LinerAndAgencyEditBookingPage); // javascript code
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.LinerAndAgencyBookings;
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion
	}
}
