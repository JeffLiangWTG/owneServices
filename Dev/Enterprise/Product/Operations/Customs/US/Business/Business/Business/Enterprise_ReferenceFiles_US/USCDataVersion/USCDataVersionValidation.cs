//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCDataVersionValidation
//
//    This class should be used for overriding validation in AutoUSCDataVersionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCDataVersionValidation : AutoUSCDataVersionValidation
	{
		public USCDataVersionValidation(AutoUSCDataVersion parent)
			: base(parent)
		{
		}
	}
}
