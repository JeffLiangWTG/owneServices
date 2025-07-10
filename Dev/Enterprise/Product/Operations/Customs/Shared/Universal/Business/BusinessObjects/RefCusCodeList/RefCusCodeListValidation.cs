//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefCusCodeListValidation
//
//    This class should be used for overriding validation in AutoRefCusCodeListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal
{
	public class RefCusCodeListValidation : AutoRefCusCodeListValidation
	{
		public RefCusCodeListValidation(AutoRefCusCodeList parent) : base(parent)
		{
		}
	}
}
