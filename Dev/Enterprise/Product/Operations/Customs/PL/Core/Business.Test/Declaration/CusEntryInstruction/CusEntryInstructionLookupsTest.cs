using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.CusEntryInstructionLookupsTest
{
	protected override Type TypeOfDeclarationOfInstruction => typeof(JobDeclaration);

	protected override Type GetExpectedEntrySubStyleListType() => typeof(EntrySubStyleList);

	protected override ZString GetExpectedEntrySubStyleListCodesAsStringWithoutDeclaration() => string.Join(", ", new EntrySubStyleList().GetAllCodes().OrderBy(code => code));

	protected override ZString GetExpectedEntrySubStyleListCodesAsString() => string.Join(", ", new EntrySubStyleList().GetAllCodes().OrderBy(code => code));

	public void TestEadPrintOutList()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Poland should contain specified list from EadPrintOutList",
			"0, 1, 2",
			instruction.Lookups.EadPrintOutList.CodesAsString);
	}

	public void TestTemporaryLocationCodeTypeList()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Poland should contain specified list from TemporaryLocationCodeTypeList",
			"CODE, DESC",
			instruction.Lookups.TemporaryLocationCodeTypeList.CodesAsString);
	}

	public void TestCPCList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures;
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Poland, "Poland");
		helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Germany, "Germany");
		helper.CreateNewOrGetExistingCusCodeType(codeType, "CustomsProcedures", CountryCodes.Poland);
		helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Poland, codeType, "11", "PLI1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "11", "", "", "", "IMP", "");
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "98", "00", "C00", "", "IMP", "");
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "97", "31", "", "", "IMP", "");
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Germany, "", "66", "", "", "", "IMP", "");
		helper.CreateNewOrGetExistingCusCodeList(CountryCodes.Poland, codeType, "95", "PLI2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "99", "", "", "", "EXP", "");
		helper.CreateOrFindExistingRefCusProcedure(CountryCodes.Poland, "", "95", "00", "", "", "EXP", "");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_Style = ZString.Empty;
			AssertEquals("11, 97, 98", instruction.Lookups.CPCList.CodesAsString);
			AssertEquals("PLI1", instruction.Lookups.CPCList.GetDescriptionFromCode("11"));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("95, 99", instruction.Lookups.CPCList.CodesAsString);
			AssertEquals("PLI2", instruction.Lookups.CPCList.GetDescriptionFromCode("95"));
		});
	}

	public void TestEntrySubstyleCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entrySubStyleList = instruction.Lookups.EntrySubStyleList.GetAllCodes();

		AssertContainsExactElementsInAnyOrder(new EntrySubStyleList().GetAllCodes(), entrySubStyleList);
		AssertEquals(true, entrySubStyleList.Zip(entrySubStyleList.Skip(1), (first, second) => string.Compare(first, second) <= 0).All(e => e));
	}
}
