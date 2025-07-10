//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCAESResponseCodeValidation
//
//    This class should be used for overriding validation in AutoUSCAESResponseCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	public class USCAESResponseCodeValidation : AutoUSCAESResponseCodeValidation
	{
		public USCAESResponseCodeValidation(AutoUSCAESResponseCode parent)
			: base(parent)
		{
		}
	}
}
