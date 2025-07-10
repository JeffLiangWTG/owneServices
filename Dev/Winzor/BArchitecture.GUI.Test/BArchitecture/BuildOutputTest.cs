using System.IO;
using NUnit.Framework;

namespace WinzorFramework;

class BuildOutputTest
{
	[Test, Explicit("WI00729199 will remove GenerateRuntimeConfigDevFile, .playwright generated before build since Microsoft.Playwright package will not copied to Bin folder")]
	public void NoPlaywrightFiles()
	{
		Assert.That(Path.Combine(TestContext.CurrentContext.TestDirectory, ".playwright"), Does.Not.Exist);
	}
}
