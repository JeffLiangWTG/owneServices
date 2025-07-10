using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class CusRefTariffLanguageViewCollection : TariffRelatedFilteredCollection<CusRefTariffLanguageView>
	{
		public CusRefTariffLanguageViewCollection(TariffView parentTariff)
				: base(parentTariff, CusRefTariffLanguageViewSchema.ZX7_ZZ1_Tariff)
		{
		}

		protected override void SetDefaultsForNewElementCore(CusRefTariffLanguageView newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.ZX7_ZZ1_Tariff = parentTariff.PK;
		}

		protected override bool AllowNew => true;
	}
}
