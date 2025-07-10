//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryLineValidation
//
//    This class should be used for overriding validation in AutoCusEntryLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEntryLineValidation : AutoCusEntryLineValidation
	{
		public CusEntryLineValidation(AutoCusEntryLine parent) : base(parent)
		{
		}
	}
}
