using CargoWise.EntityFramework.Testing;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterData.GUI.Tests
{
	public class DeduplicationQueryAnalyzerTest : TestCaseWithFactory
	{
		public void TestShouldHandleException_WhenQueryHasError()
		{
			var query = "wrong query";
			UnitTestUserNotification.Instance.ClearMessages();
			using (var analyzer = new MockDeduplicationQueryAnalyzer(query))
			{
				AssertNoExceptionThrown(analyzer.Show);
				AssertContains("Could not find stored procedure 'wrong'", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	#region Implementation

	public class MockDeduplicationQueryAnalyzer : DeduplicationQueryAnalyzer
	{
		public MockDeduplicationQueryAnalyzer(string sqlText) : base(new DeduplicationMonitoringMethodNameItem(string.Empty, string.Empty) { DBCommandText = sqlText })
		{
		}

		protected override void RunCommand()
		{
			ExecuteQuery();
		}
	}
	#endregion
}
