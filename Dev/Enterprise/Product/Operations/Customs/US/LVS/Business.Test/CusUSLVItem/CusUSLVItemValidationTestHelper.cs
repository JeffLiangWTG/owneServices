using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVItemValidationTestHelper : TestCaseWithFactory
	{
		internal void CheckProductCode_WarnIfSupplierOrImporterAreNotEntered(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;
			var seller = factory.NewWithValidTestData<OrgHeader>();
			var importer = factory.NewWithValidTestData<OrgHeader>();

			item.PartSyncManager.Enabled = false;
			propertyInfo.SetValueFromString("IHOPENOBODYADDSTHIS");
			AssertNoWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter);

			item.PartSyncManager.Enabled = true;
			propertyInfo.BizObj.RunPreSaveValidation();
			AssertHasWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter);

			item.Consignment.SellerOrgPK = seller.PK;
			item.Consignment.ConsigneeOrgPK = importer.PK;
			propertyInfo.SetValueFromString("ORTHISONEEITHER");
			AssertNoWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter);
		}

		internal void CheckProductCode_WarnWhenFoundButNotRelated(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;
			const string TestValidPartNo = "T44330";

			using (CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var seller = factory.NewWithValidTestData<OrgHeader>();
				var importer = factory.NewWithValidTestData<OrgHeader>();

				item.Consignment.SellerOrgPK = seller.PK;
				item.Consignment.ConsigneeOrgPK = importer.PK;

				var otherImporter1 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller1 = factory.NewWithValidTestData<OrgHeader>();
				var otherImporter2 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller2 = factory.NewWithValidTestData<OrgHeader>();

				var validPart1 = factory.NewWithValidTestData<OrgSupplierPart>();
				validPart1.OP_PartNum = TestValidPartNo;
				validPart1.RelatedOrganisations.AddOrganisationIfNotExist(otherSeller1.PK, OrgPartRelation.RelationshipTypes.Supplier);
				validPart1.RelatedOrganisations.AddOrganisationIfNotExist(otherImporter1.PK, OrgPartRelation.RelationshipTypes.Owner);

				propertyInfo.SetValueFromString(TestValidPartNo);

				propertyInfo.BizObj.RunPreSaveValidation();
				AssertHasWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);

				var validPart2 = factory.NewWithValidTestData<OrgSupplierPart>();
				validPart2.OP_PartNum = TestValidPartNo;
				validPart2.RelatedOrganisations.AddOrganisationIfNotExist(otherSeller2.PK, OrgPartRelation.RelationshipTypes.Supplier);
				validPart2.RelatedOrganisations.AddOrganisationIfNotExist(otherImporter2.PK, OrgPartRelation.RelationshipTypes.Owner);

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertHasWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);

				var validPart3 = factory.NewWithValidTestData<OrgSupplierPart>();
				validPart3.OP_PartNum = TestValidPartNo;
				validPart3.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
				validPart3.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);
			}
		}

		internal void CheckProductCode_WarnWhenPartFoundButInactive(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;

			const string TestValidPartNo = "T44330";

			using (CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var seller = factory.NewWithValidTestData<OrgHeader>();
				var importer = factory.NewWithValidTestData<OrgHeader>();

				item.Consignment.SellerOrgPK = seller.PK;
				item.Consignment.ConsigneeOrgPK = importer.PK;

				var otherImporter1 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller1 = factory.NewWithValidTestData<OrgHeader>();
				var otherImporter2 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller2 = factory.NewWithValidTestData<OrgHeader>();

				var validPart = factory.NewWithValidTestData<OrgSupplierPart>();
				validPart.OP_PartNum = TestValidPartNo;
				validPart.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
				validPart.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				propertyInfo.SetValueFromString(TestValidPartNo);

				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);

				validPart.OP_IsActive = false;

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertHasWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);

				var otherPart = factory.NewWithValidTestData<OrgSupplierPart>();
				otherPart.OP_IsActive = false;
				otherPart.OP_PartNum = TestValidPartNo;
				otherPart.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
				otherPart.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertHasWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);

				validPart.OP_IsActive = true;

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodesFoundButNotRelatedToSellerConsigneeCombination);
			}
		}

		internal void CheckProductCode_WarnWhenFoundButNotRelated_WithComplexRelationships(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;
			const string TestValidPartNo = "T44330";

			using (CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var seller = factory.NewWithValidTestData<OrgHeader>();
				var importer = factory.NewWithValidTestData<OrgHeader>();

				item.Consignment.SellerOrgPK = seller.PK;
				item.Consignment.ConsigneeOrgPK = importer.PK;

				var otherImporter1 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller1 = factory.NewWithValidTestData<OrgHeader>();
				var otherImporter2 = factory.NewWithValidTestData<OrgHeader>();
				var otherSeller2 = factory.NewWithValidTestData<OrgHeader>();

				var validPart1 = factory.NewWithValidTestData<OrgSupplierPart>();
				validPart1.OP_PartNum = TestValidPartNo;
				validPart1.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
				validPart1.RelatedOrganisations.AddOrganisationIfNotExist(otherSeller1.PK, OrgPartRelation.RelationshipTypes.Both);
				validPart1.RelatedOrganisations.AddOrganisationIfNotExist(otherSeller2.PK, OrgPartRelation.RelationshipTypes.Owner);

				var validOwner = validPart1.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

				propertyInfo.SetValueFromString(TestValidPartNo);
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);

				validOwner.Delete();

				item.PartSyncManager.Refresh();
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertHasWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);

				item.PartSyncManager.Enabled = false;
				propertyInfo.BizObj.RunPreSaveValidation();
				AssertNoWarning(propertyInfo, CusUSLVItemValidationHelper.WarningPartCodeFoundButNotRelatedToSellerConsigneeCombination);
			}
		}

		internal void CheckProductCode_WarnPartCodeNotFoundAtAll(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;

			var seller = factory.NewWithValidTestData<OrgHeader>();
			var importer = factory.NewWithValidTestData<OrgHeader>();

			const string TestValidPartNo = "T44330";
			factory.Save();

			item.Consignment.SellerOrgPK = seller.PK;
			item.Consignment.ConsigneeOrgPK = importer.PK;

			propertyInfo.SetValueFromString(TestValidPartNo);
			item.PartSyncManager.Enabled = false;
			propertyInfo.BizObj.RunPreSaveValidation();
			AssertNoWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCodeNotFoundAtAll);

			item.PartSyncManager.Enabled = true;
			propertyInfo.BizObj.RunPreSaveValidation();
			AssertHasWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCodeNotFoundAtAll);

			var validPart = factory.NewWithValidTestData<OrgSupplierPart>();
			validPart.OP_PartNum = TestValidPartNo;
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(seller.PK, OrgPartRelation.RelationshipTypes.Supplier);
			validPart.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			item.PartSyncManager.Refresh();
			propertyInfo.BizObj.RunPreSaveValidation();
			AssertNoWarning(propertyInfo, InvoiceLineProductValidationHelper.Warnings.PartCodeNotFoundAtAll);
		}

		internal void CheckProductCode_WarnIfMoreThanOneMatchingPart(CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var factory = item.Factory;

			const string TestValidPartNo = "T44330";

			var seller = factory.NewWithValidTestData<OrgHeader>();
			var importer = factory.NewWithValidTestData<OrgHeader>();

			item.Consignment.SellerOrgPK = seller.PK;
			item.Consignment.ConsigneeOrgPK = importer.PK;

			var otherSupplier1 = factory.NewWithValidTestData<OrgHeader>();
			var otherSupplier2 = factory.NewWithValidTestData<OrgHeader>();

			var testValidPart = factory.NewWithValidTestData<OrgSupplierPart>();
			testValidPart.OP_PartNum = TestValidPartNo;
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(otherSupplier1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var testValidPart2 = factory.New<OrgSupplierPart>();
			testValidPart2.OP_PartNum = TestValidPartNo;
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(otherSupplier2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			testValidPart2.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			using (CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				propertyInfo.SetValueFromString(TestValidPartNo);
				AssertEquals("Item.PartSyncManager.TotalMatchCount", 2, item.PartSyncManager.TotalMatchCount);
				AssertHasWarningContaining(propertyInfo, InvoiceLineProductValidationHelper.Warnings.MoreThanOneProductMatchFound);
			}

			using (CustomsDataRegistry.Instance.EnableExactMatchForProduct.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				item.ULI_PartNo = string.Empty;
				propertyInfo.SetValueFromString(TestValidPartNo);
				AssertEquals("Item.PartSyncManager.TotalMatchCount", 0, item.PartSyncManager.TotalMatchCount);
				AssertNoWarningContaining(propertyInfo, InvoiceLineProductValidationHelper.Warnings.MoreThanOneProductMatchFound);
			}
		}

		internal void CheckLineValue(BusinessObjectFactory factory, CusUSLVClearance clearance, CusUSLVItem item, ZPropertyInfo propertyInfo)
		{
			var cnyCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "CNY");
			cnyCurrency.SetCustomsRate(ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), 3m);

			var eurCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "EUR");
			eurCurrency.SetCustomsRate(ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1), 0.5m);

			clearance.ULH_DepartureDate = ZDate.Today;

			propertyInfo.BizObj.RunPreSaveValidation();
			AssertHasMessageError(propertyInfo, "Line price is mandatory for entry type '86'.");

			propertyInfo.SetValueFromString("-1");
			AssertHasError(propertyInfo, "Line Value cannot be negative.");

			item.ULI_RX_NKCurrency = "USD";

			propertyInfo.SetValueFromString("0.0");
			AssertHasMessageError(propertyInfo, "Line price is mandatory for entry type '86'.");

			propertyInfo.SetValueFromString("0.3");
			AssertHasWarningContaining(propertyInfo, "Line value will be rounded to $1 USD.");

			propertyInfo.SetValueFromString("0.6");
			AssertNoWarning(propertyInfo, "Line value will be rounded to $1 USD.");

			propertyInfo.SetValueFromString("1.2");
			AssertNoWarning(propertyInfo, "Line value will be rounded to $1 USD.");

			item.ULI_RX_NKCurrency = "CNY";

			propertyInfo.SetValueFromString("0.0");
			AssertEquals("Actual Customs Value", 0m, item.ULI_GoodsValueInUSD);
			AssertHasMessageError(propertyInfo, "Line price is mandatory for entry type '86'.");

			propertyInfo.SetValueFromString("0.3");
			AssertEquals("Actual Customs Value", 0.1m, item.ULI_GoodsValueInUSD);
			AssertHasWarningContaining(propertyInfo, "Line value will be rounded to $1 USD.");

			propertyInfo.SetValueFromString("0.6");
			AssertEquals("Actual Customs Value", 0.2m, item.ULI_GoodsValueInUSD);
			AssertHasWarning(propertyInfo, "Line value will be rounded to $1 USD.");

			item.ULI_RX_NKCurrency = "EUR";

			propertyInfo.SetValueFromString("0.0");
			AssertEquals("Actual Customs Value", 0m, item.ULI_GoodsValueInUSD);
			AssertHasMessageError(propertyInfo, "Line price is mandatory for entry type '86'.");

			propertyInfo.SetValueFromString("0.3");
			AssertEquals("Actual Customs Value", 0.6m, item.ULI_GoodsValueInUSD);
			AssertNoWarning(propertyInfo, "Line value will be rounded to $1 USD.");

			propertyInfo.SetValueFromString("0.6");
			AssertEquals("Actual Customs Value", 1.2m, item.ULI_GoodsValueInUSD);
			AssertNoWarning(propertyInfo, "Line value will be rounded to $1 USD.");
		}

		internal void CheckTariff(ZPropertyInfo propertyInfo)
		{
			propertyInfo.SetValueFromString("1234");
			AssertHasMessageError(propertyInfo, "Tariff unable to be found.");

			propertyInfo.SetValueFromString(string.Empty);
			AssertHasMessageError(propertyInfo, "Tariff may not be empty.");

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2930904310";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_DutyComputationCode = "7";
			tariff.UE_Column1RateAdValorem = 0.065m;

			propertyInfo.SetValueFromString("2930904310");
			AssertEquals("Has no errors", false, propertyInfo.HasErrors());
		}

		internal void CheckCountryOfOrigin(ZPropertyInfo propertyInfo)
		{
			AssertNullOrEmpty("pre-condition", propertyInfo.Value.ToString());
			propertyInfo.BizObj.RunPreSaveValidation();
			AssertHasMessageError(propertyInfo, "You have not entered a Country/Region of Origin.");

			propertyInfo.SetValueFromString("@@");
			AssertHasMessageError(propertyInfo, "The code you have selected is not in the list.");
		}

		internal void CheckCurrency(ZPropertyInfo propertyInfo)
		{
			propertyInfo.SetValueFromString("KKK");
			AssertEquals("The code you have selected is not in the list.", propertyInfo.Notifications.First().Message);

			propertyInfo.SetValueFromString(string.Empty);
			AssertEquals("You have not entered a " + propertyInfo.Description + ".", propertyInfo.Notifications.First().Message);

			propertyInfo.SetValueFromString("USD");
			AssertEquals("Has no errors", false, propertyInfo.HasMessageErrors());
		}

		internal void CheckGoodsDescription(CusUSLVItem cusUSLVItem, ZPropertyInfo propertyInfo)
		{
			Assert("precondition", !(cusUSLVItem.CusUSLVItemPGAs.Count > 0));
			propertyInfo.SetValueFromString(string.Empty);
			AssertNoMessageError(propertyInfo, MandatoryValidation.YouHaveNotEntered);

			cusUSLVItem.CusUSLVItemPGAs.AddNew();
			Assert("precondition", cusUSLVItem.CusUSLVItemPGAs.Count > 0);
			propertyInfo.SetValueFromString(string.Empty);
			AssertHasMessageErrorContaining(propertyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		internal void CheckAntiDumping(ZPropertyInfo propertyInfo, CusUSLVItem cusUSLVItem)
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var otherTariff = Factory.New<USCTariff>();
			otherTariff.UE_Tariff = "1111111111";
			otherTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			otherTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			cusUSLVItem.ULI_RN_NKCountryOfOrigin = "KR";
			cusUSLVItem.ULI_Tariff = importTariff.UE_Tariff;

			propertyInfo.SetValueFromString("n");
			AssertHasMessageErrorContaining(propertyInfo, "may be subject to ADD");

			propertyInfo.SetValueFromString("y");
			AssertNoMessageErrors(propertyInfo);

			cusUSLVItem.ULI_Tariff = otherTariff.UE_Tariff;
			propertyInfo.SetValueFromString("n");
			AssertNoMessageErrors(propertyInfo);
		}

		internal void CheckCountervailing(ZPropertyInfo propertyInfo, CusUSLVItem cusUSLVItem)
		{
			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "C9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var otherTariff = Factory.New<USCTariff>();
			otherTariff.UE_Tariff = "1111111111";
			otherTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			otherTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			cusUSLVItem.ULI_RN_NKCountryOfOrigin = "KR";
			cusUSLVItem.ULI_Tariff = importTariff.UE_Tariff;

			propertyInfo.SetValueFromString("N");
			AssertHasMessageErrorContaining(propertyInfo, "may be subject to CVD");

			propertyInfo.SetValueFromString("Y");
			AssertNoMessageErrors(propertyInfo);

			cusUSLVItem.ULI_Tariff = otherTariff.UE_Tariff;
			propertyInfo.SetValueFromString("N");
			AssertNoMessageErrors(propertyInfo);
		}
	}
}
