namespace OcmPoc.Tests.Scenario.Configuration
{
	public class TestConfiguration
	{
		public FileSystemConfig ProviderA { get; } = new FileSystemConfig();
		public FileSystemConfig ProviderB { get; } = new FileSystemConfig();
		public ApiConfig CW1 { get; } = new ApiConfig();
	}
}