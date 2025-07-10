using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WeightMeasure : UnitOfMeasure
	{
		public WeightMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ)
			: base(totalQuantityInfo, totalUQ)
		{
		}

		public WeightMeasure(ZString name, ZString totalUQ)
			: base(name, totalUQ)
		{
		}

		public override int DecimalPlacesForRounding => 2;

		protected override ZDecimal GetQuantityFromProduct(OrgSupplierPart part) => part.OP_Weight;
		protected override ZString GetUQFromProduct(OrgSupplierPart part) => part.OP_WeightUQ;
	}
}
