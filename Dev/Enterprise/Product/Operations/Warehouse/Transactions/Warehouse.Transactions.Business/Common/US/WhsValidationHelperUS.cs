using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsInventoryValidation & US WhsTransferLineValidation && US WhsStocktakeLineValidation && WhsOrderLineValidationUS
	public abstract class WhsValidationHelperUS<T>
		where T : BusinessObject, ILineAttributes
	{
		protected WhsValidationHelperUS(T lineAttributes)
		{
			LineAttributes = lineAttributes;
		}

		protected readonly T LineAttributes;

		protected abstract ZGuid ClientPK { get; }
		protected abstract ZGuid ProductPK { get; }
		protected abstract ZGuid LocationPK { get; }
		protected abstract ZDecimal Units { get; }
		protected abstract ZDecimal PerPackageQty { get; }
		protected abstract ZString PackageGroupID { get; }
		protected abstract IValidateParentWithLines ProductPackageTotalsParent { get; }

		#region CheckUnitsIsDivisibleByPerPackageQty

		public void CheckUnitsIsDivisibleByPerPackageQty(ZPropertyInfo info)
		{
			if (!info.HasErrors()
				&& PerPackageQty > 0m
				&& Units % PerPackageQty != 0m)
			{
				info.AddError(UnitsNotDivisibleByPerPackageQtyErrorMessage);
			}
		}

		protected abstract string UnitsNotDivisibleByPerPackageQtyErrorMessage { get; }

		#endregion

		#region CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered

		public void CheckIfPackageIDIsEnteredPerPackageQtyIsAlsoEntered(ZPropertyInfo perPackageQtyInfo)
		{
			if (!perPackageQtyInfo.HasErrors() && !PackageGroupID.IsEmpty && PerPackageQty == 0m)
			{
				perPackageQtyInfo.AddError(PerPackageQtyMustBeSpecifiedIfPackageGroupIDIsSpecified);
			}
		}

		protected string PerPackageQtyMustBeSpecifiedIfPackageGroupIDIsSpecified => Res.GetString("e7e4770f-4511-4f95-bc63-9f4a1f596161", "Per Group Quantity must be specified if Package Group ID is specified.");

		#endregion

		#region CheckAllSameProductsWithSamePackageGroupIDHaveSameTotalPacks

		public void CheckAllSameProductsWithSamePackageGroupIDHaveSameTotalPacks(ZPropertyInfo unitsInfo, ZPropertyInfo perPackageQtyInfo)
		{
			var productPackageTotalsParent = ProductPackageTotalsParent;
			if (productPackageTotalsParent != null && !unitsInfo.HasErrors() && PerPackageQty > 0m && !PackageGroupID.IsEmpty && ProductPK.IsValid && !perPackageQtyInfo.HasErrors())
			{
				var totalPacksByProductByPackageGroup = productPackageTotalsParent.GetParentProductPackageTotals(GetSiblings().OfType<ICalculateProductPackageTotals>());
				var products = totalPacksByProductByPackageGroup.GetProducts(PackageGroupID);
				foreach (var productPackTotalsPerPackageID in totalPacksByProductByPackageGroup.GetProductPackTotals(PackageGroupID))
				{
					var firstProductPackTotal = productPackTotalsPerPackageID.Values.First();
					if (products.Any(product => !productPackTotalsPerPackageID.ContainsKey(product))
						|| productPackTotalsPerPackageID.Values.Any(packs => packs != firstProductPackTotal))
					{
						unitsInfo.AddError(Res.GetString("f3081698-ce89-4ce2-81d9-34eff694f411", "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity."));
						break;
					}
				}
			}
		}

		#endregion

		#region CheckPerPackageQtyDoesNotHaveMoreDecimalsThanSpecifiedOnProduct

		public void CheckPerPackageQtyDoesNotHaveMoreDecimalsThanSpecifiedOnProduct(ZPropertyInfo perPackageQtyInfo, OrgSupplierPart product)
		{
			if (!perPackageQtyInfo.HasErrors()
				&& PerPackageQty > 0m
				&& product != null
				&& PerPackageQty.DecimalPlaces > product.OP_CountDecimalPlaces)
			{
				perPackageQtyInfo.AddError(Res.GetString("6efb8558-3824-4813-a595-a7a27847d28c",
					"Per Group Quantity must have {0} decimal place(s) as specified on Product '{1}'.", product.OP_CountDecimalPlaces, product.OP_PartNum));
			}
		}

		#endregion

		#region CheckSameProductsWithSamePackageGroupIDHaveSamePerPackageQty

		public void CheckSameProductsWithSamePackageGroupIDHaveSamePerPackageQty(ZPropertyInfo perPackageQtyInfo)
		{
			if (!perPackageQtyInfo.HasErrors() && PerPackageQty > 0m && !PackageGroupID.IsEmpty)
			{
				foreach (var sibling in GetSiblings())
				{
					var helper = GetHelper(sibling);
					if (sibling.PK != LineAttributes.PK
						&& helper.ProductPK == ProductPK
						&& helper.PackageGroupID.EqualsIgnoringCase(PackageGroupID)
						&& helper.PerPackageQty != PerPackageQty
						&& AttributeComparer.CompareWithoutBondedEntryKey(sibling, LineAttributes))
					{
						perPackageQtyInfo.AddError(Res.GetString("31910b20-2b5e-48da-b397-b8365d54660e",
							"Same product with the same Package Group ID must have the same Per Group Quantity."));
						break;
					}
				}
			}
		}

		protected virtual T[] GetSiblings() => Array.Empty<T>();

		protected abstract WhsValidationHelperUS<T> GetHelper(T lineAttributes);

		#endregion
	}
}
