using Microsoft.Build.Locator;
using NUnit.Framework;
using System.Linq;

namespace eHub.DatImplementation.Deployment.Tests
{
	[SetUpFixture]
	public sealed class Initialization
	{
		[OneTimeSetUp]
		public void SetUp() => EnsureInitialized();

		public static void EnsureInitialized()
		{
			if (MSBuildLocator.IsRegistered)
			{
				return;
			}

			var instances = MSBuildLocator.QueryVisualStudioInstances();
			var latestInstance = instances.OrderByDescending(i => i.Version).FirstOrDefault();
			Assert.That(latestInstance, Is.Not.Null);
			MSBuildLocator.RegisterInstance(latestInstance);
		}
	}
}