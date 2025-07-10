using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.PL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExitSummaryJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOrganisationPK_Eori()
	{
		var errorMessage = "EORI number is missing in Organization data for the Supplier.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
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
}
