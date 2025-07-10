using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class VATApplicabilityViewCollection : TariffEffectiveDatesRelatedFilteredCollection<VATApplicabilityView>
	{
		public VATApplicabilityViewCollection(TariffView parentTariff)
			: this(parentTariff, false, false)
		{
		}

		internal VATApplicabilityViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, VATApplicabilityViewSchema.ZX5_ZZ1_ParentTariffOrNationalCode, enableEffectiveDataGrouping, enableEffectiveDateFilter)
		{
		}

		protected override bool MatchesDataGroupingFilter(TariffView master, string dataGrouping)
		{
			return master.Wrapper.MatchEffectiveDataGrouping(dataGrouping);
		}
	}
}
