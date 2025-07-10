using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class TariffUOMViewCollection : TariffDataGroupingRelatedFilteredCollection<TariffUOMView>
	{
		public TariffUOMViewCollection(TariffView parentTariff, bool enableEffectiveDataGrouping = false)
			: base(parentTariff: parentTariff,
				TariffUOMViewSchema.ZZ8_ZZ1_ParentTariffOrNationalCode,
				enableEffectiveDataGrouping)
		{
		}

		protected override void SetDefaultsForNewElementCore(TariffUOMView newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZZ8_ZZ1_ParentTariffOrNationalCode = parentTariff.PK;
		}
	}
}
