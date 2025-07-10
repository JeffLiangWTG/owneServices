using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageSealCollection : ActiveBusinessObjectCollection<PkgPackageSeal>
	{
		public PkgPackageSealCollection(PkgPackage master)
			   : base(master.Factory, master, new ZQuery(), PkgPackageSealSchema.KPE_KP_Package)
		{
		}
	}
}
