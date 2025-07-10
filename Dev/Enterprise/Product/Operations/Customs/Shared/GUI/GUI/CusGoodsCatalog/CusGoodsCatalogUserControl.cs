using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusGoodsCatalogUserControl : ZUserControl
	{
		public CusGoodsCatalogUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				TariffNumFindBox.GetCountryCode = GetCustomsCountryCode;
				TariffNumFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
				TariffNumFindBox.TariffType = UniversalTariffType;
			}
		}

		protected virtual ZString UniversalTariffType => GoodsCatalog?.UniversalTariffType ?? Universal.Constants.TariffTypes.HarmonizedSystem;
		protected virtual ZString GetDataGroupingForUniversalTariff() => GoodsCatalog?.CustomsCountryCode ?? ZString.Empty;
		protected virtual string GetCustomsCountryCode() => this.IsDesignMode() ? null : GoodsCatalog?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected BaseCusGoodsCatalog GoodsCatalog => CurrentDataItem is BaseCusGoodsCatalog goodsCatalog && !goodsCatalog.IsDeleted ? goodsCatalog : null;
	}
}
