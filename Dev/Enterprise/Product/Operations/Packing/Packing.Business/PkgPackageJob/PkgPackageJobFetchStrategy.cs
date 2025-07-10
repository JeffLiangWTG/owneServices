using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	class PkgPackageJobFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PkgPackageJobFetchStrategy(PkgPackageJob packageJob)
			: base(packageJob)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob, BusinessObject.PK);
		}
	}
}
