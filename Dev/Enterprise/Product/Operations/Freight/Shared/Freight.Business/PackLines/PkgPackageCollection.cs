using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class PkgPackageCollection : ActiveBusinessObjectCollection<PkgPackage>
	{
		public PkgPackageCollection(BusinessObject master)
			: base(master, typeof(JobPackLinePackage), new ZQuery(), JobPackLinePackageSchema.JPP_JL_PackLine, JobPackLinePackageSchema.JPP_KP_Packge)
		{
		}
	}
}
