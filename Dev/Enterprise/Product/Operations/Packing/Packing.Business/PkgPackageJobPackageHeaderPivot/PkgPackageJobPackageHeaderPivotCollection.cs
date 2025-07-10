using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobPackageHeaderPivotCollection : ActiveBusinessObjectCollection<PkgPackageJobPackageHeaderPivot>
	{
		public PkgPackageJobPackageHeaderPivotCollection(PkgPackageJob job)
			: base(job.Factory, job, new ZQuery(), PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob)
		{
		}
	}
}
