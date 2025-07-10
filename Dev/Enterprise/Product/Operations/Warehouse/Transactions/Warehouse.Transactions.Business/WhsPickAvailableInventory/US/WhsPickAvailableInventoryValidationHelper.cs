using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsPickAvailableInventoryValidation
	class WhsPickAvailableInventoryValidationHelper : WhsValidationHelperUS<WhsPickAvailableInventory>
	{
		public WhsPickAvailableInventoryValidationHelper(WhsPickAvailableInventory availableInventory)
			: base(availableInventory)
		{
		}

		protected override ZGuid ClientPK => LineAttributes.Client.PK;

		protected override ZGuid ProductPK => LineAttributes.SupplierPart.PK;

		protected override ZGuid LocationPK => LineAttributes.LocationPK;

		protected override ZDecimal Units => LineAttributes.PickLineQuantity;

		protected override ZDecimal PerPackageQty => LineAttributes.PerPackageQty;

		protected override ZString PackageGroupID => LineAttributes.PackageGroupId;

		protected override IValidateParentWithLines ProductPackageTotalsParent => null;

		protected override string UnitsNotDivisibleByPerPackageQtyErrorMessage => Res.GetString("55C3628F-CBEA-4D07-B29C-74BF1A722EA8", "Units picked must be divisible by Per Group Quantity.");

		protected override WhsValidationHelperUS<WhsPickAvailableInventory> GetHelper(WhsPickAvailableInventory lineAttributes)
		{
			return new WhsPickAvailableInventoryValidationHelper(lineAttributes);
		}
	}
}
