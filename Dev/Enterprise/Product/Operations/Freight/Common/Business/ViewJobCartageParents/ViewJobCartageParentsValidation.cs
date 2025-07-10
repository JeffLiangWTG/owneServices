//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewJobCartageParentsValidation
//
//    This class should be used for overriding validation in AutoViewJobCartageParentsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Common.Business
{
	public class ViewJobCartageParentsValidation : AutoViewJobCartageParentsValidation
	{
		public ViewJobCartageParentsValidation(AutoViewJobCartageParents parent) : base(parent)
		{
		}
	}
}
