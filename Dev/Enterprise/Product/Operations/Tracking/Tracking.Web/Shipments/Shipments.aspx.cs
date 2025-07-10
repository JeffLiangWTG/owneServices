using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
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
	public partial class Shipments : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string Show = "Show";
		public const string DoNotShow = "DoNotShow";
		public const string IsPerformSearchIndexer = "IsPerformSearch";
		public const string ShowUnshippedOrdersIndexer = "ShowUnshippedOrders";
		public const string UseUnshippedOrdersIndexer = "UseUnshippedOrders";

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

		TrackingShipmentFilterBusinessObject FilterBO
		{
			get { return DataSource as TrackingShipmentFilterBusinessObject; }
		}

		#endregion

		#region Page Setup

		bool UnshippedOrdersVisible
		{
			get { return UseUnshippedOrders && ShowUnshippedOrders; }
		}

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		#endregion

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.Shipments;
		}

		protected override void OnLoad(EventArgs e)
		{
			UnshippedOrdersContentPane.Visible = UnshippedOrdersVisible;
			if (UnshippedOrdersContentPane.Visible)
			{
				UnshippedOrdersSearchControl.FilterStripBizO = new OrdersFilterBusinessObject();
			}
			// Moved before base.OnLoad because we need to override FilterStripBizO before modules are set up on load
			base.OnLoad(e);
			searchControl.OnSearch += new EventHandler(SearchControl_OnSearch);
			SwitchHyperLink.Visible = UseUnshippedOrders;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (UseUnshippedOrders)
			{
				SwitchHyperLink.Text = ShowUnshippedOrders ? Res.GetString("ef68fbd4-0081-459a-a246-310b8369b8c9", "Hide unshipped orders") : Res.GetString("d8346f7b-fca6-4c3b-a8a9-e077e53ad0c4", "Show unshipped orders");
				SwitchHyperLink.NavigateUrl = String.Format((NoResString)"{0}?{1}Search={2}", // partial URL
						AppInstance.ShipmentsPage, ShowUnshippedOrders ? "" : "Mode=" + Show + "&", IsPerformSearch);
				ModeInSession = ShowUnshippedOrders;
			}
			if (UnshippedOrdersSearchControl != null)
			{
				UnshippedOrdersSearchControl.ShowCollapsedMessage = false;
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewShipments; }
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Forwarding Shipments"; } // Event logging
		}

		#endregion

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		protected void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.ModuleID = WebModuleIDs.TrackingShipments;
			searchControl.IsNewButtonVisible = false;
			searchControl.OnIsResultsRelatedOperationChanged += OnSearchControlIsResultsRelatedOperationChanged;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		void OnSearchControlIsResultsRelatedOperationChanged(object sender, EventArgs e)
		{
			if (UnshippedOrdersSearchControl != null && searchControl != null)
			{
				if (!UnshippedOrdersSearchControl.IsResultsRelatedOperation && searchControl.IsResultsRelatedOperation)
				{
					UnshippedOrdersSearchControl.IsResultsRelatedOperation = searchControl.IsResultsRelatedOperation;
				}
			}
		}

		ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl();
		}

		#endregion

		#region Searching

		void SearchControl_OnSearch(object sender, EventArgs e)
		{
			IsPerformSearch = true;
			if (UnshippedOrdersVisible)
			{
				new ShipmentToOrderFilterMapHelper(FilterBO, (OrdersFilterBusinessObject)UnshippedOrdersSearchControl.FilterStripBizO).MapFilters();
				UnshippedOrdersSearchControl.FindButton_Click(this, new EventArgs());
			}
		}

		void SearchControl_OnClear(object sender, EventArgs e)
		{
			IsPerformSearch = false;
		}

		bool IsPerformSearch
		{
			get
			{
				object result = ViewState[IsPerformSearchIndexer];
				return result != null ? (bool)result : SearchParam;
			}
			set { ViewState[IsPerformSearchIndexer] = value; }
		}

		#endregion

		#region Unshipped Orders Search Control setup

		protected ZSearchControl UnshippedOrdersSearchControl;

		protected void SetupUnshippedOrdersSearchControl()
		{
			if (UseUnshippedOrders && ShowUnshippedOrders)
			{
				UnshippedOrdersSearchControl = GetNewSearchControl();
				UnshippedOrdersSearchControl.ModuleID = WebModuleIDs.TrackingOrders;
				UnshippedOrdersSearchControl.HideFilterHeader();
				UnshippedOrdersSearchControl.IsExportToExcelButtonVisible = true;
				UnshippedOrdersSearchControl.IsCustomiseColumnsButtonVisible = false;
				UnshippedOrdersSearchControl.OnIsResultsRelatedOperationChanged += UnshippedOrdersOnIsResultsRelatedOperationChanged;
				if (searchControl != null)
				{
					//UnshippedOrdersSearchControl.IsResultsRelatedOperation = searchControl.IsResultsRelatedOperation;
				}
				UnshippedOrdersSearchControlHolder.Controls.AddAt(0, UnshippedOrdersSearchControl);
			}
		}

		void UnshippedOrdersOnIsResultsRelatedOperationChanged(object sender, EventArgs e)
		{
			if (UnshippedOrdersSearchControl != null && searchControl != null)
			{
				if (!searchControl.IsResultsRelatedOperation && UnshippedOrdersSearchControl.IsResultsRelatedOperation)
				{
					searchControl.IsResultsRelatedOperation = UnshippedOrdersSearchControl.IsResultsRelatedOperation;
				}
			}
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
			SetupUnshippedOrdersSearchControl();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			PageTitleLabel.BindTo = null;
			UnshippedOrdersTitleLabel.BindTo = null;
		}

		#endregion

		#endregion

		#region Params

		#region Search param

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		protected
#if DEBUG
 virtual
#endif
 bool SearchParam
		{
			get
			{
				string result = Request.Params["Search"] as string;
				return !string.IsNullOrEmpty(result) && result.ToLower() == "true";
			}
		}

		protected
#if DEBUG
 virtual
#endif
 bool SearchParamExist
		{
			get
			{
				return Request.Params["Search"] is string;
			}
		}

		#endregion

		#region Mode param

		protected
#if DEBUG
 virtual
#endif
 bool ModeParam
		{
			get
			{
				string result = Request.Params["Mode"] as string;
				return !string.IsNullOrEmpty(result) && result == Show;
			}
		}

		protected
#if DEBUG
 virtual
#endif
 bool ModeInSession
		{
			get
			{
				if (Session[ShowUnshippedOrdersIndexer] == null)
				{
					bool mode;
					string modeFromRegistry = ReadFromRegistry();
					if (string.IsNullOrEmpty(modeFromRegistry))
					{
						mode = true;
						WriteToRegistry(Show);
					}
					else
					{
						mode = modeFromRegistry == Show;
					}
					Session.Add(ShowUnshippedOrdersIndexer, mode);
				}
				return (bool)Session[ShowUnshippedOrdersIndexer];
			}
			set
			{
				if (Session[ShowUnshippedOrdersIndexer] != null && (bool)Session[ShowUnshippedOrdersIndexer] != value)
				{
					WriteToRegistry(value ? Show : DoNotShow);
				}
				Session[ShowUnshippedOrdersIndexer] = value;
			}
		}

		#endregion

		#endregion

		#region Registry

		protected
#if DEBUG
 virtual
#endif
 string ReadFromRegistry()
		{
			var userPK = (SiteUser?.LoggedInUserPK).GetValueOrDefault();
			if (!userPK.IsEmpty)
			{
				return Env.Registry.GetShowUnshippedOrdersMode(userPK.ToGuid());
			}

			return null;
		}

		protected
#if DEBUG
 virtual
#endif
 void WriteToRegistry(string mode)
		{
			var userPK = (SiteUser?.LoggedInUserPK).GetValueOrDefault();
			if (!userPK.IsEmpty)
			{
				Env.Registry.SetShowUnshippedOrdersMode(userPK.ToGuid(), mode);
			}
		}

		#endregion

		#region ShowUnshippedOrders

		protected
#if DEBUG
 virtual
#endif
 bool ShowUnshippedOrders
		{
			get { return SearchParamExist ? ModeParam : ModeInSession; }
		}

		#endregion

		#region UseUnshippedOrders

		bool UseUnshippedOrders
		{
			get
			{
				if (Session[UseUnshippedOrdersIndexer] == null)
				{
					Session.Add(UseUnshippedOrdersIndexer, WebDataRegistry.Instance.UseShipmentPageUnShippedOrders.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				}
				return (bool)Session[UseUnshippedOrdersIndexer];
			}
		}

		#endregion

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				List<ILicenceCheckpoint> result = new List<ILicenceCheckpoint>();
				result.Add(Environment.Env.Licence.WebTrackerForwarding);
				if (ShowUnshippedOrders)
				{
					result.Add(Environment.Env.Licence.WebTrackerOrderManager);
				}

				return result.ToArray();
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ShipmentsPage;
		}
	}
}
