using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	/// <summary>
	/// Search form for Containers. Initially displays container search page, but after user clicks Find button,
	/// the grid with results is shown
	/// </summary>
	public partial class LinerAndAgencyContainers : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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

		#endregion

		#region Page Setup

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanViewLinerAndAgencyContainers; }
		}

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
			searchControl.ModuleID = WebModuleIDs.LinerAndAgencyContainers;
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.LinerAndAgencyContainers;
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
			PageTitleLabel.BindTo = null;
		}

		#endregion

		#endregion

		#region Overrides

		protected override ZString ModuleNameForEventLogging
		{
			get { return "LinerAndAgencyContainers"; } // Event logging related
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyContainersPage;
		}

		#endregion
	}
}
