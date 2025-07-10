//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusMiscRequestLineValidation
//
//    This class should be used for overriding validation in AutoCusMiscRequestLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusMiscRequestLineValidation : AutoCusMiscRequestLineValidation
	{
		public CusMiscRequestLineValidation(AutoCusMiscRequestLine parent) : base(parent)
		{
		}
	}
}
