using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHeaderCollection : ActiveBusinessObjectCollection<PkgPackageHeader>
	{
		public PkgPackageHeaderCollection(PkgPackageJob job)
			: base(job, typeof(PkgPackageJobPackageHeaderPivot), null, PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, PkgPackageJobPackageHeaderPivotSchema.KPJ_KPH_PackageHeader)
		{
		}
	}
}
