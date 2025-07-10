using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Search form for Shipments. Initially displays shipment search page, but after user clicks Find button,
	/// the grid with results is shown
	/// </summary>
	public partial class CFSShipments : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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
			searchControl.ModuleID = WebModuleIDs.TrackingCFSShipments;
			searchControl.IsNewButtonVisible = false;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion

		#region Overrides
		protected override string GetPageName()
		{
			return WebTracker.Pages.CFSShipments;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewCFSShipments; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"CFS Shipments"; } // Event logging
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

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				List<ILicenceCheckpoint> result = new List<ILicenceCheckpoint>();
				result.Add(Environment.Env.Licence.WebTrackerCFS);
				return result.ToArray();
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.CFSShipmentsPage;
		}
	}
}
