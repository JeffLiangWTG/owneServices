using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingPackageJob : PkgPackageJob
	{
		public ForwardingPackageJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new PackageJob.ForwardingPackageCollection Packages => (PackageJob.ForwardingPackageCollection)base.Packages;

		protected override PkgPackageCollection GetPackageCollectionCore() => new PackageJob.ForwardingPackageCollection(this);

		public new ForwardingPackage[] GetAllPackagesOnJob() => (ForwardingPackage[])base.GetAllPackagesOnJob();

		protected override PkgPackage[] GetAllPackagesOnJobCore() => Factory.Load<ForwardingPackage>(new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, PK));
	}
}
