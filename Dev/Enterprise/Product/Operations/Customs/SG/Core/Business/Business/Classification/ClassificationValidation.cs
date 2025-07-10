namespace Enterprise.Customs.SG.V4.Business
{
	public class ClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public ClassificationValidation(Classification parent)
			: base(parent)
		{
		}

		protected override void CheckCC_TariffNum()
		{
			base.CheckCC_TariffNum();

			var parent = (Classification)Parent;
			TariffValidator.ValidateTariff(parent.Factory, parent.Tariff, parent.CC_TariffNumInfo);
		}
	}
}
