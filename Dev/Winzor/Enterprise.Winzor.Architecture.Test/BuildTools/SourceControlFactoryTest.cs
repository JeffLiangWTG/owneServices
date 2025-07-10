using CargoWise.BuildTools;
using CargoWise.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

public class SourceControlFactoryTest
{
	[Test]
	public void TestGitRepository()
	{
		using var ctx = new EnterpriseTestContext();
		using (var tempDirectory = new TempDirectory())
		{
			Repository.Init(tempDirectory.DirectoryName);
			using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(tempDirectory.DirectoryName))
			{
				Assert.That(sourceControl, Is.InstanceOf<GitSourceControl>());
			}
		}
	}
}
