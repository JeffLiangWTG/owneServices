using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstructionComparer))]
sealed class CusEntryInstructionComparerTest : CusEntryInstructionComparerAbstractTest<CusEntryInstructionComparer>
{
	public override void TestCompare()
	{
		CombineAssertions(() =>
		{
			var testInstruction1 = Factory.New<CusEntryInstruction>();
			var testInstruction2 = Factory.New<CusEntryInstruction>();

			testInstruction1.CEI_SubStyle = "B";
			testInstruction1.CEI_Procedure = "40";
			testInstruction1.CEI_Description = "ABB";
			testInstruction2.CEI_SubStyle = "B";
			testInstruction2.CEI_Procedure = "40";
			testInstruction2.CEI_Description = "ABB";
			AssertEquals("Same CusEntryInstructions", true, comparer.Compare(testInstruction1, testInstruction2) == 0);
			testInstruction2.CEI_Description = "ABC";
			AssertEquals("1<2 Description", true, comparer.Compare(testInstruction1, testInstruction2) < 0);
			testInstruction2.CEI_Description = "AAA";
			AssertEquals("1>2 Description", true, comparer.Compare(testInstruction1, testInstruction2) > 0);
			testInstruction2.CEI_Description = "ABB";
			testInstruction2.CEI_Procedure = "50";
			AssertEquals("1<2 Procedure", true, comparer.Compare(testInstruction1, testInstruction2) < 0);
			testInstruction2.CEI_Procedure = "30";
			AssertEquals("1>2 Procedure", true, comparer.Compare(testInstruction1, testInstruction2) > 0);
			testInstruction2.CEI_Procedure = "40";
			testInstruction2.CEI_SubStyle = "C";
			AssertEquals("1<2 SubStyle", true, comparer.Compare(testInstruction1, testInstruction2) < 0);
			testInstruction2.CEI_SubStyle = "A";
			AssertEquals("1>2 Style", true, comparer.Compare(testInstruction1, testInstruction2) > 0);
		});
	}
}
