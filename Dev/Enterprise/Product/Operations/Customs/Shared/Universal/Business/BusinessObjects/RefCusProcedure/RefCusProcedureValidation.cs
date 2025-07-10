//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProcedureValidation
//
//    This class should be used for overriding validation in AutoRefCusProcedureValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureValidation : AutoRefCusProcedureValidation
	{
		public RefCusProcedureValidation(AutoRefCusProcedure parent) : base(parent)
		{
		}
	}
}
