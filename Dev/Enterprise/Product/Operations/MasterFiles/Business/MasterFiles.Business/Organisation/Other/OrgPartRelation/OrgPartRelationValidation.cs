using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgPartRelationValidation : AutoOrgPartRelationValidation
	{
		public OrgPartRelationValidation(AutoOrgPartRelation parent) : base(parent)
		{
		}

		new OrgPartRelation Parent
		{
			get { return (OrgPartRelation)base.Parent; }
		}

		#region CheckOU_RFAttributeConfirm

		protected override void CheckOU_RFAttributeConfirm()
		{
			base.CheckOU_RFAttributeConfirm();

			if (Parent.OU_RFAttributeConfirm == RFAttributeConfirmCode.Codes.SerialNumber)
			{
				CheckOU_RFAttributeConfirmCore(Parent.OU_UseSerialNumber, Parent.OU_IsSerialNumberReleaseCaptured);
			}
			else if (Parent.OU_PickMode == WhsPickMode.Codes.AttributeNeutral && (Parent.OU_UseSerialNumber || Parent.OU_RFAttributeConfirm != RFAttributeConfirmCode.Codes.None))
			{
				Parent.OU_RFAttributeConfirmInfo.AddError(OrgPartRelation.RFAttributeConfirm_AttributeNeutralError);
			}
			else
			{
				switch (Parent.OU_RFAttributeConfirm)
				{
					case RFAttributeConfirmCode.Codes.PartAttribute1:
						CheckOU_RFAttributeConfirmCore(Parent.OU_UsePartAttrib1, Parent.OU_IsPartAttrib1ReleaseCaptured);
						break;
					case RFAttributeConfirmCode.Codes.PartAttribute2:
						CheckOU_RFAttributeConfirmCore(Parent.OU_UsePartAttrib2, Parent.OU_IsPartAttrib2ReleaseCaptured);
						break;
					case RFAttributeConfirmCode.Codes.PartAttribute3:
						CheckOU_RFAttributeConfirmCore(Parent.OU_UsePartAttrib3, Parent.OU_IsPartAttrib3ReleaseCaptured);
						break;
				}
			}

			CheckOU_RFAttributeConfirmNotSetForNonBarcodedProduct();
		}

		#region CheckOU_RFAttributeConfirmCore

		void CheckOU_RFAttributeConfirmCore(bool isPartAttributeUsed, bool isPartAttributeReleaseCaptured)
		{
			if (isPartAttributeUsed)
			{
				if (isPartAttributeReleaseCaptured)
				{
					Parent.OU_RFAttributeConfirmInfo.AddError(OrgPartRelation.RFAttributeConfirm_ReleaseCapturedAttributeError);
				}
			}
			else
			{
				Parent.OU_RFAttributeConfirmInfo.AddError(OrgPartRelation.RFAttributeConfirmError);
			}
		}

		#endregion

		#region CheckOU_RFAttributeConfirmNotSetForNonBarcodedProduct

		void CheckOU_RFAttributeConfirmNotSetForNonBarcodedProduct()
		{
			if (Parent.SupplierPart is OrgSupplierPart supplierPart && !supplierPart.OP_IsBarcoded &&
				Parent.OU_PickMode == WhsPickMode.Codes.AttributeSpecified && Parent.OU_RFAttributeConfirm != RFAttributeConfirmCode.Codes.None)
			{
				Parent.OU_RFAttributeConfirmInfo.AddError(Res.GetString("2C64277D-FB38-4419-B7D7-F6AFF294AC88", "Non barcoded products should not have RF Confirm attributes."));
			}
		}

		#endregion

		#endregion

		#region CheckOU_RollUpAttributesOnDocuments

		protected override void CheckOU_RollUpAttributesOnDocuments()
		{
			if (Parent.OU_PickMode != WhsPickMode.Codes.AttributeNeutral && Parent.OU_RollUpAttributesOnDocuments)
			{
				Parent.OU_RollUpAttributesOnDocumentsInfo.AddError(Res.GetString("f7ad808d-4896-4882-a6ee-bb9884fafd71", "Roll Up Attributes should be false when Pick Mode is not ANE."));
			}
		}

		#endregion

		#region CheckOU_OH

		protected override void CheckOU_OH()
		{
			base.CheckOU_OH();
			CheckForDuplicates(Parent.OU_OHInfo);
			CheckForDuplicateBarCode(Parent.OU_OHInfo);

			var supplierPart = Parent.SupplierPart;

			if (supplierPart != null)
			{
				supplierPart.Validation.ValidateOP_PartNum();
				supplierPart.MarkAsNeedingValidation();

				if (Parent.OU_OH.IsValid &&
					Parent.IsInDatabase &&
					Parent.OU_OH != (ZGuid)Parent.OU_OHInfo.OriginalValue &&
					(Parent.HasTransactionsIncludingInTransit((ZGuid)Parent.OU_OHInfo.OriginalValue) ||
					Parent.HasAsnLineOnUnfinalisedReceive))
				{
					Parent.OU_OHInfo.AddError(OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);
				}
			}
		}

		#endregion

		#region CheckOU_Relationship

		protected override void CheckOU_Relationship()
		{
			base.CheckOU_Relationship();
			var info = Parent.OU_RelationshipInfo;
			if (Parent.OU_Relationship.IsEmpty)
			{
				Parent.OU_RelationshipInfo.AddError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(info);
				CheckForDuplicates(info);
				CheckForDuplicateBarCode(info);

				if (Parent.OU_Relationship == OrgPartRelation.RelationshipTypes.WarehouseConsignee)
				{
					info.AddWarning(OrgPartRelation.WarehouseConsigneeRelationshipWarning);
				}

				if (Parent.IsInDatabase && !Parent.IsOwner && (Parent.HasTransactionsIncludingInTransit(Parent.OU_OH) || Parent.HasAsnLineOnUnfinalisedReceive))
				{
					info.AddError(OrgPartRelation.CannotChangeReferenceBecauseOfExistingTransactionsError);
				}

				if (Parent.SupplierPart != null)
				{
					CheckForSameSupplierAndOwner(info);
				}
			}
		}

		void CheckForSameSupplierAndOwner(ZPropertyInfo info)
		{
			var allRelationships = ListAllRelationships();
			if (Parent.OU_Relationship == OrgPartRelation.RelationshipTypes.Owner
				&& OrgPartRelationValidationHelper.HasSameSupplierAndOwner(allRelationships, Parent.OU_OH, Parent.OU_Relationship))
			{
				info.AddError(OrgPartRelation.SameOrgAsOwnerAndSupplier);
			}
		}

		#endregion

		#region CheckOU_ClientUQ

		protected override void CheckOU_ClientUQ()
		{
			base.CheckOU_ClientUQ();
			CheckThereIsAValidUnitConverterForClientUQ();
		}

		#endregion

		#region IsPartAttribReleaseCaptured

		#region CheckOU_IsPartAttrib1ReleaseCaptured

		protected override void CheckOU_IsPartAttrib1ReleaseCaptured()
		{
			base.CheckOU_IsPartAttrib1ReleaseCaptured();
			CheckOU_IsPartAttribReleaseCaptured_NotAJulianBatchNumber(Parent.OU_IsPartAttrib1ReleaseCapturedInfo, OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, OrgPartRelationSchema.OU_IsPartAttrib1ReleaseCaptured);
			CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock(Parent.OU_IsPartAttrib1ReleaseCapturedInfo);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_IsPartAttrib1ReleaseCapturedInfo);
		}

		#endregion

		#region CheckOU_IsPartAttrib2ReleaseCaptured

		protected override void CheckOU_IsPartAttrib2ReleaseCaptured()
		{
			base.CheckOU_IsPartAttrib2ReleaseCaptured();
			CheckOU_IsPartAttribReleaseCaptured_NotAJulianBatchNumber(Parent.OU_IsPartAttrib2ReleaseCapturedInfo, OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, OrgPartRelationSchema.OU_IsPartAttrib2ReleaseCaptured);
			CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock(Parent.OU_IsPartAttrib2ReleaseCapturedInfo);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_IsPartAttrib2ReleaseCapturedInfo);
		}

		#endregion

		#region CheckOU_IsPartAttrib3ReleaseCaptured

		protected override void CheckOU_IsPartAttrib3ReleaseCaptured()
		{
			base.CheckOU_IsPartAttrib3ReleaseCaptured();
			CheckOU_IsPartAttribReleaseCaptured_NotAJulianBatchNumber(Parent.OU_IsPartAttrib3ReleaseCapturedInfo, OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, OrgPartRelationSchema.OU_IsPartAttrib3ReleaseCaptured);
			CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock(Parent.OU_IsPartAttrib3ReleaseCapturedInfo);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_IsPartAttrib3ReleaseCapturedInfo);
		}

		#endregion

		#region CheckOU_IsSerialNumberReleaseCaptured

		protected override void CheckOU_IsSerialNumberReleaseCaptured()
		{
			base.CheckOU_IsSerialNumberReleaseCaptured();
			CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock(Parent.OU_IsSerialNumberReleaseCapturedInfo);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_IsSerialNumberReleaseCapturedInfo);
		}

		#endregion

		#region CheckOU_IsPartAttribReleaseCaptured_NotAJulianBatchNumber

		void CheckOU_IsPartAttribReleaseCaptured_NotAJulianBatchNumber(ZPropertyInfo isReleaseCapturedInfo, SchemaStringColumn partAttributeTypeColumn, SchemaBoolColumn usePartAttributeColumn, SchemaBoolColumn isPartAttributeReleaseCapturedColumn)
		{
			if (!isReleaseCapturedInfo.HasErrors() && (ZBool)Parent[usePartAttributeColumn] && (ZBool)Parent[isPartAttributeReleaseCapturedColumn])
			{
				var organisation = Parent.Organisation;
				if (organisation != null && (ZString)organisation.MiscServ[partAttributeTypeColumn] == PartAttributeTypeList.Codes.JulianBatchNumber)
				{
					isReleaseCapturedInfo.AddError(Res.GetString("a3aa7b1b-17dd-4739-b68e-0fe4c1ec0b4a", "Julian Batch Numbers cannot be flagged as Release Capture."));
				}
			}
		}

		#endregion

		#region CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock

		void CheckOU_IsPartAttribReleaseCaptured_NotUsedByExistingStock(ZPropertyInfo isReleaseCapturedInfo)
		{
			if (!isReleaseCapturedInfo.HasErrors() && isReleaseCapturedInfo.HasChanges && Parent.HasCurrentStockIncludingInTransit())
			{
				isReleaseCapturedInfo.AddError(Res.GetString("8e393cde-78b2-46a2-a4df-b8539002f504",
		@"Release Capture cannot be changed because inventory already exists that uses this attribute.
You must first remove this inventory from the warehouse.", isReleaseCapturedInfo.OriginalValue));
			}
		}

		#endregion

		#endregion

		#region CheckOU_IsAttribute_UsedByUnfinalisedASNLines

		void CheckOU_IsAttribute_UsedByUnfinalisedASNLines(ZPropertyInfo attributeInfo)
		{
			if (!attributeInfo.HasErrors() && attributeInfo.HasChanges && Parent.HasAsnLineOnUnfinalisedReceive)
			{
				attributeInfo.AddError(Res.GetString("cc30b5a0-e3e9-44ea-81a6-aaaa8829a731",
		@"Attribute settings cannot be changed because there are ASN line(s) on un-finalized receives for this product.", attributeInfo.OriginalValue));
			}
		}

		#endregion

		#region CheckOU_UsePartAttrib1

		protected override void CheckOU_UsePartAttrib1()
		{
			base.CheckOU_UsePartAttrib1();
			ValidateOU_RFAttributeConfirm();
			new PartAttributeValidation().CheckAttributeDefinitionForPart(Parent.Factory, Parent, 1, Parent.OU_UsePartAttrib1Info, WhsDocketLineSchema.WE_PartAttrib1);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UsePartAttrib1Info);
			Parent.Validation.ValidateOU_UsePartAttrib2();
			Parent.Validation.ValidateOU_UsePartAttrib3();
		}

		#endregion

		#region CheckOU_UsePartAttrib2

		protected override void CheckOU_UsePartAttrib2()
		{
			base.CheckOU_UsePartAttrib2();
			ValidateOU_RFAttributeConfirm();
			new PartAttributeValidation().CheckAttributeDefinitionForPart(Parent.Factory, Parent, 2, Parent.OU_UsePartAttrib2Info, WhsDocketLineSchema.WE_PartAttrib2);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UsePartAttrib2Info);
			Parent.Validation.ValidateOU_UsePartAttrib1();
			Parent.Validation.ValidateOU_UsePartAttrib3();
		}

		#endregion

		#region CheckOU_UsePartAttrib3

		protected override void CheckOU_UsePartAttrib3()
		{
			base.CheckOU_UsePartAttrib3();
			ValidateOU_RFAttributeConfirm();
			new PartAttributeValidation().CheckAttributeDefinitionForPart(Parent.Factory, Parent, 3, Parent.OU_UsePartAttrib3Info, WhsDocketLineSchema.WE_PartAttrib3);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UsePartAttrib3Info);
			Parent.Validation.ValidateOU_UsePartAttrib1();
			Parent.Validation.ValidateOU_UsePartAttrib2();
		}

		#endregion

		#region CheckOU_UseSerialNumber

		protected override void CheckOU_UseSerialNumber()
		{
			base.CheckOU_UseSerialNumber();
			ValidateOU_RFAttributeConfirm();
			ValidateOU_PickMode();
			new PartAttributeValidation().CheckSerialNumberDefinitionForPart(Parent.Factory, Parent, Parent.OU_UseSerialNumberInfo, WhsDocketLineSchema.WE_SerialNumber);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UseSerialNumberInfo);
		}

		#endregion

		#region CheckOU_UseExpiryDate

		protected override void CheckOU_UseExpiryDate()
		{
			base.CheckOU_UseExpiryDate();
			new PartAttributeValidation().CheckExpiryDateDefinitionForPart(Parent.Factory, Parent.Organisation, Parent.SupplierPart, Parent.OU_UseExpiryDateInfo);
			CheckOU_UseExpiryDate_IsUsedIfJulianBatchNumberIsUsed();
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UseExpiryDateInfo);
		}

		void CheckOU_UseExpiryDate_IsUsedIfJulianBatchNumberIsUsed()
		{
			if (!Parent.OU_UseExpiryDateInfo.HasErrors() && !Parent.OU_UseExpiryDate)
			{
				var organisation = Parent.Organisation;
				var miscServ = (organisation != null) ? organisation.MiscServ : null;
				if (miscServ != null &&
					((Parent.OU_UsePartAttrib1 && miscServ.OM_IMPartAttrib1Type == PartAttributeTypeList.Codes.JulianBatchNumber) ||
					(Parent.OU_UsePartAttrib2 && miscServ.OM_IMPartAttrib2Type == PartAttributeTypeList.Codes.JulianBatchNumber) ||
					(Parent.OU_UsePartAttrib3 && miscServ.OM_IMPartAttrib3Type == PartAttributeTypeList.Codes.JulianBatchNumber)))
				{
					Parent.OU_UseExpiryDateInfo.AddError(Res.GetString("6e40d00f-94f8-4d2f-99c9-9055dbf500ff", "Use Expiry Date must be checked on in conjunction with Julian Batch Numbers."));
				}
			}
		}

		#endregion

		#region CheckOU_UsePackingDate

		protected override void CheckOU_UsePackingDate()
		{
			base.CheckOU_UsePackingDate();
			new PartAttributeValidation().CheckPackingDateDefinitionForPart(Parent.Factory, Parent.Organisation, Parent.SupplierPart, Parent.OU_UsePackingDateInfo);
			CheckOU_IsAttribute_UsedByUnfinalisedASNLines(Parent.OU_UsePackingDateInfo);
		}

		#endregion

		#region CheckOU_CompletePalletPicking

		protected override void CheckOU_CompletePalletPicking()
		{
			base.CheckOU_CompletePalletPicking();
			CheckCompletePalletPickingNotEnabledWithRCA();
		}

		void CheckCompletePalletPickingNotEnabledWithRCA()
		{
			if (!Parent.OU_CompletePalletPickingInfo.HasErrors()
				&& Parent.OU_CompletePalletPicking
				&& (Parent.OU_IsPartAttrib1ReleaseCaptured || Parent.OU_IsPartAttrib2ReleaseCaptured || Parent.OU_IsPartAttrib3ReleaseCaptured || Parent.OU_IsSerialNumberReleaseCaptured))
			{
				Parent.OU_CompletePalletPickingInfo.AddError(Res.GetString("4a6f649b-2ee1-4149-967a-6922bcb08701", "Complete Pallet Picking is not supported for products with Release Captured Attributes."));
			}
		}

		#endregion

		#region CheckOU_UnitPrice

		protected override void CheckOU_UnitPrice()
		{
			base.CheckOU_UnitPrice();
			MandatoryValidation.CheckNotNegative(Parent.OU_UnitPriceInfo);
			ValidateOU_RX_NKUnitPriceCurrency();
		}

		#endregion

		#region CheckOU_RX_NKUnitPriceCurrency

		protected override void CheckOU_RX_NKUnitPriceCurrency()
		{
			base.CheckOU_RX_NKUnitPriceCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.OU_RX_NKUnitPriceCurrencyInfo);
			if (Parent.OU_UnitPrice > 0)
			{
				MandatoryValidation.CheckEntered(Parent.OU_RX_NKUnitPriceCurrencyInfo);
			}
		}

		#endregion

		#region CheckOU_WCG_CartonGroup

		protected override void CheckOU_WCG_CartonGroup()
		{
			base.CheckOU_WCG_CartonGroup();
			ListValidation.ErrorIfInvalidPK(Parent.OU_WCG_CartonGroupInfo);

			if (Parent.OU_Relationship != OrgPartRelation.RelationshipTypes.Owner
				&& Parent.OU_Relationship != OrgPartRelation.RelationshipTypes.Both
				&& !Parent.OU_WCG_CartonGroup.IsEmpty)
			{
				Parent.OU_WCG_CartonGroupInfo.AddError(Res.GetString("a5b17736-c968-43f5-ab2b-1ef293248da5", "Carton Groups can only be entered on 'Owner' or 'Both' Part Relationships."));
			}
		}

		#endregion

		#region CheckOU_PickMode

		protected override void CheckOU_PickMode()
		{
			base.CheckOU_PickMode();

			ListValidation.ErrorIfInvalidCode(Parent.OU_PickModeInfo);
			if (!Parent.OU_PickModeInfo.HasErrors())
			{
				MandatoryValidation.CheckEntered(Parent.OU_PickModeInfo);
				if (!Parent.OU_PickModeInfo.HasErrors())
				{
					CheckOU_PickMode_WorksForSerialNumberProductsOnly();
				}
			}
		}

		void CheckOU_PickMode_WorksForSerialNumberProductsOnly()
		{
			if (Parent.OU_PickMode == WhsPickMode.Codes.AttributeNeutral && AtLeastOneAttributeIsUsed && !Parent.OU_UseSerialNumber)
			{
				Parent.OU_PickModeInfo.AddError(OrgPartRelation.PickModeError);
			}
		}

		bool AtLeastOneAttributeIsUsed => (Parent.OU_UsePartAttrib1 || Parent.OU_UsePartAttrib2 || Parent.OU_UsePartAttrib3 || Parent.OU_UseSerialNumber);

		#endregion

		#region CheckOU_JulianBatchNoFormat

		protected override void CheckOU_JulianBatchNoFormat()
		{
			base.CheckOU_JulianBatchNoFormat();
			ListValidation.ErrorIfInvalidCode(Parent.OU_JulianBatchNoFormatInfo);
			CheckOU_JulianBatchNumberFormat_IsEnteredIfJulianBatchNumberIsUsed();
			CheckOU_JulianBatchNumberFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock();
		}

		#region CheckOU_JulianBatchNumberFormat_IsEnteredIfJulianBatchNumberIsUsed

		void CheckOU_JulianBatchNumberFormat_IsEnteredIfJulianBatchNumberIsUsed()
		{
			if (!Parent.OU_JulianBatchNoFormatInfo.HasErrors())
			{
				var organisation = Parent.Organisation;
				var miscServ = (organisation != null) ? organisation.MiscServ : null;
				if (miscServ != null &&
					((Parent.OU_UsePartAttrib1 && miscServ.OM_IMPartAttrib1Type == PartAttributeTypeList.Codes.JulianBatchNumber) ||
					(Parent.OU_UsePartAttrib2 && miscServ.OM_IMPartAttrib2Type == PartAttributeTypeList.Codes.JulianBatchNumber) ||
					(Parent.OU_UsePartAttrib3 && miscServ.OM_IMPartAttrib3Type == PartAttributeTypeList.Codes.JulianBatchNumber)))
				{
					MandatoryValidation.CheckEntered(Parent.OU_JulianBatchNoFormatInfo);
				}
			}
		}

		#endregion

		#region CheckOU_JulianBatchNumberFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock

		void CheckOU_JulianBatchNumberFormat_CannotBeModifiedIfJulianBatchNumberIsUsedAndHasStock()
		{
			var part = Parent.SupplierPart;
			var organisation = Parent.Organisation;
			if (part != null && organisation != null && organisation.PartAttributeManager.IsAJulianBatchNumberAttributeUsed(part) &&
				!Parent.OU_JulianBatchNoFormatInfo.OriginalValue.IsEmpty && !Parent.OU_JulianBatchNoFormatInfo.OriginalValue.Equals(Parent.OU_JulianBatchNoFormatInfo.Value) &&
				Parent.HasCurrentStockIncludingInTransit())
			{
				Parent.OU_JulianBatchNoFormatInfo.AddError(PartAttributeValidation.GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage(Parent.OU_JulianBatchNoFormatInfo.HumanReadableName));
			}
		}

		#endregion

		#endregion

		#region CheckOU_WHC_DefaultInventoryHoldCode

		protected override void CheckOU_WHC_DefaultInventoryHoldCode()
		{
			base.CheckOU_WCG_CartonGroup();
			ListValidation.ErrorIfInvalidPK(Parent.OU_WHC_DefaultInventoryHoldCodeInfo);
		}

		#endregion

		#region CheckForDuplicates

		void CheckForDuplicates(ZPropertyInfo info)
		{
			if (Parent.SupplierPart != null)
			{
				CheckForDuplicateRelation(info);
			}
		}

		void CheckForDuplicateRelation(ZPropertyInfo info)
		{
			var allRelationships = ListAllRelationships(false);
			if (OrgPartRelationValidationHelper.HasDuplicateRelationship(allRelationships, Parent.OU_OH, Parent.OU_Relationship))
			{
				info.AddError(OrgPartRelation.DuplicateRelationshipError);
			}
		}

		void CheckForDuplicateBarCode(ZPropertyInfo info)
		{
			if (!info.HasErrors() && Parent.IsOwner)
			{
				var product = Parent.SupplierPart;
				if (product != null && product.OP_IsActive)
				{
					var barcodes = product.PartBarcodes.Cast<OrgSupplierPartBarcode>().Where(b => !b.PH_Barcode.IsEmpty).Select(b => b.PH_Barcode);
					var productNumsWithDuplicatedBarcodes = OrgPartRelationValidationHelper.GetProductNumbersWithDuplicateBarcode(product.Factory, product.PK, new[] { Parent.OU_OH }, new[] { product.OP_PartNum }.Union(barcodes));
					if (productNumsWithDuplicatedBarcodes.Any())
					{
						info.AddError(Res.GetString("FFEA414E-B0F3-455E-9E58-8C9E0C960600", "The same organization has a non-unique barcode or a barcode same to the Product Code on Product '{0}'.", string.Join(", ", productNumsWithDuplicatedBarcodes)));
					}
				}
			}
		}

		#endregion

		#region CheckThereIsAValidUnitConverterForClientUQ

		void CheckThereIsAValidUnitConverterForClientUQ()
		{
			if (!Parent.OU_ClientUQ.IsEmpty && Parent.SupplierPart is OrgSupplierPart supplierPart && !supplierPart.UnitConverter.Convertible(supplierPart.OP_StockKeepingUnit, Parent.OU_ClientUQ))
			{
				Parent.OU_ClientUQInfo.AddWarning(OrgPartRelation.NoClientUQConverterError);
			}
		}

		#endregion

		#region CheckOU_ConsigneeMinShelfLifeAccepted

		protected override void CheckOU_ConsigneeMinShelfLifeAccepted()
		{
			base.CheckOU_ConsigneeMinShelfLifeAccepted();

			var info = Parent.OU_ConsigneeMinShelfLifeAcceptedInfo;
			MandatoryValidation.CheckNotNegative(info);
			if (!info.HasErrors())
			{
				var errorMessage = OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedGreaterThanMaximumShelfLife(Parent, Parent.OU_ConsigneeMinShelfLifeAccepted);
				if (errorMessage.IsEmpty && Parent.IsInDatabase)
				{
					errorMessage = OrgPartRelationValidationHelper.CheckConsigneeMinShelfLifeAcceptedCannotBeIncreasedIfPickIsInProgress(Parent, Parent.OU_ConsigneeMinShelfLifeAccepted);
				}

				if (!errorMessage.IsEmpty)
				{
					info.AddError(errorMessage);
				}
			}
			OrgMiscServValidation.CheckMinimumShelfLifeRangeWarning(info);
		}

		#endregion

		#region CheckOU_ReceiveOverageTolerancePercent

		protected override void CheckOU_ReceiveOverageTolerancePercent()
		{
			base.CheckOU_ReceiveOverageTolerancePercent();

			CompareValidation.CheckWithinRange(Parent.OU_ReceiveOverageTolerancePercentInfo, 0, 500);
		}

		#endregion

		#region CheckDateFormatStrings

		#region CheckOU_ExpiryDateFormatString

		protected override void CheckOU_ExpiryDateFormatString()
		{
			base.CheckOU_ExpiryDateFormatString();

			if (!Parent.OU_ExpiryDateFormatStringInfo.HasErrors())
			{
				CheckDateFormatCore(Parent.OU_ExpiryDateFormatStringInfo, Parent.OU_ExpiryDateFormatStringInfo.HumanReadableName, Parent.OU_ExpiryDateFormatString);
			}
		}

		#endregion

		#region CheckOU_PackingDateFormatString

		protected override void CheckOU_PackingDateFormatString()
		{
			base.CheckOU_PackingDateFormatString();

			if (!Parent.OU_PackingDateFormatStringInfo.HasErrors())
			{
				CheckDateFormatCore(Parent.OU_PackingDateFormatStringInfo, Parent.OU_PackingDateFormatStringInfo.HumanReadableName, Parent.OU_PackingDateFormatString);
			}
		}

		#endregion

		#region CheckDateFormat

		void CheckDateFormatCore(ZPropertyInfo fieldInfo, string fieldName, ZString dateFormat)
		{
			if (!dateFormat.IsEmpty)
			{
				try
				{
					if (!ValidateAgainstDateUnsafe(dateFormat, new DateTime(2045, 11, 1)))
					{
						fieldInfo.AddError(GetNotEnoughInfoError(fieldName));
					}
					else
					{
						if (!ValidateAgainstDateUnsafe(dateFormat, new DateTime(2045, 11, 1, 16, 23, 54, 235), new DateTime(2045, 11, 1)))
						{
							fieldInfo.AddWarning(GetContainsTimeWarning(fieldName));
						}

						if (!ValidateAgainstDateUnsafe(dateFormat, new DateTime(2045, 11, 12)))
						{
							fieldInfo.AddWarning(GetNoDayWarning(fieldName));
						}

						if (!ValidateAgainstDateUnsafe(dateFormat, new DateTime(2145, 11, 1)))
						{
							fieldInfo.AddWarning(GetY2KWarning(fieldName));
						}
					}
				}
				catch (FormatException)
				{
					fieldInfo.AddError(GetInvalidFormatError(fieldName));
				}
			}

			bool ValidateAgainstDateUnsafe(ZString dateFormat, DateTime date, DateTime? dateToTestAgainst = null)
			{
				if (dateToTestAgainst == null)
				{
					dateToTestAgainst = date;
				}

				var dateString = date.ToString(dateFormat, CultureInfo.InvariantCulture);
				var parsedDate = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
				return parsedDate == dateToTestAgainst;
			}
		}

		#endregion

		#region GetInvalidDateFormatMessages

		static string GetContainsTimeWarning(string fieldName)
		{
			return Res.GetString(
				"D0907FA3-DAFA-4476-8016-D3C23DC1B894",
				"{0} contains time (time data will be ignored)",
				fieldName);
		}

		static string GetNoDayWarning(string fieldName)
		{
			return Res.GetString(
				"7282CDDB-3AD3-4100-A7C9-D0B75E34FE27",
				"{0} has no day (will default to first of month)",
				fieldName);
		}

		static string GetY2KWarning(string fieldName)
		{
			return Res.GetString(
				"992B45C6-319B-4265-B533-E8B83086FC6E",
				"{0} is subject to Y2K-style errors as only 2 digits for year are used",
				fieldName);
		}

		static string GetInvalidFormatError(string fieldName)
		{
			return Res.GetString(
				"0EC52D71-6944-4783-8364-2F5ADC42AECB",
				"{0} uses a format that is invalid",
				fieldName);
		}

		static string GetNotEnoughInfoError(string fieldName)
		{
			return Res.GetString(
				"60EDB9B8-7C38-4ACF-917E-8FC9B1D2679B",
				"{0} does not contain enough information (must have a minimum of month and year, e.g. MM/yyyy)",
				fieldName);
		}

		#endregion

		#endregion

		#region ListAllRelationships

		List<RelatedParty> ListAllRelationships(bool includeSelf = true)
		{
			var allRelationships = new List<RelatedParty>();
			var relationsToUse = Parent.SupplierPart.RelatedOrganisations.OfType<OrgPartRelation>().ToList();
			if (!includeSelf)
			{
				relationsToUse.Remove(Parent);
			}
			foreach (OrgPartRelation ou in relationsToUse)
			{
				allRelationships.Add(new RelatedParty(ou.OU_Relationship, ou.OU_OH));
				ou.MarkAsNeedingValidation();  // For LightValidationTest
			}
			return allRelationships;
		}

		#endregion
	}
}
