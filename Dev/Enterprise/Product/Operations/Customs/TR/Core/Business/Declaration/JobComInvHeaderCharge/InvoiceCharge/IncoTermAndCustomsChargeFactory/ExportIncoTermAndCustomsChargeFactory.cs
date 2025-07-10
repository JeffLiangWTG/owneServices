using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class ExportIncoTermAndCustomsChargeFactory : TRBaseIncoTermAndCustomsChargeFactory
	{
		protected override CustomsChargeCode InternationalFreight => ChargeProvider.InternationalFreight;

		protected override CustomsChargeCode InternationalInsurance => ChargeProvider.InternationalInsurance;
	}
}


