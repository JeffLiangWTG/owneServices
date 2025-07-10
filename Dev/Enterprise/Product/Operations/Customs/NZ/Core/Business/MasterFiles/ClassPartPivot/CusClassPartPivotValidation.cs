using Enterprise.Customs.NZ.Business.TariffValidation;

namespace Enterprise.Customs.NZ.Business.MasterFiles
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

			var nzCusClassPartPivot = (CusClassPartPivot)Parent;
			new TariffValidator(nzCusClassPartPivot).CheckMainTariff();
		}
	}
}
