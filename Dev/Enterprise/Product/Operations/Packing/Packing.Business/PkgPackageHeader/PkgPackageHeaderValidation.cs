//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageHeaderValidation
//
//    This class should be used for overriding validation in AutoPkgPackageHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHeaderValidation : AutoPkgPackageHeaderValidation
	{
		public PkgPackageHeaderValidation(AutoPkgPackageHeader parent)
			: base(parent)
		{
		}

		protected override void CheckKPH_PackageID()
		{
			base.CheckKPH_PackageID();
			MandatoryValidation.CheckEntered(Parent.KPH_PackageIDInfo);
		}
	}
}
