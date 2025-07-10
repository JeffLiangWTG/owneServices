namespace Enterprise.Customs.Universal
{
	public class FilteredTariffAdditionalCodeViewCollection : TariffAdditionalCodeViewCollection
	{
		public FilteredTariffAdditionalCodeViewCollection(TariffView cusTariff, bool enableEffectiveDataGrouping)
			: base(cusTariff, enableEffectiveDataGrouping)
		{
		}
	}
}
