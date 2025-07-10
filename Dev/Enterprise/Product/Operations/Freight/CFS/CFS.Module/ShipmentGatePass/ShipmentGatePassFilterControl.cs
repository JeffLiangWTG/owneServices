using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Freight.CFS.Module
{
	public partial class ShipmentGatePassFilterControl : ZFilterStripControl<ShipmentGatePassFilterStrip>, IGridControl
	{
		readonly System.ComponentModel.Container components;

		public ShipmentGatePassFilterControl()
		{
			InitializeComponent();
			ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentGridColumnsProvider>().AddColumns(this);
		}

		public ShipmentGatePassFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentGridColumnsProvider>().AddColumns(this);
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();

				zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|cb000900-5df3-4255-b30c-1079df31f51d", "House CCN");
				zTextBoxColumnStyleInfo1.ColumnName = "CanadaHouseCCN";
				zTextBoxColumnStyleInfo1.IsVisible = false;

				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
