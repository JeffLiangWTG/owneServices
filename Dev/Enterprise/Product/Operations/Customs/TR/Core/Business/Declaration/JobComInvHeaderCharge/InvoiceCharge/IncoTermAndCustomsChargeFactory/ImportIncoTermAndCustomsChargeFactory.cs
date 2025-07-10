using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ImportIncoTermAndCustomsChargeFactory : TRBaseIncoTermAndCustomsChargeFactory
	{
		protected override CustomsChargeCode InternationalFreight { get => ChargeProvider.InternationalFreight; }

		protected override CustomsChargeCode InternationalInsurance { get => ChargeProvider.InternationalInsurance; }
	}
}
