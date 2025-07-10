using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI
{
	public partial class LandCostInputUserControl : ZUserControl
	{
		ZPanel HeaderPanel;
		ZDateEdit LT_DateOfEntryDateEdit;
		internal ZTemplateTabControl MainTabControl;
		internal ZTabPage CostTabPage;
		ZPanel BodyPanel;
		CargoWise.Windows.UI.KSplitter splitter1;
		ZPanel LinePanel;
		ZGroupBox DistributionGroupBox;
		ZCalcEdit LI_ServiceExRateCalcEdit;
		ZCalcFindBox AmountCalcFindBox;
		ZDropEdit LI_DistributCostByDropEdit;
		ZDropEdit LinkedObjectDropEdit;
		ZGroupBox ChargesGroupBox;
		ZDropEdit LI_LandedCostGroupDropEdit;
		ZTextBox LI_ChargeDescriptionTextBox;
		ZGuidFindBox LI_AC_ChargeCodeGuidFindBox;
		ZPanel zPanel1;
		ZTabPage ExchangeRateTabPage;
		ZGrid ExRatesGrid;
		ZDateEdit LT_DateOfProcessingDateEdit;
		internal ZTabPage LinesTabPage;
		ZGrid CostInputGrid;
		internal ZCalcEdit EstimatedDutyPercentCalcEdit;
		internal ZPanel NotePanel;
		internal ZLabel NoteLabel;
		IContainer components;

		internal const decimal DefaultExchangeRate = 0.07345M;

		public LandCostInputUserControl()
		{
			InitializeComponent();

			CostInputGrid.CurrentCellChanged += OnInputGridCurrentCellChanged;
			ExRatesGrid.CurrentCellChanged += OnInputGridCurrentCellChanged;
		}

		public LandedCostHeader LCHeader
		{
			get { return (LandedCostHeader)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (LCHeader != null)
			{
				EstimatedDutyPercentCalcEdit.Visible = LCHeader.IsEstimatedLC;
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			MainTabControl_SelectedIndexChanged(null, null);
		}

		#region Exchange rate edit note
		void OnInputGridCurrentCellChanged(object sender, EventArgs eventArgs)
		{
			var inputGrid = sender as ZGrid;
			if (LCHeader != null && inputGrid != null && inputGrid.CurrentCell.RowNumber > -1 && inputGrid.CurrentCell.ColumnNumber > -1)
			{
				var localCurrency = LCHeader.LocalCurrencyCode;
				var foreignCurrency = ZString.Empty;

				var costInput = inputGrid.ListManager.GetCurrent() as LandCostInput;
				if (costInput != null)
				{
					foreignCurrency = costInput.LI_RX_NKCostCurrency;
				}
				else
				{
					var landedCostingExRate = inputGrid.ListManager.GetCurrent() as LandedCostingExRate;
					if (landedCostingExRate != null)
					{
						foreignCurrency = landedCostingExRate.CurrencyCode;
					}
				}

				if (foreignCurrency.IsEmpty || foreignCurrency == localCurrency)
				{
					FormatExchangeRateNoteMessage(localCurrency, localCurrency, DefaultExchangeRate);
				}
				else
				{
					ZDateTime foundDate;
					var foundExchangeRate =
						CurrencyConverter.GetExchangeRate(
							LCHeader.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, foreignCurrency), out foundDate);

					var exchangeRate = DefaultExchangeRate;
					if (foundDate.IsEmpty)
					{
						foreignCurrency = "USD";
					}
					else
					{
						exchangeRate = foundExchangeRate;
					}

					FormatExchangeRateNoteMessage(localCurrency, foreignCurrency, exchangeRate);
				}
			}
		}

		CurrencyConverter CurrencyConverter
			=> currencyConverter
			?? (currencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(LCHeader.Factory, CurrencyConverterDataProviderWithFixedExRates));
		CurrencyConverter currencyConverter;

		ExchangeRateDataProvider CurrencyConverterDataProviderWithFixedExRates
			=> exchangeRateDataProvider
			?? (exchangeRateDataProvider = new ExchangeRateDataProvider(LCHeader.LocalCurrencyCode, DefaultExchangeRate));
		ExchangeRateDataProvider exchangeRateDataProvider;

		void RemoveInputGridsCellChangedHandlers()
		{
			CostInputGrid.CurrentCellChanged -= OnInputGridCurrentCellChanged;
			ExRatesGrid.CurrentCellChanged -= OnInputGridCurrentCellChanged;
		}

		void ShowOrHideExchangeRateNote(bool show)
		{
			this.NotePanel.Visible = LCHeader != null && !LCHeader.IsReciprocalExRate && show;
		}

		void FormatExchangeRateNoteMessage(ZString localCurrency, ZString foreignCurrency, decimal exchangeRate)
		{
			var noteLabelText = Res.GetString("283E26DE-7E87-46F1-9076-7858F0931D80", "Note that manually entered foreign exchange rates need to be entered as a direct quote");

			if (foreignCurrency != localCurrency)
			{
				var exRateExampleFormat = Res.GetString("23CF3384-330A-4B52-A867-D01EAE065A09", ", i.e. 1 {0} = {1:G29} {2}", localCurrency, exchangeRate, foreignCurrency);
				noteLabelText += exRateExampleFormat;
			}

			this.NoteLabel.Text = noteLabelText;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				if (MainTabControl.SelectedTab == this.LinesTabPage)
				{
					if (LinesTabPage.Controls.Count == 0)
					{
						historiesUserControl = GetNewHistoriesUserControl();
						historiesUserControl.SetBindTo("Histories");
						historiesUserControl.LCHistoryMaster = LCHeader;
						historiesUserControl.SetDataBinding(LCHeader, "");
						historiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
						LinesTabPage.Controls.Add(this.historiesUserControl);
					}
				}
				else if (MainTabControl.SelectedTab == this.CostTabPage)
				{
					OnInputGridCurrentCellChanged(CostInputGrid, null);
				}
				else if (MainTabControl.SelectedTab == this.ExchangeRateTabPage)
				{
					OnInputGridCurrentCellChanged(ExRatesGrid, null);
				}
			}

			ShowOrHideExchangeRateNote(MainTabControl.SelectedTab != LinesTabPage);
		}

		internal LandCostHistoryUserControl historiesUserControl;

		protected virtual LandCostHistoryUserControl GetNewHistoriesUserControl()
		{
			return new LandCostHistoryUserControl();
		}

		#region ExchangeRateDataProvider
		class ExchangeRateDataProvider : ICurrencyConverterDataProviderWithFixedExRates
		{
			public ExchangeRateDataProvider(string currencyCode, decimal exchangeRate)
			{
				FixedExchangeRateCurrencyCode = currencyCode;
				FixedExchangeRate = exchangeRate;
			}

			public string FixedExchangeRateCurrencyCode { get; private set; }
			public decimal FixedExchangeRate { get; private set; }

			#region Not Supported

			public ZDateTime DateOfValuation => ZDate.Today;
			public ExchangeRateType RateType => ExchangeRateType.Customs;
			public int MaximumDaysToFallback => 365;
			public GlbCompany Company => GlbCompany.CurrentCompany;
			public ZString LocalCurrencyCodeOverride => ZString.Empty;
			public ZBool? IsReciprocalOverride => false;

			#endregion
		}
		#endregion

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RemoveInputGridsCellChangedHandlers();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}

