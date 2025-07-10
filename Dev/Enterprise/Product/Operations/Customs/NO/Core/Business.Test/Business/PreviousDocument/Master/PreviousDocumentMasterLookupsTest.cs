using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(PreviousDocumentMasterLookups))]
sealed class PreviousDocumentMasterLookupsTest : TestCaseWithFactory
{
	public void TestProcedureList()
	{
		var entryInstruction = Factory.New<JobDeclaration>()
			.CustomsEntryInstructions.AddNew();
		var master = new PreviousDocumentMaster(Factory, entryInstruction);
		var lookups = new PreviousDocumentMasterLookups(master);
		AssertContainsExactElementsInAnyOrder("Procedure Code List", new[] { "71A", "71A8", "71E" }, lookups.ProcedureList.GetAllCodes());
	}
}
