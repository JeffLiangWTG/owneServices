using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var validation = (FormalImportJobDeclarationValidation)declaration.Validation;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			AssertEquals(true, validation.IsCargoReleaseValidationMode);
			AssertEquals(true, validation.IsNonInBondValidationModeOn);
			AssertEquals(false, validation.IsEntrySummaryValidationMode);

			declaration.US_EnableCRL = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertEquals(true, validation.IsCargoReleaseValidationMode);
			AssertEquals(true, validation.IsNonInBondValidationModeOn);
			AssertEquals(true, validation.IsEntrySummaryValidationMode);

			declaration.US_CertifyCargoRelease = false;
			AssertEquals(false, validation.IsCargoReleaseValidationMode);

			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			AssertEquals(false, validation.IsCargoReleaseValidationMode);
			AssertEquals(false, validation.IsNonInBondValidationModeOn);
			AssertEquals(false, validation.IsEntrySummaryValidationMode);

			var bill = declaration.Bills.AddNew();
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
			AssertEquals(false, validation.IsCargoReleaseValidationMode);
			AssertEquals(false, validation.IsNonInBondValidationModeOn);
			AssertEquals(false, validation.IsEntrySummaryValidationMode);

			declaration.ValidationModes = ValidationModes.CargoRelease;
			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			AssertEquals(true, validation.IsCargoReleaseValidationMode);
		}

		public void TestCheckIOROrgPK()
		{
			var declarationForIOR = Factory.New<JobDeclaration>();
			declarationForIOR.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationForIOR.US_EnableCRL = true;
			declarationForIOR.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declarationForIOR.IOROrgPK = Guid.Empty;

			var org1 = Factory.New<OrgHeader>();
			org1.OH_FullName = "Stupid EIN";
			declarationForIOR.Validation.CheckIOROrgPK(declarationForIOR.IOROrgPKInfo);
			AssertHasMessageError(declarationForIOR.IOROrgPKInfo, "You have not entered an IOR.");

			declarationForIOR.IOROrgPK = org1.PK;
			AssertNoMessageError(declarationForIOR.IOROrgPKInfo, "You have not entered an IOR.");
			AssertHasMessageError(declarationForIOR.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			org1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "EIN777", Core.Constants.CountryCodes.UnitedStates);
			declarationForIOR.IOROrgPKInfo.ClearAllNotifications();
			declarationForIOR.Validation.CheckIOROrgPK(declarationForIOR.IOROrgPKInfo);
			AssertNoMessageError(declarationForIOR.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			declarationForIOR.US_EntryType = EntryTypeList.Codes.LowValue;
			declarationForIOR.IOROrgPK = Guid.Empty;
			AssertNoMessageError(declarationForIOR.IOROrgPKInfo, "You have not entered an IOR.");

			var header = declarationForIOR.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.US_TTBInd = "C";
			line.US_TTBDisclaimReason = "D";
			declarationForIOR.IOROrgPK = Guid.Empty;
			declarationForIOR.Validation.CheckIOROrgPK(declarationForIOR.IOROrgPKInfo);
			AssertHasMessageError(declarationForIOR.IOROrgPKInfo, "You have not entered an IOR.");
			declarationForIOR.IOROrgPK = org1.PK;
			AssertNoMessageError(declarationForIOR.IOROrgPKInfo, FormalImportJobDeclarationValidation.IORCustomsRegNoRequired);

			var poaDocument = declarationForIOR.DocsAndCartage.RequiredDocuments.AddNew();
			poaDocument.EQ_DocType = RefDocTypes.PowerOfAttorney;
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = JobRequiredDocuments.DocumentPeriods.Periodic;
			poaDocument.EQ_ValidToDate = ZDate.Today.AddMonths(-2);

			AssertNoExceptionThrown(() => declarationForIOR.Validation.CheckIOROrgPK(declarationForIOR.IOROrgPKInfo));
		}

		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;

			var validation = (FormalImportJobDeclarationValidation)declaration.Validation;

			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);

			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
		}

		public void TestCheckWHSInvLineFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OP = part.PK;
			invoiceLine.JI_InvoiceQuantity = 1m;
			var whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSInvLineFilter = invoiceLine.PK;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.WHSInvLineFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			invoiceLine.JI_OP = ZGuid.Empty;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSInvLineFilter = ZGuid.Invalid;
			AssertNoWarningContaining(declaration.WHSInvLineFilterInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckJE_ApplicationCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.JE_ApplicationCode = "";
			AssertHasMessageErrorContaining(declaration.JE_ApplicationCodeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ApplicationCode = "~~~";
			AssertNoMessageErrorContaining(declaration.JE_ApplicationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasErrorContaining(declaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeError);

			declaration.JE_ApplicationCode = "";
			AssertNoErrorContaining(declaration.JE_ApplicationCodeInfo, ListValidation.InvalidCodeError);
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dec2 = factory2.Load<JobDeclaration>(declaration.PK);

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dec3 = factory3.Load<JobDeclaration>(declaration.PK);
			dec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			factory3.Save();

			dec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			bool saveException = false;
			try
			{
				factory2.Save();
			}
			catch (ZSaveException)
			{
				saveException = true;
			}
			Assert("We cannot have JE_ApplicationCode be merged when conflicts occur.", saveException);
		}

		public void TestCheckJE_MasterBillIssuerSCAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_MasterBillIssuerSCAC = "aaa";
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);
		}

		public void TestCheckJE_HouseBillIssuerSCAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_HouseBillIssuerSCAC = "aaa";
			AssertHasMessageError(declaration.JE_HouseBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_HouseBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_HouseBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_HouseBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_HouseBillIssuerSCACInfo, FormalImportJobDeclarationValidation.IssuerSCACiSNotPermitted);
		}

		public void TestCheckWHSProductFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			var whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSProductFilter = part.OP_PartNum;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.WHSProductFilter = "#DS#@";
			AssertHasWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_EnableENS = true;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			invoiceLine.JI_PartNo = ZString.Empty;
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			declaration.WHSProductFilter = "#DS#@";
			AssertNoWarningContaining(declaration.WHSProductFilterInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckWHSPackageFilter()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var org2 = Factory.New<OrgHeader>();
			org2.CompanyData.OB_IMUsedBondedWhs = false;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			var whsPack = declaration.WHSPacks.AddNew();
			var whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
			declaration.WHSPackageFilter = whsPack.PK;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.WHSPackageFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_EnableENS = true;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_OH_Importer = org2.PK;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			invoiceLine.JI_OP = ZGuid.Empty;
			AssertNoWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-1);
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, declaration.IsWHSUniversalXMLActive);

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			whsPack = declaration.WHSPacks.AddNew();
			whsPackLine = declaration.WHSPackLines.AddNew(whsPack);
			declaration.WHSPackageFilter = ZGuid.Invalid;
			AssertHasWarningContaining(declaration.WHSPackageFilterInfo, ListValidation.InvalidCodeError);
		}

		public void TestAddAWarningForEmptyETA()
		{
			DataRegistry.Business.USCustomsDataRegistry.Instance.EnableETAValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
			AssertHasWarning(declaration.JE_DateAtFinalDestinationInfo, FormalImportJobDeclarationValidation.EmptyETAWarning);

			declaration.JE_DateAtFinalDestination = new ZDateTime(2012, 1, 1);
			AssertNoWarning(declaration.JE_DateAtFinalDestinationInfo, FormalImportJobDeclarationValidation.EmptyETAWarning);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
			AssertNoWarning(declaration.JE_DateAtFinalDestinationInfo, FormalImportJobDeclarationValidation.EmptyETAWarning);

			DataRegistry.Business.USCustomsDataRegistry.Instance.EnableETAValidation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateAtFinalDestination = ZDateTime.Empty;
			AssertNoWarning(declaration.JE_DateAtFinalDestinationInfo, FormalImportJobDeclarationValidation.EmptyETAWarning);
		}

		public void TestCusRegistrationNoOnOrg()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;

			var ior = Factory.New<OrgHeader>();
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "AAAAAAAAAABBBBBBBBBB");
			dec.IOROrgPK = ior.PK;
			AssertHasMessageError(dec.IOROrgPKInfo, "The Importer Number, 'AAAAAAAAAABBBBBBBBBB', should be less than 12");

			dec.IOROrgPKInfo.ClearAllNotifications();
			ior.CustomsCodes.RemoveAndDeleteAll();
			ior.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "061122-12345");
			AssertNoMessageErrors(dec.IOROrgPKInfo);
		}

		public void TestMoveCBPF4811FromImporterToIOR()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			var importer = Factory.New<OrgHeader>();
			var importerWrapper = OrgHeaderWrapper.New(importer);
			importerWrapper.ZO_NPID = "IMPORTER";
			var ior = Factory.New<OrgHeader>();
			dec.IOROrgPK = ior.PK;

			dec.JE_OH_Importer = importer.PK;
			AssertHasMessageError(dec.JE_OH_ImporterInfo, FormalImportJobDeclarationValidation.MoveCBPF4811FromImporterToIOR);
			dec.IORWrapper.ZO_NPID = "IOR";
			dec.JE_OH_Importer = ZGuid.Empty;
			dec.JE_OH_Importer = importer.PK;
			AssertNoMessageError(dec.JE_OH_ImporterInfo, FormalImportJobDeclarationValidation.MoveCBPF4811FromImporterToIOR);
		}

		public void TestCheckDecEntryNumberWhenInsufficientBuffer()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(companyStmNums.SN_MaximumValue);
			companyStmNums.TryGetNumberFountain().GetNext(Factory);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var companyMessage = string.Format(ACEEntryStmNumsSetting.NotEnoughAvailableEntryNumbersForCompany, GlbCompany.CurrentCompany.GC_Code, "XJ5");
			AssertEquals("PreCondition", companyMessage, declaration.DisallowAllocateImportEntryNumber);
			declaration.DecEntryNumber = ZString.Empty;
			AssertHasMessageError(declaration.DecEntryNumberInfo, companyMessage);
		}

		public void TestCheckJE_MasterBillForSouthTruck()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_UC_NKCountryOfExport = "MX";
			declaration.US_SchDEntry = "2704";

			declaration.JE_MasterBill = ZString.Empty;
			AssertHasWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.EmptyMasterBillNumberWarningForTruck);

			declaration.ImportEntryNumber = "1";
			AssertNoWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.EmptyMasterBillNumberWarningForTruck);

			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageError(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.EmptyMasterBillNumberMessageErrorForTruck);
			AssertNoWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.MasterBillNumberForMXTruckIsDifferentToEntryNumber);
			declaration.JE_MasterBill = "XJ512345678";
			AssertHasWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.MasterBillNumberForMXTruckIsDifferentToEntryNumber);
			declaration.JE_MasterBill = "XJ512345678A";
			AssertNoWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.MasterBillNumberForMXTruckIsDifferentToEntryNumber);
			declaration.JE_MasterBill = "ZZZ12345678";
			AssertNoWarning(declaration.JE_MasterBillInfo, FormalImportJobDeclarationValidation.MasterBillNumberForMXTruckIsDifferentToEntryNumber);
		}

		public void TestCheckJE_MasterBillIssuerSCACRequiredForTruck()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_SchDEntry = "2704";

			declaration.JE_MasterBillIssuerSCAC = ZString.Empty;
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			Assert("Precondition", declaration.IsEntryNumberToBeDefaultedToMasterBill);
			declaration.JE_MasterBill = ZString.Empty;
			Assert("Should not have a mandatory validation as entry number is going to be defaulted into it", !declaration.JE_MasterBillInfo.HasMessageErrors());
		}

		public void TestCheckJE_ApplicationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertNoErrors(declaration.JE_ApplicationCodeInfo);

			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNoErrors(declaration.JE_ApplicationCodeInfo);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			Factory.Save();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertNoErrors(declaration.JE_ApplicationCodeInfo);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			Factory.Save();
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			string message = string.Format(FormalImportJobDeclarationValidation.CantChangeApplicationCodeUntilWithdrawn, JobApplicationCodeList.Codes.ACE);
			AssertHasError(declaration.JE_ApplicationCodeInfo, message);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertNoErrors(declaration.JE_ApplicationCodeInfo);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Factory.Save();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertNoError(declaration.JE_ApplicationCodeInfo, message);
		}

		public void TestCheckJE_ApplicationCodeForACEEntryTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertHasMessageError(declaration.JE_ApplicationCodeInfo, FormalImportJobDeclarationValidation.MessageModeMustBeACE);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.Validation.ValidateJE_ApplicationCode();
			AssertNoMessageError(declaration.JE_ApplicationCodeInfo, FormalImportJobDeclarationValidation.MessageModeMustBeACE);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.Validation.ValidateJE_ApplicationCode();
			AssertHasMessageError("Not lodged at Customs yet", declaration.JE_ApplicationCodeInfo, FormalImportJobDeclarationValidation.MessageModeMustBeACE);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			declaration.Validation.ValidateJE_ApplicationCode();
			AssertNoMessageError("Entries that are lodged before the date in ACS can be followed up in ACS after the date. Once it is lodged, we dontt really care about message mode.",
				declaration.JE_ApplicationCodeInfo, FormalImportJobDeclarationValidation.MessageModeMustBeACE);
		}

		public void TestApplicationCodeForACEEntryTypesAfterAdditionalACEEntryTypesEffectiveDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			declaration.Validation.ValidateJE_ApplicationCode();
			AssertHasMessageError(declaration.JE_ApplicationCodeInfo, FormalImportJobDeclarationValidation.MessageModeMustBeACE);
		}

		public void TestCheckJE_OH_ExternalBroker()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			OrgHeader broker = Factory.New<OrgHeader>();
			declaration.JE_OH_ExternalBroker = broker.PK;
			AssertHasWarning(declaration.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.NoValidCommunicationMethodIsDefinedForBIRD);

			EDICommunicationsMode ediMode = broker.EDICommunicationsModes.AddNew();
			ediMode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;
			ediMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			ediMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP;

			declaration.JE_OH_ExternalBroker = broker.PK;
			AssertNoWarning(declaration.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.NoValidCommunicationMethodIsDefinedForBIRD);

			declaration.JE_OH_ExternalBroker = ZGuid.Invalid;
			AssertHasError(declaration.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.SelectValidExternalBroker);

			declaration.JE_OH_ExternalBroker = ZGuid.Empty;
			AssertNoError(declaration.JE_OH_ExternalBrokerInfo, FormalImportJobDeclarationValidation.SelectValidExternalBroker);
		}

		public void TestCheckDecEntryNumberOverMaxLength()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "ABC";
			declaration.DecEntryNumber = "123456";

			Factory.Save();

			declaration.RunPreSaveValidation();
			AssertNoErrors(declaration.DecEntryNumberInfo);

			declaration.DecEntryNumber = "123457890";
			declaration.Validation.ValidateDecEntryNumber();
			AssertHasError(declaration.DecEntryNumberInfo, string.Format(FormalImportJobDeclarationValidation.MaxLengthExceeded, declaration.DecEntryNumber, MQEDIMessage.USEntryNumberPlaceHolder.Length));
		}

		public void TestCheckDecEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.RunPreSaveValidation();
			AssertNoErrors(declaration.DecEntryNumberInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryFilerCode = "ABC";
			declaration.DecEntryNumber = "123456";

			Factory.Save();

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration2.US_EntryFilerCode = "ABC";
			declaration2.DecEntryNumber = "123456";
			AssertHasError(declaration2.DecEntryNumberInfo, string.Format(FormalImportJobDeclarationValidation.DuplicateEntryNumber, declaration.JE_DeclarationReference));

			declaration2.DecEntryNumber = "123457";
			AssertNoError(declaration2.DecEntryNumberInfo, string.Format(FormalImportJobDeclarationValidation.DuplicateEntryNumber, declaration.JE_DeclarationReference));

			declaration2.DecEntryNumber = "123456";
			declaration2.US_EntryFilerCode = "ABD";
			AssertNoError(declaration2.DecEntryNumberInfo, string.Format(FormalImportJobDeclarationValidation.DuplicateEntryNumber, declaration.JE_DeclarationReference));
		}

		public void TestITDateWithNoITNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "OBL";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_ITDate = ZDateTime.Now;
			declaration.JE_PrimaryITNumber = "1";
			AssertNoMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITnumberIsRequiredWhenITDateIsEntered);

			declaration.JE_PrimaryITNumber = "";
			AssertHasMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITnumberIsRequiredWhenITDateIsEntered);
		}

		public void TestITNumberWhenEntryAndDischargeDiffer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.JE_MasterBill = "OBL";
			declaration.US_SchDArrival = "2904";
			declaration.US_SchDEntry = "2907";
			declaration.JE_PrimaryITNumber = "";
			declaration.Validation.ValidateJE_PrimaryITNumber();
			AssertHasMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargePortDifferBCR);

			declaration.US_SchDEntry = "2904";
			declaration.JE_PrimaryITNumber = "";
			declaration.Validation.ValidateJE_PrimaryITNumber();
			AssertNoMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargePortDifferBCR);

			declaration.US_SchDEntry = "2907";
			declaration.JE_PrimaryITNumber = "5001";
			AssertNoMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargePortDifferBCR);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.US_SchDArrival = "2904";
			declaration.US_SchDEntry = "1101";
			declaration.JE_PrimaryITNumber = "";
			declaration.Validation.ValidateJE_PrimaryITNumber();
			AssertHasMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer);

			declaration.JE_PrimaryITNumber = "1";
			AssertNoMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer);

			declaration.Bills[0].ITAndSplitDetails.RemoveAndDeleteAll();
			declaration.US_SchDEntry = "1101";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.Validation.ValidateJE_PrimaryITNumber();
			AssertHasMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.Validation.ValidateJE_PrimaryITNumber();
			AssertNoMessageError(declaration.JE_PrimaryITNumberInfo, ITNumberValidator.Constants.ITNumber.ITNumberIsRequiredWhenEntryAndDischargeDistrictsDiffer);
		}

		public void TestCheckJE_MessageTypeForEntryFilerCode()
		{
			DeclarationTestHelper.SetEntryFilerCode("");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			var messageError = string.Format(CommonImportJobDeclarationValidation.EntryFilerCodeIsEmptyAndEntryNumberCannotBeGenerated, CommonImportJobDeclarationValidation.WhyNeedEntryFilerCode) + ((IRegistryItemInternals)USCustomsDataRegistry.Instance.EntryFiler).Location;
			AssertHasError(declaration.JE_MessageTypeInfo, messageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNoError(declaration.JE_MessageTypeInfo, messageError);

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertNoError(declaration.JE_MessageTypeInfo, messageError);

			var outportDec = Factory.New<JobDeclaration>();
			outportDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals("PreCondition", ZString.Empty, outportDec.US_EntryFilerCode);
			AssertNoError(outportDec.JE_MessageTypeInfo, messageError);
		}

		public void TestMasterBillNotMandatoryForEachTransportMode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBill = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_MasterBill = ZString.Empty;
			AssertHasWarningContaining(declaration.JE_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestChangeBranchWithDifferentFilerCodeAfterMessageIsSent()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "~~1";
			branch.GB_RL_NKHomePort = "USLAX";

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			AssertNoNotifications(declaration.JE_GBInfo);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			Factory.Save();

			declaration.JE_GB = branch.PK;
			AssertHasErrors(declaration.JE_GBInfo);
		}

		public void TestJE_HouseBillPullsValidationInformationFromPrimaryHouseBill()
		{
			BillValidatorTest.AssertValidate(declaration.JE_HouseBillInfo, "House Bill");
		}

		public void TestJE_MasterBillPullsValidationInformationFromPrimaryMasterBill()
		{
			BillValidatorTest.AssertValidate(declaration.JE_MasterBillInfo, "Master Bill");
		}

		public void TestMasterBillMandatoryValidation()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBillIssuerSCAC = "ABCD";
			declaration.JE_MasterBill = string.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, "You have not entered a Master Bill");
			declaration.JE_MasterBill = "1";
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, "You have not entered a Master Bill");

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.JE_MasterBill = string.Empty;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillInfo, "You have not entered a Master Bill");

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.JE_MasterBill = string.Empty;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, "You have not entered a Master Bill");

			declaration.JE_MasterBillIssuerSCAC = string.Empty;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillInfo, "You have not entered a Master Bill");
		}

		public void TestUS_SchDLoading()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_SchDLoading = "";
			AssertNoMessageErrorContaining(declaration.US_SchDLoadingInfo, FormalImportAddInfoJobDeclarationValidation.PortOfLoadingRequiredForWaterBorne);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_SchDLoading = "";
			AssertHasMessageErrorContaining(declaration.US_SchDLoadingInfo, FormalImportAddInfoJobDeclarationValidation.PortOfLoadingRequiredForWaterBorne);

			declaration.US_SchDLoading = "60267";
			AssertNoMessageErrorContaining(declaration.US_SchDLoadingInfo, FormalImportAddInfoJobDeclarationValidation.PortOfLoadingRequiredForWaterBorne);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.US_SchDLoading = "";
			AssertHasMessageErrorContaining(declaration.US_SchDLoadingInfo, FormalImportAddInfoJobDeclarationValidation.PortOfLoadingRequiredForWaterBorne);

			declaration.US_SchDLoading = "60267";
			AssertNoMessageErrorContaining(declaration.US_SchDLoadingInfo, FormalImportAddInfoJobDeclarationValidation.PortOfLoadingRequiredForWaterBorne);
		}

		public void TestUS_SchDArrivalForAir()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2881", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");

			Factory.Save();

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			AssertEquals("IsAir", true, declaration.IsAir);

			declaration.US_SchDArrival = "";
			AssertHasMessageErrors(declaration.US_SchDArrivalInfo);

			declaration.US_SchDArrival = "2881";
			AssertNoMessageErrors(declaration.US_SchDArrivalInfo);
		}

		public void TestDateOfFirstArrival()
		{
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/22/23"));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/22/23"));

			declaration.US_NAFTAReconIndicator = true;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertHasMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/22/23"));

			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertNoMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/22/23"));

			declaration.US_NAFTAReconIndicator = false;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertNoMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/22/23"));

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.Validation.ValidateJE_DateOfArrival();
			AssertHasMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/23"));

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.Validation.ValidateJE_DateOfArrival();
			AssertNoMessageError(declaration.JE_DateOfArrivalInfo, ZString.Format(FormalImportJobDeclarationValidation.DateOfImportationRequiredForACertainEntryType, "21/23"));
		}

		public void TestCheckJE_OH_Importer()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			declaration.ValidationModes = ValidationModes.None;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.ValidationModes = ValidationModes.EntrySummary;
			AssertEquals("PreCondition: IsEntrySummaryValidationMode", true, declaration.IsEntrySummaryValidationMode);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ORG" + new Random().Next(1000000).ToString();

			var iorWrapper = OrgHeaderWrapper.New(importer);
			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.No;

			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			var customsCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123");
			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			iorWrapper.ZO_IsEINNumberVerifiedIndicator = YesNoDefaultList.Codes.Yes;
			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.OrganisationShouldBeRegisteredInCustoms);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_OH_Importer = importer.PK;
			AssertNoMessageErrorContaining(declaration.JE_OH_ImporterInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "123456789";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncated);

			declaration.JE_VoyageFlightNo = "12346";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncated);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "AA0016";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncatedAir);

			declaration.JE_VoyageFlightNo = "AA001456";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncatedAir);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_VoyageFlightNo = "";
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoRequired);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoRequired);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestCheckJE_VoyageFlightNoForNumericAirlineCodeUse()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			declaration.JE_VoyageFlightNo = "5X123456";
			AssertHasWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncatedAir);

			declaration.JE_VoyageFlightNo = "5X123";
			AssertNoWarningContaining(declaration.JE_VoyageFlightNoInfo, FormalImportJobDeclarationValidation.VoyageFlightNoTruncatedAir);
		}

		public void TestCheckFlightNoFormatForNumericAirlineCodeUse()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "5X12345678";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "X1";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "5X";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "123";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "1234";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "123A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "1234B";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "5X1";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "X512";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "5X12A";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "5X123";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "5X3004";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "5X250A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "X50250A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "X50250AB";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "9W02A5";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "X123";
			AssertHasMessageError("Flight No not formatted", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "350A";
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "0350A";
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "9WE";
			AssertNoMessageErrors("Vessel No should not be affected", declaration.JE_VoyageFlightNoInfo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_VoyageFlightNo = ZString.Empty;
			AssertNoMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
		}

		public void TestVesselVoyageNotRequiredForFTZ()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertHasMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertNoMessageErrorContaining(declaration.JE_VoyageFlightNoInfo, JobDeclarationValidation.VoyageFlightNoRequired);
		}

		public void TestCheckFlightNoFormat()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "123456789";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "1";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "12";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "123";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "1234";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "123A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "1234B";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "QF1";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "QF12";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "BA12A";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "UA123";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "BA3004";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "QF250A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "SQ0250A";
			AssertNoMessageError("Valid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "SQ0250AB";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "SQ02A5";
			AssertHasMessageError("Invalid Flight No format", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "A123";
			AssertHasMessageError("Flight No not formatted", declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_VoyageFlightNo = "350A";
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_VoyageFlightNo = "0350A";
			AssertNoMessageErrors("Flight No is valid", declaration.JE_VoyageFlightNoInfo);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "22E";
			AssertNoMessageErrors("Vessel No should not be affected", declaration.JE_VoyageFlightNoInfo);
		}

		public void TestCheckFlightNoForHandCarry()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_MasterBillIssuerSCAC = "AA";
			declaration.US_EnableENS = true;

			declaration.JE_VoyageFlightNo = "QF12";
			AssertHasWarning(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);

			declaration.JE_MasterBillIssuerSCAC = "AAAA";
			declaration.JE_VoyageFlightNo = "QF12";
			AssertNoWarning(declaration.JE_VoyageFlightNoInfo, USAirlineNumberValidator.InvalidFlightNoFormat);
		}

		public void TestCheckJE_ExportDate()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_ExportDate = ZDateTime.Empty;
			declaration.US_ITDate = ZDateTime.Today.AddDays(-2);
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.US_EntryType = "";
			declaration.US_EnableCRL = true;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);

			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-2).AddHours(10);
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);

			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-4);
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);

			declaration.US_ITDate = ZDateTime.Today.AddDays(-1);
			declaration.JE_ExportDate = ZDateTime.Today;
			AssertHasMessageErrorContaining(declaration.JE_ExportDateInfo, FormalImportJobDeclarationValidation.ExportDateShouldBeLessThanITDate);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(declaration.JE_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_TransportMode()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_IsHMFApplicable = "";
			AssertHasMessageErrorContaining(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSea);
			AssertHasMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);

			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoMessageErrorContaining(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			AssertHasMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForBWBMOT);
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			AssertNoMessageError(declaration.US_IsHMFApplicableInfo, FormalImportAddInfoJobDeclarationValidation.FeeApplicableForBWBMOT);
			AssertNoMessageErrorContaining(declaration.US_CargoReleaseTypeInfo, FormalImportAddInfoJobDeclarationValidation.BRCIsInvalid);
		}

		public void TestCheckMasterHouseBillsSCAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			declaration.JE_MasterBill = "1";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			AssertNotNull(primaryMasterBill);
			declaration.JE_HouseBill = "H1";
			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertNotNull(primaryHouseBill);

			AssertNoMessageError(primaryMasterBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertNoMessageError(primaryHouseBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(primaryMasterBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertHasMessageError(primaryHouseBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			primaryMasterBill.US_UI_NKBillIssuerSCAC = "AD";
			primaryHouseBill.US_UI_NKBillIssuerSCAC = "TH";
			AssertNoMessageError(primaryMasterBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertNoMessageError(primaryHouseBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			primaryMasterBill.US_UI_NKBillIssuerSCAC = "";
			primaryHouseBill.US_UI_NKBillIssuerSCAC = "";
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(primaryMasterBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertHasMessageError(primaryHouseBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			declaration.Validation.ValidateJE_HouseBillIssuerSCAC();
			AssertHasMessageError(primaryMasterBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertNoMessageError(primaryHouseBill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.Bills.RemoveAndDeleteAll();
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertHasMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_MasterBillIssuerSCAC();
			AssertNoMessageError(declaration.JE_MasterBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
		}

		public void TestCheckJE_OH_FDASubmitter()
		{
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			var fdaLine1 = invoiceLine.ACE_FDALines.AddNew();
			fdaLine1.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			FDAOrganisationValidatorTest.AssertOrganisation(declaration.JE_OH_FDASubmitterInfo, (ZGuid organisationPK) => declaration.JE_OH_FDASubmitter = organisationPK);

			var fdaSubmitter = Factory.New<OrgHeader>();
			fdaSubmitter.OH_Code = "ZXCVCXZV";
			declaration.JE_OH_FDASubmitter = fdaSubmitter.PK;
			var fdaSubmitterWrapped = OrgHeaderWrapper.New(fdaSubmitter);
			AssertHasMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.SubmitterFirmType);
			fdaSubmitterWrapped.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.I;
			Factory.Save();
			declaration.Validation.ValidateJE_OH_FDASubmitter();
			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.SubmitterFirmType);

			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.SubmitterFirmTypeValid);
			fdaSubmitterWrapped.ZO_SubmitterFirmType = "~";
			Factory.Save();
			declaration.Validation.ValidateJE_OH_FDASubmitter();
			AssertHasMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.SubmitterFirmTypeValid);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			DeclarationTestHelper.AddPGAContact(fdaSubmitter, null, null, null, ZString.Empty, ZString.Empty);
			declaration.Validation.ValidateJE_OH_FDASubmitter();
			AssertHasMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.OrganisationFAXOrEmailRequired);

			DeclarationTestHelper.AddPGAContact(fdaSubmitter, null, null, null, "Test@126.com", ZString.Empty);
			declaration.Validation.ValidateJE_OH_FDASubmitter();
			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.OrganisationFAXOrEmailRequired);

			DeclarationTestHelper.AddPGAContact(fdaSubmitter, null, null, null, ZString.Empty, "086784111100");
			declaration.Validation.ValidateJE_OH_FDASubmitter();
			AssertNoMessageError(declaration.JE_OH_FDASubmitterInfo, Business.ValidationConstants.PriorNotice.OrganisationFAXOrEmailRequired);
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Port", startDate, endDate);
			newFactory.Save();

			var coll = new BorderCargoPortCollection();
			var bcPort = coll.AddNew();
			bcPort.PortCode = "2704";
			bcPort.CRProcess = CRProcessList.Codes.OneStep;
			bcPort.Location = LocationList.Codes.South;
			DataRegistry.Business.USCustomsDataRegistry.Instance.BorderCargoReleasePorts.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, coll);
			Factory.Save();
		}
	}
}
