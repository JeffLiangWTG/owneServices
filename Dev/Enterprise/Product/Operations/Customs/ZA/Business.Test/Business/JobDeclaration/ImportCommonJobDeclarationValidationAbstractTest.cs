using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestsSubclassesOf(typeof(ImportCommonJobDeclarationValidation))]
	abstract class ImportCommonJobDeclarationValidationAbstractTest : TestCaseWithFactory
	{
		public void TestImporterMandatory()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, ImportCommonJobDeclarationValidation.ImporterMandatory);
		}

		public void TestImporterMustHaveCustomsCodeForImportDeclarationIfNoEntry()
		{
			declaration.JE_OH_Importer = importer.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, ImportCommonJobDeclarationValidation.CustomsImporterCodeMessageError);
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "01796636");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, ImportCommonJobDeclarationValidation.CustomsImporterCodeMessageError);
		}

		public void TestImporterMustHaveValidCustomsCode()
		{
			declaration.JE_OH_Importer = importer.PK;
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "junk");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, ImportCommonJobDeclarationValidation.CustomsImporterCodeInvalid);
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "01796636");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, ImportCommonJobDeclarationValidation.CustomsImporterCodeInvalid);
		}

		public void TestWarnIfPortOfLoadingIsInvalidWhenImporting()
		{
			declaration.JE_RL_NKPortOfLoading = "OZZO";
			AssertHasWarning(declaration.JE_RL_NKPortOfLoadingInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestImporterMustHaveValidIDOrVATNumberIfNotRegisteredImporter()
		{
			OrgHeader importer = OrgHeader.New(Factory);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, ValidationConstants.Declaration.UnregisteredTraderCustomsCode);
			importer.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.TaxFileCode, "");
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, ValidationConstants.Declaration.UnregisteredTraderDoesNotHaveIDOrGTXCode("Importer"));
			importer.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "1234567890128");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.TaxFileCode, "");
			declaration.Validation.ValidateJE_OH_Importer();
			Assert("No message error", !declaration.JE_OH_ImporterInfo.GetMessageErrors().ContainsNotificationContaining("Customs Importer code"));
			importer.SetLocalCustomsCode(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "");
			importer.SetLocalCustomsCode(OrgCusCode.CodeTypes.TaxFileCode, "4770181941");
			declaration.Validation.ValidateJE_OH_Importer();
			Assert("No message error", !declaration.JE_OH_ImporterInfo.GetMessageErrors().ContainsNotificationContaining("Customs Importer code"));
		}

		public void TestCheckJE_OH_AgentOverride()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DBN");
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			importer.OH_FullName = "buyer";
			importer.OH_IsConsignee = true;
			importer.OH_Code = "IMP#@$43";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			var agent4 = Factory.New<OrgHeader>();
			agent4.OH_FullName = "buyer1";
			agent4.OH_IsConsignee = true;
			agent4.OH_Code = "IMP#@$44";
			agent4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			var creditorOrgHeader = Factory.New<OrgHeader>();
			creditorOrgHeader.OH_FullName = "creditor";
			creditorOrgHeader.OH_IsConsignee = true;
			creditorOrgHeader.CompanyData.OB_IsCreditor = true;
			creditorOrgHeader.OH_Code = "CRE#@$43";
			Factory.Save();
			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = importer.PK;
			creditor.CustomsOfficeCode = "DBN";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "DBN";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_AgentOverride = importer.PK;
			AssertNoErrorContaining(declaration.JE_OH_AgentOverrideInfo, "The Importer & Agent fields must have the same organization codes");
			declaration.JE_OH_Importer = agent4.PK;
			declaration.JE_OH_AgentOverride = importer.PK;
			AssertHasErrorContaining(declaration.JE_OH_AgentOverrideInfo, "The Importer & Agent fields must have the same organization codes");
			creditor.ImporterPays = ZBool.False;
			creditor.CreditorPK = creditorOrgHeader.PK;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration1.JE_CustomsOffice = "DBN";
			declaration1.JE_OH_Importer = agent4.PK;
			declaration1.JE_OH_AgentOverride = importer.PK;
			AssertNoErrorContaining(declaration1.JE_OH_AgentOverrideInfo, "The Importer & Agent fields must have the same organization codes");
			creditor.ImporterPays = ZBool.True;
			creditor.CreditorPK = ZGuid.Empty;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration2.JE_OH_Importer = agent4.PK;
			declaration2.JE_CustomsOffice = "JHB";
			declaration2.JE_OH_AgentOverride = importer.PK;
			AssertNoErrorContaining(declaration2.JE_OH_AgentOverrideInfo, "The Importer & Agent fields must have the same organization codes");
		}

		public void TestCheckJE_BOESightDate()
		{
			declaration.JE_BOESightNumber = "1";
			declaration.JE_BOESightDate = ZDateTime.Empty;
			Assert("BOE Sight Date should not be empty", declaration.JE_BOESightDateInfo.HasErrors());
			declaration.JE_BOESightDate = ZDateTime.Today;
			Assert("BOE Sight Date should not have errors", !declaration.JE_BOESightDateInfo.HasErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageType;
			importer = Factory.NewWithValidTestData<OrgHeader>();
		}

		protected abstract ZString MessageType { get; }

		protected JobDeclaration declaration;
		protected OrgHeader importer;
	}
}
