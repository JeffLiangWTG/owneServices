//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefTradeGroupViewValidation
//
//    This class should be used for overriding validation in AutoCusRefTradeGroupViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupViewValidation : AutoCusRefTradeGroupViewValidation
	{
		public CusRefTradeGroupViewValidation(AutoCusRefTradeGroupView parent) : base(parent)
		{
		}

		protected override void CheckZZA_ZZZ_NKDataGrouping()
		{
			if (!Parent.ZZA_IsSystem && Parent.ZZA_ZZZ_NKDataGrouping.Length > 2)
			{
				Parent.ZZA_ZZZ_NKDataGroupingInfo.AddError(Res.GetString("DDE9DF6B-483A-4248-8842-F1B58158D97F", "Country/Region code cannot be longer than 2 characters."));
			}
		}

		protected override void CheckZZA_TradeGroup()
		{
			if (!Parent.ZZA_IsSystem && Parent.ZZA_TradeGroup.Length > 10)
			{
				Parent.ZZA_TradeGroupInfo.AddError(Res.GetString("7FAA8875-302C-4764-B4A7-839E6BC7AE9F", "Non-system trade group cannot be longer than 10 characters."));
			}
		}
	}
}
