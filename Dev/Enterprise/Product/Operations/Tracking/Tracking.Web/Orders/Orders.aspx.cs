using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Orders
{
	/// <summary>
	/// Orders summary and search page.
	/// </summary>
	public partial class Orders : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		#region Constants

		public const string TimeLine = "TimeLine";

		public static string Normal
		{
			get { return Res.GetString("1f9d3eba-abb3-494b-a87c-8fe791fa9bb0", "Normal"); }
		}

		#endregion

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return searchControl.Module.CreateNewFilterBusinessObject();
		}

		public OrdersFilterBusinessObject FilterBusinessObject
		{
			get { return DataSource as OrdersFilterBusinessObject; }
		}

		#endregion

		#region Page Setup

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		#endregion

		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			searchControl.OnSearch += new EventHandler(SearchControl_OnSearch);
			searchControl.OnClear += new EventHandler(SearchControl_OnClear);

			if (!Page.IsPostBack)
			{
				ConfigurationHelper.ConfigureLegendLabels(PendingLegendLabel, OverdueLegendLabel, CompletedLegendLabel, CompletedLateLegendLabel);
			}
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			SwitchHyperLink.Visible = isAuthorised;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewOrders; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			SwitchHyperLink.Text = IsTimeLine ? Res.GetString("3785de65-ab26-45bc-9544-ae62b2b02dbe", "Switch to regular view") : Res.GetString("a10af9da-37d7-49bd-a10f-2c116f213bff", "Switch to Time-line");
			SwitchHyperLink.OnClientClick = string.Format(CultureInfo.InvariantCulture, (NoResString)@"__doPostBack('{0}', ''); return false;", SwitchHyperLink.ClientID); // Java script
			SetupTimeLineLegends();
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Forwarding Orders"; } // Event logging
		}

		#endregion

		#region Searching

		void SearchControl_OnSearch(object sender, EventArgs e)
		{
			IsPerformSearch = true;
			SetupTimeLineLegends();
		}

		void SearchControl_OnClear(object sender, EventArgs e)
		{
			IsPerformSearch = false;
		}

		protected bool IsTimeLine
		{
			get
			{
				return fIsTimeLine;
			}
			set
			{
				fIsTimeLine = value;
			}
		}
		protected bool fIsTimeLine = true;

		bool IsPerformSearch
		{
			set { ViewState["IsPerformSearch"] = value; }
		}

		#endregion

		#region Time Line Legends

		void SetupTimeLineLegends()
		{
			TimeLineLegendHolder.Visible = IsTimeLine;
		}

		#endregion

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = (IsTimeLine) ? WebModuleIDs.TrackingOrdersTimeline : WebModuleIDs.TrackingOrders;
			searchControl.NewButtonUrl = AppInstance.EditOrderPage;
			searchControl.NewButtonText = Res.GetString("d79fa209-985e-400d-86e8-55c87ab36544", "Place an order");
			searchControl.IsNewButtonVisible = SiteUser.CanEditOrders;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion

		#region Automatically generated

		#region Web Form Designer generated code
		protected override void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
			HandleModeSwitchClick();
		}

		protected void HandleModeSwitchClick()
		{
			CheckViewMode();
			bool switchClicked = false;
			if (IsSwitchHyperLinkClick)
			{
				switchClicked = true;
				ViewModeInSession = IsTimeLine ? Normal : TimeLine;
				WriteToRegistry(ViewModeInSession);
				CheckViewMode();
			}
			SetupSearchControl();
			if (switchClicked)
			{
				searchControl.IsResultsRelatedOperation = true;
				SetupTimeLineLegends();
			}
		}

		protected virtual bool IsSwitchHyperLinkClick
		{
			get { return Request.Form["__EVENTTARGET"] == SwitchHyperLink.ClientID; }
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			OrdersLabel.BindTo = null;
		}

		#endregion

		#endregion

		#region ViewMode

		protected void CheckViewMode()
		{
			if (string.IsNullOrEmpty(ViewModeInSession))
			{
				ViewModeInSession = ReadFromRegistry();
			}
			IsTimeLine = ViewModeInSession == TimeLine;
		}

		protected
#if DEBUG
 virtual
#endif
 string ReadFromRegistry()
		{
			return Env.Registry.GetTimeLineView(SiteUser.LoggedInUser.PK.ToGuid());
		}

		protected
#if DEBUG
 virtual
#endif
 void WriteToRegistry(string mode)
		{
			Env.Registry.SetTimeLineView(SiteUser.LoggedInUser.PK.ToGuid(), mode);
		}

		protected
#if DEBUG
 virtual
#endif
 string ViewModeInSession
		{
			get
			{
				return Session["TimeLineView"] as string;
			}
			set
			{
				if (Session["TimeLineView"] == null)
				{
					Session.Add("TimeLineView", value);
				}
				else
				{
					Session["TimeLineView"] = value;
				}
			}
		}

		#endregion

		protected override string GetPageName()
		{
			return WebTracker.Pages.Orders;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.OrdersPage;
		}
	}
}
