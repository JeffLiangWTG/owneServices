using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Business.Utilities.Testing
{
	sealed class UCRHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 6, 01)]
		public void TestGetLastCharOfYear1()
		{
			AssertEquals("7", ucrHelper.CalculateUCR(cei).SubstringSafe(0, 1));
		}

		public void TestGetCountryCode()
		{
			AssertEquals(Core.Constants.CountryCodes.SouthAfrica, ucrHelper.CalculateUCR(cei).SubstringSafe(1, 2));
		}

		public void TestGetEntityCodeDecision_IDPassportNumber()
		{
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.IDPassportNumber, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type IDO or PAS between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.IDPassportNumber, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.IDPassportNumber, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type IDO or PAS between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.IDPassportNumber, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.IDPassportNumber, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
		}

		public void TestGetEntityCodeDecision_CompanyRegistrationNumber()
		{
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.GovBusinessCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CompanyRegistrationNumber, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type GBR between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.GovBusinessCode, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CompanyRegistrationNumber, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CompanyRegistrationNumber, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
		}

		public void TestGetEntityCodeDecision_TaxCode()
		{
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type VAT or GTX between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TaxFileCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type VAT or GTX between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_OH_Supplier = supplier.PK;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type VAT or GTX between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TaxFileCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type VAT or GTX between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.TaxCode, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
		}

		public void TestGetEntityCodeDecision_CustomsCode()
		{
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_GoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type CSC between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_OH_Supplier = supplier.PK;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "12345", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals("The Main Supplier must have a ZA Registration Code of type CSC between 8 and 13 characters inclusive.", ucrHelper.DecisionReason);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "1234567890", Core.Constants.CountryCodes.SouthAfrica);
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals(ZString.Empty, ucrHelper.DecisionReason);
			dec.JE_OH_Supplier = ZGuid.Empty;
			ucrHelper.GetEntityCode(EntityTypeList.Codes.CustomsCode, dec);
			AssertEquals("The Declaration must have a valid Main Supplier to use this Entity Type.", ucrHelper.DecisionReason);
		}

		public void TestGetEntityCode()
		{
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_GoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;
			cei.CEI_EntityType = EntityTypeList.Codes.IDPassportNumber;
			var idCusCode = supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.IDPassportNumber)", "12345678");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "23456789", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.IDPassportNumber)", "12345678");
			supplier.CustomsCodes.RemoveAndDelete(idCusCode);
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.IDPassportNumber)", "23456789");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.IDPassportNumber)", "12345678");
			cei.CEI_EntityType = EntityTypeList.Codes.CompanyRegistrationNumber;
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.CompanyRegistrationNumber)", ZString.Empty);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.GovBusinessCode, "34567890", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(cusEntryInstruction.CEI_EntityType == EntityTypeList.Codes.CompanyRegistrationNumber)", "34567890");
			cei.CEI_EntityType = EntityTypeList.Codes.TaxCode;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", ZString.Empty);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "45678901", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", "45678901");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TaxFileCode, "55678901", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", "55678901");
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TaxFileCode, "", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export)", ZString.Empty);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "56789012", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export)", "56789012");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TaxFileCode, "66789012", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export)", "66789012");
			cei.CEI_EntityType = EntityTypeList.Codes.CustomsCode;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", ZString.Empty);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "56789012", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "67890123", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Export)", "67890123");
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", ZString.Empty);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "67890123", Core.Constants.CountryCodes.SouthAfrica);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "78901234", Core.Constants.CountryCodes.SouthAfrica);
			AssertEntityCode("(jobDeclaration.JE_MessageType == ZAJobMessageTypeList.Codes.Import)", "67890123");
		}

		public void TestGetEntityType()
		{
			cei.CEI_EntityType = EntityTypeList.Codes.IDPassportNumber;
			AssertEquals(EntityTypeList.Codes.IDPassportNumber, ucrHelper.CalculateUCR(cei).SubstringSafe(11, 1));
		}

		public void TestGetRefType()
		{
			cei.CEI_RefType = RefTypeList.Codes.Other;
			AssertEquals(RefTypeList.Codes.Other, ucrHelper.CalculateUCR(cei).SubstringSafe(12, 3));
		}

		public void TestGetRefNum()
		{
			cei.CEI_UCROrderNumber = "1234567890123456789";
			AssertEquals("1234567890123456789", ucrHelper.CalculateUCR(cei).SubstringSafe(15, 19));
			cei.CEI_UCROrderNumber = ZString.Empty;
			cei.CEI_Scope = ScopeList.Codes.SingleUse;
			var invHeader = dec.Invoices.AddNew();
			cei.CEI_RefType = RefTypeList.Codes.Other;
			invHeader.JZ_InvoiceNumber = "12345678901234567890";
			cei.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertEquals("JZ_InvoiceNumber should be truncated to 19 chars", "1234567890123456789S", ucrHelper.CalculateUCR(cei).SubstringSafe(15, 20));
			cei.CEI_RefType = RefTypeList.Codes.Other;
			invHeader.JZ_InvoiceNumber = "1234567890 !@#$%^&*() 123456789";
			cei.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertEquals("Special Chars & spaces shouldn't be included.", "1234567890123456789S", ucrHelper.CalculateUCR(cei).SubstringSafe(15, 20));
		}

		public void TestGetScope()
		{
			cei.CEI_Scope = ScopeList.Codes.MultipleUse;
			AssertEquals(ScopeList.Codes.MultipleUse, ucrHelper.CalculateUCR(cei).SubstringSafe(34, 1));
		}

		public void TestValidateUCRwithBLNS()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_UCROverride = "Unit Test";
			AssertNoMessageErrorContaining(cei.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_RL_NKOrigin = Core.Constants.CountryCodes.Namibia;
			cei.CEI_UCROverride = "Unit Test 2";
			AssertHasMessageErrorContaining(cei.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);

			dec.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			cei.CEI_UCROverride = "Unit Test 3";
			AssertNoMessageErrorContaining(cei.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);

			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			cei.CEI_UCROverride = "Unit Test 4";
			AssertHasMessageErrorContaining(cei.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);
		}

		[TestDate(2032, 06, 04)]
		public void TestCalculateUCR()
		{
			var supplier = Factory.Load<OrgHeader>(dec.JE_OH_Supplier);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890123", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("2ZA1234567890123POTH12345678901234M", ucrHelper.CalculateUCR(cei));
		}

		protected override void SetUp()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZAAM";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			ucrHelper = new UCRHelper();
			dec = Factory.New<JobDeclaration>();
			cei = dec.CustomsEntryInstructions.AddNew();
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			dec.JE_OH_Supplier = supplier.PK;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_RL_NKOrigin = unloco.RL_Code;
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			cei.CEI_EntityType = EntityTypeList.Codes.IDPassportNumber;
			cei.CEI_RefType = RefTypeList.Codes.Other;
			cei.CEI_UCROrderNumber = "1234567890123456789";
			cei.CEI_Scope = ScopeList.Codes.MultipleUse;
		}

		UCRHelper ucrHelper;
		CusEntryInstruction cei;
		JobDeclaration dec;
		OrgHeader supplier;

		void AssertEntityCode(string msg, string actual)
		{
			AssertEquals(msg, actual, ucrHelper.GetEntityCode(cei.CEI_EntityType, dec));
		}
	}
}
