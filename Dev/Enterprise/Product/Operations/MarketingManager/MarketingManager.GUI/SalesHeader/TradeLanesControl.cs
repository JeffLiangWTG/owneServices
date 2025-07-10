using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeLanesControl : ZUserControl
	{
		#region Static / Constructors

		public static TradeLanesControl New(OrgSalesProduct salesProduct)
		{
			switch (salesProduct.MP_Code)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
					return new CustomsBrokerageTradeLanesControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					return new ForwardingShipmentTradeLanesControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.LinerAgency:
					return new LinerAgencyTradeLanesControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.Transport:
					return new TransportTradeLanesControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.Warehouse:
					return new WarehouseTradeLanesControl(salesProduct);

				default:
					return new GenericTradeLanesControl(salesProduct);
			}
		}

		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public TradeLanesControl()
		{
			InitializeComponent();
		}

		public TradeLanesControl(OrgSalesProduct salesProduct)
		{
			Argument.NotNull(salesProduct, "salesProduct");

			this.salesProduct = salesProduct;

			var customColumnCollection = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Lane, new ReadOnlyBusinessObjectFactory() { RefreshEnabled = false });
			customColumnCollection.Load();
			customColumns = customColumnCollection.Cast<OrgSalesProductCustomColumnDefinition>().ToList();

			InitializeComponent();
		}

		readonly OrgSalesProduct salesProduct;

		readonly IList<OrgSalesProductCustomColumnDefinition> customColumns;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupTradeLanesGrid();
		}

		#endregion

		#region DataBinding

		SalesHeader SalesHeader
		{
			get { return (SalesHeader)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (SalesHeader != null)
			{
				SalesHeader.EntitySalesCollectionProductView.SalesMatchingPropertyChanged -= SalesMatchingPropertyChanged;
				SalesHeader.RevenueDisplayOptionChanged -= SalesHeader_RevenueDisplayOptionChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			AddAdditionalColumns();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (SalesHeader != null)
			{
				SalesHeader.EntitySalesCollectionProductView.SalesMatchingPropertyChanged -= SalesMatchingPropertyChanged;
				SalesHeader.EntitySalesCollectionProductView.SalesMatchingPropertyChanged += SalesMatchingPropertyChanged;

				SalesHeader.RevenueDisplayOptionChanged -= SalesHeader_RevenueDisplayOptionChanged;
				SalesHeader.RevenueDisplayOptionChanged += SalesHeader_RevenueDisplayOptionChanged;
				SetupTradeLaneGridColumnCaptions();
			}
		}

		void SalesHeader_RevenueDisplayOptionChanged(object sender, EventArgs e)
		{
			SetupTradeLaneGridColumnCaptions();
		}

		#endregion

		#region Sales Matching

		void SalesMatchingPropertyChanged(object sender, EventArgs e)
		{
			var collectionView = TradeLanesGrid.List as IBusinessObjectCollectionView;
			if (collectionView == null)
			{
				return;
			}

			var sales = sender as EntitySalesWrapper;
			if (sales != null)
			{
				var salesMatchingOptions = salesProduct.SalesMatchingOptions;
				var allSalesPropertiesEntered = salesMatchingOptions.SalesPropertiesForMatching.All(property => !sales.FindPropertyInfo(property.Item1).Value.IsEmpty);
				if (allSalesPropertiesEntered)
				{
					var tradeDetail = sales.EntityTradeDetails.FirstOrDefault();
					if (salesProduct.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse && tradeDetail != null && tradeDetail.PA_OP.IsEmpty)
					{
						SalesHeader.ViewingOrg.SalesCollection.RefreshBinding();
						return;
					}

					var matching = new SalesMatching(SalesHeader.ViewingOrg, sales, tradeDetail, salesMatchingOptions);
					if (matching.MatchedSalesCollection.Count > 0)
					{
						var hasModeAndType = !(salesProduct.MP_Code == SystemDefinedSalesProductList.Codes.Transport || salesProduct.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse);
						var form = new SalesMatchingForm(matching, hasModeAndType);
						form.FormClosed += SalesMatchingForm_FormClosed;
						ZFormModaliser.ShowDialogAndDispose(form);
					}
				}
			}
		}

		void SalesMatchingForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			var form = (SalesMatchingForm)sender;
			if (form != null)
			{
				form.FormClosed -= SalesMatchingForm_FormClosed;
				var grid = TradeLanesGrid as AutoMatchingTradeLanesGrid;
				if (form.SelectedSalesMatchingData != null && grid != null)
				{
					grid.SetCurrentRowIndexOverride(form.SelectedSalesMatchingData.Sales.PK);

					var tradeLane = (EntitySalesWrapper)SalesHeader.EntitySalesCollectionProductView.FindByPK(form.SelectedSalesMatchingData.Sales.PK);
					if (tradeLane.HasGroupings)
					{
						var grouping = tradeLane.EntityDetailGroupings;
						grouping.RemoveAll();
						grouping.PopulateDefaultElements();
					}
				}
			}
		}

		#endregion

		#region TradeLanesGrid

		MenuItem DetachMenuItem;

		public virtual ZGrid TradeLanesGrid
		{
			get { return null; }
		}

		void SetupTradeLanesGrid()
		{
			if (TradeLanesGrid != null)
			{
				new TradeLanesGridMenuItemsBuilder(salesProduct, TradeLanesGrid).AddMenuItems();
				TradeLanesGrid.RowsDeleting += TradeLanesGrid_RowsDeleting;
				TradeLanesGrid.ContextMenu.Popup += TradeLanesGridContextMenu_Popup;

				DetachMenuItem = new ZMenuItem(ResString.GetMultilingualString("fdef9906-3548-4080-9dab-7df02081c03a", "Detach"), DetachMenuItem_Click);
				var deleteMenuItemIndex = TradeLanesGrid.ContextMenu.MenuItems.IndexOf(TradeLanesGrid.DeleteMenuItem);
				TradeLanesGrid.ContextMenu.MenuItems.Add(deleteMenuItemIndex + 1, DetachMenuItem);
			}
		}

		void TradeLanesGridContextMenu_Popup(object sender, EventArgs e)
		{
			if (TradeLanesGrid != null)
			{
				var tradeLane = TradeLanesGrid.GetCurrent() as EntitySalesWrapper;
				DetachMenuItem.Enabled = (tradeLane != null && !tradeLane.IsEditable);
				DetachMenuItem.Visible = (TradeLanesGrid.RemoveAction != RemoveAction.NoRemovePossible && TradeLanesGrid.List.AllowRemove);
			}
		}

		void DetachMenuItem_Click(object sender, EventArgs e)
		{
			if (TradeLanesGrid != null)
			{
				var revertedDetails = new List<OrgTradeDetail>();
				if (TradeLanesGrid.GetSelectedRows().Length > 0)
				{
					foreach (var row in TradeLanesGrid.GetSelectedRows())
					{
						var sales = (EntitySalesWrapper)row;
						revertedDetails.AddRange(sales.RevertSupersededPeriodsForDetachedSales());
						sales.EntityTradeDetailsCollection.RemoveAndDeleteAll();
					}
				}
				else
				{
					var sales = (EntitySalesWrapper)TradeLanesGrid.ListManager.GetCurrent();
					revertedDetails = sales.RevertSupersededPeriodsForDetachedSales();
					sales.EntityTradeDetailsCollection.RemoveAndDeleteAll();
				}

				TradeLanesGrid.DeleteMenuItem.PerformClick();
				SalesHeader.HasChanges = true;
				InformUserOfRevert(revertedDetails);
			}
		}

		static void InformUserOfRevert(List<OrgTradeDetail> details)
		{
			var associations2 = details.Aggregate(string.Empty, (current, detail) => current + string.Join(",", detail.SalesAssociationPivotCollectionGlobal.Select(x => x.AssociatedEntity.ID)));
			Globals.Message.Show(Res.GetString("acea6377-9ef6-4c5a-bb99-a477961428ad", "As a consequence of this Opportunity's actions, some estimate values for the detail associated with Activity ID(s): {0} were superseded. These have now been reinstated.", associations2));
		}

		#region Columns

		void AddAdditionalColumns()
		{
			if (!additionalColumnsAdded)
			{
				AddProductCustomColumns();
				AddAuditColumns();

				additionalColumnsAdded = true;
			}
		}
		bool additionalColumnsAdded;

		void AddProductCustomColumns()
		{
			if (TradeLanesGrid != null)
			{
				var builder = new CustomGridPropertiesBuilder(TradeLanesGrid);
				builder.Add(
					x => OrgSalesProduct.GetNewCustomGridColumnsBusinessObject(x, customColumns, salesProduct.FormLayout.TradeLaneGridColumnDefinitions),
					OrgSalesProduct.GetCustomGridColumns(null, customColumns, salesProduct.FormLayout.TradeLaneGridColumnDefinitions));
			}
		}

		void AddAuditColumns()
		{
			if (TradeLanesGrid != null)
			{
				FilterStripAuditDetails.AddAuditDetailsColumns(TradeLanesGrid, OrgSalesSchema.Constants.TableName, typeof(OrgSales));
			}
		}

		#endregion

		#region Delete Row

		void TradeLanesGrid_RowsDeleting(object sender, RowsDeletingEventArgs e)
		{
			TradeLanesGrid.RemoveAction = RemoveAction.RemoveAndDelete;
			foreach (OrgSales sales in e.Objects)
			{
				if (!sales.IsEditable)
				{
					var isRemoveAllowed = salesProduct.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgSales);
					if (isRemoveAllowed)
					{
						TradeLanesGrid.RemoveAction = RemoveAction.Remove;
					}
					break;
				}
			}
		}

		#endregion

		#endregion

		#region ShowActualsIfSupported

		public void ShowActualsIfSupported()
		{
			if (salesProduct.IsActualsSupported)
			{
				if (!showingActualsIfSupported)
				{
					showingActualsIfSupported = true;

					if (TradeLanesGrid != null)
					{
						TradeLanesControlActualColumnsTemplate.AddActualColumnStyles(TradeLanesGrid);
						TradeLanesGrid.GridColourSchemeManagerDeciding += TradeLanesGrid_GridColourSchemeManagerDeciding;

						var markAsProspectMenuItem = new ZMenuItem(ResString.GetMultilingualString("440f521d-ab29-4400-9d78-b7d08e194903", "Mark as PRS (Prospective)"), MarkAsProspectMenuItemOnClick);
						TradeLanesGrid.ContextMenu.MenuItems.Add(markAsProspectMenuItem);
					}
				}
			}
		}

		bool showingActualsIfSupported;

		void TradeLanesGrid_GridColourSchemeManagerDeciding(object sender, GridColourSchemeManagerDecidingEventArgs e)
		{
			if (SalesHeader != null)
			{
				e.GridColourSchemeManager = new EstimateSalesAnalysisTradeLaneGridColourSchemeManager(TradeLanesGrid, SalesHeader.FilterableEntitySalesCollection);
			}
		}

		void MarkAsProspectMenuItemOnClick(object sender, EventArgs e)
		{
			var current = TradeLanesGrid.GetCurrent() as EntitySalesWrapper;
			if (current != null)
			{
				if (current.ActualsInformation.Status == OrgSalesActualsStatusList.Codes.Lost)
				{
					current.OW_LatestProspectDate = ZDate.Today;
				}
				else
				{
					Globals.Message.ShowError(
						Res.GetString("2ad912fa-b545-4929-8e9e-76ade85c8612", "You can only mark lost trade lanes as prospective."),
						Res.GetString("071ad7ea-09a1-4add-b5b3-7c55169783b5", "Cannot mark as prospective"));
				}
			}
		}

		void SetupTradeLaneGridColumnCaptions()
		{
			if (showingActualsIfSupported)
			{
				if (SalesHeader == null || SalesHeader.RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear)
				{
					TradeLanesGrid.SetColumnVisible(true, "ActualsInformation+CommittedYearToDateValue");
					TradeLanesGrid.SetColumnVisible(true, "ActualsInformation+CommittedYearToDateTEUQuantity");
					TradeLanesGrid.SetColumnCaption("TotalTradedValue", SalesHeaderValueColumnCaptionHelper.TradedCurrentFinancialYearValue);
					TradeLanesGrid.SetColumnCaption("TotalTradedTEUQuantity", SalesHeaderValueColumnCaptionHelper.TradedCurrentFinancialYearTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedYearToDateValue", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearToDateValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedYearToDateTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearToDateTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastCurrentFinancialYearValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastCurrentFinancialYearTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedNextFinancialYearValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedNextFinancialYearTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNextFinancialYearValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNextFinancialYearTEUQuantity);
				}
				else
				{
					TradeLanesGrid.SetColumnVisible(false, "ActualsInformation+CommittedYearToDateValue");
					TradeLanesGrid.SetColumnVisible(false, "ActualsInformation+CommittedYearToDateTEUQuantity");
					TradeLanesGrid.SetColumnCaption("TotalTradedValue", SalesHeaderValueColumnCaptionHelper.TradedTrailing12MonthsValue);
					TradeLanesGrid.SetColumnCaption("TotalTradedTEUQuantity", SalesHeaderValueColumnCaptionHelper.TradedTrailing12MonthsTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedTrailing12MonthsValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedTrailing12MonthsTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastTrailing12MonthsValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastTrailing12MonthsTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedNext12MonthsValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedNext12MonthsTEUQuantity);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNext12MonthsValue);
					TradeLanesGrid.SetColumnCaption("ActualsInformation+CommittedAndForecastNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNext12MonthsTEUQuantity);
				}
			}
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (TradeLanesGrid != null)
			{
				TradeLanesGrid.RowsDeleting -= TradeLanesGrid_RowsDeleting;
				TradeLanesGrid.ContextMenu.Popup -= TradeLanesGridContextMenu_Popup;
			}

			if (DetachMenuItem != null)
			{
				DetachMenuItem.Dispose();
				DetachMenuItem = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
