using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Declaration
{
	public partial class Declarations : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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
			searchControl.ModuleID = WebModuleIDs.TrackingDeclarations;
			searchControl.IsNewButtonVisible = false;

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.Declarations;
		}

		#endregion

		#region Overrides

		protected override bool CanAccessAuthorisedContent => SiteUser?.CanViewDeclarations ?? false;

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Declarations"; } // Event logging
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.DeclarationModulePage;
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
