using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public partial class BillsOfLading : BasePageWithAuthorisation, IModulePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewLinerAndAgencyBillsOfLading; }
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

		protected override string GetPageName()
		{
			return WebTracker.Pages.BillsOfLading;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.LinerAndAgencyBillsOfLadingPage;
		}

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Liner And Agency Bills Of Lading"; } // Event logging related
		}

		#endregion

		#region Search Control setup

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);
			SetupSearchControl();
		}

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.LinerAndAgencyBillsOfLading;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion
	}
}
