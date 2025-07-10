using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusConditionCollection : TariffEffectiveDatesRelatedFilteredCollection<RefCusCondition>
	{
		public RefCusConditionCollection(TariffView parentTariff)
			: this(parentTariff, false, false)
		{
		}

		protected RefCusConditionCollection(TariffView parentTariff, bool enableEffectiveDataGrouping, bool enableEffectiveDateFilter)
			: base(parentTariff, RefCusConditionSchema.ZX1_ZZ1_Tariff, enableEffectiveDataGrouping, enableEffectiveDateFilter)
		{
		}

		protected override bool AllowNew => false;
	}
}
