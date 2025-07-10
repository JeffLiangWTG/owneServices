using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TestOrgMiscServValidation : BusinessObjectValidationTestCase
	{
		#region CheckOM_ConsigneeAuthorityToLeave

		public void CheckOM_ConsigneeAuthorityToLeave()
		{
			MiscServ.OM_ConsigneeAuthorityToLeave = "XXX";
			AssertHasError(MiscServ.OM_ConsigneeAuthorityToLeaveInfo, "Enter a valid Authority To Leave.");

			MiscServ.OM_ConsigneeAuthorityToLeave = "DEF";
			AssertNoErrors("DEF is a valid Authority To Leave option.", MiscServ.OM_ConsigneeAuthorityToLeaveInfo);

			MiscServ.OM_ConsigneeAuthorityToLeave = "YES";
			AssertNoErrors("YES is a valid Authority To Leave option.", MiscServ.OM_ConsigneeAuthorityToLeaveInfo);

			MiscServ.OM_ConsigneeAuthorityToLeave = "NO";
			AssertNoErrors("NO is a valid Authority To Leave option.", MiscServ.OM_ConsigneeAuthorityToLeaveInfo);

			MiscServ.OM_ConsigneeAuthorityToLeave = "";
			AssertHasError(MiscServ.OM_ConsigneeAuthorityToLeaveInfo, "Enter a valid Authority To Leave.");
		}

		#endregion

		#region CheckOM_ConsignorAuthorityToLeave

		public void CheckOM_ConsignorAuthorityToLeave()
		{
			MiscServ.OM_ConsignorAuthorityToLeave = "XXX";
			AssertHasError(MiscServ.OM_ConsignorAuthorityToLeaveInfo, "Enter a valid Authority To Leave.");

			MiscServ.OM_ConsignorAuthorityToLeave = "DEF";
			AssertNoErrors("DEF is a valid Authority To Leave option.", MiscServ.OM_ConsignorAuthorityToLeaveInfo);

			MiscServ.OM_ConsignorAuthorityToLeave = "YES";
			AssertNoErrors("YES is a valid Authority To Leave option.", MiscServ.OM_ConsignorAuthorityToLeaveInfo);

			MiscServ.OM_ConsignorAuthorityToLeave = "NO";
			AssertNoErrors("NO is a valid Authority To Leave option.", MiscServ.OM_ConsignorAuthorityToLeaveInfo);

			MiscServ.OM_ConsignorAuthorityToLeave = "";
			AssertHasError(MiscServ.OM_ConsignorAuthorityToLeaveInfo, "Enter a valid Authority To Leave.");
		}

		#endregion

		#region Warehouse

		#region TestCheckOM_WhsABCAnalysisMethod

		public void TestCheckOM_WhsABCAnalysisMethod()
		{
			AssertNoErrors("Precondition", MiscServ.OM_WhsABCAnalysisMethodInfo);

			MiscServ.OM_WhsABCAnalysisMethod = "XXX";
			AssertHasError(MiscServ.OM_WhsABCAnalysisMethodInfo, "Enter a valid ABC Analysis Method.");

			MiscServ.OM_WhsABCAnalysisMethod = "VLC";
			AssertNoErrors(MiscServ.OM_WhsABCAnalysisMethodInfo);

			MiscServ.OM_WhsABCAnalysisMethod = "";
			AssertHasError(MiscServ.OM_WhsABCAnalysisMethodInfo, "Please enter an ABC Analysis Method.");

			MiscServ.OM_WhsABCAnalysisMethod = "VLC";
			AssertNoErrors(MiscServ.OM_WhsABCAnalysisMethodInfo);
		}

		#endregion

		#region TestCheckOM_WhsABCAnalysisPeriod

		public void TestCheckOM_WhsABCAnalysisPeriod()
		{
			AssertNoErrors("Precondition", MiscServ.OM_WhsABCAnalysisPeriodInfo);

			MiscServ.OM_WhsABCAnalysisPeriod = "XXX";
			AssertHasError(MiscServ.OM_WhsABCAnalysisPeriodInfo, "Enter a valid ABC Analysis Period.");

			MiscServ.OM_WhsABCAnalysisPeriod = "WKY";
			AssertNoErrors(MiscServ.OM_WhsABCAnalysisPeriodInfo);

			MiscServ.OM_WhsABCAnalysisPeriod = "";
			AssertHasError(MiscServ.OM_WhsABCAnalysisPeriodInfo, "Please enter an ABC Analysis Period.");

			MiscServ.OM_WhsABCAnalysisPeriod = "WKY";
			AssertNoErrors(MiscServ.OM_WhsABCAnalysisPeriodInfo);
		}

		#endregion

		#region TestCheckOM_IMDefaultWarehousePickOption

		public void TestCheckOM_IMDefaultWarehousePickOption()
		{
			AssertNoErrors(MiscServ.OM_IMDefaultWarehousePickOptionInfo);

			MiscServ.OM_IMDefaultWarehousePickOption = "";
			AssertHasErrors(MiscServ.OM_IMDefaultWarehousePickOptionInfo);

			MiscServ.OM_IMDefaultWarehousePickOption = "XXX";
			AssertHasErrors(MiscServ.OM_IMDefaultWarehousePickOptionInfo);

			MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.ManualWithAutoAllocate;
			AssertNoErrors(MiscServ.OM_IMDefaultWarehousePickOptionInfo);
		}

		#endregion

		#region TestCheckOM_WCG_CartonGroup

		public void TestCheckOM_WCG_CartonGroup()
		{
			AssertNoErrors(MiscServ.OM_WCG_CartonGroupInfo);

			MiscServ.OM_WCG_CartonGroup = Company.PK;
			AssertHasError(MiscServ.OM_WCG_CartonGroupInfo, "Enter a valid Carton Group.");

			var group = Factory.New<IWhsCartonGroup>();
			group.WCG_Code = "G1";
			group.WCG_Description = "Group";

			MiscServ.OM_WCG_CartonGroup = group.PK;
			AssertNoErrors(MiscServ.OM_WCG_CartonGroupInfo);
		}

		#endregion

		#region TestCheckOM_WhsOrderFulfillmentRule

		public void TestCheckOM_WhsOrderFulfillmentRule()
		{
			MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			AssertNoErrors(MiscServ.OM_WhsOrderFulfillmentRuleInfo);

			MiscServ.OM_WhsOrderFulfillmentRule = "";
			AssertHasErrors(MiscServ.OM_WhsOrderFulfillmentRuleInfo);

			MiscServ.OM_WhsOrderFulfillmentRule = "XXX";
			AssertHasErrors(MiscServ.OM_WhsOrderFulfillmentRuleInfo);

			MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
			AssertNoErrors(MiscServ.OM_WhsOrderFulfillmentRuleInfo);
		}

		#endregion

		#region TestCheckOM_WhsOrderDefaultPickPriority

		public void TestCheckOM_WhsOrderDefaultPickPriority()
		{
			AssertNoErrors(MiscServ.OM_WhsOrderDefaultPickPriorityInfo);

			MiscServ.OM_WhsOrderDefaultPickPriority = 21;
			AssertHasErrors(MiscServ.OM_WhsOrderDefaultPickPriorityInfo);

			MiscServ.OM_WhsOrderDefaultPickPriority = 1;
			AssertNoErrors(MiscServ.OM_WhsOrderDefaultPickPriorityInfo);
		}

		#endregion

		#region TestCheckOM_WhsDefaultWarehousePickMode

		public void TestCheckOM_WhsDefaultWarehousePickMode()
		{
			AssertNoErrors(MiscServ.OM_WhsOrderFulfillmentRuleInfo);

			MiscServ.OM_WhsDefaultWarehousePickMode = "";
			AssertHasErrors(MiscServ.OM_WhsDefaultWarehousePickModeInfo);

			MiscServ.OM_WhsDefaultWarehousePickMode = "XXX";
			AssertHasErrors(MiscServ.OM_WhsDefaultWarehousePickModeInfo);

			MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeNeutral;
			AssertNoErrors(MiscServ.OM_WhsDefaultWarehousePickModeInfo);
		}

		#endregion

		#region Warehouse Part Attribute Rules Validation

		#region TestValidatePartAttributes

		public void TestValidatePartAttributes()
		{
			// detailed validation tests reside in PartAttributeValidation
			// here we are only verifying PartAttributeValidation is being used
			AssertPartAttributes(Company.MiscServ.OM_IMPartAttrib1NameInfo, Company.MiscServ.OM_IMPartAttrib1TypeInfo);
			AssertPartAttributes(Company.MiscServ.OM_IMPartAttrib2NameInfo, Company.MiscServ.OM_IMPartAttrib2TypeInfo);
			AssertPartAttributes(Company.MiscServ.OM_IMPartAttrib3NameInfo, Company.MiscServ.OM_IMPartAttrib3TypeInfo);

			Company.MiscServ.OM_IMPartAttrib1Name = "Name";
			Company.MiscServ.OM_IMPartAttrib2Name = "Name";
			AssertHasErrors("Same value, should have error", Company.MiscServ.OM_IMPartAttrib2NameInfo);
		}

		void AssertPartAttributes(ZPropertyInfo nameInfo, ZPropertyInfo typeInfo)
		{
			nameInfo.Value = ZString.Empty;
			typeInfo.Value = ZString.Empty;
			AssertNoErrors("blank name and type should not cause error", nameInfo);
			AssertNoErrors("blank name and type should not cause error", typeInfo);

			typeInfo.Value = new ZString(new PartAttributeTypeList()[0].Code);
			AssertNoErrors("blank name and setting valid type should not cause error", typeInfo);

			nameInfo.Value = new ZString("Name");
			AssertNoErrors("name not blank type not blank should not cause error", nameInfo);

			nameInfo.Value = ZString.Empty;
			AssertHasErrors("setting name blank with something in type should cause error", nameInfo);

			nameInfo.Value = new ZString("Name");
			typeInfo.Value = ZString.Empty;
			AssertHasErrors("setting type blank with something in name should cause error", typeInfo);

			typeInfo.Value = new ZString("XXX");
			AssertHasErrors("setting type to invalid valie should cause error", typeInfo);

			nameInfo.Value = ZString.Empty;
			typeInfo.Value = ZString.Empty;
		}

		#endregion

		#region TestCheckOM_IMPartAttribType_CannotBeChangedIfStockOnHandUsingAttributeExists

		/// 
		/// DO NOT REMOVE THESE TESTS UNLESS YOU ADD TESTS TO PartAttributeValidation (OR DELETE THE CODE THERE).
		/// 

		public void TestCheckOM_IMPartAttrib1Type_CannotBeChangedIfStockOnHandUsingAttributeExists()
		{
			AssertCheckOM_IMPartAttribType_CannotBeChangedIfStockOnHandUsingAttributeExists(OrgMiscServSchema.OM_IMPartAttrib1Type, 1);
		}

		public void TestCheckOM_IMPartAttrib2Type_CannotBeChangedIfStockOnHandUsingAttributeExists()
		{
			AssertCheckOM_IMPartAttribType_CannotBeChangedIfStockOnHandUsingAttributeExists(OrgMiscServSchema.OM_IMPartAttrib2Type, 2);
		}

		public void TestCheckOM_IMPartAttrib3Type_CannotBeChangedIfStockOnHandUsingAttributeExists()
		{
			AssertCheckOM_IMPartAttribType_CannotBeChangedIfStockOnHandUsingAttributeExists(OrgMiscServSchema.OM_IMPartAttrib3Type, 3);
		}

		void AssertCheckOM_IMPartAttribType_CannotBeChangedIfStockOnHandUsingAttributeExists(SchemaStringColumn partAttribTypeColumn, byte attribNo)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var whs = helper.CreateWarehouse("MEL", "A");
			var owner1 = Factory.Load<OrgHeader>(helper.CreateClient("Owner1"));
			var owner2 = Factory.Load<OrgHeader>(helper.CreateClient("Owner2"));
			var owner3 = Factory.Load<OrgHeader>(helper.CreateClient("Owner3"));
			Factory.Save();

			// use attribute #1 for all owners
			owner1.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			owner3.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;

			// create Parts A + B for Owner 1, and Part C for Owner 2
			var partA = (OrgSupplierPart)helper.CreateProduct(owner1.PK, "PartA");
			var partB = (OrgSupplierPart)helper.CreateProduct(owner1.PK, "PartB");
			var partC = (OrgSupplierPart)helper.CreateProduct(owner2.PK, "PartC");

			// use attribute #1 for Parts A + C (not B)
			owner1.PartAttributeManager.SetProductToUseAttribute(partA, attribNo, true);
			owner2.PartAttributeManager.SetProductToUseAttribute(partC, attribNo, true);

			// create stock for Parts B + C
			helper.CreateStock(whs.PK, owner1.PK, "R1", partB.PK, 10m);

			BusinessObject stock;

			switch (attribNo)
			{
				case 1:
					stock = helper.CreateStock(whs.PK, owner2.PK, "R2", partC.PK, 10m, "ABC");
					break;
				case 2:
					stock = helper.CreateStock(whs.PK, owner2.PK, "R2", partC.PK, 10m, "", "ABC");
					break;
				case 3:
					stock = helper.CreateStock(whs.PK, owner2.PK, "R2", partC.PK, 10m, "", "", "ABC");
					break;
				default:
					throw new NotSupportedException();
			}

			// create new relation
			partC.RelatedOrganisations.AddOwner(owner3);
			owner3.PartAttributeManager.SetProductToUseAttribute(partC, attribNo, true);

			// add stock for same product with different owner
			switch (attribNo)
			{
				case 1:
					helper.CreateStock(whs.PK, owner3.PK, "DR1", partC.PK, 10m, "ABC");
					break;
				case 2:
					helper.CreateStock(whs.PK, owner3.PK, "DR1", partC.PK, 10m, "", "ABC");
					break;
				case 3:
					helper.CreateStock(whs.PK, owner3.PK, "DR1", partC.PK, 10m, "", "", "ABC");
					break;
				default:
					throw new NotSupportedException();
			}

			Factory.Save();

			// attribute type remains unchanged, no error expected
			AssertNoErrors(owner1.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
			AssertNoErrors(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);

			// attribute type has changed, expect an error where the client has a product using attribute 1, and has stock
			owner1.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			AssertNoErrors(owner1.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
			AssertHasError(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name],
				"This attribute must remain as 'BAT' because there is current inventory for products using this attribute.\r\n" +
				"You must remove this inventory from the warehouse before this attribute can be changed.");

			// change the attribute type back to what it was, no error expected
			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertNoErrors(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);

			var orderPK = helper.CreateWhsOrder(owner2.PK, whs.PK, "O1", null);
			var orderLinePK = helper.CreateWhsOrderLine(orderPK, partC.PK, 10);
			var pickPK = helper.CreateWhsPick(new[] { orderPK });
			var pickLine = helper.GetPickLines(pickPK).Single();
			helper.PickAndMakeInTransitTransfer(pickLine, ZDateTime.Now);
			Factory.Save();

			owner1.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			AssertNoErrors(owner1.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
			AssertHasError("Should show error for In-Transit stock.", owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name],
				"This attribute must remain as 'BAT' because there is current inventory for products using this attribute.\r\n" +
				"You must remove this inventory from the warehouse before this attribute can be changed.");

			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			AssertNoErrors(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);

			helper.FinaliseDocket(orderPK);
			helper.FinalisePick(pickPK);
			Factory.Save();

			// attribute type remains unchanged, no error expected
			AssertNoErrors(owner1.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
			AssertNoErrors(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);

			// attribute type has changed, but inventory amount is zero so there should be no error
			owner1.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			owner2.MiscServ[partAttribTypeColumn] = PartAttributeTypeList.Codes.VIN;
			AssertNoErrors(owner1.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
			AssertNoErrors(owner2.MiscServ.ZPropertyInfoHash[partAttribTypeColumn.Name]);
		}

		#endregion

		#region TestCheckPartAttributeTypesForDuplicateJulianBatchNumberTypes

		public void TestCheckPartAttributeTypesForDuplicateJulianBatchNumberTypes()
		{
			AssertJulianBatchNumberAttributeValidation(m => m.OM_IMPartAttrib1TypeInfo, m => m.OM_IMPartAttrib2TypeInfo, m => m.OM_IMPartAttrib3TypeInfo);
			AssertJulianBatchNumberAttributeValidation(m => m.OM_IMPartAttrib2TypeInfo, m => m.OM_IMPartAttrib1TypeInfo, m => m.OM_IMPartAttrib3TypeInfo);
			AssertJulianBatchNumberAttributeValidation(m => m.OM_IMPartAttrib3TypeInfo, m => m.OM_IMPartAttrib1TypeInfo, m => m.OM_IMPartAttrib2TypeInfo);
		}

		void AssertJulianBatchNumberAttributeValidation(Func<OrgMiscServ, ZPropertyInfo> getMatchProperty, Func<OrgMiscServ, ZPropertyInfo> getProperty1, Func<OrgMiscServ, ZPropertyInfo> getProperty2)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var owner = Factory.Load<OrgHeader>(helper.CreateClient("Org"));
			var matchingProperty = getMatchProperty(owner.MiscServ);
			var property1 = getProperty1(owner.MiscServ);
			var property2 = getProperty2(owner.MiscServ);

			matchingProperty.Value = (ZString)PartAttributeTypeList.Codes.JulianBatchNumber;
			property1.Value = (ZString)PartAttributeTypeList.Codes.JulianBatchNumber;
			property2.Value = (ZString)PartAttributeTypeList.Codes.JulianBatchNumber;
			AssertNoErrors(matchingProperty);
			AssertHasError(property1, "Julian Batch number Attribute type should not be duplicated.");
			AssertHasError(property2, "Julian Batch number Attribute type should not be duplicated.");

			property1.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertNoErrors(matchingProperty);
			AssertNoErrors(property1);
			AssertHasError(property2, "Julian Batch number Attribute type should not be duplicated.");

			property1.Value = ZString.Empty;
			AssertNoErrors(matchingProperty);
			AssertNoErrors(property1);
			AssertHasError(property2, "Julian Batch number Attribute type should not be duplicated.");

			property1.Value = (ZString)PartAttributeTypeList.Codes.JulianBatchNumber;
			property2.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertNoErrors(matchingProperty);
			AssertHasError(property1, "Julian Batch number Attribute type should not be duplicated.");
			AssertNoErrors(property2);

			property2.Value = ZString.Empty;
			AssertNoErrors(matchingProperty);
			AssertHasError(property1, "Julian Batch number Attribute type should not be duplicated.");
			AssertNoErrors(property2);

			property1.Value = ZString.Empty;
			AssertNoErrors(matchingProperty);
			AssertNoErrors(property1);
			AssertNoErrors(property2);

			property1.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			property2.Value = (ZString)PartAttributeTypeList.Codes.Mandatory;
			AssertNoErrors(matchingProperty);
			AssertNoErrors(property1);
			AssertNoErrors(property2);
		}

		#endregion

		#region TestCheckOM_IMUseExpiryDate

		public void TestCheckOM_IMUseExpiryDate()
		{
			var org = Factory.New<OrgHeader>();
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(org.MiscServ, OrgMiscServSchema.OM_IMPartAttrib1Type);
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(org.MiscServ, OrgMiscServSchema.OM_IMPartAttrib2Type);
			AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(org.MiscServ, OrgMiscServSchema.OM_IMPartAttrib3Type);
		}

		void AssertExpiryDateIsUsedIfJulianBatchNumberIsUsed(OrgMiscServ miscServ, SchemaStringColumn partAttributeTypeColumn)
		{
			var expectedErrorMessage = "'Use Expiry Date' must be enabled in conjunction with Julian Batch Numbers.";
			AssertNoError(miscServ.OM_IMUseExpiryDateInfo, expectedErrorMessage);

			miscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.JulianBatchNumber;
			miscServ.OM_IMUseExpiryDate = true;
			AssertNoError(miscServ.OM_IMUseExpiryDateInfo, expectedErrorMessage);

			miscServ.OM_IMUseExpiryDate = false;
			AssertHasError(miscServ.OM_IMUseExpiryDateInfo, expectedErrorMessage);

			miscServ[partAttributeTypeColumn] = PartAttributeTypeList.Codes.BatchNumber;
			miscServ.OM_IMUseExpiryDate = false;
			AssertNoError(miscServ.OM_IMUseExpiryDateInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckOM_IMUseSerialNumber

		public void TestCheckOM_IMUseSerialNumber_ProductHasSerialNumberEnabled()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var whs = helper.CreateWarehouse("MEL", "A");
			var owner1 = Factory.Load<OrgHeader>(helper.CreateClient("Owner1"));
			var owner2 = Factory.Load<OrgHeader>(helper.CreateClient("Owner2"));
			Factory.Save();

			var partA = (OrgSupplierPart)helper.CreateProduct(owner1.PK, "PartA");
			var partB = (OrgSupplierPart)helper.CreateProduct(owner2.PK, "PartB");

			Factory.Save();

			AssertNoErrors("No error expected.", owner1.MiscServ.OM_IMUseSerialNumberInfo);
			AssertNoErrors("No error expected.", owner2.MiscServ.OM_IMUseSerialNumberInfo);

			owner1.MiscServ.OM_IMUseSerialNumber = true;
			owner2.MiscServ.OM_IMUseSerialNumber = true;

			AssertNoErrors("No error expected.", owner1.MiscServ.OM_IMUseSerialNumberInfo);
			AssertNoErrors("No error expected.", owner2.MiscServ.OM_IMUseSerialNumberInfo);

			owner1.PartAttributeManager.SetProductToUseAttribute(partA, 6, false);
			owner2.PartAttributeManager.SetProductToUseAttribute(partB, 6, true);

			Factory.Save();

			owner1.MiscServ.OM_IMUseSerialNumber = false;
			owner2.MiscServ.OM_IMUseSerialNumber = false;
			AssertNoErrors("No product exists with serial enabled, no errors should arise.", owner1.MiscServ.OM_IMUseSerialNumberInfo);
			AssertHasError("Product exists with serial enabled, error should arise.", owner2.MiscServ.OM_IMUseSerialNumberInfo,
				"This attribute is being used by at least one product with a relationship to this organization.\r\n" +
				"These product relationships must have this attribute disabled before you can disable it for the organization.\r\n" +
				"See Product Entry.");

			owner2.MiscServ.OM_IMUseSerialNumber = true;
			AssertNoErrors("Reset flag, error should be removed.", owner2.MiscServ.OM_IMUseSerialNumberInfo);
		}

		#endregion

		#endregion

		#region TestCheckOM_MinimumShelfLifeAccepted

		public void TestCheckOM_MinimumShelfLifeAccepted()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			var expectedWarningMessage = "Minimum Shelf Life is less than 30 days.";
			var expectedErrorMessage = "Minimum Shelf Life (Days) cannot be negative.";

			AssertNoError(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
			AssertNoWarning(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = -5;
			AssertHasError(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
			AssertNoWarning(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 15;
			AssertNoError(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
			AssertHasWarning(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 30;
			AssertNoError(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
			AssertNoWarning(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 45;
			AssertNoError(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
			AssertNoWarning(miscServ.OM_MinimumShelfLifeAcceptedInfo, expectedWarningMessage);
		}

		#endregion

		#region TestGetMinimumShelfLifeRangeCheckingMessage

		public void TestGetMinimumShelfLifeRangeCheckingMessage()
		{
			var miscServ = Factory.New<OrgMiscServ>();
			var expectedWarningMessage = "Minimum Shelf Life is less than 30 days.";
			var info = miscServ.OM_MinimumShelfLifeAcceptedInfo;

			miscServ.OM_MinimumShelfLifeAccepted = ZShort.Zero;
			miscServ.Validation.ValidateOM_MinimumShelfLifeAccepted();
			AssertNoWarning(info, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = -5;
			miscServ.Validation.ValidateOM_MinimumShelfLifeAccepted();
			AssertNoWarning(info, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 15;
			miscServ.Validation.ValidateOM_MinimumShelfLifeAccepted();
			AssertHasWarning(info, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 30;
			miscServ.Validation.ValidateOM_MinimumShelfLifeAccepted();
			AssertNoWarning(info, expectedWarningMessage);

			miscServ.OM_MinimumShelfLifeAccepted = 45;
			miscServ.Validation.ValidateOM_MinimumShelfLifeAccepted();
			AssertNoWarning(info, expectedWarningMessage);
		}

		#endregion

		#region TestCheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfNotFinalisedPicksThatUseJulianBatchNumbersExist

		public void TestCheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfNotFinalisedPicksThatUseJulianBatchNumbersExist()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			var warehouse = helper.CreateWarehouse("WHS", "A");
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "CLIENT";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.JulianBatchNumber;

			var part_NotExpiryDate = Factory.New<OrgSupplierPart>();
			var part_UseExpiryDate = Factory.New<OrgSupplierPart>();
			part_NotExpiryDate.OP_PartNum = "P1";
			part_UseExpiryDate.OP_PartNum = "P2";

			var relation_NotExpiryDate = part_NotExpiryDate.RelatedOrganisations.AddOwner(client);
			var relation_UseExpiryDate = part_UseExpiryDate.RelatedOrganisations.AddOwner(client);
			relation_NotExpiryDate.OU_UsePartAttrib1 = true;
			relation_UseExpiryDate.OU_UsePartAttrib2 = true;
			relation_UseExpiryDate.OU_UseExpiryDate = true;

			var expectedErrorMessage = "Minimum Shelf Life cannot be increased because there are un-finalized Picks for this Consignee that have Products with expiry dates.";

			// If there no Orders or Picks for the consignee.
			Factory.Save();
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			AssertNoError("Precondition", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 8;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			// With Orders
			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			helper.CreateWhsOrderLine(order.PK, part_NotExpiryDate.PK, 10m);
			Factory.Save();
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 7;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			// With Pick for normal attribute
			helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 6;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			// With Pick for Julian Batch Number.
			var orderline = helper.CreateWhsOrderLine(order.PK, part_UseExpiryDate.PK, 10m);
			Factory.Save();
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertHasError("User should NOT be allowed to increase Minimum Shelf Life.", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			AssertNoError("User should be allowed to decrease Minimum Shelf Life.", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfPickInprogressHasProductWithExpiryDate

		public void TestCheckOM_MinimumShelfLifeAccepted_CannotBeModifiedIfPickInprogressHasProductWithExpiryDate()
		{
			var today = ZDate.Today;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("MEL", "A");
			var client = Factory.Load<OrgHeader>(helper.CreateClient("Owner"));
			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "PartA");
			helper.CreateProductParamsByWhsAndClient(part.PK, client.PK, warehouse.PK, 60);
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";
			client.MiscServ.OM_IMUseExpiryDate = true;

			var relation = part.RelatedOrganisations.Cast<OrgPartRelation>().Single();
			relation.OU_UseExpiryDate = true;
			Factory.Save();

			var receivePK = helper.CreateWhsReceive(client.PK, warehouse.PK, "R1", new ZArchitecture.NotificationBuffer());
			var receiveLinePK = helper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A");
			Factory.Load<IWhsDocketLine>(receiveLinePK).WE_ExpiryDate = today.AddDays(10);
			helper.WhsReceiveAllocateLocationsMock(receivePK);
			helper.FinaliseDocketWithoutUserConfirmation(receivePK);

			var expectedErrorMessage = "Minimum Shelf Life cannot be increased because there are un-finalized Picks for this Consignee that have Products with expiry dates.";

			// If there no Orders or Picks for the consignee.
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 0;
			AssertNoError("Precondition", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 8;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			// With Orders
			var order = helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, "O1");
			var orderLinePK = helper.CreateWhsOrderLine(order.PK, part.PK, 10m);
			Factory.Load<IWhsDocketLine>(orderLinePK).WE_ExpiryDate = today.AddDays(10);
			Factory.Save();
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 7;
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			// Pick
			var pickPK = helper.CreateWhsPick(new ZGuid[] { order.PK });
			Factory.Save();
			AssertNoError(consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;
			AssertHasError("User should NOT be allowed to increase Minimum Shelf Life.", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			AssertNoError("User should be allowed to decrease Minimum Shelf Life.", consignee.MiscServ.OM_MinimumShelfLifeAcceptedInfo, expectedErrorMessage);
		}

		#endregion

		#region HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttrbutes

		#region TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Receive()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Receive(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Held()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Held(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_AdjustmentIn()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_AdjustmentIn(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Transfer()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Transfer(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_TransferLineFinalised()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_TransferLineFinalised(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_InTransit()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_InTransit(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		public void TestCheckOM_IMPartAttrib1Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Staged()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Staged(OrgMiscServSchema.OM_IMPartAttrib1Type, OrgPartRelationSchema.OU_UsePartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
		}

		#endregion

		#region TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Receive()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Receive(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Held()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Held(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_AdjustmentIn()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_AdjustmentIn(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Transfer()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Transfer(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_TransferLineFinalised()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_TransferLineFinalised(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_InTransit()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_InTransit(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		public void TestCheckOM_IMPartAttrib2Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Staged()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Staged(OrgMiscServSchema.OM_IMPartAttrib2Type, OrgPartRelationSchema.OU_UsePartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
		}

		#endregion

		#region TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Receive()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Receive(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Held()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Held(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_AdjustmentIn()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_AdjustmentIn(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Transfer()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Transfer(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_TransferLineFinalised()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_TransferLineFinalised(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_InTransit()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_InTransit(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		public void TestCheckOM_IMPartAttrib3Type_HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributes_Staged()
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Staged(OrgMiscServSchema.OM_IMPartAttrib3Type, OrgPartRelationSchema.OU_UsePartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
		}

		#endregion

		#region HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Receive(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Receive);
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Held(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Held);
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_AdjustmentIn(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_AdjustmentIn);
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Transfer(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn,
				(helper, whs, client, part, partAttrib) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, part, partAttrib, false));
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_TransferLineFinalised(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn,
				(helper, whs, client, part, partAttrib) => InventoryTestHelperForPartAttributeValidation.CreateInventory_Transfer(helper, whs, client, part, partAttrib, true));
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_InTransit(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_InTransit);
		}

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore_Staged(SchemaColumn miscServColumn, SchemaColumn relationColumn, SchemaColumn docketLineColumn)
		{
			HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(miscServColumn, relationColumn, docketLineColumn, InventoryTestHelperForPartAttributeValidation.CreateInventory_Staged);
		}

		delegate void CreateInventoryDelegate(IWhsTransactionTestHelper helper, IWhsWarehouse warehouse, OrgHeader org, OrgSupplierPart part, SchemaColumn partAttribColumn);

		void HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttributesCore(SchemaColumn orgMiscServPartAttributeColumn, SchemaColumn relationColumn, SchemaColumn docketLineAttributeColumn, CreateInventoryDelegate createInventory)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = (IWhsWarehouse)helper.CreateWarehouse("WH1", "A", 2, 2);
			var client = Factory.Load<OrgHeader>(helper.CreateClient("CLIENT1"));

			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServAttribs(client, PartAttributeTypeList.Codes.NonMandatory);

			var part = (OrgSupplierPart)helper.CreateProduct(client.PK, "P1");
			var relation = part.RelatedOrganisations.Single();
			relation[relationColumn] = true;
			Factory.Save();

			var propertyInfo = client.MiscServ.ZPropertyInfoHash[orgMiscServPartAttributeColumn.Name];
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);

			createInventory(helper, whs, client, part, null); // no attribute
			Factory.Save();

			// Set bad state
			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServAttribs(client, PartAttributeTypeList.Codes.Mandatory);
			client.MiscServ.RunPreSaveValidation();
			AssertHasError("Should return error as there is inventory.", propertyInfo, "You are changing this attribute from non-mandatory to mandatory but there is current inventory for products using this attribute where the attribute is empty. You must remove this inventory from the warehouse before this attribute can be made mandatory");

			// Revert, assert no errors
			InventoryTestHelperForPartAttributeValidation.SetOrgMiscServAttribs(client, PartAttributeTypeList.Codes.NonMandatory);
			client.MiscServ.RunPreSaveValidation();
			AssertNoErrors("Precondition: Should not return error.", propertyInfo);
		}

		#endregion

		#endregion

		#region CheckOM_WhsPackageWeightTolerance

		public void TestCheckOM_WhsPackageWeightTolerance()
		{
			AssertNoErrors("Precondition", MiscServ.OM_WhsPackageWeightTolerancePercentInfo);

			MiscServ.OM_WhsPackageWeightTolerancePercent = 100m;
			AssertNoErrors("Value within acceptable range of values", MiscServ.OM_WhsPackageWeightTolerancePercentInfo);

			MiscServ.OM_WhsPackageWeightTolerancePercent = 0m;
			AssertNoErrors("Value within acceptable range of values", MiscServ.OM_WhsPackageWeightTolerancePercentInfo);

			MiscServ.OM_WhsPackageWeightTolerancePercent = 50m;
			AssertNoErrors("Value within acceptable range of values", MiscServ.OM_WhsPackageWeightTolerancePercentInfo);

			MiscServ.OM_WhsPackageWeightTolerancePercent = 101m;
			AssertHasError(MiscServ.OM_WhsPackageWeightTolerancePercentInfo, "Please enter a valid package weight tolerance level value. Value should be from 0-100.");

			MiscServ.OM_WhsPackageWeightTolerancePercent = -1m;
			AssertHasError(MiscServ.OM_WhsPackageWeightTolerancePercentInfo, "Please enter a valid package weight tolerance level value. Value should be from 0-100.");
		}

		#endregion

		#endregion

		#region TestCheckOM_IMAirCargoReportDefaultConsigne

		public void TestCheckOM_IMAirCargoReportDefaultConsigne()
		{
			MiscServ.OM_IMAirCargoReportDefaultConsignee = "ZZZ";
			MiscServ.Validation.ValidateOM_IMAirCargoReportDefaultConsignee();
			AssertHasErrorContaining(MiscServ.OM_IMAirCargoReportDefaultConsigneeInfo, ListValidation.InvalidCodeError);

			MiscServ.OM_IMAirCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Consignee;
			MiscServ.Validation.ValidateOM_IMAirCargoReportDefaultConsignee();
			AssertNoErrorContaining(MiscServ.OM_IMAirCargoReportDefaultConsigneeInfo, ListValidation.InvalidCodeError);

			MiscServ.OM_IMAirCargoReportDefaultConsignee = ZString.Empty;
			MiscServ.Validation.ValidateOM_IMAirCargoReportDefaultConsignee();
			AssertHasErrorContaining(MiscServ.OM_IMAirCargoReportDefaultConsigneeInfo, MandatoryValidation.MustBeEntered);
		}

		#endregion

		#region TestCheckOM_IMSeaCargoReportDefaultConsignee

		public void TestCheckOM_IMSeaCargoReportDefaultConsignee()
		{
			MiscServ.OM_IMSeaCargoReportDefaultConsignee = "ZZZ";
			MiscServ.Validation.ValidateOM_IMSeaCargoReportDefaultConsignee();
			AssertHasErrorContaining(MiscServ.OM_IMSeaCargoReportDefaultConsigneeInfo, ListValidation.InvalidCodeError);

			MiscServ.OM_IMSeaCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Consignee;
			MiscServ.Validation.ValidateOM_IMSeaCargoReportDefaultConsignee();
			AssertNoErrorContaining(MiscServ.OM_IMSeaCargoReportDefaultConsigneeInfo, ListValidation.InvalidCodeError);

			MiscServ.OM_IMSeaCargoReportDefaultConsignee = ZString.Empty;
			MiscServ.Validation.ValidateOM_IMSeaCargoReportDefaultConsignee();
			AssertHasErrorContaining(MiscServ.OM_IMSeaCargoReportDefaultConsigneeInfo, MandatoryValidation.MustBeEntered);
		}

		#endregion

		#region TestCheckOM_RS_NKEXDefaultServiceLevel

		public void TestCheckOM_RS_NKEXDefaultServiceLevel()
		{
			MiscServ.OM_RS_NKEXDefaultServiceLevel = "AAA";
			AssertHasErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKEXDefaultServiceLevel = "A";
			AssertHasErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKEXDefaultServiceLevel = "34";
			AssertHasErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);

			var refServiceLevel = Factory.New<RefServiceLevel>();
			refServiceLevel.RS_Code = "ABB";
			refServiceLevel.RS_IsActive = false;
			MiscServ.OM_RS_NKEXDefaultServiceLevel = "ABB";
			AssertHasErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKEXDefaultServiceLevel = null;
			AssertNoErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);

			refServiceLevel.RS_Code = "AAA";
			refServiceLevel.RS_IsActive = true;
			MiscServ.OM_RS_NKEXDefaultServiceLevel = "AAA";
			AssertNoErrors(MiscServ.OM_RS_NKEXDefaultServiceLevelInfo);
		}

		#endregion

		#region TestCheckOM_RS_NKIMDefaultServiceLevel

		public void TestCheckOM_RS_NKIMDefaultServiceLevel()
		{
			MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";
			AssertHasErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKIMDefaultServiceLevel = "A";
			AssertHasErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKIMDefaultServiceLevel = "34";
			AssertHasErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);

			var refServiceLevel = Factory.New<RefServiceLevel>();
			refServiceLevel.RS_Code = "ABB";
			refServiceLevel.RS_IsActive = false;
			MiscServ.OM_RS_NKIMDefaultServiceLevel = "ABB";
			AssertHasErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);

			MiscServ.OM_RS_NKIMDefaultServiceLevel = null;
			AssertNoErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);

			refServiceLevel.RS_Code = "AAA";
			refServiceLevel.RS_IsActive = true;
			MiscServ.OM_RS_NKIMDefaultServiceLevel = "AAA";
			AssertNoErrors(MiscServ.OM_RS_NKIMDefaultServiceLevelInfo);
		}

		#endregion

		#region TestCheckOM_RX_NKEXDefCurrency

		public void TestOM_RX_NKEXDefCurrency()
		{
			Company.OH_IsConsignor = false;
			MiscServ.OM_RX_NKEXDefCurrency = "///";
			Assert("Company is not Consignor, no notifications", !MiscServ.OM_RX_NKEXDefCurrencyInfo.HasNotifications());

			Company.OH_IsConsignor = true;
			MiscServ.OM_RX_NKEXDefCurrency = "///";
			Assert("Company is Consignor, has errors", MiscServ.OM_RX_NKEXDefCurrencyInfo.HasErrors());

			MiscServ.OM_RX_NKEXDefCurrency = "KRW";
			Assert("Correct currency, no notifications", !MiscServ.OM_RX_NKEXDefCurrencyInfo.HasNotifications());
		}

		#endregion

		#region TestCheckOM_RX_NKFWDefCurrency

		public void TestOM_RX_NKFWDefCurrency()
		{
			Company.OH_IsForwarder = false;
			MiscServ.OM_RX_NKFWDefCurrency = "///";
			Assert("Company is not Forwarder, no notifications", !MiscServ.OM_RX_NKFWDefCurrencyInfo.HasNotifications());

			Company.OH_IsForwarder = true;
			MiscServ.OM_RX_NKFWDefCurrency = "///";
			Assert("Company is Forwarder, has errors", MiscServ.OM_RX_NKFWDefCurrencyInfo.HasErrors());

			MiscServ.OM_RX_NKFWDefCurrency = "KRW";
			Assert("Correct currency, no notifications", !MiscServ.OM_RX_NKFWDefCurrencyInfo.HasNotifications());
		}

		#endregion

		#region TestCheckOM_RN_NKEXDefaultCntryOfOrigin

		public void TestOM_RN_NKEXDefaultCntryOfOrigin()
		{
			Company.OH_IsConsignor = false;
			MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = "//";
			Assert("Company is not Consignor, no notifications", !MiscServ.OM_RN_NKEXDefaultCntryOfOriginInfo.HasNotifications());

			Company.OH_IsConsignor = true;
			MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = "//";
			Assert("Company is Consignor, has errors", MiscServ.OM_RN_NKEXDefaultCntryOfOriginInfo.HasErrors());

			MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = "AU";
			Assert("Correct country, no notifications", !MiscServ.OM_RN_NKEXDefaultCntryOfOriginInfo.HasNotifications());
		}

		#endregion

		#region TestCheckOM_IMAdvanceCargoReportingSelfFiler

		public void TestCheckOM_IMAdvanceCargoReportingSelfFiler()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsConsignee = true;
			org2.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_RN_NKImporterCountry = "AU";
			existingRelationShip.PR_Location = "AUSYD";

			Factory.Save();

			org2.MiscServ.OM_IMAdvanceCargoReportingSelfFiler = false;
			Assert(org2.MiscServ.OM_IMAdvanceCargoReportingSelfFilerInfo.HasChanges);

			org2.MiscServ.Validation.ValidateOM_IMAdvanceCargoReportingSelfFiler();
			AssertHasError(org2.MiscServ.OM_IMAdvanceCargoReportingSelfFilerInfo, @"This organization is currently setup as the Self-Filer for other Organizations. Please verify the related party setup before taking further action.
To proceed, you can remove parties related to this self-filer by navigating to: Organization > Details > Related Parties > Parties Related To This Organization.");
		}

		#endregion

		#region TestOM_FWAdvanceCargoReportingSelfFiler

		public void TestOM_FWAdvanceCargoReportingSelfFiler()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "org1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "org2";
			org2.OH_IsConsignee = true;
			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = true;

			var existingRelationShip = org1.AllRelatedParties.AddNew();
			existingRelationShip.PR_PartyType = RelatedPartyTypeList.Codes.SelfFilerForICS2;
			existingRelationShip.PR_OH_RelatedParty = org2.PK;
			existingRelationShip.PR_RN_NKImporterCountry = "AU";
			existingRelationShip.PR_Location = "AUSYD";

			Factory.Save();

			org2.MiscServ.OM_FWAdvanceCargoReportingSelfFiler = false;
			Assert(org2.MiscServ.OM_FWAdvanceCargoReportingSelfFilerInfo.HasChanges);

			org2.MiscServ.Validation.ValidateOM_FWAdvanceCargoReportingSelfFiler();
			AssertHasError(org2.MiscServ.OM_FWAdvanceCargoReportingSelfFilerInfo, @"This organization is currently setup as the Self-Filer for other Organizations. Please verify the related party setup before taking further action.
To proceed, you can remove parties related to this self-filer by navigating to: Organization > Details > Related Parties > Parties Related To This Organization.");
		}

		#endregion

		#region OH_IsConsignee = true

		#region TestValidateOM_IMDefaultINCOTerm

		public void TestValidateOM_IMDefaultINCOTerm()
		{
			Company.OH_IsConsignee = true;
			MiscServ.OM_IMDefaultINCOTerm = "DAT";
			MiscServ.Validation.ValidateOM_IMDefaultINCOTerm();
			Assert("Company is Consignee, DAT code has warning", MiscServ.OM_IMDefaultINCOTermInfo.HasWarnings());
		}

		#endregion

		#region TestValidateOM_IMMinEFTAmount

		public void TestValidateOM_IMMinEFTAmount()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMMaxEFTAmount = 0;
			MiscServ.OM_IMMinEFTAmount = -5;
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMMinEFTAmountInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMMinEFTAmount = -4;
			Assert("Company is Consignee, OM_IMMinEFTAmount < 0, has errors", MiscServ.OM_IMMinEFTAmountInfo.HasErrors());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMMinEFTAmount = 0;
			Assert("Company is Consignee, OM_IMMinEFTAmount = 0, no errors", !MiscServ.OM_IMMinEFTAmountInfo.HasErrors());

			MiscServ.OM_IMMaxEFTAmount = 0;
			MiscServ.OM_IMMinEFTAmount = 4;
			Assert("Company is Consignee, OM_IMMinEFTAmount > OM_IMMaxEFTAmount, has errors", MiscServ.OM_IMMinEFTAmountInfo.HasErrors());

			MiscServ.OM_IMMaxEFTAmount = 7;
			MiscServ.OM_IMMinEFTAmount = 5;
			Assert("Company is Consignee, OM_IMMinEFTAmount < OM_IMMaxEFTAmount, no errors", !MiscServ.OM_IMMinEFTAmountInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMMaxEFTAmount

		public void TestValidateOM_IMMaxEFTAmount()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMMinEFTAmount = 0;
			MiscServ.OM_IMMaxEFTAmount = -5;
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMMaxEFTAmountInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMMaxEFTAmount = 0;
			Assert("Company is Consignee, OM_IMMaxEFTAmount = 0, no errors", !MiscServ.OM_IMMaxEFTAmountInfo.HasErrors());

			MiscServ.OM_IMMaxEFTAmount = -4;
			Assert("Company is Consignee, OM_IMMaxEFTAmount < 0, has errors", MiscServ.OM_IMMaxEFTAmountInfo.HasErrors());

			MiscServ.OM_IMMinEFTAmount = 6;
			MiscServ.OM_IMMaxEFTAmount = 5;
			Assert("Company is not Consignee, OM_IMMinEFTAmount > OM_IMMaxEFTAmount", MiscServ.OM_IMMaxEFTAmountInfo.HasErrors());

			MiscServ.OM_IMMinEFTAmount = 3;
			MiscServ.OM_IMMaxEFTAmount = 5;
			Assert("Company is Consignee, OM_IMMinEFTAmount < OM_IMMaxEFTAmount, no errors", !MiscServ.OM_IMMaxEFTAmountInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMEstDaysDeliveryAir

		public void TestValidateOM_IMEstDaysDeliveryAir()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMEstDaysDeliveryAir = -5;
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMEstDaysDeliveryAirInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMEstDaysDeliveryAir = -4;
			Assert("Company is Consignee, OOM_IMEstDaysDeliveryAir < 0, has errors", MiscServ.OM_IMEstDaysDeliveryAirInfo.HasErrors());

			MiscServ.OM_IMEstDaysDeliveryAir = 5;
			Assert("Company is Consignee, OM_IMEstDaysDeliveryAir > 0, no errors", !MiscServ.OM_IMEstDaysDeliveryAirInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMEstDaysDeliveryLCL

		public void TestValidateOM_IMEstDaysDeliveryLCL()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMEstDaysDeliveryLCL = -5;
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMEstDaysDeliveryLCLInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMEstDaysDeliveryLCL = -4;
			Assert("Company is Consignee, OOM_IMEstDaysDeliveryLCL < 0, has errors", MiscServ.OM_IMEstDaysDeliveryLCLInfo.HasErrors());

			MiscServ.OM_IMEstDaysDeliveryLCL = 5;
			Assert("Company is Consignee, OM_IMEstDaysDeliveryLCL > 0, no errors", !MiscServ.OM_IMEstDaysDeliveryLCLInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMEstDaysDeliveryFCL

		public void TestValidateOM_IMEstDaysDeliveryFCL()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMEstDaysDeliveryFCL = -5;
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMEstDaysDeliveryFCLInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMEstDaysDeliveryFCL = -4;
			Assert("Company is Consignee, OOM_IMEstDaysDeliveryFCL < 0, has errors", MiscServ.OM_IMEstDaysDeliveryFCLInfo.HasErrors());

			MiscServ.OM_IMEstDaysDeliveryFCL = 5;
			Assert("Company is Consignee, OM_IMEstDaysDeliveryFCL > 0, no errors", !MiscServ.OM_IMEstDaysDeliveryFCLInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMMergeCustomsInvoiceLinesBy

		public void TestValidateOM_IMMergeCustomsInvoiceLinesBy()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "XXX";
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "AAA";
			Assert("Company is Consignee, Error Expected", MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.HasErrors());

			MiscServ.OM_IMMergeCustomsInvoiceLinesBy = OrgConstants.MergeInvoiceLines.Tariff;
			Assert("Company is Consignee, No error", !MiscServ.OM_IMMergeCustomsInvoiceLinesByInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_RH_NKCMMainExportCmdty

		public void TestValidateOM_RH_NKCMMainExportCmdty()
		{
			string invalidCode = "XXX";
			Assert(!MiscServ.CommodityCodes.Any(t => t.RH_Code == invalidCode));

			MiscServ.OM_RH_NKCMMainExportCmdty = invalidCode;
			Assert(MiscServ.OM_RH_NKCMMainExportCmdtyInfo.HasError("Enter a valid Main Export Commodity."));

			MiscServ.OM_RH_NKCMMainExportCmdty = MiscServ.CommodityCodes[0].RH_Code;
			Assert(!MiscServ.OM_RH_NKCMMainExportCmdtyInfo.HasError("Enter a valid Main Export Commodity."));
		}

		#endregion

		#region TestValidateOM_RH_NKCMMainImportCmdty

		public void TestValidateOM_RH_NKCMMainImportCmdty()
		{
			string invalidCode = "XXX";
			Assert(!MiscServ.CommodityCodes.Any(t => t.RH_Code == invalidCode));

			MiscServ.OM_RH_NKCMMainImportCmdty = invalidCode;
			Assert(MiscServ.OM_RH_NKCMMainImportCmdtyInfo.HasError("Enter a valid Main Import Commodity."));

			MiscServ.OM_RH_NKCMMainImportCmdty = MiscServ.CommodityCodes[0].RH_Code;
			Assert(!MiscServ.OM_RH_NKCMMainImportCmdtyInfo.HasError("Enter a valid Main Import Commodity."));
		}

		#endregion

		#region TestValidateOM_IMSendImportDocsTo

		public void TestValidateOM_IMSendImportDocsTo()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMSendImportDocsTo = "XXX";
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMSendImportDocsToInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMSendImportDocsTo = "AAA";
			Assert("Company is Consignee, error expected with invalid code.", MiscServ.OM_IMSendImportDocsToInfo.HasErrors());

			MiscServ.OM_IMSendImportDocsTo = "BTH";
			Assert("Company is Consignee, no error expected with valid code.", !MiscServ.OM_IMSendImportDocsToInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMSendSeaImportDocsTo

		public void TestValidateOM_IMSendSeaImportDocsTo()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMSendSeaImportDocsTo = "XXX";
			Assert("Company is not Consignee, no notifications.", !MiscServ.OM_IMSendSeaImportDocsToInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMSendSeaImportDocsTo = "AAA";
			Assert("Company is Consignee, error expected with invalid code.", MiscServ.OM_IMSendSeaImportDocsToInfo.HasErrors());

			MiscServ.OM_IMSendSeaImportDocsTo = "BTH";
			Assert("Company is Consignee, no error expected with valid code.", !MiscServ.OM_IMSendSeaImportDocsToInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMImporterCategory

		public void TestValidateOM_IMImporterCategory()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMImporterCategory = "XXX";
			Assert("Company is not Consignee, no notifications", !MiscServ.OM_IMImporterCategoryInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMImporterCategory = "AAA";
			Assert("Company is Consignee, Error Expected", MiscServ.OM_IMImporterCategoryInfo.HasErrors());

			MiscServ.OM_IMImporterCategory = "STD";
			Assert("Company is Consignee, No error", !MiscServ.OM_IMImporterCategoryInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_IMAutoPopulateOwnerRefWithOrderNums

		public void TestValidateOM_IMAutoPopulateOwnerRefWithOrderNums()
		{
			Company.OH_IsConsignee = false;
			MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = "XXX";
			Assert("Org is not a Consignee, no notifications should show", !MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo.HasNotifications());

			Company.OH_IsConsignee = true;
			MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = "AAA";
			Assert("Company is Consignee & field has invalid data, Error Expected", MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo.HasErrors());

			MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNums = "DEF";
			Assert("Field has valid data, No error should occur", !MiscServ.OM_IMAutoPopulateOwnerRefWithOrderNumsInfo.HasErrors());
		}

		#endregion

		#endregion

		#region OH_IsConsignor = true

		#region TestValidateOM_EXDefaultIncoTerm

		public void TestValidateOM_EXDefaultIncoTerm_DAT()
		{
			Company.OH_IsConsignor = true;
			MiscServ.OM_EXDefaultIncoTerm = "DAT";
			MiscServ.Validation.ValidateOM_EXDefaultIncoTerm();
			Assert("Company is Consignor, DAT code has warning", MiscServ.OM_EXDefaultIncoTermInfo.HasWarnings());
		}

		public void TestValidateOM_EXDefaultIncoTerm()
		{
			Company.OH_IsConsignor = false;
			MiscServ.OM_EXDefaultIncoTerm = "DDD";
			MiscServ.Validation.ValidateOM_EXDefaultIncoTerm();
			Assert("Company is not Consignor, no notifications", !MiscServ.OM_EXDefaultIncoTermInfo.HasNotifications());

			Company.OH_IsConsignor = true;
			MiscServ.Validation.ValidateOM_EXDefaultIncoTerm();
			Assert("Company is Consignor, invalid code, has errors", MiscServ.OM_EXDefaultIncoTermInfo.HasErrors());

			MiscServ.OM_EXDefaultIncoTerm = "FOB";
			MiscServ.Validation.ValidateOM_EXDefaultIncoTerm();
			Assert("Company is Consignor, valid code, no errors", !MiscServ.OM_EXDefaultIncoTermInfo.HasErrors());
		}

		#endregion

		#endregion

		#region OH_IsSalesLead = true

		#region TestValidateOM_CMOverallClientRelation

		public void TestValidateOM_CMOverallClientRelation()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallClientRelation = 11;
			MiscServ.Validation.ValidateOM_CMOverallClientRelation();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallClientRelationInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallClientRelation = 11;
			MiscServ.Validation.ValidateOM_CMOverallClientRelation();
			Assert("Not SalesLead, value > 10, error expected", MiscServ.OM_CMOverallClientRelationInfo.HasErrors());

			MiscServ.OM_CMOverallClientRelation = 10;
			MiscServ.Validation.ValidateOM_CMOverallClientRelation();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMOverallClientRelationInfo.HasErrors());

			MiscServ.OM_CMOverallClientRelation = 0;
			MiscServ.Validation.ValidateOM_CMOverallClientRelation();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMOverallClientRelationInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMClientsDesireToRemain

		public void TestValidateOM_CMClientsDesireToRemain()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMClientsDesireToRemain = 11;
			MiscServ.Validation.ValidateOM_CMClientsDesireToRemain();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMClientsDesireToRemainInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMClientsDesireToRemain = 11;
			MiscServ.Validation.ValidateOM_CMClientsDesireToRemain();
			Assert("Not SalesLead, value > 10, error expected", MiscServ.OM_CMClientsDesireToRemainInfo.HasErrors());

			MiscServ.OM_CMClientsDesireToRemain = 10;
			MiscServ.Validation.ValidateOM_CMClientsDesireToRemain();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMClientsDesireToRemainInfo.HasErrors());

			MiscServ.OM_CMClientsDesireToRemain = 0;
			MiscServ.Validation.ValidateOM_CMClientsDesireToRemain();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMClientsDesireToRemainInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMEaseClientCanBePoached

		public void TestValidateOM_CMEaseClientCanBePoached()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMEaseClientCanBePoached = 11;
			MiscServ.Validation.ValidateOM_CMEaseClientCanBePoached();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMEaseClientCanBePoachedInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMEaseClientCanBePoached = 11;
			MiscServ.Validation.ValidateOM_CMEaseClientCanBePoached();
			Assert("Not SalesLead, value > 10, error expected", MiscServ.OM_CMEaseClientCanBePoachedInfo.HasErrors());

			MiscServ.OM_CMEaseClientCanBePoached = 10;
			MiscServ.Validation.ValidateOM_CMEaseClientCanBePoached();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMEaseClientCanBePoachedInfo.HasErrors());

			MiscServ.OM_CMEaseClientCanBePoached = 0;
			MiscServ.Validation.ValidateOM_CMEaseClientCanBePoached();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMEaseClientCanBePoachedInfo.HasErrors());
		}

		#endregion

		#region TestValidateVoyageRecyclingPeriodCode

		public void TestValidateVoyageRecyclingPeriodCode()
		{
			MiscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.ThreeMonths;
			AssertNoErrors(MiscServ.VoyageRecyclingPeriodCodeInfo);

			MiscServ.VoyageRecyclingPeriodCode = "XXX";
			AssertHasErrors(MiscServ.VoyageRecyclingPeriodCodeInfo);

			MiscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.Default;
			AssertNoErrors(MiscServ.VoyageRecyclingPeriodCodeInfo);

			MiscServ.VoyageRecyclingPeriodCode = VoyageRecyclingPeriodList.Codes.None;
			AssertNoErrors(MiscServ.VoyageRecyclingPeriodCodeInfo);

			MiscServ.VoyageRecyclingPeriodCode = ZString.Empty;
			AssertHasErrors(MiscServ.VoyageRecyclingPeriodCodeInfo);
		}

		#endregion

		#region TestValidateOM_CMAmountOfElectronicIntegration

		public void TestValidateOM_CMAmountOfElectronicIntegration()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMAmountOfElectronicIntegration = 11;
			MiscServ.Validation.ValidateOM_CMAmountOfElectronicIntegration();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMAmountOfElectronicIntegrationInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMAmountOfElectronicIntegration = 11;
			MiscServ.Validation.ValidateOM_CMAmountOfElectronicIntegration();
			Assert("Not SalesLead, value > 10, error expected", MiscServ.OM_CMAmountOfElectronicIntegrationInfo.HasErrors());

			MiscServ.OM_CMAmountOfElectronicIntegration = 10;
			MiscServ.Validation.ValidateOM_CMAmountOfElectronicIntegration();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMAmountOfElectronicIntegrationInfo.HasErrors());

			MiscServ.OM_CMAmountOfElectronicIntegration = 0;
			MiscServ.Validation.ValidateOM_CMAmountOfElectronicIntegration();
			Assert("Not SalesLead, value between 0 and 10, no error expected", !MiscServ.OM_CMAmountOfElectronicIntegrationInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMWarehouseRevenue

		public void TestValidateOM_CMWarehouseRevenue()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMWarehouseRevenue = -6;
			MiscServ.Validation.ValidateOM_CMWarehouseRevenue();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMWarehouseRevenueInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMWarehouseRevenue = -6;
			MiscServ.Validation.ValidateOM_CMWarehouseRevenue();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMWarehouseRevenueInfo.HasErrors());

			MiscServ.OM_CMWarehouseRevenue = 5;
			MiscServ.Validation.ValidateOM_CMWarehouseRevenue();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMWarehouseRevenueInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMEstimatedProfit

		public void TestValidateOM_CMEstimatedProfit()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMEstimatedProfit = -6;
			MiscServ.Validation.ValidateOM_CMEstimatedProfit();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMEstimatedProfitInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMEstimatedProfit = -6;
			MiscServ.Validation.ValidateOM_CMEstimatedProfit();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMEstimatedProfitInfo.HasErrors());

			MiscServ.OM_CMEstimatedProfit = 5;
			MiscServ.Validation.ValidateOM_CMEstimatedProfit();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMEstimatedProfitInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMAcheivableClientRevenue

		public void TestValidateOM_CMAcheivableClientRevenue()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMAcheivableClientRevenue = -6;
			MiscServ.Validation.ValidateOM_CMAcheivableClientRevenue();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMAcheivableClientRevenueInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMAcheivableClientRevenue = -6;
			MiscServ.Validation.ValidateOM_CMAcheivableClientRevenue();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMAcheivableClientRevenueInfo.HasErrors());

			MiscServ.OM_CMAcheivableClientRevenue = 5;
			MiscServ.Validation.ValidateOM_CMAcheivableClientRevenue();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMAcheivableClientRevenueInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMConsultingRevenue

		public void TestValidateOM_CMConsultingRevenue()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMConsultingRevenue = -6;
			MiscServ.Validation.ValidateOM_CMConsultingRevenue();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMConsultingRevenueInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMConsultingRevenue = -6;
			MiscServ.Validation.ValidateOM_CMConsultingRevenue();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMConsultingRevenueInfo.HasErrors());

			MiscServ.OM_CMConsultingRevenue = 5;
			MiscServ.Validation.ValidateOM_CMConsultingRevenue();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMConsultingRevenueInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMPaidUpCapital

		public void TestValidateOM_CMPaidUpCapital()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMPaidUpCapital = -6;
			MiscServ.Validation.ValidateOM_CMPaidUpCapital();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMPaidUpCapitalInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMPaidUpCapital = -6;
			MiscServ.Validation.ValidateOM_CMPaidUpCapital();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMPaidUpCapitalInfo.HasErrors());

			MiscServ.OM_CMPaidUpCapital = 5;
			MiscServ.Validation.ValidateOM_CMPaidUpCapital();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMPaidUpCapitalInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMNoOfEmployees

		public void TestValidateOM_CMNoOfEmployees()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMNoOfEmployees = -6;
			MiscServ.Validation.ValidateOM_CMNoOfEmployees();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMNoOfEmployeesInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMNoOfEmployees = -6;
			MiscServ.Validation.ValidateOM_CMNoOfEmployees();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMNoOfEmployeesInfo.HasErrors());

			MiscServ.OM_CMNoOfEmployees = 5;
			MiscServ.Validation.ValidateOM_CMNoOfEmployees();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMNoOfEmployeesInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMPercentage

		public void TestValidateOM_CMPercentage()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMPercentage = -6;
			MiscServ.Validation.ValidateOM_CMPercentage();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMPercentageInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMPercentage = -6;
			MiscServ.Validation.ValidateOM_CMPercentage();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMPercentageInfo.HasErrors());

			MiscServ.OM_CMPercentage = 5;
			MiscServ.Validation.ValidateOM_CMPercentage();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMPercentageInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMTotalClientRevenue

		public void TestValidateOM_CMTotalClientRevenue()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMTotalClientRevenue = -6;
			MiscServ.Validation.ValidateOM_CMTotalClientRevenue();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMTotalClientRevenueInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMTotalClientRevenue = -6;
			MiscServ.Validation.ValidateOM_CMTotalClientRevenue();
			Assert("Not SalesLead, invalid value, error expected", MiscServ.OM_CMTotalClientRevenueInfo.HasErrors());

			MiscServ.OM_CMTotalClientRevenue = 5;
			MiscServ.Validation.ValidateOM_CMTotalClientRevenue();
			Assert("Not SalesLead, valid value, no error expected", !MiscServ.OM_CMTotalClientRevenueInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMCompetitorActivity

		public void TestValidateOM_CMCompetitorActivity()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMCompetitorActivity = "XXX";
			MiscServ.Validation.ValidateOM_CMCompetitorActivity();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMCompetitorActivityInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMCompetitorActivity = "XXX";
			MiscServ.Validation.ValidateOM_CMCompetitorActivity();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMCompetitorActivityInfo.HasErrors());

			MiscServ.OM_CMCompetitorActivity = "ALL";
			MiscServ.Validation.ValidateOM_CMCompetitorActivity();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMCompetitorActivityInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMGrowthOutlook

		public void TestValidateOM_CMGrowthOutlook()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMGrowthOutlook = "XXX";
			MiscServ.Validation.ValidateOM_CMGrowthOutlook();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMGrowthOutlookInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMGrowthOutlook = "XXX";
			MiscServ.Validation.ValidateOM_CMGrowthOutlook();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMGrowthOutlookInfo.HasErrors());

			MiscServ.OM_CMGrowthOutlook = "SIN";
			MiscServ.Validation.ValidateOM_CMGrowthOutlook();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMGrowthOutlookInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMClientSize

		public void TestValidateOM_CMClientSize()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMClientSize = "XXX";
			MiscServ.Validation.ValidateOM_CMClientSize();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMClientSizeInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMClientSize = "XXX";
			MiscServ.Validation.ValidateOM_CMClientSize();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMClientSizeInfo.HasErrors());

			MiscServ.OM_CMClientSize = "MED";
			MiscServ.Validation.ValidateOM_CMClientSize();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMClientSizeInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMOverallEffectOfClientOnAirfreightCosts

		public void TestValidateOM_CMOverallEffectOfClientOnAirfreightCosts()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnAirfreightCosts();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnAirfreightCosts();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.HasErrors());

			MiscServ.OM_CMOverallEffectOfClientOnAirfreightCosts = "MIC";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnAirfreightCosts();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMOverallEffectOfClientOnAirfreightCostsInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMOverallEffectOfClientOnLCLCosts

		public void TestValidateOM_CMOverallEffectOfClientOnLCLCosts()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnLCLCosts();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnLCLCosts();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.HasErrors());

			MiscServ.OM_CMOverallEffectOfClientOnLCLCosts = "SDC";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnLCLCosts();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMOverallEffectOfClientOnLCLCostsInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMOverallEffectOfClientOnOtherCosts

		public void TestValidateOM_CMOverallEffectOfClientOnOtherCosts()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnOtherCosts();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnOtherCosts();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.HasErrors());

			MiscServ.OM_CMOverallEffectOfClientOnOtherCosts = "MDC";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnOtherCosts();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMOverallEffectOfClientOnOtherCostsInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMOverallEffectOfClientOnTEUCosts

		public void TestValidateOM_CMOverallEffectOfClientOnTEUCosts()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnTEUCosts();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnTEUCosts();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.HasErrors());

			MiscServ.OM_CMOverallEffectOfClientOnTEUCosts = "LIC";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnTEUCosts();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMOverallEffectOfClientOnTEUCostsInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMOverallEffectOfClientOnWarehousingCosts

		public void TestValidateOM_CMOverallEffectOfClientOnWarehousingCosts()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnWarehousingCosts();
			Assert("Not SalesLead, no notifications", !MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.HasNotifications());

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "XXX";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnWarehousingCosts();
			Assert("Is SalesLead, invalid code, error expected", MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.HasErrors());

			MiscServ.OM_CMOverallEffectOfClientOnWarehousingCosts = "NIL";
			MiscServ.Validation.ValidateOM_CMOverallEffectOfClientOnWarehousingCosts();
			Assert("Is SalesLead, valid code, no error expected", !MiscServ.OM_CMOverallEffectOfClientOnWarehousingCostsInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CMIndustryVertical

		public void TestValidateOM_CMIndustryVertical()
		{
			var industryVerticalTypes = new CodeDescriptionBoolCollection(OrgMiscServSchema.OM_CMIndustryVertical.MaxLength);
			industryVerticalTypes.Add("AAA", (NoResString)"AAA Desc", true);
			industryVerticalTypes.Add("BBB", (NoResString)"BBB Desc", false);
			OrganisationsDataRegistry.Instance.IndustryVerticalTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, industryVerticalTypes);

			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMIndustryVertical = "XXX";
			AssertNoNotifications("Not SalesLead, no notifications", MiscServ.OM_CMIndustryVerticalInfo);

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMIndustryVertical = "XXX";
			AssertListValidationInvalidCodeError(MiscServ.OM_CMIndustryVerticalInfo, true);

			MiscServ.OM_CMIndustryVertical = "AAA";
			AssertNoNotifications(MiscServ.OM_CMIndustryVerticalInfo);

			MiscServ.OM_CMIndustryVertical = "BBB";
			AssertListValidationInvalidCodeError(MiscServ.OM_CMIndustryVerticalInfo, true);

			Factory.Save();
			MiscServ.OM_CMIndustryVertical = "BBB";
			AssertNoErrors(MiscServ.OM_CMIndustryVerticalInfo);
			AssertHasWarning(MiscServ.OM_CMIndustryVerticalInfo, ListValidation.InactiveCodeMessage);
		}

		#endregion

		#region TestValidateOM_CMPeriodOfActivity

		public void TestValidateOM_CMPeriodOfActivity()
		{
			var periodOfActivities = new CodeDescriptionWithEnabledAndDefaultCollection(OrgMiscServSchema.OM_CMPeriodOfActivity.MaxLength);
			periodOfActivities.AddNew("AAA", (NoResString)"AAA Desc", true, true);
			periodOfActivities.AddNew("BBB", (NoResString)"BBB Desc", false, false);
			OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, periodOfActivities);

			Company.OH_IsSalesLead = false;
			MiscServ.OM_CMPeriodOfActivity = "XXX";
			AssertNoNotifications("Not SalesLead, no notifications", MiscServ.OM_CMPeriodOfActivityInfo);

			Company.OH_IsSalesLead = true;
			MiscServ.OM_CMPeriodOfActivity = "XXX";
			AssertListValidationInvalidCodeError(MiscServ.OM_CMPeriodOfActivityInfo, true);

			MiscServ.OM_CMPeriodOfActivity = "AAA";
			AssertNoNotifications(MiscServ.OM_CMPeriodOfActivityInfo);

			MiscServ.OM_CMPeriodOfActivity = "BBB";
			AssertListValidationInvalidCodeError(MiscServ.OM_CMPeriodOfActivityInfo, true);

			Factory.Save();
			MiscServ.OM_CMPeriodOfActivity = "BBB";
			AssertNoErrors(MiscServ.OM_CMPeriodOfActivityInfo);
			AssertHasWarning(MiscServ.OM_CMPeriodOfActivityInfo, ListValidation.InactiveCodeMessage);
		}

		#endregion

		#region TestValidateOM_GC_CMPreferredPaymentCompany

		public void TestValidateOM_GC_CMPreferredPaymentCompany()
		{
			Company.OH_IsSalesLead = false;
			MiscServ.UseTransactionCompanyAsPreferredPayment = false;
			MiscServ.OM_GC_CMPreferredPaymentCompany = ZGuid.Empty;
			MiscServ.Validation.ValidateOM_GC_CMPreferredPaymentCompany();
			AssertMandatoryValidationError(MiscServ.OM_GC_CMPreferredPaymentCompanyInfo, false);

			Company.OH_IsSalesLead = true;
			MiscServ.Validation.ValidateOM_GC_CMPreferredPaymentCompany();
			AssertMandatoryValidationError(MiscServ.OM_GC_CMPreferredPaymentCompanyInfo, true);

			MiscServ.OM_GC_CMPreferredPaymentCompany = GlbCompany.CurrentCompany.PK;
			AssertMandatoryValidationError(MiscServ.OM_GC_CMPreferredPaymentCompanyInfo, false);

			MiscServ.OM_GC_CMPreferredPaymentCompany = ZGuid.Empty;
			AssertMandatoryValidationError(MiscServ.OM_GC_CMPreferredPaymentCompanyInfo, true);

			MiscServ.UseTransactionCompanyAsPreferredPayment = true;
			AssertMandatoryValidationError(MiscServ.OM_GC_CMPreferredPaymentCompanyInfo, false);
		}

		#endregion

		#endregion

		#region OH_IsCompetitor = true

		#region TestValidateOM_CICapitalEmployed

		public void TestValidateOM_CICapitalEmployed()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CICapitalEmployed = -6;
			MiscServ.Validation.ValidateOM_CICapitalEmployed();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CICapitalEmployedInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CICapitalEmployed = -6;
			MiscServ.Validation.ValidateOM_CICapitalEmployed();
			Assert("Not Competitor, invalid value, error expected", MiscServ.OM_CICapitalEmployedInfo.HasErrors());

			MiscServ.OM_CICapitalEmployed = 5;
			MiscServ.Validation.ValidateOM_CICapitalEmployed();
			Assert("Not Competitor, valid value, no error expected", !MiscServ.OM_CICapitalEmployedInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CIProfit

		public void TestValidateOM_CIProfit()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CIProfit = -6;
			MiscServ.Validation.ValidateOM_CIProfit();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CIProfitInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CIProfit = -6;
			MiscServ.Validation.ValidateOM_CIProfit();
			Assert("Not Competitor, invalid value, error expected", MiscServ.OM_CIProfitInfo.HasErrors());

			MiscServ.OM_CIProfit = 5;
			MiscServ.Validation.ValidateOM_CIProfit();
			Assert("Not Competitor, valid value, no error expected", !MiscServ.OM_CIProfitInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CITurnover

		public void TestValidateOM_CITurnover()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CITurnover = -6;
			MiscServ.Validation.ValidateOM_CITurnover();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CITurnoverInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CITurnover = -6;
			MiscServ.Validation.ValidateOM_CITurnover();
			Assert("Not Competitor, invalid value, error expected", MiscServ.OM_CITurnoverInfo.HasErrors());

			MiscServ.OM_CITurnover = 5;
			MiscServ.Validation.ValidateOM_CITurnover();
			Assert("Not Competitor, valid value, no error expected", !MiscServ.OM_CITurnoverInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CIEstimatedStaffThisCountry

		public void TestValidateOM_CIEstimatedStaffThisCountry()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CIEstimatedStaffThisCountry = -6;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisCountry();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CIEstimatedStaffThisCountryInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CIEstimatedStaffThisCountry = -6;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisCountry();
			Assert("Not Competitor, invalid value, error expected", MiscServ.OM_CIEstimatedStaffThisCountryInfo.HasErrors());

			MiscServ.OM_CIEstimatedStaffThisCountry = 5;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisCountry();
			Assert("Not Competitor, valid value, no error expected", !MiscServ.OM_CIEstimatedStaffThisCountryInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CIEstimatedStaffThisLocation

		public void TestValidateOM_CIEstimatedStaffThisLocation()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CIEstimatedStaffThisLocation = -6;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisLocation();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CIEstimatedStaffThisLocationInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CIEstimatedStaffThisLocation = -6;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisLocation();
			Assert("Not Competitor, invalid value, error expected", MiscServ.OM_CIEstimatedStaffThisLocationInfo.HasErrors());

			MiscServ.OM_CIEstimatedStaffThisLocation = 5;
			MiscServ.Validation.ValidateOM_CIEstimatedStaffThisLocation();
			Assert("Not Competitor, valid value, no error expected", !MiscServ.OM_CIEstimatedStaffThisLocationInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CITypeOfService

		public void TestValidateOM_CITypeOfService()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CITypeOfService = "XXX";
			MiscServ.Validation.ValidateOM_CITypeOfService();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CITypeOfServiceInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CITypeOfService = "XXX";
			MiscServ.Validation.ValidateOM_CITypeOfService();
			Assert("Is Competitor, invalid code, error expected", MiscServ.OM_CITypeOfServiceInfo.HasErrors());

			MiscServ.OM_CITypeOfService = "FBW";
			MiscServ.Validation.ValidateOM_CITypeOfService();
			Assert("Is Competitor, valid code, no error expected", !MiscServ.OM_CITypeOfServiceInfo.HasErrors());
		}

		#endregion

		#region TestValidateOM_CISellingStyle

		public void TestValidateOM_CISellingStyle()
		{
			Company.OH_IsCompetitor = false;
			MiscServ.OM_CISellingStyle = "XXX";
			MiscServ.Validation.ValidateOM_CISellingStyle();
			Assert("Not Competitor, no notifications", !MiscServ.OM_CISellingStyleInfo.HasNotifications());

			Company.OH_IsCompetitor = true;
			MiscServ.OM_CISellingStyle = "XXX";
			MiscServ.Validation.ValidateOM_CISellingStyle();
			Assert("Is Competitor, invalid code, error expected", MiscServ.OM_CISellingStyleInfo.HasErrors());

			MiscServ.OM_CISellingStyle = "SPC";
			MiscServ.Validation.ValidateOM_CISellingStyle();
			Assert("Is Competitor, valid code, no error expected", !MiscServ.OM_CISellingStyleInfo.HasErrors());
		}

		#endregion

		#endregion

		#region OH_IsForwarder = true

		#region TestValidateOM_FWIATACode

		public void TestValidateOM_FWIATACode()
		{
			Company.OH_IsForwarder = false;
			MiscServ.OM_FWIATACode = "22334455";
			Assert("Not forwarder, no notifications", !MiscServ.OM_FWIATACodeInfo.HasNotifications());

			Company.OH_IsForwarder = true;
			MiscServ.OM_FWIATACode = "2233445566";
			Assert("Incorrect format, warning expected", MiscServ.OM_FWIATACodeInfo.HasWarnings());

			MiscServ.OM_FWIATACode = "22=3 4455/6677";
			Assert("Incorrect format, warning expected", MiscServ.OM_FWIATACodeInfo.HasWarnings());

			MiscServ.OM_FWIATACode = "22-3/4455/6677";
			Assert("Incorrect format, warning expected", MiscServ.OM_FWIATACodeInfo.HasWarnings());

			MiscServ.OM_FWIATACode = "22-3 4455}6677";
			Assert("Incorrect format, warning expected", MiscServ.OM_FWIATACodeInfo.HasWarnings());

			MiscServ.OM_FWIATACode = "22-3 4455/66AA";
			Assert("Incorrect format, warning expected", MiscServ.OM_FWIATACodeInfo.HasWarnings());

			MiscServ.OM_FWIATACode = "02-3 3002/2122";
			Assert("Correct format, no warning expected", !MiscServ.OM_FWIATACodeInfo.HasWarnings());
		}

		#endregion

		#endregion

		#region OH_IsShippingProvider = true

		#region TestValidateOM_RM_Airline

		public void TestValidateOM_RM_Airline()
		{
			RefAirline airline = Factory.New<RefAirline>();

			Company.OH_IsShippingProvider = false;
			Company.OH_IsAirLine = true;
			MiscServ.OM_RM_Airline = new ZGuid();
			AssertNoErrors("Not ShippingProvider, no notifications", MiscServ.OM_RM_AirlineInfo);

			Company.OH_IsShippingProvider = true;
			Company.OH_IsAirLine = false;
			MiscServ.OM_RM_Airline = new ZGuid();
			AssertNoErrors("Not Airline, no notifications", MiscServ.OM_RM_AirlineInfo);

			Company.OH_IsShippingProvider = true;
			Company.OH_IsAirLine = true;

			MiscServ.OM_RM_Airline = airline.PK;
			AssertNoErrors("OM_RM_Airline exists in RefAirline table, no error expected", MiscServ.OM_RM_AirlineInfo);

			MiscServ.OM_RM_Airline = ZGuid.Empty;
			AssertHasError("OM_RM_Airline not entered, error expected", MiscServ.OM_RM_AirlineInfo, "Please enter a value.");

			MiscServ.OM_RM_Airline = airline.PK;
			Factory.Save();

			OrgHeader company2 = Factory.New<OrgHeader>();
			OrgMiscServ miscServ2 = company2.MiscServ;
			company2.OH_IsShippingProvider = true;
			company2.OH_IsAirLine = true;
			miscServ2.OM_RM_Airline = airline.PK;
			AssertNoErrors("No error, empty related port", miscServ2.OM_RM_AirlineInfo);

			company2.OH_RL_NKClosestPort = "USSFO";
			miscServ2.OM_RM_Airline = ZGuid.Empty;
			miscServ2.OM_RM_Airline = airline.PK;
			AssertHasError(miscServ2.OM_RM_AirlineInfo, (NoResString)string.Format("This master bill prefix has already been used on organization {0}.", "TESTCOLAX"));
			company2.OH_RL_NKClosestPort = "INBOM";
			miscServ2.OM_RM_Airline = ZGuid.Empty;
			miscServ2.OM_RM_Airline = airline.PK;
			AssertNoErrors("No error, different country", miscServ2.OM_RM_AirlineInfo);
		}

		#endregion

		#endregion

		#region OM_WhsDefaultExpiryNotificationPeriodInDays >= 0

		#region TestValidateOM_WhsDefaultExpiryNotificationPeriodInDays

		public void TestValidateOM_WhsDefaultExpiryNotificationPeriodInDays()
		{
			MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 0;
			AssertNoErrors("Period is zero so no errors", MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDaysInfo);

			MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 5;
			AssertNoErrors("Period is positive so no errors", MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDaysInfo);

			MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = -5;
			AssertHasError("Period is negative so should have error", MiscServ.OM_WhsDefaultExpiryNotificationPeriodInDaysInfo, "Please enter a 'Default Expiry Notification Period (Days)' greater than or equal to 0.");
		}

		#endregion

		#endregion
		#region ValidateOM_EXDefaultDGContactPhoneUsed

		public void TestValidateOM_EXDefaultDGContactPhoneUsed()
		{
			MiscServ.OM_OC_EXDefaultDGContact = ZGuid.Empty;
			AssertDGContactPhoneUsed(true);

			MiscServ.OM_OC_EXDefaultDGContact = new ZGuid();
			AssertDGContactPhoneUsed(true);

			OrgContact contact = Company.Contacts.AddNew();
			MiscServ.OM_OC_EXDefaultDGContact = contact.PK;
			AssertDGContactPhoneUsed(false);
		}

		void AssertDGContactPhoneUsed(ZBool expectErrors)
		{
			AssertOM_EXDefaultDGContactPhoneUsed("", !expectErrors, false, !expectErrors, expectErrors);
			AssertOM_EXDefaultDGContactPhoneUsed("XXX", true, true, false, expectErrors);
			AssertOM_EXDefaultDGContactPhoneUsed("HOM", expectErrors, false, false, expectErrors);
			AssertOM_EXDefaultDGContactPhoneUsed("WRK", expectErrors, false, false, expectErrors);
			AssertOM_EXDefaultDGContactPhoneUsed("MOB", expectErrors, false, false, expectErrors);
			AssertOM_EXDefaultDGContactPhoneUsed("OTH", expectErrors, false, false, expectErrors);
		}

		void AssertOM_EXDefaultDGContactPhoneUsed(ZString name, ZBool raiseError, ZBool containsValidTypeSelectionError, ZBool containsEnterValueError, ZBool containsValidDGContactError)
		{
			Company.MiscServ.OM_EXDefaultDGContactPhoneUsed = name;
			if (raiseError)
			{
				if (containsValidDGContactError)
				{ AssertHasError("name should cause error", Company.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo, "Please select a valid Contact."); }
				if (containsValidTypeSelectionError)
				{ AssertHasError("name should cause error", Company.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo, "Enter a valid DG Contact Number."); }
				if (containsEnterValueError)
				{ AssertHasError("name should cause error", Company.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo, "Please enter a DG Contact Number."); }
			}
			else
			{
				AssertNoErrors("name should not cause error", Company.MiscServ.OM_EXDefaultDGContactPhoneUsedInfo);
			}
		}

		#endregion

		#region ValidateOM_CMDistanceCalculationProvider

		public void TestValidateOM_CMDistanceCalculationProvider()
		{
			MiscServ.OM_CMDistanceCalculationProvider = "XXX";
			AssertHasErrors("Wrong code", MiscServ.OM_CMDistanceCalculationProviderInfo);

			MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.DefaultFromRegistry;
			AssertNoErrors("Correct code", MiscServ.OM_CMDistanceCalculationProviderInfo);

			foreach (CodeDescriptionPair pair in DistanceCalculationLists.Instance.Providers)
			{
				MiscServ.OM_CMDistanceCalculationProvider = pair.Code;
				AssertNoErrors("Correct code", MiscServ.OM_CMDistanceCalculationProviderInfo);
			}

			MiscServ.OM_CMDistanceCalculationProvider = "";
			AssertHasErrors("No code", MiscServ.OM_CMDistanceCalculationProviderInfo);
		}

		#endregion

		#region ValidateOM_CMDistanceCalculationVersion

		public void TestValidateOM_CMDistanceCalculationVersion()
		{
			MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
			MiscServ.OM_CMDistanceCalculationVersion = "32";
			AssertNoErrors("Any version can be entered", MiscServ.OM_CMDistanceCalculationVersionInfo);

			MiscServ.OM_CMDistanceCalculationVersion = "XXX";
			AssertNoErrors("Any version can be entered", MiscServ.OM_CMDistanceCalculationVersionInfo);

			MiscServ.OM_CMDistanceCalculationVersion = "";
			AssertHasErrors("Must enter a version", MiscServ.OM_CMDistanceCalculationVersionInfo);
		}

		#endregion

		#region ValidateOM_CMDistanceCalculationMethod

		public void TestValidateOM_CMDistanceCalculationMethod()
		{
			MiscServ.OM_CMDistanceCalculationProvider = DistanceCalculationConstants.Providers.PCMiler;
			MiscServ.OM_CMDistanceCalculationMethod = DistanceCalculationConstants.CalculationMethods.PCMiler.Practical;
			AssertNoErrors("Valid method", MiscServ.OM_CMDistanceCalculationMethodInfo);

			MiscServ.OM_CMDistanceCalculationMethod = "XXX";
			AssertHasErrors("Invalid method", MiscServ.OM_CMDistanceCalculationMethodInfo);

			MiscServ.OM_CMDistanceCalculationMethod = "";
			AssertHasErrors("Must enter a method", MiscServ.OM_CMDistanceCalculationMethodInfo);
		}

		#endregion

		#region ValidateOM_FWAsAgentOption

		public void TestValidateOM_FWAsAgentOption()
		{
			MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgent;
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);

			MiscServ.OM_FWAsAgentOption = "DUM";
			AssertHasError(MiscServ.OM_FWAsAgentOptionInfo, "Enter a valid As Agent Option.");

			MiscServ.OM_FWAsAgentOption = string.Empty;
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);

			MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);

			MiscServ.OM_FWAsAgentName = "Line ABC";
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);

			MiscServ.OM_FWAsAgentOption = string.Empty;
			AssertHasError(MiscServ.OM_FWAsAgentOptionInfo, "As Agent Option must have a value to save As Agent Details. Please select a value in As Agent Option.");

			MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);

			MiscServ.OM_FWAsAgentOption = string.Empty;
			AssertHasError(MiscServ.OM_FWAsAgentOptionInfo, "As Agent Option must have a value to save As Agent Details. Please select a value in As Agent Option.");

			MiscServ.OM_FWAsAgentName = string.Empty;
			AssertNoErrors(MiscServ.OM_FWAsAgentOptionInfo);
		}

		#endregion

		#region ValidateOM_FWAsAgentName

		public void TestValidateOM_FWAsAgentName()
		{
			MiscServ.OM_FWAsAgentName = "ABC";
			MiscServ.OM_FWAsAgentName = string.Empty;
			AssertNoWarnings(MiscServ.OM_FWAsAgentNameInfo);

			MiscServ.OM_FWAsAgentName = "ABC";
			AssertNoWarnings(MiscServ.OM_FWAsAgentNameInfo);

			MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsCarrier;
			AssertNoWarnings(MiscServ.OM_FWAsAgentNameInfo);

			MiscServ.OM_FWAsAgentName = string.Empty;
			AssertHasWarning(MiscServ.OM_FWAsAgentNameInfo, $"Enter As Agent Details value if you would like Shipper Name on MBL, Carrier Booking/SI/etc. to read {MiscServ.Header.OH_FullName} {MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsCarrier)} plus value entered in this field.");

			MiscServ.OM_FWAsAgentName = "GHI";
			AssertNoWarnings(MiscServ.OM_FWAsAgentNameInfo);

			MiscServ.OM_FWAsAgentOption = "DUM";
			MiscServ.OM_FWAsAgentName = string.Empty;
			AssertNoWarnings(MiscServ.OM_FWAsAgentNameInfo);

			MiscServ.OM_FWAsAgentOption = OrgMiscServLookups.AsAgentOption.AsAgentForCarrier;
			AssertHasWarning(MiscServ.OM_FWAsAgentNameInfo, $"Enter As Agent Details value if you would like Shipper Name on MBL, Carrier Booking/SI/etc. to read {MiscServ.Header.OH_FullName} {MiscServ.Lookups.AsAgentOptions.GetDescriptionFromCode(OrgMiscServLookups.AsAgentOption.AsAgentForCarrier)} plus value entered in this field.");
		}

		#endregion

		#region ValidateOM_CMEstablishedDate

		public void TestValidateOM_CMEstablishedDate()
		{
			Company.MiscServ.OM_CMEstablishedDate = new ZDateTime(1950, 02, 02);
			AssertEquals("Date of Establishment should not have errors, even though it is 10 years outside of the current date.",
				false, Company.MiscServ.OM_CMEstablishedDateInfo.HasErrors());

			Company.MiscServ.OM_CMEstablishedDate = ZDateTime.Today.AddDays(1);
			AssertEquals("Date of Establishment should have errors, because it cannot be set to future.",
				true, Company.MiscServ.OM_CMEstablishedDateInfo.HasErrors());

			Company.MiscServ.OM_CMEstablishedDate = ZDateTime.Invalid;
			AssertEquals("Date of Establishment should have errors", true, Company.MiscServ.OM_CMEstablishedDateInfo.HasErrors());
		}

		#endregion

		#region ValidateOM_CarrierPackageGrouping

		public void TestValidateOM_CarrierPackageGrouping()
		{
			MiscServ.OM_CarrierPackageGrouping = ZString.Empty;
			AssertHasError(MiscServ.OM_CarrierPackageGroupingInfo, "Please enter a value.");

			MiscServ.OM_CarrierPackageGrouping = "ERR";
			AssertHasError(MiscServ.OM_CarrierPackageGroupingInfo, "Enter a valid selection.");

			MiscServ.OM_CarrierPackageGrouping = CarrierPackageGroupingList.Codes.SHP;
			AssertNoErrors(MiscServ.OM_CarrierPackageGroupingInfo);
		}

		#endregion

		#region ValidateOM_FWAgentPackageGrouping

		public void TestValidateOM_FWAgentPackageGrouping()
		{
			MiscServ.OM_FWAgentPackageGrouping = ZString.Empty;
			AssertHasError(MiscServ.OM_FWAgentPackageGroupingInfo, "Please enter a value.");

			MiscServ.OM_FWAgentPackageGrouping = "ERR";
			AssertHasError(MiscServ.OM_FWAgentPackageGroupingInfo, "Enter a valid selection.");

			MiscServ.OM_FWAgentPackageGrouping = FWAgentPackageGroupingList.Codes.SHP;
			AssertNoErrors(MiscServ.OM_FWAgentPackageGroupingInfo);
		}

		#endregion

		#region TestCheckOM_ARGlobalOnCreditHold

		[TestDate]
		public void TestCheckOM_ARGlobalOnCreditHold()
		{
			Company.CompanyData.OB_IsDebtor = true;
			AssertEquals("Precondition", false, MiscServ.OM_ARGlobalOnCreditHold);
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "Adam BC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			authorizingUser.GS_Code = "ABC";
			Factory.Save();

			var expectedErrorMessage = "You do not have the appropriate security rights to un-tick the 'Global AR on Credit Hold' check box. This is because the credit on hold has been set by Adam Brian Carlson (ABC) without any approval right and so only the same user or user with approval level 3 can un-tick it.";
			SetupAndAssertARGlobalOnCreditHold(authorizingUser, 0, 1, true, expectedErrorMessage);

			expectedErrorMessage = string.Empty;
			SetupAndAssertARGlobalOnCreditHold(authorizingUser, 1, 2, false, expectedErrorMessage);

			expectedErrorMessage = "You do not have the appropriate security rights to un-tick the 'Global AR on Credit Hold' check box. This is because the credit on hold has been approved by Adam Brian Carlson (ABC) with higher approval right.";
			SetupAndAssertARGlobalOnCreditHold(authorizingUser, 3, 2, true, expectedErrorMessage);
		}

		public void TestCurrentUser_WithoutRight_SetARGlobalOnCreditHold()
		{
			Company.CompanyData.OB_IsDebtor = true;
			var authorizingUser = Factory.NewWithValidTestData<GlbStaff>();
			authorizingUser.GS_LoginName = "Adam BC";
			authorizingUser.GS_FullName = "Adam Brian Carlson";
			authorizingUser.GS_Code = "ABC";
			Factory.Save();
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldFirstLevel, false);
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldSecondLevel, false);
			OrgCompanyDataTest.SetSecurityForUser(authorizingUser.PK, Env.Security.OrgCreditOnHoldThirdLevel, false);

			var expectedWarningForCurrentUser =
				@"You ticked 'Global AR on Credit Hold' check box without any approval right.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.";

			var expectedWarningForNonCurrentUser =
				@"User Adam Brian Carlson (ABC) without any approval right ticked 'Global AR on Credit Hold' check box.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.";

			using (Env.SetTemporaryUserContext(authorizingUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition", false, MiscServ.OM_ARGlobalOnCreditHold);
				MiscServ.OM_ARGlobalOnCreditHold = true;
				AssertHasWarning(MiscServ.OM_ARGlobalOnCreditHoldInfo, expectedWarningForCurrentUser);
				Factory.Save();
			}

			MiscServ.OM_ARGlobalOnCreditHold = false;
			AssertNoWarnings(MiscServ.OM_ARGlobalOnCreditHoldInfo);
			MiscServ.OM_ARGlobalOnCreditHold = true;
			AssertHasWarning(MiscServ.OM_ARGlobalOnCreditHoldInfo, expectedWarningForNonCurrentUser);
		}

		void SetupAndAssertARGlobalOnCreditHold(GlbStaff authorizingUser, int authorizingUserLevel, int currentUserLevel, bool expectedHasError, string expectedErrorMessage)
		{
			OrgCompanyDataTest.SetCreditOnHoldViaUserLevel(MiscServ, authorizingUser, authorizingUserLevel, true);

			Env.Security.OrgCreditOnHoldFirstLevel.IsAllowed = currentUserLevel == 1;
			Env.Security.OrgCreditOnHoldSecondLevel.IsAllowed = currentUserLevel == 2;
			Env.Security.OrgCreditOnHoldThirdLevel.IsAllowed = currentUserLevel == 3;

			MiscServ.OM_ARGlobalOnCreditHold = false;

			if (expectedHasError)
			{
				AssertHasErrors(MiscServ.OM_ARGlobalOnCreditHoldInfo);
				AssertHasErrorContaining(MiscServ.OM_ARGlobalOnCreditHoldInfo, expectedErrorMessage);
			}
			else
			{
				AssertNoErrors(MiscServ.OM_ARGlobalOnCreditHoldInfo);
			}

			MiscServ.OM_ARGlobalOnCreditHold = true;
		}
		#endregion

		#region ContainerFillingPercentage

		public void TestMinimumContainerFillingPercentageCannotBeNegative()
		{
			MiscServ.OM_ORDMinimumContainerFillingPercentage = -1.5m;

			AssertHasError(
				MiscServ.OM_ORDMinimumContainerFillingPercentageInfo,
				"Please enter a valid minimum container filling percentage. Value should be from 0-100."
			);
		}

		public void TestMinimumContainerFillingPercentageCannotBeGreater100()
		{
			MiscServ.OM_ORDMinimumContainerFillingPercentage = 105m;

			AssertHasError(
				MiscServ.OM_ORDMinimumContainerFillingPercentageInfo,
				"Please enter a valid minimum container filling percentage. Value should be from 0-100."
			);
		}

		public void TestMaximumContainerFillingPercentageCannotBeNegative()
		{
			MiscServ.OM_ORDMaximumContainerFillingPercentage = -1.5m;

			AssertHasError(
				MiscServ.OM_ORDMaximumContainerFillingPercentageInfo,
				"Please enter a valid maximum container filling percentage. Value should be from 0-100."
			);
		}

		public void TestMaximumContainerFillingPercentageCannotBeGreater100()
		{
			MiscServ.OM_ORDMaximumContainerFillingPercentage = 105m;

			AssertHasError(
				MiscServ.OM_ORDMaximumContainerFillingPercentageInfo,
				"Please enter a valid maximum container filling percentage. Value should be from 0-100."
			);
		}

		public void TestMaximumContainerFillingPercentageMustBeGreaterThanMinimumContainerFillingPercentage()
		{
			MiscServ.OM_ORDMinimumContainerFillingPercentage = 10m;
			MiscServ.OM_ORDMaximumContainerFillingPercentage = 100m;

			Assert("Pre-condition - maximum is higher than minimum, should have no errors", !MiscServ.HasErrors);

			MiscServ.OM_ORDMinimumContainerFillingPercentage = 50m;
			MiscServ.OM_ORDMaximumContainerFillingPercentage = 40m;

			AssertHasError(
				MiscServ.OM_ORDMaximumContainerFillingPercentageInfo,
				$"Please enter a valid maximum container filling percentage. Value should be strictly greater than the minimum container filling percentage ({MiscServ.OM_ORDMinimumContainerFillingPercentage}%)."
			);
		}

		#endregion

		public void TestCheckOM_ARGlobalCreditLimit()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TestOrg";
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;
			Factory.Save();

			org.MiscServ.OM_ARGlobalCreditLimit = -10m;
			AssertHasErrorContaining(org.MiscServ.OM_ARGlobalCreditLimitInfo, "The Global Credit Limit must be greater than zero.");

			org.CompanyData.OB_ARCreditLimit = 300m;
			org.MiscServ.OM_ARGlobalCreditLimit = 300m;
			AssertNoErrors(org.MiscServ.OM_ARGlobalCreditLimitInfo);
			org.MiscServ.OM_ARGlobalCreditLimit = 299m;
			AssertHasErrorContaining(org.MiscServ.OM_ARGlobalCreditLimitInfo, "The organization TestOrg Global Credit Limit must be greater than the sum of all Local Credit Limit for this organization.");

			org.MiscServ.OM_ARGlobalCreditApproved = false;
			org.MiscServ.OM_ARGlobalCreditLimit = 298m;
			AssertNoErrors(org.MiscServ.OM_ARGlobalCreditLimitInfo);
		}

		public void TestCheckOM_OH_ARGlobalCreditGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;
			org.MiscServ.OM_ARGlobalCreditLimit = 500m;
			org.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			org.MiscServ.OM_ARGlobalCreditApproved = true;

			var groupMiscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			groupMiscServ.Header.CompanyData.OB_IsDebtor = false;
			Factory.Save();

			org.MiscServ.OM_OH_ARGlobalCreditGroup = org.PK;
			AssertHasErrorContaining(org.MiscServ.OM_OH_ARGlobalCreditGroupInfo, "If this organization is the Global Credit Group, please do not specify a value.");

			org.MiscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertHasErrorContaining(org.MiscServ.OM_OH_ARGlobalCreditGroupInfo, "The selected organization is not a receivable organization.");

			org.MiscServ.OM_OH_ARGlobalCreditGroup = ZGuid.Empty;
			groupMiscServ.Header.CompanyData.OB_IsDebtor = true;
			groupMiscServ.OM_OH_ARGlobalCreditGroup = Factory.NewWithValidTestData<OrgMiscServ>().PK;
			org.MiscServ.OM_OH_ARGlobalCreditGroup = groupMiscServ.OM_OH;
			AssertHasErrorContaining(org.MiscServ.OM_OH_ARGlobalCreditGroupInfo, "The current organization is a Global Credit Group. Please leave this value as empty.");
		}

		[TestDate(2020, 12, 18)]
		public void TestValidateOM_CIFinancialDetailsApplicableFromDate()
		{
			var applicableDateErrorMessage = "Applicable From Date should be greater than twelve months old.";

			MiscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Invalid;
			MiscServ.Validation.ValidateOM_CIFinancialDetailsApplicableFromDate();
			AssertHasError(MiscServ.OM_CIFinancialDetailsApplicableFromDateInfo, "Enter a valid Applicable From Date.");

			MiscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Today.AddMonths(-1);
			MiscServ.Validation.ValidateOM_CIFinancialDetailsApplicableFromDate();
			AssertHasError(MiscServ.OM_CIFinancialDetailsApplicableFromDateInfo, applicableDateErrorMessage);

			MiscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Today.AddYears(-1).AddDays(1);
			MiscServ.Validation.ValidateOM_CIFinancialDetailsApplicableFromDate();
			AssertNoErrors(MiscServ.OM_CIFinancialDetailsApplicableFromDateInfo);

			MiscServ.OM_CIFinancialDetailsApplicableFromDate = ZDate.Today.AddYears(-1).AddDays(2);
			MiscServ.Validation.ValidateOM_CIFinancialDetailsApplicableFromDate();
			AssertHasError(MiscServ.OM_CIFinancialDetailsApplicableFromDateInfo, applicableDateErrorMessage);
		}

		#region Implementation

		protected OrgHeader Company;
		protected OrgMiscServ MiscServ;

		protected override void SetUp()
		{
			base.SetUp();
			Company = Factory.New<OrgHeader>();
			MiscServ = Company.MiscServ;

			Company.OH_RL_NKClosestPort = "USLAX";
			Company.OH_FullName = "TestCompany";
			Company.MainAddress.OA_Address1 = "TestAddress";
		}

		#endregion
	}
}
