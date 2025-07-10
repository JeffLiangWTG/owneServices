using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CommittingLinesTransferHelperTest : CommittingLinesParentHelperTest<WhsTransferLine>
	{
		#region Implementation

		protected override BusinessObject GetNewParent(TestDataSimpleEnvironment data)
		{
			return Helper.CreateWhsTransfer(data.Org1, data.Whs1);
		}

		protected override WhsTransferLine GetNewLine(BusinessObject parent, OrgSupplierPart part, WhsLocation location)
		{
			return Helper.CreateWhsTransferLine((WhsTransfer)parent, part, 0m, location.ToLocationString(), "");
		}

		protected override CommittingLinesParentHelper<WhsTransferLine> GetNewHelper(BusinessObject parent)
		{
			return new CommittingLinesTransferHelper((WhsTransfer)parent);
		}

		protected override void SetProduct(WhsTransferLine line, OrgSupplierPart differentPart)
		{
			line.WE_OP = differentPart.PK;
		}

		#endregion
	}
}
