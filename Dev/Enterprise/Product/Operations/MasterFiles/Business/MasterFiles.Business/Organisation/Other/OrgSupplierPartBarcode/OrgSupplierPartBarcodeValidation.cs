//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSupplierPartBarcodeValidation
//
//    This class should be used for overriding validation in AutoOrgSupplierPartBarcodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartBarcodeValidation : AutoOrgSupplierPartBarcodeValidation
	{
		public OrgSupplierPartBarcodeValidation(AutoOrgSupplierPartBarcode parent) : base(parent)
		{
		}

		#region Parent

		protected new OrgSupplierPartBarcode Parent
		{
			get { return (OrgSupplierPartBarcode)base.Parent; }
		}

		#endregion

		#region PH_Barcode

		protected override void CheckPH_Barcode()
		{
			base.CheckPH_Barcode();

			MandatoryValidation.CheckEntered(Parent.PH_BarcodeInfo);
			CheckProductIsBarcoded(Parent.SupplierPart);

			CheckIfBarcodeExisted(Parent.PH_BarcodeInfo);
			CheckIfTrimIsNeeded(Parent.PH_BarcodeInfo);
		}

		void CheckIfBarcodeExisted(ZPropertyInfo propertyInfo)
		{
			if (!Parent.PH_Barcode.IsEmpty)
			{
				var product = Parent.SupplierPart;
				if (product != null && product.OP_IsActive)
				{
					if (Parent.SupplierPart.PartBarcodes.Cast<OrgSupplierPartBarcode>().Any(b => b.PH_Barcode == Parent.PH_Barcode && b.PK != Parent.PK))
					{
						var errorMsg = Res.GetString("C9236726-7521-4590-BFA4-0B4C950950DF", "Barcode '{0}' has already existed in this product.", Parent.PH_Barcode);
						propertyInfo.AddError(errorMsg);
					}

					CheckBarcodeIsEqualToProductCode(propertyInfo);
					CheckBarcodeIsDuplicated(propertyInfo);
				}
			}
		}

		void CheckBarcodeIsEqualToProductCode(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && Parent.PH_Barcode == Parent.SupplierPart.OP_PartNum)
			{
				propertyInfo.AddError(Res.GetString("ddd0767a-e4da-4ab8-8ca0-8235af4cc8c1", "Barcode cannot be the same as the Product Code."));
			}
		}

		#region CheckBarcodeIsDuplicated

		void CheckBarcodeIsDuplicated(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors())
			{
				var duplicatedProducts = FindProductWithDuplicateBarcodeByOwners();
				if (duplicatedProducts.Any())
				{
					var errorMsg = Res.GetString("3bcbde43-3c47-430f-8ab0-377bf574b579", "Barcode '{0}' is the same as the Product Code or is already being used by Product '{1}' from the same owner.", Parent.PH_Barcode, string.Join(", ", duplicatedProducts.ToArray()));
					propertyInfo.AddError(errorMsg);
				}
			}
		}

		IEnumerable<ZString> FindProductWithDuplicateBarcodeByOwners()
		{
			var supplierPart = Parent.SupplierPart;
			var owners = supplierPart.RelatedOrganisations.Cast<OrgPartRelation>().Where(o => o.IsOwner).Select(o => o.OU_OH);
			return OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Parent.Factory, supplierPart.PK, owners, new[] { Parent.PH_Barcode, supplierPart.OP_PartNum });
		}

		#endregion

		void CheckIfTrimIsNeeded(ZPropertyInfo propertyInfo)
		{
			var propertyValue = (ZString)propertyInfo.Value;
			if (!propertyValue.Equals(propertyValue.Trim()))
			{
				propertyInfo.AddError(OrgSupplierPartBarcodeValidation.ValueHasToBeTrimmed);
			}
		}

		#endregion

		#region PH_F3_NKPackType

		protected override void CheckPH_F3_NKPackType()
		{
			base.CheckPH_F3_NKPackType();
			var parent = Parent;
			var supplierPart = parent?.SupplierPart;
			var packTypeInfo = parent?.PH_F3_NKPackTypeInfo;
			MandatoryValidation.CheckEntered(packTypeInfo);
			ListValidation.ErrorIfInvalidCode(packTypeInfo);
			CheckIfPackTypeIsStockUnit(packTypeInfo, supplierPart);
			CheckIfUseForDocumentsIsSet(packTypeInfo, supplierPart);
			if (!parent.PH_F3_NKPackType.IsEmpty && supplierPart != null)
			{
				if (!OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsConvertible(parent.PH_F3_NKPackType, supplierPart))
				{
					parent.PH_F3_NKPackTypeInfo.AddError(NoUnitConversionWarningMessagePrefix + parent.PH_F3_NKPackType);
				}
				if (OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsVolumeOrWeightAndNotStockUnit(parent.PH_F3_NKPackType, supplierPart))
				{
					parent.PH_F3_NKPackTypeInfo.AddError(NoWeightsOrVolumesSelectedUnlessStockKeepingUnitIsTheSame);
				}
			}
		}

		void CheckProductIsBarcoded(OrgSupplierPart orgSupplierPart)
		{
			if (orgSupplierPart != null && !orgSupplierPart.OP_IsBarcoded)
			{
				Parent.PH_BarcodeInfo.AddError(Res.GetString("cf32e6c6-e0bb-44ea-8d1a-996e161eb7f1", "Non barcoded products should not have barcodes defined."));
			}
		}

		#endregion

		#region UseForDocuments

		protected override void CheckPH_UseForDocuments()
		{
			base.CheckPH_UseForDocuments();
			var part = Parent?.SupplierPart;
			var useForDocumentsInfo = Parent?.PH_UseForDocumentsInfo;
			CheckIfPackTypeIsStockUnit(useForDocumentsInfo, part);
			CheckIfUseForDocumentsIsSet(useForDocumentsInfo, part);
		}

		void CheckIfPackTypeIsStockUnit(ZPropertyInfo propertyInfo, OrgSupplierPart part)
		{
			if(!OrgSupplierPartBarcodeValidationHelper.CheckIfPackTypeIsStockUnit(Parent.PH_F3_NKPackType, Parent.PH_UseForDocuments, part))
			{
				propertyInfo.AddError(PackTypeIsNotStockUnitFlagCanNotBeSet);
			}
		}

		void CheckIfUseForDocumentsIsSet(ZPropertyInfo propertyInfo, OrgSupplierPart part)
		{
			if (part != null)
			{
				var parent = Parent;
				var stockUnit = part?.OP_StockKeepingUnit ?? ZString.Empty;
				if (parent.PH_F3_NKPackType.EqualsIgnoringCase(stockUnit))
				{
					var useForDocumentsBarcodes = part.PartBarcodes.Where(b => (b.PH_F3_NKPackType.EqualsIgnoringCase(stockUnit)) && b.PH_UseForDocuments).ToArray();
					if ((useForDocumentsBarcodes.Length > 1 && parent.PH_UseForDocuments) || ((useForDocumentsBarcodes.Length == 0) && !parent.PH_UseForDocuments))
					{
						propertyInfo.AddError(PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne);
					}
				}
			}
		}

		#endregion

		#region Messages

		public static string NoUnitConversionWarningMessagePrefix
		{
			get { return Res.GetString("21b5bd3a-6cd6-4561-ad28-65f282071477", "No conversion to Stock Unit from") + " "; }
		}

		public static string ValueHasToBeTrimmed
		{
			get { return Res.GetString("74CE209D-F349-4921-AA4F-743CB1A948FE", "This value cannot begin or end with white-spaces."); }
		}

		public static string NoWeightsOrVolumesSelectedUnlessStockKeepingUnitIsTheSame
		{
			get
			{
				return Res.GetString("0105c3d8-3fc9-411a-83d3-f3af910bf81f",
				"You cannot select a weight or volume Pack Type, unless it matches the Stock Unit. E.g. If the Stock Unit is KG, then you can select Pack Type as KG.");
			}
		}

		public static string PackTypeIsNotStockUnitFlagCanNotBeSet
		{
			get
			{
				return Res.GetString("4FB03813-C94C-4624-BFEF-6B711C3C08CA", "Use for Documents cannot be set for Barcodes where the Pack Type is not the Stock Unit.");
			}
		}

		public static string PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne
		{
			get
			{
				return Res.GetString("8ABDC746-E2D4-41D1-84B6-086125BA3899", "Another barcode for the Stock Unit already exists where Use for Documents is set, only one barcode can have this value set.");
			}
		}

		#endregion
	}
}
