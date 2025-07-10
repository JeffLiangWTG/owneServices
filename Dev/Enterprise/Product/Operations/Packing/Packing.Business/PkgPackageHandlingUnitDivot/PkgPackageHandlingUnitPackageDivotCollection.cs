using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHandlingUnitPackageDivotCollection : ActiveBusinessObjectCollection<PkgPackageHandlingUnitDivot>
	{
		public PkgPackageHandlingUnitPackageDivotCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package)
		{
		}
	}
}
