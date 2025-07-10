using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: declaration",
				() => new ImportJobDocAddressValidation(Factory.New<JobDocAddress>(), null));
		});
	}

	public void TestCheckRuleR257()
	{
		var messageError = "(R257) Fiscal role code FR7 with a valid VAT number is missing.";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.AdditionalInfos.AddNew();
		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.First();
		var entryLineFee = entryLine.Fees.AddNew();

		var declarantAddress = Factory.New<OrgAddress>();
		var orgHeader = Factory.New<OrgHeader>();
		declarantAddress.OA_OH = orgHeader.PK;
		declarantAddress.OA_Address1 = "Some street";
		var orgCusCodeSrt = Factory.New<OrgCusCode>();
		orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
		orgCusCodeSrt.OK_CodeType = OrgCusCode.PolandCodeTypes.TIN;
		orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;
		orgHeader.CustomsCodes.Add(orgCusCodeSrt);

		var importer = declaration.ImporterDocumentaryAddress;
		importer.OrganisationPK = orgHeader.PK;
		CombineAssertions(() =>
		{
			entryLineFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			importer.Validation.ValidateOrganisationPK();
			AssertHasMessageError("MethodOfPayment = 'G', ChargeType = 'BOO', no FR7 with a valid VAT number", importer.OrganisationPKInfo, messageError);

			var fiscalReference = invoiceLine.FiscalReferences.AddNew();
			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR7_Taxpayer;
			importer.Validation.ValidateOrganisationPK();
			AssertHasMessageError("MethodOfPayment = 'G', ChargeType = 'BOO', FR7 exist but VAT number is missing", importer.OrganisationPKInfo, messageError);

			fiscalReference.CFR_Reference = "123";
			importer.Validation.ValidateOrganisationPK();
			AssertNoMessageError("MethodOfPayment = 'G', ChargeType = 'BOO', FR7 exist with valid VAT number", importer.OrganisationPKInfo, messageError);

			fiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			importer.Validation.ValidateOrganisationPK();
			AssertHasMessageError("MethodOfPayment = 'G', ChargeType = 'BOO', no FR7 with a valid VAT number is missing", importer.OrganisationPKInfo, messageError);

			entryLineFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.A;
			importer.Validation.ValidateOrganisationPK();
			AssertNoMessageError("MethodOfPayment = 'A', ChargeType = 'BOO', no FR7 with a valid VAT number is missing", importer.OrganisationPKInfo, messageError);

			entryLineFee.CF_MethodOfPayment = PLMethodOfPaymentList.Codes.G;
			entryLineFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty;
			importer.Validation.ValidateOrganisationPK();
			AssertNoMessageError("MethodOfPayment = 'G', ChargeType = 'A35', no FR7 with a valid VAT number is missing", importer.OrganisationPKInfo, messageError);
		});
	}

	public void TestCheckOrganisationName()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var orgHeaderBUS = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderBUS.OH_Category = OrgConstants.Category.Business;
		orgHeaderBUS.OH_FullName = new ZString('1', 80);

		var orgHeaderNAT = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNAT.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNAT.OH_FullName = new ZString('1', 40).Insert(3, " ");

		var orgHeaderNATEmpty = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderNATEmpty.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		orgHeaderNATEmpty.OH_FullName = ZString.Empty;

		PLOrgHeaderValidationHelperTest.AssertValidationOrganizationName(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, orgHeaderBUS, orgHeaderNAT, orgHeaderNATEmpty);
	}
}
