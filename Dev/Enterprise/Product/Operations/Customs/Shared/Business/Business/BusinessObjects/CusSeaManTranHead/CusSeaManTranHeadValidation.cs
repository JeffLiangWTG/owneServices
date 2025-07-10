//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManTranHeadValidation
//
//    This class should be used for overriding validation in AutoCusSeaManTranHeadValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSeaManTranHeadValidation : AutoCusSeaManTranHeadValidation
	{
		public CusSeaManTranHeadValidation(AutoCusSeaManTranHead parent) : base(parent)
		{
		}
	}
}
