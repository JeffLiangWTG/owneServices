//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusEntryCPDecValidation
//
//    This class should be used for overriding validation in AutoCusEntryCPDecValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusEntryCPDecValidation : AutoCusEntryCPDecValidation
	{
		public CusEntryCPDecValidation(AutoCusEntryCPDec parent) : base(parent)
		{
		}

		protected override void CheckON_CPDecStartDateIsValidZDateTimeRange()
		{
			// Date range should not be validated
		}

		protected override void CheckON_CPDecEndDateIsValidZDateTimeRange()
		{
			// Date range should not be validated
		}
	}
}
