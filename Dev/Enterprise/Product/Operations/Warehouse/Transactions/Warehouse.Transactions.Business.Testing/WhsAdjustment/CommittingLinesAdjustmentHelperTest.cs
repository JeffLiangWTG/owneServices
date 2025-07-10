using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CommittingLinesAdjustmentHelperTest : CommittingLinesParentHelperTest<WhsAdjustmentLine>
	{
		#region Implementation

		protected override BusinessObject GetNewParent(TestDataSimpleEnvironment data)
		{
			return Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
		}

		protected override WhsAdjustmentLine GetNewLine(BusinessObject parent, OrgSupplierPart part, WhsLocation location)
		{
			return Helper.CreateWhsAdjustmentLine((WhsAdjustment)parent, part, 0m, location);
		}

		protected override CommittingLinesParentHelper<WhsAdjustmentLine> GetNewHelper(BusinessObject parent)
		{
			return new CommittingLinesAdjustmentHelper((WhsAdjustment)parent);
		}

		protected override void SetProduct(WhsAdjustmentLine line, OrgSupplierPart differentPart)
		{
			line.WE_OP = differentPart.PK;
		}

		#endregion
	}
}
