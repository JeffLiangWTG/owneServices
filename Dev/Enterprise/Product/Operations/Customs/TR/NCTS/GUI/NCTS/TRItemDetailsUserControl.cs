using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.TR.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class TRItemDetailsUserControl : ItemDetailsUserControl
	{
		public TRItemDetailsUserControl()
		{
			InitializeComponent();
			this.CommodityCodeTariffFindBox.TariffType = NctsDepartureCargoDesc.CommodityCodeType;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			CustomsValueDropEdit.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("5786F6CB-E55C-41A9-9258-A6A59E641E87", "Statistical Value in USD");
		}
	}
}
