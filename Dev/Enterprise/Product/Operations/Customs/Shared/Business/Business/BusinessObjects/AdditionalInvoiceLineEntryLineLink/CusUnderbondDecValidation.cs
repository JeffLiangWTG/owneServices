//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusUnderbondDecValidation
//
//    This class should be used for overriding validation in AutoCusUnderbondDecValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusUnderbondDecValidation : AutoCusUnderbondDecValidation
	{
		public CusUnderbondDecValidation(AutoCusUnderbondDec parent) : base(parent)
		{
		}
	}
}
