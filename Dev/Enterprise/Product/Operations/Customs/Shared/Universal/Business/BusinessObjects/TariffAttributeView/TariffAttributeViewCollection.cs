using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffAttributeViewCollection : TariffRelatedFilteredCollection<TariffAttributeView>
	{
		public TariffAttributeViewCollection(TariffView cusTariff)
			: base(cusTariff, TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode)
		{
		}
	}
}
