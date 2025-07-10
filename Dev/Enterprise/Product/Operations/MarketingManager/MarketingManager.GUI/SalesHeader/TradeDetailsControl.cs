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
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeDetailsControl : ZUserControl
	{
		#region Static / Constructors

		public static TradeDetailsControl New(OrgSalesProduct salesProduct)
		{
			switch (salesProduct.MP_Code)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
					return new CustomsBrokerageTradeDetailsControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.LinerAgency:
					return new JobCommonTradeDetailsControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					return new ForwardingShipmentTradeDetailsControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.Transport:
					return new TransportTradeDetailsControl(salesProduct);

				case SystemDefinedSalesProductList.Codes.Warehouse:
					return new WarehouseTradeDetailsControl(salesProduct);

				default:
					return new GenericTradeDetailsControl(salesProduct);
			}
		}

		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public TradeDetailsControl()
		{
			InitializeComponent();
		}

		public TradeDetailsControl(OrgSalesProduct salesProduct)
		{
			Argument.NotNull(salesProduct, "salesProduct");

			InitializeComponent();
			this.salesProduct = salesProduct;

			var customColumnCollection = new OrgSalesProductCustomColumnDefinitionCollection(salesProduct, SalesProductDefinitionGroup.Detail, new ReadOnlyBusinessObjectFactory() { RefreshEnabled = false });
			customColumnCollection.Load();
			customColumns = customColumnCollection.Cast<OrgSalesProductCustomColumnDefinition>().ToList();
		}

		protected OrgSalesProduct SalesProduct
		{
			get { return salesProduct; }
		}

		readonly OrgSalesProduct salesProduct;

		readonly IList<OrgSalesProductCustomColumnDefinition> customColumns;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupTradeDetailsGrid();
		}

		#endregion

		#region DataBinding

		protected EntitySalesWrapper EntitySales
		{
			get { return CurrentDataItem; }
		}

		public new EntitySalesWrapper CurrentDataItem
		{
			get { return (EntitySalesWrapper)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			AddCustomColumns(TradeDetailsGrid);

			if (EntitySales != null)
			{
				EntitySales.EntityTradeDetailsCollection.SalesMatchingPropertyChanged -= EntityTradeDetailsCollection_SalesMatchingPropertyChanged;
				EntitySales.EntityTradeDetailsCollection.SalesMatchingPropertyChanged += EntityTradeDetailsCollection_SalesMatchingPropertyChanged;
				EntitySales.EntityTradeDetailsCollection.DefaultTradeDetailRemovedBySalesMatching -= EntityTradeDetailsCollection_DefaultTradeDetailRemovedBySalesMatching;
				EntitySales.EntityTradeDetailsCollection.DefaultTradeDetailRemovedBySalesMatching += EntityTradeDetailsCollection_DefaultTradeDetailRemovedBySalesMatching;
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (EntitySales != null)
			{
				EntitySales.EntityTradeDetailsCollection.SalesMatchingPropertyChanged -= EntityTradeDetailsCollection_SalesMatchingPropertyChanged;
				EntitySales.EntityTradeDetailsCollection.DefaultTradeDetailRemovedBySalesMatching -= EntityTradeDetailsCollection_DefaultTradeDetailRemovedBySalesMatching;
			}
		}

		#endregion

		#region Trade Details Grid

		public virtual ZGrid TradeDetailsGrid
		{
			get { return null; }
		}

		void SetupTradeDetailsGrid()
		{
			if (TradeDetailsGrid != null)
			{
				new TradeDetailsGridMenuItemsBuilder(SalesProduct, TradeDetailsGrid).AddMenuItems();
			}
		}

		#region Columns

		protected void AddCustomColumns(ZGrid grid)
		{
			if (grid != null && !customColumnsAdded.Contains(grid))
			{
				var builder = new CustomGridPropertiesBuilder(grid);
				builder.Add(
					x => OrgSalesProduct.GetNewCustomGridColumnsBusinessObject(x, customColumns, salesProduct.FormLayout.TradeDetailGridColumnDefinitions),
					OrgSalesProduct.GetCustomGridColumns(null, customColumns, SalesProduct.FormLayout.TradeDetailGridColumnDefinitions));

				customColumnsAdded.Add(grid);
			}
		}
		readonly HashSet<ZGrid> customColumnsAdded = new HashSet<ZGrid>();

		#endregion

		#endregion

		#region Sales Matching

		void EntityTradeDetailsCollection_SalesMatchingPropertyChanged(object sender, EventArgs e)
		{
			var tradeDetail = (EntityTradeDetailWrapper)sender;
			if (tradeDetail != null && salesProduct.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				var salesMatchingOptions = salesProduct.SalesMatchingOptions;
				var allMatchingPropertiesEntered = salesMatchingOptions.TradeDetailPropertiesForMatching.All(property => !tradeDetail.FindPropertyInfo(property.Item1).Value.IsEmpty);
				if (allMatchingPropertiesEntered)
				{
					var org = GetOrgHeader(tradeDetail);
					if (org != null)
					{
						var matching = new SalesMatching(org, null, tradeDetail, salesMatchingOptions);
						if (matching.MatchedSalesCollection.Count > 0)
						{
							var form = new SalesMatchingForm(matching, false);
							form.FormClosed += SalesMatchingForm_FormClosed;
							ZFormModaliser.ShowDialogAndDispose(form);
						}
					}
				}
			}
		}

		OrgHeader GetOrgHeader(EntityTradeDetailWrapper tradeDetail)
		{
			var orgEntity = tradeDetail.Entity as OrgHeader;
			if (orgEntity != null)
			{
				return orgEntity;
			}

			var orgPkInfo = tradeDetail.Entity.OrgPkInfo;
			if (orgPkInfo != null)
			{
				return tradeDetail.Factory.Load<OrgHeader>((ZGuid)orgPkInfo.Value);
			}

			return null;
		}

		void SalesMatchingForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			var form = (SalesMatchingForm)sender;
			if (form != null)
			{
				form.FormClosed -= SalesMatchingForm_FormClosed;
				var grid = TradeDetailsGrid as AutoMatchingTradeLanesGrid;
				if (form.SelectedSalesMatchingData != null && form.SelectedSalesMatchingData.TradeDetail != null && grid != null)
				{
					grid.SetCurrentRowIndexOverride(form.SelectedSalesMatchingData.TradeDetail.PK);
				}
			}
		}

		protected virtual void RebuildGroupingCollection()
		{
		}

		protected void EntityTradeDetailsCollection_DefaultTradeDetailRemovedBySalesMatching(object sender, EventArgs e)
		{
			RebuildGroupingCollection();
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
