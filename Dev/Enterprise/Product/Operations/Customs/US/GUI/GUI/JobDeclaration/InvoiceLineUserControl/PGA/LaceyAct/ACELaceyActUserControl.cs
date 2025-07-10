using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class ACELaceyActUserControl : ZUserControl
	{
		public ACELaceyActUserControl()
		{
			InitializeComponent();

			PGACommonGrid.AfterBind += new EventHandler(PGACommonGrid_AfterBind);
			new ZGridPGADataCorrectionSupporter(PGACommonGrid).AddPGALineEditMenu();
		}

		void PGACommonGrid_AfterBind(object sender, EventArgs e)
		{
			PGACommonGrid.ListManager.PositionChanged += ListManager_PositionChanged;
			PGACommonGrid.ListManager.ListChanged += ListManager_ListChanged;
			UpdateCurrentLacey();
			SetCountriesGridVisibility();
		}

		internal void RemoveColumns()
		{
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle("PGAValue"));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle("Status"));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle("StatusDesc"));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle("StatusDate"));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle(USPGAAddInfoSchema.Constants.US_InvCurrPGAValue));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle(PGA.Schema.US_TrackingStatusDesc));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle(USPGAAddInfoSchema.Constants.US_PGAContactName));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle(USPGAAddInfoSchema.Constants.US_PGAContactEmail));
			PGACommonGrid.ColumnStyles.Remove(PGACommonGrid.GetColumnStyle(USPGAAddInfoSchema.Constants.US_PGAContactPhoneNo));
			ConstituentElementsGrid.ColumnStyles.Remove(ConstituentElementsGrid.GetColumnStyle(USConstituentElementAddInfoSchema.Constants.US_PGAQuantityOfConstituentElement));
		}

		void ListManager_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			SetCountriesGridVisibility();
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			UpdateCurrentLacey();
			SetCountriesGridVisibility();
		}

		void SetCountriesGridVisibility()
		{
			var currentLacey = GetCurrentLacey();
			CountriesGroupBox.Visible = currentLacey != null && currentLacey.US_UnknownBreakdownTotal;
		}

		void UpdateCurrentLacey()
		{
			var currentLacey = GetCurrentLacey();
			if (currentLacey != previousLacey && currentLacey != null)
			{
				currentLacey.US_UnknownBreakdownTotalInfo.ValueChanged -= US_UnknownBreakdownTotalInfo_ValueChanged;
				currentLacey.US_UnknownBreakdownTotalInfo.ValueChanged += US_UnknownBreakdownTotalInfo_ValueChanged;
				previousLacey = currentLacey;
			}
		}
		PGA previousLacey;

		PGA GetCurrentLacey()
		{
			var listManager = PGACommonGrid.ListManager;
			return listManager != null ? (PGA)listManager.GetCurrent() : null;
		}

		void US_UnknownBreakdownTotalInfo_ValueChanged(object sender, EventArgs e)
		{
			SetCountriesGridVisibility();
		}
	}
}
