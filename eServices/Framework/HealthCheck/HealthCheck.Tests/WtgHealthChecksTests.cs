using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NUnit.Framework;

namespace eServices.HealthCheck.Tests
{
	public class WtgHealthChecksTests
	{
		public static IEnumerable<TestCaseData> WtgHealthCheck_TestCaseSource = [
			new("H", "H", "H", "H",
				HttpStatusCode.OK, "Healthy",
				HttpStatusCode.OK, """
				INFO(Default)
				INFO(Ready)
				INFO(Check1)
				INFO(Check2): Check2 state is 'H'|STATE: H|Message: Check2 state is 'H'
				INFO(Check3)
				
				"""),
			new("D", "H", "H", "H",
				HttpStatusCode.OK, "Degraded",
				HttpStatusCode.OK, """
				INFO(Default)
				WARNING(Ready)
				INFO(Check1)
				INFO(Check2): Check2 state is 'H'|STATE: H|Message: Check2 state is 'H'
				INFO(Check3)
				
				"""),
			new("U", "H", "H", "H",
				HttpStatusCode.ServiceUnavailable, "Unhealthy",
				HttpStatusCode.OK, """
				INFO(Default)
				ERROR(Ready)
				INFO(Check1)
				INFO(Check2): Check2 state is 'H'|STATE: H|Message: Check2 state is 'H'
				INFO(Check3)
				
				"""),
			new("H", "D", "H", "H",
				HttpStatusCode.OK, "Healthy",
				HttpStatusCode.OK, """
				INFO(Default)
				INFO(Ready)
				WARNING(Check1)
				INFO(Check2): Check2 state is 'H'|STATE: H|Message: Check2 state is 'H'
				INFO(Check3)
				
				"""),
			new("H", "U", "H", "H",
				HttpStatusCode.OK, "Healthy",
				HttpStatusCode.OK, """
				INFO(Default)
				INFO(Ready)
				ERROR(Check1)
				INFO(Check2): Check2 state is 'H'|STATE: H|Message: Check2 state is 'H'
				INFO(Check3)
				
				"""),
			new("H", "U", "U", "U",
				HttpStatusCode.OK, "Healthy",
				HttpStatusCode.OK, """
				INFO(Default)
				INFO(Ready)
				ERROR(Check1)
				ERROR(Check2): Check2 state is 'U'|STATE: U|Message: Check2 state is 'U'|System.InvalidOperationException: Operation is not valid due to the current state of the object.
				ERROR(Check3): Check3 has failed.|System.InvalidOperationException: Check3 has failed.
				
				""")
		];

		[TestCaseSource(nameof(WtgHealthCheck_TestCaseSource))]
		public async Task WtgHealthCheckTest(
			string readyState,
			string check1State,
			string check2State,
			string check3State,
			HttpStatusCode readyHttpStatus,
			string readyContent,
			HttpStatusCode statusHttpStatus,
			string statusContent)
		{
			Environment.SetEnvironmentVariable("ASPNETCORE_TEST_CONTENTROOT_ESERVICES_HEALTHCHECK_TESTS", TestContext.CurrentContext.TestDirectory);
			Environment.SetEnvironmentVariable("ASPNETCORE_HEALTHCHECK_READY", readyState);
			Environment.SetEnvironmentVariable("ASPNETCORE_HEALTHCHECK_CHECK1", check1State);
			Environment.SetEnvironmentVariable("ASPNETCORE_HEALTHCHECK_CHECK2", check2State);
			Environment.SetEnvironmentVariable("ASPNETCORE_HEALTHCHECK_CHECK3", check3State);

			await using var application = new WebApplicationFactory<TestApp>();
			using var client = application.CreateClient();

			var readyResponse = await client!.GetAsync("/wtg/ready");
			var statusResponse = await client!.GetAsync("/wtg/status");

			await Assert.MultipleAsync(async () =>
			{
				Assert.That(readyResponse.StatusCode, Is.EqualTo(readyHttpStatus));
				Assert.That(await readyResponse.Content.ReadAsStringAsync(), Is.EqualTo(readyContent));
				Assert.That(statusResponse.StatusCode, Is.EqualTo(statusHttpStatus));
				Assert.That(await statusResponse.Content.ReadAsStringAsync(), Is.EqualTo(statusContent));
			});
		}
	}
}
