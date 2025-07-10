//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSImportEntryLineValidation
//
//    This class should be used for overriding validation in AutoUSImportEntryLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USImportEntryLineValidation : AutoUSImportEntryLineValidation
	{
		public USImportEntryLineValidation(AutoUSImportEntryLine parent)
			: base(parent)
		{
		}
	}
}
