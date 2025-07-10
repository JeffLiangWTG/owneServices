using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobUSDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DRWRetailSalesSubstitution()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = "";
			var usDeclaration = declaration.USDeclaration;
			var validator = usDeclaration.Validation;
			declaration.USD_RetailSalesSubstitutionIndicator = false;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertNoMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
			declaration.USD_RetailSalesSubstitutionIndicator = true;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertHasMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._56;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertNoMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._57;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertHasMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._70;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertNoMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
			declaration.USD_RetailSalesSubstitutionIndicator = false;
			validator.ValidateUSD_RetailSalesSubstitutionIndicator();
			AssertNoMessageErrorContaining(usDeclaration.USD_RetailSalesSubstitutionIndicatorInfo, JobUSDeclarationValidation.RetailSalesSubstitutionShouldTickOff);
		}

		public void TestFPPIHasPOAWhenRouted()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.FPPIRequired);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.FPPIRequired);
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.USD_OH_ForeignPrincipalParty = orgHeader.PK;
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.POARequiredForRouted);
			AssertNoMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.FPPIRequired);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.FPPINotRequired);
			AssertNoMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.POARequiredForRouted);
			declaration.USD_OH_ForeignPrincipalParty = ZGuid.Empty;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.FPPINotRequired);
			var poaDocument = orgHeader.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(10);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			declaration.USD_OH_ForeignPrincipalParty = orgHeader.PK;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, JobUSDeclarationValidation.POARequiredForRouted);
		}

		public void TestForeignSupplierGetPOADatesValidatedWhenRouted()
		{
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			declaration.USD_OH_ForeignPrincipalParty = orgHeader.PK;
			var poaDocument = orgHeader.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(10);
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoMessageErrors(declaration.USD_OH_ForeignPrincipalPartyInfo);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(-1);
			var errorMsg = $"The Power of Attorney Document on the organization (eDocs > Document Tracking) expired on {poaDocument.EQ_ValidToDate.ToShortDateString()}.";
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageError(declaration.USD_OH_ForeignPrincipalPartyInfo, errorMsg);
		}

		[TestDate(2022, 02, 16)]
		public void TestFPPIRequiredForRoutedTransaction()
		{
			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var fPPI = Factory.New<OrgHeader>();
			fPPI.OH_IsConsignor = true;
			fPPI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.USD_OH_ForeignPrincipalParty = fPPI.PK;
			declaration.JE_OH_Supplier = supplierOrg.PK;
			declaration.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;

			var poaDocument = fPPI.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(10);
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			declaration.USD_OH_ForeignPrincipalParty = fPPI.PK;

			var countryCode = Core.Constants.CountryCodes.UnitedStates;
			var messageUS = ZString.Format(AESAddressValidator.FPPIRequiredForRoutedTransaction, countryCode);
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageUS);
			fPPI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageUS);
			supplierOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			countryCode = Core.Constants.CountryCodes.PuertoRico;
			var messagePR = ZString.Format(AESAddressValidator.FPPIRequiredForRoutedTransaction, countryCode);
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messagePR);
			fPPI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			countryCode = Core.Constants.CountryCodes.VirginIslands;
			var messageVI = ZString.Format(AESAddressValidator.FPPIRequiredForRoutedTransaction, countryCode);
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageVI);
			supplierOrg.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertHasMessageErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageVI);
			fPPI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageVI);

			declaration.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			declaration.USDeclaration.Validation.ValidateUSD_OH_ForeignPrincipalParty();
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageUS);
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messagePR);
			AssertNoErrorContaining(declaration.USD_OH_ForeignPrincipalPartyInfo, messageVI);
		}
	}
}
