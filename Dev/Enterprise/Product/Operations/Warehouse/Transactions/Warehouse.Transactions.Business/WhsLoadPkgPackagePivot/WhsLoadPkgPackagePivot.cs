using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadPkgPackagePivot : AutoWhsLoadPkgPackagePivot
	{
		public WhsLoadPkgPackagePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public WhsLoad Load => Factory.Load<WhsLoad>(WLP_WLO_Load);
		public PkgPackage Package => Factory.Load<PkgPackage>(WLP_KP_Package);

		[RelatedBusinessObject(nameof(Package))]
		public override ZGuid WLP_KP_Package
		{
			get => base.WLP_KP_Package;
			set => base.WLP_KP_Package = value;
		}

		[RelatedBusinessObject(nameof(Load))]
		public override ZGuid WLP_WLO_Load
		{
			get => base.WLP_WLO_Load;
			set => base.WLP_WLO_Load = value;
		}

		public bool IsLoaded => WLP_LoadedTime.IsValid && !WLP_UnloadedTime.IsValid;
	}
}
