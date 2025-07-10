using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageScreeningCollection : ActiveBusinessObjectCollection<PkgPackageScreening>
	{
		public PkgPackageScreeningCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(), PkgPackageScreeningSchema.KPS_KP_Package) { }
	}
}
