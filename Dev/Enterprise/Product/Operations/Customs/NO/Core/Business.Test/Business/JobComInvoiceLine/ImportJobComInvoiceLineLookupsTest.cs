using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ImportJobComInvoiceLineLookups))]
sealed class ImportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ImportJobComInvoiceLineLookups>
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	public void Test_JI_PrimaryPreference()
	{
		RefCusProcedureHelper.CreateCusPreference(Factory);
		var codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.PrimaryPreferenceList, codeList);
			AssertType<PrimaryPreferenceCodeList>("Type", codeList);
			AssertEquals("Codes from lookup", "A, B, C, G, P, J, N", codeList.CodesAsString);
		});
	}

	public void Test_JI_PrimaryPreferenceForTariff()
	{
		RefCusProcedureHelper.CreateCusPreference(Factory);
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var tariffTest1 = tariffTestHelper.CreateImportTariff("77777777");
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.A, "1", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.G, "2", Core.Constants.CountryCodes.Sweden);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.C, "3", Core.Constants.CountryCodes.Sweden, Core.Constants.CountryCodes.Brazil);
		tariffTestHelper.AddRate(tariffTest1, PrimaryPreferenceCodeList.Codes.B, "3", Core.Constants.CountryCodes.Sweden);
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "77777777";
			var codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			AssertSame("Cached", lookups.PrimaryPreferenceList, codeList);
			AssertType<PrimaryPreferenceCodeList>("Type", codeList);
			AssertEquals("Codes from lookup, no country", "A, B, C, G, P, J, N", codeList.CodesAsString);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
			codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			AssertType<CodeDescriptionPairList>("Type", codeList);
			AssertEquals("Codes from lookup, Sweden", "A, B, C, G, J, N", codeList.CodesAsString);

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Brazil;
			codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			AssertType<CodeDescriptionPairList>("Type", codeList);
			AssertEquals("Codes from lookup, Brazil", "C, J, N", codeList.CodesAsString);

			invoiceLine.JI_Tariff = string.Empty;
			codeList = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			AssertType<PrimaryPreferenceCodeList>("Type", codeList);
			AssertEquals("Codes from lookup with no tariff, Brazil", "A, B, C, G, P, J, N", codeList.CodesAsString);
		});
	}

	public void TestJI_ProcedureFromStyle()
	{
		RefCusProcedureHelper.CreateRefCusProcedureList(Factory);
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var codeList = invoiceLine.Lookups.Procedures;
		instruction.CEI_Style = "4";
		invoiceLine.JI_CEI = instruction.PK;
		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType} : Style={instruction.CEI_Style}", expected: true, lookups.Procedures.ContainsCode("4050"));
			AssertEquals($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType} : Style={instruction.CEI_Style}", expected: false, lookups.Procedures.ContainsCode("5011"));
			AssertSame($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType}: cached", lookups.Procedures, lookups.Procedures);
			instruction.CEI_Style = "5";
			AssertEquals($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType} : Style={instruction.CEI_Style}", expected: true, lookups.Procedures.ContainsCode("5011"));
			AssertEquals($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType} : Style={instruction.CEI_Style}", expected: false, lookups.Procedures.ContainsCode("4050"));
			AssertSame($"{nameof(declaration.JE_MessageType)}={declaration.JE_MessageType}: cached", lookups.Procedures, lookups.Procedures);
		});
	}
}
