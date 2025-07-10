using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.ImporterSecurityFiling
{
	/// <summary>
	/// ImporterSecurityFilings summary and search page.
	/// </summary>
	public partial class ImporterSecurityFiling : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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
			searchControl.ModuleID = WebModuleIDs.TrackingImporterSecurityFiling;
			searchControl.NewButtonUrl = AppInstance.EditISFPage;
			searchControl.NewButtonText = Res.GetString("8e1a3a1c-1a9b-4d6a-80f3-8302676e4c2e", "New Importer Security Filing");
			searchControl.IsNewButtonVisible = SiteUser.CanEditISF;

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.ImporterSecurityFiling;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ISFPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewISF; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return Res.GetString("6fd05f9c-3e7d-4d41-a097-e6d5cc6a9572", "Forwarding Importer Security Filing"); }
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
	}
}
