
using Enterprise.Customs.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class ChargeTypeList : CodeDescriptionPairList
	{
		public ChargeTypeList()
			: base()
		{
			AddPair(CustomsChargeTypeList.Codes.OverseasFreight, CustomsChargeTypeList.Descriptions.OverseasFreight);
			AddPair(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance);
			AddPair(CustomsChargeTypeList.Codes.OtherCharges, CustomsChargeTypeList.Descriptions.OtherCharges);
		}
	}
}
