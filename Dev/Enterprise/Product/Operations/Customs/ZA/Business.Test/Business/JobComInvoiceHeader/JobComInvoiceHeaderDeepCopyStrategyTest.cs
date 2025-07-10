using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderDeepCopyStrategyTest : TestCaseWithFactory
	{
		public void TestCopyCustomsEntryInstructions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var clonedDeclaration = (JobDeclaration)new ZAJobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy, Factory).Clone();
			AssertEquals("Entry Instruction Copy - Count", 1, clonedDeclaration.CustomsEntryInstructions.Count);
			AssertEquals("Entry Instruction Copy - ProcedureCode", "11", clonedDeclaration.CustomsEntryInstructions[0].CEI_Style);
		}
	}
}
