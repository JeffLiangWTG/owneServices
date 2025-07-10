using System;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Containers
{
	/// <summary>
	/// Search form for Containers. Initially displays container search page, but after user clicks Find button,
	/// the grid with results is shown
	/// </summary>
	public partial class Containers : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
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
			get { return SiteUser != null && SiteUser.CanViewTrackingContainers; }
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
			searchControl.ModuleID = WebModuleIDs.TrackingContainers;
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;

			searchControl.NewButtonText = Res.GetString("086e0f41-6acf-47f0-af71-583c7a9d530e", "View Summary");
			searchControl.IsNewButtonVisible = IsPostBack;
			searchControl.CustomNewButtonClick += new EventHandler(ViewSummaryForSelectedContainersHandler);

			searchControl.ViewButtonText = Res.GetString("8d9f2370-1023-45be-8798-3031cf4200d7", "Update Selected Containers");
			searchControl.IsViewButtonVisible = IsPostBack;
			searchControl.CustomViewButtonClick += new EventHandler(UpdateSelectedContainersHandler);

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		void UpdateSelectedContainersHandler(object sender, EventArgs e)
		{
			ZGuid[] selected = searchControl.SearchResultsDataGrid.GetSelectedPKs();

			if (selected.Length > 0)
			{
				ZStringBuilder containers = new ZStringBuilder();

				for (int i = 0; i < selected.Length; i++)
				{
					containers.Append(selected[i].ToString());
				}

				ZString pKs = containers.ToStringWithDelimiterBetweenAppends(",");

				Response.Redirect(String.Format((NoResString)"{0}?Ref={1}", AppInstance.ContainerBatchUpdatePage, pKs)); // Partial URL
			}
		}

		void ViewSummaryForSelectedContainersHandler(object sender, EventArgs e)
		{
			searchControl.RePopulateGrid();

			TrackingContainerStandaloneCollection containers = searchControl.SearchResultsDataGrid.DataSource as TrackingContainerStandaloneCollection;

			if (containers != null && containers.Count > 0)
			{
				ContainerBatchSummary summary = new ContainerBatchSummary(containers);
				HttpContext.Current.Session[summary.PK.ToString()] = summary;

				Response.Redirect(String.Format((NoResString)"{0}?Ref={1}", AppInstance.ContainerSummaryPage, summary.PK)); // Partial URL
			}
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.Containers;
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
			get { return Res.GetString("61e33aee-cc16-4070-bb2f-42ace47086ea", "Forwarding Containers"); }
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ContainersPage;
		}

		#endregion
	}
}
