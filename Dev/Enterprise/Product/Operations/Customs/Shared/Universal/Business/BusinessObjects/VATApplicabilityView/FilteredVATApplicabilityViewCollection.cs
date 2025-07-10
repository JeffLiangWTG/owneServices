namespace Enterprise.Customs.Universal
{
	public class FilteredVATApplicabilityViewCollection : VATApplicabilityViewCollection
	{
		public FilteredVATApplicabilityViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping)
			: base(parentTariff, enableEffectiveDataGrouping, true)
		{
		}
	}
}
