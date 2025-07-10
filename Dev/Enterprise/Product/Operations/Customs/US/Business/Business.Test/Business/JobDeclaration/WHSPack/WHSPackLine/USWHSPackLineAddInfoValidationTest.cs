using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USWHSPackLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_B7_WHSPack()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CompanyData.OB_IMUsedBondedWhs = true;
			var org2 = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			var relOrg1 = part.RelatedOrganisations.AddOwner(org1);
			var relOrg2 = part.RelatedOrganisations.AddOwner(org2);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg1.OU_OH;
			pivot.CI_TariffNum = "1010101010";
			pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg2.OU_OH;
			pivot.CI_TariffNum = "1010101010";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			var pack1 = declaration.WHSPacks.AddNew();
			var packLine1 = declaration.WHSPackLines.AddNew(pack1);
			var packLine2 = declaration.WHSPackLines.AddNew(pack1);
			packLine2.US_JI_InvoiceLine = invoiceLine.PK;
			packLine1.US_JI_InvoiceLine = invoiceLine.PK;
			AssertHasError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);
			packLine1.US_JI_InvoiceLine = ZGuid.Empty;
			AssertNoError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);
			packLine1.US_JI_InvoiceLine = invoiceLine.PK;
			AssertHasError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);

			packLine1.US_B7_WHSPack = ZGuid.Empty;
			AssertNoError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);
			packLine1.US_B7_WHSPack = pack1.PK;
			AssertHasError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);
			packLine2.Delete();
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoError(packLine1.US_B7_WHSPackInfo, ValidationConstants.WHSPackLine.DuplicatePackageReferenceAndInvoiceLineReference);

			packLine1.US_JI_InvoiceLine = ZGuid.Empty;
			packLine1.US_B7_WHSPack = ZGuid.Empty;
			AssertHasErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			packLine1.US_B7_WHSPack = pack1.PK;
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			packLine1.US_B7_WHSPack = ZGuid.Invalid;
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			declaration.JE_OH_Importer = org2.PK;
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			declaration.JE_OH_Importer = org1.PK;
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			packLine1.AddInfoValidation.ValidateUS_B7_WHSPack();
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-2);
			pack1 = declaration.WHSPacks.AddNew();
			packLine1 = declaration.WHSPackLines.AddNew();
			packLine1.US_B7_WHSPack = ZGuid.Invalid;
			AssertNoErrorContaining(packLine1.US_B7_WHSPackInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine1.US_B7_WHSPackInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckUS_PackedQty()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CompanyData.OB_IMUsedBondedWhs = true;
			var org2 = Factory.New<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			var pack = declaration.WHSPacks.AddNew();
			var packLine = declaration.WHSPackLines.AddNew(pack);
			packLine.US_PackedQty = ZDecimal.Zero;
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			packLine.US_PackedQty = 10m;
			AssertNoMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			packLine.US_PackedQty = -10m;
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.JE_OH_Importer = org2.PK;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertNoMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.JE_OH_Importer = org1.PK;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertNoMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertNoMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			packLine.AddInfoValidation.ValidateUS_PackedQty();
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyIsRequired);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			var relOrg1 = part.RelatedOrganisations.AddOwner(org1);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg1.OU_OH;
			pivot.CI_TariffNum = "1010101010";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			pack = declaration.WHSPacks.AddNew();
			packLine = declaration.WHSPackLines.AddNew(pack);
			invoiceLine.JI_InvoiceQuantity = 110m;
			packLine.US_JI_InvoiceLine = invoiceLine.PK;
			packLine.US_PackedQty = 200m;
			AssertHasMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyCannotBeGreaterThanInvoiceQuantity);
			packLine.US_PackedQty = 100m;
			AssertNoMessageError(packLine.US_PackedQtyInfo, ValidationConstants.WHSPackLine.PackedQtyCannotBeGreaterThanInvoiceQuantity);
		}

		public void TestCheckUS_JI_InvoiceLine()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.CompanyData.OB_IMUsedBondedWhs = true;
			var org2 = Factory.New<OrgHeader>();
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PD3234";
			var relOrg1 = part.RelatedOrganisations.AddOwner(org1);
			var relOrg2 = part.RelatedOrganisations.AddOwner(org2);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg1.OU_OH;
			pivot.CI_TariffNum = "1010101010";
			pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = relOrg2.OU_OH;
			pivot.CI_TariffNum = "1010101010";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			var pack = declaration.WHSPacks.AddNew();
			var packLine = declaration.WHSPackLines.AddNew(pack);
			packLine.US_JI_InvoiceLine = ZGuid.Empty;
			AssertHasErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			packLine.US_JI_InvoiceLine = invoiceLine.PK;
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			packLine.US_JI_InvoiceLine = ZGuid.Invalid;
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration.JE_OH_Importer = org2.PK;
			packLine.AddInfoValidation.ValidateUS_JI_InvoiceLine();
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration.JE_OH_Importer = org1.PK;
			packLine.AddInfoValidation.ValidateUS_JI_InvoiceLine();
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			packLine.AddInfoValidation.ValidateUS_JI_InvoiceLine();
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			packLine.AddInfoValidation.ValidateUS_JI_InvoiceLine();
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			packLine.AddInfoValidation.ValidateUS_JI_InvoiceLine();
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-2);
			pack = declaration.WHSPacks.AddNew();
			packLine = declaration.WHSPackLines.AddNew(pack);
			packLine.US_JI_InvoiceLine = ZGuid.Invalid;
			AssertNoErrorContaining(packLine.US_JI_InvoiceLineInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(packLine.US_JI_InvoiceLineInfo, ListValidation.InvalidCodeError);
		}
	}
}
