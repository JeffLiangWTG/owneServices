using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportJobDeclarationValidation_InbondTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestHouseBillNotRequiredAndMasterBillMandatory()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "";
			AssertEquals("Masterbill is mandatory", true, declaration.JE_MasterBillInfo.HasMessageErrors());

			declaration.JE_HouseBill = "";
			AssertEquals("Housebill non-mandatory", false, declaration.JE_HouseBillInfo.HasMessageErrors());

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.CU_ParentBillUniqueCode = "";
			AssertHasMessageErrors(bill.CU_ParentBillUniqueCodeInfo);
		}

		public void TestCheckJE_OH_ShippingLine()
		{
			declaration.JE_OH_ShippingLine = ZGuid.Empty;

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.FillWithValidTestData();
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.ValidationModes = ValidationModes.EntrySummary | ValidationModes.CargoRelease;
			declaration.JE_OH_ShippingLine = ZGuid.Empty;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.US_EnableENS = true;
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			declaration.Validation.ValidateJE_OH_ShippingLine();
			AssertHasMessageError(declaration.JE_OH_ShippingLineInfo, FormalImportJobDeclarationValidation.CarrierRequiredForPST);

			declaration.JE_OH_ShippingLine = shippingLine.PK;
			AssertNoMessageError(declaration.JE_OH_ShippingLineInfo, FormalImportJobDeclarationValidation.CarrierRequiredForPST);
			AssertHasMessageErrorContaining(declaration.JE_OH_ShippingLineInfo, "USP Allocated Contact and make sure the contact is active for Customs on Organization -> Contact -> Allocated Contact.");
		}

		public void TestCheckJE_TotalNoOfPacks()
		{
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;

			Package package1 = bill1.PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 10;

			Package package2 = bill2.PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 12;

			Package package3 = bill2.PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackQty = 8;

			declaration.JE_TotalNoOfPacks = 30;
			AssertNoWarning("Total package count on the main Declaration tab must be equal to the total number of packages on the Packing Tab > Package Details.", declaration.JE_TotalNoOfPacksInfo, FormalImportJobDeclarationValidation.PackagesDifferFromTotalPacks);

			declaration.JE_TotalNoOfPacks = 20;
			AssertHasWarning("Total package count on the main Declaration tab must be different to the total number of packages on the Packing Tab > Package Details.", declaration.JE_TotalNoOfPacksInfo, FormalImportJobDeclarationValidation.PackagesDifferFromTotalPacks);
		}

		public void TestValidateConsigneeAddressWhenForeignIOR()
		{
			var importerofRecord = Factory.New<OrgHeader>();
			importerofRecord.OH_RL_NKClosestPort = "AUSYD";

			var domesticUltimateConsignee = Factory.New<OrgHeader>();
			domesticUltimateConsignee.OH_RL_NKClosestPort = "USCHI";

			var prUltimateConsignee = Factory.New<OrgHeader>();
			prUltimateConsignee.OH_RL_NKClosestPort = "PRADJ";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerofRecord.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			declaration.IOROrgPK = importerofRecord.PK;
			declaration.JE_OA_ConsigneeAddress = importerofRecord.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasWarning(declaration.JE_OA_ConsigneeAddressInfo, "You have entered a foreign-based Ultimate Consignee with a foreign-based Importer of Record.");

			declaration.JE_OA_ConsigneeAddress = domesticUltimateConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, "You have entered a foreign-based Ultimate Consignee with a foreign-based Importer of Record.");

			declaration.JE_OA_ConsigneeAddress = importerofRecord.MainAddress.PK;
			declaration.JE_OA_ConsigneeAddress = prUltimateConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoWarning(declaration.JE_OA_ConsigneeAddressInfo, "You have entered a foreign-based Ultimate Consignee with a foreign-based Importer of Record.");
		}

		public void TestCheckJE_OA_ConsigneeAddress()
		{
			OrgHeader ultConsignee = Factory.New<OrgHeader>();

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.US_EnableCRL = true;

			string ultimateConsigneeMessageError = string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee");
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);

			declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0].US_UseConsigneeNameAddress = true;
			declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);

			declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0].US_UseConsigneeNameAddress = false;
			declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, ultimateConsigneeMessageError);

			ultConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-123GFD43");
			declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;
			declaration.Validation.ValidateJE_OA_ConsigneeAddress();
			AssertNoMessageErrors(declaration.JE_OA_ConsigneeAddressInfo);
		}

		public void TestCheckJE_OA_ConsigneeAddressWhenLineLevelUltimateConsigneeExist()
		{
			OrgHeader ultConsignee = Factory.New<OrgHeader>();

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			declaration.US_EnableCRL = true;
			declaration.JE_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_ConsigneeAddress = ultConsignee.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));

			OrgHeader ultConsignee2 = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ConsigneeAddress = ultConsignee2.MainAddress.PK;
			AssertHasMessageError(invoiceLine.JI_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertHasMessageError(declaration.JE_OA_ConsigneeAddressInfo, string.Format(OrganisationValidation.EIN_SSN_CBN_EncryptedCodeRequired, "Ultimate Consignee"));
		}

		public void TestCheckDuplicateMasterBillForConsolidatedEntry()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "ORG1";

			var declarationForReleaseEntry = Factory.NewWithValidTestData<JobDeclaration>();
			declarationForReleaseEntry.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationForReleaseEntry.JE_TransportMode = TransportTypeList.Codes.Sea;
			declarationForReleaseEntry.JE_MasterBill = "ABC123";
			declarationForReleaseEntry.JE_MasterBillIssuerSCAC = "APLU";
			declarationForReleaseEntry.JE_OH_Supplier = org1.PK;
			declarationForReleaseEntry.US_EnableENS = true;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "ABC123";
			declaration.JE_MasterBillIssuerSCAC = "APPL";
			declaration.JE_OH_Supplier = org1.PK;
			AssertNoWarningContaining(declaration.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declarationForReleaseEntry.JE_DeclarationReference, declarationForReleaseEntry.Company.GC_Name, declarationForReleaseEntry.Branch.GB_BranchName));

			declaration.JE_MasterBillIssuerSCAC = "APLU";
			AssertHasWarningContaining(declaration.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declarationForReleaseEntry.JE_DeclarationReference, declarationForReleaseEntry.Company.GC_Name, declarationForReleaseEntry.Branch.GB_BranchName));

			declaration.US_ConsolACE = ZBool.True;
			declaration.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(declaration.JE_MasterBillInfo, BillValidator.AlreadyContainsMasterBill(declarationForReleaseEntry.JE_DeclarationReference, declarationForReleaseEntry.Company.GC_Name, declarationForReleaseEntry.Branch.GB_BranchName));
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = false;
			declaration.US_EnableINB = true;
			AssertEquals("InBond", true, declaration.IsInBond);
		}
	}
}
