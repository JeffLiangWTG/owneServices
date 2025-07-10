using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffAdditionalCodeViewCollection : TariffDataGroupingRelatedFilteredCollection<TariffAdditionalCodeView>
	{
		public TariffAdditionalCodeViewCollection(TariffView cusTariff)
			: this(cusTariff, false)
		{
		}

		protected TariffAdditionalCodeViewCollection(TariffView cusTariff, bool enableEffectiveDataGrouping)
			: base(cusTariff, TariffAdditionalCodeViewSchema.ZY2_ZZ1_ParentTariffOrNationalCode, enableEffectiveDataGrouping)
		{
		}
	}
}
