using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradedSalesAnalysisControl : ZUserControl
	{
		public TradedSalesAnalysisControl(TradedSalesAnalysis salesAnalysis)
		{
			InitializeComponent();
			SetDataBinding(salesAnalysis, null);

			this.SalesAnalysis = salesAnalysis;

			SetupFilters();
			SetupMainGroupingGrid();
		}

		public readonly TradedSalesAnalysis SalesAnalysis;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetSplitterDistanceToFixMainGroupingGrid();
		}

		#region Filters

		void SetupFilters()
		{
			this.treeControl.ShowJobValueColumn = SalesAnalysis.IncludeJobValue;
			this.treeControl.ProductCode = SalesAnalysis.SalesHeader.SalesProductCode;
			this.treeControl.SetTEUQuantityColumn();
			this.locationGroupingTypeDropEdit.Visible = (SalesAnalysis.LocationGroupingTypeList != null && SalesAnalysis.LocationGroupingTypeList.Count > 0);
			this.SalesAnalysis.IncludeJobValueInfo.ValueChanged += IncludeJobValueInfo_ValueChanged;

			if (SalesAnalysis.SalesHeader.SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				this.revenueNoteLabel.Text = Res.GetString("05f17ad2-e6f8-47ce-b691-ef53f6e46c94", "Note: For \'Orders\' and \'Receipts\', the revenue breakdown per product will only include the amounts explicitly invoiced against that product. Revenue not directly linked to a product will only be included in the parent job revenue total.");
			}
		}

		void IncludeJobValueInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshMainColumns();
			treeControl.ShowJobValueColumn = SalesAnalysis.IncludeJobValue;
		}

		#endregion

		#region MainGroupingGrid

		protected ZGrid MainGroupingGrid
		{
			get { return mainGroupingGrid; }
		}

		void SetupMainGroupingGrid()
		{
			this.SalesAnalysis.MainGroupingChanged += SalesAnalysis_MainGroupingChanged;
		}

		void RefreshMainColumns()
		{
			var mainColumns = GetMainColumns(SalesAnalysis);
			var mainColumnsLookup = new HashSet<string>(mainColumns);
			using (mainGroupingGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				foreach (ZGridColumnInfo column in mainGroupingGrid.ColumnStyles)
				{
					var columnName = column.ColumnName;
					mainGroupingGrid.SetAvailability(mainColumnsLookup.Contains(columnName), columnName);
				}

				mainGroupingGrid.ReOrderColumns(mainColumns);
			}

			SetSplitterDistanceToFixMainGroupingGrid();
		}

		void SetSplitterDistanceToFixMainGroupingGrid()
		{
			var minWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);

			this.splitContainer.SplitterDistance =
				Math.Max(minWidth,
					mainGroupingGrid.RowHeaderWidth
					+ mainGroupingGrid.Columns.Cast<ZGridColumn>().Where(x => !x.IsUnavailable && x.IsVisible).Sum(x => x.ColumnStyle.Width)
					+ CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20));
		}

		public void SetAnalysisPeriod(string analysisPeriod)
		{
			Argument.NotNullOrEmpty(analysisPeriod, nameof(analysisPeriod));
			if (SalesAnalysis.PeriodList.ContainsCode(analysisPeriod))
			{
				SalesAnalysis.Period = analysisPeriod;
			}
		}

		void SalesAnalysis_MainGroupingChanged(object sender, EventArgs e)
		{
			RefreshMainColumns();
		}

		static string[] GetMainColumns(TradedSalesAnalysis salesAnalysis)
		{
			return GetMainColumnsCore(salesAnalysis).Union(GetAdditionalMainColumns(salesAnalysis)).ToArray();
		}

		static IEnumerable<string> GetAdditionalMainColumns(TradedSalesAnalysis salesAnalysis)
		{
			yield return TradePeriodGrouping.Schema.GrossRevenue;
			if (salesAnalysis.IncludeJobValue)
			{
				yield return TradePeriodGrouping.Schema.JobRevenue;
				yield return TradePeriodGrouping.Schema.JobCost;
				yield return TradePeriodGrouping.Schema.JobProfit;
			}
			yield return TradePeriodGrouping.Schema.CurrencyCode;
		}

		static IEnumerable<string> GetMainColumnsCore(TradedSalesAnalysis salesAnalysis)
		{
			switch (salesAnalysis.MainGroupingType)
			{
				case SalesAnalysisMainGroupingTypeList.Codes.Location:
					return GetMainColumnsForLocation(salesAnalysis.LocationGroupingType);

				case SalesAnalysisMainGroupingTypeList.Codes.Service:
					return new[] { TradePeriodGrouping.Schema.Service };

				case SalesAnalysisMainGroupingTypeList.Codes.Mode:
					return new[] { TradePeriodGrouping.Schema.Mode };

				case SalesAnalysisMainGroupingTypeList.Codes.ModeAndType:
					return new[] { TradePeriodGrouping.Schema.Mode, TradePeriodGrouping.Schema.Type };

				case SalesAnalysisMainGroupingTypeList.Codes.Product:
					return new[] { TradePeriodGrouping.Schema.SupplierPartNum, TradePeriodGrouping.Schema.SupplierPartDescription };
			}

			return Array.Empty<string>();
		}

		static IEnumerable<string> GetMainColumnsForLocation(string locationGroupingType)
		{
			switch (locationGroupingType)
			{
				case SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry:
					return new[] { TradePeriodGrouping.Schema.OriginCountry, TradePeriodGrouping.Schema.DestinationCountry };

				case SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry:
					return new[] { TradePeriodGrouping.Schema.DestinationCountry };

				case SalesAnalysisLocationGroupingTypeList.Codes.DestinationState:
					return new[] { TradePeriodGrouping.Schema.DestinationState };

				case SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest:
					return new[] { TradePeriodGrouping.Schema.Origin, TradePeriodGrouping.Schema.Destination };

				case SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry:
					return new[] { TradePeriodGrouping.Schema.OriginCountry };

				case SalesAnalysisLocationGroupingTypeList.Codes.OriginState:
					return new[] { TradePeriodGrouping.Schema.OriginState };

				case SalesAnalysisLocationGroupingTypeList.Codes.StateToState:
					return new[] { TradePeriodGrouping.Schema.OriginState, TradePeriodGrouping.Schema.DestinationState };

				case SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco:
					return new[] { TradePeriodGrouping.Schema.OriginUnloco, TradePeriodGrouping.Schema.DestinationUnloco };

				case SalesAnalysisLocationGroupingTypeList.Codes.Warehouse:
					return new[] { TradePeriodGrouping.Schema.Warehouse, TradePeriodGrouping.Schema.WarehouseCountry };

				case SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry:
					return new[] { TradePeriodGrouping.Schema.WarehouseCountry };

				default:
					return Array.Empty<string>();
			}
		}

		#endregion
	}
}
