using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public partial class ETradePackedItemDetailsUserControl : ZUserControl
	{
		public ETradePackedItemDetailsUserControl()
		{
			InitializeComponent();
			BanderolTariffFindBox.TariffType = TaxCodeList.RelatedMiscCodes.BanderolTariffType;
			BanderolTariffFindBox.GetDataGrouping = () => { return Core.Constants.CountryCodes.Turkey; };
		}
	}
}
