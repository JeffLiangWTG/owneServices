//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoViewPkgPackageJobParentsValidation
//
//    This class should be used for overriding validation in AutoViewPkgPackageJobParentsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class ViewPkgPackageJobParentsValidation : AutoViewPkgPackageJobParentsValidation
	{
		public ViewPkgPackageJobParentsValidation(AutoViewPkgPackageJobParents parent)
			: base(parent)
		{
		}
	}
}
