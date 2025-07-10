using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.Universal.GUI
{
	public class TariffColumnStyle : ZBaseFindBoxColumnStyle
	{
		public TariffColumnStyle(TariffColumnStyleInfo info)
			: base(() => new TariffGridFindBox
			{
				GetCountryCode = info.GetCountryCode,
				TariffType = info.TariffType,
				PartialDescriptionMinLengthForSearch = info.PartialDescriptionMinLengthForSearch,
				NomenclatureGroupAdditionalFilter = info.NomenclatureGroupAdditionalFilter,
				TariffAdditionalFilter = info.TariffAdditionalFilter,
				GetEffectiveDate = info.GetEffectiveDate,
				GetTariffType = info.GetTariffType,
				GetDataGrouping = info.GetDataGrouping,
				SelectNomenclatureModes = info.SelectNomenclatureModes,
				GetSelectNomenclatureModes = info.GetSelectNomenclatureModes,
				ShowDescriptionFilterOnNonNomenclatureTariffModule = info.ShowDescriptionFilterOnNonNomenclatureTariffModule,
				NeedLoadParentDataGroup = info.NeedLoadParentDataGroup,
				NeedLoadNomenclatureWhenTariffNotFound = info.NeedLoadNomenclatureWhenTariffNotFound
			}, info)
		{
		}

		public void SetCountryCode(string countryCode)
		{
			((TariffGridFindBox)FindBox).GetCountryCode = () => countryCode;
		}

		public void SetDataGrouping(string dataGrouping)
		{
			((TariffGridFindBox)FindBox).GetDataGrouping = () => dataGrouping;
		}

		public void SetErrorForUnsupportedCountry(ZString error)
		{
			((TariffGridFindBox)FindBox).ErrorForUnsupportedCountry = error;
		}
	}
}
