using Microsoft.Extensions.Hosting;

namespace CargoWise.Blazor.Common
{
	public static class HostEnvironmentEnvForTestExtensions
	{
		public const string DAT = "DAT";
		public const string IntegrationTest = "IntegrationTest";

		static bool IsIntegrationTest(this IHostEnvironment hostEnvironment) =>
			hostEnvironment.IsEnvironment(IntegrationTest);

		static bool IsDAT(this IHostEnvironment hostEnvironment) =>
			hostEnvironment.IsEnvironment(DAT);

		public static bool IsDevelopmentOrIntegrationTest(this IHostEnvironment hostEnvironment) =>
			hostEnvironment.IsDevelopment()
			|| hostEnvironment.IsIntegrationTest();

		public static bool IsDevelopmentOrIntegrationTestOrDAT(this IHostEnvironment hostEnvironment) =>
			hostEnvironment.IsDevelopmentOrIntegrationTest()
			|| hostEnvironment.IsDAT();
	}
}
