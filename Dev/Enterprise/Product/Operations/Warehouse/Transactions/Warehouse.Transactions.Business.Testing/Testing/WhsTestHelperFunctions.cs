using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine.Shared;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	//  IMPORTANT: CHANGING THE DATA SETUPS IN THESE HELPERS IS EXTREMELY LIKELY TO INVALIDATE DEPENDANT AUTO TESTS
	//  DO NOT CHANGE THESE HELPERS WITHOUT DOING A FULL ANALYSIS OF ALL DEPENDANT TESTS.

	#region AttributeNumber

	public enum AttributeNumber
	{
		One,
		Two,
		Three,
		Serial,
		ExpiryDate,
		PackingDate
	}

	#endregion

	#region WhsTestHelperFunctions

	public class WhsTestHelperFunctions : WhsTestHelperFunctionsEnv, IWhsTransactionTestHelper
	{
		public WhsTestHelperFunctions(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Client

		public virtual OrgHeader CreateOrLoadClientInSeperateFactory(string code, string name, BusinessObjectFactory factory)
		{
			OrgHeader org = OrgHeader.LoadFromCode(factory, code);

			if (org == null)
			{
				org = factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = name;
				org.OH_Code = code;
			}
			return org;
		}

		public OrgCustomLabels CreateCustomLabel(OrgHeader org, string fieldName, string caption, bool isMandatory)
		{
			OrgCustomLabels label = org.CustomLabels.AddNew();
			label.OT_OH = org.PK;
			label.OT_FieldName = fieldName;
			label.OT_Caption = caption;
			label.OT_Type = OrgConstants.CustomLabelType.Form;
			label.OT_IsMandatory = isMandatory;
			return label;
		}

		public void SetAllCustomLabels(OrgHeader org, CustomLabelInfoList customLabels, bool isMandatory)
		{
			foreach (CustomLabelInfoBase label in customLabels)
			{
				CreateCustomLabel(org, label.PropertyName, label.Caption, isMandatory);
			}
		}

		#endregion

		#region Product

		#region CreateProductCategory

		public OrgPartCategory CreateProductCategory(ZString categoryCode, ZString categoryDescription, OrgPartCategory parentCategory = null)
		{
			var category = Factory.New<OrgPartCategory>();
			category.OPC_CategoryCode = categoryCode;
			category.OPC_CategoryDescription = categoryDescription;
			if (parentCategory != null)
			{
				category.OPC_OPC_Parent = parentCategory.PK;
			}
			return category;
		}

		public OrgPartCategory CreateProductCategory(OrgHeader owner, OrgSupplierPart part, ZString categoryCode)
		{
			var category = Factory.New<OrgPartCategory>();
			category.OPC_CategoryCode = categoryCode;
			if (owner != null && part != null)
			{
				var relationship = CreateProductClientRelationShip(owner, part);
				relationship.OU_OPC_Category = category.PK;
			}
			return category;
		}

		#endregion

		#region CreateProductPickFace

		public WhsPickFace CreateProductPickFace(WhsProduct product, OrgHeader org, WhsWarehouse whs, string location)
		{
			return CreateProductPickFace(product, org, whs.FindLocation(location));
		}

		public WhsPickFace CreateProductPickFace(WhsProduct product, OrgHeader org, WhsLocation location)
		{
			return CreateProductPickFace(product, org, location, 0, 1);
		}

		public WhsPickFace CreateProductPickFace(WhsProduct product, OrgHeader org, WhsLocation location, ZDecimal replenishMin, ZDecimal replenishMax, decimal replenishMultiple = 1m)
		{
			return CreateProductPickFace(product.Parent, org, location, replenishMin, replenishMax, replenishMultiple);
		}

		#endregion

		#region CreateProductBOM

		public OrgPartBOM CreateProductBOM(
			OrgSupplierPart part,
			OrgSupplierPart subPart)
			=> CreateProductBOM(part, subPart, 1, subPart.OP_StockKeepingUnit);

		public OrgPartBOM CreateProductBOM(
			OrgSupplierPart part,
			OrgSupplierPart subPart,
			ZDecimal componentQty,
			ZString componentPack)
		{
			var bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = part.PK;
			bom.OE_OP_Component = subPart.PK;
			bom.OE_ComponentQty = componentQty;
			bom.OE_F3_NKPackType = componentPack;
			return bom;
		}

		#endregion

		#region CreateProductParamsByWhsAndClient

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZString receiveUQ)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, minimum, economicQty, replenishmentMultiple, receiveUQ, 0);
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZDecimal minimum, ZDecimal economicQty, ZString receiveUQ)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, minimum, economicQty, 0m, receiveUQ, 0);
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZDecimal minimum, ZDecimal economicQty)
		{
			return CreateProductParamsByWhsAndClient(part, client, warehouse, minimum, economicQty, "");
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse)
		{
			return CreateProductParamsByWhsAndClient(part, client, warehouse, 0m, 0m);
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZShort maximumShelfLife)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, ZGuid.Empty, 0m, 0m, 0m, "", maximumShelfLife, "");
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZShort maximumShelfLife)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, ZGuid.Empty, minimum, economicQty, replenishmentMultiple, "", maximumShelfLife, "");
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZGuid stagingLocationBOMPK)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, stagingLocationBOMPK, 0m, 0m, 0m, "", 0, "");
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZShort maximumShelfLife, string stockTakeCycle)
		{
			return CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, ZGuid.Empty, 0m, 0m, 0m, "", 0, stockTakeCycle);
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZString receiveUQ, ZShort maximumShelfLife)
		{
			return CreateProductParamsByWhsAndClient(partPK, clientPK, warehousePK, ZGuid.Empty, minimum, economicQty, replenishmentMultiple, receiveUQ, maximumShelfLife, "");
		}

		public WhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZGuid stagingLocationBOMPK, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZString receiveUQ, ZShort maximumShelfLife, string stockTakeCycle)
		{
			var productParams = Factory.New<WhsProductParamsByWhsAndClient>();
			productParams.W3_OP = partPK;
			productParams.W3_OH = clientPK;
			productParams.W3_WW = warehousePK;
			productParams.W3_WL_StagingLocationBOM = stagingLocationBOMPK;

			productParams.W3_ReplenishmentMinimum = minimum;
			productParams.W3_EconomicQuantity = economicQty;
			productParams.W3_ReplenishmentMultiple = replenishmentMultiple;
			productParams.W3_MaximumShelfLife = maximumShelfLife;
			productParams.W3_StockTakeCycle = stockTakeCycle;
			if (!receiveUQ.IsEmpty)
			{
				productParams.W3_F3_NKReceivedPackType = receiveUQ;
			}
			return productParams;
		}

		#endregion

		#region CreateProductBarcode

		public OrgSupplierPartBarcode CreateProductBarcode(OrgSupplierPart part, ZString packType, ZString barcode)
		{
			var orgBarcode = part.PartBarcodes.AddNew();
			orgBarcode.PH_F3_NKPackType = packType;
			orgBarcode.PH_Barcode = barcode;

			return orgBarcode;
		}

		#endregion

		#region CreateSecondaryProduct

		public OrgSecondaryPartBOM CreateSecondaryProduct(OrgSupplierPart part, OrgSupplierPart secondaryPart, decimal quantity)
		{
			var secondaryProduct = part.SecondaryParts.AddNew();
			secondaryProduct.OSB_OP_SecondaryProduct = secondaryPart.PK;
			secondaryProduct.OSB_ProductQuantity = quantity;

			return secondaryProduct;
		}

		#endregion

		#region SetClientAttributeType

		public void SetClientAttributeType(OrgHeader owner, AttributeNumber attribNo, ZString partAttributeType, string attributeName = "")
		{
			var miscServ = owner.MiscServ;

			switch (attribNo)
			{
				case AttributeNumber.One:
					miscServ.OM_IMPartAttrib1Type = partAttributeType;
					miscServ.OM_IMPartAttrib1Name = attributeName;
					break;

				case AttributeNumber.Two:
					miscServ.OM_IMPartAttrib2Type = partAttributeType;
					miscServ.OM_IMPartAttrib2Name = attributeName;
					break;

				case AttributeNumber.Three:
					miscServ.OM_IMPartAttrib3Type = partAttributeType;
					miscServ.OM_IMPartAttrib3Name = attributeName;
					break;

				case AttributeNumber.Serial:
					miscServ.OM_IMUseSerialNumber = (partAttributeType == PartAttributeTypeList.Codes.Mandatory);
					break;

				case AttributeNumber.ExpiryDate:
					miscServ.OM_IMUseExpiryDate = (partAttributeType == PartAttributeTypeList.Codes.Mandatory);
					break;

				case AttributeNumber.PackingDate:
					miscServ.OM_IMUsePackingDate = (partAttributeType == PartAttributeTypeList.Codes.Mandatory);
					break;
			}
		}

		public void SetClientAttributeType(OrgHeader owner, AttributeNumber attribNo, bool mandatoryAttributeType, string attributeName = "")
		{
			if (mandatoryAttributeType)
			{
				SetClientAttributeType(owner, attribNo, PartAttributeTypeList.Codes.Mandatory, attributeName);
			}
			else
			{
				SetClientAttributeType(owner, attribNo, PartAttributeTypeList.Codes.NonMandatory, attributeName);
			}
		}

		public void SetClientAllAttributeType(OrgHeader owner, bool mandatoryAttributeType)
		{
			SetClientAttributeType(owner, AttributeNumber.One, mandatoryAttributeType);
			SetClientAttributeType(owner, AttributeNumber.Two, mandatoryAttributeType);
			SetClientAttributeType(owner, AttributeNumber.Three, mandatoryAttributeType);
			SetClientAttributeType(owner, AttributeNumber.Serial, mandatoryAttributeType);
			SetClientAttributeType(owner, AttributeNumber.ExpiryDate, true);
			SetClientAttributeType(owner, AttributeNumber.PackingDate, true);
		}

		#endregion

		#region SetProductAttributeUse

		public void SetProductAttributeUse(OrgHeader owner, OrgSupplierPart part, AttributeNumber attribNo, bool use, bool setReleaseCaptured = false, string relationshipType = "OWN")
		{
			var relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(owner.PK, relationshipType);
			if (relation != null)
			{
				switch (attribNo)
				{
					case AttributeNumber.One:
						relation.OU_UsePartAttrib1 = use;
						relation.OU_IsPartAttrib1ReleaseCaptured = setReleaseCaptured;
						break;
					case AttributeNumber.Two:
						relation.OU_UsePartAttrib2 = use;
						relation.OU_IsPartAttrib2ReleaseCaptured = setReleaseCaptured;
						break;
					case AttributeNumber.Three:
						relation.OU_UsePartAttrib3 = use;
						relation.OU_IsPartAttrib3ReleaseCaptured = setReleaseCaptured;
						break;
					case AttributeNumber.Serial:
						relation.OU_UseSerialNumber = use;
						relation.OU_IsSerialNumberReleaseCaptured = setReleaseCaptured;
						break;
					case AttributeNumber.ExpiryDate:
						relation.OU_UseExpiryDate = use;
						break;
					case AttributeNumber.PackingDate:
						relation.OU_UsePackingDate = use;
						break;
				}
			}
			else
			{
				throw new Exception("Tried to set Product Attribute Use on non-existent OrgPartRelation.");
			}
		}

		public void SetProductAllAttributeUse(OrgHeader owner, OrgSupplierPart part, bool use, bool setReleaseCaptured = false, bool useSerialNumber = true)
		{
			SetProductAttributeUse(owner, part, AttributeNumber.One, use, setReleaseCaptured);
			SetProductAttributeUse(owner, part, AttributeNumber.Two, use, setReleaseCaptured);
			SetProductAttributeUse(owner, part, AttributeNumber.Three, use, setReleaseCaptured);
			SetProductAttributeUse(owner, part, AttributeNumber.ExpiryDate, use);
			SetProductAttributeUse(owner, part, AttributeNumber.PackingDate, use);
			if (useSerialNumber)
			{
				SetProductAttributeUse(owner, part, AttributeNumber.Serial, use, setReleaseCaptured);
			}
		}

		#endregion

		#region SetProductWeightAndVolume

		public void SetProductWeightAndVolume(OrgSupplierPart part, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			part.OP_Weight = weight;
			part.OP_WeightUQ = weightUQ;
			part.OP_Cubic = volume;
			part.OP_CubicUQ = volumeUQ;
		}

		#endregion

		#region CreateABCCategory

		public WhsABCCategory CreateABCCategory(OrgSupplierPart part, OrgHeader client, WhsWarehouse warehouse, ZString categoryName, ZDateTimeOffset analysisDateFrom, ZDateTimeOffset analysisDateTo)
		{
			var abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_OP_Product = part.PK;
			abcCategory.WJ_OH_Client = client.PK;
			abcCategory.WJ_WW_Warehouse = warehouse.PK;
			abcCategory.WJ_Category = categoryName;
			abcCategory.WJ_AnalysisDateFrom = analysisDateFrom;
			abcCategory.WJ_AnalysisDateTo = analysisDateTo;
			return abcCategory;
		}

		#endregion

		#endregion

		#region InventoryHeldCode

		public WhsInventoryHeldCode CreateInventoryHeldCode(ZString code, ZString description, ZGuid? clientPK = null, bool isSystem = false)
		{
			var heldCode = Factory.NewWithValidTestData<WhsInventoryHeldCode>();
			heldCode.WHC_Code = code;
			heldCode.WHC_Description = description;
			heldCode.WHC_IsSystem = isSystem;
			heldCode.WHC_OH_Client = clientPK == null ? ZGuid.Empty : clientPK.Value;
			return heldCode;
		}

		BusinessObject IWhsTransactionTestHelper.CreateInventoryHeldCode(ZString code, ZString description, bool isSystem)
		{
			return CreateInventoryHeldCode(code, description, ZGuid.Empty, isSystem);
		}

		BusinessObject IWhsTransactionTestHelper.CreateInventoryHeldCode(ZString code, ZString description, ZGuid clientPK, bool isSystem)
		{
			return CreateInventoryHeldCode(code, description, clientPK, isSystem);
		}

		#endregion

		#region Attributes

		#region SetDocketCustomAttributes

		public void SetDocketCustomAttributes(WhsDocket docket, string customAttrib1, string customAttrib2, string customAttrib3, string customAttrib4, string customAttrib5,
			ZDateTime customDate1, ZDateTime customDate2,
			decimal customDecimal1, decimal customDecimal2, decimal customDecimal3, decimal customDecimal4, decimal customDecimal5,
			bool customFlag1, bool customFlag2, bool customFlag3, bool customFlag4, bool customFlag5)
		{
			docket.WD_CustomAttrib1 = customAttrib1;
			docket.WD_CustomAttrib2 = customAttrib2;
			docket.WD_CustomAttrib3 = customAttrib3;
			docket.WD_CustomAttrib4 = customAttrib4;
			docket.WD_CustomAttrib5 = customAttrib5;
			docket.WD_CustomDate1 = customDate1;
			docket.WD_CustomDate2 = customDate2;
			docket.WD_CustomDecimal1 = customDecimal1;
			docket.WD_CustomDecimal2 = customDecimal2;
			docket.WD_CustomDecimal3 = customDecimal3;
			docket.WD_CustomDecimal4 = customDecimal4;
			docket.WD_CustomDecimal5 = customDecimal5;
			docket.WD_CustomFlag1 = customFlag1;
			docket.WD_CustomFlag2 = customFlag2;
			docket.WD_CustomFlag3 = customFlag3;
			docket.WD_CustomFlag4 = customFlag4;
			docket.WD_CustomFlag5 = customFlag5;
		}

		#endregion

		#region SetDocketLineAttributes

		public void SetDocketLineAttributes(WhsDocketLine line, WhsInventoryView from)
		{
			SetDocketLineAttributes(line, from.WI_ExpiryDate, from.WI_PackingDate, from.WI_PartAttrib1, from.WI_PartAttrib2, from.WI_PartAttrib3, from.WI_SerialNumber, from.WI_BondedEntryKey);
		}

		public void SetDocketLineAttributes(WhsDocketLine line, WhsDocketLine from)
		{
			SetDocketLineAttributes(line, from.WE_ExpiryDate, from.WE_PackingDate, from.WE_PartAttrib1, from.WE_PartAttrib2, from.WE_PartAttrib3, from.WE_SerialNumber, from.WE_BondedEntryKey);
		}

		public void SetDocketLineAttributes(WhsDocketLine line, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string sn, string bEK)
		{
			SetDocketLineAttributes(line, eD, pD, pA1, pA2, pA3, sn);
			line.WE_BondedEntryKey = bEK;
		}

		public void SetDocketLineAttributes(WhsDocketLine line, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string sn)
		{
			line.WE_ExpiryDate = eD;
			line.WE_PackingDate = pD;
			line.WE_PartAttrib1 = pA1;
			line.WE_PartAttrib2 = pA2;
			line.WE_PartAttrib3 = pA3;
			line.WE_SerialNumber = sn;
		}

		#endregion

		#region SetDocketLineCustomAttributes

		public void SetDocketLineCustomAttributes(WhsDocketLine line, string cA1, string cA2, string cA3, string cA4, string cA5, string cA6, ZDecimal cDecimal1, ZDecimal cDecimal2, ZDecimal cDecimal3, ZDecimal cDecimal4, ZDecimal cDecimal5, ZDateTime cDate1, ZDateTime cDate2, ZDateTime cDate3, ZDateTime cDate4, ZDateTime cDate5, ZBool cFlag1, ZBool cFlag2, ZBool cFlag3, ZBool cFlag4, ZBool cFlag5, ZString textBlob1)
		{
			line.WE_CustomAttrib1 = cA1;
			line.WE_CustomAttrib2 = cA2;
			line.WE_CustomAttrib3 = cA3;
			line.WE_CustomAttrib4 = cA4;
			line.WE_CustomAttrib5 = cA5;
			line.WE_CustomAttrib6 = cA6;
			line.WE_CustomDecimal1 = cDecimal1;
			line.WE_CustomDecimal2 = cDecimal2;
			line.WE_CustomDecimal3 = cDecimal3;
			line.WE_CustomDecimal4 = cDecimal4;
			line.WE_CustomDecimal5 = cDecimal5;
			line.WE_CustomDate1 = cDate1;
			line.WE_CustomDate2 = cDate2;
			line.WE_CustomDate3 = cDate3;
			line.WE_CustomDate4 = cDate4;
			line.WE_CustomDate5 = cDate5;
			line.WE_CustomFlag1 = cFlag1;
			line.WE_CustomFlag2 = cFlag2;
			line.WE_CustomFlag3 = cFlag3;
			line.WE_CustomFlag4 = cFlag4;
			line.WE_CustomFlag5 = cFlag5;
			line.WE_CustomTextBlob1 = textBlob1;
		}

		#endregion

		#region SetInventoryAttributes

		public void SetInventoryAttributes(WhsInventoryView line, ZDate eD, ZDate pD, string pA1, string pA2, string pA3)
		{
			SetInventoryAttributes(line, eD, pD, pA1, pA2, pA3, "", "");
		}

		public void SetInventoryAttributes(WhsInventoryView line, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			SetInventoryAttributes(line, eD, pD, pA1, pA2, pA3, "", "");
		}

		public void SetInventoryAttributes(WhsInventoryView line, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK, string serialNum)
		{
			line.WI_ExpiryDate = eD;
			line.WI_PackingDate = pD;
			line.WI_PartAttrib1 = pA1;
			line.WI_PartAttrib2 = pA2;
			line.WI_PartAttrib3 = pA3;
			line.WI_SerialNumber = serialNum;
			line.WI_BondedEntryKey = bEK.ToUpper();
		}

		#endregion

		#region SetInventoryCustomAttributes

		public void SetInventoryCustomAttributes(WhsInventoryView line, string cA1, string cA2, string cA3, string cA4, string cA5, string cA6, ZDecimal cDecimal1, ZDecimal cDecimal2, ZDecimal cDecimal3, ZDecimal cDecimal4, ZDecimal cDecimal5, ZDateTime cDate1, ZDateTime cDate2, ZDateTime cDate3, ZDateTime cDate4, ZDateTime cDate5, ZBool cFlag1, ZBool cFlag2, ZBool cFlag3, ZBool cFlag4, ZBool cFlag5, ZString textBlob1)
		{
			line.WI_CustomAttrib_1 = cA1;
			line.WI_CustomAttrib_2 = cA2;
			line.WI_CustomAttrib_3 = cA3;
			line.WI_CustomAttrib4 = cA4;
			line.WI_CustomAttrib5 = cA5;
			line.WI_CustomAttrib6 = cA6;
			line.WI_CustomDecimal1 = cDecimal1;
			line.WI_CustomDecimal2 = cDecimal2;
			line.WI_CustomDecimal3 = cDecimal3;
			line.WI_CustomDecimal4 = cDecimal4;
			line.WI_CustomDecimal5 = cDecimal5;
			line.WI_CustomDate1 = cDate1;
			line.WI_CustomDate2 = cDate2;
			line.WI_CustomDate3 = cDate3;
			line.WI_CustomDate4 = cDate4;
			line.WI_CustomDate5 = cDate5;
			line.WI_CustomFlag1 = cFlag1;
			line.WI_CustomFlag2 = cFlag2;
			line.WI_CustomFlag3 = cFlag3;
			line.WI_CustomFlag4 = cFlag4;
			line.WI_CustomFlag5 = cFlag5;
			line.WI_CustomTextBlob1 = textBlob1;
		}

		#endregion

		#region AssertDocketCustomAttributes

		public void AssertDocketCustomAttributes(WhsDocket docket, string customAttrib1, string customAttrib2, string customAttrib3, string customAttrib4, string customAttrib5,
			ZDateTime customDate1, ZDateTime customDate2,
			decimal customDecimal1, decimal customDecimal2, decimal customDecimal3, decimal customDecimal4, decimal customDecimal5,
			bool customFlag1, bool customFlag2, bool customFlag3, bool customFlag4, bool customFlag5)
		{
			AssertEquals("WD_CustomAttrib1", customAttrib1, docket.WD_CustomAttrib1);
			AssertEquals("WD_CustomAttrib2", customAttrib2, docket.WD_CustomAttrib2);
			AssertEquals("WD_CustomAttrib3", customAttrib3, docket.WD_CustomAttrib3);
			AssertEquals("WD_CustomAttrib4", customAttrib4, docket.WD_CustomAttrib4);
			AssertEquals("WD_CustomAttrib5", customAttrib5, docket.WD_CustomAttrib5);
			AssertEquals("WD_CustomDate1", customDate1, docket.WD_CustomDate1);
			AssertEquals("WD_CustomDate2", customDate2, docket.WD_CustomDate2);
			AssertEquals("WD_CustomDecimal1", customDecimal1, docket.WD_CustomDecimal1);
			AssertEquals("WD_CustomDecimal2", customDecimal2, docket.WD_CustomDecimal2);
			AssertEquals("WD_CustomDecimal3", customDecimal3, docket.WD_CustomDecimal3);
			AssertEquals("WD_CustomDecimal4", customDecimal4, docket.WD_CustomDecimal4);
			AssertEquals("WD_CustomDecimal5", customDecimal5, docket.WD_CustomDecimal5);
			AssertEquals("WD_CustomFlag1", customFlag1, docket.WD_CustomFlag1);
			AssertEquals("WD_CustomFlag2", customFlag2, docket.WD_CustomFlag2);
			AssertEquals("WD_CustomFlag3", customFlag3, docket.WD_CustomFlag3);
			AssertEquals("WD_CustomFlag4", customFlag4, docket.WD_CustomFlag4);
			AssertEquals("WD_CustomFlag5", customFlag5, docket.WD_CustomFlag5);
		}

		#endregion

		#region AssertDocketLineAttributes

		public void AssertDocketLineAttributes(WhsDocketLine line, ZDate expiryDate, ZDate packingDate,
			ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString bondedEntryKey)
		{
			AssertEquals("WE_ExpiryDate set incorrectly", expiryDate, line.WE_ExpiryDate);
			AssertEquals("WE_PackingDate set incorrectly", packingDate, line.WE_PackingDate);
			AssertEquals("WE_PartAttrib1 set incorrectly", partAttrib1, line.WE_PartAttrib1);
			AssertEquals("WE_PartAttrib2 set incorrectly", partAttrib2, line.WE_PartAttrib2);
			AssertEquals("WE_PartAttrib3 set incorrectly", partAttrib3, line.WE_PartAttrib3);
			AssertEquals("WE_BondedEntryKey set incorrectly", bondedEntryKey, line.WE_BondedEntryKey);
		}

		#endregion

		#region AssertInventoryAttributes

		public void AssertInventoryAttributes(WhsInventoryView line, ZDate expiryDate, ZDate packingDate,
			ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString bondedEntryKey)
		{
			AssertEquals("WI_ExpiryDate set incorrectly", expiryDate, line.WI_ExpiryDate);
			AssertEquals("WI_PackingDate set incorrectly", packingDate, line.WI_PackingDate);
			AssertEquals("WI_PartAttrib1 set incorrectly", partAttrib1, line.WI_PartAttrib1);
			AssertEquals("WI_PartAttrib2 set incorrectly", partAttrib2, line.WI_PartAttrib2);
			AssertEquals("WI_PartAttrib3 set incorrectly", partAttrib3, line.WI_PartAttrib3);
			AssertEquals("WI_BondedEntryKey set incorrectly", bondedEntryKey, line.WI_BondedEntryKey);
		}

		#endregion

		#region AssertAttributes

		public void AssertAttributes(ILineAttributes expected, ILineAttributes actual)
		{
			AssertEquals("ExpiryDate set incorrectly", expected.ExpiryDate, actual.ExpiryDate);
			AssertEquals("PackingDate set incorrectly", expected.PackingDate, actual.PackingDate);
			AssertEquals("PartAttrib1 set incorrectly", expected.PartAttrib1, actual.PartAttrib1);
			AssertEquals("PartAttrib2 set incorrectly", expected.PartAttrib2, actual.PartAttrib2);
			AssertEquals("PartAttrib3 set incorrectly", expected.PartAttrib3, actual.PartAttrib3);
			AssertEquals("BondedEntryKey set incorrectly", expected.BondedEntryKey, actual.BondedEntryKey);
		}

		#endregion

		#region AssertDocketLineCustomAttributes

		public void AssertDocketLineCustomAttributes(WhsDocketLine line, ZString customAttrib1, ZString customAttrib2, ZString customAttrib3, ZString customAttrib4, ZString customAttrib5, ZString customAttrib6, ZDecimal customDecimall, ZDecimal customDecimal2, ZDecimal customDecimal3, ZDecimal customDecimal4, ZDecimal customDecimal5, ZDateTime customDate1, ZDateTime customDate2, ZDateTime customDate3, ZDateTime customDate4, ZDateTime customDate5, ZBool customFlag1, ZBool customFlag2, ZBool customFlag3, ZBool customFlag4, ZBool customFlag5, ZString customTextBlob1)
		{
			AssertEquals("WE_CustomAttrib1 set incorrectly", customAttrib1, line.WE_CustomAttrib1);
			AssertEquals("WE_CustomAttrib2 set incorrectly", customAttrib2, line.WE_CustomAttrib2);
			AssertEquals("WE_CustomAttrib3 set incorrectly", customAttrib3, line.WE_CustomAttrib3);
			AssertEquals("WE_CustomAttrib4 set incorrectly", customAttrib4, line.WE_CustomAttrib4);
			AssertEquals("WE_CustomAttrib5 set incorrectly", customAttrib5, line.WE_CustomAttrib5);
			AssertEquals("WE_CustomAttrib6 set incorrectly", customAttrib6, line.WE_CustomAttrib6);
			AssertEquals("WE_CustomDecimal1 set incorrectly", customDecimall, line.WE_CustomDecimal1);
			AssertEquals("WE_CustomDecimal2 set incorrectly", customDecimal2, line.WE_CustomDecimal2);
			AssertEquals("WE_CustomDecimal3 set incorrectly", customDecimal3, line.WE_CustomDecimal3);
			AssertEquals("WE_CustomDecimal4 set incorrectly", customDecimal4, line.WE_CustomDecimal4);
			AssertEquals("WE_CustomDecimal5 set incorrectly", customDecimal5, line.WE_CustomDecimal5);
			AssertEquals("WE_CustomDate1 set incorrectly", customDate1, line.WE_CustomDate1);
			AssertEquals("WE_CustomDate2 set incorrectly", customDate2, line.WE_CustomDate2);
			AssertEquals("WE_CustomDate3 set incorrectly", customDate3, line.WE_CustomDate3);
			AssertEquals("WE_CustomDate4 set incorrectly", customDate4, line.WE_CustomDate4);
			AssertEquals("WE_CustomDate5 set incorrectly", customDate5, line.WE_CustomDate5);
			AssertEquals("WE_CustomFlag1 set incorrectly", customFlag1, line.WE_CustomFlag1);
			AssertEquals("WE_CustomFlag2 set incorrectly", customFlag2, line.WE_CustomFlag2);
			AssertEquals("WE_CustomFlag3 set incorrectly", customFlag3, line.WE_CustomFlag3);
			AssertEquals("WE_CustomFlag4 set incorrectly", customFlag4, line.WE_CustomFlag4);
			AssertEquals("WE_CustomFlag5 set incorrectly", customFlag5, line.WE_CustomFlag5);
			AssertEquals("WE_CustomTextBlob1 set incorrectly", customTextBlob1, line.WE_CustomTextBlob1);
		}

		public void AssertDocketLineCustomAttributes(WhsInventoryView line, ZString customAttrib1, ZString customAttrib2, ZString customAttrib3, ZString customAttrib4, ZString customAttrib5, ZString customAttrib6, ZDecimal customDecimall, ZDecimal customDecimal2, ZDecimal customDecimal3, ZDecimal customDecimal4, ZDecimal customDecimal5, ZDateTime customDate1, ZDateTime customDate2, ZDateTime customDate3, ZDateTime customDate4, ZDateTime customDate5, ZBool customFlag1, ZBool customFlag2, ZBool customFlag3, ZBool customFlag4, ZBool customFlag5, ZString customTextBlob1)
		{
			AssertEquals("WI_CustomAttrib1 set incorrectly", customAttrib1, line.WI_CustomAttrib_1);
			AssertEquals("WI_CustomAttrib2 set incorrectly", customAttrib2, line.WI_CustomAttrib_2);
			AssertEquals("WI_CustomAttrib3 set incorrectly", customAttrib3, line.WI_CustomAttrib_3);
			AssertEquals("WI_CustomAttrib4 set incorrectly", customAttrib4, line.WI_CustomAttrib4);
			AssertEquals("WI_CustomAttrib5 set incorrectly", customAttrib5, line.WI_CustomAttrib5);
			AssertEquals("WI_CustomAttrib6 set incorrectly", customAttrib6, line.WI_CustomAttrib6);
			AssertEquals("WI_CustomDecimal1 set incorrectly", customDecimall, line.WI_CustomDecimal1);
			AssertEquals("WI_CustomDecimal2 set incorrectly", customDecimal2, line.WI_CustomDecimal2);
			AssertEquals("WI_CustomDecimal3 set incorrectly", customDecimal3, line.WI_CustomDecimal3);
			AssertEquals("WI_CustomDecimal4 set incorrectly", customDecimal4, line.WI_CustomDecimal4);
			AssertEquals("WI_CustomDecimal5 set incorrectly", customDecimal5, line.WI_CustomDecimal5);
			AssertEquals("WI_CustomDate1 set incorrectly", customDate1, line.WI_CustomDate1);
			AssertEquals("WI_CustomDate2 set incorrectly", customDate2, line.WI_CustomDate2);
			AssertEquals("WI_CustomDate3 set incorrectly", customDate3, line.WI_CustomDate3);
			AssertEquals("WI_CustomDate4 set incorrectly", customDate4, line.WI_CustomDate4);
			AssertEquals("WI_CustomDate5 set incorrectly", customDate5, line.WI_CustomDate5);
			AssertEquals("WI_CustomFlag1 set incorrectly", customFlag1, line.WI_CustomFlag1);
			AssertEquals("WI_CustomFlag2 set incorrectly", customFlag2, line.WI_CustomFlag2);
			AssertEquals("WI_CustomFlag3 set incorrectly", customFlag3, line.WI_CustomFlag3);
			AssertEquals("WI_CustomFlag4 set incorrectly", customFlag4, line.WI_CustomFlag4);
			AssertEquals("WI_CustomFlag5 set incorrectly", customFlag5, line.WI_CustomFlag5);
			AssertEquals("WI_CustomTextBlob1 set incorrectly", customTextBlob1, line.WI_CustomTextBlob1);
		}

		#endregion

		#endregion

		#region CustomsData

		public WhsBondedWarehouseAttribute CreateCustomsData(WhsDocketLine docketLine)
		{
			WhsBondedWarehouseAttribute customsData = Factory.New<WhsBondedWarehouseAttribute>();
			customsData.SetParent(docketLine);
			return customsData;
		}

		#endregion

		#region CreateWhsHoldOrder

		public WhsHoldOrder CreateWhsHoldOrder(WhsWarehouse warehouse, OrgHeader client)
		{
			Argument.NotNull(warehouse, "warehouse");
			Argument.NotNull(client, "client");

			var holdOrder = new WhsHoldOrder(Factory);
			holdOrder.WarehousePK = warehouse.PK;
			holdOrder.ClientPK = client.PK;
			return holdOrder;
		}

		#endregion

		#region CreateWhsHoldOrderLine

		public WhsHoldOrderLine CreateWhsHoldOrderLine(WhsHoldOrder holdOrder, OrgSupplierPart product, ZString fromHoldCode, ZString toHoldCode, ZDecimal quantity,
			string pa1 = "", string pa2 = "", string pa3 = "", ZDate? expiryDate = null, ZDate? packingDate = null, string serialNumber = "")
		{
			Argument.NotNull(holdOrder, "holdOrder");
			Argument.NotNull(product, "product");

			var line = holdOrder.Lines.AddNew();
			line.ProductPK = product.PK;
			line.FromHoldCode = fromHoldCode;
			line.ToHoldCode = toHoldCode;
			line.Quantity = quantity;
			line.PartAttrib1 = pa1;
			line.PartAttrib2 = pa2;
			line.PartAttrib3 = pa3;
			line.SerialNumber = serialNumber;
			line.ExpiryDate = expiryDate ?? ZDate.Empty;
			line.PackingDate = packingDate ?? ZDate.Empty;
			return line;
		}

		#endregion

		#region Receive Docket

		#region CreateWhsReceive

		public WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsReceive(client.PK, whs.PK);
		}

		public WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse whs, ZString @ref)
		{
			return CreateWhsReceive(client.PK, whs.PK, @ref);
		}

		public WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse whs, NotificationBuffer notify)
		{
			return CreateWhsReceive(client.PK, whs.PK, notify);
		}

		public WhsReceive CreateWhsReceive(OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsReceive(client.PK, whs.PK, @ref, notify);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsReceive(clientPK, whsPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK, NotificationBuffer notify)
		{
			return CreateWhsReceive(clientPK, whsPK, "TEST", ZDateTimeOffset.Now, notify);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK, ZString @ref)
		{
			return CreateWhsReceive(clientPK, whsPK, @ref, ZDateTimeOffset.Now, null);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsReceive(clientPK, whsPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking)
		{
			return CreateWhsReceive(clientPK, whsPK, @ref, booking, null);
		}

		public WhsReceive CreateWhsReceive(ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			WhsReceive doc = Factory.New<WhsReceive>();
			SetupWhsReceiveCore(doc, clientPK, whsPK, @ref, booking, notify);
			return doc;
		}

		// CreateWhsReceiveWithInventory
		BusinessObject IWhsTransactionTestHelper.CreateWhsReceiveWithInventory(ZGuid clientPK, ZGuid whsWarehousePK, ZString reference, ZGuid partPK, ZDecimal units)
		{
			var client = Factory.Load<OrgHeader>(clientPK);
			var whs = Factory.Load<WhsWarehouse>(whsWarehousePK);
			var part = Factory.Load<OrgSupplierPart>(partPK);
			return CreateWhsReceiveWithInventory(client, whs, reference, part, units);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal units, bool allocateLocations = true, bool finalise = true)
		{
			return CreateWhsReceiveWithInventory(client, whs, reference, ZDateTimeOffset.Now, part, units, null, "", allocateLocations, finalise);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal units, ZString bondedEntryKey, bool allocateLocations = true, bool finalise = true)
		{
			return CreateWhsReceiveWithInventory(client, whs, reference, ZDateTimeOffset.Now, part, units, bondedEntryKey, null, "", allocateLocations, finalise);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, ZDateTimeOffset arrivalDate, OrgSupplierPart part, ZDecimal units, bool allocateLocations = true, bool finalise = true)
		{
			return CreateWhsReceiveWithInventory(client, whs, reference, arrivalDate, part, units, null, "", allocateLocations, finalise);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString palletID, bool allocateLocations = true, bool finalise = true)
		{
			return CreateWhsReceiveWithInventory(client, whs, reference, ZDateTimeOffset.Now, part, units, location, palletID, allocateLocations, finalise);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, ZDateTimeOffset arrivalDate, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString palletID, bool allocateLocations = true, bool finalise = true)
		{
			return CreateWhsReceiveWithInventory(client, whs, reference, arrivalDate, part, units, "", location, palletID, allocateLocations, finalise);
		}

		public WhsReceive CreateWhsReceiveWithInventory(OrgHeader client, IWhsWarehouse whs, ZString reference, ZDateTimeOffset arrivalDate, OrgSupplierPart part, ZDecimal units, ZString bondedEntryKey, WhsLocation location, ZString palletID, bool allocateLocations = true, bool finalise = true)
		{
			var receive = CreateWhsReceive(client.PK, whs.PK, reference, arrivalDate);
			CreateWhsReceiveInventoryLine(receive, part == null ? ZGuid.Empty : part.PK, units, location == null ? ZGuid.Empty : location.PK, palletID, bondedEntryKey);

			if (allocateLocations)
			{
				receive.AllocateLocationsWithMock();
			}

			if (finalise)
			{
				if (!allocateLocations && location == null)
				{
					throw new ArgumentException("location cannot be null");
				}

				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			}

			return receive;
		}

		#endregion

		#region SetUpWhsReceive

		public WhsReceive SetUpWhsReceive(WhsReceive doc, OrgHeader client, WhsWarehouse whs)
		{
			return SetUpWhsReceive(doc, client.PK, whs.PK);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, OrgHeader client, WhsWarehouse whs, ZString @ref)
		{
			return SetUpWhsReceive(doc, client.PK, whs.PK, @ref);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, OrgHeader client, WhsWarehouse whs, NotificationBuffer notify)
		{
			return SetUpWhsReceive(doc, client.PK, whs.PK, notify);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return SetUpWhsReceive(doc, client.PK, whs.PK, @ref, notify);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK)
		{
			return SetUpWhsReceive(doc, clientPK, whsPK, "TEST", FirstArrivalDateUsed, null);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK, NotificationBuffer notify)
		{
			return SetUpWhsReceive(doc, clientPK, whsPK, "TEST", FirstArrivalDateUsed, notify);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK, ZString @ref)
		{
			return SetUpWhsReceive(doc, clientPK, whsPK, @ref, FirstArrivalDateUsed, null);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return SetUpWhsReceive(doc, clientPK, whsPK, @ref, FirstArrivalDateUsed, notify);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTime booking)
		{
			return SetUpWhsReceive(doc, clientPK, whsPK, @ref, booking, null);
		}

		public WhsReceive SetUpWhsReceive(WhsReceive doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTime booking, NotificationBuffer notify)
		{
			SetupWhsReceiveCore(doc, clientPK, whsPK, @ref, booking.ToOffset(), notify);
			return doc;
		}

		public void SetupWhsReceiveCore(WhsReceive receive, ZGuid clientPK, ZGuid whsPK, ZString reference, ZDateTimeOffset arrivalDate, NotificationBuffer notify)
		{
			SetupDocketCore(receive, clientPK, whsPK, reference, notify);
			receive.WD_ArrivalDate = arrivalDate;
		}

		// To avoid the common class of amnesty where multiple receives are created, but developer assumes they will be grouped into a single AvailableInventory.
		ZDateTime FirstArrivalDateUsed => firstArrivalDateUsed ?? (firstArrivalDateUsed = ZDateTime.Now).Value;
		ZDateTime? firstArrivalDateUsed;

		#endregion

		#region CreateWhsReceiveInventoryLine

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units)
		{
			return CreateWhsReceiveInventoryLine(receive, part.PK, units);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location)
		{
			return CreateWhsReceiveInventoryLine(receive, part.PK, units, location.PK, "");
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, string palletID)
		{
			return CreateWhsReceiveLine(receive, part, units, location, palletID).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, string palletID, string inventoryStatus, string heldCode = "")
		{
			return CreateWhsReceiveLine(receive, part, units, location, palletID, inventoryStatus, heldCode).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			return CreateWhsReceiveInventoryLine(receive, part, units, null, eD, pD, pA1, pA2, pA3, bEK);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			return CreateWhsReceiveInventoryLine(receive, part, units, location, "", eD, pD, pA1, pA2, pA3, bEK);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString palletID, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			return CreateWhsReceiveLine(receive, part, units, location, palletID, eD, pD, pA1, pA2, pA3, bEK).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string sn, string bEK)
		{
			return CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, eD, pD, pA1, pA2, pA3, sn, bEK).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZString entryKey, ZString packageGroupID, ZDecimal perPackageQty)
		{
			return CreateWhsReceiveInventoryLine(receive, part, units, ZGuid.Empty, entryKey, packageGroupID, perPackageQty);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZGuid locationPK, ZString entryKey, ZString packageGroupID, ZDecimal perPackageQty)
		{
			return CreateWhsReceiveLine(receive, part, units, locationPK, entryKey, packageGroupID, perPackageQty).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZString entryKey)
		{
			return CreateWhsReceiveInventoryLine(receive, part, units, entryKey, "", 0m);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units)
		{
			return CreateWhsReceiveInventoryLine(receive, partPK, units, "");
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZString entryKey, ZString inventoryStatus, string heldCode = "")
		{
			return CreateWhsReceiveLine(receive, partPK, units, ZGuid.Empty, "", entryKey, inventoryStatus, heldCode).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID)
		{
			return CreateWhsReceiveInventoryLine(receive, partPK, units, locationPK, palletID, "");
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZShort lineNo, ZShort subLineNo)
		{
			return CreateWhsReceiveLine(receive, part, units, lineNo, subLineNo).Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZString entryKey)
		{
			return CreateWhsReceiveInventoryLine(receive, partPK, units, ZGuid.Empty, "", entryKey);
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZString entryKey)
		{
			return CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, entryKey, "", "").Inventory[0];
		}

		public WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZString entryKey, ZString inventoryStatus, string heldCode = "")
		{
			return CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, entryKey, inventoryStatus, heldCode).Inventory[0];
		}

		#endregion

		#region CreateWhsReceiveLine

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units)
		{
			return CreateWhsReceiveLine(receive, part.PK, units);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location)
		{
			return CreateWhsReceiveLine(receive, part.PK, units, location.PK, "");
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, string palletID)
		{
			var locationPK = location != null ? location.PK : ZGuid.Empty;
			return CreateWhsReceiveLine(receive, part.PK, units, locationPK, palletID, "");
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, string palletID, string inventoryStatus, string heldCode = "")
		{
			var locationPK = location != null ? location.PK : ZGuid.Empty;
			return CreateWhsReceiveLine(receive, part.PK, units, locationPK, palletID, "", inventoryStatus, heldCode);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			return CreateWhsReceiveLine(receive, part, units, null, eD, pD, pA1, pA2, pA3, bEK);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			return CreateWhsReceiveLine(receive, part, units, location, "", eD, pD, pA1, pA2, pA3, bEK);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString palletID, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string bEK)
		{
			ZGuid locationPK = (location != null) ? location.PK : ZGuid.Empty;
			return CreateWhsReceiveLine(receive, part.PK, units, locationPK, palletID, eD, pD, pA1, pA2, pA3, "", bEK);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZDate eD, ZDate pD, string pA1, string pA2, string pA3, string sn, string bEK)
		{
			var line = CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, bEK);
			SetDocketLineAttributes(line, eD, pD, pA1, pA2, pA3, sn);
			return line;
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZString entryKey, ZString packageGroupID, ZDecimal perPackageQty)
		{
			return CreateWhsReceiveLine(receive, part, units, ZGuid.Empty, entryKey, packageGroupID, perPackageQty);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZGuid locationPK, ZString entryKey, ZString packageGroupID, ZDecimal perPackageQty)
		{
			var line = CreateWhsReceiveLine(receive, part.PK, units, locationPK, "", entryKey);
			line.WE_PackageGroupId = packageGroupID;
			line.WE_PerPackageQty = perPackageQty;

			return line;
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZString entryKey)
		{
			return CreateWhsReceiveLine(receive, part, units, entryKey, "", 0m);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units)
		{
			return CreateWhsReceiveLine(receive, partPK, units, "");
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZString entryKey, ZString inventoryStatus, string heldCode = "")
		{
			return CreateWhsReceiveLine(receive, partPK, units, ZGuid.Empty, "", entryKey, inventoryStatus, heldCode);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID)
		{
			return CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, "");
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZShort lineNo, ZShort subLineNo)
		{
			var line = CreateWhsReceiveLine(receive, part, units);
			line.WE_LineNo = lineNo;
			line.WE_SubLineNo = subLineNo;

			return line;
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZString entryKey)
		{
			return CreateWhsReceiveLine(receive, partPK, units, ZGuid.Empty, "", entryKey);
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZString entryKey)
		{
			return CreateWhsReceiveLine(receive, partPK, units, locationPK, palletID, entryKey, "", "");
		}

		public WhsReceiveLine CreateWhsReceiveLine(WhsReceive receive, ZGuid partPK, ZDecimal units, ZGuid locationPK, ZString palletID, ZString entryKey, ZString inventoryStatus, string heldCode = "")
		{
			var line = receive.Lines.AddNew();
			line.WE_IsOriginalInventory = true;
			line.WE_WD = receive.PK;

			line.WE_OP = partPK;
			line.WE_ClientOrderedUnits = units;
			line.WE_TransactionQuantity = units;
			line.WE_WL = locationPK;
			line.WE_BondedEntryKey = entryKey;
			line.WE_PalletID = palletID;
			if (!entryKey.IsEmpty)
			{
				var entry = WhsBondedWarehouseAttribute.BreakUpKey(entryKey);
				line.CustomsData.WB_EntryKey = entry.EntryKey;
				line.CustomsData.WB_EntryLineNo = entry.EntryLineNo;
			}

			if (!inventoryStatus.IsEmpty)
			{
				line.WE_OriginalInventoryStatus = inventoryStatus;
			}

			if (!string.IsNullOrEmpty(heldCode) || inventoryStatus == InventoryStatus.Codes.Held)
			{
				line.WE_WHC_NKOriginalInventoryHeldCode = !string.IsNullOrEmpty(heldCode) ? heldCode : InventoryStatus.Codes.Held;
			}

			return line;
		}

		#region AllocateLocationsWithMock

		public void AllocateLocationsWithMock(WhsReceive receive) => receive.AllocateLocationsWithMock();

		#endregion

		#endregion

		#region SetExpectedQuantities

		public void SetExpectedQuantities(WhsReceive doc, ZDecimal[] quantities)
		{
			for (int i = 0; i < quantities.Length; i++)
			{
				var inventory = doc.Inventory[i];
				var receiveLine = (WhsReceiveLine)inventory.InDocketLine;
				using (new SemaphoreManager(receiveLine.UpdatingTransactionQtyFromExpectedQtySemaphore))
				{
					doc.Inventory[i].WI_ExpectedReceiptQuantity = quantities[i];
				}
			}
		}

		#endregion

		#region CreateAsnLine

		public WhsAsnLine CreateAsnLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units)
		{
			return CreateAsnLine(receive, part, units, 0, 0);
		}

		public WhsAsnLine CreateAsnLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZInt lineNo, ZInt subLineNo)
		{
			return CreateAsnLine(receive, part, units, lineNo, subLineNo, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
		}

		public WhsAsnLine CreateAsnLine(WhsReceive receive, OrgSupplierPart part, ZDecimal units, ZInt lineNo, ZInt subLineNo, ZString palletId, ZDate packingDate, ZDate expiryDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
		{
			var asnLine = receive.AsnLines.AddNew();
			asnLine.WN_LineNo = lineNo;
			asnLine.WN_SubLineNo = subLineNo;
			asnLine.WN_OP = part.PK;
			asnLine.WN_Quantity = units;
			asnLine.WN_PalletId = palletId;
			asnLine.WN_PartAttrib1 = partAttrib1;
			asnLine.WN_PartAttrib2 = partAttrib2;
			asnLine.WN_PartAttrib3 = partAttrib3;
			asnLine.WN_SerialNumber = serialNumber;
			asnLine.WN_PackingDate = packingDate;
			asnLine.WN_ExpiryDate = expiryDate;

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			return asnLine;
		}

		ZGuid IWhsTransactionTestHelper.CreateAsnLine(ZGuid docketPK, ZGuid productPK, ZDecimal units)
		{
			var receive = Factory.Load<WhsReceive>(docketPK);

			var asnLine = receive.AsnLines.AddNew();
			asnLine.WN_LineNo = 0;
			asnLine.WN_SubLineNo = 0;
			asnLine.WN_OP = productPK;
			asnLine.WN_Quantity = units;
			asnLine.WN_PalletId = "";
			asnLine.WN_PartAttrib1 = "";
			asnLine.WN_PartAttrib2 = "";
			asnLine.WN_PartAttrib3 = "";
			asnLine.WN_SerialNumber = "";
			asnLine.WN_PackingDate = ZDate.Empty;
			asnLine.WN_ExpiryDate = ZDate.Empty;

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;

			return asnLine.PK;
		}

		#endregion

		#region CreateWhsReceiveTransportationUnit

		public BusinessObject CreateWhsReceiveTransportationUnit(ZString reference, ZGuid warehousePK, ZGuid locationPK, DateTimeOffset dateTimeOffset)
		{
			var receiveTransportationUnit = (BusinessObject)Factory.New<IWhsItemReceiveTransportationUnit>();
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber] = reference;
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = locationPK;
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse] = warehousePK;
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation] = locationPK;
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_GateInTime] = dateTimeOffset;
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime] = dateTimeOffset.AddHours(2);
			receiveTransportationUnit[WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteNotYetProcessedTime] = dateTimeOffset.AddHours(2);
			return receiveTransportationUnit;
		}

		#endregion

		#region CreateWhsItemPackageState

		public BusinessObject CreateWhsItemPackageState(string status, IWhsLocation location, BusinessObject rtu, BusinessObject package, string unitType = "PKG")
		{
			var packageState = (BusinessObject)Factory.New<IWhsItemPackageState>();
			packageState[WhsItemPackageStateSchema.WPS_Status] = status;
			packageState[WhsItemPackageStateSchema.WPS_UnitType] = unitType;
			packageState[WhsItemPackageStateSchema.WPS_WL_LastLocation] = location.PK;
			packageState[WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader] = rtu.PK;
			packageState[WhsItemPackageStateSchema.WPS_KP_Package] = package.PK;
			packageState[WhsItemPackageStateSchema.WPS_WW_Warehouse] = location.WLV_WW_Whs;
			packageState[WhsItemPackageStateSchema.WPS_UnloadedTime] = ZDateTimeOffset.Now;
			return packageState;
		}

		#endregion

		#endregion

		#region Order Docket

		#region CreateWhsOrderWithOrderLine

		public WhsOrder CreateWhsOrderWithOrderLine(OrgHeader client, IWhsWarehouse whs, OrgSupplierPart part, ZDecimal qty)
		{
			return CreateWhsOrderWithOrderLine(client, whs, "TEST", part, qty);
		}

		public WhsOrder CreateWhsOrderWithOrderLine(OrgHeader client, IWhsWarehouse whs, ZString reference, OrgSupplierPart part, ZDecimal qty, string pickOption = WhsPickOption.Codes.Auto)
		{
			return CreateWhsOrderWithOrderLine(client, whs, reference, ZDateTimeOffset.Now, part, qty, pickOption);
		}

		public WhsOrder CreateWhsOrderWithOrderLine(OrgHeader client, IWhsWarehouse whs, ZString reference, ZDateTimeOffset requiredDate, OrgSupplierPart part, ZDecimal qty, string pickOption = WhsPickOption.Codes.Auto)
		{
			WhsOrder order = CreateWhsOrder(client.PK, whs.PK, client.PK, reference, requiredDate, null, pickOption);
			CreateWhsOrderLine(order, part, qty);
			return order;
		}

		#endregion

		#region CreateWhsOrder

		public WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsOrder(client.PK, whs.PK);
		}

		public WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse whs, ZString reference, string pickOption = WhsPickOption.Codes.Auto)
		{
			return CreateWhsOrder(client.PK, whs.PK, client.PK, reference, ZDateTimeOffset.Now, null, pickOption);
		}

		public WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse whs, NotificationBuffer notify)
		{
			return CreateWhsOrder(client.PK, whs.PK, notify);
		}

		public WhsOrder CreateWhsOrder(OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsOrder(client.PK, whs.PK, @ref, notify);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsOrder(clientPK, whsPK, clientPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, NotificationBuffer notify)
		{
			return CreateWhsOrder(clientPK, whsPK, clientPK, "TEST", ZDateTimeOffset.Now, notify);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsOrder(clientPK, whsPK, clientPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref)
		{
			return CreateWhsOrder(clientPK, whsPK, consPK, @ref, ZDateTimeOffset.Now, null);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsOrder(clientPK, whsPK, consPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, ZDateTimeOffset required)
		{
			return CreateWhsOrder(clientPK, whsPK, consPK, @ref, required, null);
		}

		public WhsOrder CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consigneePK, ZString reference, ZDateTimeOffset requiredDate, NotificationBuffer notify, string pickOption = WhsPickOption.Codes.Auto)
		{
			var order = Factory.New<WhsOrder>();
			SetupWhsOrderCore(order, clientPK, whsPK, consigneePK, reference, requiredDate, notify);
			order.WD_PickOption = pickOption;
			return order;
		}

		public void SetOrderType(ZGuid orderPK, ZString orderType, bool isImportingData)
		{
			var order = Factory.Load<WhsOrder>(orderPK);
			order.WD_DocketSubType = orderType;
			order.IsImportingData = isImportingData;
		}

		#endregion

		#region SetUpWhsOrder

		public WhsOrder SetUpWhsOrder(WhsOrder doc, OrgHeader client, WhsWarehouse whs)
		{
			return SetUpWhsOrder(doc, client.PK, whs.PK);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, OrgHeader client, WhsWarehouse whs, ZString @ref)
		{
			return SetUpWhsOrder(doc, client.PK, whs.PK, @ref);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, OrgHeader client, WhsWarehouse whs, NotificationBuffer notify)
		{
			return SetUpWhsOrder(doc, client.PK, whs.PK, notify);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return SetUpWhsOrder(doc, client.PK, whs.PK, @ref, notify);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, clientPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, NotificationBuffer notify)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, clientPK, "TEST", ZDateTimeOffset.Now, notify);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, clientPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZString @ref)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, clientPK, @ref, ZDateTimeOffset.Now, null);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, consPK, @ref, ZDateTimeOffset.Now, null);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, NotificationBuffer notify)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, consPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, ZDateTimeOffset required)
		{
			return SetUpWhsOrder(doc, clientPK, whsPK, consPK, @ref, required, null);
		}

		public WhsOrder SetUpWhsOrder(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, ZDateTimeOffset required, NotificationBuffer notify)
		{
			SetupWhsOrderCore(doc, clientPK, whsPK, consPK, @ref, required, notify);
			return doc;
		}

		void SetupWhsOrderCore(WhsOrder doc, ZGuid clientPK, ZGuid whsPK, ZGuid consPK, ZString @ref, ZDateTimeOffset required, NotificationBuffer notify)
		{
			SetupDocketCore(doc, clientPK, whsPK, @ref, notify);
			doc.WD_RequiredDate = required;
			doc.ConsigneePK = consPK;
			doc.ConsigneeAddressPK = (Factory.Load<OrgHeader>(consPK)).MainAddress.PK;
		}

		#endregion

		#region CreateWhsOrderLine

		public WhsOrderLine CreateWhsOrderLine(WhsOrder doc, OrgSupplierPart part, ZDecimal units)
		{
			return CreateWhsOrderLine(doc, part.PK, units);
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder doc, OrgSupplierPart part, ZDecimal units, ZShort lineNo, ZShort subLineNo)
		{
			WhsOrderLine line = CreateWhsOrderLine(doc, part, units);
			line.WE_LineNo = lineNo;
			line.WE_SubLineNo = subLineNo;
			return line;
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZShort pickGroup)
		{
			var line = CreateWhsOrderLine(order, part, units);
			line.WE_PickGroup = pickGroup;
			return line;
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString palletID)
		{
			var line = CreateWhsOrderLine(order, part, units);
			line.WE_PalletID = palletID;
			return line;
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString inwardsEntryKey, ZString outwardsEntryKey)
		{
			return CreateWhsOrderLine(order, part, units, inwardsEntryKey, outwardsEntryKey, "");
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString inwardsEntryKey, ZString outwardsEntryKey, ZString packageGroupID)
		{
			var line = CreateWhsOrderLine(order, part.PK, units, inwardsEntryKey, outwardsEntryKey);
			line.WE_PackageGroupId = packageGroupID;
			return line;
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString inwardsEntryKey, ZString outwardsEntryKey)
		{
			return CreateWhsOrderLine(order, part.PK, units, expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, inwardsEntryKey, outwardsEntryKey, "");
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString inwardsEntryKey, ZString outwardsEntryKey, ZString palletID)
		{
			return CreateWhsOrderLine(order, part.PK, units, expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, inwardsEntryKey, outwardsEntryKey, palletID);
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder doc, ZGuid partPK, ZDecimal units)
		{
			return CreateWhsOrderLine(doc, partPK, units, "", "");
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, ZGuid partPK, ZDecimal units, ZString inwardsEntryKey, ZString outwardsEntryKey)
		{
			return CreateWhsOrderLine(order, partPK, units, ZDate.Empty, ZDate.Empty, "", "", "", inwardsEntryKey, outwardsEntryKey,"");
		}

		public WhsOrderLine CreateWhsOrderLine(WhsOrder order, ZGuid partPK, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString inwardsEntryKey, ZString outwardsEntryKey, ZString palletID)
		{
			WhsOrderLine orderLine = order.Lines.AddNew();
			orderLine.WE_OP = partPK;
			orderLine.WE_TransactionQuantity = units;
			orderLine.WE_ExpiryDate = expiryDate;
			orderLine.WE_PackingDate = packingDate;
			orderLine.WE_PartAttrib1 = partAttrib1;
			orderLine.WE_PartAttrib2 = partAttrib2;
			orderLine.WE_PartAttrib3 = partAttrib3;
			orderLine.WE_BondedEntryKey = inwardsEntryKey;
			orderLine.WE_FinalisedDate = order.WD_FinalisedDate;
			orderLine.WE_PalletID = palletID;

			SetOutwardsEntryKeyForOrderLine(orderLine, outwardsEntryKey);

			return orderLine;
		}

		public void SetOutwardsEntryKeyForOrderLine(WhsOrderLine orderLine, ZString outwardsEntryKey)
		{
			if (!outwardsEntryKey.IsEmpty)
			{
				var entry = WhsBondedWarehouseAttribute.BreakUpKey(outwardsEntryKey);
				orderLine.CustomsData.WB_EntryKey = entry.EntryKey;
				orderLine.CustomsData.WB_EntryLineNo = entry.EntryLineNo;
			}
		}

		#endregion

		#endregion

		#region CreateDefaultPrinter

		public DocumentEngineIntegration.IStmDefaultPrinter CreateDefaultPrinter(ZGuid menuItemPK, GlbStaff staff, ZGuid printerPK, ZByte numberOfCopies)
		{
			var stmMenuItem = Factory.Load<IStmMenuItem>(menuItemPK);
			var defaultPrinter = StmDefaultPrinter.LoadOrCreateDefaultPrinter(Factory, staff, stmMenuItem);
			defaultPrinter.SDP_SQ_Printer = printerPK;
			defaultPrinter.SDP_NumberOfCopies = numberOfCopies;
			return defaultPrinter;
		}

		#endregion

		#region CreateServiceAreaForVASOrder

		public WhsArea CreateServiceAreaForVASOrder(WhsWarehouse warehouse, short cols = 1, short levels = 1, short trays = 1, string areaName = "SERVICE AREA", string rowCode = "SERVICEROW")
		{
			var serviceArea = CreateArea(warehouse, areaName);
			var row = CreateRowAndGenerateLocations(warehouse, rowCode, cols, levels, trays);
			row.Locations.ForEach(l => l.WLV_WA_PickingArea = serviceArea.PK);
			row.Locations.ForEach(l => l.WLV_WA_PutawayArea = serviceArea.PK);

			return serviceArea;
		}

		#endregion

		#region CreateWhsAdHocServiceJob

		public WhsAdHocServiceJob CreateWhsAdHocServiceJob(WhsWarehouse warehouse, OrgHeader client, ZDateTime billingDate, string customReferenceNo = "", bool finalised = false)
		{
			Argument.NotNull(warehouse, "warehouse");
			Argument.NotNull(client, "client");

			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_WW_Whs = warehouse.PK;
			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.BillingDate = billingDate;
			adHocServiceJob.WSJ_CustomerReference = customReferenceNo;
			adHocServiceJob.WSJ_IsFinalised = finalised;
			var jobHeader = adHocServiceJob.JobHeader;

			return adHocServiceJob;
		}

		#endregion

		#region CreateWhsVASOrder

		public WhsVASOrder CreateWhsVASOrder(WhsArea serviceArea, OrgHeader client)
		{
			Argument.NotNull(serviceArea, "serviceArea");
			Argument.NotNull(client, "client");

			var vasOrder = Factory.New<WhsVASOrder>();

			using (vasOrder.SuspendMarkingAsNeedingValidation())
			using (vasOrder.SuspendSettingHasChanges())
			{
				vasOrder.WVO_WA_ServiceArea = serviceArea.PK;
				vasOrder.WVO_OH_Client = client.PK;
			}

			return vasOrder;
		}

		#endregion

		#region CreateWhsVASOrderLine

		public WhsVASOrderLine CreateWhsVASOrderLine(WhsVASOrder vasOrder, OrgSupplierPart part, decimal quantity, ZDate? packingDate = null, ZDate? expiryDate = null)
		{
			Argument.NotNull(vasOrder, "vasOrder");
			Argument.NotNull(part, "part");

			var line = vasOrder.Lines.AddNew();
			line.WVL_OP_Product = part.PK;
			line.WVL_Quantity = quantity;
			line.WVL_PackingDate = packingDate ?? ZDate.Empty;
			line.WVL_ExpiryDate = expiryDate ?? ZDate.Empty;

			return line;
		}

		#endregion

		#region CreateWhsVASOrderWithLine

		public WhsVASOrder CreateWhsVASOrderWithLine(WhsArea serviceArea, OrgHeader client, OrgSupplierPart part, decimal quantity, ZDate? packingDate = null, ZDate? expiryDate = null, bool createTransferAndFinalise = false)
		{
			Argument.NotNull(serviceArea, "serviceArea");
			Argument.NotNull(client, "client");
			Argument.NotNull(part, "part");

			var vasOrder = CreateWhsVASOrder(serviceArea, client);
			CreateWhsVASOrderLine(vasOrder, part, quantity, packingDate, expiryDate);
			Factory.Save();

			if (createTransferAndFinalise)
			{
				var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
				initialTransfer.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransfer);

				vasOrder.MarkVASOrderAsCompleted(Notify);
				Factory.Save();
				AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

				vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder);
				Factory.Save();
			}

			return vasOrder;
		}

		#endregion

		#region Packages

		public PkgPackage CreatePackage(string packType, string packageID, PkgPackageCollection packages, string goodsDesc = "")
		{
			var package = packages.AddNew(packType);
			package.KP_PackageID = packageID;
			package.KP_GoodsDescription = goodsDesc;
			return package;
		}

		public void DepartPackageNow(WhsLoadPkgPackagePivot loadPkgPackagePivot)
		{
			var now = ZDateTimeOffset.Now;
			if (loadPkgPackagePivot.Load.WLO_StartTime.IsEmpty)
			{
				loadPkgPackagePivot.Load.WLO_TransportationUnitNumber = "1";
				loadPkgPackagePivot.Load.WLO_StartTime = now;
			}
			loadPkgPackagePivot.Load.WLO_GateOutTime = now;
			loadPkgPackagePivot.Load.WLO_CompleteTime = now;
		}

		public WhsLoadPkgPackagePivot CreateLoadPkgPackagePivot(ZGuid packagePK, WhsLoad load)
		{
			var loadPkgPackagePivot = Factory.New<WhsLoadPkgPackagePivot>();
			loadPkgPackagePivot.WLP_KP_Package = packagePK;
			loadPkgPackagePivot.WLP_WLO_Load = load.PK;

			return loadPkgPackagePivot;
		}

		#endregion

		#region PackageAudit

		public WhsPackageAudit CreateWhsPackageAudit(WhsOrder order)
		{
			var audit = Factory.New<WhsPackageAudit>();
			audit.WPA_WD_Order = order.PK;
			return audit;
		}

		public WhsPackageAudit CreateWhsPackageAudit(WhsOrder order, ZString packageNum, ZDateTimeOffset? completeTime = null)
		{
			var audit = CreateWhsPackageAudit(order);
			audit.WPA_PackageID = packageNum;
			audit.WPA_AuditCompleteTime = completeTime ?? ZDateTimeOffset.Now;
			return audit;
		}

		public WhsPackageAudit CreateWhsPackageAudit(PkgPackage package, ZDateTimeOffset? completeTime = null)
		{
			WhsPackageAudit audit = null;
			var packingParent = package.PackageJob?.ParentJob;
			if (packingParent?.GetType() == typeof(WhsOrder))
			{
				audit = CreateWhsPackageAudit((WhsOrder)packingParent, package.KP_PackageID, completeTime);
			}
			return audit;
		}

		#endregion

		#region PackageAuditWithLineFailure

		public WhsPackageAudit CreateWhsPackageAuditWithLineFailure(WhsOrder order, ZString packageNum, OrgSupplierPart part, decimal expected, decimal audited, ZDateTimeOffset? completeTime = null)
		{
			var audit = CreateWhsPackageAudit(order, packageNum, completeTime);
			CreateWhsPackageAuditLineFailure(audit, part, expected, audited);
			return audit;
		}

		public WhsPackageAudit CreateWhsPackageAuditWithLineFailure(PkgPackage package, OrgSupplierPart part, decimal expected, decimal audited, ZDateTimeOffset? completeTime = null)
		{
			var audit = CreateWhsPackageAudit(package, completeTime);
			CreateWhsPackageAuditLineFailure(audit, part, expected, audited);
			return audit;
		}

		#endregion

		#region PackageAuditLineFailure

		public WhsPackageAuditLineFailure CreateWhsPackageAuditLineFailure(WhsPackageAudit audit, OrgSupplierPart part, decimal expected, decimal audited)
		{
			WhsPackageAuditLineFailure auditFailure = null;
			if (audit != null)
			{
				auditFailure = audit.PackageAuditFailureLines.AddNew();
				auditFailure.WPF_OP = part.PK;
				auditFailure.WPF_ExpectedQty = expected;
				auditFailure.WPF_AuditedQty = audited;
			}
			return auditFailure;
		}

		#endregion

		#region CreateWhsPutawayJob

		public WhsPutawayJob CreateWhsPutawayJob(WhsWarehouse warehouse, GlbStaff staff)
		{
			Argument.NotNull(warehouse, "warehouse");
			Argument.NotNull(staff, "staff");

			var putawayJob = warehouse.Factory.New<WhsPutawayJob>();
			putawayJob.WPJ_WW_Warehouse = warehouse.PK;
			putawayJob.WPJ_GS_NKUser = staff.GS_Code;

			return putawayJob;
		}

		#endregion

		#region CreateWhsPutawayLine

		public WhsPutawayLine CreateWhsPutawayLine(WhsPutawayJob parentJob, ZString palletID, bool isPuttingAway = false, bool isFinalised = false)
		{
			Argument.NotNull(parentJob, "parentJob");
			Argument.NotNullOrEmpty(palletID, "palletID");

			var putawayLine = parentJob.Factory.New<WhsPutawayLine>();
			putawayLine.WPL_WPJ_PutawayJob = parentJob.PK;
			putawayLine.WPL_PalletID = palletID;
			putawayLine.WPL_IsPuttingAway = isPuttingAway;
			putawayLine.WPL_IsFinalized = isFinalised;

			return putawayLine;
		}

		#endregion

		#region Cartage Job

		public ICommonCartage CreateCartageJob(WhsOrder order)
		{
			Argument.NotNull(order, "order");

			ICommonCartage result = Factory.New<ICommonCartage>();

			BusinessObject cartageJob = (BusinessObject)result;
			cartageJob.FillWithValidTestData();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageJob[JobCartageSchema.JJ_ParentTableCode] = order.TablePrefix;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = order.WD_DocketID;

			return result;
		}

		/// <summary>
		/// Create a Cartage job for the pick.CurrentOrder.
		/// </summary>
		public ICommonCartage CreateCartageJob(WhsPick pick)
		{
			Argument.NotNull(pick.CurrentOrder, "pick.CurrentOrder");
			return CreateCartageJob(pick.CurrentOrder);
		}

		#endregion

		#region Work Order Docket

		public WhsWorkOrder CreateWhsWorkOrderWithLine(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, ZDecimal qty)
		{
			return CreateWhsWorkOrderWithLine(client, whs, "", part, qty);
		}

		public WhsWorkOrder CreateWhsWorkOrderWithLine(OrgHeader client, WhsWarehouse whs, ZString externalRef, OrgSupplierPart part, ZDecimal qty)
		{
			return CreateWhsWorkOrderWithLine(client, whs, externalRef, WorkOrderType.Codes.Assemble, part, qty);
		}

		public WhsWorkOrder CreateWhsWorkOrderWithLine(OrgHeader client, WhsWarehouse whs, ZString externalRef, ZString workOrderType, OrgSupplierPart part, ZDecimal qty)
		{
			var order = CreateWhsWorkOrder(client.PK, whs.PK, externalRef, workOrderType);
			CreateWhsWorkOrderLine(order, part, qty);
			return order;
		}

		public WhsWorkOrder CreateWhsWorkOrder(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsWorkOrder(client.PK, whs.PK);
		}

		public WhsWorkOrder CreateWhsWorkOrder(OrgHeader client, WhsWarehouse whs, ZString @ref, string workOrderType = WorkOrderType.Codes.Assemble)
		{
			return CreateWhsWorkOrder(client.PK, whs.PK, @ref, workOrderType);
		}

		public WhsWorkOrder CreateWhsWorkOrder(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsWorkOrder(clientPK, whsPK, "TEST");
		}

		public WhsWorkOrder CreateWhsWorkOrder(ZGuid clientPK, ZGuid whsPK, ZString @ref, string workOrderType = WorkOrderType.Codes.Assemble)
		{
			return CreateWhsWorkOrder(clientPK, whsPK, @ref, null, workOrderType);
		}

		public WhsWorkOrder CreateWhsWorkOrder(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify, string workOrderType = WorkOrderType.Codes.Assemble)
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			SetupWhsComponentOrderCore(workOrder, clientPK, whsPK, @ref, notify);
			workOrder.WD_DocketSubType = workOrderType;
			return workOrder;
		}

		void SetupWhsComponentOrderCore(WhsComponentOrder doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			doc.WD_RequiredDate = ZDateTimeOffset.Now;
			doc.ConsigneeAddressPK = Factory.Load<OrgHeader>(clientPK).MainAddress.PK;
			SetupDocketCore(doc, clientPK, whsPK, @ref, notify);
		}

		public WhsWorkOrderLine CreateWhsWorkOrderLine(WhsWorkOrder workOrder, OrgSupplierPart part, ZDecimal units)
		{
			return CreateWhsWorkOrderLine(workOrder, part.PK, units);
		}

		public WhsWorkOrderLine CreateWhsWorkOrderLine(WhsWorkOrder workOrder, ZGuid partPK, ZDecimal units)
		{
			return (WhsWorkOrderLine)CreateWhsPickableDocketLine(workOrder, partPK, units);
		}

		public WhsWorkOrderLine CreateWhsWorkOrderLine(WhsWorkOrder workOrder, OrgSupplierPart part, ZDecimal units, ZString packType)
		{
			return (WhsWorkOrderLine)CreateWhsPickableDocketLine(workOrder, part.PK, units, packType);
		}

		#endregion

		#region Dynamic Work Order Docket

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrder(IOrgHeader client, IWhsWarehouse whs)
			=> CreateWhsDynamicWorkOrder(client, whs, "");

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrder(IOrgHeader client, IWhsWarehouse whs, ZString @ref)
			=> CreateWhsDynamicWorkOrder(client.PK, whs.PK, @ref, null);

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrder(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify, string dynamicWorkOrderType = WorkOrderType.Codes.Assemble)
		{
			var dynamicWorkOrder = Factory.New<WhsDynamicWorkOrder>();
			SetupWhsComponentOrderCore(dynamicWorkOrder, clientPK, whsPK, @ref, notify);
			dynamicWorkOrder.WD_DocketSubType = dynamicWorkOrderType;
			return dynamicWorkOrder;
		}

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrderWithLine(IOrgHeader client, IWhsWarehouse whs, IOrgSupplierPart part, ZDecimal qty)
			=> CreateWhsDynamicWorkOrderWithLine(client, whs, "", part, qty);

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrderWithLine(IOrgHeader client, IWhsWarehouse whs, ZString externalRef, IOrgSupplierPart part, ZDecimal qty)
			=> CreateWhsDynamicWorkOrderWithLine(client, whs, externalRef, WorkOrderType.Codes.Assemble, part, qty);

		public WhsDynamicWorkOrder CreateWhsDynamicWorkOrderWithLine(IOrgHeader client, IWhsWarehouse whs, ZString externalRef, ZString dynamicWorkOrderType, IOrgSupplierPart part, ZDecimal qty)
		{
			var order = CreateWhsDynamicWorkOrder(client.PK, whs.PK, externalRef, null, dynamicWorkOrderType);
			CreateWhsDynamicWorkOrderLine(order, part, qty);
			return order;
		}

		public WhsDynamicWorkOrderLine CreateWhsDynamicWorkOrderLine(WhsDynamicWorkOrder dynamicWorkOrder, IOrgSupplierPart part, ZDecimal units)
			=> CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, part.PK, units);

		public WhsDynamicWorkOrderLine CreateWhsDynamicWorkOrderLine(WhsDynamicWorkOrder dynamicWorkOrder, ZGuid partPK, ZDecimal units)
			=> (WhsDynamicWorkOrderLine)CreateWhsPickableDocketLine(dynamicWorkOrder, partPK, units);

		public WhsDynamicWorkOrderLine CreateWhsDynamicWorkOrderLine(WhsDynamicWorkOrder dynamicWorkOrder, IOrgSupplierPart part, ZDecimal units, ZString packType)
			=> (WhsDynamicWorkOrderLine)CreateWhsPickableDocketLine(dynamicWorkOrder, part.PK, units, packType);

		#endregion

		#region Pickable Docket

		public WhsPickableDocketLine CreateWhsPickableDocketLine(WhsPickableDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			return CreateWhsPickableDocketLine(docket, part.PK, units);
		}

		public WhsPickableDocketLine CreateWhsPickableDocketLine(WhsPickableDocket docket, ZGuid partPK, ZDecimal units)
		{
			var part = Factory.Load<OrgSupplierPart>(partPK);
			return CreateWhsPickableDocketLine(docket, partPK, units, part != null ? part.OP_StockKeepingUnit : ZString.Empty);
		}

		public WhsPickableDocketLine CreateWhsPickableDocketLine(WhsPickableDocket docket, ZGuid partPK, ZDecimal units, ZString packType)
		{
			var line = docket.Lines.AddNew();
			line.WE_OP = partPK;
			line.WE_TransactionQuantity = units;
			line.WE_F3_NKPackType = packType;
			if (docket.WD_DocketStatus == DocketStatus.Codes.Finalised)
			{
				foreach (var pickableLine in docket.AllLines)
				{
					pickableLine.WE_DocketLineStatus = DocketLineStatus.Codes.Finalised;
				}
			}
			if (!docket.WD_FinalisedDate.IsEmpty)
			{
				foreach (var pickableLine in docket.AllLines)
				{
					pickableLine.WE_FinalisedDate = docket.WD_FinalisedDate;
				}
			}

			return line;
		}

		#endregion

		#region Adjustment

		public WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse whs, ZString @ref)
		{
			return CreateWhsAdjustment(client.PK, whs.PK, @ref, null);
		}

		public WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(client.PK, whs.PK, @ref, notify);
		}

		public WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse whs, ZString reference, NotificationBuffer notify, OrgHeader newClient)
		{
			return CreateWhsAdjustment(client.PK, whs.PK, reference, ZDateTimeOffset.Now, notify, newClient.PK);
		}

		public WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsAdjustment(client.PK, whs.PK);
		}

		public WhsAdjustment CreateWhsAdjustment(OrgHeader client, WhsWarehouse whs, OrgHeader newClient, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(client.PK, whs.PK, newClient.PK, notify);
		}

		public WhsAdjustment CreateWhsAdjustment(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsAdjustment(clientPK, whsPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsAdjustment CreateWhsAdjustment(ZGuid clientPK, ZGuid whsPK, ZGuid newClientPK, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(clientPK, whsPK, "TEST", ZDateTimeOffset.Now, notify, newClientPK);
		}

		public WhsAdjustment CreateWhsAdjustment(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(clientPK, whsPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsAdjustment CreateWhsAdjustment(ZGuid clientPK, ZGuid whsPK, ZString reference, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(clientPK, whsPK, reference, booking, notify, ZGuid.Empty);
		}

		public WhsAdjustment CreateWhsAdjustment(ZGuid clientPK, ZGuid whsPK, ZString reference, ZDateTimeOffset booking, NotificationBuffer notify, ZGuid newClientPK)
		{
			var adjustment = Factory.New<WhsAdjustment>();
			SetupWhsAdjustmentCore(adjustment, clientPK, whsPK, reference, booking, notify);

			if (newClientPK != ZGuid.Empty)
			{
				adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;

				var childAdjustment = Factory.New<WhsAdjustment>();
				childAdjustment.WD_WD_ParentDocket = adjustment.PK;
				adjustment.ChildAdjustment.WD_OH_Client = newClientPK;
				childAdjustment.WD_WW_Whs = whsPK;
				childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			}

			return adjustment;
		}

		void SetupWhsAdjustmentCore(WhsAdjustment doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			SetupDocketCore(doc, clientPK, whsPK, @ref, notify);
			doc.WD_BookingDate = booking;
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, ZDecimal units, WhsLocation location)
		{
			return CreateWhsAdjustmentLine(adjustment, part.PK, units, location.ToLocationString());
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString heldCode)
		{
			return CreateWhsAdjustmentLine(adjustment, part.PK, units, location.ToLocationString(), "", ZDateTimeOffset.Now, heldCode, "", "", "", "", ZDate.Empty, ZDate.Empty);
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZString bondedEntryKey, ZString packageGroupId, ZDecimal perPackageQty)
		{
			WhsAdjustmentLine line = CreateWhsAdjustmentLine(adjustment, part, units, location);
			line.WE_PackageGroupId = packageGroupId;
			line.WE_BondedEntryKey = bondedEntryKey;
			line.WE_PerPackageQty = perPackageQty;

			return line;
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, ZDecimal units, string location, string palletID = "")
		{
			return CreateWhsAdjustmentLine(adjustment, part.PK, units, location, palletID);
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, ZDecimal units, ZGuid locationPK)
		{
			return CreateWhsAdjustmentLine(adjustment, part.PK, units, LocnToString(locationPK));
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, ZGuid partPK, ZDecimal units, string location, string palletID, ZDateTimeOffset? arrival = null)
		{
			return CreateWhsAdjustmentLine(adjustment, partPK, units, location, palletID, arrival, "", "", "", "", "", ZDate.Empty, ZDate.Empty);
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, ZGuid partPK, ZDecimal units, string location, ZDateTimeOffset? arrival = null)
		{
			return CreateWhsAdjustmentLine(adjustment, partPK, units, location, arrival, "");
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, ZGuid partPK, ZDecimal units, string location, ZDateTimeOffset? arrival, ZString heldCode)
		{
			return CreateWhsAdjustmentLine(adjustment, partPK, units, location, "", arrival, heldCode, "", "", "", "", ZDate.Empty, ZDate.Empty);
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, ZGuid partPK, ZDecimal units, string location, string pa1, string pa2, string pa3, string serialNumber, ZDate expiryDate, ZDate packingDate)
		{
			return CreateWhsAdjustmentLine(adjustment, partPK, units, location, "", ZDateTimeOffset.Now, "", pa1, pa2, pa3, serialNumber, expiryDate, packingDate);
		}

		public WhsAdjustmentLine CreateWhsAdjustmentLine(WhsAdjustment adjustment, ZGuid partPK, ZDecimal units, string location, string palletID, ZDateTimeOffset? arrival, ZString heldCode, string pa1, string pa2, string pa3, string serialNumber, ZDate expiryDate, ZDate packingDate)
		{
			var line = adjustment.Lines.AddNew();
			line.WE_OP = partPK;
			line.WE_TransactionQuantity = units;
			line.WE_LineComment = "Comment";
			line.WE_ReasonCode = adjustment.WD_DocketSubType == AdjustmentType.Codes.OwnershipAdjustment ? "OCH" : "CLI";
			line.LocationString = location;
			line.WE_PalletID = palletID;
			line.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
			line.WE_PartAttrib1 = pa1;
			line.WE_PartAttrib2 = pa2;
			line.WE_PartAttrib3 = pa3;
			line.WE_SerialNumber = serialNumber;
			line.WE_ExpiryDate = expiryDate;
			line.WE_PackingDate = packingDate;
			line.WE_AdjustmentArrivalDate = arrival ?? GetValidAdjustmentArrivalDate(line);

			return line;
		}

		#endregion

		#region Transfer

		// create transfer

		public WhsTransfer CreateWhsTransfer(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsTransfer(client.PK, whs.PK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsTransfer CreateWhsTransfer(OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify = null, string transferType = null)
		{
			var result = CreateWhsTransfer(client.PK, whs.PK, @ref, ZDateTimeOffset.Now, notify);
			if (transferType != null)
			{
				result.WD_DocketSubType = transferType;
			}

			return result;
		}

		public WhsTransfer CreateWhsTransfer(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsTransfer(clientPK, whsPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsTransfer CreateWhsTransfer(ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			return CreateWhsTransfer(clientPK, whsPK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsTransfer CreateWhsTransfer(OrgHeader client, WhsWarehouse whs, ZString reference, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			var transfer = Factory.New<WhsTransfer>();
			SetupWhsTransferCore(transfer, client.PK, whs.PK, reference, booking, notify);
			return transfer;
		}

		public WhsTransfer CreateWhsTransfer(ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			WhsTransfer doc = Factory.New<WhsTransfer>();
			SetupWhsTransferCore(doc, clientPK, whsPK, @ref, booking, notify);
			return doc;
		}

		// setup transfer

		public WhsTransfer SetupWhsTransfer(WhsTransfer doc, OrgHeader client, WhsWarehouse whs, ZString @ref)
		{
			return SetupWhsTransfer(doc, client.PK, whs.PK, @ref, ZDateTimeOffset.Now, null);
		}

		public WhsTransfer SetupWhsTransfer(WhsTransfer doc, OrgHeader client, WhsWarehouse whs, ZString @ref, NotificationBuffer notify)
		{
			return SetupWhsTransfer(doc, client.PK, whs.PK, @ref, ZDateTimeOffset.Now, notify);
		}

		public WhsTransfer SetupWhsTransfer(WhsTransfer doc, ZGuid clientPK, ZGuid whsPK)
		{
			return SetupWhsTransfer(doc, clientPK, whsPK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsTransfer SetupWhsTransfer(WhsTransfer doc, OrgHeader client, WhsWarehouse whs)
		{
			return SetupWhsTransfer(doc, client.PK, whs.PK, "TEST", ZDateTimeOffset.Now, null);
		}

		public WhsTransfer SetupWhsTransfer(WhsTransfer doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			SetupWhsTransferCore(doc, clientPK, whsPK, @ref, booking, notify);
			return doc;
		}

		void SetupWhsTransferCore(WhsTransfer doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, ZDateTimeOffset booking, NotificationBuffer notify)
		{
			SetupDocketCore(doc, clientPK, whsPK, @ref, notify);
			doc.WD_BookingDate = booking;
		}

		// create transfer line

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, WhsLocation sourceLocation, ZString pickedBy)
		{
			var transferLine = CreateWhsTransferLine(transfer, part, 10m, sourceLocation.ToLocationString(), "");
			transferLine.GS_NKPickedBy = pickedBy;

			return transferLine;
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, WhsLocation sourceLocation, WhsLocation destLocation)
		{
			return CreateWhsTransferLine(transfer, part.PK, units, sourceLocation.ToLocationString(), destLocation.Row.WR_WW_Whs, destLocation.ToLocationString());
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, WhsLocation sourceLocation, ZString sourcePalletId, WhsLocation destLocation,
			ZDate expiryDate, ZDate packingDate, ZString attribute1, ZString attribute2, ZString attribute3)
		{
			return CreateWhsTransferLine(transfer, part.PK, units, sourceLocation.ToLocationString(), sourcePalletId, ZGuid.Empty, destLocation.ToLocationString(), "",
				ZDateTimeOffset.Empty, expiryDate, packingDate, attribute1, attribute2, attribute3);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, WhsLocation sourceLocation, WhsLocation destLocation, GlbStaff pickedBy)
		{
			var transferLine = CreateWhsTransferLine(transfer, part.PK, units, sourceLocation.ToLocationString(), destLocation.Row.WR_WW_Whs, destLocation.ToLocationString());
			transferLine.GS_NKPickedBy = pickedBy != null ? pickedBy.GS_Code : ZString.Empty;
			return transferLine;
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, WhsLocation sourceLocation, WhsLocation destLocation, ZString bondedEntryKey)
		{
			var transferLine = CreateWhsTransferLine(transfer, part, units, sourceLocation, destLocation);

			if (!bondedEntryKey.IsEmpty)
			{
				var entry = WhsBondedWarehouseAttribute.BreakUpKey(bondedEntryKey);
				transferLine.CustomsData.WB_EntryKey = entry.EntryKey;
				transferLine.CustomsData.WB_EntryLineNo = entry.EntryLineNo;
			}

			transferLine.WE_BondedEntryKey = bondedEntryKey;

			return transferLine;
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, WhsLocation sourceLocation, WhsLocation destLocation, ZString bondedEntryKey, ZString packageGroupID)
		{
			var transferLine = CreateWhsTransferLine(transfer, part, units, sourceLocation, destLocation, bondedEntryKey);
			transferLine.WE_PackageGroupId = packageGroupID;

			return transferLine;
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, ZString sourceLocation, ZString destLocation, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, part, units, sourceLocation, "", destLocation, "", heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, ZString sourceLocation, ZString sourcePalletID, ZString destLocation, ZString destPalletID, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, part.PK, units, sourceLocation, sourcePalletID, transfer.WD_WW_Whs, destLocation, destPalletID, ZDateTimeOffset.Empty, heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, ZString sourceLocation, ZGuid secondWhsPK, ZString destLocation, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, part.PK, units, sourceLocation, secondWhsPK, destLocation, heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, OrgSupplierPart part, ZDecimal units, ZGuid sourceLocationPK, ZGuid destLocationPK)
		{
			return CreateWhsTransferLine(transfer, part.PK, units, LocnToString(sourceLocationPK), LocnToString(destLocationPK));
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, ZGuid partPK, ZDecimal units, ZString sourceLocation, ZString destLocation, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, partPK, units, sourceLocation, "", transfer.WD_WW_Whs, destLocation, "", ZDateTimeOffset.Empty, heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, ZGuid partPK, ZDecimal units, ZString sourceLocation, ZGuid secondWhsPK, ZString destLocation, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, partPK, units, sourceLocation, "", secondWhsPK, destLocation, "", ZDateTimeOffset.Empty, heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, ZGuid partPK, ZDecimal units, ZString sourceLocation, ZString sourcePalletID, ZGuid secondWhsPK, ZString destLocation, ZString destPalletID,
			ZDateTimeOffset arrivalDate, string heldCode = "")
		{
			return CreateWhsTransferLine(transfer, partPK, units, sourceLocation, sourcePalletID, secondWhsPK, destLocation, destPalletID, arrivalDate,
				ZDate.Empty, ZDate.Empty, "", "", "", heldCode);
		}

		public WhsTransferLine CreateWhsTransferLine(WhsTransfer transfer, ZGuid partPK, ZDecimal units, ZString sourceLocation, ZString sourcePalletID, ZGuid secondWhsPK, ZString destLocation, ZString destPalletID,
			ZDateTimeOffset arrivalDate, ZDate expiryDate, ZDate packingDate, ZString attribute1, ZString attribute2, ZString attribute3, string heldCode = "")
		{
			var transferLine = transfer.Lines.AddNew();
			transferLine.WE_OP = partPK;
			transferLine.WE_TransactionQuantity = units;
			transferLine.WE_AdjustmentArrivalDate = arrivalDate;
			transferLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;

			var sourceWhsPK = (transfer.WD_DocketSubType == TransferType.Codes.InterWhsDest) ? secondWhsPK : transfer.WD_WW_Whs;
			secondWhsPK = (transfer.WD_DocketSubType == TransferType.Codes.InterWhsDest) ? transfer.WD_WW_Whs : secondWhsPK;

			transferLine.TransferFromWarehousePK = sourceWhsPK;
			transferLine.WE_TransferFromPalletId = sourcePalletID;
			transferLine.TransferFromLocationString = sourceLocation;

			transferLine.DestinationWarehousePK = secondWhsPK;
			transferLine.WE_PalletID = destPalletID;
			transferLine.LocationString = destLocation;
			transferLine.WE_ExpiryDate = expiryDate;
			transferLine.WE_PackingDate = packingDate;
			transferLine.WE_PartAttrib1 = attribute1;
			transferLine.WE_PartAttrib2 = attribute2;
			transferLine.WE_PartAttrib3 = attribute3;

			return transferLine;
		}

		public WhsTransferLine CreateWhsTransferLineWithInTransitInventory(WhsTransfer transfer, OrgSupplierPart part, decimal units, WhsLocation locationFrom, ZString palletFrom, WhsLocation locationTo, ZString palletTo, GlbStaff pickedBy, ZDateTimeOffset? pickedTime = null)
		{
			var transferLine = CreateWhsTransferLine(transfer, part, units, locationFrom.WLV_LocationString, palletFrom, locationTo.WLV_LocationString, palletTo);
			transferLine.GS_NKPickedBy = pickedBy.GS_Code;
			transferLine.PickedTime = pickedTime ?? ZDateTimeOffset.Now; // Will create In-Transit Inventory
			AssertEquals("Precondition: Inventory must be set In Transit.", InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);

			return transferLine;
		}

		public WhsTransferLine PickAndMakeInTransitTransfer(WhsPickLine pickLine, ZDateTimeOffset pickedTime, bool allowMultipleSteps = false)
		{
			var originalInventoryLine = pickLine.InventoryLine;

			if (pickLine.IsPicked)
			{
				throw new InvalidOperationException("Attempt to pick and make In-Transit transfer for an already picked line!");
			}
			else if (originalInventoryLine.WE_OriginalInventoryStatus == InventoryStatus.Codes.InTransit)
			{
				throw new InvalidOperationException("Attempt to pick and make In-Transit transfer for inventory already In-Transit!");
			}
			else if (!allowMultipleSteps && pickLine.WZ_WE_OriginalPickedInventoryLine.IsValid)
			{
				throw new InvalidOperationException("Attempt to make In-Transit Transfer for a PickLine with OriginalPickedInventoryLine already set!");
			}

			var pick = pickLine.Pick;
			var transactionLine = pickLine.DocketLine;
			var client = transactionLine.Docket.Client;
			var warehouse = transactionLine.Warehouse;
			var inventory = pickLine.Inventory;

			pickLine.WZ_PickedDateTime = pickedTime;
			ObjectFactory.Get<IOutboundDockDoorTransferCreator>().CreateOutboundDockDoorTransfer(pickLine);

			var newTransferLine = (WhsTransferLine)pickLine.InventoryLine;
			AssertEquals("Precondition: In-Transit.", InventoryStatus.Codes.InTransit, newTransferLine.WE_CurrentInventoryStatus);

			if (newTransferLine.WE_WL.IsEmpty)
			{
				newTransferLine.WE_WL = warehouse.WW_DefaultOutboundDockDoor;
			}

			return newTransferLine;
		}

		#endregion

		#region OrgAddress

		public OrgAddress SetUpOrgAddress(string address1, string address2, string postCode, string city, string state, string portCode, IOrgHeader owner)
		{
			var address = OrgAddress.New(Factory);
			address.Address1 = address1;
			address.Address2 = address2;
			address.Postcode = postCode;
			address.City = city;
			address.State = state;
			address.OA_RL_NKRelatedPortCode = portCode;
			address.OA_OH = owner.PK;
			return address;
		}

		IOrgAddress IWhsTransactionTestHelper.SetUpOrgAddress(string address1, string address2, string postCode, string city, string state, string portCode, IOrgHeader owner)
		{
			return SetUpOrgAddress(address1, address2, postCode, city, state, portCode, owner);
		}

		#endregion

		#region RateTransportZone

		public RateTransportProvider SetUpRateTransportProvider(bool isActive, string zoneType, string country, string zoneMode, OrgHeader relatedParty)
		{
			var result = Factory.New<RateTransportProvider>();
			result.TP_IsActive = isActive;
			result.TP_ZoneType = zoneType;
			result.TP_RN_NKCountry = country;
			result.TP_ZoneMode = zoneMode;
			result.TP_OH_RelatedParty = relatedParty.PK;
			return result;
		}

		public RateTransportZone SetUpRateTransportZone(RateTransportProvider provider, bool isActive, string zoneName)
		{
			var result = provider.Zones.AddNew();
			result.TZ_IsActive = isActive;
			result.TZ_ZoneName = zoneName;
			return result;
		}

		public RateTransportZoneItem SetUpRateTransportZoneItem(RateTransportZone zone, string country, string fromPostCode)
		{
			var result = zone.Items.AddNew();
			result.TQ_FromPostCode = fromPostCode;
			result.TQ_RN_NKCountry = country;
			return result;
		}

		#endregion

		#region Cycle Count

		ZGuid IWhsTransactionTestHelper.CreateWhsCycleCountLocation(ZGuid locationPK, ZString granularity, ZString taskPlanningStatus)
		{
			var cycleCount = CreateWhsCycleCountLocation(Factory, locationPK, granularity, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
			cycleCount.WCL_TaskPlanningStatus = taskPlanningStatus;
			return cycleCount.PK;
		}

		public WhsCycleCountLocation CreateWhsCycleCountLocation(WhsLocation location, ZString granularity)
		{
			return CreateWhsCycleCountLocation(location, granularity, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, "");
		}

		public WhsCycleCountLocation CreateWhsCycleCountLocation(WhsLocation location, ZString granularity, ZDateTimeOffset startTime, ZDateTimeOffset endTime, string assignedTo = "", WhsCycleCountLocation rejectedCycleCount = null, byte priority = 0)
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			cycleCount.WCL_WL_Location = location.PK;
			cycleCount.WCL_Granularity = granularity;
			cycleCount.WCL_StartTime = startTime;
			cycleCount.WCL_EndTime = endTime;
			cycleCount.WCL_GS_NKAssignedTo = assignedTo;
			cycleCount.WCL_WCL_RejectedCycleCount = rejectedCycleCount == null ? ZGuid.Empty : rejectedCycleCount.PK;
			cycleCount.WCL_Priority = priority;

			return cycleCount;
		}

		public WhsCycleCountLocation CreateWhsCycleCountLocation(BusinessObjectFactory factory, ZGuid locationPK, ZString granularity, ZDateTimeOffset startTime, ZDateTimeOffset endTime, string assignedTo = "", WhsCycleCountLocation rejectedCycleCount = null, byte priority = 0)
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			cycleCount.WCL_WL_Location = locationPK;
			cycleCount.WCL_Granularity = granularity;
			cycleCount.WCL_StartTime = startTime;
			cycleCount.WCL_EndTime = endTime;
			cycleCount.WCL_GS_NKAssignedTo = assignedTo;
			cycleCount.WCL_WCL_RejectedCycleCount = rejectedCycleCount == null ? ZGuid.Empty : rejectedCycleCount.PK;
			cycleCount.WCL_Priority = priority;

			return cycleCount;
		}

		public WhsCycleCountLocationVariance CreateWhsCycleCountLocationVariance(WhsCycleCountLocation cycleCount, ZString status, bool isCountingPalletsOnly, ZDecimal varianceQty, decimal expectedQty = 0, string authorizedAction = "")
		{
			return CreateWhsCycleCountLocationVariance(cycleCount, status, palletID: "", client: null, part: null, varianceQty: varianceQty, expectedQty: expectedQty, isCountingPalletsOnly: isCountingPalletsOnly, authorizedAction: authorizedAction);
		}

		public WhsCycleCountLocationVariance CreateWhsCycleCountLocationVariance(WhsCycleCountLocation cycleCount, ZString status, OrgHeader client, OrgSupplierPart part, decimal varianceQty, decimal expectedQty = 0, string authorizedAction = "")
		{
			return CreateWhsCycleCountLocationVariance(cycleCount, status, palletID: "", client: client, part: part, varianceQty: varianceQty, expectedQty: expectedQty, authorizedAction: authorizedAction);
		}

		public WhsCycleCountLocationVariance CreateWhsCycleCountLocationVariance(WhsCycleCountLocation cycleCount, ZString status, ZString palletID, OrgHeader client, OrgSupplierPart part, ZDecimal varianceQty, string partAttrib1 = "", string partAttrib2 = "", string partAttrib3 = "", string serialNumber = "", ZDate? expiryDate = null, ZDate? packingDate = null, decimal expectedQty = 0, WhsLocation expectedLocation = null, bool isCountingPalletsOnly = false, string authorizedAction = "", string adjustmentReasonCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment)
		{
			var variance = Factory.New<WhsCycleCountLocationVariance>();
			variance.WCC_WCL_CycleCountLocation = cycleCount.PK;
			variance.WCC_Status = status;
			variance.WCC_OH_Client = client?.PK ?? ZGuid.Empty;
			variance.WCC_OP_Product = part?.PK ?? ZGuid.Empty;
			variance.WCC_PalletID = palletID;
			variance.WCC_PartAttrib1 = partAttrib1;
			variance.WCC_PartAttrib2 = partAttrib2;
			variance.WCC_PartAttrib3 = partAttrib3;
			variance.WCC_SerialNumber = serialNumber;
			variance.WCC_ExpiryDate = expiryDate ?? ZDate.Empty;
			variance.WCC_PackingDate = packingDate ?? ZDate.Empty;
			variance.WCC_ExpectedQty = expectedQty;
			variance.WCC_VarianceQty = varianceQty;
			variance.WCC_WL_ExpectedStockLocation = expectedLocation?.PK ?? ZGuid.Empty;
			variance.WCC_IsCountingPalletsOnly = isCountingPalletsOnly;
			variance.WCC_AuthorizedAction = authorizedAction;
			variance.WCC_AdjustmentReasonCode = adjustmentReasonCode;

			return variance;
		}

		#endregion

		#region WhsCheckTransactionAndPickQtyIsCorrect

		public const string WhsCheckTransactionAndPickQtyIsCorrect = "WhsCheckTransactionAndPickQtyIsCorrect_V3";

		#endregion

		#region LoadInventory

		// These methods are a lightweight implementation of the now deleted WhsInventoryQuery,
		// created so as not to require refactoring existing unit tests. There are probably better ways
		// to test your functionality rather than relying on these methods.
		public InventoriesWrapper LoadInventory()
		{
			return LoadInventory((WhsLocation)null);
		}

		public InventoriesWrapper LoadInventory(string bondedEntryKey)
		{
			return LoadInventory(null, null, null, null, "", bondedEntryKey);
		}

		public InventoriesWrapper LoadInventory(OrgHeader client, OrgSupplierPart part)
		{
			return LoadInventory(client, part, "");
		}

		public InventoriesWrapper LoadInventory(OrgHeader client, OrgSupplierPart part, string bondedEntryKey)
		{
			return LoadInventory(client, part, null, null, "", bondedEntryKey);
		}

		public InventoriesWrapper LoadInventory(OrgHeader client, OrgSupplierPart part, WhsWarehouse warehouse)
		{
			return LoadInventory(client, part, null, warehouse, "", "");
		}

		public InventoriesWrapper LoadInventory(OrgHeader client, OrgSupplierPart part, WhsLocation location)
		{
			return LoadInventory(client, part, location, null, "", "");
		}

		public InventoriesWrapper LoadInventory(WhsLocation location)
		{
			return LoadInventory(location, "");
		}

		public InventoriesWrapper LoadInventory(WhsLocation location, string status)
		{
			return LoadInventory(null, null, location, null, status, "");
		}

		public InventoriesWrapper LoadInventory(WhsDocketLine line)
		{
			return LoadInventory(line, line.Location);
		}

		public InventoriesWrapper LoadInventory(WhsDocketLine line, WhsLocation location)
		{
			var docket = line.Docket;
			var query = WhsInventoryFilterBuilder.BuildFilter(docket.Client, line.SupplierPart, docket.Warehouse, null, location, "", line.WE_BondedEntryKey,
				line.WE_ExpiryDate, line.WE_PackingDate, line.WE_PartAttrib1, line.WE_PartAttrib2, line.WE_PartAttrib3, line.WE_SerialNumber, ZDateTimeOffset.Empty);
			Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);
			return new InventoriesWrapper(Factory.Load<WhsInventoryView>(query));
		}

		InventoriesWrapper LoadInventory(OrgHeader client, OrgSupplierPart part, WhsLocation location, WhsWarehouse warehouse, string status, string bondedEntryKey)
		{
			var query = new ZQuery();

			if (client != null)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			}

			if (part != null)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, part.PK);
			}

			if (location != null)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_WL, location.PK);
			}

			if (!status.IsNullOrEmpty())
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, status);
			}

			if (!bondedEntryKey.IsNullOrEmpty())
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, bondedEntryKey);
			}

			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);
			var loadedInventory = Factory.Load<WhsInventoryView>(query);
			var filteredInventory = warehouse != null ? loadedInventory.Where(i => i.Location.WLV_WW_Whs == warehouse.PK).ToArray() : loadedInventory;
			return new InventoriesWrapper(filteredInventory);
		}

		public class InventoriesWrapper
		{
			internal InventoriesWrapper(WhsInventoryView[] inventories)
			{
				Inventories = inventories;
			}

			readonly WhsInventoryView[] Inventories;

			public decimal UnitsAvailable => UnitsTotal - UnitsCommitted;
			public decimal UnitsCommitted => Inventories.Sum(i => i.CommittedQuantityIncludingUnfinalisedReceipt);
			public decimal UnitsTotal => Inventories.Sum(i => i.WI_TotalUnits);
			public IEnumerable<WhsInventoryView> Inventory => Inventories.ToArray();
		}

		#endregion

		#region CreateInventoryForDockDoorLocation

		public WhsInventoryView CreateInventoryForDockDoorLocation(WhsReceive receive, OrgSupplierPart part, WhsLocation dockDoorLocation, string palletID, ZDecimal qty)
		{
			var inventory = CreateWhsReceiveInventoryLine(receive, part, qty);
			inventory.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventory.InDocketLine.WE_PalletID = palletID;
			inventory.InDocketLine.WE_AdjustmentArrivalDate = GetValidAdjustmentArrivalDate(inventory.InDocketLine);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);
			AssertNotEquals("Precondition", ZGuid.Empty, inventory.WI_WL);
			AssertEquals("Precondition", ZGuid.Empty, inventory.InDocketLine.WE_WL_TransferFrom);
			return inventory;
		}

		#endregion

		#region SetupTransferLineForDockDoorLocation

		public WhsTransferLine SetupTransferLineForDockDoorLocation(WhsTransfer transfer, OrgSupplierPart part, WhsLocation dockDoorLocation, WhsLocation nonDockDoorLocation, string palletID, ZDecimal quantity)
		{
			var line = CreateWhsTransferLine(transfer, part, quantity, dockDoorLocation.ToLocationString(), nonDockDoorLocation?.ToLocationString() ?? string.Empty);
			line.WE_TransferFromPalletId = palletID;
			line.WE_PalletID = palletID;
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Received;
			AssertEquals("Precondition - ensure no stock is committed.", 0m, line.GetQtyCommittedToThisLine());
			return line;
		}

		#endregion

		#region CreateMatchingLine

		public WhsTransferLine CreateMatchingLine(WhsTransferLine line, ZDecimal quantity)
		{
			var clone = line.Clone<WhsTransferLine>();
			clone.WE_WD = line.WE_WD;
			clone.WE_PutawayTime = line.WE_PutawayTime;
			clone.WE_GS_NKPutawayBy = line.WE_GS_NKPutawayBy;
			clone.WE_WE_MatchingLine = line.PK;
			clone.WE_TransactionQuantity = quantity;

			return clone;
		}

		#endregion

		#region Pick

		#region CreatePickNew

		public WhsPick CreatePickNew(params WhsPickableDocket[] pickableDockets)
		{
			return CreatePickNew(false, false, true, true, pickableDockets);
		}

		/// <summary>
		/// For tests that specifically need to use unmocked allocation rules.
		/// </summary>
		public WhsPick CreatePickNew_WithoutAllocationEngineMock(params WhsPickableDocket[] pickableDockets)
		{
			return CreatePickNew(false, false, true, false, pickableDockets);
		}

		public WhsPick CreatePickNew(string pickOption, params WhsPickableDocket[] pickableDockets)
		{
			return CreatePickNew(false, false, true, pickOption, true, pickableDockets);
		}

		BusinessObject IWhsTransactionTestHelper.CreatePickNew(bool finaliseOrders, bool finalisePick, params ZGuid[] pickableDocketPKs)
		{
			var pickableDockets = pickableDocketPKs.Select(x => Factory.Load<WhsPickableDocket>(x)).ToArray();
			return CreatePickNew(finaliseOrders, finalisePick, true, true, pickableDockets);
		}

		public WhsPick CreatePickNew(bool finaliseOrders, bool finalisePick, params WhsPickableDocket[] pickableDockets)
		{
			return CreatePickNew(finaliseOrders, finalisePick, true, true, pickableDockets);
		}

		public WhsPick CreatePickNew(bool finaliseOrders, bool finalisePick, bool checkPick, bool useAllocMock, params WhsPickableDocket[] pickableDockets)
		{
			return CreatePickNew(finaliseOrders, finalisePick, checkPick, string.Empty, useAllocMock, pickableDockets);
		}

		public WhsPick CreatePickNew(bool finaliseOrders, bool finalisePick, bool checkPick, string pickOption, bool useAllocMock, params WhsPickableDocket[] pickableDockets)
		{
			var pick = Factory.New<WhsPick>();
			if (!string.IsNullOrEmpty(pickOption))
			{
				pick.WP_PickOption = pickOption;
			}
			WhsPick.DocketPickabilityEventArgs args = null;
			pick.PickOrdersFailed += (sender, e) => { args = e; };
			if (useAllocMock)
			{
				pick.PickOrdersWithAllocationMock(pickableDockets);
			}
			else
			{
				pick.PickOrders(pickableDockets);
			}

			if (checkPick)
			{
				var errorMessage = args?.Message ?? ZString.Empty;
				var assertionMessage = $"Precondition - Ensure pickableDocket is picked. Pick failed with error message: {errorMessage}";

				foreach (var pickableDocket in pickableDockets)
				{
					AssertEquals(assertionMessage, pick, pickableDocket.Pick);
				}
			}

			if (finaliseOrders)
			{
				pick.FinaliseAllOrders();
				foreach (var order in pickableDockets)
				{
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
				}

				if (finalisePick)
				{
					pick.FinalisePick();
					WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
				}
			}
			else if (finalisePick)
			{
				throw new ArgumentException("You cannot finalise Pick without finalising orders first.");
			}

			return pick;
		}

		#endregion

		#region CreatePickByAttachingOrders

		public WhsPick CreatePickByAttachingOrders(params WhsPickableDocket[] pickableDockets)
		{
			var pick = Factory.New<WhsPick>();
			pick.Orders.AddRange(pickableDockets);
			pick.AutoAllocateItemsWithMock();
			return pick;
		}

		public WhsPick CreatePickByAttachingOrders(WhsPickableDocket pickableDocket)
		{
			return CreatePickByAttachingOrders(new WhsPickableDocket[] { pickableDocket });
		}

		#endregion

		#region CreatePick_OLD

		/// <summary>
		/// DO NOT USE THIS METHOD - Use CreatePickNew().
		/// </summary>
		public WhsPick CreatePick_OLD(params WhsPickableDocket[] pickableDockets)
		{
			Factory.Save();
			WhsPick pick = CreatePick_OLD();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				pick.CreatePick_OBSOLETE(pickableDockets);
			}

			return pick;
		}

		public WhsPick CreatePick_OLD()
		{
			return CreatePick_OLD(ZGuid.Empty);
		}

		public WhsPick CreatePick_OLD(WhsWarehouse whs)
		{
			return CreatePick_OLD(whs.PK);
		}

		public WhsPick CreatePick_OLD(ZGuid whsPK)
		{
			Factory.Save();
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = whsPK;
			return pick;
		}

		#endregion

		#endregion

		#region CreateWhsDockDoorAssignment

		public WhsDockDoorAssignment CreateWhsDockDoorAssignment(WhsLocation dockdoorLocation, params WhsPick[] picks)
		{
			if (picks.Length == 0)
			{
				throw new ArgumentException("Dock Door Assignments need picks when created.");
			}

			var dda = Factory.New<WhsDockDoorAssignment>();
			dda.WDA_WL_AssignedDockDoor = dockdoorLocation.PK;
			foreach (var pick in picks)
			{
				pick.WP_WL_DockDoor = ZGuid.Empty;
				pick.WP_WDA_DockDoorAssignment = dda.PK;
			}

			return dda;
		}

		#endregion

		#region CreateReservePickLine

		public WhsPickLine CreateReservePickLine(WhsPickableDocketLine line, WhsInventoryView inventory, ZDecimal quantity)
		{
			var reservedPickLine = Factory.New<WhsPickLine>();
			reservedPickLine.IsReserveLine = true;
			reservedPickLine.WZ_WE_TransactionLine = line.PK;
			reservedPickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			reservedPickLine.ReservedQuantity = quantity;

			return reservedPickLine;
		}

		public ZDecimal GetTotalPickLineQuantity(WhsPick pick)
		{
			return pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Sum(ordInv => ordInv.PickLineQuantity);
		}

		#endregion

		#region CreateWhsPickLine

		public WhsPickLine CreateWhsPickLine(WhsDocketLine docketLine, WhsInventoryView inventory, decimal qty)
		{
			var pickLine = docketLine.PickLines.AddNew();
			pickLine.WZ_WE_TransactionLine = docketLine.PK;
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_Units = qty;

			return pickLine;
		}

		#endregion

		#region AddClientPickParamsForWarehouse

		public WhsClientPickPackParamsByWhs AddClientPickParamsForWarehouse(WhsClientPickingParams clientParams, WhsWarehouse warehouse, string packType, int labelsToPrintOnClose, int labelsToPrintOnNew, bool isPickAndPackEnabled)
		{
			var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
			pickPackParams.WPP_WW_Warehouse = warehouse.PK;
			pickPackParams.WPP_F3_NKPackType = packType;
			pickPackParams.WPP_NumberOfLabelsToPrintOnClose = labelsToPrintOnClose;
			pickPackParams.WPP_NumberOfLabelsToPrintOnNew = labelsToPrintOnNew;
			pickPackParams.WPP_IsPickAndPackEnabled = isPickAndPackEnabled;
			return pickPackParams;
		}

		public WhsClientPickPackParamsByWhs AddClientPickParamsForWarehouse(WhsClientPickingParams clientParams, WhsWarehouse warehouse, string packType)
		{
			var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
			pickPackParams.WPP_WW_Warehouse = warehouse.PK;
			pickPackParams.WPP_F3_NKPackType = packType;
			return pickPackParams;
		}

		public WhsClientPickPackParamsByWhs AddClientPickParamsForWarehouse(WhsClientPickingParams clientParams, WhsWarehouse warehouse)
		{
			var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
			pickPackParams.WPP_WW_Warehouse = warehouse.PK;
			return pickPackParams;
		}

		#endregion

		#region OrderedInventory

		public WhsPickOrderedInventory CreateOrderedInventory()
		{
			var whs = CreateWarehouse("OI1", "A", 2, 2);
			var org = CreateClient();
			var part = CreateProduct(org, "P1");
			var order = CreateWhsOrder(org, whs, "1");
			var orderLine = CreateWhsOrderLine(order, part, 10m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			return pick.OrderedInventories[0];
		}

		public void CommitSomeUnits(WhsInventoryView inventory, ZDecimal quantity)
		{
			if (inventory.Client == null)
			{
				inventory.WI_OH_Client = CreateClient().PK;
			}

			if (inventory.SupplierPart == null)
			{
				inventory.WI_OP = CreateProduct(inventory.Client, "P1").PK;
			}

			if (inventory.Warehouse == null)
			{
				WhsWarehouse whs = CreateWarehouse("W1", "A");
				inventory.WI_WL = whs.Rows[0].Locations[0].PK;
			}
			inventory.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Available;
			WhsOrder order = CreateWhsOrder(inventory.Client, inventory.Warehouse);
			WhsOrderLine orderLine = CreateWhsOrderLine(order, inventory.SupplierPart, 15m);
			CreateReservePickLine(orderLine, inventory, quantity); // force pick to pick THIS line
			WhsPick pick = CreatePickByAttachingOrders(order);
		}

		#endregion

		#region Available Inventory

		public WhsPickAvailableInventory SetAvailableInventory(WhsPickAvailableInventory availableInventory, ZBool isAllocated, ZDateTimeOffset pickedTime)
		{
			availableInventory.Allocate = isAllocated;
			SetPickedDate(availableInventory, pickedTime);
			return availableInventory;
		}

		#endregion

		#region Create Stock

		public BusinessObject CreateStock(ZGuid whsPK, ZGuid clientPK, ZGuid prodPK, ZDecimal units)
		{
			return CreateStock(whsPK, clientPK, "", prodPK, units);
		}

		public BusinessObject CreateStock(ZGuid whsPK, ZGuid clientPK, ZString reference, ZGuid partPK, ZDecimal units,
			string pA1 = "",
			string pA2 = "",
			string pA3 = "",
			string sn = "",
			string bEK = "")
		{
			var receive = CreateWhsReceive(clientPK, whsPK, reference, new TestNotificationBuffer());
			var inventory = CreateWhsReceiveInventoryLine(receive, partPK, units, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, pA1, pA2, pA3, sn, bEK);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			return inventory.InDocketLine;
		}

		public WhsReceive CreateStock(WhsWarehouse whs, OrgHeader client, OrgSupplierPart prod, ZDecimal units, string locn)
		{
			return CreateStock(whs, client, "", prod, units, locn);
		}

		public WhsReceive CreateStock(WhsWarehouse whs, OrgHeader client, ZString reference, OrgSupplierPart prod, ZDecimal units, string locn)
		{
			WhsReceive receive = CreateWhsReceive(client, whs, reference, new TestNotificationBuffer());
			WhsInventoryView receiveInventory = CreateWhsReceiveInventoryLine(receive, prod, units);
			receiveInventory.LocationString = locn;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			return receive;
		}

		#endregion

		#region Accounting

		public void CreateAccountingDataWithCharge(IJobInvoicingPlugIn job)
		{
			CreateAccountingDataWithNoCharge(job);
			AddChargeLine(job);
		}

		public void CreateAccountingDataWithWIPCharge(IJobInvoicingPlugIn job)
		{
			CreateAccountingDataWithNoCharge(job);
			AddWIPChargeLine(job);
		}

		public Job CreateAccountingDataWithNoCharge(IJobInvoicingPlugIn job)
		{
			Job jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_ParentID = job.PK;
			jobHeader.JH_ParentTableCode = ((job as WhsDocket) != null) ? WhsDocketSchema.Constants.Prefix : JobStorageSchema.Constants.Prefix;

			AccTransactionHeader aRHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			aRHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			aRHeader.AH_TransactionType = TransactionTypes.Invoice;
			aRHeader.AH_JH = jobHeader.PK;

			return jobHeader;
		}

		public JobCharge AddChargeLine(IJobInvoicingPlugIn forJob)
		{
			return AddChargeLine(forJob, ZArchitecture.Core.TransactionLineTypes.Revenue);
		}

		public JobCharge AddWIPChargeLine(IJobInvoicingPlugIn forJob)
		{
			return AddChargeLine(forJob, ZArchitecture.Core.TransactionLineTypes.WIP);
		}

		public JobCharge AddChargeLine(IJobInvoicingPlugIn forJob, string lineType)
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Job jobHeader = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, forJob.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AccTransactionHeader aRHeader = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_JH, jobHeader.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, jobHeader.JH_GC));
			AccTransactionLines aRLine = Factory.NewWithValidTestData<AccTransactionLines>();
			JobCharge charge = jobHeader.Charges.AddNew();

			aRLine.AL_AH = aRHeader.PK;
			aRLine.AL_LineType = lineType;
			aRLine.AL_JH = jobHeader.PK;
			aRLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AL_ARLine = aRLine.PK;

			return charge;
		}

		public void AddOnePostedChargeLine(WhsDocket docket)
		{
			JobHeader jobHeader = AddJobToDocket(docket);
			AccTransactionLines line1 = Factory.New<AccTransactionLines>();
			line1.AL_JH = jobHeader.PK;
			line1.AL_GC = GlbCompany.CurrentCompany.PK;
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			docket.ClearAccTranLinesCache();
		}

		public JobHeader AddJobToDocket(WhsDocket docket)
		{
			JobHeader result = Factory.NewJobForTesting<JobHeader>();
			result.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			result.JH_ParentID = docket.PK;
			result.JH_GC = GlbCompany.CurrentCompany.PK;
			result.JH_GB = GlbBranch.CurrentBranch.PK;
			result.JH_GE = GlbDepartment.CurrentDepartment.PK;
			return result;
		}

		public JobStorage CreateJobStorage(ZGuid warehousePK, ZGuid client, ZDateTime startTime, ZDateTime endTime)
		{
			var jobStorage = (JobStorage)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Warehouse.IWhsInvoice)));
			jobStorage.ET_WW = warehousePK;
			jobStorage.ET_OH_Client = client;
			jobStorage.ET_StorageFromDate = startTime;
			jobStorage.ET_StorageToDate = endTime;
			return jobStorage;
		}

		#region CreateChargeCode

		public AccChargeCode CreateChargeCode(ZString code, ZString description, ZString chargeGroup, ZString chargeSubGroup)
		{
			var result = Factory.NewWithValidTestData<AccChargeCode>();

			result.AC_Code = code;
			result.AC_Desc = description;
			result.AC_ChargeGroup = chargeGroup;
			result.AC_ChargeSubGroup = chargeSubGroup;
			result.AC_ChargeType = Constants.ChargeType.Revenue;
			result.AC_IsActive = true;

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			result.AC_AT_GSTRate = rate.PK;

			return result;
		}

		#endregion

		#region CreateClientRate

		public ClientRate CreateClientRate(OrgHeader client)
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			return clientRate;
		}

		#endregion

		#region CreateRateEntry

		public RateEntry CreateRateEntry(ClientRate clientRate, ZDate rateStartDate, ZDate rateEndDate)
		{
			return CreateRateEntry(clientRate, rateStartDate, rateEndDate, "");
		}

		public RateEntry CreateRateEntry(ClientRate clientRate, ZDate rateStartDate, ZDate rateEndDate, ZString containerType)
		{
			var warehouseEntry = clientRate.AddRateEntry("WHS", "ALL", "", "", "", containerType);
			warehouseEntry.RateLines.RemoveAndDeleteAll();
			warehouseEntry.TI_RateStartDate = rateStartDate;
			warehouseEntry.TI_RateEndDate = rateEndDate;

			return warehouseEntry;
		}

		#endregion

		#region CreateRateLine

		public RateLine CreateRateLine(RateEntry rateEntry, AccChargeCode chargeCode, ZString unit, ZDecimal price)
		{
			return CreateRateLine(rateEntry, chargeCode, unit, price, UnitCalculator.Code);
		}

		public RateLine CreateRateLine(RateEntry rateEntry, AccChargeCode chargeCode, ZString unit, ZDecimal price, ZString calculator, bool isPalletized = false)
		{
			RateLine rateLine = rateEntry.AddRateLine(chargeCode, calculator, unit);
			rateLine.Calculator.Decimal1 = price;
			rateLine.TL_IsOnPallets = isPalletized;
			return rateLine;
		}

		#endregion

		#region CreateRatingJob

		public Job CreateRatingJob(IJobInvoicingPlugIn jobToRate, string jobToRatePrefix = JobStorageSchema.Constants.Prefix)
		{
			var job = Factory.NewJobForTesting<Job>();
			job.JH_ParentTableCode = jobToRatePrefix;
			job.JH_ParentID = jobToRate.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.PlugInData = jobToRate;

			return job;
		}

		#endregion

		#region CreateJobCharge

		public JobCharge CreateJobCharge(Job jobHeader)
		{
			return CreateJobCharge(jobHeader, null, 0m);
		}

		public JobCharge CreateJobCharge(Job jobHeader, AccChargeCode chargeCode, ZDecimal sellAmount, ZGuid? sellAccount = null)
		{
			var charge = jobHeader.Charges.AddNew();
			if (chargeCode != null)
			{
				charge.JR_AC = chargeCode.PK;
			}

			if (!sellAmount.IsEmpty)
			{
				charge.JR_OSSellAmt = sellAmount;
				charge.JR_LocalSellAmt = sellAmount;
				charge.JR_OH_SellAccount = sellAccount ?? jobHeader.LocalCharges.PK;
			}

			return charge;
		}

		#endregion

		#region CreateJobChargeAttrib

		public JobChargeAttrib CreateJobChargeAttrib(JobCharge charge, string name, string value)
		{
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;

			return attrib;
		}

		#endregion

		#region CreateEquipment

		public RefEquipment CreateEquipment(ZString equipmentNumber, ZDecimal weightCapacity, ZString weightUnit, ZDecimal cubicCapacity, ZString cubicUnit)
		{
			return CreateEquipment(equipmentNumber, weightCapacity, weightUnit, cubicCapacity, cubicUnit, 0, "");
		}

		public RefEquipment CreateEquipment(ZString equipmentNumber, ZDecimal weightCapacity, ZString weightUnit, ZDecimal cubicCapacity, ZString cubicUnit, ZInt packs, ZString packType)
		{
			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = equipmentNumber;
			equipment.RQ_Registration = equipmentNumber;

			equipment.RQ_WeightCapacity = weightCapacity;
			equipment.RQ_WeightUnit = weightUnit;

			equipment.RQ_CubicCapacity = cubicCapacity;
			equipment.RQ_CubicUnit = cubicUnit;

			equipment.RQ_PackCapacity = packs;
			equipment.RQ_F3_NKPackType = packType;

			return equipment;
		}

		#endregion

		#region CreateTrolley

		public RefEquipment CreateTrolley(string trolleyNumber)
		{
			var trolley = Factory.New<RefEquipment>();
			trolley.RQ_ShortCode = trolleyNumber;
			trolley.RQ_Registration = trolleyNumber;

			return trolley;
		}

		#endregion

		#region CreateWhsPickTrolleyJob

		public IWhsPickTrolleyJob CreateWhsPickTrolleyJob(ZGuid trolleyPK, ZString jobStatus)
		{
			var pickTrolley = Factory.New<IWhsPickTrolleyJob>();
			pickTrolley.WTJ_RQ_Equipment = trolleyPK;
			pickTrolley.WTJ_Status = jobStatus;

			return pickTrolley;
		}

		#endregion

		#region CreateWhsPickTrolleySlot

		public IWhsPickTrolleySlot CreateWhsPickTrolleySlot(ZGuid trolleyJobPK, ZGuid packagePK, ZShort slot)
		{
			var trolleySlot = Factory.New<IWhsPickTrolleySlot>();
			trolleySlot.WTS_WTJ_TrolleyJob = trolleyJobPK;
			trolleySlot.WTS_KP_Package = packagePK;
			trolleySlot.WTS_SlotNumber = slot;

			return trolleySlot;
		}

		#endregion

		#region AutoRateJob

		public AutoRateInfoCollection AutoRateJob(WhsDocket docket, CostSell costSell = CostSell.Revenue)
		{
			var job = (Job)docket.JobHeader ?? (CreateRatingJob(docket, WhsDocketSchema.Constants.Prefix));
			return AutoRateJob(docket, job, new[] { GetIAutoRating(docket) }, costSell);
		}

		public AutoRateInfoCollection AutoRateJob(IBusiness masterBizO, Job jobHeader, IAutoRating[] jobsToRate, CostSell costOrSell = CostSell.Revenue)
		{
			var context = new RatingContext();
			var interactor = new LoggerDecorator(context.Logger);
			var costAdapterIDs = ((IRatingSupporter)masterBizO).AdaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts).Select(x => x.OperationalJobCode).ToArray();
			var sellAdapterIDs = ((IRatingSupporter)masterBizO).AdaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts).Select(x => x.OperationalJobCode).ToArray();

			var adaptersProvider = new Mock<IRatingAdaptersProvider>();
			var strategy = new AutoRateInvoicingStrategy(masterBizO, jobHeader);
			var autoRatesCollection = new AutoRatingRunner(masterBizO, context).RetrieveAllCharges(jobsToRate, adaptersProvider.Object, costOrSell);
			strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Cost, costAdapterIDs);
			strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Revenue, sellAdapterIDs);

			return autoRatesCollection;
		}

		protected virtual IAutoRating GetIAutoRating(BusinessObject bizO)
		{
			if (bizO is WhsReceive)
			{
				return new WhsReceiveRatingAdapter((WhsReceive)bizO);
			}

			if (bizO is WhsOrder)
			{
				return new WhsOrderRatingAdapter((WhsOrder)bizO);
			}

			if (bizO is WhsAdjustment)
			{
				return new WhsAdjustmentRatingAdapter((WhsAdjustment)bizO);
			}

			throw new ArgumentException("The provided BizO is not supported.");
		}

		#endregion

		#region PostInvoice

		public InvoicingBaseCollection PostInvoice(Job job)
		{
			var postManager = new InvoicingPostManager(job);
			Factory.Save();

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			postManager.Poster.ChangeTransactionDateOnAllARInvoicesAndCFXLines(ZDateTime.Now, ZDateTime.Now);
			return postManager.Poster.PostedInvoices;
		}

		#endregion

		#endregion

		#region Workflow

		public ProcessTaskTemplate CreateWorkflowTemplate(ZString name, ZString workflowDescriptorCode, OrgHeader client = null, bool isPartial = false, string tasksFallback = FallbackTypeList.Codes.EmptyFallback, string milestoneFallback = FallbackTypeList.Codes.EmptyFallback, string triggerFallback = FallbackTypeList.Codes.EmptyFallback)
		{
			var result = Factory.New<ProcessTaskTemplate>();
			result.P0_Name = name;
			result.P0_ProcessType = workflowDescriptorCode;
			result.P0_OH_Client = client?.PK ?? ZGuid.Empty;
			result.P0_IsPartialTemplate = isPartial;
			result.P0_TaskFallbackMethod = tasksFallback;
			result.P0_MilestoneFallbackMethod = milestoneFallback;
			result.P0_TriggerFallbackMethod = triggerFallback;

			return result;
		}

		public ProcessTask CreateWorkflowMilestone(ProcessTaskCollection workflowItems, ZString description, int sequence, ZString eventCode, ZString exceptionCode)
		{
			var milestone = workflowItems.AddNew();
			milestone.P9_Type = Constants.Workflow.MilestoneType;
			milestone.P9_Description = description;
			milestone.P9_Sequence = sequence;
			milestone.P9_SE_NKExceptionEvent = exceptionCode;
			milestone.TriggerConditions.TriggerEventCode = eventCode;

			return milestone;
		}

		public ProcessTask CreateWorkflowTrigger(ProcessTaskCollection workflowItems, ZString description, int sequence, ZString eventCode, string triggerCondition = "", string triggerConditionValue = "")
		{
			var trigger = workflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.P9_Description = description;
			trigger.P9_Sequence = sequence;
			trigger.TriggerConditions.TriggerEventCode = eventCode;
			trigger.TriggerConditions.TriggerCondition = triggerCondition;
			trigger.TriggerConditions.TriggerConditionValue = triggerConditionValue;

			return trigger;
		}

		public ProcessTaskNotification CreateWorkflowNotification(ProcessTask processTask, ZString triggerType, string recepient = "", ZGuid? document = null, string fieldName = "", string fieldValue = "")
		{
			var notification = processTask.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_FieldName = fieldName;
			notification.PQ_FieldValue = fieldValue;
			notification.PQ_Calc_TriggerParty = recepient;
			notification.PQ_SU_Document = document ?? ZGuid.Empty;

			return notification;
		}

		public GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode, int displaySequence = 0, GenCustomAddOnRule addOnRule = null)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			result.XC_DisplaySequence = displaySequence;
			result.XC_XR = addOnRule?.PK ?? ZGuid.Empty;

			return result;
		}

		#endregion

		#region Docket Container

		public WhsDocketContainer CreateWhsDocketContainer(WhsDocket docket, ZString containerNumber, ZString containerType, bool isChargeable, bool isPalletized)
		{
			WhsDocketContainer container = docket.Containers.AddNew();
			container.WC_ContainerNum = containerNumber;
			container.WC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;
			container.WC_IsChargeable = isChargeable;
			container.WC_IsPalletised = isPalletized;
			return container;
		}

		#endregion

		#region Stocktake

		public WhsStocktake CreateWhsStocktake(WhsWarehouse whs, string countEmptyLocationsCategory = CountEmptyLocationCategory.Codes.ExcludeEmptyLocations)
		{
			var stocktake = CreateWhsStocktake(null, whs, null, null, StocktakeStatus.Codes.New);
			stocktake.WS_CountEmptyLocationsCategory = countEmptyLocationsCategory;
			return stocktake;
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs)
		{
			return CreateWhsStocktake(client, whs, null, null, StocktakeStatus.Codes.New);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, WhsLocation location)
		{
			return CreateWhsStocktake(client, whs, null, StocktakeStatus.Codes.New, null, "", null, location);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, WhsLocation location)
		{
			return CreateWhsStocktake(client, whs, part, StocktakeStatus.Codes.New, null, "", null, location);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, ZString status)
		{
			return CreateWhsStocktake(client, whs, null, null, status);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, ZString stocktakeType, ZString status)
		{
			return CreateWhsStocktake(client, whs, null, stocktakeType, status, null, "", null, null);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part)
		{
			return CreateWhsStocktake(client, whs, part, null, StocktakeStatus.Codes.New);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, WhsRow row)
		{
			return CreateWhsStocktake(client, whs, null, row, StocktakeStatus.Codes.New);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, WhsRow row, ZString status)
		{
			return CreateWhsStocktake(client, whs, part, row, status, "");
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, WhsRow row, ZString status, ZString pickMethod)
		{
			return CreateWhsStocktake(client, whs, part, status, row, pickMethod, null);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, ZString status, WhsRow row, ZString pickMethod, WhsArea area)
		{
			return CreateWhsStocktake(client, whs, part, status, row, pickMethod, area, null);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, ZString status, WhsRow row, ZString pickMethod, WhsArea area, WhsLocation location)
		{
			return CreateWhsStocktake(client, whs, part, "STD", status, row, pickMethod, area, location);
		}

		public WhsStocktake CreateWhsStocktake(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, ZString stocktakeType, ZString status, WhsRow row, ZString pickMethod, WhsArea area, WhsLocation location)
		{
			var stocktake = Factory.New<WhsStocktake>();
			if (client != null)
			{
				stocktake.WS_OH_Client = client.PK;
			}
			stocktake.WS_WW_Whs = whs.PK;
			stocktake.WS_WR_Row = row != null ? row.PK : ZGuid.Empty;
			stocktake.WS_StocktakeStatus = status;
			stocktake.WS_PickMethod = pickMethod;
			stocktake.WS_WA_Area = area != null ? area.PK : ZGuid.Empty;
			stocktake.WS_WL_Location = location != null ? location.PK : ZGuid.Empty;
			stocktake.WS_StocktakeType = stocktakeType;

			if (part != null)
			{
				CreateWhsStocktakeProductFilter(stocktake, part);
			}

			return stocktake;
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, null, ZDateTime.Empty);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, ZBool isManuallyAdded)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, null, ZDateTime.Empty, null, ZDateTime.Empty, 1m, 1, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, 1m, isManuallyAdded);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal systemCount, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, lastCountColumnNumber, lineStatus, InventoryStatus.Codes.Available, systemCount, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal systemCount, ZDecimal lastVerifiedCount, ZDateTime lastVerfiedDate, ZByte lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, lastVerfiedDate, lastVerifiedCount, lastCountColumnNumber, lineStatus, InventoryStatus.Codes.Available, systemCount, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, lineStatus);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString lineStatus, ZString inventoryStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDate.Empty, ZDate.Empty, "", "", "", lineStatus, inventoryStatus);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString lineStatus, ZString inventoryStatus, ZByte lastCountColumnNumber, GlbStaff verifiedBy, ZDateTime lastVerifiedTime)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, verifiedBy, lastVerifiedTime, 0, lastCountColumnNumber, lineStatus, inventoryStatus, 0m, "", false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString lineStatus, ZString inventoryStatus, ZBool isManuallyAddedLine)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, 0, 1, lineStatus, inventoryStatus, 0m, true);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString palletId, ZBool isManuallyAddedLine)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, palletId, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available, isManuallyAddedLine);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString palletId, ZString lineStatus, ZString inventoryStatus, ZBool isManuallyAddedLine)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, 0, 1, lineStatus, inventoryStatus, 0m, palletId, true);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, lastCountColumnNumber, lineStatus, "", 0m, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus, ZBool isManuallyAddedLine)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, lastCountColumnNumber, lineStatus, InventoryStatus.Codes.Available, 0m, isManuallyAddedLine);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal systemCount, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			var line = CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, lastCountColumnNumber, lineStatus, InventoryStatus.Codes.Available, systemCount, false);
			SetStocktakeLineAttributes(line, ZDate.Today, ZDate.Today, partAttrib1, partAttrib2, partAttrib3);
			return line;
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, ZDateTime lastVerfiedDate, ZByte lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, lastVerfiedDate, lastVerifiedCount, lastCountColumnNumber, lineStatus, "", 0m, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, GlbStaff verifiedBy, ZByte lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, verifiedBy, ZDateTime.Empty, lastVerifiedCount, lastCountColumnNumber, lineStatus, "", 0m, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDecimal lastVerifiedCount, ZDateTime lastVerfiedDate, GlbStaff verifiedBy, ZInt lastCountColumnNumber, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, verifiedBy, lastVerfiedDate, lastVerifiedCount, lineStatus);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDateTime dateClosed)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, dateClosed, null, ZDateTime.Empty, 0, "");
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDate eD, ZDate pD, ZString pA1, ZString pA2, ZString pA3)
		{
			var stocktakeLine = CreateWhsStocktakeLine(stocktake, client, part, location);
			SetStocktakeLineAttributes(stocktakeLine, eD, pD, pA1, pA2, pA3);

			return stocktakeLine;
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDate eD, ZDate pD, ZString pA1, ZString pA2, ZString pA3, ZString stocktakeLineStatus, ZString inventoryStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, eD, pD, pA1, pA2, pA3, stocktakeLineStatus, inventoryStatus, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDate eD, ZDate pD, ZString pA1, ZString pA2, ZString pA3, ZString stocktakeLineStatus, ZString inventoryStatus, ZBool isManuallyAddedLine)
		{
			var stocktakeLine = CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, new ZDecimal(0), new ZByte(1), stocktakeLineStatus, inventoryStatus, 0m, isManuallyAddedLine);
			SetStocktakeLineAttributes(stocktakeLine, eD, pD, pA1, pA2, pA3);

			return stocktakeLine;
		}

		static void SetStocktakeLineAttributes(WhsStocktakeLine stocktakeLine, ZDate eD, ZDate pD, ZString pA1, ZString pA2, ZString pA3)
		{
			stocktakeLine.WU_ExpiryDate = eD;
			stocktakeLine.WU_PackingDate = pD;
			stocktakeLine.WU_PartAttrib1 = pA1;
			stocktakeLine.WU_PartAttrib2 = pA2;
			stocktakeLine.WU_PartAttrib3 = pA3;
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, GlbStaff verifiedBy, ZDateTime lastVerifiedTime, ZDecimal lastVerifiedCount, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, verifiedBy, lastVerifiedTime, lastVerifiedCount, lineStatus);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDateTime dateClosed, GlbStaff verifiedBy, ZDateTime lastVerifiedTime, ZDecimal lastVerifiedCount, ZString lineStatus)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, dateClosed, verifiedBy, lastVerifiedTime, lastVerifiedCount, 1, lineStatus, "", 0m, false);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZString lineStatus, ZString inventoryStatus, ZDecimal lastVerifiedCount, ZDecimal systemCount, bool isManuallyAdded = false)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, ZDateTime.Empty, null, ZDateTime.Empty, lastVerifiedCount, new ZByte(1), lineStatus, inventoryStatus, systemCount, isManuallyAdded);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDateTime dateClosed, GlbStaff verifiedBy, ZDateTime lastVerifiedTime, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus, ZString inventoryStatus, ZDecimal systemCount, ZBool isManuallyAdded)
		{
			return CreateWhsStocktakeLine(stocktake, client, part, location, dateClosed, verifiedBy, lastVerifiedTime, lastVerifiedCount, lastCountColumnNumber, lineStatus, inventoryStatus, systemCount, ZString.Empty, isManuallyAdded);
		}

		public WhsStocktakeLine CreateWhsStocktakeLine(WhsStocktake stocktake, OrgHeader client, OrgSupplierPart part, WhsLocation location, ZDateTime dateClosed, GlbStaff verifiedBy, ZDateTime lastVerifiedTime, ZDecimal lastVerifiedCount, ZByte lastCountColumnNumber, ZString lineStatus, ZString inventoryStatus, ZDecimal systemCount, ZString palletId, ZBool isManuallyAdded)
		{
			var stocktakeLine = Factory.New<WhsStocktakeLine>();
			stocktakeLine.WU_InventoryStatus = string.IsNullOrEmpty(inventoryStatus) && part == null ? (ZString)"EMP" : inventoryStatus;
			stocktakeLine.WU_LineNo = stocktake.Lines.GetNextLineNo();
			stocktakeLine.WU_IsManuallyAdded = isManuallyAdded;
			stocktakeLine.WU_WS = stocktake.PK;
			stocktakeLine.WU_OH_Client = client != null ? client.PK : ZGuid.Empty;
			stocktakeLine.WU_OP = part != null ? part.PK : ZGuid.Empty;
			stocktakeLine.WU_WL = location != null ? location.PK : ZGuid.Empty;
			stocktakeLine.WU_DateClosed = dateClosed;
			stocktakeLine.WU_TotalCounts = lastCountColumnNumber;
			stocktakeLine.CurrentCount = lastVerifiedCount;
			stocktakeLine.CurrentCountVerifiedDate = lastVerifiedTime;
			stocktakeLine.CurrentCountVerifiedBy = verifiedBy != null ? verifiedBy.GS_Code : ZString.Empty;
			stocktakeLine.WU_Status = lineStatus;
			stocktakeLine.WU_SystemUnits = systemCount;
			stocktakeLine.WU_PalletID = palletId;

			return stocktakeLine;
		}

		public WhsStocktakeLine CreateEmptyWhsStocktakeLine(WhsStocktake stocktake, WhsLocation location)
		{
			var line = CreateWhsStocktakeLine(stocktake, null, null, location);
			line.WU_Status = StocktakeLineStatus.Codes.Empty;
			return line;
		}

		public WhsStocktakeProductFilter CreateWhsStocktakeProductFilter(WhsStocktake stocktake, OrgSupplierPart part)
		{
			var whsStocktakeProductFilter = Factory.New<WhsStocktakeProductFilter>();
			whsStocktakeProductFilter.WSP_OP_Product = part.PK;
			whsStocktakeProductFilter.WSP_WS_Stocktake = stocktake.PK;
			return whsStocktakeProductFilter;
		}

		#endregion

		#region GetLogFilter

		public ZQuery GetLogFilter(ZString code, string reference)
		{
			var logFilter = new ZQuery();
			logFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, code);
			if (reference != null)
			{
				logFilter.AddToFilter(StmALogSchema.SL_Reference, reference);
			}

			return logFilter;
		}

		public ZQuery GetLogFilter(ZString code)
		{
			return GetLogFilter(code, null);
		}

		#endregion

		#region SetLogUTCTimeOnFactorySave

		public void SetLogUTCTimeOnFactorySave(DbConnection connection, StmALog log, ZDateTime date)
		{
			AssertEquals("Cannot set UTC time on a log that does not yet have a parent job.", true, log.SL_Parent.IsValid);
			// we want to set the utc date to be later then the 'hold' event's utc date
			Factory.Saved += delegate
			{
				// setting utc time is not allowed through business layer
				CargoWise.Database.TestFramework.ObjectModel.StmALog
					.UpdateWhere(l => l.PK == log.PK)
					.Set(l => l.SL_PostedTimeUtc, date.ToDateTime().ToUniversalTime())
					.Set(l => l.SL_Parent, log.SL_Parent.ToGuid()).Post(connection);
			};
		}

		#endregion

		#region CreateUNDGDataItem

		public UNDGDataItem CreateUNDGDataItem(OrgSupplierPart part, ZString undgSubstanceCode, ZString imoClass)
		{
			return CreateUNDGDataItem(part, undgSubstanceCode, 1m, "KG", 1m, "M3", imoClass);
		}

		public UNDGDataItem CreateUNDGDataItem(OrgSupplierPart part, ZString undgSubstanceCode, ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ)
		{
			return CreateUNDGDataItem(part, undgSubstanceCode, weight, weightUQ, volume, volumeUQ, "");
		}

		public UNDGDataItem CreateUNDGDataItem(
			OrgSupplierPart part,
			ZString undgSubstanceCode,
			ZDecimal weight,
			ZString weightUQ,
			ZDecimal volume,
			ZString volumeUQ,
			ZString imoClass)
		{
			var unno = undgSubstanceCode.SubstringSafe(0, 4);
			var variant = undgSubstanceCode.SubstringSafe(4, 2);
			var standard = UNDGSubstanceStandardTypes.IMO;
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard).FirstOrDefault();
			if (substance == null)
			{
				substance = Factory.New<UNDGSubstance>();
				substance.DG_UNNO = undgSubstanceCode.SubstringSafe(0, UNDGSubstanceSchema.DG_UNNO.MaxLength);
				substance.DG_Variant = undgSubstanceCode.SubstringSafe(UNDGSubstanceSchema.DG_UNNO.MaxLength, UNDGSubstanceSchema.DG_Variant.MaxLength);
				substance.DG_Identifier = substance.DG_UNNO + substance.DG_Variant + "-IMO";
			}

			return CreateUNDGDataItem(part, substance, weight, weightUQ, volume, volumeUQ, imoClass);
		}

		public UNDGDataItem CreateUNDGDataItem(
			OrgSupplierPart part,
			UNDGSubstance substance,
			ZDecimal weight,
			ZString weightUQ,
			ZDecimal volume,
			ZString volumeUQ,
			ZString imoClass)
		{
			var result = part.UNDGs.AddNew();
			result.DI_DG = substance.PK;
			result.DI_DGWeight = weight;
			result.DI_UnitOfWeight = weightUQ;
			result.DI_DGVolume = volume;
			result.DI_UnitOfVolume = volumeUQ;
			result.DI_IMOClass = imoClass;

			return result;
		}

		public UNDGSubstancePivot CreateUNDGSubstancePivot(
			ZGuid parentId,
			ZString parentTableCode,
			ZString unno,
			ZString variant,
			ZString standard,
			bool isDefault = true)
		{
			var pivot = Factory.New<UNDGSubstancePivot>();
			pivot.DP_UNNO = unno;
			pivot.DP_Variant = variant;
			pivot.DP_ParentId = parentId;
			pivot.DP_ParentTableCode = parentTableCode;
			pivot.DP_Standard = standard;
			pivot.DP_IsDefault = isDefault;

			return pivot;
		}

		public UNDGCountryReferencePivot CreateUNDGCountryReferencePivot(
			ZGuid dcrPK,
			ZString unno,
			ZString variant,
			ZString standard)
		{
			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = dcrPK;
			pivot.DCP_UNNO = unno;
			pivot.DCP_Variant = variant;
			pivot.DCP_Standard = standard;

			return pivot;
		}

		#endregion

		#region Implementation

		void SetupDocketCore(WhsDocket doc, ZGuid clientPK, ZGuid whsPK, ZString @ref, NotificationBuffer notify)
		{
			doc.WD_OH_Client = clientPK;
			doc.WD_WW_Whs = whsPK;
			doc.WD_ExternalReference = @ref;
			doc.NotificationManager.Push(notify ?? Notify);
		}

		public static ZDateTimeOffset GetValidAdjustmentArrivalDate(WhsDocketLine docketLine)
		{
			var date = docketLine.WE_AdjustmentArrivalDate;
			var lineType = docketLine.WE_DocketLineType;
			if (date.IsEmpty)
			{
				var docketArrivalDate = docketLine.Docket.WD_ArrivalDate;
				if (docketLine.WE_OriginalInventoryStatus != "PND" && docketLine.WE_StockOnHand != 0 && lineType != "ORD" && lineType != "WOR")
				{
					date = docketArrivalDate.IsEmpty ? (docketLine.WE_FinalisedDate.IsEmpty ? ZDateTimeOffset.Now : docketLine.WE_FinalisedDate) : docketArrivalDate;
				}
				else if (!docketLine.WE_FinalisedDate.IsEmpty && (lineType == "ADJ" || lineType == "INW"))
				{
					date = docketArrivalDate.IsEmpty ? docketLine.WE_FinalisedDate : docketArrivalDate;
				}
			}
			else if (lineType == "WOR" || lineType == "ORD")
			{
				date = ZDateTimeOffset.Empty;
			}

			return date;
		}

		#endregion

		#region IWhsTransactionTestHelper Members

		BusinessObject IWhsTransactionTestHelper.CreateWhsAdHocServiceJob(ZGuid warehousePK, ZGuid clientPK, ZDateTime billingDate, string customerReferenceNo)
		{
			return CreateWhsAdHocServiceJob(Factory.Load<WhsWarehouse>(warehousePK), Factory.Load<OrgHeader>(clientPK), billingDate, customerReferenceNo);
		}

		BusinessObject IWhsTransactionTestHelper.CreateProductCategory(string categoryCode, string categoryDescription, ZGuid parentCategoryPK)
		{
			var parentCategory = parentCategoryPK != ZGuid.Empty ? Factory.Load<OrgPartCategory>(parentCategoryPK) : null;
			return CreateProductCategory(categoryCode, categoryDescription, parentCategory);
		}

		BusinessObject IWhsTransactionTestHelper.CreateFTZWarehouse(ZGuid addressPK)
		{
			var warehouse = CreateFTZWarehouseInUS();
			warehouse.WW_OA_WarehouseAddress = addressPK;
			return warehouse;
		}

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString name) => CreateWarehouse(name);

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString name, string rowName)
		{
			return CreateWarehouse(name, rowName);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString name, string rowName, short columns, short levels)
		{
			return CreateWarehouse(name, rowName, columns, levels);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString code, IGlbBranch branch)
		{
			return CreateWarehouse(code, null, (GlbBranch)branch);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString name, ZString code, string rowName)
		{
			return CreateWarehouse(name, code, rowName);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWarehouse(ZString name, IOrgAddress address, IGlbBranch branch)
		{
			return CreateWarehouse(name, (OrgAddress)address, (GlbBranch)branch);
		}

		BusinessObject IWhsTransactionTestHelper.CreateTRWWarehouse()
		{
			return CreateTRWWarehouse();
		}

		BusinessObject IWhsTransactionTestHelper.CreateTRWWarehouse(string warehouseCode, string rowName, short columns, short levels, string countrycode)
		{
			return CreateTRWWarehouse(warehouseCode, rowName, columns, levels, countrycode);
		}

		BusinessObject IWhsTransactionTestHelper.CreateReceiveConsignment(string consignmentID, ZGuid warehousePK)
		{
			return CreateReceiveConsignment(consignmentID, warehousePK) as BusinessObject;
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsArea(ZGuid whsPK, ZString name)
		{
			return CreateArea(Factory.Load<WhsWarehouse>(whsPK), name);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsArea(ZGuid whsPK, ZString name, ZString type)
		{
			return CreateArea(Factory.Load<WhsWarehouse>(whsPK), name, type);
		}

		IWhsRow IWhsTransactionTestHelper.CreateRow(IWhsWarehouse whs, string code)
		{
			return CreateRow(whs, code);
		}

		IWhsRow IWhsTransactionTestHelper.CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols, short levels)
		{
			return CreateRowAndGenerateLocations(whs, code, cols, levels);
		}

		IWhsRow IWhsTransactionTestHelper.CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols, short levels, short trays, short rowSequence)
		{
			return CreateRowAndGenerateLocations(whs, code, cols, levels, trays, rowSequence);
		}

		IWhsLocation IWhsTransactionTestHelper.FindLocation(ZGuid whsPK, ZString locationString)
		{
			return Factory.Load<WhsWarehouse>(whsPK).FindLocation(locationString);
		}

		void IWhsTransactionTestHelper.GenerateLocations(ZGuid whsPK)
		{
			var whs = Factory.Load<WhsWarehouse>(whsPK);
			((IWhsWarehouseInternals)whs).GenerateLocations();
		}

		IWhsClientParameterByWarehouse IWhsTransactionTestHelper.CreateWhsClientParameterByWarehouse(ZGuid clientPK, ZGuid whsPK)
		{
			return CreateWhsClientParameterByWarehouse(clientPK, whsPK);
		}

		ZGuid IWhsTransactionTestHelper.CreateClient(ZString clientCode)
		{
			return CreateClient(clientCode).PK;
		}

		BusinessObject IWhsTransactionTestHelper.CreateClient(ZString clientCode, ZString clientName)
		{
			return CreateClient(clientCode, clientName);
		}

		ZGuid IWhsTransactionTestHelper.CreatePickface(ZGuid orgPK, ZGuid whsPK, ZGuid productPK, ZString locnString)
		{
			return CreateProductPickFace(WhsProduct.GetWhsProduct(Factory.Load<OrgSupplierPart>(productPK)), Factory.Load<OrgHeader>(orgPK), Factory.Load<WhsWarehouse>(whsPK), locnString).PK;
		}

		BusinessObject IWhsTransactionTestHelper.CreatePickface(ZGuid orgPK, ZGuid whsPK, ZGuid productPK, ZGuid locationPK)
		{
			return CreateProductPickFace(WhsProduct.GetWhsProduct(Factory.Load<OrgSupplierPart>(productPK)), Factory.Load<OrgHeader>(orgPK), Factory.Load<WhsLocation>(locationPK));
		}

		BusinessObject IWhsTransactionTestHelper.CreateProduct(ZGuid orgPK, ZString productCode)
		{
			return CreateProduct(Factory.Load<OrgHeader>(orgPK), productCode);
		}

		BusinessObject IWhsTransactionTestHelper.CreateProductClientRelationShip(ZGuid ownerPK, ZGuid productPK)
		{
			return CreateProductClientRelationShip(Factory.Load<OrgHeader>(ownerPK), Factory.Load<OrgSupplierPart>(productPK));
		}

		BusinessObject IWhsTransactionTestHelper.CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZString receiveUQ, ZShort maximumShelfLife)
		{
			return CreateProductParamsByWhsAndClient(partPK, clientPK, warehousePK, minimum, economicQty, replenishmentMultiple, receiveUQ, maximumShelfLife);
		}

		IWhsProductParamsByWhsAndClient IWhsTransactionTestHelper.CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZShort maximumShelfLife)
		{
			var part = Argument.NotNull(Factory.Load<OrgSupplierPart>(partPK), "part");
			var client = Argument.NotNull(Factory.Load<OrgHeader>(clientPK), "client");
			var warehouse = Argument.NotNull(Factory.Load<WhsWarehouse>(warehousePK), "warehouse");
			return CreateProductParamsByWhsAndClient(part, client, warehouse, maximumShelfLife);
		}
		IWhsProductParamsByWhsAndClient IWhsTransactionTestHelper.CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZGuid stagingLocationBOMPK, ZGuid putawayAreaPK)
		{
			var part = Argument.NotNull(Factory.Load<OrgSupplierPart>(partPK), "part");
			var client = Argument.NotNull(Factory.Load<OrgHeader>(clientPK), "client");
			var warehouse = Argument.NotNull(Factory.Load<WhsWarehouse>(warehousePK), "warehouse");
			var stagingLocationBOM = Argument.NotNull(Factory.Load<WhsLocation>(stagingLocationBOMPK), "stagingLocationBOM");
			return CreateProductParamsByWhsAndClient(part, client, warehouse, stagingLocationBOMPK);
		}

		IWhsProductParamsByWhsAndClient IWhsTransactionTestHelper.GetProductParamsByWhsAndClient(ZGuid partPK, ZGuid warehousePK, ZGuid clientPK)
		{
			var part = Argument.NotNull(Factory.Load<OrgSupplierPart>(partPK), "part");
			return WhsProduct.GetWhsProduct(part).GetParamsByWhsAndClient(warehousePK, clientPK);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsAdjustment(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify)
		{
			return CreateWhsAdjustment(orgPK, whsPK, reference, notify);
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString)
		{
			return CreateWhsAdjustmentLine(Factory.Load<WhsAdjustment>(docketPK), productPK, quantity, locnString).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationPK)
		{
			return CreateWhsAdjustmentLine(Factory.Load<WhsAdjustment>(docketPK), Factory.Load<OrgSupplierPart>(productPK), quantity, locationPK).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString, ZString pa1, ZString pa2, ZString pa3, string bondedEntryKey, decimal perPackageQty, string packageGroupID, string serialNumber)
		{
			var adjustmentLine = CreateWhsAdjustmentLine(Factory.Load<WhsAdjustment>(docketPK), productPK, quantity, locnString, pa1, pa2, pa3, "", ZDate.Empty, ZDate.Empty);
			adjustmentLine.WE_BondedEntryKey = bondedEntryKey;
			adjustmentLine.WE_AdjustmentArrivalDate = quantity > 0 ? adjustmentLine.WE_AdjustmentArrivalDate : ZDateTimeOffset.Empty;
			adjustmentLine.WE_PerPackageQty = perPackageQty;
			adjustmentLine.WE_PackageGroupId = packageGroupID;
			adjustmentLine.WE_SerialNumber = serialNumber;

			return adjustmentLine.PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsReceive(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify)
		{
			return CreateWhsReceive(orgPK, whsPK, reference, notify).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsReceive(ZGuid orgPK, ZGuid whsPK, ZString reference, ZString docketSubType, NotificationBuffer notify)
		{
			var receive = CreateWhsReceive(orgPK, whsPK, reference, notify);
			receive.WD_DocketSubType = docketSubType;
			return receive.PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString)
		{
			var inventory = CreateWhsReceiveInventoryLine(Factory.Load<WhsReceive>(docketPK), productPK, quantity);
			inventory.LocationString = locnString;
			return inventory.PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString, ZString heldCode, string palletID)
		{
			var inventory = CreateWhsReceiveInventoryLine(Factory.Load<WhsReceive>(docketPK), productPK, quantity);
			inventory.LocationString = locnString;
			inventory.OriginalInventoryHeldCode = heldCode;
			if (palletID != null)
			{
				inventory.WI_PalletID = palletID;
			}
			return inventory.PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationPK)
		{
			WhsInventoryView inventory = CreateWhsReceiveInventoryLine(Factory.Load<WhsReceive>(docketPK), productPK, quantity);
			inventory.WI_WL = locationPK;
			return inventory.PK;
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString entryKey)
		{
			return CreateWhsReceiveInventoryLine(Factory.Load<WhsReceive>(docketPK), productPK, units, ZGuid.Empty, "", expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, "", entryKey);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString entryKey, ZString locnString)
		{
			var inventory = CreateWhsReceiveInventoryLine(Factory.Load<WhsReceive>(docketPK), productPK, units, ZGuid.Empty, "", expiryDate, packingDate, partAttrib1, partAttrib2, partAttrib3, "", entryKey);
			inventory.LocationString = locnString;
			return inventory;
		}

		IWhsInventoryView IWhsTransactionTestHelper.CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal perPackageQty, ZDecimal units, ZDecimal currentQty, ZString packageGroupId, ZString packType, ZDateTimeOffset arrivalDate, ZString entryKey, ZString locnString)
		{
			var receive = Factory.Load<WhsReceive>(docketPK);
			var line = CreateWhsReceiveLine(receive, productPK, units, entryKey);
			line.WE_PerPackageQty = perPackageQty;
			line.WE_PackageGroupId = packageGroupId;
			line.WE_F3_NKPackType = packType.IsEmpty ? (ZString)"UNT" : packType; // should be before setting total units
			line.WE_StockOnHand = currentQty;
			line.WE_WE_OriginalDocketLineForRating = line.PK;
			line.WE_AdjustmentArrivalDate = arrivalDate;

			var inventory = line.Inventory[0];
			inventory.LocationString = locnString;
			return inventory;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsOrder(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify)
		{
			return CreateWhsOrder(orgPK, whsPK, reference, notify).PK;
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consigneePK, ZString reference)
		{
			return CreateWhsOrder(clientPK, whsPK, consigneePK, reference);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsOrderLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString inwardsEntryKey, ZString outwardsEntryKey, ZString tariff, ZString countryOfOrigin,
			ZString primaryPreference, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty,
			ZDecimal customsSecondQuantity, ZString customsSecondUnitQty, ZString addInfo, ZDecimal bondedWhsQty, ZGuid manufacturerAddress, ZString zoneStatus)
		{
			var orderLine = CreateWhsOrderLine(Factory.Load<WhsOrder>(docketPK), productPK, quantity, inwardsEntryKey, outwardsEntryKey);
			orderLine.CustomsData.WB_Tariff = tariff;
			orderLine.CustomsData.WB_RN_NKCountryOfOrigin = countryOfOrigin;
			orderLine.CustomsData.WB_PrimaryPreference = primaryPreference;
			orderLine.CustomsData.WB_ValueForDuty = valueForDuty;
			orderLine.CustomsData.WB_CustomsQty = customsQty;
			orderLine.CustomsData.WB_CustomsUnitOfQty = customsUnitOfQty;
			orderLine.CustomsData.WB_CustomsSecondQuantity = customsSecondQuantity;
			orderLine.CustomsData.WB_CustomsSecondUnitQty = customsSecondUnitQty;
			orderLine.CustomsData.WB_AddInfo = addInfo;
			orderLine.CustomsData.WB_EntryKey = outwardsEntryKey;
			orderLine.CustomsData.WB_BondedWhsQty = bondedWhsQty;
			orderLine.CustomsData.WB_OA_ManufacturerAddress = manufacturerAddress;
			orderLine.CustomsData.WB_ZoneStatus = zoneStatus;
			return orderLine;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsOrderLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity)
		{
			return CreateWhsOrderLine(Factory.Load<WhsOrder>(docketPK), productPK, quantity).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsPick(ZGuid[] orderPKs)
		{
			return CreatePickNew(orderPKs.Select(orderPK => Factory.Load<WhsPickableDocket>(orderPK)).ToArray()).PK;
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsPickLine(IWhsDocketLine docketLine, IWhsInventoryView inventory, decimal qty)
		{
			return CreateWhsPickLine(Factory.Load<WhsDocketLine>(docketLine.PK), Factory.Load<WhsInventoryView>(inventory.PK), qty);
		}

		IEnumerable<IWhsPickLine> IWhsTransactionTestHelper.GetPickLines(ZGuid pickPK)
		{
			var pick = Argument.NotNull(Factory.Load<WhsPick>(pickPK), "pick");
			return pick.GetAllPickLines();
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsTransfer(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify)
		{
			return CreateWhsTransfer(orgPK, whsPK, reference, ZDateTimeOffset.Now, notify).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnStringFrom, ZString locnStringTo)
		{
			return CreateWhsTransferLine(Factory.Load<WhsTransfer>(docketPK), productPK, quantity, locnStringFrom, locnStringTo).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationFromPK, ZGuid locationToPK)
		{
			return CreateWhsTransferLine(Factory.Load<WhsTransfer>(docketPK), Factory.Load<OrgSupplierPart>(productPK), quantity, locationFromPK, locationToPK).PK;
		}

		ZGuid IWhsTransactionTestHelper.CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnStringFrom, ZString locnStringTo, ZDateTimeOffset arrivalDate, ZDate expiryDate, ZDate packingDate, ZString pa1, ZString pa2, ZString pa3)
		{
			return CreateWhsTransferLine(Factory.Load<WhsTransfer>(docketPK), productPK, quantity, locnStringFrom, "", ZGuid.Empty, locnStringTo, "", arrivalDate, expiryDate, packingDate, pa1, pa2, pa3).PK;
		}

		void IWhsTransactionTestHelper.FinaliseDocket(ZGuid docketPK)
		{
			var docket = Factory.Load<WhsDocket>(docketPK);
			AssertNotNull("Precondition - Docket exists.", docket);

			docket.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
		}

		void IWhsTransactionTestHelper.FinaliseDocketLine(ZGuid docketLinePK)
		{
			var docketLine = Factory.Load<WhsDocketLine>(docketLinePK);
			AssertNotNull("Precondition - DocketLine exists.", docketLine);

			var transferLine = docketLine as WhsTransferLine;
			AssertNotNull("Precondition - DocketLine must be able to finalised independent of its docket.", transferLine); // Only transfers does this currently

			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);
		}

		void IWhsTransactionTestHelper.FinaliseDocketWithoutUserConfirmation(ZGuid docketPK)
		{
			var docket = Factory.Load<WhsDocket>(docketPK);
			AssertNotNull("Precondition - Docket exists.", docket);

			docket.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
		}

		void IWhsTransactionTestHelper.FinalisePick(ZGuid pickPK)
		{
			var pick = Factory.Load<WhsPick>(pickPK);
			AssertNotNull("Precondition - Pick exists.", pick);

			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
		}

		IDisposable IWhsTransactionTestHelper.MockOutboundDockDoorCreator() => OutboundDockDoorHelper.MockOutboundDockDoorCreator();

		IWhsDocketLine IWhsTransactionTestHelper.PickAndMakeInTransitTransfer(IWhsPickLine pickLine, ZDateTime pickedTime, bool allowMultipleSteps)
		{
			return PickAndMakeInTransitTransfer((WhsPickLine)pickLine, new ZDateTimeOffset(pickedTime), allowMultipleSteps);
		}

		void IWhsTransactionTestHelper.WhsReceiveAllocateLocationsMock(ZGuid docketPK)
		{
			Factory.Load<WhsReceive>(docketPK).AllocateLocationsWithMock();
		}

		void IWhsTransactionTestHelper.WhsPickAllocationItems(ZGuid pickPK)
		{
			Factory.Load<WhsPick>(pickPK).AutoAllocateItemsWithMock();
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsPickTrolleyJob(ZGuid trolleyPK, ZString jobStatus)
		{
			return (BusinessObject)CreateWhsPickTrolleyJob(trolleyPK, jobStatus);
		}

		BusinessObject IWhsTransactionTestHelper.CreateWhsPickTrolleySlot(ZGuid trolleyJobPK, ZGuid packagePK, ZShort slot)
		{
			return (BusinessObject)CreateWhsPickTrolleySlot(trolleyJobPK, packagePK, slot);
		}

		IDisposable IWhsTransactionTestHelper.UsePutawayEngineManagerMock()
		{
			return ObjectFactory.Substitute(PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive().Object);
		}

		IDisposable IWhsTransactionTestHelper.UseAllocationEngineMock() => ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock());

		BusinessObject IWhsTransactionTestHelper.CreateWhsSalesChannel(string code, string description)
		{
			return CreateWhsSalesChannel(code, description);
		}

		void IWhsTransactionTestHelper.SetUpBondedWarehouse(ZGuid whsPK, ZGuid addressPK)
		{
			var whs = Factory.Load<WhsWarehouse>(whsPK);
			var address = Factory.Load<OrgAddress>(addressPK);
			whs.SetUpBondedWarehouse(address);
		}

		void IWhsTransactionTestHelper.EnableWarehouseForBond(IWhsWarehouse whs, bool enable) => EnableWarehouseForBond((WhsWarehouse)whs, enable);

		IWhsPickLine IWhsTransactionTestHelper.ReserveStockForOrderLineIfAbleTo(IWhsDocketLine orderLine, IWhsInventoryView inventory)
		{
			var whsOrderLine = Argument.NotNull(orderLine as WhsOrderLine, $"PreCondition - {nameof(orderLine)} is {nameof(WhsOrderLine)}.");
			var whsInventoryView = inventory as WhsInventoryView;
			return whsOrderLine.ReserveStockIfAbleTo(whsInventoryView);
		}

		BusinessObject IWhsTransactionTestHelper.CreateProductBOM(ZGuid partPK, ZGuid subPartPK, ZDecimal componentQty)
		{
			var subPart = Factory.Load<OrgSupplierPart>(subPartPK);
			return CreateProductBOM(Factory.Load<OrgSupplierPart>(partPK), subPart, componentQty, subPart.OP_StockKeepingUnit);
		}

		IWhsDocket IWhsTransactionTestHelper.CreateWhsWorkOrderWithLine(ZGuid clientPK, ZGuid warehousePK, ZGuid partPK, ZDecimal qty) => CreateWhsWorkOrderWithLine(Factory.Load<OrgHeader>(clientPK), Factory.Load<WhsWarehouse>(warehousePK), Factory.Load<OrgSupplierPart>(partPK), qty);

		#endregion

		#region SetPickedDate

		public void SetPickedDate(WhsPickAvailableInventory availableInventory, ZDateTimeOffset pickedDate)
		{
			foreach (var pickLine in availableInventory.PickLines)
			{
				pickLine.WZ_PickedDateTime = pickedDate;
			}
		}

		#endregion

		#region SetAssignToPk

		public void SetAssignToPk(WhsPickAvailableInventory availableInventory, ZGuid assignToPk)
		{
			if (availableInventory.PickLines.Any())
			{
				var assignedTo = Factory.Load<GlbStaff>(assignToPk);
				foreach (WhsPickLine pickLine in availableInventory.PickLines)
				{
					pickLine.WZ_GS_NKAssignedTo = assignedTo?.GS_Code ?? ZString.Empty;
				}
			}
		}

		#endregion

		#region SuspendTrigger

		public static IDisposable SuspendTrigger(string triggerName, string tableName, DbConnection connection = null)
		{
			var conn = connection ?? Db.Connection;
			return new DisposableAction(
				() => conn.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}"),
				() => conn.ExecuteNonQuery($"ENABLE TRIGGER {triggerName} ON {tableName}")
			);
		}

		public static void DisableTrigger(string triggerName, string tableName)
		{
			Db.Connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName}");
		}

		#endregion

		#region SuspendTriggerAndRunAtEnd

		public static IDisposable SuspendTriggerAndRunAtEnd(DbConnection connection, string triggerName, string tableName, string pkColumnName, string checkProcedureName)
		{
			return new DisposableAction(
				() => connection.ExecuteNonQuery($"DISABLE TRIGGER {triggerName} ON {tableName};"),
				() =>
				{
					connection.ExecuteNonQuery($@"ENABLE TRIGGER {triggerName} ON {tableName};

DECLARE @PKs dbo.TVP_uniqueidentifier;
INSERT INTO @PKs
SELECT DISTINCT {pkColumnName} FROM {tableName}

EXEC {checkProcedureName} @PKs;
");
				});
		}

		#endregion

		#region SuspendDatabaseProcForTest

		public static void SuspendDatabaseProcForTest(string procArguments, string returnValue, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			connection.ExecuteNonQuery($@"
ALTER PROC {procArguments} AS
BEGIN
	RETURN {returnValue}
END");
		}

		#endregion

		#region DisableConstraint

		public static IDisposable DisableConstraint(string tableName, string constraintName, DbConnection connection = null)
		{
			var dbConn = connection ?? Db.Connection;
			return new DisposableAction(
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} NOCHECK CONSTRAINT[{constraintName}]"),
				 () => dbConn.ExecuteNonQuery($"IF OBJECT_ID('{constraintName}', 'C') IS NOT NULL ALTER TABLE {tableName} CHECK CONSTRAINT[{constraintName}]"));
		}

		#endregion

		#region DisablePageLocks

		public static void DisablePageLocks(DbConnection connection)
		{
			// Due to lack of data in test DB, row locks sometimes get escalated to page locks, causing locking of unintended records
			// This can cause some strange test behaviours
			connection.ExecuteNonQuery(@"
				ALTER INDEX ALL ON WhsPick SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsDocket SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsDocketLine SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsPickLine SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsLocation SET (ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsRow SET (ALLOW_PAGE_LOCKS = OFF)");
		}

		#endregion

		#region PutawayEngineManagerMock

		public IDisposable GetPutawayEngineManagerForVASTransferLineMock()
		{
			var putawayEngineMock = new Mock<IPutawayEngineManagerForVASTransferLine>(MockBehavior.Strict);
			putawayEngineMock.Setup(putawayEngine => putawayEngine.Putaway(It.IsAny<IEnumerable<WhsVASOrder>>(), It.IsAny<IEnumerable<VASReturnTransferLine>>(), It.IsNotNull<INotifications>(), null))
				.Callback((Action<IEnumerable<WhsVASOrder>, IEnumerable<VASReturnTransferLine>, INotifications, RefEquipment>)((vasOrderParam, vasReturnTransferLines, iNotifications, refEquipment) =>
				{
					var vasOrder = vasOrderParam.First();
					foreach (var vasTransferLine in vasReturnTransferLines)
					{
						vasTransferLine.TransferLine.WE_WL = vasOrder.Warehouse.DefaultLocation.PK;
					}
				}));

			return ObjectFactory.Substitute(putawayEngineMock.Object);
		}

		#endregion

		#region GetChangedValue

		public IZType GetChangedValue(ZPropertyInfo info)
		{
			switch (info.Value)
			{
				case ZBool zBool:
					return (ZBool)!zBool;
				case ZByte zByte:
					return (ZByte)(zByte + 1);
				case ZBlob zBlob:
					var blobValue = new ZBlob(new byte[] { 1, 2, 3 });
					return zBlob != blobValue ? blobValue : new ZBlob(new byte[] { 4, 5, 6 });
				case ZInt zInt:
					return (ZInt)(zInt + 1);
				case ZString zString:
					var stringValue = new ZString("A");
					return zString != stringValue ? stringValue : new ZString("Z");
				case ZDateTime zDateTime:
					return zDateTime.IsEmpty ? ZDateTime.Today : zDateTime.AddDays(1);
				case ZDateTimeOffset zDateTimeOffset:
					return zDateTimeOffset.IsEmpty ? ZDateTimeOffset.Today : zDateTimeOffset.AddDays(1);
				case ZDate zDate:
					return zDate.IsEmpty ? ZDate.Today : zDate.AddDays(1);
				case ZGuid zGuid:
					return ZGuid.NewZGuid();
				case ZShort zShort:
					return zShort + 1;
				case ZDecimal zDecimal:
					return (ZDecimal)(zDecimal + 1);
				default:
					throw new InvalidOperationException("ZType " + info.Value.GetType().Name + " is not supported in this test. Consider adding it above.");
			}
		}

		#endregion

		#region CreateWhsLoad

		ZGuid IWhsTransactionTestHelper.CreateWhsLoad(ZGuid transportCompanyPK, ZGuid dockDoorPK, ZString taskPlanningStatus, string jobID)
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_OH_TransportCompany = transportCompanyPK;
			load.WLO_WL_PlannedDockDoor = dockDoorPK;
			load.WLO_PL_NKCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			load.WLO_TaskPlanningStatus = taskPlanningStatus;
			load.WLO_JobID = jobID;
			return load.PK;
		}

		public WhsLoad CreateWhsLoad(OrgHeader transportCompany, WhsLocation dockDoor, string jobID = null, string carrierServiceLevel = null, RefEquipment transportUnit = null, DateTimeOffset? startTime = null)
		{
			var load = Factory.New<WhsLoad>();
			load.WLO_OH_TransportCompany = transportCompany.PK;
			load.WLO_WL_PlannedDockDoor = dockDoor.PK;
			load.WLO_PL_NKCarrierServiceLevel = carrierServiceLevel ?? OrgCarrierServiceLevel.StandardCode;
			load.WLO_JobID = jobID;
			load.WLO_RQ_TransportationUnit = transportUnit?.PK ?? ZGuid.Empty;
			if (startTime.HasValue)
			{
				load.WLO_StartTime = startTime.Value;
				load.WLO_TransportationUnitNumber = transportUnit?.RQ_Registration ?? "ABC456";
			}
			return load;
		}

		#endregion

		#region WhsSerialNumber

		public WhsSerialNumber CreateWhsSerialNumber(OrgHeader client, OrgSupplierPart product, string snNumber)
		{
			var serialNumber = Factory.New<WhsSerialNumber>();
			serialNumber.WSN_OH_Client = client.PK;
			serialNumber.WSN_OP_Product = product.PK;
			serialNumber.WSN_SerialNumber = snNumber;
			return serialNumber;
		}

		public WhsSerialNumberPivot CreateWhsSerialNumberPivot(WhsReceiveLine receiveLine, WhsSerialNumber serialNumber)
		{
			var serialNumberPivot = Factory.New<WhsSerialNumberPivot>();
			serialNumberPivot.WSV_ParentID = receiveLine.PK;
			serialNumberPivot.WSV_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
			serialNumberPivot.WSV_IsReleaseCaptured = false;
			serialNumberPivot.WSV_WSN_SerialNumber = serialNumber.PK;
			return serialNumberPivot;
		}

		public WhsSerialNumberPivot CreateWhsSerialNumberPivot(WhsPickLine pickLine, WhsSerialNumber serialNumber)
		{
			var serialNumberPivot = Factory.New<WhsSerialNumberPivot>();
			serialNumberPivot.WSV_ParentID = pickLine.PK;
			serialNumberPivot.WSV_ParentTableCode = WhsPickLineSchema.Constants.Prefix;
			serialNumberPivot.WSV_IsReleaseCaptured = true;
			serialNumberPivot.WSV_WZ_PickingLine = pickLine.PK;
			serialNumberPivot.WSV_WSN_SerialNumber = serialNumber.PK;
			return serialNumberPivot;
		}

		public WhsSerialNumberPivot CreateWhsSerialNumberPivot(WhsAsnLine asnLine, WhsSerialNumber serialNumber)
		{
			var serialNumberPivot = Factory.New<WhsSerialNumberPivot>();
			serialNumberPivot.WSV_ParentID = asnLine.PK;
			serialNumberPivot.WSV_ParentTableCode = WhsAsnLineSchema.Constants.Prefix;
			serialNumberPivot.WSV_IsReleaseCaptured = false;
			serialNumberPivot.WSV_WSN_SerialNumber = serialNumber.PK;
			return serialNumberPivot;
		}

		#endregion

		#region FindLogs

		public StmALog[] FindLogs(Logs logs, Event lookupEvent) => logs.Find(FindLogsQuery(lookupEvent));

		ZQuery FindLogsQuery(Event lookupEvent)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, lookupEvent.Code);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			return query;
		}

		public StmALog[] FindLogs(Logs logs, Event lookupEvent, ZString lookupEventReference)
		{
			var query = FindLogsQuery(lookupEvent);
			query.AddToFilter(StmALogSchema.SL_Reference, lookupEventReference);

			return logs.Find(query);
		}

		#endregion

		#region CreateProcessTask

		public ProcessTask CreateProcessTaskForReceive(WhsReceive receive, GlbStaff staff = null)
		{
			var task = Factory.New<WhsReceiveProcessTasks>();
			task.P9_ParentID = receive.PK;
			task.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.UnloadJob;
			task.P9_GS_NKAssignedStaffMember = staff?.GS_Code ?? string.Empty;
			task.P9_Type = "UDF";
			task.P9_Description = "Some Temp Description";

			return task;
		}

		public ProcessTask CreateProcessTaskForTransfer(WhsTransfer transfer, GlbStaff staff = null)
		{
			var task = Factory.New<WhsTransferProcessTasks>();
			task.P9_ParentID = transfer.PK;
			task.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			if (staff != null)
			{
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			}
			task.P9_Type = "UDF";
			task.P9_Description = "Some Temp Description";

			if (transfer.WD_IsPutawayTransfer)
			{
				task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob;
			}
			else if (transfer.WD_IsPickFaceReplenishment)
			{
				task.P9_FormFlowType = WarehouseTaskFormFlowTypes.ReplenishmentJob;
			}
			else
			{
				task.P9_FormFlowType = WarehouseTaskFormFlowTypes.TransferJob;
			}

			foreach (var line in transfer.Lines)
			{
				line.WE_P9_Task = task.PK;
			}

			return task;
		}

		public ProcessTask CreateProcessTaskForPickJob(WhsPick pick, GlbStaff staff, bool setPickLineFKs = true)
		{
			var task = CreateProcessTaskForPick(pick, staff);
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PickJob;
			if (setPickLineFKs)
			{
				foreach (var pickLine in pick.GetAllPickLines().Where(l => l.WZ_GS_NKAssignedTo.IsEmpty || l.WZ_GS_NKAssignedTo == staff.GS_Code))
				{
					pickLine.WZ_P9_Task = task.PK;
				}
			}
			return task;
		}

		public ProcessTask CreateProcessTaskForPickByLabelJob(WhsPick pick, GlbStaff staff, IWhsPickByLabelJob pickByLabelJob)
		{
			var task = CreateProcessTaskForPick(pick, staff);
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PickByLabelJob;
			pickByLabelJob.WTK_P9_Task = task.PK;
			return task;
		}

		public ProcessTask CreateProcessTaskForDirectedPackingJob(WhsPick pick, GlbStaff staff)
		{
			var task = CreateProcessTaskForPick(pick, staff);
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.DirectedPackingJob;
			var order = pick.Orders.Cast<WhsOrder>().Single();
			order.WD_P9_PackingTask = task.PK;
			return task;
		}

		ProcessTask CreateProcessTaskForPick(WhsPick pick, GlbStaff staff)
		{
			var task = Factory.New<WhsPickProcessTask>();
			task.P9_ParentID = pick.PK;
			task.P9_ParentTableCode = WhsPickSchema.Constants.Prefix;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			return task;
		}

		public ProcessTask CreateProcessTaskForLoad(WhsLoad load, GlbStaff staff)
		{
			var task = Factory.New<WhsLoadProcessTask>();
			task.P9_ParentID = load.PK;
			task.P9_ParentTableCode = WhsLoadSchema.Constants.Prefix;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.LoadJob;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			return task;
		}

		public ProcessTask CreateProcessTaskForCycleCountWave(WhsCycleCountWave wave, GlbStaff staff)
		{
			var task = Factory.New<WhsCycleCountWaveProcessTask>();
			task.P9_ParentID = wave.PK;
			task.P9_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.CycleCountJob;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			return task;
		}

		#endregion

		#region AssertZCannotSaveExceptionThrown

		public void AssertZCannotSaveExceptionThrown(string expectedMessage, AnonymousMethod codeToRun)
		{
			var ex1 = (ZCannotSaveException)null;
			try
			{
				codeToRun();
			}
			catch (Exception ex2)
			{
				AssertEquals(true, ex2 is ZCannotSaveException);

				ex1 = (ZCannotSaveException)ex2;
				CombineAssertions(() =>
				{
					AssertEquals(expectedMessage, ex1.Message);
					AssertEquals(ExceptionType.BusinessFailure, ex1.Type);
				});
			}

			AssertNotNull("Expected an exception of type ZCannotSaveException but none were thrown.", ex1);
		}

		#endregion
	}

	#endregion

	#region TestILineAttributes

	public class TestILineAttributes : ILineAttributes
	{
		#region ctor

		public TestILineAttributes()
		{
		}

		public TestILineAttributes(string bondedEntryKey, ZDate expiryDate, ZDate packingDate, string partAttrib1, string partAttrib2, string partAttrib3)
		{
			BondedEntryKey = bondedEntryKey;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
			PartAttrib1 = partAttrib1;
			PartAttrib2 = partAttrib2;
			PartAttrib3 = partAttrib3;
		}

		public TestILineAttributes(string bondedEntryKey, ZDate expiryDate, ZDate packingDate, string partAttrib1, string partAttrib2, string partAttrib3, string serialNumber)
		{
			BondedEntryKey = bondedEntryKey;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
			PartAttrib1 = partAttrib1;
			PartAttrib2 = partAttrib2;
			PartAttrib3 = partAttrib3;
			SerialNumber = serialNumber;
		}

		#endregion

		#region ILineAttributes Members

		public ZDate ExpiryDate
		{
			get;
			set;
		}

		public ZDate PackingDate
		{
			get;
			set;
		}

		public ZString BondedEntryKey
		{
			get;
			set;
		}

		public ZString AllocationKey
		{
			get;
			set;
		}

		public ZString PartAttrib1
		{
			get;
			set;
		}

		public ZString PartAttrib2
		{
			get;
			set;
		}

		public ZString PartAttrib3
		{
			get;
			set;
		}

		public ZString SerialNumber
		{
			get;
			set;
		}

		public void SetAttributes(ILineAttributes a)
		{
			BondedEntryKey = a.BondedEntryKey.ToUpper();
			ExpiryDate = a.ExpiryDate;
			PackingDate = a.PackingDate;
			PartAttrib1 = a.PartAttrib1;
			PartAttrib2 = a.PartAttrib2;
			PartAttrib3 = a.PartAttrib3;
			SerialNumber = a.SerialNumber;
		}

		#endregion
	}

	#endregion

	#region TestILineCustomAttributes

	public class TestILineCustomAttributes : ILineCustomAttributes
	{
		#region ctor

		public TestILineCustomAttributes()
		{
		}

		public TestILineCustomAttributes(string customAttrib1, string customAttrib2, string customAttrib3, string customAttrib4, string customAttrib5, string customAttrib6, decimal customDecimal1, decimal customDecimal2, decimal customDecimal3, decimal customDecimal4, decimal customDecimal5, ZDateTime customDate1, ZDateTime customDate2, ZDateTime customDate3, ZDateTime customDate4, ZDateTime customDate5, bool customFlag1, bool customFlag2, bool customFlag3, bool customFlag4, bool customFlag5, string customTextBlob1)
		{
			CustomAttrib1 = customAttrib1;
			CustomAttrib2 = customAttrib2;
			CustomAttrib3 = customAttrib3;
			CustomAttrib4 = customAttrib4;
			CustomAttrib5 = customAttrib5;
			CustomAttrib6 = customAttrib6;
			CustomDecimal1 = customDecimal1;
			CustomDecimal2 = customDecimal2;
			CustomDecimal3 = customDecimal3;
			CustomDecimal4 = customDecimal4;
			CustomDecimal5 = customDecimal5;
			CustomDate1 = customDate1;
			CustomDate2 = customDate2;
			CustomDate3 = customDate3;
			CustomDate4 = customDate4;
			CustomDate5 = customDate5;
			CustomFlag1 = customFlag1;
			CustomFlag2 = customFlag2;
			CustomFlag3 = customFlag3;
			CustomFlag4 = customFlag4;
			CustomFlag5 = customFlag5;
			CustomTextBlob1 = customTextBlob1;
		}

		#endregion

		#region TestILineCustomAttributes Members

		public ZString CustomAttrib1
		{
			get;
			set;
		}

		public ZString CustomAttrib2
		{
			get;
			set;
		}

		public ZString CustomAttrib3
		{
			get;
			set;
		}

		public ZString CustomAttrib4
		{
			get;
			set;
		}

		public ZString CustomAttrib5
		{
			get;
			set;
		}

		public ZString CustomAttrib6
		{
			get;
			set;
		}

		public ZDecimal CustomDecimal1
		{
			get;
			set;
		}

		public ZDecimal CustomDecimal2
		{
			get;
			set;
		}

		public ZDecimal CustomDecimal3
		{
			get;
			set;
		}

		public ZDecimal CustomDecimal4
		{
			get;
			set;
		}

		public ZDecimal CustomDecimal5
		{
			get;
			set;
		}

		public ZDateTime CustomDate1
		{
			get;
			set;
		}

		public ZDateTime CustomDate2
		{
			get;
			set;
		}

		public ZDateTime CustomDate3
		{
			get;
			set;
		}

		public ZDateTime CustomDate4
		{
			get;
			set;
		}

		public ZDateTime CustomDate5
		{
			get;
			set;
		}

		public ZBool CustomFlag1
		{
			get;
			set;
		}

		public ZBool CustomFlag2
		{
			get;
			set;
		}

		public ZBool CustomFlag3
		{
			get;
			set;
		}

		public ZBool CustomFlag4
		{
			get;
			set;
		}

		public ZBool CustomFlag5
		{
			get;
			set;
		}

		public ZString CustomTextBlob1
		{
			get;
			set;
		}

		public void SetCustomAttributes(ILineCustomAttributes a)
		{
			CustomAttrib1 = a.CustomAttrib1;
			CustomAttrib2 = a.CustomAttrib2;
			CustomAttrib3 = a.CustomAttrib3;
			CustomAttrib4 = a.CustomAttrib4;
			CustomAttrib5 = a.CustomAttrib5;
			CustomAttrib6 = a.CustomAttrib6;
			CustomDecimal1 = a.CustomDecimal1;
			CustomDecimal2 = a.CustomDecimal2;
			CustomDecimal3 = a.CustomDecimal3;
			CustomDecimal4 = a.CustomDecimal4;
			CustomDecimal5 = a.CustomDecimal5;
			CustomDate1 = a.CustomDate1;
			CustomDate2 = a.CustomDate2;
			CustomDate3 = a.CustomDate3;
			CustomDate4 = a.CustomDate4;
			CustomDate5 = a.CustomDate5;
			CustomFlag1 = a.CustomFlag1;
			CustomFlag2 = a.CustomFlag2;
			CustomFlag3 = a.CustomFlag3;
			CustomFlag4 = a.CustomFlag4;
			CustomFlag5 = a.CustomFlag5;
			CustomTextBlob1 = a.CustomTextBlob1;
		}

		#endregion
	}

	#endregion

	#region TestIPickForm

	public class TestIPickForm : IPickForm
	{
		public TestIPickForm()
		{
		}

		public TestIPickForm(WhsOrder order)
		{
			this.order = order;
		}

		public WhsOrder CurrentOrder
		{
			get { return order; }
		}

		public List<WhsPickableDocketLine> SelectedOrderLines
		{
			get { return (order != null) ? new List<WhsPickableDocketLine>(CurrentOrder.Lines.ToArray<WhsPickableDocketLine>()) : null; }
		}

		readonly WhsOrder order;
	}

	#endregion

	#region SimpleDocketForTesting

	public abstract class TestDataSimpleDocket
	{
		public TestDataSimpleDocket(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		WhsTestHelperFunctions fHelper;
		public WhsTestHelperFunctions Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new WhsTestHelperFunctions(Factory);
				}
				return fHelper;
			}
		}

		WhsWarehouse fWhs;
		public WhsWarehouse Whs
		{
			get
			{
				if (fWhs == null)
				{
					fWhs = Helper.CreateWarehouse("WHS");
					Helper.CreateRowAndGenerateLocations(Whs, "A", 2, 1);
				}
				return fWhs;
			}
			set { fWhs = value; }
		}

		WhsRow fRow;
		public WhsRow Row
		{
			get
			{
				if (fRow == null)
				{
					fRow = fWhs.Rows[0];
				}
				return fRow;
			}
			set { fRow = value; }
		}

		WhsLocation fLoc1;
		public WhsLocation Loc1
		{
			get
			{
				if (fLoc1 == null)
				{
					fLoc1 = Row.Locations[0];
				}
				return fLoc1;
			}
			set { fLoc1 = value; }
		}

		WhsLocation fLoc2;
		public WhsLocation Loc2
		{
			get
			{
				if (fLoc2 == null)
				{
					fLoc2 = Row.Locations[1];
				}
				return fLoc2;
			}
			set { fLoc2 = value; }
		}

		OrgHeader fOrg;
		public OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Helper.CreateClient("ORG1", "ORG1");
				}
				return fOrg;
			}
			set { fOrg = value; }
		}

		OrgSupplierPart fPart1;
		public OrgSupplierPart Part1
		{
			get
			{
				if (fPart1 == null)
				{
					fPart1 = Helper.CreateProduct(Org, "P1");
				}
				return fPart1;
			}
			set { fPart1 = value; }
		}

		OrgSupplierPart fPart2;
		public OrgSupplierPart Part2
		{
			get
			{
				if (fPart2 == null)
				{
					fPart2 = Helper.CreateProduct(Org, "P2");
				}
				return fPart2;
			}
			set { fPart2 = value; }
		}

		public WhsDocket fDocket;
		public WhsDocket Docket
		{
			get
			{
				if (fDocket == null)
				{
					fDocket = GetDocket();
				}
				return fDocket;
			}
			set { fDocket = value; }
		}

		WhsDocketLine fLine;
		public WhsDocketLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = GetDocketLine();
				}
				return fLine;
			}
			set { fLine = value; }
		}

		protected abstract WhsDocket GetDocket();
		protected abstract WhsDocketLine GetDocketLine();

		public void RemoveLineFromParent()
		{
			Line.WE_WD = ZGuid.Empty;
		}

		public void AttachLineToParent()
		{
			Line.WE_WD = Docket.PK;
		}
	}

	public class TestDataSimpleDocketAdjustmentDocket : TestDataSimpleDocket
	{
		public TestDataSimpleDocketAdjustmentDocket(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsDocket GetDocket()
		{
			return Helper.CreateWhsAdjustment(Org, Whs, "REF01");
		}

		protected override WhsDocketLine GetDocketLine()
		{
			WhsAdjustmentLine line = Docket.Lines.AddNew();
			return line;
		}

		public new WhsAdjustment Docket
		{
			get { return (WhsAdjustment)base.Docket; }
			set { base.Docket = value; }
		}

		public new WhsAdjustmentLine Line
		{
			get { return (WhsAdjustmentLine)base.Line; }
			set { base.Line = value; }
		}
	}

	#endregion

	#region FinalisableDocketHelperForTesting

	#region FinalisableDocketHelper

	public abstract class FinalisableDocketHelper<TDocket> : Assertion
		where TDocket : WhsDocket
	{
		public FinalisableDocketHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public TDocket GetNewFinalisableDocketWithOneLine(TestDataSimpleEnvironment data)
		{
			var result = GetNewFinalisableDocketWithOneLineCore(data, 10m);
			Factory.Save();

			return result;
		}

		public TDocket GetNewFinalisableDocketWithOneLine(TestDataSimpleEnvironment data, decimal units, string externalReference = null, bool finalise = false, string originalHoldCode = null)
		{
			var result = GetNewFinalisableDocketWithOneLineCore(data, units, externalReference, originalHoldCode);

			if (finalise)
			{
				result.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(result);
			}

			Factory.Save();

			return result;
		}

		protected abstract TDocket GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null);

		#region Implementation

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		protected BusinessObjectFactory Factory;

		#endregion
	}

	#endregion

	#region FinalisableReceiveHelper

	public class FinalisableReceiveHelper : FinalisableDocketHelper<WhsReceive>
	{
		public FinalisableReceiveHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsReceive GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, externalReference ?? "R1", data.Part1, units, finalise: false);
			receive.Lines[0].WE_WHC_NKOriginalInventoryHeldCode = originalHoldCode;
			receive.NotificationManager.Push(Helper.Notify);
			return receive;
		}
	}

	#endregion

	#region FinalisableTransferHelper

	public class FinalisableTransferHelper : FinalisableDocketHelper<WhsTransfer>
	{
		public FinalisableTransferHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsTransfer GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			var receive = new FinalisableReceiveHelper(Factory).GetNewFinalisableDocketWithOneLine(data, units, externalReference, finalise: true, originalHoldCode: originalHoldCode);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, externalReference ?? "T1", Helper.Notify);
			var locationFrom = receive.Lines[0].Location.ToLocationString();
			var locationTo = data.Whs1.FindLocation("A-2") != null ? (ZString)"A-2" : locationFrom;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1, units, locationFrom, "", locationTo, "");
			line.WE_WHC_NKOriginalInventoryHeldCode = originalHoldCode;
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is Committed.", units, line.QtyCommittedIncludingMatchingLines);
			return transfer;
		}
	}

	#endregion

	#region FinalisableAdjustmentHelper

	public class FinalisableAdjustmentHelper : FinalisableDocketHelper<WhsAdjustment>
	{
		public FinalisableAdjustmentHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsAdjustment GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			WhsLocation locationFrom = null;

			if (units < 0)
			{
				var receive = new FinalisableReceiveHelper(Factory).GetNewFinalisableDocketWithOneLine(data, -units, externalReference, finalise: true, originalHoldCode: originalHoldCode);
				locationFrom = receive.Lines[0].Location;
			}

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, externalReference ?? "A1", Helper.Notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, units, locationFrom ?? data.Whs1.DefaultLocation);
			adjustmentLine.WE_WHC_NKOriginalInventoryHeldCode = originalHoldCode;

			if (units < 0)
			{
				adjustment.RunPreSaveValidation();
				AssertEquals("Precondition: Stock is Committed.", -units, adjustmentLine.CommittedQuantity);
			}

			return adjustment;
		}
	}

	#endregion

	#region FinalisableOrderHelper

	public class FinalisableOrderHelper : FinalisableDocketHelper<WhsOrder>
	{
		public FinalisableOrderHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsOrder GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			new FinalisableReceiveHelper(Factory).GetNewFinalisableDocketWithOneLine(data, units, externalReference, finalise: true);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, externalReference ?? "O1", data.Part1, units);
			order.NotificationManager.Push(Helper.Notify);
			Helper.CreatePickNew(order);
			return order;
		}
	}

	#endregion

	#region FinalisableWorkOrderHelper

	public class FinalisableWorkOrderHelper : FinalisableDocketHelper<WhsWorkOrder>
	{
		public FinalisableWorkOrderHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsWorkOrder GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, data.Part2.OP_StockKeepingUnit);

			var receive = new FinalisableReceiveHelper(Factory).GetNewFinalisableDocketWithOneLine(data, units, externalReference, finalise: true);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, externalReference ?? "O1", data.Part2, units / 2);
			workOrder.NotificationManager.Push(Helper.Notify);
			Helper.CreatePickNew(workOrder);
			return workOrder;
		}
	}

	#endregion

	#region FinalisableDynamicWorkOrderHelper

	public class FinalisableDynamicWorkOrderHelper : FinalisableDocketHelper<WhsDynamicWorkOrder>
	{
		public FinalisableDynamicWorkOrderHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsDynamicWorkOrder GetNewFinalisableDocketWithOneLineCore(TestDataSimpleEnvironment data, decimal units, string externalReference = null, string originalHoldCode = null)
		{
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var inwardProcessingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "IPR", 1, 1).Locations[0];
			inwardProcessingLocation.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			inwardProcessingLocation.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_IsInwardsProcessingJob = true;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var recLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, units, inwardProcessingLocation);
			recLine1.CustomsData.WB_EntryKey = "ENT - 1";
			receive.FinaliseDocket();
			Factory.Save();
			Assert("Precondition", receive.IsFinalised);

			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = data.Org1.PK;
			workOrder.WD_WW_Whs = data.Whs1.PK;
			workOrder.WD_ExternalReference = externalReference ?? "O1";
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			var parentLine = workOrder.Lines.AddNew();
			parentLine.WE_OP = data.Part2.PK;
			parentLine.WE_TransactionQuantity = units / 2;
			parentLine.IsMainInwardProcessedItem = true;

			var childLine = workOrder.Lines.AddNew();
			childLine.WE_OP = data.Part1.PK;
			childLine.WE_TransactionQuantity = units;
			childLine.WE_WE_ParentDocketLine = parentLine.PK;

			workOrder.NotificationManager.Push(Helper.Notify);
			Helper.CreatePickNew(workOrder);
			return workOrder;
		}
	}

	#endregion

	#endregion

	#region TestDataSimpleEnvironment

	public class TestDataSimpleEnvironment : EnvTestDataSimpleEnvironment
	{
		public TestDataSimpleEnvironment(BusinessObjectFactory factory, bool saveFactory_doNotUseForNewTests = true)
			: this(factory, 1, 1, saveFactory_doNotUseForNewTests)
		{
		}

		public TestDataSimpleEnvironment(BusinessObjectFactory factory, short cols, short levels, bool saveFactory_doNotUseForNewTests = true)
			: base(factory, cols, levels, saveFactory_doNotUseForNewTests)
		{
		}

		WhsTestHelperFunctions helper;
		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
	}

	#endregion

	#region TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup

	public class TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup : TestDataSimpleEnvironment
	{
		public TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(BusinessObjectFactory factory)
			: this(factory, 1, 1)
		{
		}

		public TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(BusinessObjectFactory factory, short cols, short levels)
			: base(factory, cols, levels)
		{
		}

		protected override void CreateEnvironment()
		{
			base.CreateEnvironment();
			Whs1.WW_WarehouseCode = GetUniqueNameForFatDat(WhsWarehouseSchema.WW_WarehouseCode);
			Org1.OH_Code = GetUniqueNameForFatDat(OrgHeaderSchema.OH_Code);
		}

		public string GetUniqueNameForFatDat(SchemaColumn column)
		{
			return UniqueNameForFatDat.GetUniqueNameForFatDat(column);
		}

		UniqueNameForFatDatTest UniqueNameForFatDat => uniqueNameForFatDat ?? (uniqueNameForFatDat = new UniqueNameForFatDatTest(Factory));
		UniqueNameForFatDatTest uniqueNameForFatDat;
	}

	#endregion

	#region UniqueNameForFatDat

	public class UniqueNameForFatDatTest
	{
		public UniqueNameForFatDatTest(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public string GetUniqueNameForFatDat(SchemaColumn column)
		{
			Stack<string> uniqueKeys;
			if (!uniqueKeysByColumn.TryGetValue(column, out uniqueKeys))
			{
				uniqueKeys = GetUniqueNames(column);
				uniqueKeysByColumn.Add(column, uniqueKeys);
			}
			return uniqueKeys.Pop();
		}

		Stack<string> GetUniqueNames(SchemaColumn column)
		{
			var sql = string.Format(@"
WITH Codes (Code) AS
(
	SELECT TOP {2} CAST(ROW_NUMBER() OVER (ORDER BY object_id) as VARCHAR) as Code FROM sys.objects
)

SELECT Code FROM Codes WHERE Code NOT IN (SELECT {0} from {1})", column.Name, column.TableName, column.MaxLength == 3 ? 999 : 9999);

			var stack = new Stack<string>();
			using (var reader = ((IDbConnected)Factory).Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					stack.Push(reader.GetString(0));
				}
			}
			return stack;
		}

		readonly Dictionary<SchemaColumn, Stack<string>> uniqueKeysByColumn = new Dictionary<SchemaColumn, Stack<string>>();
		readonly BusinessObjectFactory Factory;
	}

	#endregion

	#region Test Classes for Generating Inventory

	public class TestDataForInventory : Assertion
	{
		public const bool DoNotFinaliseReceive = false;

		public TestDataForInventory(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public TestDataForInventory(BusinessObjectFactory factory, TestNotificationBuffer notify)
		{
			this.Factory = factory;
			this.Notify = notify ?? new TestNotificationBuffer();
			Helper = new WhsTestHelperFunctions(factory);
		}

		public void CreateSimpleInventory()
		{
			CreateSimpleInventory(true, ZDateTimeOffset.Now, false);
		}

		public void CreateSimpleInventory(ZString whsName)
		{
			CreateSimpleInventory(whsName, true, ZDateTimeOffset.Now);
		}

		public void CreateSimpleInventoryNoAllocate()
		{
			CreateSimpleInventory(false, false, ZDateTimeOffset.Now, false);
		}

		public void CreateSimpleInventory(ZDateTimeOffset arrivalDate)
		{
			CreateSimpleInventory(true, true, arrivalDate, false);
		}

		public void CreateSimpleInventory(bool finalise)
		{
			CreateSimpleInventory(finalise, ZDateTimeOffset.Now, false);
		}

		public void CreateSimpleInventory(ZString whsName, bool finalise, ZDateTimeOffset arrivalDate)
		{
			CreateSimpleInventory(whsName, true, finalise, arrivalDate, false);
		}

		public void CreateSimpleInventoryWithSaveFactoryForWarehouse()
		{
			CreateSimpleInventory(true, ZDateTimeOffset.Now, saveFactoryForWarehouse: true);
		}

		public void CreateSimpleInventory(bool finalise, ZDateTimeOffset arrivalDate, bool saveFactoryForWarehouse)
		{
			CreateSimpleInventory(true, finalise, arrivalDate, saveFactoryForWarehouse);
		}

		public void CreateSimpleInventory(bool allocate, bool finalise, ZDateTimeOffset arrivalDate, bool saveFactoryForWarehouse)
		{
			CreateSimpleInventory("1", allocate, finalise, arrivalDate, saveFactoryForWarehouse);
		}

		public void CreateSimpleInventory(ZString whsName, bool allocate, bool finalise, ZDateTimeOffset arrivalDate, bool saveFactoryForWarehouse)
		{
			Whs1 = Helper.CreateWarehouse(whsName, "A", 2, 1);
			Whs1.WarehouseAddress.OA_Address1 = "DifferentToData_Org1_MainAddress"; // to prevent buggering up matching via universal
			Org1 = Helper.CreateClient("111", "111");
			Product1 = WhsProduct.GetWhsProduct(Part1 = Helper.CreateProduct(Org1, "P1"));

			if (saveFactoryForWarehouse)
			{
				Factory.Save();
			}

			Receive11 = Helper.CreateWhsReceive(Org1, Whs1, "11", Notify);
			Line111 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, 100m);
			Receive11.WD_ArrivalDate = arrivalDate;

			if (allocate || finalise)
			{
				Receive11.AllocateLocationsWithMock();
			}

			if (finalise)
			{
				Receive11.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive11);
			}
		}

		public void CreateSimpleInventoryManyLines()
		{
			CreateSimpleInventoryManyLines(20m, 20m, 20m, 20m, 20m);
		}

		public void CreateSimpleInventoryManyLines(ZDecimal qty1, ZDecimal qty2, ZDecimal qty3, ZDecimal qty4, ZDecimal qty5)
		{
			CreateSimpleInventoryManyLines(qty1, qty2, qty3, qty4, qty5, true, true);
		}

		public void CreateSimpleInventoryManyLines(ZDecimal qty1, ZDecimal qty2, ZDecimal qty3, ZDecimal qty4, ZDecimal qty5, bool finalise)
		{
			CreateSimpleInventoryManyLines(qty1, qty2, qty3, qty4, qty5, true, finalise);
		}

		public void CreateSimpleInventoryManyLines(ZDecimal qty1, ZDecimal qty2, ZDecimal qty3, ZDecimal qty4, ZDecimal qty5, bool allocate, bool finalise)
		{
			Whs1 = Whs1 ?? Helper.CreateWarehouse("IL1", "A", 3, 2);
			Org1 = Helper.CreateClient("1", "1");
			Product1 = WhsProduct.GetWhsProduct(Part1 = Helper.CreateProduct(Org1, "P1"));

			Receive11 = Helper.CreateWhsReceive(Org1, Whs1, "11", Notify);
			if (qty1 > 0m)
			{
				Line111 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, qty1);
			}

			if (qty2 > 0m)
			{
				Line112 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, qty2);
			}

			if (qty3 > 0m)
			{
				Line113 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, qty3);
			}

			if (qty4 > 0m)
			{
				Line114 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, qty4);
			}

			if (qty5 > 0m)
			{
				Line115 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, qty5);
			}

			if (allocate || finalise)
			{
				Receive11.AllocateLocationsWithMock();
			}

			if (finalise)
			{
				Receive11.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive11);
			}
		}

		public void CreateSimpleInventoryManyLines(ZDecimal[] quantities)
		{
			CreateSimpleInventoryManyLines(quantities, "1", true);
		}

		public void CreateSimpleInventoryManyLines(ZDecimal[] quantities, bool finalise)
		{
			CreateSimpleInventoryManyLines(quantities, "1", finalise);
		}

		public void CreateSimpleInventoryManyLines(ZDecimal[] quantities, ZString reference, bool finalise)
		{
			var whs = Helper.CreateWarehouse("IL2", "A", 3, 2);
			var org = Helper.CreateClient("1");
			var part = Helper.CreateProduct(org, "P1");
			Product1 = WhsProduct.GetWhsProduct(part);
			Factory.Save();
			CreateSimpleInventoryManyLines(whs, org, part, quantities, reference, finalise);
		}

		public void CreateSimpleInventoryManyLines(WhsWarehouse whs, OrgHeader org, OrgSupplierPart part, ZDecimal[] quantities)
		{
			CreateSimpleInventoryManyLines(whs, org, part, quantities, "1", true);
		}

		public void CreateSimpleInventoryManyLines(WhsWarehouse whs, OrgHeader org, OrgSupplierPart part, ZDecimal[] quantities, bool finalise)
		{
			CreateSimpleInventoryManyLines(whs, org, part, quantities, "1", finalise);
		}

		public void CreateSimpleInventoryManyLines(WhsWarehouse whs, OrgHeader org, OrgSupplierPart part, ZDecimal[] quantities, ZString reference, bool finalise = true)
		{
			if (Whs1 == null)
			{
				Whs1 = whs;
			}

			if (Org1 == null)
			{
				Org1 = org;
			}

			if (Part1 == null)
			{
				Part1 = part;
			}

			Receive11 = Helper.CreateWhsReceive(org, whs, reference, Notify);

			foreach (ZDecimal quantity in quantities)
			{
				Helper.CreateWhsReceiveInventoryLine(Receive11, part, quantity);
			}
			Receive11.AllocateLocationsWithMock();

			if (finalise)
			{
				Receive11.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive11);
			}
		}

		public void MakeSimpleInventoryLinesMergebleForPicking()
		{
			MakeInventoryMergebleForPicking(Line112, Line111);
			MakeInventoryMergebleForPicking(Line113, Line111);
			MakeInventoryMergebleForPicking(Line114, Line111);
			MakeInventoryMergebleForPicking(Line115, Line111);
		}

		public void MakeInventoryMergebleForPicking(WhsInventoryView from, WhsInventoryView to)
		{
			if (from != null && to != null)
			{
				from.WI_WL = to.WI_WL;
				from.InDocketLine.WE_WL = from.WI_WL;
				from.WI_ArrivalDate = to.WI_ArrivalDate;
			}
		}

		public void SetExpectedQuantities(WhsReceive receive, ZDecimal[] quantities)
		{
			Helper.SetExpectedQuantities(receive, quantities);
		}

		public void CreateMultiWarehouseClientProductInventory(StockType typeOfStockToCreate = StockType.WithBondEntryKeys)
		{
			CreateMultiWarehouseClientProductInventory(true, typeOfStockToCreate);
		}

		public void CreateMultiWarehouseClientProductInventory(bool finalise, StockType typeOfStockToCreate = StockType.WithBondEntryKeys)
		{
			Whs1 = Helper.CreateWarehouse("CP1", "A", 3, 1);
			Whs2 = Helper.CreateWarehouse("CP2", "A");
			Org1 = Helper.CreateClient("1");
			Org2 = Helper.CreateClient("2");
			Product1 = WhsProduct.GetWhsProduct(Part1 = Helper.CreateProduct(Org1, "P1"));
			Product2 = WhsProduct.GetWhsProduct(Part2 = Helper.CreateProduct(Org2, "P2"));
			Helper.CreateProductClientRelationShip(Org2, Part1);
			Helper.CreateProductClientRelationShip(Org1, Part2);

			Org2.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Org2.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			Org2.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			Org2.MiscServ.OM_IMUseExpiryDate = true;
			Org2.MiscServ.OM_IMUsePackingDate = true;

			foreach (AttributeNumber attribNo in Enum.GetValues(typeof(AttributeNumber)))
			{
				Helper.SetProductAttributeUse(Org2, Part2, attribNo, true);
			}
			Factory.Save();

			Receive11 = Helper.CreateWhsReceive(Org1, Whs1, "11", Notify);
			Receive12 = Helper.CreateWhsReceive(Org1, Whs2, "12", Notify);
			Receive21 = Helper.CreateWhsReceive(Org2, Whs1, "21", Notify);
			Receive22 = Helper.CreateWhsReceive(Org2, Whs2, "22", Notify);
			ReceiveNonBonded = Helper.CreateWhsReceive(Org2, Whs1, "NonBonded", Notify);

			var locationA1Whs1 = Whs1.FindLocation("A-1");
			if (typeOfStockToCreate == StockType.WithBondEntryKeys)
			{
				var bondedAreaWhs1 = Helper.CreateArea(Whs1, "C", AreaTypes.Codes.Bonded);
				var bondedAreaWhs2 = Helper.CreateArea(Whs2, "C", AreaTypes.Codes.Bonded);
				locationA1Whs1.WLV_WA_PickingArea = bondedAreaWhs1.PK;
				locationA1Whs1.WLV_WA_PutawayArea = bondedAreaWhs1.PK;
				Whs2.FindLocation("A").WLV_WA_PickingArea = bondedAreaWhs2.PK;
				Whs2.FindLocation("A").WLV_WA_PutawayArea = bondedAreaWhs2.PK;
				Receive11.WD_DocketSubType = ReceiveType.Codes.Customs;
				Receive12.WD_DocketSubType = ReceiveType.Codes.Customs;
				Receive21.WD_DocketSubType = ReceiveType.Codes.Customs;
				Receive22.WD_DocketSubType = ReceiveType.Codes.Customs;
			}

			// changing these quantities will break tests
			Line111 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part1, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK11-1" : "");
			Line112 = Helper.CreateWhsReceiveInventoryLine(Receive11, Part2, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK11-2" : "");
			Line121 = Helper.CreateWhsReceiveInventoryLine(Receive12, Part1, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK12-1" : "");
			Line122 = Helper.CreateWhsReceiveInventoryLine(Receive12, Part2, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK12-2" : "");
			Line211 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part1, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-1" : "");
			Line212 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1", "PA2", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-2" : "");
			Line221 = Helper.CreateWhsReceiveInventoryLine(Receive22, Part1, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK22-1" : "");
			Line222 = Helper.CreateWhsReceiveInventoryLine(Receive22, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1", "PA2", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK22-2" : "");
			Line223 = Helper.CreateWhsReceiveInventoryLine(Receive22, Part1, 100m, typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK22-3" : "");

			// special for attribute tests
			Line213 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(2), ZDate.Today.AddMonths(-1), "PA1", "PA2", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-3" : "");
			Line214 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-2), "PA1", "PA2", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-3" : "");
			Line215 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA12", "PA2", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-3" : "");
			Line216 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1", "PA22", "PA3", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-3" : "");
			Line217 = Helper.CreateWhsReceiveInventoryLine(Receive21, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1", "PA2", "PA32", typeOfStockToCreate == StockType.WithBondEntryKeys ? "BEK21-3" : "");
			LineNonBonded = Helper.CreateWhsReceiveInventoryLine(ReceiveNonBonded, Part2, 100m, ZDate.Today.AddMonths(1), ZDate.Today.AddMonths(-1), "PA1Non", "PA2Non", "PANon", "");
			Factory.Save();

			Line111.WI_WL = locationA1Whs1.PK;
			Line112.WI_WL = locationA1Whs1.PK;
			Line211.WI_WL = locationA1Whs1.PK;
			Line212.WI_WL = locationA1Whs1.PK;
			Line213.WI_WL = locationA1Whs1.PK;
			Line214.WI_WL = locationA1Whs1.PK;
			Line215.WI_WL = locationA1Whs1.PK;
			Line216.WI_WL = locationA1Whs1.PK;
			Line217.WI_WL = locationA1Whs1.PK;

			Receive11.AllocateLocationsWithMock();
			Receive12.AllocateLocationsWithMock();
			Receive21.AllocateLocationsWithMock();
			Receive22.AllocateLocationsWithMock();
			ReceiveNonBonded.AllocateLocationsWithMock();

			if (finalise)
			{
				Receive11.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive11);

				Receive12.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive12);

				Receive21.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive21);

				Receive22.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Receive22);

				ReceiveNonBonded.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(ReceiveNonBonded);

				InventoryForLine215 = Line215;
			}

			Factory.Save();
		}

		public void CreateInventoryWithHeldCode()
		{
			CreateSimpleInventory(false, ZDateTimeOffset.Now, false);
			Helper.SetClientAttributeType(Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(Org1, Part1, AttributeNumber.One, true);
			Line111.WI_PalletID = "PLABC";
			Line111.Client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Line111.InDocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			Line111.WI_PartAttrib1 = "PA1";
			Line111.InDocketLine.WE_PartAttrib1 = "PA1";
			Line111.WI_CustomAttrib_2 = "CA2";
			Line111.InDocketLine.WE_CustomAttrib2 = "CA2";
			Line111.InDocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			Line111.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			Line111.InDocketLine.Docket.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(Line111.InDocketLine.Docket);

			Line111.InDocketLine.WE_PackageGroupId = "123";
			Line111.InDocketLine.WE_PerPackageQty = 2m;
			Factory.Save();

			Line111.CustomsData.WB_EntryKey = "ABC";
			Line111.CustomsData.WB_EntryLineNo = 10;
		}

		public enum StockType
		{
			WithBondEntryKeys,
			WithoutBondEntryKeys
		}

		public WhsWarehouse Whs1;
		public WhsWarehouse Whs2;
		public OrgHeader Org1;
		public OrgHeader Org2;
		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		public WhsProduct Product1;
		public WhsProduct Product2;
		public WhsInventoryView InventoryForLine215;
		public TestNotificationBuffer Notify;
		public WhsReceive Receive11;
		public WhsReceive Receive12;
		public WhsReceive Receive21;
		public WhsReceive Receive22;
		public WhsReceive ReceiveNonBonded;
		public WhsInventoryView Line111;
		public WhsInventoryView Line112;
		public WhsInventoryView Line113;
		public WhsInventoryView Line114;
		public WhsInventoryView Line115;
		public WhsInventoryView Line121;
		public WhsInventoryView Line122;
		public WhsInventoryView Line211;
		public WhsInventoryView Line212;
		public WhsInventoryView Line213;
		public WhsInventoryView Line214;
		public WhsInventoryView Line215;
		public WhsInventoryView Line216;
		public WhsInventoryView Line217;
		public WhsInventoryView Line221;
		public WhsInventoryView Line222;
		public WhsInventoryView Line223;
		public WhsInventoryView LineNonBonded;

		protected WhsTestHelperFunctions Helper;
		protected BusinessObjectFactory Factory;
	}

	#endregion

	#region Test Classes for Bonded Warehousing

	#region TestDataForBondedEntries

	public class TestDataForBondedEntries
	{
		public TestDataForBondedEntries(BusinessObjectFactory factory, bool saveFactory_doNotUseForNewTests = true)
			: this(factory, true, saveFactory_doNotUseForNewTests)
		{
		}

		public TestDataForBondedEntries(BusinessObjectFactory factory, bool processBondedInwardDuringConstruction, bool saveFactory_doNotUseForNewTests = true)
		{
			this.Factory = factory;
			SetupData(saveFactory_doNotUseForNewTests);
			if (processBondedInwardDuringConstruction)
			{
				var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();
				using (ObjectFactory.Substitute(putawayEngineMock.Object))
				{
					ProcessBondedMovement();
				}
			}
		}

		void SetupData(bool saveFactory)
		{
			Helper = new WhsTestHelperFunctions(Factory);
			Org = Helper.CreateClient();
			Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;

			Address = Org.Addresses.AddNew();
			Address.OA_Address1 = "1 BOND ST";

			Whs = Helper.CreateWarehouse("SD1", "A", 10, 2);
			Whs.WW_OA_WarehouseAddress = Address.PK;
			Helper.EnableWarehouseForBond(Whs, true);
			Helper.EnableWarehouseForFreeStore(Whs, false);

			Array.ForEach(Whs.Rows.Cast<WhsRow>().SelectMany(r => r.Locations).ToArray(), l => l.WLV_WA_PickingArea = Whs.Areas.Single(a => a.WA_AreaType == "BON").PK);
			Array.ForEach(Whs.Rows.Cast<WhsRow>().SelectMany(r => r.Locations).ToArray(), l => l.WLV_WA_PutawayArea = Whs.Areas.Single(a => a.WA_AreaType == "BON").PK);

			Part1 = Helper.CreateProduct(Org, "P1");
			Part2 = Helper.CreateProduct(Org, "P2");

			if (saveFactory)
			{
				Factory.Save();
			}

			SetupPreData();

			IReceive = new WhsBondedWarehouseTransaction();
			IReceive.Date = ZDateTime.Now;
			IReceive.Client = Org;
			IReceive.Warehouse = Address;
			IReceive.AdditionalReferences = Array.Empty<AdditionalReference>();// { Ref };

			IReceiveLine1 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine1.EntryKey = "E11AA1";
			IReceiveLine1.EntryLineNumber = 1;
			IReceiveLine1.EntryDate = ZDateTime.Today;
			IReceiveLine1.Product = Part1;
			IReceiveLine1.Quantity = 50;
			IReceiveLine1.QuantityUnit = "UNT";
			IReceiveLine1.CustomsQuantity = 100m;
			IReceiveLine1.CustomsQuantityUnit = "KG";
			IReceiveLine1.CustomsSecondQuantity = 10m;
			IReceiveLine1.CustomsSecondQuantityUnit = "M3";
			IReceiveLine1.CustomsThirdQuantity = 1m;
			IReceiveLine1.CustomsThirdQuantityUnit = "CU";
			IReceiveLine1.ValueForDuty = 200m;

			IReceiveLine1.TILV = new Money(51m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine1.BondedWarehouseQuantity = 50;
			IReceiveLine1.BondedWarehouseQuantityUnit = "UNT";
			IReceiveLine1.AddInfo = "ADD INFO1";
			IReceiveLine1.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
			IReceiveLine1.Warehouse = Address;

			IReceiveLine2 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine2.EntryKey = "E11AA1";
			IReceiveLine2.EntryLineNumber = 2;
			IReceiveLine2.EntryDate = ZDateTime.Today;
			IReceiveLine2.Product = Part1;
			IReceiveLine2.Quantity = 5;
			IReceiveLine2.QuantityUnit = "UNT";
			IReceiveLine2.CustomsQuantity = 10m;
			IReceiveLine2.CustomsQuantityUnit = "KG";
			IReceiveLine2.CustomsSecondQuantity = 5m;
			IReceiveLine2.CustomsSecondQuantityUnit = "M3";
			IReceiveLine2.CustomsThirdQuantity = 2.5m;
			IReceiveLine2.CustomsThirdQuantityUnit = "CU";
			IReceiveLine2.ValueForDuty = 20;
			IReceiveLine2.TILV = new Money(5.1m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine2.BondedWarehouseQuantity = 5;
			IReceiveLine2.BondedWarehouseQuantityUnit = "UNT";
			IReceiveLine2.AddInfo = "ADD INFO2";
			IReceiveLine2.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "NZ");
			IReceiveLine2.Warehouse = Address;

			IReceiveLine3 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine3.EntryKey = "E11AA1";
			IReceiveLine3.EntryLineNumber = 3;
			IReceiveLine3.EntryDate = ZDateTime.Today;
			IReceiveLine3.Product = Part2;
			IReceiveLine3.Quantity = 15;
			IReceiveLine3.QuantityUnit = "UNT";
			IReceiveLine3.CustomsQuantity = 20m;
			IReceiveLine3.CustomsQuantityUnit = "KG";
			IReceiveLine3.CustomsSecondQuantity = 10m;
			IReceiveLine3.CustomsSecondQuantityUnit = "M3";
			IReceiveLine3.CustomsThirdQuantity = 5m;
			IReceiveLine3.CustomsThirdQuantityUnit = "CU";
			IReceiveLine3.ValueForDuty = 30;
			IReceiveLine3.TILV = new Money(10m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine3.BondedWarehouseQuantity = 15;
			IReceiveLine3.BondedWarehouseQuantityUnit = "UNT";
			IReceiveLine3.AddInfo = "ADD INFO3";
			IReceiveLine3.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "US");
			IReceiveLine3.Warehouse = Address;

			IReceive.Lines.Add(IReceiveLine1);
			IReceive.Lines.Add(IReceiveLine2);
			IReceive.Lines.Add(IReceiveLine3);

			SetupPostData();

			if (saveFactory)
			{
				Factory.Save();
			}
		}

		public void ProcessBondedMovement()
		{
			Creator.Process();

			var receiveList = new WhsReceiveCollection(Factory);
			Receive = receiveList[0];
		}

		protected virtual void SetupPreData()
		{
		}

		protected virtual void SetupPostData()
		{
		}

		public virtual int DummyBondedWarehouseTransactionLineCount
		{
			get { return 3; }
		}

		public virtual WhsInventoryViewCollection FindInventory(OrgSupplierPart part, string location)
		{
			var factory = new BusinessObjectFactory();
			var inventory = new WhsInventoryViewCollection(factory);
			foreach (WhsInventoryView inv in Receive.Inventory)
			{
				if (inv.SupplierPart.PK == part.PK && (string.IsNullOrEmpty(location) || location == inv.LocationString))
				{
					inventory.Add(inv);
				}
			}
			return inventory;
		}

		public virtual WhsInventoryViewCollection FindInventory(string bondedEntryKey)
		{
			var query = new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, bondedEntryKey);
			var inventory = new WhsInventoryViewCollection(Factory, query);
			inventory.Load();
			return inventory;
		}

		public OrgHeader Org;
		public OrgAddress Address;
		public WhsWarehouse Whs;
		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		public WhsOrder Order1;
		public WhsReceive Receive;
		public WhsTestHelperFunctions Helper;

		WhsBondedTransactionProcessor creator;
		public WhsBondedTransactionProcessor Creator
		{
			get
			{
				return creator ?? (creator = new WhsBondedTransactionProcessor(Factory, IReceive));
			}
		}

		public WhsBondedWarehouseTransaction IReceive;
		public WhsBondedWarehouseTransactionLine IReceiveLine1;
		public WhsBondedWarehouseTransactionLine IReceiveLine2;
		public WhsBondedWarehouseTransactionLine IReceiveLine3;
		protected BusinessObjectFactory Factory;
	}

	#endregion

	#region TestDataForBondedEntriesWithBondIDs

	public class TestDataForBondedEntriesWithBondIDs : TestDataForBondedEntries
	{
		public TestDataForBondedEntriesWithBondIDs(BusinessObjectFactory factory, bool saveFactory_doNotUseForNewTests = true)
			: base(factory, saveFactory_doNotUseForNewTests: saveFactory_doNotUseForNewTests)
		{
		}

		public TestDataForBondedEntriesWithBondIDs(BusinessObjectFactory factory, bool processBondedInwardDuringConstruction, bool saveFactory_doNotUseForNewTests = true)
			: base(factory, processBondedInwardDuringConstruction: processBondedInwardDuringConstruction, saveFactory_doNotUseForNewTests: saveFactory_doNotUseForNewTests)
		{
		}

		protected override void SetupPostData()
		{
			Part3 = Helper.CreateProduct(Org, "P3");
			Helper.SetProductAttributeUse(Org, Part3, AttributeNumber.One, true);

			Part4 = Helper.CreateProduct(Org, "P4");
			Helper.SetProductAttributeUse(Org, Part4, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(Org, Part4, AttributeNumber.Two, true);

			IReceiveLine4 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine4.EntryKey = "E11AA1";
			IReceiveLine4.EntryLineNumber = 4;
			IReceiveLine4.EntryDate = ZDateTime.Today;
			IReceiveLine4.Product = Part3;
			IReceiveLine4.Quantity = 100;
			IReceiveLine4.QuantityUnit = "UNT";
			IReceiveLine4.CustomsQuantity = 150;
			IReceiveLine4.CustomsQuantityUnit = "KG";
			IReceiveLine4.CustomsSecondQuantity = 15;
			IReceiveLine4.CustomsSecondQuantityUnit = "M3";
			IReceiveLine4.CustomsThirdQuantity = 10;
			IReceiveLine4.CustomsThirdQuantityUnit = "CU";
			IReceiveLine4.ValueForDuty = 175;
			IReceiveLine4.TILV = new Money(25m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine4.BondedWarehouseQuantity = 66;
			IReceiveLine4.BondedWarehouseQuantityUnit = "KG";
			IReceiveLine4.AddInfo = "ADD INFO4";
			IReceiveLine4.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "US");
			IReceiveLine4.PartAttrib1 = "VIN41";
			IReceiveLine4.Warehouse = Address;

			IReceiveLine5 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine5.EntryKey = "E11AA1";
			IReceiveLine5.EntryLineNumber = 5;
			IReceiveLine5.EntryDate = ZDateTime.Today;
			IReceiveLine5.Product = Part4;
			IReceiveLine5.Quantity = 150;
			IReceiveLine5.QuantityUnit = "UNT";
			IReceiveLine5.CustomsQuantity = 155;
			IReceiveLine5.CustomsQuantityUnit = "KG";
			IReceiveLine5.CustomsSecondQuantity = 25;
			IReceiveLine5.CustomsSecondQuantityUnit = "M3";
			IReceiveLine5.CustomsThirdQuantity = 30;
			IReceiveLine5.CustomsThirdQuantityUnit = "CU";
			IReceiveLine5.ValueForDuty = 175;
			IReceiveLine5.TILV = new Money(25m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine5.BondedWarehouseQuantity = 66;
			IReceiveLine5.BondedWarehouseQuantityUnit = "KG";
			IReceiveLine5.AddInfo = "ADD INFO5";
			IReceiveLine5.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "US");
			IReceiveLine5.PartAttrib1 = "VIN51";
			IReceiveLine5.PartAttrib2 = "ENGINE51";
			IReceiveLine5.Warehouse = Address;

			IReceiveLine6 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine6.EntryKey = "E11AA1";
			IReceiveLine6.EntryLineNumber = 6;
			IReceiveLine6.EntryDate = ZDateTime.Today;
			IReceiveLine6.Product = Part4;
			IReceiveLine6.Quantity = 150;
			IReceiveLine6.QuantityUnit = "UNT";
			IReceiveLine6.CustomsQuantity = 155;
			IReceiveLine6.CustomsQuantityUnit = "KG";
			IReceiveLine6.CustomsSecondQuantity = 35;
			IReceiveLine6.CustomsSecondQuantityUnit = "M3";
			IReceiveLine6.CustomsThirdQuantity = 40;
			IReceiveLine6.CustomsThirdQuantityUnit = "CU";
			IReceiveLine6.ValueForDuty = 175;
			IReceiveLine6.TILV = new Money(25m, GlbCompany.CurrentCompany.LocalCurrency);
			IReceiveLine6.BondedWarehouseQuantity = 66;
			IReceiveLine6.BondedWarehouseQuantityUnit = "KG";
			IReceiveLine6.AddInfo = "ADD INFO5";
			IReceiveLine6.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "US");
			IReceiveLine6.PartAttrib1 = "VIN61";
			IReceiveLine6.PartAttrib2 = "ENGINE61";
			IReceiveLine6.Warehouse = Address;

			IReceive.Lines.Add(IReceiveLine4);
			IReceive.Lines.Add(IReceiveLine5);
			IReceive.Lines.Add(IReceiveLine6);
		}

		public override int DummyBondedWarehouseTransactionLineCount
		{
			get { return 6; }
		}

		public OrgSupplierPart Part3;
		public OrgSupplierPart Part4;
		public WhsBondedWarehouseTransactionLine IReceiveLine4;
		public WhsBondedWarehouseTransactionLine IReceiveLine5;
		public WhsBondedWarehouseTransactionLine IReceiveLine6;
	}

	#endregion

	#region TestDataForBondedEntriesWithVOC

	public class TestDataForBondedEntriesWithVOC : TestDataForBondedEntries
	{
		public TestDataForBondedEntriesWithVOC(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TestDataForBondedEntriesWithVOC(BusinessObjectFactory factory, bool processBondedInwardDuringConstruction)
			: base(factory, processBondedInwardDuringConstruction: processBondedInwardDuringConstruction)
		{
		}

		protected override void SetupPreData()
		{
			base.SetupPreData();

			IReceive = new WhsBondedWarehouseTransaction();
			IReceive.Date = ZDateTime.Now;
			IReceive.Client = Org;
			IReceive.Warehouse = Address;
			IReceive.AdditionalReferences = Array.Empty<AdditionalReference>();

			IReceiveLine1 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine1.EntryKey = "E11AA1";
			IReceiveLine1.EntryLineNumber = 1;
			IReceiveLine1.EntryDate = ZDateTime.Today;
			IReceiveLine1.Product = Part1;
			IReceiveLine1.Quantity = 300;
			IReceiveLine1.QuantityUnit = "UNT";
			IReceiveLine1.CustomsQuantity = 300m;
			IReceiveLine1.CustomsQuantityUnit = "KG";
			IReceiveLine1.CustomsSecondQuantity = 200m;
			IReceiveLine1.CustomsSecondQuantityUnit = "M3";
			IReceiveLine1.CustomsThirdQuantity = 100m;
			IReceiveLine1.CustomsThirdQuantityUnit = "CU";
			IReceiveLine1.ValueForDuty = 300m;
			IReceiveLine1.TILV = new Money(51m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"));
			IReceiveLine1.BondedWarehouseQuantity = 300;
			IReceiveLine1.BondedWarehouseQuantityUnit = "UNT";
			IReceiveLine1.AddInfo = "ADD INFO1";
			IReceiveLine1.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");

			IReceiveLine2 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine2.EntryKey = "E11AA1";
			IReceiveLine2.EntryLineNumber = 2;
			IReceiveLine2.EntryDate = ZDateTime.Today;
			IReceiveLine2.Product = Part1;
			IReceiveLine2.Quantity = 500;
			IReceiveLine2.QuantityUnit = "UNT";
			IReceiveLine2.CustomsQuantity = 500m;
			IReceiveLine2.CustomsQuantityUnit = "KG";
			IReceiveLine2.CustomsSecondQuantity = 250m;
			IReceiveLine2.CustomsSecondQuantityUnit = "M3";
			IReceiveLine2.CustomsThirdQuantity = 125m;
			IReceiveLine2.CustomsThirdQuantityUnit = "CU";
			IReceiveLine2.ValueForDuty = 500;
			IReceiveLine2.TILV = new Money(5.1m, Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD"));
			IReceiveLine2.BondedWarehouseQuantity = 500;
			IReceiveLine2.BondedWarehouseQuantityUnit = "UNT";
			IReceiveLine2.AddInfo = "ADD INFO2";
			IReceiveLine2.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "NZ");

			IReceive.Lines.Add(IReceiveLine1);
			IReceive.Lines.Add(IReceiveLine2);
		}
	}

	#endregion

	#region TestDataForBondedEntriesMultiWarehouse

	public class TestDataForBondedEntriesMultiWarehouse
	{
		public TestDataForBondedEntriesMultiWarehouse(BusinessObjectFactory factory, bool saveFactory_doNotUseForNewTests = true)
			: this(factory, true, saveFactory_doNotUseForNewTests)
		{
		}

		public TestDataForBondedEntriesMultiWarehouse(BusinessObjectFactory factory, bool processBondedInwardDuringConstruction, bool saveFactory_doNotUseForNewTests = true)
		{
			this.Factory = factory;

			SetupData();
			if (saveFactory_doNotUseForNewTests)
			{
				factory.Save();
			}

			if (processBondedInwardDuringConstruction)
			{
				ProcessBondedMovement();
			}
		}

		protected virtual void BondedTransactionsAlreadyInDatabase() { }

		void SetupData()
		{
			#region Environment

			Helper = new WhsTestHelperFunctions(Factory);
			Org = Helper.CreateClient();
			Org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			Org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;

			Address1 = Factory.NewWithValidTestData<OrgAddress>();
			Address1.OA_Address1 = "1 BOND ST";
			Address1.OA_OH = Org.PK;

			Address2 = Factory.NewWithValidTestData<OrgAddress>();
			Address2.OA_Address1 = "2 BOND ST";
			Address2.OA_OH = Org.PK;

			Address3 = Factory.NewWithValidTestData<OrgAddress>();
			Address3.OA_Address1 = "3 BOND ST";
			Address3.OA_OH = Org.PK;

			Whs1 = Helper.CreateWarehouse("1", "A");
			Whs2 = Helper.CreateWarehouse("2", "A");
			Whs3 = Helper.CreateWarehouse("3", "A");

			Whs1.WW_OA_WarehouseAddress = Address1.PK;
			Whs2.WW_OA_WarehouseAddress = Address2.PK;
			Whs3.WW_OA_WarehouseAddress = Address3.PK;
			Helper.EnableWarehouseForBond(Whs1, true);
			Helper.EnableWarehouseForBond(Whs2, true);
			Helper.EnableWarehouseForBond(Whs3, true);

			Part1 = Helper.CreateProduct(Org, "P1");
			Part2 = Helper.CreateProduct(Org, "P2");
			Part3 = Helper.CreateProduct(Org, "P3");

			#endregion

			BondedTransactionsAlreadyInDatabase();

			IReceive1 = new WhsBondedWarehouseTransaction();
			IReceive1.Date = ZDateTime.Now;
			IReceive1.Client = Org;
			IReceive1.Warehouse = Address1;
			IReceive1.AdditionalReferences = Array.Empty<AdditionalReference>();

			#region Warehouse 1

			IReceiveLine11 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine11.EntryKey = "E11AA1";
			IReceiveLine11.EntryLineNumber = 1;
			IReceiveLine11.Product = Part1;
			IReceiveLine11.Quantity = 10;
			IReceiveLine11.QuantityUnit = "UNT";
			IReceiveLine11.Warehouse = Address1;

			IReceiveLine12 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine12.EntryKey = "E11AA1";
			IReceiveLine12.EntryLineNumber = 2;
			IReceiveLine12.Product = Part2;
			IReceiveLine12.Quantity = 15;
			IReceiveLine12.QuantityUnit = "UNT";
			IReceiveLine12.Warehouse = Address1;

			#endregion

			#region Warehouse 2

			IReceiveLine21 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine21.EntryKey = "E12AA1";
			IReceiveLine21.EntryLineNumber = 1;
			IReceiveLine21.Product = Part1;
			IReceiveLine21.Quantity = 20;
			IReceiveLine21.QuantityUnit = "UNT";
			IReceiveLine21.Warehouse = Address2;

			IReceiveLine22 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine22.EntryKey = "E12AA1";
			IReceiveLine22.EntryLineNumber = 2;
			IReceiveLine22.Product = Part2;
			IReceiveLine22.Quantity = 25;
			IReceiveLine22.QuantityUnit = "UNT";
			IReceiveLine22.Warehouse = Address2;

			#endregion

			#region Warehouse 3

			IReceiveLine31 = new WhsBondedWarehouseTransactionLine();
			IReceiveLine31.EntryKey = "E13AA1";
			IReceiveLine31.EntryLineNumber = 1;
			IReceiveLine31.Product = Part3;
			IReceiveLine31.Quantity = 30;
			IReceiveLine31.QuantityUnit = "UNT";
			IReceiveLine31.Warehouse = Address3;

			#endregion

			IReceive1.Lines.Add(IReceiveLine11);
			IReceive1.Lines.Add(IReceiveLine12);
			IReceive1.Lines.Add(IReceiveLine21);
			IReceive1.Lines.Add(IReceiveLine22);
			IReceive1.Lines.Add(IReceiveLine31);

			SetupMoreData();
		}

		public void ProcessBondedMovement()
		{
			new WhsBondedTransactionProcessor(Factory, IReceive1).Process();
			new WhsBondedTransactionProcessor(Factory, IReceive2).Process();
			new WhsBondedTransactionProcessor(Factory, IReceive3).Process();
		}

		protected virtual void SetupMoreData()
		{
		}

		public OrgHeader Org;
		public OrgAddress Address1;
		public OrgAddress Address2;
		public OrgAddress Address3;
		public WhsWarehouse Whs1;
		public WhsWarehouse Whs2;
		public WhsWarehouse Whs3;
		public OrgSupplierPart Part1;
		public OrgSupplierPart Part2;
		public OrgSupplierPart Part3;
		public WhsTestHelperFunctions Helper;
		public WhsBondedWarehouseTransaction IReceive1;
		public WhsBondedWarehouseTransaction IReceive2;
		public WhsBondedWarehouseTransaction IReceive3;
		public WhsBondedWarehouseTransactionLine IReceiveLine11;
		public WhsBondedWarehouseTransactionLine IReceiveLine12;
		public WhsBondedWarehouseTransactionLine IReceiveLine21;
		public WhsBondedWarehouseTransactionLine IReceiveLine22;
		public WhsBondedWarehouseTransactionLine IReceiveLine31;
		protected BusinessObjectFactory Factory;
	}

	#endregion

	#endregion

	#region TestClasses for IFS

	public class TestDataForIFS
	{
		public TestDataForIFS(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region CreateIfsEnvironment

		public void CreateIfsEnvironment()
		{
			// create an Org to use as the IFS Org Proxy and update the registry to point to the new org
			OrgForIFS = factory.NewWithValidTestData<OrgHeader>();
			WarehouseDataRegistry.Instance.IFSOrgProxy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgForIFS.PK.ToGuid());

			// create a client and the code mapping for the client on the IFS Org
			Client = Helper.CreateClient("CLIENT");
			Client.MainAddress.FillWithValidTestData();
			var clientEdiCode = OrgForIFS.CreatePatternMatchOverrideForTest();
			clientEdiCode.OO_ForeignCode = "CLIENT IN IFS";
			clientEdiCode.OO_LocalGuid = Client.PK;

			// create a transportCo and the code mapping for the transportCo on the IFS Org
			TransportCo = factory.NewWithValidTestData<OrgHeader>();
			TransportCo.OH_Code = "STARTRACK";
			TransportCo.OH_IsLocalTransport = true;
			var transportCoEdiCode = OrgForIFS.CreatePatternMatchOverrideForTest();
			transportCoEdiCode.OO_ForeignCode = "STARTRACK IN IFS";
			transportCoEdiCode.OO_LocalGuid = TransportCo.PK;

			// create a service for the transportCo created above
			Service = TransportCo.MiscServ.CarrierServiceLevels.AddNew();
			Service.PL_Code = "RD";
			Service.PL_CarrierServiceLevelDescription = "ROAD";

			Warehouse = Helper.CreateWarehouse("Sydney");
			Warehouse.WarehouseAddress.FillWithValidTestData();
			Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";
		}

		#endregion

		#region CreateIfsOrders

		public void CreateIfsOrders()
		{
			if (OrgForIFS == null)
			{
				CreateIfsEnvironment();
			}

			Order1 = Helper.CreateWhsOrder(Client, Warehouse, "ORDER1");
			Order1.WD_ExternalReference = "W00000447";
			Order1.TransportBillToDocAddress.OrganisationPK = Warehouse.WarehouseAddress.OA_OH;

			Order2 = Helper.CreateWhsOrder(Client, Warehouse, "ORDER2");
			Order2.WD_ExternalReference = "W00000448";
			Order2.TransportBillToDocAddress.OrganisationPK = Warehouse.WarehouseAddress.OA_OH;

			Order3 = Helper.CreateWhsOrder(Client, Warehouse, "ORDER3");
			Order3.WD_ExternalReference = "W00000449";
			Order3.TransportBillToDocAddress.OrganisationPK = Warehouse.WarehouseAddress.OA_OH;

			Order4 = Helper.CreateWhsOrder(Client, Warehouse, "ORDER4");
			Order4.WD_ExternalReference = "W00000450";
			Order4.TransportBillToDocAddress.OrganisationPK = Warehouse.WarehouseAddress.OA_OH;

			Order5 = Helper.CreateWhsOrder(Client, Warehouse, "ORDER5");
			Order5.WD_ExternalReference = "W00000451";
			Order5.TransportBillToDocAddress.OrganisationPK = Warehouse.WarehouseAddress.OA_OH;
		}

		#endregion

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		public OrgHeader OrgForIFS { get; private set; }
		public OrgHeader Client { get; private set; }
		public OrgHeader TransportCo { get; private set; }
		public WhsWarehouse Warehouse { get; private set; }
		public OrgCarrierServiceLevel Service { get; private set; }

		public WhsOrder Order1 { get; private set; }
		public WhsOrder Order2 { get; private set; }
		public WhsOrder Order3 { get; private set; }
		public WhsOrder Order4 { get; private set; }
		public WhsOrder Order5 { get; private set; }

		readonly BusinessObjectFactory factory;
	}

	#endregion

	#region TestClasses for BOM

	public class TestDataForBOM : TestDataForInventory
	{
		public TestDataForBOM(BusinessObjectFactory factory)
			: this(factory, null)
		{
		}

		public TestDataForBOM(BusinessObjectFactory factory, TestNotificationBuffer notify)
			: base(factory, notify)
		{
		}

		#region CreateOrderWithBOMShortfall

		public const decimal DefaultShortfallQty = 10m;

		public WhsOrder CreateOrderWithBOMShortfall()
		{
			return CreateOrderWithBOMShortfall(DefaultShortfallQty);
		}

		public WhsOrder CreateOrderWithBOMShortfall(ZDecimal shortfallQty)
		{
			CreateMultiWarehouseClientProductInventory(StockType.WithoutBondEntryKeys); // create 100 units of each product in inventory

			var data1And2Bom = Part1.BillOfMaterials.AddNew();
			data1And2Bom.OE_OP_Component = Part2.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(Org1, Whs1, Part1, 100m + shortfallQty);
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new WhsOrder[] { order });

			return order;
		}

		#endregion

		#region Create BOM Products / Inventory

		public void CreateBOMProducts(bool saveFactoryForWarehouse = false)
		{
			BOM.CreateBOMProducts(saveFactoryForWarehouse);
		}

		/// <summary>
		/// Receive 100 Bikes into inventory.
		/// </summary>
		public void CreateBOMProductsInInventory(bool saveFactoryForWarehouse = false)
		{
			BOM.CreateBOMProductsInInventory(saveFactoryForWarehouse);
		}

		/// <summary>
		/// Receive 10 Engines and 20 Wheels into inventory, enough to build 10 Bikes.
		/// </summary>
		public void CreateBOMComponentsInInventory(bool saveFactoryForWarehouse = false)
		{
			BOM.CreateBOMComponentsInInventory(saveFactoryForWarehouse);
		}

		/// <summary>
		/// Receive enough sub components to build 10 bikes from their sub parts.
		/// </summary>
		public void CreateBOMSubComponentsInInventory()
		{
			BOM.CreateBOMSubComponentsInInventory();
		}

		public void CreateProductInInventory(ZString receiveRef, OrgSupplierPart product, ZDecimal amountToReceiveIntoInventory)
		{
			BOM.CreateProductInInventory(receiveRef, product, amountToReceiveIntoInventory);
		}

		#endregion

		#region BOM Products

		public class BOMHelper
		{
			public BOMHelper(TestDataForBOM data)
			{
				Data = data;
			}

			public readonly TestDataForBOM Data;

			#region CreateBOMProducts

			public void CreateBOMProducts(bool saveFactoryForWarehouse = false)
			{
				if (Data.Org1 == null)
				{
					if (saveFactoryForWarehouse)
					{
						Data.CreateSimpleInventoryWithSaveFactoryForWarehouse();
					}
					else
					{
						Data.CreateSimpleInventory();
					}
				}

				// BOM products
				BikeProduct = WhsProduct.GetWhsProduct(Bike = Data.Helper.CreateProduct(Data.Org1, "Motorbike"));
				BikeEngineProduct = WhsProduct.GetWhsProduct(BikeEngine = Data.Helper.CreateProduct(Data.Org1, "Engine"));
				BikeWheelProduct = WhsProduct.GetWhsProduct(BikeWheel = Data.Helper.CreateProduct(Data.Org1, "Wheel"));

				// Common BOM components
				PolishProduct = WhsProduct.GetWhsProduct(Polish = Data.Helper.CreateProduct(Data.Org1, "Polish"));

				// Wheel BOM components
				WheelTyreProduct = WhsProduct.GetWhsProduct(WheelTyre = Data.Helper.CreateProduct(Data.Org1, "Tyre"));
				WheelRimProduct = WhsProduct.GetWhsProduct(WheelRim = Data.Helper.CreateProduct(Data.Org1, "Rim"));

				// Engine BOM components
				EngineBlockProduct = WhsProduct.GetWhsProduct(EngineBlock = Data.Helper.CreateProduct(Data.Org1, "Block"));
				EnginePistonProduct = WhsProduct.GetWhsProduct(EnginePiston = Data.Helper.CreateProduct(Data.Org1, "Piston"));
				EngineOilProduct = WhsProduct.GetWhsProduct(EngineOil = Data.Helper.CreateProduct(Data.Org1, "Oil"));

				// Piston BOM components
				PistonHeadProduct = WhsProduct.GetWhsProduct(PistonHead = Data.Helper.CreateProduct(Data.Org1, "Piston Head"));
				PistonCrankProduct = WhsProduct.GetWhsProduct(PistonCrank = Data.Helper.CreateProduct(Data.Org1, "Piston Crank"));
				PistonRingProduct = WhsProduct.GetWhsProduct(PistonRing = Data.Helper.CreateProduct(Data.Org1, "Piston Ring"));

				// BOM join (bike has 1 engine and 2 wheels + 1 polish)
				BikeEngineBOMPart = Data.Helper.CreateProductBOM(Bike, BikeEngine, 1, "UNT");
				BikeWheelBOMPart = Data.Helper.CreateProductBOM(Bike, BikeWheel, 2, "UNT");
				BikePolishBOMPart = Data.Helper.CreateProductBOM(Bike, Polish, 1, "UNT");
				BikePolishBOMPart.OE_CanReuse = false; // polish cannot be reused.

				// BOM join (bike has 2 wheels, each wheel has 1 tyre + 1 rim + 1 polish)
				WheelTyreBOMPart = Data.Helper.CreateProductBOM(BikeWheel, WheelTyre, 1, "UNT");
				WheelRimBOMPart = Data.Helper.CreateProductBOM(BikeWheel, WheelRim, 1, "UNT");
				WheelPolishBOMPart = Data.Helper.CreateProductBOM(BikeWheel, Polish, 1, "UNT");
				WheelPolishBOMPart.OE_CanReuse = false; // polish cannot be reused.

				// BOM join (engine has 1 block + 4 pistons + 1 oil + 1 polish)
				EngineBlockBOMPart = Data.Helper.CreateProductBOM(BikeEngine, EngineBlock, 1, "UNT");
				EnginePistonBOMPart = Data.Helper.CreateProductBOM(BikeEngine, EnginePiston, 4, "UNT");
				EngineOilBOMPart = Data.Helper.CreateProductBOM(BikeEngine, EngineOil, 1, "UNT");
				EngineOilBOMPart.OE_CanReuse = false; // oil cannot be reused.
				EnginePolishBOMPart = Data.Helper.CreateProductBOM(BikeEngine, Polish, 1, "UNT");
				EnginePolishBOMPart.OE_CanReuse = false; // polish cannot be reused.

				// BOM join (piston has 1 head + 1 crank + 1 ring)
				PistonHeadBOMPart = Data.Helper.CreateProductBOM(EnginePiston, PistonHead, 1, "UNT");
				PistonCrankBOMPart = Data.Helper.CreateProductBOM(EnginePiston, PistonCrank, 1, "UNT");
				PistonRingBOMPart = Data.Helper.CreateProductBOM(EnginePiston, PistonRing, 1, "UNT");

				// create stock queries
				InventoryQueries.CreateInventoryQueries();
			}

			public WhsProduct BikeProduct;
			public WhsProduct BikeEngineProduct;
			public WhsProduct BikeWheelProduct;
			public WhsProduct PolishProduct;
			public WhsProduct WheelTyreProduct;
			public WhsProduct WheelRimProduct;
			public WhsProduct EngineBlockProduct;
			public WhsProduct EnginePistonProduct;
			public WhsProduct EngineOilProduct;
			public WhsProduct PistonHeadProduct;
			public WhsProduct PistonCrankProduct;
			public WhsProduct PistonRingProduct;

			#endregion

			#region CreateBOMProductsInInventory

			public void CreateBOMProductsInInventory(bool saveFactoryForWarehouse = false)
			{
				if (Bike == null)
				{
					CreateBOMProducts(saveFactoryForWarehouse);
				}

				CreateProductInInventory("100 Bikes for BOM", Bike, 100m);
			}

			public void CreateBOMComponentsInInventory(bool saveFactoryForWarehouse = false)
			{
				if (Bike == null)
				{
					CreateBOMProducts(saveFactoryForWarehouse);
				}

				CreateProductInInventory("10 engines for BOM", BikeEngine, 10m);
				CreateProductInInventory("20 wheels for BOM", BikeWheel, 20m);
				CreateProductInInventory("10 units of polish for BOM", Polish, 10m);
			}

			public void CreateBOMSubComponentsInInventory()
			{
				if (Bike == null)
				{
					CreateBOMProducts();
				}
				// Enough for 10 bikes to be build from the subparts up

				// BOM join (bike has 2 wheels, each wheel has 1 tyre + 1 rim + 1 polish)
				CreateProductInInventory("Tyres for 20 wheels", WheelTyre, 20m * 1);
				CreateProductInInventory("Rims for 20 wheels", WheelRim, 20m * 1);
				CreateProductInInventory("Polish for 20 wheels", Polish, 20m * 1);

				// BOM join (engine has 1 block + 4 pistons + 1 oil + 1 polish)
				CreateProductInInventory("Blocks for 10 engines", EngineBlock, 10m * 1);
				CreateProductInInventory("Oil for 10 engines", EngineOil, 10m * 1);
				CreateProductInInventory("Pistons for 10 engines", EnginePiston, 10m * 4);
				CreateProductInInventory("Polish for 10 engines", Polish, 10m * 1);

				// BOM join (piston has 1 head + 1 crank + 1 ring)
				CreateProductInInventory("Piston heads for 40 pistons", PistonHead, 40m * 1);
				CreateProductInInventory("Piston cranks for 40 pistons", PistonCrank, 40m * 1);
				CreateProductInInventory("Piston rings for 40 pistons", PistonRing, 40m * 1);
			}

			public void CreateProductInInventory(ZString receiveRef, OrgSupplierPart product, ZDecimal amountToReceiveIntoInventory)
			{
				var receive = Data.Helper.CreateWhsReceive(Data.Org1, Data.Whs1, receiveRef, Data.Notify);
				receive.FillWithValidTestData();
				receive.WD_ArrivalDate = ZDateTimeOffset.Now;
				Data.Helper.CreateWhsReceiveInventoryLine(receive, product, amountToReceiveIntoInventory);

				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			}

			#endregion

			#region BOM Products

			// products

			public OrgSupplierPart Bike { get; private set; }
			public OrgSupplierPart BikeEngine { get; private set; }
			public OrgSupplierPart BikeWheel { get; private set; }

			public OrgSupplierPart Polish { get; private set; }

			public OrgSupplierPart WheelTyre { get; private set; }
			public OrgSupplierPart WheelRim { get; private set; }

			public OrgSupplierPart EngineBlock { get; private set; }
			public OrgSupplierPart EnginePiston { get; private set; }
			public OrgSupplierPart EngineOil { get; private set; }

			public OrgSupplierPart PistonHead { get; private set; }
			public OrgSupplierPart PistonCrank { get; private set; }
			public OrgSupplierPart PistonRing { get; private set; }

			// bom parts (product joins)

			public OrgPartBOM BikeEngineBOMPart { get; private set; }
			public OrgPartBOM BikeWheelBOMPart { get; private set; }
			public OrgPartBOM BikePolishBOMPart { get; private set; }

			public OrgPartBOM WheelTyreBOMPart { get; private set; }
			public OrgPartBOM WheelRimBOMPart { get; private set; }
			public OrgPartBOM WheelPolishBOMPart { get; private set; }

			public OrgPartBOM EngineBlockBOMPart { get; private set; }
			public OrgPartBOM EnginePistonBOMPart { get; private set; }
			public OrgPartBOM EngineOilBOMPart { get; private set; }
			public OrgPartBOM EnginePolishBOMPart { get; private set; }

			public OrgPartBOM PistonHeadBOMPart { get; private set; }
			public OrgPartBOM PistonCrankBOMPart { get; private set; }
			public OrgPartBOM PistonRingBOMPart { get; private set; }

			#endregion

			#region Lines

			public class LinesHelper
			{
				public LinesHelper(BOMHelper parent)
				{
					Parent = parent;
				}

				readonly BOMHelper Parent;

				#region Bike

				public WhsWorkOrderLine Bike(WhsWorkOrder workOrder)
				{
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.SupplierPart.OP_Desc == "Motorbike");
				}

				public WhsWorkOrderLine BikePolish(WhsWorkOrder workOrder)
				{
					WhsWorkOrderLine bikeLine = Bike(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.Polish.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				}

				#region Wheels

				public WhsWorkOrderLine BikeWheel(WhsWorkOrder workOrder)
				{
					WhsWorkOrderLine bikeLine = Bike(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.BikeWheel.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				}

				public WhsWorkOrderLine WheelRim(WhsWorkOrder workOrder)
				{
					WhsWorkOrderLine wheelLine = BikeWheel(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.WheelRim.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
				}

				public WhsWorkOrderLine WheelTyre(WhsWorkOrder workOrder)
				{
					WhsWorkOrderLine wheelLine = BikeWheel(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.WheelTyre.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
				}

				public WhsWorkOrderLine WheelPolish(WhsWorkOrder workOrder)
				{
					WhsWorkOrderLine wheelLine = BikeWheel(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.Polish.PK && line.WE_WE_ParentDocketLine == wheelLine.PK);
				}

				#endregion

				#region Engine

				public WhsWorkOrderLine BikeEngine(WhsWorkOrder workOrder)
				{
					var bikeLine = Bike(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.BikeEngine.PK && line.WE_WE_ParentDocketLine == bikeLine.PK);
				}

				public WhsWorkOrderLine BikeEngineStandAlone(WhsWorkOrder workOrder)
				{
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.BikeEngine.PK && line.WE_WE_ParentDocketLine == ZGuid.Empty);
				}

				public WhsWorkOrderLine EngineBlock(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EngineBlock.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EngineBlockOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngineStandAlone(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EngineBlock.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EnginePiston(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EnginePiston.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EnginePistonOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngineStandAlone(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EnginePiston.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EngineOil(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EngineOil.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EngineOilOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngineStandAlone(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.EngineOil.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EnginePolish(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.Polish.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				public WhsWorkOrderLine EnginePolishOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var engineLine = BikeEngineStandAlone(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.Polish.PK && line.WE_WE_ParentDocketLine == engineLine.PK);
				}

				#region Pistons

				public WhsWorkOrderLine PistonHead(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePiston(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonHead.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				public WhsWorkOrderLine PistonHeadOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePistonOnStandAloneEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonHead.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				public WhsWorkOrderLine PistonCrank(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePiston(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonCrank.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				public WhsWorkOrderLine PistonCrankOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePistonOnStandAloneEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonCrank.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				public WhsWorkOrderLine PistonRing(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePiston(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonRing.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				public WhsWorkOrderLine PistonRingOnStandAloneEngine(WhsWorkOrder workOrder)
				{
					var pistonLine = EnginePistonOnStandAloneEngine(workOrder);
					return (WhsWorkOrderLine)workOrder.AllLines.Single(line => line.WE_OP == Parent.PistonRing.PK && line.WE_WE_ParentDocketLine == pistonLine.PK);
				}

				#endregion

				#endregion

				#endregion
			}

			public LinesHelper Lines
			{
				get { return lines ?? (lines = new LinesHelper(this)); }
			}

			LinesHelper lines;

			#endregion

			#region InventoryQueries

			public class InventoryQueriesHelper
			{
				public InventoryQueriesHelper(BOMHelper parent)
				{
					Parent = parent;
				}

				BusinessObjectFactory Factory { get { return Data.Factory; } }
				TestDataForBOM Data { get { return Parent.Data; } }

				readonly BOMHelper Parent;

				#region CreateInventoryQueries

				public void CreateInventoryQueries()
				{
					bike = new WhsInventoryQuery(Factory, Data.Org1, Parent.Bike);
					bikeEngine = new WhsInventoryQuery(Factory, Data.Org1, Parent.BikeEngine);
					bikeWheel = new WhsInventoryQuery(Factory, Data.Org1, Parent.BikeWheel);

					polish = new WhsInventoryQuery(Factory, Data.Org1, Parent.Polish);

					wheelTyre = new WhsInventoryQuery(Factory, Data.Org1, Parent.WheelTyre);
					wheelRim = new WhsInventoryQuery(Factory, Data.Org1, Parent.WheelRim);

					engineBlock = new WhsInventoryQuery(Factory, Data.Org1, Parent.EngineBlock);
					enginePiston = new WhsInventoryQuery(Factory, Data.Org1, Parent.EnginePiston);
					engineOil = new WhsInventoryQuery(Factory, Data.Org1, Parent.EngineOil);

					pistonHead = new WhsInventoryQuery(Factory, Data.Org1, Parent.PistonHead);
					pistonCrank = new WhsInventoryQuery(Factory, Data.Org1, Parent.PistonCrank);
					pistonRing = new WhsInventoryQuery(Factory, Data.Org1, Parent.PistonRing);
				}

				#endregion

				#region ReloadAll

				public void ReloadAll()
				{
					Bike.Reload();
					BikeEngine.Reload();
					BikeWheel.Reload();

					Polish.Reload();

					WheelTyre.Reload();
					WheelRim.Reload();

					EngineBlock.Reload();
					EnginePiston.Reload();
					EngineOil.Reload();

					PistonHead.Reload();
					PistonCrank.Reload();
					PistonRing.Reload();
				}

				#endregion

				#region Queries

				public WhsInventoryQuery Bike { get { return bike; } }
				public WhsInventoryQuery BikeEngine { get { return bikeEngine; } }
				public WhsInventoryQuery BikeWheel { get { return bikeWheel; } }

				public WhsInventoryQuery Polish { get { return polish; } }

				public WhsInventoryQuery WheelTyre { get { return wheelTyre; } }
				public WhsInventoryQuery WheelRim { get { return wheelRim; } }

				public WhsInventoryQuery EngineBlock { get { return engineBlock; } }
				public WhsInventoryQuery EnginePiston { get { return enginePiston; } }
				public WhsInventoryQuery EngineOil { get { return engineOil; } }

				public WhsInventoryQuery PistonHead { get { return pistonHead; } }
				public WhsInventoryQuery PistonCrank { get { return pistonCrank; } }
				public WhsInventoryQuery PistonRing { get { return pistonRing; } }

				WhsInventoryQuery bike;
				WhsInventoryQuery bikeEngine;
				WhsInventoryQuery bikeWheel;

				WhsInventoryQuery polish;

				WhsInventoryQuery wheelTyre;
				WhsInventoryQuery wheelRim;

				WhsInventoryQuery engineBlock;
				WhsInventoryQuery enginePiston;
				WhsInventoryQuery engineOil;

				WhsInventoryQuery pistonHead;
				WhsInventoryQuery pistonCrank;
				WhsInventoryQuery pistonRing;

				#endregion

				#region WhsInventoryQuery

				public class WhsInventoryQuery
				{
					public WhsInventoryQuery(BusinessObjectFactory factory, OrgHeader client, OrgSupplierPart part)
					{
						Factory = factory;
						Client = client;
						Part = part;
					}

					BusinessObjectFactory Factory { get; }
					OrgHeader Client { get; }
					OrgSupplierPart Part { get; }

					public decimal UnitsAvailable
					{
						get
						{
							if (inventories == null)
							{
								var query = new ZQuery(WhsInventoryViewSchema.WI_OH_Client, Client.PK);
								query.AddToFilter(WhsInventoryViewSchema.WI_OP, Part.PK);
								query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

								// Factory caches query results for same query text and it does not know that changing docketline values will affect WhsInventoryView.
								// so if we execute the same query a second time the factory will give old results instead of hitting the database.
								Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);

								var loadedInventory = Factory.Load<WhsInventoryView>(query);
								inventories = new WhsTestHelperFunctions.InventoriesWrapper(loadedInventory);

								// add fetch hints to avoid 1 DB hit per inventory
								foreach (var inv in loadedInventory)
								{
									Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inv.WI_WE_InDocketLine);
									Factory.AddFetchHint(OrgSupplierPartSchema.PK, inv.WI_OP);
								}

								foreach (var inv in loadedInventory)
								{
									foreach (var pickLine in inv.AllPickLines)
									{
										Factory.AddFetchHint(WhsDocketLineSchema.PK, pickLine.WZ_WE_TransactionLine);
									}
								}
							}

							return inventories.UnitsAvailable;
						}
					}

					public void Reload() => inventories = null;

					WhsTestHelperFunctions.InventoriesWrapper inventories;
				}

				#endregion
			}

			public InventoryQueriesHelper InventoryQueries
			{
				get { return inventoryQueries ?? (inventoryQueries = new InventoryQueriesHelper(this)); }
			}

			InventoryQueriesHelper inventoryQueries;

			#endregion

			#region Bulk Load Deep Level Master Child Work Orders using dummy products

			public void BulkLoadDeepLevelMasterChildWorkOrders(WhsWorkOrder workOrder, int levels)
			{
				OrgSupplierPart part = Data.Part1;
				OrgSupplierPart subPart;
				for (int idx = 0; idx < levels - 1; idx++)
				{
					subPart = Data.Helper.CreateProduct(Data.Org1, "P1" + idx);
					Data.Helper.CreateProductBOM(part, subPart, 1m, "UNT");
					part = subPart;
				}

				workOrder.WD_OH_Client = Data.Org1.PK;
				workOrder.WD_WW_Whs = Data.Whs1.PK;
				WhsWorkOrderLine line1 = Data.Helper.CreateWhsWorkOrderLine(workOrder, Data.Part1, 10m);
				workOrder.BOM.ExpandAllLines();
				workOrder.BOM.AutoCreateWorkOrders(new NotificationBuffer());
				AssertEquals(levels, workOrder.Lines.Count);
			}

			#endregion

			#region StagingLocation

			public void SetPartStagingLocation(WhsProduct product, WhsLocation stagingLocation)
			{
				SetPartStagingLocation(product, stagingLocation, Data.Org1);
			}

			public void SetPartStagingLocation(WhsProduct product, WhsLocation stagingLocation, OrgHeader client)
			{
				SetPartStagingLocation(product, stagingLocation, client, Data.Whs1);
			}

			public void SetPartStagingLocation(WhsProduct product, WhsLocation stagingLocation, OrgHeader client, WhsWarehouse warehouse)
			{
				var part = product.Parent;
				WhsProductParamsByWhsAndClient productParams = product.ParamsByWhsAndClient.AddNew();

				productParams.W3_OP = part.PK;
				productParams.W3_OH = client.PK;
				productParams.W3_WW = warehouse.PK;
				productParams.W3_WL_StagingLocationBOM = stagingLocation.PK;
			}

			#endregion
		}

		public BOMHelper BOM
		{
			get { return bom ?? (bom = new BOMHelper(this)); }
		}

		BOMHelper bom;

		#endregion
	}

	#endregion

	#region DummyAfterOnSavingBOProcessingService class

	public class DummyAfterOnSavingBOProcessingService : IAfterOnSavingBOProcessingService
	{
		public DummyAfterOnSavingBOProcessingService(BusinessObjectFactory factory, Action<IEnumerable<BusinessObject>> doSomething)
		{
			Factory = factory;
			DoSomething = doSomething;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in DummyAfterOnSavingBOProcessingService")]
		readonly BusinessObjectFactory Factory;
		readonly Action<IEnumerable<BusinessObject>> DoSomething;

		void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			DoSomething(businessObjectsInOnSavingOrder);
		}
	}

	#endregion
}
