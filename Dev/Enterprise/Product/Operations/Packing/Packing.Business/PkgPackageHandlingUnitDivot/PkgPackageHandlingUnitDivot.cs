using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PkgPackageHandlingUnitDivot : AutoPkgPackageHandlingUnitDivot
	{
		public PkgPackageHandlingUnitDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		public PkgPackage HandlingUnit => Factory.Load<PkgPackage>(KPD_KP_HandlingUnit);

		public PkgPackage Package => GetPackageCore();

		protected virtual PkgPackage GetPackageCore() => Factory.Load<PkgPackage>(KPD_KP_Package);

		#endregion

		#region Properties

		[RelatedBusinessObject(nameof(HandlingUnit))]
		public override ZGuid KPD_KP_HandlingUnit { get => base.KPD_KP_HandlingUnit; }

		[RelatedBusinessObject(nameof(Package))]
		public override ZGuid KPD_KP_Package { get => base.KPD_KP_Package; }

		#endregion
	}
}
