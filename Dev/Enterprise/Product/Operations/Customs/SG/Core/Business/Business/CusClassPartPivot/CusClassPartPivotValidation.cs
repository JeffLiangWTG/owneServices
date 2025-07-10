namespace Enterprise.Customs.SG.V4.Business
{
	public class CusClassPartPivotValidation : Customs.Business.BaseCusClassPartPivotValidation
	{
		public CusClassPartPivotValidation(CusClassPartPivot parent)
			: base(parent)
		{
		}

		protected override void CheckCI_TariffNum()
		{
			base.CheckCI_TariffNum();
			var pivot = (CusClassPartPivot)Parent;
			TariffValidator.ValidateTariff(pivot.Factory, pivot.Tariff, pivot.CI_TariffNumInfo);
		}
	}
}
