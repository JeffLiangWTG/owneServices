//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSecondaryPartBOMValidation
//
//    This class should be used for overriding validation in AutoOrgSecondaryPartBOMValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMValidation : AutoOrgSecondaryPartBOMValidation
	{
		public OrgSecondaryPartBOMValidation(AutoOrgSecondaryPartBOM parent)
			: base(parent)
		{
		}

		protected override void CheckOSB_OP_SecondaryProduct()
		{
			base.CheckOSB_OP_SecondaryProduct();

			if (!Parent.OSB_OP_SecondaryProductInfo.HasErrors() && Parent.OSB_OP_SecondaryProduct.IsValid)
			{
				var mainProduct = Parent.MainProduct;
				if (Parent.OSB_OP_SecondaryProduct == Parent.OSB_OP_MainProduct)
				{
					Parent.OSB_OP_SecondaryProductInfo.AddError(SecondaryPartCannotBeMainProduct);
				}
				else if (IsProductWithIsPickOnOrderEnabled(mainProduct))
				{
					Parent.OSB_OP_SecondaryProductInfo.AddError(MainProductSetToPickWithWorkOrder);
				}
				else if (mainProduct != null && mainProduct.BillOfMaterials.Count == 0)
				{
					Parent.OSB_OP_SecondaryProductInfo.AddError(MainProductDoesNotHaveBillOfMaterials);
				}
				else if (mainProduct?.SecondaryParts.Any(s => s.PK != Parent.PK && s.OSB_OP_SecondaryProduct == Parent.OSB_OP_SecondaryProduct) ?? false)
				{
					Parent.OSB_OP_SecondaryProductInfo.AddError(SecondaryPartCannotBeAddedTwice);
				}
				else
				{
					var secondaryProduct = Parent.SecondaryProduct;
					if (secondaryProduct != null)
					{
						if (IsProductWithIsPickOnOrderEnabled(secondaryProduct))
						{
							Parent.OSB_OP_SecondaryProductInfo.AddError(SecondaryProductSetToPickWithWorkOrder);
						}
						else if (!OrgPartBOMValidationHelper.DoesProductHaveOwner(mainProduct)
							|| !OrgPartBOMValidationHelper.DoesProductHaveOwner(secondaryProduct)
							|| !OrgPartBOMValidationHelper.DoCommonOwnersExist(mainProduct, secondaryProduct))
						{
							Parent.OSB_OP_SecondaryProductInfo.AddError(Res.GetString("1d999ec5-7cb4-4753-a27c-3dbd44f5a951", "Both the Main Product and Secondary Part should have at least one owner and one common owner."));
						}
					}
				}
			}
		}

		public static bool IsProductWithIsPickOnOrderEnabled(OrgSupplierPart product)
		{
			return product != null && (product.OP_IsComponentPickedOnSalesOrder
				|| OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(product));
		}

		protected override void CheckOSB_ProductQuantity()
		{
			base.CheckOSB_ProductQuantity();
			MandatoryValidation.CheckNotZero(Parent.OSB_ProductQuantityInfo);
			MandatoryValidation.CheckNotNegative(Parent.OSB_ProductQuantityInfo);
		}

		#region Error messages

		public static string MainProductDoesNotHaveBillOfMaterials => Res.GetString("71949ac7-44a8-42d8-9053-7b5c9b64dc8a", "You cannot add Secondary Parts if Main Product has no Bill of Materials.");

		public static string SecondaryPartHasBillOfMaterials => Res.GetString("a1de5971-565b-4164-8cc9-5dc12227aa10", "You cannot add Secondary Parts if it has Bill of Materials.");

		public static string MainProductSetToPickWithWorkOrder => Res.GetString("697ea395-9b25-402a-99ec-cea9420c2e40", "Secondary part can not be added to this product, because the main product is set to 'Can Pick without Work Order'.");

		public static string SecondaryProductSetToPickWithWorkOrder => Res.GetString("7be2b06c-653e-4261-a267-642daa3e6b4c", "Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.");

		public static string SecondaryPartCannotBeMainProduct => Res.GetString("3000e536-d413-4ad0-a7ce-3d73f2836a24", "Cannot Select the Main Product as a Secondary Product for BOM.");

		public static string SecondaryPartCannotBeAddedTwice => Res.GetString("510a8784-ae77-4a22-bcaa-6fe59ccce3d0", "Cannot Select the same Secondary Product twice.");

		#endregion
	}
}
