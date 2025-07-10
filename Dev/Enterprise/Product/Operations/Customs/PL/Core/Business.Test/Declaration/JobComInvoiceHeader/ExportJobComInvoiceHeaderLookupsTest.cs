using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

internal class ExportJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestValuationCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.InvoiceHeaderValuationCodes;
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "3333", "description1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "4444", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		var lookups = invoiceHeader.Lookups;
		var codes = lookups.ValuationCodeList;
		CombineAssertions(() =>
		{
			AssertEquals("Contains 3333", true, codes.ContainsCode("3333"));
			AssertEquals("Contains 4444", true, codes.ContainsCode("4444"));
			AssertEquals("1 Description", "description1", codes.GetDescriptionFromCode("3333"));
			AssertEquals("2 Description", "description2", codes.GetDescriptionFromCode("4444"));
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			lookups = invoiceHeader.Lookups;
			codes = lookups.ValuationCodeList;
			AssertEquals("Doesn't contain for import", false, codes.ContainsCode("3333"));
		});
	}
}
