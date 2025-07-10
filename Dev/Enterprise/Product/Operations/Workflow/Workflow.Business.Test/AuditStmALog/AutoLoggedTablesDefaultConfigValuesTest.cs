using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	public class AutoLoggedTablesDefaultConfigValuesTest : TestCaseWithFactory
	{
		public void TestSubstitutedConfigInstanceIsReferred()
		{
			try
			{
				TypeDecider.AddSubstitution(typeof(AutoLoggedTablesDefaultConfigValues), typeof(AutoLoggedTablesDefaultConfigValuesForTesting));

				Assert(AutoLoggedTablesDefaultConfigValues.Instance is AutoLoggedTablesDefaultConfigValuesForTesting);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(AutoLoggedTablesDefaultConfigValuesForTesting));
			}
		}
	}

	public class AutoLoggedTablesDefaultConfigValuesForTesting : AutoLoggedTablesDefaultConfigValues
	{
	}
}
