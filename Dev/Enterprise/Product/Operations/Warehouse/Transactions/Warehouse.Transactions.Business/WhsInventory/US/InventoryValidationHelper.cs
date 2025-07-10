using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsInventoryValidation
	class InventoryValidationHelper : WhsValidationHelperUS<WhsInventoryView>
	{
		public InventoryValidationHelper(WhsInventoryView inventory)
			: base(inventory)
		{
		}

		protected override ZGuid ClientPK => LineAttributes.WI_OH_Client;

		protected override ZGuid ProductPK => LineAttributes.WI_OP;

		protected override ZGuid LocationPK => LineAttributes.WI_WL;

		protected override ZDecimal Units => LineAttributes.WI_InDocketLineUnits;

		protected override ZDecimal PerPackageQty => LineAttributes.PerPackageQty;

		protected override ZString PackageGroupID => LineAttributes.PackageGroupId;

		protected override IValidateParentWithLines ProductPackageTotalsParent => LineAttributes.Docket as WhsReceive;

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage => Res.GetString("e3352cbf-f847-430a-88e0-e4b8c9dfd8a3", "Units received must be divisible by Per Group Quantity.");

		protected override WhsValidationHelperUS<WhsInventoryView> GetHelper(WhsInventoryView inventory)
		{
			return new InventoryValidationHelper(inventory);
		}

		protected override WhsInventoryView[] GetSiblings()
		{
			var receive = LineAttributes.Docket as WhsReceive;
			return receive != null ? receive.Inventory.Cast<WhsInventoryView>().ToArray() : base.GetSiblings();
		}
	}
}
