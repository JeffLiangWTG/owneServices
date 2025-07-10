using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

public class ExportJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOrganisationPK_Eori()
	{
		var errorMessage = "EORI number is missing in Organization data for the Supplier.";
		var declaration = Factory.New<JobDeclaration>();
		var supplier = declaration.SupplierDocumentaryAddress;
		var supplierHeader = Factory.New<OrgHeader>();
		supplier.OrganisationPK = supplierHeader.PK;
		var propertyInfo = supplier.OrganisationPKInfo;
		CombineAssertions(() =>
		{
			supplier.Validation.ValidateOrganisationPK();
			AssertHasMessageError("No eori", propertyInfo, errorMessage);
			supplierHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Poland);
			supplier.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Eori exists", propertyInfo, errorMessage);
		});
	}

	public void TestCheckOrganisationPK_SupplierExporter()
	{
		var errorMessage = "You have not selected Supplier / Exporter address.";
		var declaration = Factory.New<JobDeclaration>();
		var supplier = declaration.SupplierDocumentaryAddress;
		var supplierHeader = Factory.New<OrgHeader>();
		var exporter = declaration.ExporterDocAddress;
		var exporterHeader = Factory.New<OrgHeader>();
		var exporterAddress = exporterHeader.Addresses.AddNew();
		var propertyInfo = supplier.OrganisationPKInfo;
		CombineAssertions(() =>
		{
			supplier.Validation.ValidateOrganisationPK();
			AssertHasMessageError("No supplier, no exporter", propertyInfo, errorMessage);
			supplier.OrganisationPK = supplierHeader.PK;
			supplier.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Supplier, no exporter", propertyInfo, errorMessage);
			supplier.OrganisationPK = ZGuid.Empty;
			exporter.OrganisationPK = exporterHeader.PK;
			exporter.E2_OA_Address = exporterAddress.PK;
			supplier.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Exporter, no supplier", propertyInfo, errorMessage);
		});
	}

	public void TestCheckE2_OA_Address_Eori()
	{
		var errorMessage = "Selected organization does not have an EORI number.";
		var declaration = Factory.New<JobDeclaration>();
		var exporter = declaration.ExporterDocAddress;
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		var otherOrgAddress = orgHeader.Addresses.AddNew();
		var propertyInfo = exporter.E2_OA_AddressInfo;
		CombineAssertions(() =>
		{
			exporter.OrganisationPK = orgHeader.PK;
			exporter.E2_OA_Address = orgAddress.PK;
			exporter.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("No eori at all", propertyInfo, errorMessage);
			var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Poland);
			exporter.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("Eori for all addresses", propertyInfo, errorMessage);
			eoriCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			exporter.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("Eori for same address", propertyInfo, errorMessage);
			eoriCusCode.OK_OA_PremisesAddress = otherOrgAddress.PK;
			exporter.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("Eori for other address", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0088E()
	{
		const string errorMessage = "[R0088E] Either NIP and/or REGON is required when EORI is not available.";
		var (exporter, exporterHeader) = InitR0088EExporter();
		var propertyInfo = exporter.E2_OA_AddressInfo;
		CombineAssertions(() =>
		{
			exporter.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("ExporterAddress does not have eorinumber", propertyInfo, errorMessage);

			AddEoriNumberForOrgHeader(exporterHeader);
			exporter.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("ExporterAddress have eorinumber", propertyInfo, errorMessage);

			exporter.E2_OA_Address = ZGuid.Empty;
			var (supplier, supplierHeader) = InitR0088ESupplier();
			propertyInfo = supplier.E2_OA_AddressInfo;
			supplier.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("SupplierAddress does not have eorinumber", propertyInfo, errorMessage);

			AddEoriNumberForOrgHeader(supplierHeader);
			supplier.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("ExporterAddress have eorinumber", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0088E_WhenIsExporterAddressAndIsNaturalPersonIndividual()
	{
		var errorMessage = "[R0088E] Either PESEL and/or other identification number is required when EORI is not available.";
		var declaration = Factory.New<JobDeclaration>();
		var (exporter, exporterHeader) = InitR0088EExporter();
		exporterHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		var propertyInfo = exporter.E2_OA_AddressInfo;
		CombineAssertions(() =>
		{
			exporter.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("IdentificationDataPL is empty", propertyInfo, errorMessage);

			AddPeselForOrgHeader(exporterHeader);
			exporter.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("IdentificationDataPL is not empty", propertyInfo, errorMessage);
		});
	}

	public void TestCheckRuleR0088E_WhenIsSupplierAddressAndNotNaturalPersonIndividual()
	{
		var errorMessage = "[R0088E] Either NIP and/or REGON is required when EORI is not available.";
		var (supplier, supplierHeader) = InitR0088ESupplier();
		supplierHeader.OH_Category = OrgConstants.Category.Business;
		var propertyInfo = supplier.E2_OA_AddressInfo;
		CombineAssertions(() =>
		{
			supplier.Validation.ValidateE2_OA_Address();
			AssertHasMessageError("IdentificationDataPL is empty", propertyInfo, errorMessage);

			AddNipForOrgHeader(supplierHeader);
			supplier.Validation.ValidateE2_OA_Address();
			AssertNoMessageError("IdentificationDataPL is not empty", propertyInfo, errorMessage);
		});
	}

	(JobDocAddress exporter, OrgHeader exporterHeader) InitR0088EExporter()
	{
		var declaration = Factory.New<JobDeclaration>();
		var exporter = declaration.ExporterDocAddress;
		var exporterHeader = Factory.New<OrgHeader>();
		var orgAddress = exporterHeader.Addresses.AddNew();
		exporter.OrganisationPK = exporterHeader.PK;
		exporter.E2_OA_Address = orgAddress.PK;
		return (exporter, exporterHeader);
	}

	(JobDocAddress supplier, OrgHeader supplierHeader) InitR0088ESupplier()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supplier = declaration.SupplierDocumentaryAddress;
		var supplierHeader = Factory.New<OrgHeader>();
		var supplierAddress = supplierHeader.Addresses.AddNew();
		supplier.OrganisationPK = supplierHeader.PK;
		supplier.E2_OA_Address = supplierAddress.PK;
		return (supplier, supplierHeader);
	}

	void AddNipForOrgHeader(OrgHeader header)
	{
		var cusCode = header.CustomsCodes.AddNew();
		cusCode.OK_CodeType = OrgCusCode.PolandCodeTypes.NIP;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		cusCode.OK_CustomsRegNo = "NIPNO";
	}

	void AddPeselForOrgHeader(OrgHeader header)
	{
		var cusCode = header.CustomsCodes.AddNew();
		cusCode.OK_CodeType = Constants.CusCodeTypes.PES;
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Poland;
		cusCode.OK_CustomsRegNo = "PESELNO";
	}

	void AddEoriNumberForOrgHeader(OrgHeader orgHeader) => orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123");
}
