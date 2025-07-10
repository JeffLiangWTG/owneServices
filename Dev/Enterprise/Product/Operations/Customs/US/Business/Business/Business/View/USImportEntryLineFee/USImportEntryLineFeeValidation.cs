//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSImportEntryLineFeeValidation
//
//    This class should be used for overriding validation in AutoUSImportEntryLineFeeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USImportEntryLineFeeValidation : AutoUSImportEntryLineFeeValidation
	{
		public USImportEntryLineFeeValidation(AutoUSImportEntryLineFee parent)
			: base(parent)
		{
		}
	}
}
