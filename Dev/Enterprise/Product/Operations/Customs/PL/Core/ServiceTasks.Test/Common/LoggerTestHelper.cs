using System;
using System.Linq;
using Enterprise.Integration;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

static class LoggerTestHelper
{
	public static void AssertContainsMessages(this ILogger logger, params string[] expectedMessages)
	{
		var loggedMessages = logger.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		foreach (var expectedMessage in expectedMessages)
		{
			Assert($"Expected service log processing message: {expectedMessage}", loggedMessages.Any(x => x.Contains(expectedMessage)));
		}
	}
}
