using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHandlingUnitHandlingUnitDivotCollection : ActiveBusinessObjectCollection<PkgPackageHandlingUnitDivot>
	{
		public PkgPackageHandlingUnitHandlingUnitDivotCollection(PkgPackage master)
			: base(master.Factory, master, new ZQuery(), PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit)
		{
		}
	}
}
