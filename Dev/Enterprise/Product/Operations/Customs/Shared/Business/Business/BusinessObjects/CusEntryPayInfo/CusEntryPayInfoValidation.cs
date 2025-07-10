//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryPayInfoValidation
//
//    This class should be used for overriding validation in AutoCusEntryPayInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEntryPayInfoValidation : AutoCusEntryPayInfoValidation
	{
		public CusEntryPayInfoValidation(AutoCusEntryPayInfo parent) : base(parent)
		{
		}
	}
}
