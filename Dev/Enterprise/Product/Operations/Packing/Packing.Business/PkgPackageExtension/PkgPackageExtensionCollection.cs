using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageExtensionCollection : ActiveBusinessObjectCollection<PkgPackageExtension>
	{
		public PkgPackageExtensionCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(PkgPackageExtensionSchema.KPN_IsActive, true), PkgPackageExtensionSchema.KPN_KP_Package)
		{
		}
	}
}
