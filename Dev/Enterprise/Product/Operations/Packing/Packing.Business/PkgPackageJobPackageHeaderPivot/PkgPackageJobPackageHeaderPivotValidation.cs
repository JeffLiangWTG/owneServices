//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageJobPackageHeaderPivotValidation
//
//    This class should be used for overriding validation in AutoPkgPackageJobPackageHeaderPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobPackageHeaderPivotValidation : AutoPkgPackageJobPackageHeaderPivotValidation
	{
		public PkgPackageJobPackageHeaderPivotValidation(AutoPkgPackageJobPackageHeaderPivot parent)
			: base(parent)
		{
		}

		protected override void CheckKPJ_KJ_PackageJob()
		{
			base.CheckKPJ_KJ_PackageJob();
			MandatoryValidation.CheckEntered(Parent.KPJ_KJ_PackageJobInfo);
		}

		protected override void CheckKPJ_KPH_PackageHeader()
		{
			base.CheckKPJ_KPH_PackageHeader();
			MandatoryValidation.CheckEntered(Parent.KPJ_KPH_PackageHeaderInfo);
		}
	}
}
