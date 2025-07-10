//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAccessorialValidation
//
//    This class should be used for overriding validation in AutoRefAccessorialValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefAccessorialValidation : AutoRefAccessorialValidation
	{
		public RefAccessorialValidation(AutoRefAccessorial parent) : base(parent)
		{
		}
	}
}
