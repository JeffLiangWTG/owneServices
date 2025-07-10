//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusDispositionValidation
//
//    This class should be used for overriding validation in AutoCusDispositionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusDispositionValidation : AutoCusDispositionValidation
	{
		public CusDispositionValidation(AutoCusDisposition parent) : base(parent)
		{
		}
	}
}
