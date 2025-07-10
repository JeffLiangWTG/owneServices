using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class OrgSupplierParts : WarehousingBasePage, IRememberFilterCriteriaPage, IModulePage
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

		#region Overrides

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ProductProfilesPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return true; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Products"; } // Event logging
		}

		#endregion

		#region Search Control setup

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.OrgSupplierPartTracking;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.OrgSupplierParts;
		}

		#endregion

		#region Initialization

		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
			SetupSearchControl();
		}

		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
		}

		#endregion
	}
}
