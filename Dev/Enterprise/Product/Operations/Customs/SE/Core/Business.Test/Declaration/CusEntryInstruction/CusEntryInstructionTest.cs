using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		public void TestJobDeclaration()
		{
			AssertType<JobDeclaration>(entryInstruction.JobDeclaration);
		}

		public void TestLookups()
		{
			AssertType<CusEntryInstructionLookups>(entryInstruction.Lookups);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			declaration = factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
