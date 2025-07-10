//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusRefRateCodeViewValidation
//
//    This class should be used for overriding validation in AutoCusRefRateCodeViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Customs.Universal
{
	public class CusRefRateCodeViewValidation : AutoCusRefRateCodeViewValidation
	{
		public CusRefRateCodeViewValidation(AutoCusRefRateCodeView parent) : base(parent)
		{
		}

		protected override void CheckZY1_ZZZ_NKDataGrouping()
		{
			if (!Parent.ZY1_IsSystem && Parent.ZY1_ZZZ_NKDataGrouping.Length != 2)
			{
				Parent.ZY1_ZZZ_NKDataGroupingInfo.AddError(Res.GetString("228F0ABF-6BFF-428E-9AEB-FC4E17F69A4E", "Country/Region code must be 2 characters."));
			}
		}
	}
}
