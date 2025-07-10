using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class CusFiscalReferenceLookupsTest : TestCaseWithFactory
{
	public void TestCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();

		var list = cusFiscalReference.Lookups.CodeList;
		CombineAssertions("Export", () =>
		{
			AssertEquals("CodesAsString", "FR1, FR2, FR3, FR4, FR5", list.CodesAsString);
			AssertEquals("Cached", list, cusFiscalReference.Lookups.CodeList);
		});

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		list = cusFiscalReference.Lookups.CodeList;
		CombineAssertions("Import", () =>
		{
			AssertEquals("CodesAsString", "FR1, FR2, FR3, FR4, FR5, FR7", list.CodesAsString);
			AssertEquals("Cached", list, cusFiscalReference.Lookups.CodeList);
		});
	}
}
