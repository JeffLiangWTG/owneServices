using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	[DefaultDataSourceBindingMember(null)]
	public partial class SalesBreakdownControl : ZUserControl
	{
		public SalesBreakdownControl()
		{
			InitializeComponent();
			salesHeaderControl.Grid.AfterBind += SalesHeaderControlGrid_AfterBind;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			salesHeaderControl.GridRowDoubleClicked += SalesHeaderControl_GridRowDoubleClicked;
			estimateSalesAnalysisControl.DynamicTradeLaneWithDetailsControl.TradeLaneGridRowDoubleClicked += DynamicTradeLaneWithDetailsControl_TradeLaneGridRowDoubleClicked;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ShowAllRevenueInfo.ValueChanged -= ShowAllRevenueInfo_ValueChanged;
				CurrentDataItem.ShowFinancialYearRevenueInfo.ValueChanged -= ShowFinancialYearToDateRevenueInfo_ValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			if (CurrentDataItem != null && CurrentDataItem.Org != null)
			{
				dynamicTradedSalesAnalysisControl.SetViewpointOrg(CurrentDataItem.Org);
				chartViewModel = new SalesAnalysisChartViewModel(CurrentDataItem);
#if !WINZOR
				if (ChartView == null)
				{
					plotView = new OxyplotView
					{
						Dock = DockStyle.Fill,
						Model = chartViewModel.SalesTimeLineChartModel
					};

					ChartViewPanel.Controls.Add(plotView);
					plotView.InvalidatePlot(true);

					ChartViewPanel.Resize += ChartViewPanel_Resize;
					ChartViewPanel_Resize(this, EventArgs.Empty);
				}
#endif
				CurrentDataItem.ShowAllRevenueInfo.ValueChanged += ShowAllRevenueInfo_ValueChanged;
				CurrentDataItem.ShowFinancialYearRevenueInfo.ValueChanged += ShowFinancialYearToDateRevenueInfo_ValueChanged;
				CurrentDataItem.CompanyFilterInfo.ValueChanged += CompanyFilterInfo_ValueChanged;
				CurrentDataItem.RequireRefreshInfo.ValueChanged += RequireRefreshInfo_ValueChanged;
				SetupTradeAnalysisPeriod();
				SetupSalesHeaderGridColumnCaptions();
			}

			RefreshTradedSalesAnalysisTabPageVisibility();
			SetupProspectValueButtons();
		}

		void ShowFinancialYearToDateRevenueInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupTradeAnalysisPeriod();
			SetupSalesHeaderGridColumnCaptions();
		}

		void SetupTradeAnalysisPeriod()
		{
			if (dynamicTradedSalesAnalysisControl.InnerControl != null)
			{
				if (CurrentDataItem.ShowFinancialYearRevenue)
				{
					dynamicTradedSalesAnalysisControl.InnerControl.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.CurrentFinancialYear);
				}
				else
				{
					dynamicTradedSalesAnalysisControl.InnerControl.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.Trailing12Months);
				}
			}
		}

		void SetupSalesHeaderGridColumnCaptions()
		{
			if (CurrentDataItem.ShowFinancialYearRevenue)
			{
				salesHeaderControl.Grid.SetColumnVisible(true, "CommittedYearToDateValue");
				salesHeaderControl.Grid.SetColumnVisible(true, "CommittedYearToDateTEUQuantity");
				salesHeaderControl.Grid.SetColumnCaption("TradedAnnualTotal", SalesHeaderValueColumnCaptionHelper.TradedCurrentFinancialYearValue);
				salesHeaderControl.Grid.SetColumnCaption("TradedAnnualTEUTotalQuantity", SalesHeaderValueColumnCaptionHelper.TradedCurrentFinancialYearTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("TradedMonthlyAverage", SalesHeaderValueColumnCaptionHelper.TradedCurrentFinancialYearMonthlyValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedYearToDateValue", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearToDateValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedYearToDateTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearToDateTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedCurrentFinancialYearTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastCurrentFinancialYearValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastCurrentFinancialYearTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedNextFinancialYearValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedNextFinancialYearTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNextFinancialYearValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNextFinancialYearTEUQuantity);
			}
			else
			{
				salesHeaderControl.Grid.SetColumnVisible(false, "CommittedYearToDateValue");
				salesHeaderControl.Grid.SetColumnVisible(false, "CommittedYearToDateTEUQuantity");
				salesHeaderControl.Grid.SetColumnCaption("TradedAnnualTotal", SalesHeaderValueColumnCaptionHelper.TradedTrailing12MonthsValue);
				salesHeaderControl.Grid.SetColumnCaption("TradedAnnualTEUTotalQuantity", SalesHeaderValueColumnCaptionHelper.TradedTrailing12MonthsTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("TradedMonthlyAverage", SalesHeaderValueColumnCaptionHelper.TradedTrailing12MonthsMonthlyValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedTrailing12MonthsValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedTrailing12MonthsTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastCurrentYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastTrailing12MonthsValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastCurrentYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastTrailing12MonthsTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedNext12MonthsValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedNext12MonthsTEUQuantity);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastNextYearValue", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNext12MonthsValue);
				salesHeaderControl.Grid.SetColumnCaption("CommittedAndForecastNextYearTEUQuantity", SalesHeaderValueColumnCaptionHelper.CommittedAndForecastNext12MonthsTEUQuantity);
			}
		}

		void SalesHeaderControlGrid_AfterBind(object sender, EventArgs e)
		{
			salesHeaderControl.Grid.ListManager.CurrentItemChanged -= SalesHeaderControlGrid_ListManagerCurrentItemChanged;
			salesHeaderControl.Grid.ListManager.CurrentChanged -= SalesHeaderControlGrid_ListManagerCurrentChanged;
			salesHeaderControl.Grid.ListManager.CurrentChanged += SalesHeaderControlGrid_ListManagerCurrentChanged;
			salesHeaderControl.Grid.ListManager.CurrentItemChanged += SalesHeaderControlGrid_ListManagerCurrentItemChanged;
		}

		#region Charting

		SalesAnalysisChartViewModel chartViewModel;

		protected OxyplotView plotView;
#if !WINZOR
		OxyplotView ChartView
		{
			get
			{
				var container = ChartViewPanel;
				if (container.Controls.Count > 0)
				{
					return (OxyplotView)container.Controls[0];
				}
				else
				{
					return null;
				}
			}
		}
#endif

#endregion

		#region DataBinding

		public new SalesBreakdown CurrentDataItem
		{
			get { return (SalesBreakdown)base.CurrentDataItem; }
		}

		#endregion

		#region Event Handlers

		void SalesHeaderControlGrid_ListManagerCurrentChanged(object sender, EventArgs e)
		{
			RefreshTradedSalesAnalysisTabPageVisibility();
			RefreshChartViewModel();
			SetupTradeAnalysisPeriod();
		}

		void SalesHeaderControlGrid_ListManagerCurrentItemChanged(object sender, EventArgs e)
		{
			SetupTradeAnalysisPeriod();
		}

		void SalesHeaderControlGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			RefreshTradedSalesAnalysisTabPageVisibility();
			RefreshChartViewModel();
		}

		void SalesHeaderControl_GridRowDoubleClicked(object sender, MouseEventArgs e)
		{
			ViewProspectValuesButton_Click(null, EventArgs.Empty);
		}

		void DynamicTradeLaneWithDetailsControl_TradeLaneGridRowDoubleClicked(object sender, EntitySalesEventArgs e)
		{
			ShowProspectiveTradeProfileForm(e.EntitySales);
		}

		void CompanyFilterInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshChartViewModel();
		}

		void ShowAllRevenueInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshChartViewModel();
		}

		void RequireRefreshInfo_ValueChanged(object sender, EventArgs e)
		{
			RefreshChartViewModel();
		}

		void RefreshChartViewModel()
		{
			if (CurrentDataItem != null)
			{
				if (CurrentDataItem.ShowSelectedProductRevenue)
				{
					var salesHeader = salesHeaderControl.Grid.ListManager.GetCurrent() as SalesHeader;
					if (salesHeader != null)
					{
						CurrentDataItem.SelectedProduct = salesHeader.SalesProduct;
					}
				}
				else if (CurrentDataItem.ShowAllRevenue)
				{
					CurrentDataItem.SelectedProduct = null;
				}

				chartViewModel.RefreshChartModel();
			}
		}

		void ChartViewPanel_Resize(object sender, EventArgs e)
		{
#if !WINZOR
			var container = ChartViewPanel;
			var chart = ChartView;
			if (chart != null)
			{
				// elements inside ElementHost are DPI aware. Need to set height to be unscaled height
				var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(container.Height) - 15;
				ControlDpiScalingHelper.SetHeight(ref chart, Math.Max(height, 0), false);
			}
#endif
		}

		#endregion

		#region EstimateSalesAnalysisControl

		internal EstimateSalesAnalysisControl EstimateSalesAnalysisControl
		{
			get { return estimateSalesAnalysisControl; }
		}

		#endregion

		#region TradedSalesAnalysisTabPage

		void RefreshTradedSalesAnalysisTabPageVisibility()
		{
			var salesHeader = (SalesHeader)salesHeaderControl.Grid.ListManager.GetCurrent();
			TradedSalesAnalysisTabPage.TabVisible = salesHeader != null && salesHeader.SalesProduct.IsActualsSupported;
		}

		#endregion

		#region SetupProspectValueButtons

		void SetupProspectValueButtons()
		{
			viewProspectValuesButton.Image = Icons.GetImage(IconTypes.ViewButtonRest);

			if (CurrentDataItem != null)
			{
				viewProspectValuesButton.Enabled = (CurrentDataItem.SalesHeaderCollection.Count > 0);

				if (CurrentDataItem.ReadOnly)
				{
					viewProspectValuesButton.Enabled = false;
				}
			}
		}

		#endregion

		#region ViewProspectValuesButton

		void ViewProspectValuesButton_Click(object sender, EventArgs e)
		{
			OrgSalesProduct salesProduct = null;
			if (CurrentDataItem.SalesHeaderCollection.Count > 0 && salesHeaderControl.Grid.ListManager != null)
			{
				var salesHeader = salesHeaderControl.Grid.ListManager.GetCurrent() as SalesHeader;
				if (salesHeader != null)
				{
					salesProduct = salesHeader.SalesProduct;
				}
			}

			ShowProspectiveTradeProfileForm(salesProduct, false);
		}

		#endregion

		#region ShowProspectiveTradeProfileForm

		void ShowProspectiveTradeProfileForm(OrgSalesProduct salesProduct, bool onNewRow)
		{
			ShowProspectiveTradeProfileForm(form => form.Focus(salesProduct, onNewRow));
		}

		void ShowProspectiveTradeProfileForm(EntitySalesWrapper entitySales)
		{
			ShowProspectiveTradeProfileForm(form => form.Focus(entitySales));
		}

		void ShowProspectiveTradeProfileForm(Action<ProspectiveTradeProfileForm> onFormShownAction)
		{
			if (CurrentDataItem == null || CurrentDataItem.Org == null)
			{
				return;
			}

			var org = CurrentDataItem.Org;
			if (!org.IsInDatabase || org.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("665b97d6-1ec8-49a7-b919-6495a20f912c", "Cannot add or edit estimate values until {0} is saved.", org.HumanReadableName),
					CannotAddOrEditProspectValuesCaption);

				return;
			}

			var newFactory = new BusinessObjectFactory();
			var orgInNewFactory = newFactory.Load<OrgHeader>(org.PK);
			if (orgInNewFactory == null)
			{
				Globals.Message.ShowError(
					Res.GetString("3b15b214-21ba-4666-bb78-2dc4f36b84e8", "{0} has been deleted.", org.HumanReadableName),
					CannotAddOrEditProspectValuesCaption);
				return;
			}

			var form = new ProspectiveTradeProfileForm(orgInNewFactory);
			orgInNewFactory.SetReadOnlyIncludingChildren(true);
			foreach (var salesHeader in orgInNewFactory.ProspectiveSalesHeaderCollection.Cast<SalesHeader>())
			{
				salesHeader.CompanyFilter = CurrentDataItem.CompanyFilter;
			}

			EventHandler onShownHandler = null;
			onShownHandler = (o, e) =>
			{
				onFormShownAction(form);
				form.Shown -= onShownHandler;
			};
			form.Shown += onShownHandler;

			ZFormModaliser.Show(form, ParentForm);
		}

		static string CannotAddOrEditProspectValuesCaption
		{
			get { return Res.GetString("1e759c34-a192-42df-8f33-4bccb413a3cc", "Cannot Add / Edit Estimate Values"); }
		}

		#endregion

		#region Dispose

		protected override void OnHandleDestroyed(EventArgs e)
		{
			if (salesHeaderControl.Grid.ListManager != null)
			{
				salesHeaderControl.Grid.ListManager.CurrentChanged -= SalesHeaderControlGrid_ListManagerCurrentChanged;
				salesHeaderControl.Grid.ListManager.CurrentItemChanged -= SalesHeaderControlGrid_ListManagerCurrentItemChanged;
			}
			ChartViewPanel.Resize -= ChartViewPanel_Resize;
			base.OnHandleDestroyed(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ShowAllRevenueInfo.ValueChanged -= ShowAllRevenueInfo_ValueChanged;
				CurrentDataItem.CompanyFilterInfo.ValueChanged -= CompanyFilterInfo_ValueChanged;
				CurrentDataItem.RequireRefreshInfo.ValueChanged -= RequireRefreshInfo_ValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing && plotView != null)
			{
				plotView.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
