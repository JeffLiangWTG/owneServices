//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusProcedureAttributeValidation
//
//    This class should be used for overriding validation in AutoRefCusProcedureAttributeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureAttributeValidation : AutoRefCusProcedureAttributeValidation
	{
		public RefCusProcedureAttributeValidation(AutoRefCusProcedureAttribute parent) : base(parent)
		{
		}
	}
}
