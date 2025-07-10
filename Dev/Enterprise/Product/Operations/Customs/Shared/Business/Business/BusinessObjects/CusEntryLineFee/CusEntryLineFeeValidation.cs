//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryLineFeeValidation
//
//    This class should be used for overriding validation in AutoCusEntryLineFeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEntryLineFeeValidation : AutoCusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
		{
		}
	}
}
