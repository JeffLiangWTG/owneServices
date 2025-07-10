//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusCodeListValidation
//
//    This class should be used for overriding validation in AutoZZRefCusCodeListValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Universal.Internal
{
	public class ZZRefCusCodeListValidation : AutoZZRefCusCodeListValidation
	{
		public ZZRefCusCodeListValidation(AutoZZRefCusCodeList parent)
			: base(parent)
		{
		}
	}
}
