using CargoWise.Application;
using NUnit.Framework;

namespace CargoWise.Blazor.HostedAppRuntime.Test
{
	public class EmbeddedResourcesTests
	{
		[Test]
		public void AppServerAssembly_ManifestResourceName_HasExpectedEmbeddedResourceNames(
			[Values("Configuration_CargoWiseOneObjectFactory.xml")] string name)
		{
			var assembly = typeof(HostedAppRuntimeConfigurationExtensions).Assembly;
			var manifestNames = assembly.GetManifestResourceNames();
			Assert.That(
				manifestNames,
				Contains.Item($"{assembly.GetName().Name}.{name}"),
				message: $"Manifest is required and must have name of format <namespace>.<resourcename> to be loadable in {nameof(ObjectFactory)}");
		}
	}
}
