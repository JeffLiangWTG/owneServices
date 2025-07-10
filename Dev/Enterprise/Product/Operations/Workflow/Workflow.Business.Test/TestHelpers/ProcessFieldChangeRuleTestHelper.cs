using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business.Test
{
	public class ProcessFieldChangeRuleTestHelper : IProcessFieldChangeRuleTestHelper
	{
		public void TurnOffBlacklistForTest()
		{
			ProcessFieldChangeRuleField.TurnOffBlacklistForTest.Value = true;
		}
	}
}
