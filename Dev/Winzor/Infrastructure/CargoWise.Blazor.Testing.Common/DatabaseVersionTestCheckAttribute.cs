using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.PlaywrightTesting.Helpers;

namespace CargoWise.Blazor.Testing.Common
{
	/// <summary>
	/// Quickly fail all tests if any test fails with a database upgrade required error
	/// </summary>
	public sealed class DatabaseVersionTestCheckAttribute : TestActionAttribute
	{
		public override ActionTargets Targets => ActionTargets.Test;

		bool ShouldRun => !TestEnvironment.IsDatTesting;

		public override void AfterTest(ITest test)
		{
			if (ShouldRun
				&& TestContext.CurrentContext.Result.FailCount > 0
				&& TestContext.CurrentContext.Result.Message.Contains("CargoWise.Data.DatabaseUpgradedException", StringComparison.InvariantCultureIgnoreCase))
			{
				hasFailedDueToDatabase = true;
			}
		}

		public override void BeforeTest(ITest test)
		{
			if (ShouldRun && hasFailedDueToDatabase)
			{
				throw new InvalidOperationException("Database needs upgrading: a previous test failed with DatabaseUpgradedException. Further tests are not running to save time.");
			}
		}

		bool hasFailedDueToDatabase;
	}
}
