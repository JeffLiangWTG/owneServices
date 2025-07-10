using Enterprise.Customs.Common;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	partial class TSWIncoTermAndCustomsChargeFactory
	{
		public static CustomsChargeCode RoyaltiesCharge => new CustomsChargeCode(CustomsChargeTypeList.TSWCodes.Royalties, CustomsChargeTypeList.TSWDescriptions.Royalties)
		{
			IsDutiable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncoTermNeutral = true
		};
	}
}
