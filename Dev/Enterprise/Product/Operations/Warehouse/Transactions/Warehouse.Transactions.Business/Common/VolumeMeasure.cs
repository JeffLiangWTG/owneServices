using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class VolumeMeasure : UnitOfMeasure
	{
		public VolumeMeasure(ZPropertyInfo totalQuantityInfo, ZString totalUQ)
			: base(totalQuantityInfo, totalUQ)
		{
		}

		public VolumeMeasure(ZString name, ZString totalUQ)
			: base(name, totalUQ)
		{
		}

		public override int DecimalPlacesForRounding => 3;

		protected override ZDecimal GetQuantityFromProduct(OrgSupplierPart part) => part.OP_Cubic;
		protected override ZString GetUQFromProduct(OrgSupplierPart part) => part.OP_CubicUQ;
	}
}
