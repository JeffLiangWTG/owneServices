using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class LoadListDetailsUserControl : ZUserControl
	{
		public LoadListDetailsUserControl()
		{
			InitializeComponent();

			if (GlbCompany.CurrentCompany.Country.Code != Core.Constants.CountryCodes.Canada)
			{
				ZGridColumnInfo column = this.ShipmentReceivalModuleButtonGrid.InnerGrid.GetColumnStyle("CanadaHouseCCN");
				this.ShipmentReceivalModuleButtonGrid.InnerGrid.ColumnStyles.Remove(column);
			}

			ControlsToBind.AllowOverlap(ShipmentReceivalModuleButtonGrid);
		}

		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				this.NumbersPanel.Visible = true;
			}
			else
			{
				this.NumbersPanel.Visible = false;
			}

			ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentGridColumnsProvider>().AddColumns(this.ShipmentReceivalModuleButtonGrid);
		}
	}
}
