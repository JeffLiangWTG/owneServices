using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class WebServicesTest
{
	[Test]
	public void MachineConfigExistTest()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string assemblyLocation = assembly.Location;
		string binFolder = Path.GetDirectoryName(assemblyLocation);
		Assert.That(File.Exists(Path.Combine(binFolder, "../winzor/Config/machine.config")), $"The static asset {Path.Combine(binFolder, "../winzor/Config/machine.config")} was not found.");
		Assert.That(File.Exists(Path.Combine(binFolder, "../AppServer/Config/machine.config")), $"The static asset {Path.Combine(binFolder, "../AppServer/Config/machine.config")} was not found.");
	}
}
