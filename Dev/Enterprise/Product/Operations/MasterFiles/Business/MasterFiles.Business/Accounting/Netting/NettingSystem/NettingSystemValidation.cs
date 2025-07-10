//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingSystemValidation
//
//    This class should be used for overriding validation in AutoNettingSystemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class NettingSystemValidation : AutoNettingSystemValidation
	{
		public NettingSystemValidation(AutoNettingSystem parent) : base(parent)
		{
		}
	}
}
