using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclarationTypeList_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertContainsExactElementsInAnyOrder("DeclarationTypeList AllCodes", new[] { "H1", "H1", "H3", "H4", "H5", "H7" }, lookups.DeclarationTypeList.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			lookups = instruction.Lookups;
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryInstructionLookups lookups;
	}
}
