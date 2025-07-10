using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class OfficeCodeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPurposeListForImportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			var officeCode = declaration.CustomsOfficesForBinding.AddNew();
			AssertContainsExactElementsInAnyOrder("No offices, Import dec", new[] { "SCO", "PRE" }, officeCode.Lookups.OfficeTypeLookupList.GetAllCodes());

			officeCode.CY_Code = EuOfficeCodesTypes.Codes.AuthorityControlCode;
			var officeCode2 = declaration.CustomsOfficesForBinding.AddNew();
			AssertContainsExactElementsInAnyOrder("List for next office, Import dec", new[] { "PRE" }, officeCode2.Lookups.OfficeTypeLookupList.GetAllCodes());

			officeCode2.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			AssertContainsExactElementsInAnyOrder("List for existing office, Import dec", new[] { "SCO" }, officeCode.Lookups.OfficeTypeLookupList.GetAllCodes());
		});
	}

	public void TestPurposeListForExportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		CombineAssertions(() =>
		{
			var officeCode = declaration.CustomsOfficesForBinding.AddNew();
			AssertContainsExactElementsInAnyOrder("No offices, Export dec", new[] { "SCO", "PRE" }, officeCode.Lookups.OfficeTypeLookupList.GetAllCodes());

			officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
			var officeCode2 = declaration.CustomsOfficesForBinding.AddNew();
			AssertContainsExactElementsInAnyOrder("List for next office, Export dec", new[] { "SCO" }, officeCode2.Lookups.OfficeTypeLookupList.GetAllCodes());

			officeCode2.CY_Code = EuOfficeCodesTypes.Codes.SupervisingCustomsOffice;
			AssertContainsExactElementsInAnyOrder("List for existing office, Export dec", new[] { "PRE" }, officeCode.Lookups.OfficeTypeLookupList.GetAllCodes());
		});
	}
}
