//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSupplierPartValidation
//
//    This class should be used for overriding validation in AutoOrgSupplierPartValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierPartValidation : AutoOrgSupplierPartValidation
	{
		public OrgSupplierPartValidation(AutoOrgSupplierPart parent)
			: base(parent)
		{
		}

		#region Parent

		protected new OrgSupplierPart Parent
		{
			get { return (OrgSupplierPart)base.Parent; }
		}

		#endregion

		#region CheckOP_IsActive

		protected override void CheckOP_IsActive()
		{
			base.CheckOP_IsActive();

			if (!Parent.OP_IsActive)
			{
				if (Parent.HasStockOnHandOrInTransit)
				{
					Parent.OP_IsActiveInfo.AddError(CannotDeactivateAProductWithSOHError);

					if (ObjectFactory.Get<Enterprise.Integration.Customs.ZA.IZACustomsRegistry>().IsWarehouseOperatorTransactionsModuleEnabled
						&& Parent.HasActiveOperatorTransaction)
					{
						Parent.OP_IsActiveInfo.AddError(CannotDeactivateAProductWithActiveTransactionAndSOHError);
					}
				}
				else if (Parent.HasAsnLineOnUnfinalisedReceive)
				{
					Parent.OP_IsActiveInfo.AddError(CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError);
				}
			}

			CheckDuplicatedBarcodeWhenActivating(Parent.OP_IsActiveInfo);
		}

		public static string CannotDeactivateAProductWithSOHError
		{
			get { return Res.GetString("93153d6d-d657-4f7a-b3ed-3cfd0e8abaab", "This product cannot be deactivated because it has current stock on hand quantities in the warehouse module. You must remove these quantities first, either using a Warehouse Release or Warehouse Adjustment. Print a Warehouse Stock on Hand report or use the Warehouse Inventory module to find these stock quantities"); }
		}

		public static string CannotDeactivateAProductWithActiveTransactionAndSOHError => Res.GetString("D82E9843-7861-4F94-80CC-C76D81822FF5", "This product cannot be deactivated because it has current stock on hand quantities in the operator warehouse transaction module. You must remove these quantities first by using operator warehouse transaction Orders to reduce the stock on hand.");

		public static string CannotDeactivateAProductWithAsnLineOnUnfinalisedReceiveError => Res.GetString("c0b5590f-c59f-4084-9500-561a48d35c48", "This product cannot be deactivated because there are ASN line(s) on un-finalized receive is referencing the product.");

		void CheckDuplicatedBarcodeWhenActivating(ZPropertyInfo propertyInfo)
		{
			var productCodes = GetProductNumsWithDuplicateBarcode(propertyInfo);
			if (productCodes.Any())
			{
				Parent.OP_IsActiveInfo.AddError(string.Format(Culture.Invariant, CannotActivateAProductWithDuplicatedBarcode, string.Join(", ", productCodes)));
			}
		}

		public static string CannotActivateAProductWithDuplicatedBarcode
		{
			get { return Res.GetString("0B2635CC-EB80-4490-ABD4-148EEE3E6BB2", "This product cannot be activated due to either Product code is already used as barcode or it has a Barcode already been used on Product '{0}'"); }
		}

		#endregion

		#region CheckOP_IsBarcoded

		protected override void CheckOP_IsBarcoded()
		{
			base.CheckOP_IsBarcoded();

			CheckNonBarcodedProductDoesNotHaveAnyBarcodes();
			CheckNonBarcodedProductDoesNotHaveConfirmAttributeSet();
		}

		void CheckNonBarcodedProductDoesNotHaveAnyBarcodes()
		{
			if (!Parent.OP_IsBarcoded && Parent.PartBarcodes.Any())
			{
				Parent.OP_IsBarcodedInfo.AddError(Res.GetString("0feaf2b7-88f5-445c-ae2d-580bd57d5167", "Non barcoded products should not have barcodes defined."));
			}
		}

		void CheckNonBarcodedProductDoesNotHaveConfirmAttributeSet()
		{
			if (!Parent.OP_IsBarcoded)
			{
				var organisationsWithRFConfirmAttributeDefined = Parent.RelatedOrganisations
					.Cast<OrgPartRelation>()
					.Where(ro => ro.OU_PickMode == WhsPickMode.Codes.AttributeSpecified && ro.OU_RFAttributeConfirm != CodeLists.RFAttributeConfirmCode.Codes.None)
					.Select(ro => ro.Header.OH_Code).ToArray();
				if (organisationsWithRFConfirmAttributeDefined.Any())
				{
					Parent.OP_IsBarcodedInfo.AddError(Res.GetString("f9343467-f56d-4fa1-8e6c-f71a8cb09a5e", "Non barcoded products should not have RF Confirm attributes. Confirm attributes are specified for the following clients: {0}.", string.Join(", ", organisationsWithRFConfirmAttributeDefined)));
				}
			}
		}

		#endregion

		#region CheckOP_IsComponentPickedOnSalesOrder

		protected override void CheckOP_IsComponentPickedOnSalesOrder()
		{
			base.CheckOP_IsComponentPickedOnSalesOrder();

			if (Parent.OP_IsComponentPickedOnSalesOrder && Parent.BillOfMaterials.Any(x => x.Component != null && x.Component.BillOfMaterials.Count > 0))
			{
				Parent.OP_IsComponentPickedOnSalesOrderInfo.AddError(CannotSetPickOnSalesOrderIfBOMHasMultiLevelBOMComponent);
			}
		}

#if DEBUG
		public
#endif
		string CannotSetPickOnSalesOrderIfBOMHasMultiLevelBOMComponent
		{
			get { return Res.GetString("08c9a2a8-ced3-4e80-8c01-ec1485feb4af", "This Product cannot be Picked without a Work Order because one or more components has child components."); }
		}

		#endregion

		#region CheckOP_CountDecimalPlaces

		protected override void CheckOP_CountDecimalPlaces()
		{
			base.CheckOP_CountDecimalPlaces();
			CompareValidation.CheckWithinRange(Parent.OP_CountDecimalPlacesInfo, 0, 9);
		}

		#endregion

		#region CheckOP_RX_NKLastWeightedCostCurr

		protected override void CheckOP_RX_NKLastWeightedCostCurr()
		{
			base.CheckOP_RX_NKLastWeightedCostCurr();
			ListValidation.ErrorIfInvalidCode(Parent.OP_RX_NKLastWeightedCostCurrInfo);
		}

		#endregion

		#region CheckOP_RH_NKCommodityCode

		protected override void CheckOP_RH_NKCommodityCode()
		{
			base.CheckOP_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.OP_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region Check OP_StockKeepingUnitPerPallet

		public void ValidateOP_StockKeepingUnitPerPallet()
		{
			ValidateCalculatedProperty(Parent.OP_StockKeepingUnitPerPalletInfo);
		}

		protected void CheckOP_StockKeepingUnitPerPallet()
		{
			if (Parent.OP_StockKeepingUnitPerPallet == 0m)
			{
				Parent.OP_StockKeepingUnitPerPalletInfo.AddWarning(NoPalletSizeWarningMsg);
			}
		}

		public static string NoPalletSizeWarningMsg
		{
			get
			{
				return Res.GetString("59ba1e72-d94e-4478-b4ba-1d57500f5b27"
					, @"The pallet size for this product has not been defined.
This means the warehouse putaway, picking and charge auto-rating functions cannot calculate pallet related information. To define the pallet size, go to the Unit Conversions tab and enter the number of stock keeping units per pallet (PLT)");
			}
		}

		#endregion

		#region CheckOP_PartNum

		protected override void CheckOP_PartNum()
		{
			base.CheckOP_PartNum();
			if (!Parent.OP_PartNum.IsEmpty && Parent.HasChanges)
			{
				var duplicateProductDetector = new DuplicateProductDetectorNoBizO(Parent.PK, Parent.OP_PartNum, Parent.OP_IsActive, DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Parent), DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Parent));
				duplicateProductDetector.Validate();
				if (duplicateProductDetector.HasError)
				{
					Parent.OP_PartNumInfo.AddError(duplicateProductDetector.ErrorMessage);
				}
				else if (duplicateProductDetector.HasWarning)
				{
					Parent.OP_PartNumInfo.AddWarning(duplicateProductDetector.WarningMessage);
				}

				if (!Parent.OP_PartNumInfo.HasErrors() && Parent.OP_IsActive)
				{
					if (Parent.PartBarcodes.Cast<OrgSupplierPartBarcode>().Any(b => b.PH_Barcode.EqualsIgnoringCase(Parent.OP_PartNum)))
					{
						Parent.OP_PartNumInfo.AddError(Res.GetString("38243593-8721-4857-a1eb-658b56a004ee", "Product code cannot be the same to the barcode."));
					}

					CheckPartNumNotEqualToOtherBarcodes(Parent.OP_PartNumInfo);
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(Parent.OP_PartNumInfo);
			}
			if (!Parent.RemoveNonEssentialValidationForBulkTariffUpdate && Parent.RelatedOrganisations.Count == 0)
			{
				Parent.OP_PartNumInfo.AddError(ErrorPartMustBeLinkedToOrganisation);
			}

			CheckIfTrimIsNeeded(Parent.OP_PartNumInfo);
		}

		void CheckIfTrimIsNeeded(ZPropertyInfo propertyInfo)
		{
			var propertyValue = (ZString)propertyInfo.Value;
			if (!propertyValue.Equals(propertyValue.Trim()))
			{
				propertyInfo.AddError(Res.GetString("9A8FFE9F-84D9-4949-9AD4-9FFB00017C1B", "This value cannot begin or end with white-spaces."));
			}
		}

		void CheckPartNumNotEqualToOtherBarcodes(ZPropertyInfo propertyInfo)
		{
			var productCodes = GetProductNumsWithDuplicateBarcode(propertyInfo);
			if (productCodes.Any())
			{
				Parent.OP_PartNumInfo.AddError(string.Format(Culture.Invariant, PartNumCannotEqualToBarcode, string.Join(", ", productCodes)));
			}
		}

		public static string ErrorPartMustBeLinkedToOrganisation
		{
			get { return Res.GetString("36f5b81d-08ad-4ec5-b8da-acc95437f68b", "A Product must be related to at least one Organization. Please enter at least one Organization Reference."); }
		}

		public static string PartNumCannotEqualToBarcode
		{
			get { return Res.GetString("7709717d-5061-4cb4-921c-565a272a5662", "Product code is already used as barcode on Product '{0}'."); }
		}

		#endregion

		#region CheckOP_NetWeight

		protected override void CheckOP_NetWeight()
		{
			base.CheckOP_NetWeight();
			MandatoryValidation.CheckNotNegative(Parent.OP_NetWeightInfo);
		}

		#endregion

		#region CheckOP_Weight

		protected override void CheckOP_Weight()
		{
			base.CheckOP_Weight();
			MandatoryValidation.CheckNotNegative(Parent.OP_WeightInfo);
			ValidateOP_WeightUQ();
		}

		#endregion

		#region CheckOP_Width

		protected override void CheckOP_Width()
		{
			base.CheckOP_Width();
			MandatoryValidation.CheckNotNegative(Parent.OP_WidthInfo);
		}

		#endregion

		#region CheckOP_Height

		protected override void CheckOP_Height()
		{
			base.CheckOP_Height();
			MandatoryValidation.CheckNotNegative(Parent.OP_HeightInfo);
		}

		#endregion

		#region CheckOP_Depth

		protected override void CheckOP_Depth()
		{
			base.CheckOP_Depth();
			MandatoryValidation.CheckNotNegative(Parent.OP_DepthInfo);
		}

		#endregion

		#region CheckOP_WeightUQ

		protected override void CheckOP_WeightUQ()
		{
			base.CheckOP_WeightUQ();
			MandatoryValidation.CheckUnitEntered(Parent.OP_WeightUQInfo, Parent.OP_WeightInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OP_WeightUQInfo);

			CheckForMeaninglessCombinationOfDimensionAndSKU(Parent.OP_WeightUQInfo, Parent.OP_Weight, () => Core.Constants.Weight.ContainsCode(Parent.OP_StockKeepingUnit));
		}

		void CheckForMeaninglessCombinationOfDimensionAndSKU(ZPropertyInfo uqInfo, ZDecimal quantity, Func<bool> isSKUInTheSameDimension)
		{
			var unitInQuestion = uqInfo.Value.ToString();
			if (quantity > 0 && isSKUInTheSameDimension())
			{
				if (uqInfo.Value.ToString() != Parent.OP_StockKeepingUnit)
				{
					uqInfo.AddError(Res.GetString("FCDB7EE5-B00C-4D97-BF40-9101B78BE7B7", "{0} per {1} is not meaningful because they measure the same dimension.", unitInQuestion, Parent.OP_StockKeepingUnit));
				}
				else if (quantity != 1)
				{
					uqInfo.AddError(Res.GetString("12345678-B00C-4D97-BF40-9101B78BE7B7", "This unit is the same as the Stock Keeping Unit, therefore the value can only be 1 or 0."));
				}
			}
			ValidateOP_StockKeepingUnit();
		}

		#endregion

		#region CheckOP_MeasureUQ

		protected override void CheckOP_MeasureUQ()
		{
			base.CheckOP_MeasureUQ();
			if (!Parent.OP_Depth.Equals(0m) && !Parent.OP_Height.Equals(0m) && !Parent.OP_Width.Equals(0m) && Parent.OP_MeasureUQ.IsEmpty)
			{
				Parent.OP_MeasureUQInfo.AddError(Res.GetString("0224ed19-8615-47c1-9865-1174704889f4", "Measurement UQ is required."));
			}
			ListValidation.ErrorIfInvalidCode(Parent.OP_MeasureUQInfo);
		}

		#endregion

		#region CheckOP_CubicUQ

		protected override void CheckOP_CubicUQ()
		{
			base.CheckOP_CubicUQ();
			MandatoryValidation.CheckUnitEntered(Parent.OP_CubicUQInfo, Parent.OP_CubicInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OP_CubicUQInfo);
			CheckForMeaninglessCombinationOfDimensionAndSKU(Parent.OP_CubicUQInfo, Parent.OP_Cubic, () => Core.Constants.Volume.ContainsCode(Parent.OP_StockKeepingUnit));
		}

		#endregion

		#region CheckOP_Cubic

		protected override void CheckOP_Cubic()
		{
			base.CheckOP_Cubic();
			MandatoryValidation.CheckNotNegative(Parent.OP_CubicInfo);
			if (Parent.CubicIsInvalid())
			{
				Parent.OP_CubicInfo.AddWarning(Res.GetString("b0eca054-8cd9-463e-a1e7-cdad1adad0b7", "The Cube does not match the Cubic Calculation of the Dimensions."));
			}
			ValidateOP_CubicUQ();
		}

		#endregion

		#region CheckOP_StockKeepingUnit

		protected override void CheckOP_StockKeepingUnit()
		{
			base.CheckOP_StockKeepingUnit();
			if (!Parent.RemoveNonEssentialValidationForBulkTariffUpdate)
			{
				MandatoryValidation.CheckEntered(Parent.OP_StockKeepingUnitInfo);
				ListValidation.ErrorIfInvalidCode(Parent.OP_StockKeepingUnitInfo);

				if (!Parent.OP_StockKeepingUnitInfo.HasNotifications())
				{
					ValidateForBOMComponentProducts(Parent);
				}
			}
			ValidateOP_WeightUQ();
			ValidateOP_CubicUQ();
		}

		void ValidateForBOMComponentProducts(OrgSupplierPart parent)
		{
			var query = new ZQuery(OrgPartBOMSchema.OE_OP_Component, parent.PK);
			var bomParts = parent.Factory.Load<OrgPartBOM>(query);
			var bomPartWithNoUnitConversion = bomParts.Where(b => !parent.UnitConverter.Convertible(parent.OP_StockKeepingUnit, b.OE_F3_NKPackType));
			if (bomPartWithNoUnitConversion != null)
			{
				foreach (var bomPart in bomPartWithNoUnitConversion)
				{
					parent.OP_StockKeepingUnitInfo.AddError(ResString.GetMultilingualString("148e123f-abdc-400b-ae41-cb55b3d4a8be", "BOM Product '{0}' uses '{1}' as a component product. Make sure a unit conversion exists between {2} and {3}.", bomPart.MainProduct.OP_PartNum, parent.OP_PartNum, parent.OP_StockKeepingUnit, bomPart.OE_F3_NKPackType));
				}
			}
		}

		#endregion

		#region CheckOP_Desc

		protected override void CheckOP_Desc()
		{
			base.CheckOP_Desc();
			if (Parent.OP_Desc.IsEmpty)
			{
				Parent.OP_DescInfo.AddError(Res.GetString("b3e80e22-594c-48f8-b3c5-e15211f5ef51", "Product description is required."));
			}
		}

		#endregion

		#region Custom Label Mandatory Validation

		protected CustomLabelPropertyValidation CustomLabelPropertyValidation
		{
			get { return customLabelPropertyValidation ?? (customLabelPropertyValidation = new CustomLabelPropertyValidation(Parent.Factory)); }
		}
		CustomLabelPropertyValidation customLabelPropertyValidation;

		#region CheckOP_CustomAttrib1-5

		protected override void CheckOP_CustomAttrib1()
		{
			base.ValidateOP_CustomAttrib1();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomAttrib1Info);
		}

		protected override void CheckOP_CustomAttrib2()
		{
			base.ValidateOP_CustomAttrib2();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomAttrib2Info);
		}

		protected override void CheckOP_CustomAttrib3()
		{
			base.ValidateOP_CustomAttrib3();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomAttrib3Info);
		}

		protected override void CheckOP_CustomAttrib4()
		{
			base.ValidateOP_CustomAttrib4();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomAttrib4Info);
		}

		protected override void CheckOP_CustomAttrib5()
		{
			base.ValidateOP_CustomAttrib5();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomAttrib5Info);
		}

		#endregion

		#region CheckOP_CustomDecimal1-5

		protected override void CheckOP_CustomDecimal1()
		{
			base.ValidateOP_CustomDecimal1();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDecimal1Info);
		}

		protected override void CheckOP_CustomDecimal2()
		{
			base.ValidateOP_CustomDecimal2();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDecimal2Info);
		}

		protected override void CheckOP_CustomDecimal3()
		{
			base.ValidateOP_CustomDecimal3();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDecimal3Info);
		}

		protected override void CheckOP_CustomDecimal4()
		{
			base.ValidateOP_CustomDecimal4();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDecimal4Info);
		}

		protected override void CheckOP_CustomDecimal5()
		{
			base.ValidateOP_CustomDecimal5();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDecimal5Info);
		}

		#endregion

		#region CheckOP_CustomDate1-5

		protected override void CheckOP_CustomDate1()
		{
			base.ValidateOP_CustomDate1();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDate1Info);
		}

		protected override void CheckOP_CustomDate2()
		{
			base.ValidateOP_CustomDate2();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDate2Info);
		}

		protected override void CheckOP_CustomDate3()
		{
			base.ValidateOP_CustomDate3();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDate3Info);
		}

		protected override void CheckOP_CustomDate4()
		{
			base.ValidateOP_CustomDate4();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDate4Info);
		}

		protected override void CheckOP_CustomDate5()
		{
			base.ValidateOP_CustomDate5();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_CustomDate5Info);
		}

		#endregion

		#region CheckOP_Department

		protected override void CheckOP_Department()
		{
			base.ValidateOP_Department();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_DepartmentInfo);
		}

		#endregion

		#region CheckOP_Division

		protected override void CheckOP_Division()
		{
			base.ValidateOP_Division();
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_DivisionInfo);
		}

		#endregion

		#region CheckOP_VendorPackQty

		protected override void CheckOP_VendorPackQty()
		{
			base.CheckOP_VendorPackQty();
			CompareValidation.CheckNumberNotNegative(Parent.OP_VendorPackQtyInfo);
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_VendorPackQtyInfo);
		}

		#endregion

		#region CheckOP_OrderMultipleQty

		protected override void CheckOP_OrderMultipleQty()
		{
			base.CheckOP_OrderMultipleQty();
			CompareValidation.CheckNumberNotNegative(Parent.OP_OrderMultipleQtyInfo);
			CustomLabelPropertyValidation.Validate(new OrgSupplierPart.CustomLabelsProvider(Parent), Parent.OP_OrderMultipleQtyInfo);
		}

		#endregion

		#endregion

		#region Implementation

		IEnumerable<ZString> GetProductNumsWithDuplicateBarcode(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && Parent.OP_IsActive)
			{
				var owners = Parent.RelatedOrganisations.Cast<OrgPartRelation>().Where(o => o.IsOwner).Select(o => o.OU_OH);
				if (propertyInfo == Parent.OP_IsActiveInfo)
				{
					var barcodes = Parent.PartBarcodes.Cast<OrgSupplierPartBarcode>().Where(b => !b.PH_Barcode.IsEmpty).Select(b => b.PH_Barcode);
					return barcodes.Any(barcode => barcode.EqualsIgnoringCase(Parent.OP_PartNum))
						? new[] { Parent.OP_PartNum }
						: OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Parent.Factory, Parent.PK, owners, new[] { Parent.OP_PartNum }.Union(barcodes));
				}
				else if (propertyInfo == Parent.OP_PartNumInfo)
				{
					return OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(Parent.Factory, Parent.PK, owners, new[] { Parent.OP_PartNum });
				}
			}

			return Enumerable.Empty<ZString>();
		}

		#endregion
	}
}
