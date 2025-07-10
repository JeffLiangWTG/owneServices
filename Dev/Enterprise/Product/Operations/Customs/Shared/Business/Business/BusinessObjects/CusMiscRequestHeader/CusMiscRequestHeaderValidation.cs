//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusMiscRequestHeaderValidation
//
//    This class should be used for overriding validation in AutoCusMiscRequestHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusMiscRequestHeaderValidation : AutoCusMiscRequestHeaderValidation
	{
		public CusMiscRequestHeaderValidation(AutoCusMiscRequestHeader parent) : base(parent)
		{
		}
	}
}
