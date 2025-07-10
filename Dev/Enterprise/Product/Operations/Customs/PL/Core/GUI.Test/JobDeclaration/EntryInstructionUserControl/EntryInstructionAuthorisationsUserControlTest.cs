using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class EntryInstructionAuthorisationsUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new EntryInstructionAuthorisationsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("Base type should be of EU Entry Instruction authorization type"
					, typeof(EU.GUI.EntryInstructionAuthorisationsComputedUserControl)
					, control.GetType().BaseType);
			});
		}
	}
}
