//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCForeignPortValidation
//
//    This class should be used for overriding validation in AutoUSCForeignPortValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCForeignPortValidation : AutoUSCForeignPortValidation
	{
		public USCForeignPortValidation(AutoUSCForeignPort parent)
			: base(parent)
		{
		}
	}
}
