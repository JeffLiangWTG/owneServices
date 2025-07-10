using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSChargeCodeGroupPivotCollection : DependentBusinessObjectCollection<AccPOSChargeCodeGroupPivot, AccPOSChargeCodeGroup>
	{
		public AccPOSChargeCodeGroupPivotCollection(AccPOSChargeCodeGroup master) : base(master)
		{ }

		protected override string FkColumnName => AccPOSChargeCodeGroupPivotViewSchema.Constants.GRP_GRO_Group;
	}
}
