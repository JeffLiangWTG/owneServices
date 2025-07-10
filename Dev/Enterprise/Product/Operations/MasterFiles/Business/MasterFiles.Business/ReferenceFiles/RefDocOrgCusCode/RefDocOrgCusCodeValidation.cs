//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefDocOrgCusCodeValidation
//
//    This class should be used for overriding validation in AutoRefDocOrgCusCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class RefDocOrgCusCodeValidation : AutoRefDocOrgCusCodeValidation
	{
		public RefDocOrgCusCodeValidation(AutoRefDocOrgCusCode parent) : base(parent)
		{
		}
	}
}
