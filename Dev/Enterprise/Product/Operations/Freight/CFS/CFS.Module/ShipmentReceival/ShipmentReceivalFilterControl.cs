using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Filter control for ShipmentReceival.
	/// </summary>
	public partial class ShipmentReceivalFilterControl : ZFilterStripControl<ShipmentReceivalModuleStrip>, IFilterControl
	{
		ZPanel TotalsPanel;
		ZLabel TotalsLabel;

		readonly Container components;

		public ShipmentReceivalFilterControl()
		{
			InitializeComponent();
			ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICFSShipmentModuleColumnsAndFiltersProvider>().AddColumns(this);
		}

		public ShipmentReceivalFilterControl(IBusinessObjectCollection gridCollection, ShipmentReceivalFilterStrip filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(TotalsLabel, new SuppressControlRequiresTextBasherAttribute());
#endif
			ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICFSShipmentModuleColumnsAndFiltersProvider>().AddColumns(this);
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|a65da233-7858-484a-b040-5efb85ea6e4e", "House CCN");
				zTextBoxColumnStyleInfo1.ColumnName = "CanadaHouseCCN";
				zTextBoxColumnStyleInfo1.IsVisible = false;

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|3653d9bd-7c07-43fc-979f-d191da67ffc0", "Load List (Console) CCN", "Comma separated list of related load list CCN numbers.");
				zTextBoxColumnStyleInfo2.ColumnName = "CanadaLoadListCCN";
				zTextBoxColumnStyleInfo2.IsVisible = false;

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|d1dfc997-663e-4551-8232-d4ece9ef5ba5", "Load List (Console) PCN", "Comma separated list of related load list PCN numbers.");
				zTextBoxColumnStyleInfo3.ColumnName = "CanadaLoadListPCN";
				zTextBoxColumnStyleInfo3.IsVisible = false;

				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		void FilteredGrid_Navigate(object sender, NavigateEventArgs ne)
		{
			CalcTotals();
		}

		void FilteredGrid_Click(object sender, EventArgs e)
		{
			CalcTotals();
		}

		void FilteredGrid_KeyDown(object sender, KeyEventArgs e)
		{
			CalcTotals();
		}

		void FilteredGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			CalcTotals();
		}
	}
}
