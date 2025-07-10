using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public partial class Inventory : WarehousingBasePage, IRememberFilterCriteriaPage, IModulePage
	{
		public const string IsRefreshIndexer = "IsRefreshIndexer";
		public const string WhsOrderShoppingCartIndexer = "WhsOrderShoppingCart";
		public const string WhsOrderShoppingCartHelperIndexer = "WhsOrderShoppingCartHelperIndexer";
		public const string BusinessObjectForValidationIndexer = "BusinessObjectForValidationIndexer";
		public const string ShouldUseWhsOrderShoppingCart = "ShouldUseWhsOrderShoppingCart";

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.WarehouseInventory;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.InventoryPage;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewInventory; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			SwitchHyperLink.Visible = isAuthorised;
			base.SetupAuthorisedContent(isAuthorised);
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Warehouse Inventory"; } // Module name for logging
		}

		#endregion

		#region IsRefresh

		bool IsRefresh
		{
			get
			{
				return isRefresh;
			}
		}
		bool isRefresh;
		bool refreshState;

		protected override void LoadViewState(object savedState)
		{
			object[] allStates = (object[])savedState;
			base.LoadViewState(allStates[0]);
			refreshState = (bool)allStates[1];
			if (Session[IsRefreshIndexer] != null)
			{
				isRefresh = (refreshState == (bool)Session[IsRefreshIndexer]);
			}
		}

		protected override object SaveViewState()
		{
			Session[IsRefreshIndexer] = refreshState;
			object[] allStates = new object[2];
			allStates[0] = base.SaveViewState();
			allStates[1] = !refreshState;
			return allStates;
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

		#endregion

		#region Details

		bool showDetails = true;
		bool moduleIsChanging;

		void SearchControl_OnSearch(object sender, EventArgs e)
		{
			IsPerformSearch = true;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			SwitchHyperLink.Text = showDetails ? Res.GetString("942712f5-9667-4a98-915f-9c7b7d8f586f", "Hide Details") : Res.GetString("9ccd1ebc-42aa-4362-981a-08da47a0390b", "Show Details");
			SwitchHyperLink.NavigateUrl = String.Format((NoResString)"{0}?Mode={1}&Search={2}", AppInstance.InventoryPage, !showDetails, IsPerformSearch); // URL parameter
		}

		bool IsPerformSearch
		{
			get
			{
				object result = ViewState["IsPerformSearch"];
				return result == null ? Request.Params["Search"] == true.ToString() : (bool)result; // URL parameter
			}
			set { ViewState["IsPerformSearch"] = value; }
		}

		protected void GetOrSetDetalisationMode()
		{
			if (Request.Params["Mode"] == null || !bool.TryParse(Request.Params["Mode"], out showDetails))
			{
				showDetails = WebDataRegistry.Instance.GetShowDetailedInventory(SiteUser.LoggedInUser.PK);
			}
			else
			{
				bool initialValue = WebDataRegistry.Instance.GetShowDetailedInventory(SiteUser.LoggedInUser.PK);
				if (initialValue != showDetails)
				{
					WebDataRegistry.Instance.SetShowDetailedInventory(SiteUser.LoggedInUser.PK, showDetails);
					moduleIsChanging = true;
				}
			}
		}

		#endregion

		#region Search Control setup

		protected void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			string buttonText;

			if (showDetails)
			{
				searchControl.ModuleID = WebModuleIDs.ShoppingCartInventory;
				searchControl.NewButtonText = Res.GetString("2d90aa51-113b-4605-a606-6999c21c01cc", "Add to Order");
				searchControl.IsNewButtonVisible = true;
				searchControl.CustomNewButtonClick += SearchControl_CustomNewButtonClick;

				searchControl.SearchResultsDataGrid.Menu.Controls.AddAt(0, DetailedExportButton);
				DetailedExportButton.Click += DetailedExportButton_Click;
				searchControl.SearchResultsDataGrid.RegisterPostBackControl(DetailedExportButton);
				searchControl.SearchResultsDataGrid.OnExternallyFiredPostback += SearchResultsDataGrid_OnExternallyFiredPostback;
				buttonText = Res.GetString("d788ad96-4dc4-4425-9848-d9a1e0bf67c3", "Export to Excel (Summary)");
			}
			else
			{
				searchControl.ModuleID = WebModuleIDs.TrackingInventory;
				searchControl.IsNewButtonVisible = false;
				DetailedExportButton.Visible = false;
				buttonText = Res.GetString("abbf3bc6-6610-4bb2-b064-62be69a695fd", "Export to Excel");
			}

			searchControl.SearchResultsDataGrid.ItemDataBound += SearchResultDataGrid_OnDataBound;
			searchControl.SearchResultsDataGrid.SetButtonText(searchControl.SearchResultsDataGrid.ExportToExcelButton, buttonText);
			searchControl.SearchResultsDataGrid.ExportToExcelButton.ToolTip = buttonText;

			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual ZSearchControl GetNewSearchControl()
		{
			return new ZSearchControl { ShouldFindCauseAsyncPostBack = true };
		}

		public ISearchControl SearchControl { get { return searchControl; } }

		ZSearchControl searchControl;

		#endregion

		#region DetailedExportButton

		Button DetailedExportButton
		{
			get
			{
				if (detailedExportButton == null)
				{
					string text = Res.GetString("2acaf839-e845-46e3-ab2b-0cabd79b4b8e", "Export to Excel (Details)");
					return detailedExportButton = new Button
					{
						ID = "DetailedExportButton",
						Visible = true,
						Text = text,
						ToolTip = text,
						CssClass = (NoResString)"Button ExportToExcelButton" // Is a CSS Class
					};
				}
				return detailedExportButton;
			}
		}
		Button detailedExportButton;

		protected void DetailedExportButton_Click(object sender, EventArgs e)
		{
			if (searchControl.SearchResultsDataGrid != null)
			{
				using (ZFilterGridModule module = ZWebModuleFactory.Create(WebModuleIDs.TrackingInventoryDetails, Factory, Page))
				{
					if (module != null)
					{
						TrackingWhsInventoryCollection collectionToExport = (TrackingWhsInventoryCollection)module.LoadExcelCollection(searchControl.FilterBusinessObject);
						searchControl.SearchResultsDataGrid.ExportIntoExcel(collectionToExport, TrackingInventoryTemplate.GetInventoriesExcelExportColumns(collectionToExport));
					}
				}
			}
		}

		#endregion

		#region CustomNewButtonClick handler

#if DEBUG
		protected
#endif
		void SearchControl_CustomNewButtonClick(object sender, EventArgs e)
		{
			if (!IsRefresh)
			{
				TrackingInventorySummaryCollection summaries = searchControl.SearchResultsDataGrid.DataSource as TrackingInventorySummaryCollection;
				if (summaries != null)
				{
					foreach (TrackingInventorySummary summary in summaries)
					{
						foreach (TrackingWhsInventory inventory in summary.Inventories)
						{
							if (inventory.Quantity > 0)
							{
								ShoppingCartHelper.AddOrderLine(inventory);
							}
						}
					}
					TryBindShoppingCartLines();
					searchControl.RePopulateGrid();
				}
			}
		}

		#endregion

		#region SearchResultDataGrid_OnDataBound

		int? imageColumnIndex;

		void SearchResultDataGrid_OnDataBound(object sender, DataGridItemEventArgs e)
		{
			var item = e.Item;
			var itemType = item.ItemType;

			if (itemType == ListItemType.Header)
			{
				GetImageColumnIndex(item);
			}

			if (itemType == ListItemType.Item || itemType == ListItemType.AlternatingItem)
			{
				if (imageColumnIndex.HasValue)
				{
					var imageCell = item.Cells[imageColumnIndex.Value];
					var trackingInventorySummary = (TrackingInventorySummary)item.DataItem;
					var hasProductImage = trackingInventorySummary.HasProductImage;
					ShowOrHideImageIcon(imageCell, hasProductImage);
				}
			}
		}

		void GetImageColumnIndex(DataGridItem item)
		{
			for (int i = 0; i < item.Cells.Count; i++)
			{
				var cell = item.Cells[i];
				if (cell.Controls.Count > 0)
				{
					var linkButton = cell.Controls[0] as LinkButton;
					if (linkButton != null && linkButton.CommandArgument.Equals(TrackingInventorySummary.Schema.HasProductImage))
					{
						imageColumnIndex = i;
						break;
					}
				}
			}
		}

		void ShowOrHideImageIcon(TableCell imageCell, ZBool hasProductImage)
		{
			if (hasProductImage)
			{
				imageCell.HorizontalAlign = HorizontalAlign.Center;
			}
			else
			{
				imageCell.Controls[0].Visible = false; // the image icon is always the first control
			}
		}

		#endregion

		#region OnExternallyFiredPostback handler

		void SearchResultsDataGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			if (searchControl.SearchResultsDataGrid.Items.Count > 0)
			{
				Dictionary<ZGuid, ZDecimal> postedQuantities = GetResultsDataGridPostedQuantities();
				SetQuantitiesToResultsDataGrid(postedQuantities);
				SetupBusinessObjectForValidation();
			}
		}

		#region GetResultsDataGridPostedQuantities

		Dictionary<ZGuid, ZDecimal> GetResultsDataGridPostedQuantities()
		{
			Dictionary<ZGuid, ZDecimal> result = new Dictionary<ZGuid, ZDecimal>();
			foreach (DataGridItem item in searchControl.SearchResultsDataGrid.Items)
			{
				if (IsTrackingInventoriesColumnOk(item))
				{
					ZDataGrid inventoriesGrid = item.Cells[InventoriesColumnIndex].Controls[1] as ZDataGrid;
					foreach (DataGridItem inventoryItem in inventoriesGrid.Items)
					{
						ZDecimal quantityNum = GetQuantityValue(inventoryItem);
						if (quantityNum > 0)
						{
							ZGuid pkValue = GetPKValue(inventoryItem);
							if (!pkValue.IsEmpty && pkValue.IsValid)
							{
								result[pkValue] = quantityNum;
							}
						}
					}
				}
			}
			return result;
		}

		#region Helper methods for GetResultsDataGridPostedQuantities

		bool IsTrackingInventoriesColumnOk(DataGridItem item)
		{
			return InventoriesColumnIndex > -1 && item.Cells[InventoriesColumnIndex].Controls.Count > 1 && item.Cells[InventoriesColumnIndex].Controls[1] is ZDataGrid;
		}

		ZDecimal GetQuantityValue(DataGridItem item)
		{
			ZDecimal result = ZDecimal.Zero;
			int quantityColumnIndex = item.Cells.Count - 1;
			bool isColumnOk = quantityColumnIndex > -1 && item.Cells[quantityColumnIndex].Controls.Count > 0 && item.Cells[quantityColumnIndex].Controls[0] is ITextControl;
			if (isColumnOk)
			{
				string quantityString = ((ITextControl)item.Cells[quantityColumnIndex].Controls[0]).Text;
				ZDecimal.TryParse(quantityString, out result);
			}
			return result;
		}

		ZGuid GetPKValue(DataGridItem item)
		{
			ZGuid result = ZGuid.Empty;
			int urlColumnIndex = item.Cells.Count - 2;
			bool isColumnOk = urlColumnIndex > -1 && item.Cells[urlColumnIndex].Controls.Count > 0 && item.Cells[urlColumnIndex].Controls[0] is HyperLink;
			if (isColumnOk)
			{
				string detailsUrl = ((HyperLink)item.Cells[urlColumnIndex].Controls[0]).NavigateUrl;
				string[] split = detailsUrl.Split(new string[] { (NoResString)"?Ref=", "&" }, StringSplitOptions.RemoveEmptyEntries); // URL parameter
				if (split.Length > 1)
				{
					ZGuid.TryParse(split[1], out result);
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region SetQuantitiesToResultsDataGrid

		void SetQuantitiesToResultsDataGrid(Dictionary<ZGuid, ZDecimal> quantities)
		{
			if (quantities.Count > 0)
			{
				int quantitiesLeft = quantities.Count;
				TrackingInventorySummaryCollection summaries = searchControl.SearchResultsDataGrid.DataSource as TrackingInventorySummaryCollection;
				foreach (TrackingInventorySummary summary in summaries)
				{
					foreach (TrackingWhsInventory inventory in summary.Inventories)
					{
						if (quantities.ContainsKey(inventory.PK))
						{
							inventory.Quantity = quantities[inventory.PK];
							quantitiesLeft--;
							if (quantitiesLeft == 0)
							{
								return;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Column index properties

		protected int InventoriesColumnIndex
		{
			get { return searchControl.SearchResultsDataGrid.Columns.Count - 1; }
		}

		#endregion

		#endregion

		#region ShoppingCart Control resource

		protected ZWebResource ShoppingCartControlResource
		{
			get
			{
				if (fShoppingCartControlResource == null)
				{
					fShoppingCartControlResource = new ZWebResource(typeof(Inventory), "ShoppingCartUserControl.ascx", Page, "Enterprise.Tracking.Web.Warehousing");
				}
				return fShoppingCartControlResource;
			}
		}
		ZWebResource fShoppingCartControlResource;

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection resources = base.Resources;
				resources.Add(ShoppingCartControlResource);
				return resources;
			}
		}

		#endregion

		#region ShoppingCart Control setup

		protected ShoppingCartUserControl ShoppingCartControl
		{
			get
			{
				if (shoppingCartControl == null)
				{
					shoppingCartControl = GetNewShoppingCartControl();
					shoppingCartControl.NewButtonClicked += new NewButtonClickedHandler(ShoppingCartControl_NewButtonClicked);
					shoppingCartControl.ClearButtonClicked += new ClearButtonClickedHanlder(ShoppingCartControl_ClearButtonClicked);
					shoppingCartControl.OrderLinesGrid.OnExternallyFiredPostback += new EventHandler(OrderLinesGrid_OnExternallyFiredPostback);
					shoppingCartControl.OrderLinesGrid.AllowDelete = true;
					shoppingCartControl.OrderLinesGrid.AfterDeleteCommand += new DataGridCommandEventHandler(OrderLinesGrid_AfterDeleteCommand);
					SearchControlHolder.Controls.AddAt(1, shoppingCartControl);
				}
				return shoppingCartControl;
			}
		}
		protected ShoppingCartUserControl shoppingCartControl;

		protected virtual ShoppingCartUserControl GetNewShoppingCartControl()
		{
			return Page.LoadControl(ShoppingCartControlResource.FileName) as ShoppingCartUserControl;
		}

		#endregion

		#region ShoppingCart Control handlers

		void OrderLinesGrid_AfterDeleteCommand(object source, DataGridCommandEventArgs e)
		{
			if (ShoppingCartHelper.ShoppingCart.Lines.Count == 0)
			{
				ShoppingCartControl_ClearButtonClicked();
			}
		}

		void OrderLinesGrid_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			ShoppingCartHelper.TryRemoveZeroLines();
			if (ShoppingCartHelper.ShoppingCart.Lines.Count == 0)
			{
				ShoppingCartControl_ClearButtonClicked();
			}
			else
			{
				TryBindShoppingCartLines();
			}
		}

		void ShoppingCartControl_NewButtonClicked()
		{
			if (!BusinessObjectForValidation.HasErrors)
			{
				Session[ShouldUseWhsOrderShoppingCart] = true;
				Session[WhsOrderShoppingCartIndexer] = ShoppingCartHelper.ShoppingCart;
				Response.Redirect(AppInstance.WarehouseEditOrders);
			}
		}

#if DEBUG
		protected
#endif
		void ShoppingCartControl_ClearButtonClicked()
		{
			ClearInventoryLinksToShoppingCart();
			ShoppingCartControl.OrderLinesGrid.UnBind();
			Session[WhsOrderShoppingCartHelperIndexer] = null;
			BusinessObjectForValidation.Clear();
			SearchControlHolder.Controls.RemoveAt(1);
			shoppingCartControl = null;
		}

		void ClearInventoryLinksToShoppingCart()
		{
			var inventoriesWithShoppingCart = ((TrackingInventorySummaryCollection)searchControl.SearchResultsDataGrid.DataSource)?
				.OfType<TrackingInventorySummary>()
				.SelectMany(s => s.Inventories)
				.OfType<TrackingWhsInventory>()
				.Where(i => i.ShoppingCart != null);
			if (inventoriesWithShoppingCart != null)
			{
				foreach (var inventory in inventoriesWithShoppingCart)
				{
					inventory.ShoppingCart = null;
				}
			}
		}

		#endregion

		#region ShoppingCartHelper

		public TrackingWhsOrderShoppingCartHelper ShoppingCartHelper
		{
			get
			{
				if (Session[WhsOrderShoppingCartHelperIndexer] == null)
				{
					TrackingWhsOrderShoppingCartHelper shoppingCartHelper = new TrackingWhsOrderShoppingCartHelper(this.SiteUser);
					Session[WhsOrderShoppingCartHelperIndexer] = shoppingCartHelper;
				}
				return Session[WhsOrderShoppingCartHelperIndexer] as TrackingWhsOrderShoppingCartHelper;
			}
		}

		void TryBindShoppingCartLines()
		{
			if (ShoppingCartHelper.ShoppingCart.Lines.Count > 0)
			{
				ShoppingCartControl.OrderLinesGrid.Bind(ShoppingCartHelper.ShoppingCart.Lines);
			}
		}

		#endregion

		#region Page OnLoad Handler

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			searchControl.OnSearch += SearchControl_OnSearch;
			searchControl.OnClear += (x, y) => IsPerformSearch = false;

			TraverseControlTreeAndExtractResources(searchControl.SearchResultsDataGrid);
			TryBindShoppingCartLines();
			NotificationFlags.DisplayAll = false;
			NotificationFlags.DisplayErrors = true;
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			if (IsPerformSearch && moduleIsChanging)
			{
				searchControl.FindButton_Click(null, EventArgs.Empty);
			}
		}

		#endregion

		#region AutoGenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			base.OnInit(e);
			InitializeComponent();
			GetOrSetDetalisationMode();
			SetupSearchControl();
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
		}
		#endregion

		#endregion

		#region Validation

#if DEBUG
		protected
#endif
		void SetupBusinessObjectForValidation()
		{
			TrackingInventorySummaryCollection collectionToValidate = searchControl.SearchResultsDataGrid.DataSource as TrackingInventorySummaryCollection;
			if (collectionToValidate != null)
			{
				foreach (TrackingInventorySummary summary in collectionToValidate)
				{
					foreach (TrackingWhsInventory inventory in summary.Inventories)
					{
						inventory.EnableOnlyAllocateColumnValidationOnPreSave();
					}
				}
			}
			BusinessObjectForValidation = new TrackingWhsOrderShoppingCart(DataSource as InventoryFilterBusinessObject, collectionToValidate);
		}

		TrackingWhsOrderShoppingCart BusinessObjectForValidation
		{
			get
			{
				return Session[BusinessObjectForValidationIndexer] as TrackingWhsOrderShoppingCart;
			}
			set
			{
				Session[BusinessObjectForValidationIndexer] = value;
			}
		}

		protected override BusinessObject BusinessObjectToValidate
		{
			get
			{
				return BusinessObjectForValidation ?? base.BusinessObjectToValidate;
			}
		}

		#endregion
	}
}
