using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Cartage
{
	public partial class Cartages : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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

		#region SearchControl

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.TrackingCartage;
			searchControl.IsNewButtonVisible = false;

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

		protected override string GetPageName()
		{
			return WebTracker.Pages.Cartages;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.CartagePage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewCartage; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return "TrackingCartages"; } // Event logging
		}

		#endregion

		#region Form

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
			SetupSearchControl();
		}

		void InitializeComponent()
		{
			PageTitleLabel.BindTo = null;
		}

		#endregion
	}
}
