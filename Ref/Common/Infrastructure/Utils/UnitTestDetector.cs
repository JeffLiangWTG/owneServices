using System;
using System.Linq;
using System.Reflection;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class UnitTestDetector
	{
		public static readonly Lazy<bool> IsRunningTests = new Lazy<bool>(
			() => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("nunit.framework", StringComparison.OrdinalIgnoreCase))
			|| Environment.GetEnvironmentVariable("RefDataRepoTesting") == "Test"
			|| (Assembly.GetEntryAssembly()?.Location?.EndsWith("testhost.dll", StringComparison.OrdinalIgnoreCase) ?? false)
		);
	}
}
