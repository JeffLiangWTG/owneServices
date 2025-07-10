using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(SupplementaryCodeLookups))]
sealed class SupplementaryCodeLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCY_CodeList() => CombineAssertions(() =>
	{
		SupplementaryCodeTestHelper.SetupTariffAndCusCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var header = declaration.Invoices.AddNew();
		var invoiceLine = header.JobComInvoiceLines.AddNew();

		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Botswana;
		invoiceLine.JI_Tariff = "DUMMYTRF";

		var additionalCodesListAll = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType is Empty", new ZString[] { "FA400", "MA400", "MB400", "MP400", "GA400", "GB400", "GP400" }, additionalCodesListAll.GetAllCodesZString());
		AssertEquals("Description for FA400", "FA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("FA400"));
		AssertEquals("Description for MA400", "MA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MA400"));
		AssertEquals("Description for MB400", "MB400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MB400"));
		AssertEquals("Description for MP400", "MP400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("MP400"));
		AssertEquals("Description for GA400", "GA400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GA400"));
		AssertEquals("Description for GB400", "GB400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GB400"));
		AssertEquals("Description for GP400", "GP400 Descriptions", additionalCodesListAll.GetDescriptionFromCode("GP400"));

		invoiceLine.JI_PackageType = "A";
		var additionalCodesListPackageTypeA = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == A", new ZString[] { "FA400", "MA400", "GA400" }, additionalCodesListPackageTypeA.GetAllCodesZString());

		invoiceLine.JI_PackageType = "B";
		var additionalCodesListPackageTypeB = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == B", new ZString[] { "FA400", "MB400", "GB400" }, additionalCodesListPackageTypeB.GetAllCodesZString());

		invoiceLine.JI_PackageType = "P";
		var additionalCodesListPackageTypeP = invoiceLine.Lookups.AdditionalCodesList;
		AssertContainsExactElementsInAnyOrder("When JI_PackageType == P", new ZString[] { "FA400", "MP400", "GP400" }, additionalCodesListPackageTypeP.GetAllCodesZString());
	});
}
