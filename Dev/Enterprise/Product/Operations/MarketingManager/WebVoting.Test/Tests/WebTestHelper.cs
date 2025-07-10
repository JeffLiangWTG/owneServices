using System;
using System.Threading;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	static class WebTestHelper
	{
		internal static void AssertNoUndisposedDbConnectionError(TestCase testcase, Action action)
		{
			var lastError = string.Empty;

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();
				new HttpContextEnabledTestAttribute().SetUp(testcase);

				action();

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});

			thread.Start();
			thread.Join(TimeSpan.FromSeconds(1));

			//There should be no errors, especially "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			Assertion.AssertNullOrEmpty(lastError);
		}
	}
}
